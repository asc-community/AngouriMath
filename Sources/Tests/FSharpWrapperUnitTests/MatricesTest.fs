module AngouriMath.FSharp.Tests.Matrices

open AngouriMath
open AngouriMath.FSharp.Matrices
open Xunit

(*

 Matrix and matrix operators
 M = matrix

*)

[<Fact>]
let ``M plus M`` () =
    Assert.Equal<Entity.Matrix>(vector [11; 32], vector [10; 30] +. vector [1; 2])

[<Fact>]
let ``M minus M`` () =
    Assert.Equal<Entity.Matrix>(vector [9; 28], vector [10; 30] -. vector [1; 2])

[<Fact>]
let ``M multiply M`` () =
    Assert.Equal<Entity.Matrix>(vector [2; 4], "[[2, 0], [0, 2]]" *. "[1, 2]")

(*

 Matrix and scalar operators
 Scalar and matrix operators
 M = matrix
 S = scalar

*)

[<Fact>]
let ``M plus S`` () =
    Assert.Equal<Entity.Matrix>(vector [11; 31], vector [10; 30] +. 1)

[<Fact>]
let ``S plus M`` () =
    Assert.Equal<Entity.Matrix>(vector [11; 31], 1 +. vector [10; 30])

[<Fact>]
let ``M minus S`` () =
    Assert.Equal<Entity.Matrix>(vector [9; 29], vector [10; 30] -. 1)

[<Fact>]
let ``S minus M`` () =
    Assert.Equal<Entity.Matrix>(vector [90; 70], 100 -. vector [10; 30])

[<Fact>]
let ``M multiply S`` () =
    Assert.Equal<Entity.Matrix>(vector [3; 6], vector [1; 2] *. 3)

[<Fact>]
let ``S multiply M`` () =
    Assert.Equal<Entity.Matrix>(vector [3; 6], 3 *. vector [1; 2])

[<Fact>]
let ``M divide S`` () =
    Assert.Equal<Entity.Matrix>(vector [5; 15], vector [10; 30] /. 2)

[<Fact>]
let ``M power S`` () =
    Assert.Equal<Entity.Matrix>(matrix [[37; 54]; [81; 118]], matrix [[1; 2]; [3; 4]] **. 3)

(*

Matrix operator priority

*)

open AngouriMath.FSharp.Core

let a = matrixWith 2 2 (fun _ _ -> parsed "a")
let b = matrixWith 2 2 (fun _ _ -> parsed "b")
let c = matrixWith 2 2 (fun _ _ -> parsed "c")

(*

Make sure +. and -. have the same priority

*)

[<Fact>]
let ``Priority minus dot plus dot`` () =
    Assert.Equal<Entity.Matrix>(a -. b +. c, (a -. b) +. c) 

[<Fact>]
let ``Priority plus dot minus dot`` () =
    Assert.Equal<Entity.Matrix>(a +. b -. c, (a +. b) -. c)


(*

Make sure **. has a higher priority than *., +., -.

*)

[<Fact>]
let ``Priority power dot plus dot`` () =
    Assert.Equal<Entity.Matrix>(a **. b +. c, (a **. b) +. c) 

[<Fact>]
let ``Priority plus dot power dot`` () =
    Assert.Equal<Entity.Matrix>(a +. b **. c, a +. (b **. c))
    
[<Fact>]
let ``Priority power dot minus dot`` () =
    Assert.Equal<Entity.Matrix>(a **. b -. c, (a **. b) -. c) 

[<Fact>]
let ``Priority minus dot power dot`` () =
    Assert.Equal<Entity.Matrix>(a -. b **. c, a -. (b **. c))
    
    
[<Fact>]
let ``Priority power dot multiply dot`` () =
    Assert.Equal<Entity.Matrix>(a **. b *. c, (a **. b) *. c) 

[<Fact>]
let ``Priority multiply dot power dot`` () =
    Assert.Equal<Entity.Matrix>(a *. b **. c, a *. (b **. c))

(*

Make sure that *. has a higher priority than both +. and -.

*)


[<Fact>]
let ``Priority plus dot multiply dot`` () =
    Assert.Equal<Entity.Matrix>(a +. b *. c, a +. (b *. c))
   
[<Fact>]
let ``Priority multiply dot plus dot`` () =
    Assert.Equal<Entity.Matrix>(a *. b +. c, (a *. b) +. c)
   
[<Fact>]
let ``Priority minus dot multiply dot`` () =
    Assert.Equal<Entity.Matrix>(a -. b *. c, a -. (b *. c))
   
[<Fact>]
let ``Priority multiply dot minus dot`` () =
    Assert.Equal<Entity.Matrix>(a *. b -. c, (a *. b) -. c)


(*

Priority tests for mixed | and non |

*)
   

[<Fact>]
let ``Priority plus multiply dot`` () =
    Assert.Equal<Entity.Matrix>(a + b *. c, a + (b *. c))
   
[<Fact>]
let ``Priority multiply plus dot`` () =
    Assert.Equal<Entity.Matrix>(a * b +. c, (a * b) +. c)
   
[<Fact>]
let ``Priority minus multiply dot`` () =
    Assert.Equal<Entity.Matrix>(a - b *. c, a - (b *. c))
   
[<Fact>]
let ``Priority multiple minus dot`` () =
    Assert.Equal<Entity.Matrix>(a * b -. c, (a * b) -. c)
   



[<Fact>]
let ``Priority plus dot multiply`` () =
    Assert.Equal<Entity.Matrix>(a +. b * c, a +. (b * c))
   
[<Fact>]
let ``Priority multiply dot plus`` () =
    Assert.Equal<Entity.Matrix>(a *. b + c, (a *. b) + c)
   
[<Fact>]
let ``Priority minus dot multiply`` () =
    Assert.Equal<Entity.Matrix>(a -. b * c, a -. (b * c))
   
[<Fact>]
let ``Priority multiply dot minus`` () =
    Assert.Equal<Entity.Matrix>(a *. b - c, (a *. b) - c)
