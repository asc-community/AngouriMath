module ReturnValues.FunctionsMatricesTest

open AngouriMath.FSharp.Core
open AngouriMath.FSharp.Functions
open Utils
open Xunit

[<Fact>]
let ``Vector test 1`` () = Assert.Equal(parsed "[1, 2, 3]", vector [1; 2; 3])
[<Fact>]
let ``Vector test 2`` () = Assert.Equal(parsed "[1, 2, 3, x]", vector ["1"; "2"; "3"; "x"])
[<Fact>]
let ``Vector test 3 transposed`` () = Assert.Equal(parsed "[1, 2, 3, x]T", transposed (vector ["1"; "2"; "3"; "x"]))
[<Fact>]
let ``Matrix test 1`` () = Assert.Equal(parsed "[[1, 2], [2, 3], [3, 4]]", matrix [[1; 2]; [2; 3]; [3; 4]])
[<Fact>]
let ``Matrix test 2 transposed`` () = Assert.Equal(parsed "[[1, 2], [2, 3], [3, 4]]T", transposed (matrix [[1; 2]; [2; 3]; [3; 4]]))
[<Fact>]
let ``Tensor product 1`` () = Assert.Equal(parsed "[a c, a d, b c, b d]", (asMatrix "[a, b]") *** (asMatrix "[c, d]"))
[<Fact>]
let ``Tensor product 2`` () = Assert.Equal(parsed "[a b]", (asMatrix "a") *** (asMatrix "b"))
[<Fact>]
let ``Tensor power 1`` () = Assert.Equal(parsed "[a2, a b, b a, b2]", (asMatrix "[a, b]") **** 2)
[<Fact>]
let ``Tensor power 2`` () = Assert.Equal(parsed "[a2 * a]", (asMatrix "a") **** 3)


open AngouriMath.FSharp.MatrixOperators

[<Fact>]
let ``matrix 2x2 func`` () = testEqual (parsed "[[a, b], [c, d]]", matrix2x2 "a" "b" "c" "d")
[<Fact>]
let ``matrix 3x3 func`` () = testEqual (parsed "[[a, b, c], [d, e, f], [g, h, e]]", matrix3x3 "a" "b" "c" "d" "e" "f" "g" "h" "e")
[<Fact>]
let ``vector 2 func`` () = testEqual (parsed "[a, b]", vector2 "a" "b")
[<Fact>]
let ``vector 3 func`` () = testEqual (parsed "[a, b, c]", vector3 "a" "b" "c")
[<Fact>]
let ``matrixWith func 1`` () = testEqual (parsed "[[a_11, a_12], [a_21, a_22]]", matrixWith 2 2 (fun r c -> upcast symbolIndexed "a" $"{r+1}{c+1}"))
[<Fact>]
let ``matrixWith func 2`` () = testEqual (parsed "[[a_11, a_12], [a_21, a_22], [a_31, a_32]]", matrixWith 3 2 (fun r c -> upcast symbolIndexed "a" $"{r+1}{c+1}"))
[<Fact>]
let ``matrixWith func 3`` () = testEqual (parsed "[66]", matrixWith 1 1 (fun r c -> parsed 66))
[<Fact>]
let ``vectorWith func`` () = testEqual (parsed "[1, 2, 3, 4]", vectorWith 4 (fun r -> parsed (r + 1)))

