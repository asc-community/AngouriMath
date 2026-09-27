module AngouriMath.Terminal.Lib.PreRunCode

open AngouriMath.Terminal.Lib.FSharpInteractive

let OpensAndOperators = @"
open AngouriMath
open AngouriMath.FSharp.Core
open AngouriMath.FSharp.Functions
open AngouriMath.FSharp.Matrices
open AngouriMath.FSharp.Shortcuts
open AngouriMath.FSharp.Constants
open AngouriMath.Interactive

let eval (x : obj) = 
    match (parsed x).InnerSimplified with
    | :? Entity.Number.Integer as i -> i.ToString()
    | :? Entity.Number.Rational as i -> i.RealPart.EDecimal.ToString()
    | :? Entity.Number.Real as re -> re.RealPart.EDecimal.ToString()
    | :? Entity.Number.Complex as cx -> cx.RealPart.EDecimal.ToString() + "" + "" + cx.ImaginaryPart.EDecimal.ToString() + ""i""
    | other -> (evaled other).ToString()

/// Every value a cell has produced, oldest first: `run.[0]` is the first, `run.[run.Count - 1]`
/// the last, and `back 1` the last, `back 2` the one before it. F# Interactive's own `it` is
/// the last value only. Declared before the arithmetic operators are opened, since under them
/// `run.Count - n` would build an expression rather than an index. https://github.com/asc-community/AngouriMath/issues/601
let run = ResizeArray<obj> ()

let back (n : int) = run.[run.Count - n]

open AngouriMath.Interactive.ArithmeticOperators

let x = symbol ""x""
let y = symbol ""y""
let z = symbol ""z""
let w = symbol ""w""
let a = symbol ""a""
let b = symbol ""b""
let c = symbol ""c""
let d = symbol ""d""
let n = symbol ""n""
let m = symbol ""m""

let help () =
    let url = ""https://github.com/asc-community/AngouriMath/wiki/Terminal""
    let psi = System.Diagnostics.ProcessStartInfo ()
    psi.FileName <- url
    psi.UseShellExecute <- true
    System.Diagnostics.Process.Start psi
    $""Sending you to {url}""

/// The page `render` opens: the expression's LaTeX -- what `latex` gives -- typeset by MathJax,
/// with the LaTeX itself beneath it, written to the temporary directory. Joined with
/// String.concat, since `+` builds an expression here.
/// https://github.com/asc-community/AngouriMath/issues/549
let renderedPage (x : obj) =
    let source = System.Net.WebUtility.HtmlEncode (latex x)
    let page = System.IO.Path.Combine (System.IO.Path.GetTempPath (), $""angourimath-{System.Guid.NewGuid ()}.html"")
    let html =
        String.concat """" [
            ""<!DOCTYPE html><html><head><meta charset='utf-8'><title>AngouriMath</title>""
            ""<script id='MathJax-script' async src='https://cdn.jsdelivr.net/npm/mathjax@3/es5/tex-mml-chtml.js'></script>""
            ""</head><body><p>$$""; source; ""$$</p><pre>""; source; ""</pre></body></html>"" ]
    System.IO.File.WriteAllText (page, html)
    page

/// The expression typeset in the browser, as Plot's charts are shown; `latex` gives its LaTeX
/// as text. https://github.com/asc-community/AngouriMath/issues/549
let render (x : obj) =
    let page = renderedPage x
    let psi = System.Diagnostics.ProcessStartInfo page
    psi.UseShellExecute <- true
    try
        System.Diagnostics.Process.Start psi |> ignore
        printfn $""Showing {page} in the browser""
    with _ ->
        printfn $""Wrote {page}, and no browser could be started to show it""

/// Arithmetic here builds expressions, so `3 / 2` is a rational rather than 1 and `x + 1` is
/// an expression rather than an error. The cost is that ordinary F# whose arithmetic must stay
/// primitive does not typecheck: `let rec fib n = if n < 2 then n else fib (n-1) + fib (n-2)`
/// is the usual example. These two say which reading you want, at any point in a session, and
/// either can follow the other.
///   open Microsoft.FSharp.Core.Operators           // ordinary F#: fib compiles, 3 / 2 is 1
///   open AngouriMath.Interactive.ArithmeticOperators  // back to expressions
/// https://github.com/asc-community/AngouriMath/issues/1133
let operators () =
    // Printed rather than returned: what a cell returns is handed to AngouriMath's formatter,
    // which parses it, and a sentence is not an expression.
    printfn ""Arithmetic builds expressions here, so 3 / 2 is 3/2 and x + 1 is an expression.""
    printfn ""For ordinary F# - such as""
    printfn ""    let rec fib n = if n < 2 then n else fib (n-1) + fib (n-2)""
    printfn ""- type""
    printfn ""    open Microsoft.FSharp.Core.Operators""
    printfn ""and to come back to expressions,""
    printfn ""    open AngouriMath.Interactive.ArithmeticOperators""
"

let enableAngouriMath kernel =
    let innerCode = OpensAndOperators.Replace("\"", "\\\"")
    let preRunCode = OpensAndOperators + $"let preRunCode = \"{innerCode}\""
    execute kernel preRunCode
