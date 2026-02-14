<#
.Synopsis
    Invoke-Build tasks
#>

# Build script parameters
[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [ValidateSet('Init', 'Clean', 'Lint', 'Build', 'Import')]
    [string[]] $Tasks = @('Build'),

    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Debug'
)

# If invoked directly (not dot-sourced by Invoke-Build), hand off execution to Invoke-Build.
if ($MyInvocation.InvocationName -ne '.') {
    Invoke-Build -File $PSCommandPath -Task $Tasks @PSBoundParameters
    exit $LASTEXITCODE
}

# --- Setup ---

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$ModuleName = Get-ChildItem ./src/*/*.psd1 | Select-Object -ExpandProperty BaseName
$ModuleSrcPath = Resolve-Path "./src/${ModuleName}/"
$ModuleSrcProject = Resolve-Path "$ModuleSrcPath/$ModuleName.fsproj"
$ModuleVersion = ($ModuleSrcProject | Select-Xml '//Version/text()').Node.Value
$ModulePublishPath = Resolve-Path "./publish/${ModuleName}/"
$PublishModuleManifest = Join-Path $ModulePublishPath "${ModuleName}.psd1"

Write-Host "Module: ${ModuleName} ver${ModuleVersion} root=${ModuleSrcProject} publish=${ModulePublishPath}" -ForegroundColor Magenta
Write-Host "Parameters: $($PSBoundParameters | ConvertTo-Json -Compress)" -ForegroundColor Green

# --- Tasks (Invoke-Build) ---

# Synopsis: Initializes the build environment by restoring NuGet packages and .NET tools.
Task Init {
    dotnet restore
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet restore failed with exit code $LASTEXITCODE"
    }

    dotnet tool restore
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet tool restore failed with exit code $LASTEXITCODE"
    }
}

Task Clean Init, {
    # Clean build outputs.
    'Debug', 'Release' | ForEach-Object {
        dotnet clean -c $_
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet clean failed for configuration $_ with exit code $LASTEXITCODE"
        }
    }

    'bin', 'obj' | ForEach-Object {
        $path = Join-Path $PSScriptRoot 'src/*' $_
        if (Test-Path $path) {
            Get-ChildItem -Path $path -Recurse | Remove-Item -Force -Recurse
        }
    }

    $debugLog = Join-Path $PSScriptRoot 'debug.log'
    if (Test-Path $debugLog) {
        Remove-Item -Path $debugLog -Force
    }

    if (Test-Path $ModulePublishPath) {
        Remove-Item -Path "${ModulePublishPath}/*" -Force -Recurse
    }
}

Task Lint Clean, {
    # Format check.
    dotnet tool run fantomas --check (Join-Path $PSScriptRoot 'src')
    if ($LASTEXITCODE -ne 0) {
        throw "fantomas check failed with exit code $LASTEXITCODE"
    }
}

Task Build Clean, {
    dotnet build -c $Configuration

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet build failed with exit code $LASTEXITCODE"
    }
}

Task Import Build, {
    dotnet publish $ModuleSrcProject -c $Configuration -o $ModulePublishPath

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed with exit code $LASTEXITCODE"
    }

    if (-not (Test-Path $PublishModuleManifest)) {
        throw "Publish manifest not found at: $PublishModuleManifest"
    }

    Import-Module -Name $PublishModuleManifest -Force
    Get-Module -Name $ModuleName
}
