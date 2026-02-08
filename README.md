# FsPowerShellTemplate

A minimal template repository for building PowerShell modules in F#.

This repository supports two use cases:

1. As a [GitHub template repository](https://docs.github.com/en/repositories/creating-and-managing-repositories/creating-a-template-repository).
2. (Planned) Packaged and published as a [NuGet template package](https://learn.microsoft.com/en-us/dotnet/core/tutorials/cli-templates-create-template-package).
   Once available, users can run `dotnet new ...` to scaffold a new F# PowerShell module project.

## Features

- [x] A sample cmdlet implemented in F#: `Get-Greeting`
- [x] Command-line predictor
- [x] Feedback Provider
- [ ] Shared state across Cmdlet, Command-line predictor and Feedback Provider
- [x] Linter and formatter with [Fantomas](https://github.com/fsprojects/fantomas) + [FSharp.Analyzers.SDK](https://github.com/ionide/FSharp.Analyzers.SDK)
- [ ] Unit tests with [Expecto](https://github.com/haf/expecto) + [FsCheck](https://github.com/fscheck/FsCheck)
- [ ] End-to-end tests for cmdlets with [Pester](https://github.com/pester/Pester)
- [ ] Documentation generation with [Microsoft.PowerShell.PlatyPS](https://github.com/PowerShell/platyPS)
- [ ] Task runner for tests, linter, formatter, documentation and [PowerShell Gallery](https://www.powershellgallery.com/) publishing workflow

## Requirements

- General
   - .NET SDK as specified in [`global.json`](./global.json) (this repository pins .NET SDK 10)
- [Command-line predictor](https://learn.microsoft.com/en-us/powershell/scripting/dev-cross-plat/create-cmdline-predictor)
   - PowerShell 7.2+
   - PSReadLine 2.2.2+
   - .NET 8 SDK 6.0.0+ (for PowerShell 7.2)
- [Feedback Provider](https://learn.microsoft.com/en-us/powershell/scripting/dev-cross-plat/create-feedback-provider)
   - PowerShell 7.4+
   - Enable the `PSFeedbackProvider` experimental feature
   - .NET 8 SDK 8.0.0+ (for PowerShell 7.4)

## License

MIT. See [`LICENSE`](./LICENSE).
