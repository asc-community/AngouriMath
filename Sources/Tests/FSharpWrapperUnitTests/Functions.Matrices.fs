module ReturnValues.FunctionsMatricesTest

open AngouriMath.FSharp.Core
open AngouriMath.FSharp.Functions
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