namespace SampleModule

open System
open System.Collections
open System.Management.Automation.Subsystem.Feedback

open SampleModule.Core

type GreetingFeedbackProvider(guid: string) =
    let id = guid |> Guid.Parse

    [<Literal>]
    let name = "Greeting"

    [<Literal>]
    let description =
        "A feedback provider that handles feedback for the greeting predictor."

    interface IFeedbackProvider with
        member __.Id = id
        member __.Name = name
        member __.Description = description
        member __.FunctionsToDefine = null
        member __.Trigger: FeedbackTrigger = FeedbackTrigger.Success

        member __.GetFeedback(context: FeedbackContext, token: Threading.CancellationToken) : FeedbackItem | null =
            // NOTE: Provide feedback only when there is an update.
            // NOTE: Using the greeting store directly here for simplicity and context does not provide suggestion acceptance status.
            if greetingStore.ConsumeUpdated() then
                let header = "Greeting Feedback"

                FeedbackItem(
                    header,
                    [ $"You have {greetingStore.Count()} greetings stored."
                      "Thank you for using the Greeting Predictor!" ]
                    |> Generic.List<string>,
                    "Feedback for the Greeting Predictor",
                    FeedbackDisplayLayout.Portrait
                )
            else
                null
