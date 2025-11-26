module Tests

open Xunit
open Swensen.Unquote

[<Fact>]
let ``My test with Unquote`` () =
    test <@ 1 + 1 = 2 @>

[<Fact>]
let ``Another test`` () =
    let result = "hello" + " " + "world"
    test <@ result = "hello world" @>
