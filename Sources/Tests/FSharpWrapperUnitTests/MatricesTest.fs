module MatricesTest

open AngouriMath
open AngouriMath.FSharp.Matrices
open Xunit

(*

 Matrix and matrix operators
 M = matrix

*)

[<Fact>]
let ``M+M`` () =
    Assert.Equal<Entity.Matrix>(vector [11; 32], vector [10; 30] +. vector [1; 2])

[<Fact>]
let ``M-M`` () =
    Assert.Equal<Entity.Matrix>(vector [9; 28], vector [10; 30] -. vector [1; 2])

[<Fact>]
let ``M*M`` () =
    Assert.Equal<Entity.Matrix>(vector [2; 4], "[[2, 0], [0, 2]]" *. "[1, 2]")

(*

 Matrix and scalar operators
 Scalar and matrix operators
 M = matrix
 S = scalar

*)

[<Fact>]
let ``M+S`` () =
    Assert.Equal<Entity.Matrix>(vector [11; 31], vector [10; 30] +. 1)

[<Fact>]
let ``S+M`` () =
    Assert.Equal<Entity.Matrix>(vector [11; 31], 1 +. vector [10; 30])

[<Fact>]
let ``M-S`` () =
    Assert.Equal<Entity.Matrix>(vector [9; 29], vector [10; 30] -. 1)

[<Fact>]
let ``S-M`` () =
    Assert.Equal<Entity.Matrix>(vector [90; 70], 100 -. vector [10; 30])

[<Fact>]
let ``M*S`` () =
    Assert.Equal<Entity.Matrix>(vector [3; 6], vector [1; 2] *. 3)

[<Fact>]
let ``S*M`` () =
    Assert.Equal<Entity.Matrix>(vector [3; 6], 3 *. vector [1; 2])

[<Fact>]
let ``M/S`` () =
    Assert.Equal<Entity.Matrix>(vector [5; 15], vector [10; 30] /. 2)

[<Fact>]
let ``M**S`` () =
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
let ``Priority -.+.`` () =
    Assert.Equal<Entity.Matrix>(a -. b +. c, (a -. b) +. c) 

[<Fact>]
let ``Priority +.-.`` () =
    Assert.Equal<Entity.Matrix>(a +. b -. c, (a +. b) -. c)


(*

Make sure **. has a higher priority than *., +., -.

*)

[<Fact>]
let ``Priority **.+.`` () =
    Assert.Equal<Entity.Matrix>(a **. b +. c, (a **. b) +. c) 

[<Fact>]
let ``Priority +.**.`` () =
    Assert.Equal<Entity.Matrix>(a +. b **. c, a +. (b **. c))
    
[<Fact>]
let ``Priority **.-.`` () =
    Assert.Equal<Entity.Matrix>(a **. b -. c, (a **. b) -. c) 

[<Fact>]
let ``Priority -.**.`` () =
    Assert.Equal<Entity.Matrix>(a -. b **. c, a -. (b **. c))
    
    
[<Fact>]
let ``Priority **.*.`` () =
    Assert.Equal<Entity.Matrix>(a **. b *. c, (a **. b) *. c) 

[<Fact>]
let ``Priority *.**.`` () =
    Assert.Equal<Entity.Matrix>(a *. b **. c, a *. (b **. c))

(*

Make sure that *. has a higher priority than both +. and -.

*)


[<Fact>]
let ``Priority +.*.`` () =
    Assert.Equal<Entity.Matrix>(a +. b *. c, a +. (b *. c))
   
[<Fact>]
let ``Priority *.+.`` () =
    Assert.Equal<Entity.Matrix>(a *. b +. c, (a *. b) +. c)
   
[<Fact>]
let ``Priority -.*.`` () =
    Assert.Equal<Entity.Matrix>(a -. b *. c, a -. (b *. c))
   
[<Fact>]
let ``Priority *.-.`` () =
    Assert.Equal<Entity.Matrix>(a *. b -. c, (a *. b) -. c)


(*

Priority tests for mixed | and non |

*)
   

[<Fact>]
let ``Priority +*.`` () =
    Assert.Equal<Entity.Matrix>(a + b *. c, a + (b *. c))
   
[<Fact>]
let ``Priority *+.`` () =
    Assert.Equal<Entity.Matrix>(a * b +. c, (a * b) +. c)
   
[<Fact>]
let ``Priority -*.`` () =
    Assert.Equal<Entity.Matrix>(a - b *. c, a - (b *. c))
   
[<Fact>]
let ``Priority *-.`` () =
    Assert.Equal<Entity.Matrix>(a * b -. c, (a * b) -. c)
   



[<Fact>]
let ``Priority +.*`` () =
    Assert.Equal<Entity.Matrix>(a +. b * c, a +. (b * c))
   
[<Fact>]
let ``Priority *.+`` () =
    Assert.Equal<Entity.Matrix>(a *. b + c, (a *. b) + c)
   
[<Fact>]
let ``Priority -.*`` () =
    Assert.Equal<Entity.Matrix>(a -. b * c, a -. (b * c))
   
[<Fact>]
let ``Priority *.-`` () =
    Assert.Equal<Entity.Matrix>(a *. b - c, (a *. b) - c)
