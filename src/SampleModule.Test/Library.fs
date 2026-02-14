namespace SampleModule.Test

open Expecto
open Expecto.Flip

module Core =
    [<Tests>]
    let tests =
        testList
            "Dummy tests for SampleModule.Core"
            [

              test "Dummy test" {

                  1 |> Expect.equal "One should be equal to one." 1

              }

              ]
