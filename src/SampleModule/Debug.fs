namespace SampleModule

[<AutoOpen>]
module DebugLogging =
    open System.Runtime.CompilerServices
    open System.Runtime.InteropServices
    open System.Diagnostics

#if DEBUG
    open System
    open System.IO

    let lockObj = new obj ()

    [<Literal>]
    let logPath = "./debug.log"
#endif

    // NOTE: Tiny debug logger only for DEBUG build.
    // NOTE: It make easier to track the flow when debugging the PowerShell predictor and feedback provider.
    [<AbstractClass; Sealed>]
    type DebugLogger =

        [<Conditional("DEBUG")>]
        static member WriteLine
            (
                res: string,
                [<Optional; DefaultParameterValue(""); CallerMemberName>] caller: string,
                [<Optional; DefaultParameterValue(""); CallerFilePath>] path: string,
                [<Optional; DefaultParameterValue(0); CallerLineNumber>] line: int
            ) =
#if DEBUG
            // NOTE: lock to avoid another process error when dotnet test.
            lock lockObj (fun () ->
                use sw = new StreamWriter(logPath, true)

                fprintfn
                    sw
                    "[%s] %s at %d %s <%A>"
                    (DateTimeOffset.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.fffzzz"))
                    path
                    line
                    caller
                    res

            )
#else
            ()
#endif
