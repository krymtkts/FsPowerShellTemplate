namespace SampleModule

open System.Management.Automation

open SampleModule.Core

[<Cmdlet(VerbsCommon.Add, "Greeting")>]
type AddGreetingCommand() =
    inherit Cmdlet()

    [<Parameter(Position = 0,
                Mandatory = true,
                ValueFromPipeline = true,
                ValueFromPipelineByPropertyName = true,
                HelpMessage = "Who to greet.")>]
    [<ValidateNotNullOrWhiteSpace>]
    member val Name = "" with get, set

    override __.BeginProcessing() = ()

    override __.ProcessRecord() : unit = __.Name |> greetingStore.Add

    override __.EndProcessing() = ()

[<Cmdlet(VerbsCommon.Get, "Greeting")>]
[<OutputType(typeof<string>)>]
type GetGreetingCommand() =
    inherit Cmdlet()

    override __.BeginProcessing() = ()

    override __.ProcessRecord() : unit =
        greetingStore.Get() |> Seq.iter __.WriteObject

    override __.EndProcessing() = ()
