/// Which operators the Terminal rebinds, and what that costs.
/// https://github.com/asc-community/AngouriMath/issues/1133
module AngouriMath.Terminal.OperatorsAtThePromptTest

open Xunit
open AngouriMath.Terminal.Lib.FSharpInteractive
open AngouriMath.Terminal.Lib.PreRunCode
open AngouriMath.Terminal.Lib.Consts

let private kernel () =
    let k =
        match createKernel () with
        | Result.Error _ ->
            Assert.True(false, "the kernel did not load")
            raise (System.Exception())
        | Result.Ok k -> k
    match enableAngouriMath k with
    | ExecutionResult.Error reason ->
        Assert.True(false, $"AngouriMath did not load: {reason}")
        raise (System.Exception())
    | _ -> k

let private answers (k: Microsoft.DotNet.Interactive.FSharp.FSharpKernel) (code: string) (expected: string) =
    match execute k code with
    | ExecutionResult.PlainTextSuccess actual -> Assert.Equal(expected, actual)
    | ExecutionResult.LatexSuccess (_, actual) -> Assert.Equal(expected, actual)
    | other -> Assert.True(false, $"expected {expected}, and the prompt answered {other}")

/// The Terminal rebinds the five arithmetic operators the wiki says it rebinds, and no longer
/// rebinds the comparisons, which it never said it did.
///
/// The comparisons cost ordinary F#: `if`, `while`, `List.filter` and a `match` guard all want a
/// `bool` and were getting an `Entity`, so a large part of the language did not typecheck at a
/// prompt whose whole purpose is to be typed at.
[<Theory>]
[<InlineData("if 1 < 2 then 1 else 0", "1")>]
[<InlineData("[1;2;3] |> List.filter (fun x -> x > 1)", "[2; 3]")>]
[<InlineData("let b: bool = 1 < 2\nb", "True")>]
[<InlineData("seq { 1..100 } |> Seq.filter (fun x -> x % 7 = 0) |> Seq.length", "14")>]
[<InlineData("let (|Even|Odd|) n = if n % 2 = 0 then Even else Odd\nmatch 4 with Even -> \"e\" | Odd -> \"o\"", "e")>]
let ``Ordinary F# with a comparison in it compiles`` (code: string) (expected: string) =
    answers (kernel ()) code expected

/// And the reading the Terminal exists for is untouched: arithmetic on anything still builds an
/// expression, which is what the wiki documents and what makes this a symbolic prompt rather than
/// an F# one.
[<Theory>]
[<InlineData("x + 1", "x + 1")>]
[<InlineData("3 / 2", "3/2")>]
[<InlineData("x ** 2", "x ^ 2")>]
[<InlineData("solutions \"x\" \"x2 - 5x + 6 = 0\"", "{ 2, 3 }")>]
[<InlineData("derivative \"x\" \"x3\"", "3 * x ^ 2")>]
let ``The symbolic reading of arithmetic survives`` (code: string) (expected: string) =
    answers (kernel ()) code expected

/// What narrowing costs, stated rather than glossed: a bare `x > 0` no longer builds an
/// inequality. It is one line to get back, and that line is the whole of the old behaviour.
[<Fact>]
let ``A comparison on symbols is one open away`` () =
    let k = kernel ()

    match execute k "x > 0" with
    | ExecutionResult.Error message -> Assert.Contains("comparison", message)
    | other -> Assert.True(false, $"expected `x > 0` to be an ordinary comparison now, got {other}")

    match execute k "open AngouriMath.Interactive.AggressiveOperators" with
    | ExecutionResult.VoidSuccess -> ()
    | other -> Assert.True(false, $"opening the aggressive operators answered {other}")

    answers k "x > 0" "x > 0"

/// <summary>
/// The part narrowing does <b>not</b> fix, kept here so nobody reads the tests above as saying
/// the prompt is now an F# console.
/// </summary>
/// <remarks>
/// Rebinding arithmetic is what the Terminal is for, and it has the same shape of cost: once
/// <c>+</c> and <c>*</c> build expressions, a function whose branches mix an <c>Entity</c> with an
/// <c>int</c> does not typecheck, and <c>List.sum</c> over expressions has no <c>+</c> it can use.
/// So <c>let rec fib n = if n &lt; 2 then n else fib (n-1) + fib (n-2)</c> still does not compile —
/// it fails on the arithmetic now rather than on the comparison. Narrowing bought back the part
/// that was costing nothing; this part is the trade the tool actually makes.
/// </remarks>
[<Theory>]
[<InlineData("let rec fib n = if n < 2 then n else fib (n-1) + fib (n-2)\nfib 20")>]
[<InlineData("[ for i in 1..10 -> i * i ] |> List.sum")>]
let ``Arithmetic on expressions still costs ordinary F#`` (code: string) =
    match execute (kernel ()) code with
    | ExecutionResult.Error _ -> ()
    | other ->
        Assert.True(false,
            $"this compiles now, so the remark above is out of date and should be rewritten: {other}")
