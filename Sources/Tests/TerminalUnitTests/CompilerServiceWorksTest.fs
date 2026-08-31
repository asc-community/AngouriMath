/// The Terminal bundles FSharp.Compiler.Service beside an FSharp.Core two majors above the one
/// it declares it needs, and NU1608 has said so on every publish since 2.3.0. This is what
/// settles whether that matters: the pair is asked to compile F#, and told to.
/// https://github.com/asc-community/AngouriMath/issues/1100
module AngouriMath.Terminal.CompilerServiceWorksTest

open Xunit
open AngouriMath.Terminal.Lib.FSharpInteractive
open AngouriMath.Terminal.Lib.PreRunCode
open AngouriMath.Terminal.Lib.Consts

/// A kernel with nothing loaded into it, so what is under test is the compiler rather than
/// AngouriMath's own bindings. The Terminal opens AggressiveOperators over the top, which
/// deliberately rebinds the comparison operators to build expressions — that is the tool's whole
/// point and it is not what this asks about.
let private newKernel () =
    match createKernel () with
    | Result.Error _ ->
        Assert.True(false, "the kernel did not load at all")
        raise (System.Exception())
    | Result.Ok kernel -> kernel

/// One kernel for the whole class, not one per case.
///
/// Creating a kernel starts a compiler service, which is the most expensive thing here by a wide
/// margin, and xUnit runs the cases in a class one after another so there is nothing to share it
/// with. Every case below only asks the kernel to compile something and reads the answer, so
/// nothing one of them does changes what the next one sees — the cases that *do* change a session
/// are in OperatorsAtThePromptTest and make their own.
let private shared = lazy (newKernel ())

let private evaluates (code: string) (expected: string) =
    match execute shared.Value code with
    | ExecutionResult.PlainTextSuccess actual -> Assert.Equal(expected, actual)
    | other ->
        Assert.True(false, $"expected {expected}, and the kernel answered {other}")

/// Each of these is a language feature the compiler service has to implement rather than a
/// value the parser can fold, so a mismatched FSharp.Core would show here rather than in
/// arithmetic. Sixteen of sixteen pass on the shipped pair.
[<Theory>]
// Inference and generics.
[<InlineData("let twice f x = f (f x)\ntwice (fun n -> n * 2) 5", "20")>]
[<InlineData("let inline add a b = a + b\nadd 2 3", "5")>]
// Type definitions of every shape.
[<InlineData("type P = { X: int; Y: int }\nlet p = { X = 1; Y = 2 }\np.X + p.Y", "3")>]
[<InlineData("type T = A of int | B\nmatch A 7 with A n -> n | B -> 0", "7")>]
[<InlineData("type C(x: int) =\n    member _.Doubled = x * 2\nC(21).Doubled", "42")>]
[<InlineData("[<Measure>] type m\nlet d = 5.0<m>\nfloat d", "5")>]
// Computation expressions and the library that backs them.
[<InlineData("async { return 1 + 1 } |> Async.RunSynchronously", "2")>]
[<InlineData("seq { 1..100 } |> Seq.filter (fun x -> x % 7 = 0) |> Seq.length", "14")>]
[<InlineData("[ for i in 1..10 -> i * i ] |> List.sum", "385")>]
// Pattern matching, including one the compiler generates code for.
[<InlineData("let (|Even|Odd|) n = if n % 2 = 0 then Even else Odd\nmatch 4 with Even -> \"e\" | Odd -> \"o\"", "e")>]
[<InlineData("[(1,2);(3,4)] |> List.map (fun (a,b) -> a + b)", "[3; 7]")>]
[<InlineData("let f x = if x > 0 then Some x else None\nOption.defaultValue 0 (f 5)", "5")>]
// Recursion, formatting, interpolation, and an object expression over an interface.
[<InlineData("let rec fib n = if n < 2 then n else fib (n-1) + fib (n-2)\nfib 20", "6765")>]
[<InlineData("sprintf \"%d %s %.2f\" 42 \"x\" 3.14159", "42 x 3.14")>]
[<InlineData("let n = 6\n$\"n is {n}\"", "n is 6")>]
[<InlineData("let c = { new System.IComparable<int> with member _.CompareTo o = 0 }\nc.CompareTo 1", "0")>]
let ``The compiler service compiles F#`` (code: string) (expected: string) =
    evaluates code expected

/// And it still refuses what it should, with a position — a compiler that accepted everything
/// would pass the theory above and be useless.
[<Theory>]
[<InlineData("let x: int = \"not an int\"", "typecheck error")>]
[<InlineData("let =", "parse error")>]
let ``The compiler service reports errors`` (code: string) (expected: string) =
    match execute shared.Value code with
    | ExecutionResult.Error message ->
        Assert.Contains(expected, message)
        // A position, not just a complaint: the Terminal shows these to whoever typed them.
        Assert.Contains("input.fsx (1,", message)
    | other ->
        Assert.True(false, $"expected an error and the kernel answered {other}")

/// The pairing the warning is about, doing the work the Terminal exists for.
[<Theory>]
[<InlineData("solutions \"x\" \"x2 - 5x + 6 = 0\"", "{ 2, 3 }")>]
[<InlineData("derivative \"x\" \"x3 + sin(x)\"", "3 * x ^ 2 + cos(x)")>]
[<InlineData("integral \"x\" \"x2\"", "x ^ 3 / 3 + C")>]
let ``AngouriMath works through the compiler service`` (code: string) (expected: string) =
    let kernel = newKernel ()
    match enableAngouriMath kernel with
    | ExecutionResult.Error reason -> Assert.True(false, $"AngouriMath did not load: {reason}")
    | _ -> ()
    match execute kernel code with
    | ExecutionResult.LatexSuccess (_, actual) -> Assert.Equal(expected, actual)
    | ExecutionResult.PlainTextSuccess actual -> Assert.Equal(expected, actual)
    | other -> Assert.True(false, $"expected {expected}, and the kernel answered {other}")
