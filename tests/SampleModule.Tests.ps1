Describe 'SampleModule' {
    Context 'SampleModule module' {
        It 'Given the SampleModule module, it should have a nonzero version' {
            $m = Get-Module 'SampleModule'
            $m.Version | Should -Not -Be $null
        }
        It 'Given the SampleModule module, it should have <Expected> commands' -TestCases @(
            @{ Expected = @('Add-Greeting', 'Get-Greeting') }
        ) {
            $m = Get-Module 'SampleModule'
            ($m.ExportedCmdlets).Values | ForEach-Object Name | Should -Eq $Expected
        }
    }
    Context 'Get-Greeting' {
        It 'Given the Get-Greeting command, it should return a greeting' {
            Get-Greeting | Should -Be $null
        }
    }
    Context 'Add-Greeting' {
        It 'Given the Add-Greeting command, it should add a greeting' {
            Add-Greeting 'sample'
            $greetings = Get-Greeting
            $greetings | Measure-Object | ForEach-Object Count | Should -Be 1
            $greetings | Select-Object -First 1 | Should -Be 'sample'
        }
    }
}
