module AngouriMath.FSharp.MatrixOperators

open AngouriMath
open AngouriMath.FSharp.Core
open AngouriMath.FSharp.Functions

let (|*) a b = (parsed a * parsed b).InnerSimplified |> asMatrix

let (|**) a b = (parsed a ** parsed b).InnerSimplified |> asMatrix

let (|+) a b = (parsed a + parsed b).InnerSimplified |> asMatrix

let (|-) a b = (parsed a - parsed b).InnerSimplified |> asMatrix

let (|/) a b = (parsed a / parsed b).InnerSimplified |> asMatrix

/// Creates a square 2x2 matrix, with four elements
/// in the following order: left-top, right-top,
/// left-bottom, right-bottom
let matrix2x2 leftTop rightTop leftBottom rightBottom = 
  matrix [[leftTop; rightTop]; [leftBottom; rightBottom]]

/// Creates a vector of two elements
let vector2 a b = vector [a; b]

/// Creates a new matrix with each
/// cell's coordinates mapped to
/// an element (first comes the number of
/// the row, then the number of column,
/// starting from 0)
let newMatrix rows cols map = MathS.ZeroMatrix(rows, cols).With(new System.Func<int, int, Entity, Entity>(fun a b _ -> map a b))
