<#
.Synopsis
    Invoke-Build tasks
#>

# Build script parameters
[CmdletBinding()]
[Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseDeclaredVarsMoreThanAssignments', '', Justification = 'Variables are used in script blocks and argument completers')]
param(
    [Parameter(Position = 0)]
    [ValidateSet('Init', 'Clean', 'Lint', 'Build', 'UnitTest', 'Import', 'E2ETest', 'GenerateHelp', 'TestAll')]
    [string[]] $Tasks = @('Build'),

    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Debug'
)

# If invoked directly (not dot-sourced by Invoke-Build), hand off execution to Invoke-Build.
if ($MyInvocation.InvocationName -ne '.') {
    $Tasks = $PSBoundParameters['Tasks']
    $forward = $PSBoundParameters.GetEnumerator() | ForEach-Object -Begin { $acc = @{} } -Process {
        Write-Host "Processing parameter: ${_}" -ForegroundColor Yellow
        if ($_.Key -ne 'Tasks') {
            $acc[$_.Key] = $_.Value
        }
    } -End { $acc }
    Invoke-Build -File $PSCommandPath -Task $Tasks @forward
    exit $LASTEXITCODE
}

# --- Setup ---

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$ModuleName = Get-ChildItem "${PSScriptRoot}/src/*/*.psd1" | Select-Object -First 1 | Select-Object -ExpandProperty BaseName
$ModuleSrcPath = Resolve-Path "${PSScriptRoot}/src/${ModuleName}/"
$ModuleSrcProject = Resolve-Path "$ModuleSrcPath/$ModuleName.fsproj"
$ModuleVersion = ($ModuleSrcProject | Select-Xml '//Version/text()').Node.Value
$ModulePublishPath = "${PSScriptRoot}/publish/${ModuleName}/"
$PublishModuleManifest = Join-Path $ModulePublishPath "${ModuleName}.psd1"

Write-Host "Module: ${ModuleName} ver${ModuleVersion} root=${ModuleSrcProject} publish=${ModulePublishPath}" -ForegroundColor Magenta
Write-Host "Parameters: $($PSBoundParameters | ConvertTo-Json -Compress)" -ForegroundColor Green

function Get-ValidMarkdownCommentHelp {
    $help = Measure-PlatyPSMarkdown "./docs/${ModuleName}/*.md" | Where-Object Filetype -Match CommandHelp
    $validations = $help.FilePath | Test-MarkdownCommandHelp -DetailView
    if (-not $validations.IsValid) {
        $validations.Messages | Where-Object { $_ -notlike 'PASS:*' } | Write-Error
        throw 'Invalid markdown help files.'
    }
    $help
}

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
    # Clean debug outputs.
    $debugLog = Join-Path $PSScriptRoot 'debug.log'
    if (Test-Path $debugLog) {
        Remove-Item -Path $debugLog -Force
    }
    # Clean publish outputs.
    if (Test-Path $ModulePublishPath) {
        Remove-Item -Path "${ModulePublishPath}/*" -Force -Recurse
    }
}

Task Build Clean, {
    dotnet build -c $Configuration

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet build failed with exit code $LASTEXITCODE"
    }
}

Task Lint Build, {
    # F# formatting and analyzers.
    dnx fantomas --check (Join-Path $PSScriptRoot 'src')
    if ($LASTEXITCODE -ne 0) {
        throw "fantomas check failed with exit code $LASTEXITCODE"
    }
    $analyzerPath = dotnet build $ModuleSrcPath --getProperty:PkgIonide_Analyzers
    Get-ChildItem './src/*/*.fsproj' | ForEach-Object {
        dotnet fsharp-analyzers --project $_ --analyzers-path $analyzerPath --report "analysis/$($_.BaseName)-report.sarif" --code-root src --exclude-files '**/obj/**/*' '**/bin/**/*'
        if (-not $?) {
            throw "dotnet fsharp-analyzers for $($_.BaseName) failed."
        }
    }
    # PowerShell script analysis.
    './build.ps1' | ForEach-Object {
        $warn = Invoke-ScriptAnalyzer -Path $_ -Settings .\PSScriptAnalyzerSettings.psd1
        if ($warn) {
            $warn
            throw "Invoke-ScriptAnalyzer for ${_} failed."
        }
    }
    # Validate markdown help files.
    Get-ValidMarkdownCommentHelp | Out-Null
}

Task UnitTest Lint, {
    dotnet test --nologo --verbosity detailed --blame-hang-timeout 5s --blame-hang-dump-type full
    if (-not $?) {
        throw 'dotnet test failed.'
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

Task E2ETest Import, {
    $result = Invoke-Pester -PassThru
    if ($result.Failed) {
        throw 'Invoke-Pester failed.'
    }
}

Task GenerateHelp Import, {
    $platyHelp = Get-ValidMarkdownCommentHelp
    $platyHelp | Format-List
    try {
        # Microsoft.PowerShell.PlatyPS 1.0.1  cmdlets do not work with StrictMode enabled; disable it for the duration of this block.
        # issue: https://github.com/PowerShell/platyPS/issues/800
        Set-StrictMode -Off
        Update-MarkdownCommandHelp -Path $platyHelp.FilePath -NoBackup
        Import-MarkdownCommandHelp -Path $platyHelp.FilePath | Export-MamlCommandHelp -OutputFolder ./src/ -Force | Out-Null
    }
    finally {
        # This script runs with StrictMode Latest by default; restore that behavior.
        Set-StrictMode -Version Latest
    }
}

Task TestAll UnitTest, E2ETest
