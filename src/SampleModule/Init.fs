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

    interface IModuleAssemblyCleanup with
        member __.OnRemove(psModuleInfo: PSModuleInfo) =
            SubsystemManager.UnregisterSubsystem(SubsystemKind.CommandPredictor, Guid.Parse(identifier))
