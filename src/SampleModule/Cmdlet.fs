namespace SampleModule

open System.Management.Automation

[<Cmdlet(VerbsCommon.Get, "Greeting")>]
[<OutputType(typeof<string>)>]
type GetGreetingCommand() =
    inherit Cmdlet()

    [<Parameter(Position = 0,
                Mandatory = true,
                ValueFromPipeline = true,
                ValueFromPipelineByPropertyName = true,
                HelpMessage = "Who to greet.")>]
    member val Name = "" with get, set

    override __.BeginProcessing() = ()

    override __.ProcessRecord() : unit =
        $"Hello {__.Name}, PowerShell from F#!" |> __.WriteObject

    override __.EndProcessing() = ()
