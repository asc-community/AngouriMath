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
/// <c>fib</c> compiles, which is what #1133 asked for — but not by default, and the difference is
/// the point rather than a caveat.
/// </summary>
/// <remarks>
/// <para>
/// Rebinding arithmetic and writing ordinary F# are <b>mutually exclusive</b>, and that was
/// measured rather than assumed. With F#'s own operators, <c>fib 20</c> is 6765 and <c>3 / 2</c> is
/// <c>1</c>; with arithmetic rebound, <c>3 / 2</c> is <c>3/2</c> and <c>x + 1</c> is an expression
/// while <c>fib</c> does not typecheck. Both readings of <c>/</c> cannot be the return type of one
/// operator.
/// </para>
/// <para>
/// An overload that keeps <c>int + int</c> an <c>int</c> was tried, as a witness-constrained SRTP
/// operator, and it does resolve the simple cases — <c>1 + 1</c> is 2, <c>x + 1</c> is an
/// expression, <c>1 + 4.5</c> is <c>11/2</c>, all at once. It does not resolve <c>let rec</c>: the
/// operator has to be picked while the function's own type is still being inferred, so it falls to
/// the general case and <c>fib</c> fails anyway. A type annotation does not help, which was checked
/// rather than assumed.
/// </para>
/// <para>
/// So the answer is that a session says which reading it wants. <c>operators ()</c> at the prompt
/// prints the two lines, and either can follow the other any number of times.
/// </para>
/// </remarks>
[<Fact>]
let ``Ordinary F# is one open away, and reversible`` () =
    let k = kernel ()

    // As the prompt starts: expressions, and fib does not compile.
    answers k "x + 1" "x + 1"
    answers k "3 / 2" "3/2"
    match execute k "let rec fib n = if n < 2 then n else fib (n-1) + fib (n-2)\nfib 20" with
    | ExecutionResult.Error _ -> ()
    | other -> Assert.True(false, $"fib compiles by default now, so this test is out of date: {other}")

    // One line, and it does.
    match execute k "open Microsoft.FSharp.Core.Operators" with
    | ExecutionResult.VoidSuccess -> ()
    | other -> Assert.True(false, $"restoring F#'s operators answered {other}")
    answers k "let rec fib n = if n < 2 then n else fib (n-1) + fib (n-2)\nfib 20" "6765"
    answers k "[ for i in 1..10 -> i * i ] |> List.sum" "385"
    answers k "3 / 2" "1"

    // And back, in the same session.
    match execute k "open AngouriMath.Interactive.ArithmeticOperators" with
    | ExecutionResult.VoidSuccess -> ()
    | other -> Assert.True(false, $"going back answered {other}")
    answers k "x + 1" "x + 1"
    answers k "3 / 2" "3/2"

/// And the prompt says so, rather than leaving it to be discovered.
[<Fact>]
let ``The prompt explains which operators it has`` () =
    // It prints rather than returns, so what is asserted is that it runs and says nothing back:
    // a returned sentence would go to AngouriMath's formatter, which would try to parse it.
    match execute (kernel ()) "operators ()" with
    | ExecutionResult.VoidSuccess -> ()
    | other -> Assert.True(false, $"operators () answered {other}")
