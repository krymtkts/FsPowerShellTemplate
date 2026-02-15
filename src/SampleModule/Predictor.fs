namespace SampleModule

open System
open System.Collections.Generic
open System.Management.Automation.Subsystem.Prediction
open System.Text.RegularExpressions
open System.Threading

open SampleModule.Core

type GreetingPredictor(guid: string) =
    let id = guid |> Guid.Parse

    let mutable miniSessionId = 0

    [<Literal>]
    let predictorName = "Greeting"

    [<Literal>]
    let description = "A predictor that suggests a greeting based on the input."

    [<Literal>]
    let suggestionPart1 = "'Hello "

    [<Literal>]
    let suggestionPart2 = ", PowerShell from F#!'"

    let greetingPattern = $"^{suggestionPart1}(?<removal>.+){suggestionPart2}$" |> Regex

    interface ICommandPredictor with
        member __.Id = id
        member __.Name = predictorName
        member __.Description = description

        member __.GetSuggestion
            (client: PredictionClient, context: PredictionContext, cancellationToken: CancellationToken)
            : SuggestionPackage =

            let input = context.InputAst.Extent.Text

            if input |> String.IsNullOrWhiteSpace then
                Unchecked.defaultof<SuggestionPackage>
            else
                let suggestions =
                    greetingStore.Get()
                    |> Seq.choose (fun name ->
                        if name.Contains(input, StringComparison.OrdinalIgnoreCase) then
                            PredictiveSuggestion(
                                $"{suggestionPart1}{name}{suggestionPart2}",
                                "A friendly greeting from F#!"
                            )
                            |> Some
                        else
                            None)

                // NOTE: empty suggestionEntries is rejected by PowerShell's internal validation.
                if Seq.isEmpty suggestions then
                    Unchecked.defaultof<SuggestionPackage>
                else
                    // NOTE: SuggestionPackage must include a mini-session id; PowerShell uses it when calling OnSuggestionDisplayed/OnSuggestionAccepted.
                    let session = Threading.Interlocked.Increment(&miniSessionId) |> uint32
                    SuggestionPackage(session, suggestions |> Linq.Enumerable.ToList)

        member __.CanAcceptFeedback(client: PredictionClient, feedback: PredictorFeedbackKind) : bool =
            DebugLogger.WriteLine $"CanAcceptFeedback: Feedback kind: {feedback}"

            // NOTE: to capture events, must be return true for expected feedback kinds.
            feedback = PredictorFeedbackKind.SuggestionAccepted

        member __.OnSuggestionDisplayed(client: PredictionClient, session: uint32, countOrIndex: int) : unit =
            DebugLogger.WriteLine $"OnSuggestionDisplayed: Displayed suggestion at index: {countOrIndex}"

        member __.OnSuggestionAccepted(client: PredictionClient, session: uint32, acceptedSuggestion: string) : unit =
            DebugLogger.WriteLine $"OnSuggestionAccepted: Accepted suggestion: {acceptedSuggestion}"

            let matches = acceptedSuggestion |> greetingPattern.Match

            if matches.Captures.Count = 1 then
                let removal = matches.Groups.["removal"].Value
                removal |> greetingStore.Remove
                DebugLogger.WriteLine $"OnSuggestionAccepted: Removed greeting for: {removal}"

        member __.OnCommandLineAccepted(client: PredictionClient, history: IReadOnlyList<string>) : unit =
            DebugLogger.WriteLine $"OnCommandLineAccepted: Command line history count: {history.Count}"

        member __.OnCommandLineExecuted(client: PredictionClient, commandLine: string, success: bool) : unit =
            DebugLogger.WriteLine $"OnCommandLineExecuted: Command line: {commandLine}, Success: {success}"
