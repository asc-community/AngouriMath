module MatrixOperatorsTest

open AngouriMath.FSharp.Core
open AngouriMath.FSharp.Functions
open AngouriMath.FSharp.MatrixOperators
open Xunit
open AngouriMath

[<Fact>]
let ``M-M *`` () =
    Assert.Equal<Entity.Matrix>(vector [2; 4], "[[2, 0], [0, 2]]" |* "[1, 2]")

[<Fact>]
let ``M-S *`` () =
    Assert.Equal<Entity.Matrix>(vector [3; 6], (vector [1; 3]) |* 3)

[<Fact>]
let ``S-M *`` () =
    Assert.Equal<Entity.Matrix>(vector [3; 6], 3 |* (vector [1; 3]))