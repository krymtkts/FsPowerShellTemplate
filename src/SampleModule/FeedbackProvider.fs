namespace SampleModule

open System
open System.Collections
open System.Management.Automation.Subsystem.Feedback

type GreetingFeedbackProvider(guid: string) =
    let id = Guid.Parse(guid)

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
            let header = "Greeting Feedback"

            FeedbackItem(
                header,
                [ "Was the greeting helpful?"; "Did you like the style of the greeting?" ]
                |> Generic.List<string>,
                "Feedback for the Greeting Predictor",
                FeedbackDisplayLayout.Portrait
            )
