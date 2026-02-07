# FsPowerShellTemplate

A minimal template repository for building PowerShell modules in F#.

This repository supports two use cases:

1. As a [GitHub template repository](https://docs.github.com/en/repositories/creating-and-managing-repositories/creating-a-template-repository).
2. (Planned) Packaged and published as a [NuGet template package](https://learn.microsoft.com/en-us/dotnet/core/tutorials/cli-templates-create-template-package).
   Once available, users can run `dotnet new ...` to scaffold a new F# PowerShell module project.

## Status

- [x] A sample cmdlet implemented in F#: `Get-Greeting`
- [ ] Command-line predictor
- [ ] Feedback Provider
- [x] Linter and formatter with [Fantomas](https://github.com/fsprojects/fantomas) + [FSharp.Analyzers.SDK](https://github.com/ionide/FSharp.Analyzers.SDK)
- [ ] Unit tests with [Expecto](https://github.com/haf/expecto) + [FsCheck](https://github.com/fscheck/FsCheck)
- [ ] End-to-end tests for cmdlets with [Pester](https://github.com/pester/Pester)
- [ ] Documentation generation with [Microsoft.PowerShell.PlatyPS](https://github.com/PowerShell/platyPS)
- [ ] Task runner for tests, linter, formatter, documentation and [PowerShell Gallery](https://www.powershellgallery.com/) publishing workflow

## Requirements

- .NET SDK as specified in [`global.json`](./global.json) (this repository pins .NET SDK 10)
- PowerShell 7.4+ (Feedback Provider requires 7.4)

## License

MIT. See [`LICENSE`](./LICENSE).
