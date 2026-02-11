namespace SampleModule

open System.Threading
open System.Collections

module Core =
    type GreetingStore() =
        [<Literal>]
        let dirtyFlag = 1

        [<Literal>]
        let cleanFlag = 0

        let gate = obj ()
        let names = Generic.List<string>()
        let mutable dirty = cleanFlag

        member __.Add(name: string) =
            lock gate (fun () ->
                name |> names.Add
                dirty <- dirtyFlag)

        member __.Get() : seq<string> =
            lock gate (fun () ->
                // NOTE: Return a snapshot to avoid enumeration issues with concurrent updates.
                names |> Seq.toArray :> seq<string>)

        member __.Count() : int = lock gate (fun () -> names.Count)

        member __.Remove(name: string) =
            lock gate (fun () ->
                if name |> names.Remove then
                    dirty <- dirtyFlag)

        member __.ConsumeUpdated() : bool =
            Interlocked.Exchange(&dirty, cleanFlag) = dirtyFlag

    let greetingStore = GreetingStore()
