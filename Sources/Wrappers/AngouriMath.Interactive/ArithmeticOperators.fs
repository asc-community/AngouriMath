/// The arithmetic operators, rebound to build expressions, and nothing else.
///
/// <c>AggressiveOperators</c> rebinds the comparisons as well, so that <c>x &gt; 0</c> is an
/// inequality rather than a boolean. That is a real feature and it stays; what it costs is that
/// ordinary F# containing a comparison stops typechecking, because <c>if</c>, <c>while</c>,
/// <c>List.filter</c> and a <c>match</c> guard all want a <c>bool</c> where they now get an
/// <c>Entity</c>. <c>let rec fib n = if n &lt; 2 then n else fib (n-1) + fib (n-2)</c> is the first
/// recursive function most people type at an F# prompt, and it does not compile.
///
/// This is the half a prompt wants by default: arithmetic on anything builds an expression, and
/// the language keeps working. Somebody who wants the comparisons too opens
/// <c>AngouriMath.Interactive.AggressiveOperators</c> over the top of it, which is one line and
/// restores exactly the old behaviour.
///
/// https://github.com/asc-community/AngouriMath/issues/1133
module AngouriMath.Interactive.ArithmeticOperators

open AngouriMath.FSharp.Core

let ( + ) a b =
    ((parsed a) + (parsed b)).InnerSimplified
let ( - ) a b =
    ((parsed a) - (parsed b)).InnerSimplified
let ( * ) a b =
    ((parsed a) * (parsed b)).InnerSimplified
let ( / ) a b =
    ((parsed a) / (parsed b)).InnerSimplified
let ( ** ) a b =
    ((parsed a).Pow(parsed b)).InnerSimplified
