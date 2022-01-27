module AngouriMath.FSharp.Tests.Common

open AngouriMath.FSharp.Core
open Xunit



let x = symbol "x"
let x_ = symbol "x"

[<Fact>]
let ``Test symbol x`` () = Assert.Equal(x, x_)
