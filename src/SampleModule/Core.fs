namespace SampleModule

open System.Collections

module Core =
    type GreetingStore() =
        let names = Generic.List<string>()

        member __.Add(name: string) = name |> names.Add

        member __.Get() = names :> seq<string>

    let greetingStore = GreetingStore()
