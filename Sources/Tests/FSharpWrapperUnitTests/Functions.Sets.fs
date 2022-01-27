module AngouriMath.FSharp.Tests.FunctionsTest

open Xunit
open AngouriMath.FSharp.Functions
open AngouriMath.FSharp.Core

[<Fact>]
let ``Closed interval test`` () =
    Assert.Equal(parsed "[2; 3]", closedInterval 2 3)
[<Fact>]
let ``Open interval test`` () =
    Assert.Equal(parsed "(2; 3)", openInterval 2 3)
[<Fact>]
let ``LeftInclusiveRightExclusive interval test`` () =
    Assert.Equal(parsed "[2; 3)", leftInclusiveRightExclusive 2 3)
[<Fact>]
let ``LeftExclusiveRightInclusive interval test`` () =
    Assert.Equal(parsed "(2; 3]", leftExclusiveRightInclusive 2 3)
[<Fact>]
let ``LeftInclusive interval test`` () =
    Assert.Equal(parsed "[2; +oo)", leftInclusive 2)
[<Fact>]
let ``LeftExclusive interval test`` () =
    Assert.Equal(parsed "(2; +oo)", leftExclusive 2)
[<Fact>]
let ``RightInclusive interval test`` () =
    Assert.Equal(parsed "(-oo; 2]", rightInclusive 2)
[<Fact>]
let ``RightExclusive interval test`` () =
    Assert.Equal(parsed "(-oo; 2)", rightExclusive 2)