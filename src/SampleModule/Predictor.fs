namespace SampleModule

open System
open System.Collections.Generic
open System.Management.Automation
open System.Management.Automation.Subsystem
open System.Management.Automation.Subsystem.Prediction
open System.Threading

type GreetingPredictor(guid: string) =
    let id = Guid.Parse(guid)

    [<Literal>]
    let name = "Greeting"

    [<Literal>]
    let description = "A predictor that suggests a greeting based on the input."

    interface ICommandPredictor with
        member __.Id = id
        member __.Name = name
        member __.Description = description

        member __.GetSuggestion
            (client: PredictionClient, context: PredictionContext, cancellationToken: CancellationToken)
            : SuggestionPackage =
            context.InputAst.Extent.Text
            |> function
                // NOTE: suggestionEntries requires non-empty by Requires.NotNullOrEmpty.
                // https://github.com/PowerShell/PowerShell/blob/eef334de1b0f648512859bd032356f9c8df7cb91/src/System.Management.Automation/engine/Subsystem/PredictionSubsystem/ICommandPredictor.cs#L278
                | input when input |> String.IsNullOrWhiteSpace -> List.Empty
                | input ->
                    [ ($"'Hello {input}, PowerShell from F#!", "A friendly greeting from F#'")
                      |> PredictiveSuggestion ]
            |> Linq.Enumerable.ToList
            |> SuggestionPackage

        member __.CanAcceptFeedback(client: PredictionClient, feedback: PredictorFeedbackKind) : bool = false

        member __.OnSuggestionDisplayed(client: PredictionClient, session: uint32, countOrIndex: int) : unit = ()

        member __.OnSuggestionAccepted(client: PredictionClient, session: uint32, acceptedSuggestion: string) : unit =
            ()

        member __.OnCommandLineAccepted(client: PredictionClient, history: IReadOnlyList<string>) : unit = ()

        member __.OnCommandLineExecuted(client: PredictionClient, commandLine: string, success: bool) : unit = ()

type Init() =
    [<Literal>]
    let identifier = "394e28cb-7f3f-4ef9-ab73-8172763ba4ac"

    interface IModuleAssemblyInitializer with
        member __.OnImport() =
            let p = new GreetingPredictor(identifier)
            SubsystemManager.RegisterSubsystem(SubsystemKind.CommandPredictor, p)

    interface IModuleAssemblyCleanup with
        member __.OnRemove(psModuleInfo: PSModuleInfo) =
            SubsystemManager.UnregisterSubsystem(SubsystemKind.CommandPredictor, Guid(identifier))
