module AngouriMath.Terminal.Lib.Consts

let EncodingPlainPrefix = "encp"

let EncodingLatexPrefix = "encl"

type ExecutionResult =
    | SuccessPackageAdded
    | Error of string
    | VoidSuccess
    | PlainTextSuccess of string
    | LatexSuccess of Latex : string * Source : string
    | EndOfFile