namespace SampleModule

open System
open System.Management.Automation
open System.Management.Automation.Subsystem

type Init() =
    [<Literal>]
    let identifier = "394e28cb-7f3f-4ef9-ab73-8172763ba4ac"

    interface IModuleAssemblyInitializer with
        member __.OnImport() =
            let p = new GreetingPredictor(identifier)
            SubsystemManager.RegisterSubsystem(SubsystemKind.CommandPredictor, p)
            let f = new GreetingFeedbackProvider(identifier)
            SubsystemManager.RegisterSubsystem(SubsystemKind.FeedbackProvider, f)

    interface IModuleAssemblyCleanup with
        member __.OnRemove(psModuleInfo: PSModuleInfo) =
            let guid = identifier |> Guid.Parse
            SubsystemManager.UnregisterSubsystem(SubsystemKind.CommandPredictor, guid)
            SubsystemManager.UnregisterSubsystem(SubsystemKind.FeedbackProvider, guid)
