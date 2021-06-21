module AngouriMath.FSharp.Matrices

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

/// Creates a square 3x3 matrix, going first from left
/// to right, then from top to bottom (the same way
/// people in Europe read)
let matrix3x3 leftTop middleTop rightTop leftMiddle middleMiddle rightMiddle leftBottom middleBottom rightBottom =
    matrix [[leftTop; middleTop; rightTop]; [leftMiddle; middleMiddle; rightMiddle]; [leftBottom; middleBottom; rightBottom]]

/// Creates a vector of two elements
let vector2 a b = vector [a; b]

/// Creates a vector of three elements
let vector3 a b c = vector [a; b; c]

/// Creates a new matrix with each
/// cell's coordinates mapped to
/// an element (first comes the number of
/// the row, then the number of column,
/// starting from 0)
let matrixWith rows cols map = MathS.Matrix(rows, cols, new System.Func<int, int, Entity>(map))

/// Creates a new vector with
/// each element mapped from its
/// position (indexed from 0)
/// to the value via the map function
let vectorWith count map = matrixWith count 1 (fun r _ -> map r)

/// Gets the determinant of a square matrix, or None on invalid inputs
let det (m: AngouriMath.Entity.Matrix) = 
    match m.Determinant with
    | null -> None
    | n -> Some(n)

/// Gets the adjugate form of a square matrix, or None on invalid inputs
let adjugate (m: AngouriMath.Entity.Matrix) = 
    match m.Adjugate with
    | null -> None
    | n -> Some(n)

/// Gets the inverse of a square matrix, or None on invalid inputs or if it does not exist
let inverse (m: AngouriMath.Entity.Matrix) = 
    match m.Inverse with
    | null -> None
    | n -> Some(n)
