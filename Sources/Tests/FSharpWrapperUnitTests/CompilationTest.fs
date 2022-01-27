module AngouriMath.FSharp.Tests.Compilation

open AngouriMath.FSharp.Compilation
open Xunit

[<Fact>]
let ``Compile from 1`` () = 
    Assert.Equal(32, (compiled1In<int, int> "x" "x * 5 + 7") 5)
[<Fact>]
let ``Compile from 2`` () = 
    Assert.Equal(32, (compiled2In<int, int, int> "x" "y" "x * 5 + y") 5 7)
[<Fact>]
let ``Compile from 3`` () = 
    Assert.Equal(1188, (compiled3In<int, int, int, int> "x" "y" "z" "(x + 1)(y + 2)(z + 3)") 5 7 19)
[<Fact>]
let ``Compile from 4`` () = 
    Assert.Equal(1191, (compiled4In<int, int, int, int, int> "x" "y" "z" "duck" "(x + 1)(y + 2)(z + 3) + duck") 5 7 19 3)
