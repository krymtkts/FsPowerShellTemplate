namespace SampleModule.Test

open System.Threading.Tasks

open Expecto
open Expecto.Flip
open FsCheck
open FsCheck.FSharp

open SampleModule.Core

module CoreTests =
    [<Struct>]
    [<RequireQualifiedAccess>]
    type Op =
        | Add of string
        | Remove of string
        | ConsumeUpdated

    let private removeFirst (value: string) (items: string list) : string list * bool =
        let rec loop acc rest =
            match rest with
            | [] -> List.rev acc, false
            | x :: xs when x = value -> List.rev acc @ xs, true
            | x :: xs -> loop (x :: acc) xs

        loop [] items

    type private Model = { Names: string list; Dirty: bool }

    let private applyModel (op: Op) (model: Model) : Model * bool option =
        match op with
        | Op.Add name ->
            {
                Names = model.Names @ [ name ]
                Dirty = true
            },
            None
        | Op.Remove name ->
            let names, removed = model.Names |> removeFirst name

            {
                Names = names
                Dirty = model.Dirty || removed
            },
            None
        | Op.ConsumeUpdated -> { model with Dirty = false }, Some model.Dirty

    let private applyStore (op: Op) (store: GreetingStore) : bool option =
        match op with
        | Op.Add name ->
            store.Add name
            None
        | Op.Remove name ->
            store.Remove name
            None
        | Op.ConsumeUpdated -> store.ConsumeUpdated() |> Some

    type private StepWeights =
        {
            add: int
            removeHit: int
            removeMiss: int
            consume: int
        }

    module private OpGenWeights =
        let withBag: StepWeights =
            {
                add = 4
                removeHit = 4
                removeMiss = 1
                consume = 1
            }

        let withoutBag: StepWeights =
            {
                add = 7
                removeHit = 0
                removeMiss = 1
                consume = 2
            }

    type private Generators =
        static member Ops() : Arbitrary<Op list> =
            let genName: Gen<string> =
                let arb = ArbMap.defaults.ArbFor<NonEmptyString>()
                arb.Generator |> Gen.map (fun s -> s.Get)

            let genMissName: Gen<string> = genName |> Gen.map (fun s -> $"miss-{s}")

            let genOps: Gen<Op list> =
                Gen.sized (fun size ->
                    let maxLen = min 200 (size + 10)

                    let genAdd (bag: string list) : Gen<Op * string list> =
                        genName |> Gen.map (fun name -> Op.Add name, bag @ [ name ])

                    let genRemoveMiss (bag: string list) : Gen<Op * string list> =
                        genMissName |> Gen.map (fun name -> Op.Remove name, bag)

                    let genConsume (bag: string list) : Gen<Op * string list> = Gen.constant (Op.ConsumeUpdated, bag)

                    let genRemoveHit (bag: string list) : Gen<Op * string list> =
                        Gen.elements bag
                        |> Gen.map (fun name ->
                            let bag', _removed = removeFirst name bag
                            Op.Remove name, bag')

                    let genStep (bag: string list) : Gen<Op * string list> =
                        match bag with
                        | [] ->
                            Gen.frequency
                                [
                                    OpGenWeights.withoutBag.add, genAdd bag
                                    OpGenWeights.withoutBag.removeMiss, genRemoveMiss bag
                                    OpGenWeights.withoutBag.consume, genConsume bag
                                ]
                        | _ ->
                            Gen.frequency
                                [
                                    OpGenWeights.withBag.add, genAdd bag
                                    OpGenWeights.withBag.removeHit, genRemoveHit bag
                                    OpGenWeights.withBag.removeMiss, genRemoveMiss bag
                                    OpGenWeights.withBag.consume, genConsume bag
                                ]

                    let rec genOpsLen (bag: string list) (len: int) : Gen<Op list> =
                        if len <= 0 then
                            Gen.constant []
                        else
                            genStep bag
                            |> Gen.bind (fun (op, bag') ->
                                genOpsLen bag' (len - 1) |> Gen.map (fun rest -> op :: rest))

                    Gen.choose (0, maxLen) |> Gen.bind (fun len -> genOpsLen [] len))

            Arb.fromGen genOps

    let private printFsCheckStats (testName: string) (data: FsCheckTestData) : unit =
        printfn "FsCheck summary (%s): tests=%d shrinks=%d" testName data.NumberOfTests data.NumberOfShrinks

        let labels = data.Labels |> Seq.toList |> List.sort

        if not labels.IsEmpty then
            labels |> String.concat ", " |> printfn "FsCheck labels (%s): %s" testName

        let stamps =
            data.Stamps
            |> Seq.map (fun (count, stampLabels) -> count, stampLabels |> Seq.toList)
            |> Seq.sortByDescending fst
            |> Seq.toList

        let stampTotal = stamps |> List.sumBy fst

        let nonZeroStamps = stamps |> List.filter (fun (count, _labels) -> count > 0)

        let omittedZeroCount = stamps.Length - nonZeroStamps.Length

        if not stamps.IsEmpty then
            printfn "FsCheck stamps (%s): (total=%d, omittedZero=%d)" testName stampTotal omittedZeroCount

            nonZeroStamps
            |> List.truncate 30
            |> List.iter (fun (count, stampLabels) -> stampLabels |> String.concat " | " |> printfn "  %4d  %s" count)

            if nonZeroStamps.Length > 30 then
                printfn "  ... (%d more)" (nonZeroStamps.Length - 30)

    let private fsCheckConfig =
        { FsCheckConfig.defaultConfig with
            maxTest = 1000
            arbitrary = [ typeof<Generators> ]
            finishedTest = fun _config testName data -> async { printFsCheckStats testName data }
        }

    let private propGreetingStoreMatchesModel (ops: Op list) : Property =
        let store = GreetingStore()

        let addCount = ops |> List.filter _.IsAdd |> List.length
        let removeCount = ops |> List.filter _.IsRemove |> List.length
        let consumeCount = ops |> List.filter _.IsConsumeUpdated |> List.length

        let removeHitCount =
            let folder (names: string list, hitCount: int) (op: Op) =
                match op with
                | Op.Add name -> names @ [ name ], hitCount
                | Op.Remove name ->
                    let names', removed = removeFirst name names
                    names', hitCount + (if removed then 1 else 0)
                | Op.ConsumeUpdated -> names, hitCount

            ops |> List.fold folder ([], 0) |> snd

        let lengthBucket =
            match ops.Length with
            | 0 -> "len=0"
            | n when n <= 5 -> "len<=5"
            | n when n <= 10 -> "len<=10"
            | n when n <= 20 -> "len<=20"
            | n when n <= 50 -> "len<=50"
            | n when n <= 100 -> "len<=100"
            | n when n <= 200 -> "len<=200"
            | _ -> "len>200"

        let rec loop (model: Model) (remaining: Op list) : bool =
            match remaining with
            | [] -> store.Count() = model.Names.Length && store.Get() |> Seq.toList = model.Names
            | op :: rest ->
                let model', expectedConsume = applyModel op model
                let actualConsume = applyStore op store

                let consumeOk =
                    match expectedConsume, actualConsume with
                    | None, None -> true
                    | Some expected, Some actual -> expected = actual
                    | _ -> false

                let stateOk =
                    store.Count() = model'.Names.Length && store.Get() |> Seq.toList = model'.Names

                consumeOk && stateOk && loop model' rest

        let ok = loop { Names = []; Dirty = false } ops

        // NOTE: Classify cases to better understand the distribution of generated test cases and identify any gaps.
        ok
        |> Prop.ofTestable
        |> Prop.collect lengthBucket
        |> Prop.classify (consumeCount > 0) "has ConsumeUpdated"
        |> Prop.classify (consumeCount = 0) "no ConsumeUpdated"
        |> Prop.classify (addCount = 0) "no Add"
        |> Prop.classify (removeCount = 0) "no Remove"
        |> Prop.classify (removeHitCount > 0) "has Remove hit"
        |> Prop.classify (removeHitCount = 0) "no Remove hit"

    [<Tests>]
    let tests =
        testList
            "SampleModule.Core"
            [
                // NOTE: Property-based test to validate that GreetingStore's behavior matches the expected model under various operation sequences.
                testPropertyWithConfig
                    fsCheckConfig
                    "GreetingStore matches model (Add/Remove/ConsumeUpdated)"
                    propGreetingStoreMatchesModel

                test "Get returns a snapshot" {
                    let store = GreetingStore()
                    store.Add "a"

                    let snapshot = store.Get()
                    store.Add "b"

                    snapshot
                    |> Seq.toList
                    |> Expect.equal "Snapshot should not include later additions" [ "a" ]
                }

                test "Concurrent Add is safe" {
                    let store = GreetingStore()

                    let addPerTask = 500
                    let taskCount = 8

                    let tasks =
                        [ 1..taskCount ]
                        |> List.map (fun taskId ->
                            Task.Run(fun () ->
                                for i in 1..addPerTask do
                                    store.Add $"t{taskId}-{i}"))
                        |> List.toArray

                    Task.WhenAll(tasks).GetAwaiter().GetResult()

                    store.Count()
                    |> Expect.equal "Count should match total adds" (addPerTask * taskCount)
                }
            ]
