namespace SampleModule.Test

open System
open System.Management.Automation
open System.Management.Automation.Runspaces

open Expecto

open SampleModule
open SampleModule.Core

module CmdletTests =

    let createRunspace () : Runspace =
        let iss = InitialSessionState.CreateDefault2()

        iss.Commands.Add(SessionStateCmdletEntry("Add-Greeting", typeof<AddGreetingCommand>, null))
        iss.Commands.Add(SessionStateCmdletEntry("Get-Greeting", typeof<GetGreetingCommand>, null))

        let runspace = RunspaceFactory.CreateRunspace(iss)
        runspace.Open()
        runspace

    let invoke
        (runspace: Runspace)
        (commandName: string)
        (parameters: (string * obj) list)
        (pipelineInput: obj list)
        : PSObject list * ErrorRecord list =
        use ps = PowerShell.Create()
        ps.Runspace <- runspace

        commandName |> ps.AddCommand |> ignore

        for name, value in parameters do
            ps.AddParameter(name, value) |> ignore

        let output =
            match pipelineInput with
            | [] -> ps.Invoke()
            | _ -> pipelineInput |> ps.Invoke
            |> Seq.toList

        let errors = ps.Streams.Error |> Seq.toList
        output, errors

    [<Tests>]
    let tests =
        testList
            "SampleModule.Cmdlet"
            [
                test "Add-Greeting accepts -Name and writes no errors" {
                    let name = $"t-{Guid.NewGuid():N}"
                    use runspace = createRunspace ()

                    try
                        let output, errors = invoke runspace "Add-Greeting" [ "Name", name ] []
                        Expect.isEmpty errors "Add-Greeting should not write errors"
                        Expect.isEmpty output "Add-Greeting should not write output"
                    finally
                        greetingStore.Remove name
                }

                test "Get-Greeting returns added name (WriteObject via runtime)" {
                    let name = $"t-{Guid.NewGuid():N}"
                    use runspace = createRunspace ()

                    try
                        let _output1, errors1 = invoke runspace "Add-Greeting" [] [ name ]
                        Expect.isEmpty errors1 "Add-Greeting (pipeline) should not write errors"

                        let output2, errors2 = invoke runspace "Get-Greeting" [] []
                        Expect.isEmpty errors2 "Get-Greeting should not write errors"

                        let names = output2 |> List.map (fun o -> string o.BaseObject)
                        Expect.isTrue (names |> List.contains name) "Get-Greeting output should contain the added name"
                    finally
                        greetingStore.Remove name
                }
            ]
