# Expression syntax

What `MathS.FromString` accepts, and what `Entity.Stringize` produces. The authority is
[`Core/Antlr/AngouriMath.g`](../../Core/Antlr/AngouriMath.g); this page is a reading of it. To
change the language, change the grammar and regenerate — see
[`ImproveParser.md`](../Contributing/ImproveParser.md).

## The contract

**Parsing what `Stringize` prints gives back the expression it printed.** That is the whole
point of the printed form, and it is enforced by `StringizeRoundTripTest`. A node whose usual
notation is not in the grammar prints as its function call instead: a lambda prints as
`lambda(x, x + 1)` and not `x -> x + 1`, because `->` is the implication operator and the arrow
form would silently come back as something else.

**LaTeX output has a round trip too, and it is checked in someone else's repository.**
[CSharpMath.Evaluation](https://github.com/verybadcat/CSharpMath/blob/master/CSharpMath.Evaluation/Evaluation.cs)
reads LaTeX back into an `Entity` and says so in its own source — *"CSharpMath must handle all
LaTeX coming from AngouriMath or a bug is present!"* — so `Latexize` is free to use `\frac`,
`\bmod` and the rest, but changing what it emits can break a downstream project and no test here
will say so ([#822](https://github.com/asc-community/AngouriMath/issues/822)).

## Operators, loosest first

Everything on one line has the same precedence and groups to the left, except `^`, which groups
to the right.

| | operators | notes |
|---|---|---|
| 1 | `provided` | |
| 2 | `implies` | |
| 3 | `or` `\|` | |
| 4 | `xor` | |
| 5 | `and` `&` | |
| 6 | `not` | prefix |
| 7 | `=` `<>` `>` `>=` `<` `<=` | chained: `a < b < c` means `a < b and b < c` |
| 8 | `in` | |
| 9 | `unite` `\/`, `setsubtract` `\` | |
| 10 | `intersect` `/\` | |
| 11 | `+` `-` | |
| 12 | `*` `/` `mod` | |
| 13 | `+` `-` | prefix |
| 14 | `^` | **groups to the right**: `a ^ b ^ c` is `a ^ (b ^ c)` |
| 15 | `!` | postfix factorial |

`%` is **not** an operator. It is left free to mean percent; the remainder is written `mod`.

`mod` is the floored remainder, `a - b * floor(a / b)`, which takes the sign of the **divisor**:
`(-7) mod 3` is 2 and `7 mod (-3)` is -2. This is the convention SymPy, Mathematica and Maxima
use, and the one under which the residues modulo n are the numbers from 0 to n - 1. C's `%`
truncates instead; it is a different operation and the library does not inherit it.

## Numbers and constants

`1`, `1.5`, `1/2`, `1e-9`, `i`, `e`, `pi`, `+oo`, `-oo`, `NaN`, `true`, `false`.

`NaN` is the value an undefined computation gives — `0/0` and `1/0` both reach it — and it is
spelled exactly that way, since only the exact word is reserved. A longer name containing it, such
as `NaNx` or `NaN_1`, is still a variable, because the lexer takes the longest match.

A variable is a letter or `_` followed by letters, digits or `_`, and Greek letters are letters.
Juxtaposition is multiplication, so `2x` is `2 * x` — which is also why an unknown function name
parses as a product rather than as an error: `foo(x)` is `foo * x`, silently, unless `foo` is one
of the names below.

## Sets

| | |
|---|---|
| finite | `{ 1, 2, 3 }`, `{}` |
| interval | `[a; b]` closed, `(a; b)` open, `[a; b)` and `(a; b]` half-open |
| conditional | `{ x : x > 0 }` |
| special | `RR` `CC` `ZZ` `QQ` `BB` |
| operations | `unite` `/\` … see the table above |

## Matrices and vectors

`[1, 2, 3]` is a column vector, `[[1, 2], [3, 4]]` a matrix, `[1, 2, 3]T` a transpose.

## Functions

Everything below is written `name(argument, ...)`.

**Trigonometric** — `sin` `cos` `tan` `cotan` `cot` `sec` `cosec` `csc`, and the inverses
`arcsin` `arccos` `arctan` `arccotan` `arccot` `arcsec` `arccosec` `arccsc`, each also spelled
`asin` `acos` `atan` `acotan` `acot` `asec` `acosec` `acsc`.

**Hyperbolic** — `sinh` `sh` `cosh` `ch` `tanh` `th` `cotanh` `coth` `cth` `sech` `sch`
`cosech` `csch`.

**Inverse hyperbolic** — the inverse of a hyperbolic function is an *area*, not an arc, so it is
`arsinh` (also `asinh`, `arsh`; `arcsinh` is **refused**), `arcosh`, `artanh`, `arcotanh`, and
their short forms `arsh` `arch` `arth` `arcth` `arsch` `arcsch`. The `arc-` spellings raise a
parse error rather than being silently read as a product.

Both hyperbolic families are **rewritten as they are parsed** and are not nodes of their own:
`sinh(x)` becomes `(e ^ x - e ^ (-x)) / 2` and `arsinh(x)` becomes `ln(x + sqrt(x ^ 2 + 1))`.
So they do not print back as themselves — what round-trips is the expression, not the spelling.
The same goes for `cbrt(x)`, which is `x ^ (1/3)`, and `sqr(x)`, which is `x ^ 2`.

**Other** — `sqrt` `cbrt` `sqr` `pow(a, b)` `ln` `log(base, x)` `abs` `signum` `sgn` `sign`
`phi` `gamma` `factorial` (or postfix `!`).

**Rounding** — `floor` `ceil` (`ceiling` is accepted on the way in and prints as `ceil`) `round`.
All three round a complex argument componentwise. `floor` and `ceil` go toward the infinities
rather than toward zero, so `floor(-3/2)` is `-2`; `round` goes to the **nearest even** on a tie,
so `round(1/2)` is `0` and `round(5/2)` is `2`, which is what Python, SymPy, Mathematica and
IEEE 754 all mean by rounding — and is *not* `floor(x + 1/2)`.

**Comparison and divisors** — `min(a, b, ...)` `max(a, b, ...)` `gcd(a, b, ...)`. All three take
any number of arguments and fold. `min` and `max` compare only where the arguments are ordered and
are otherwise left as written. `gcd` computes over integers and rationals — `gcd(1/2, 1/3)` is
`1/6` — and leaves the polynomial case alone.

**Calculus** — `derivative(expr, var, order)`, `integral(expr, var)`,
`integral(expr, var, from, to)`, `limit(expr, var, dest)`, `limitleft(...)`, `limitright(...)`.

`derivative` takes an order and `integral` does not: `derivative(f, x, 2)` is the second
derivative, while `integral`'s third and fourth arguments are the bounds of a definite integral,
not a count. `integral(f, x, 2)` is therefore a parse error rather than the second antiderivative
— three arguments name neither form. This is deliberate; the iteration count was replaced by the
range in [#657](https://github.com/asc-community/AngouriMath/pull/657).

**Structural** — `piecewise(a provided p, b provided q)`, `lambda(param, body)`,
`apply(f, arg, ...)`, `domain(expr, set)`.

**Refused by name** — `trunc` `lcm` `erf` `conjugate`. AngouriMath has none of these, and each is
what some other CAS calls a function, so a caller reaches for it. Left alone they would be read as
products under the rule below and answer silently and wrongly; they raise a parse error naming the
function instead. `re` and `im` are the same case and are *not* refused, being short enough to be
somebody's variable.

## Where it is easy to be caught out

- **`^` groups to the right.** `2 ^ 2 ^ 3` is 256, not 64. Write `(2 ^ 2) ^ 3` for the other.
- **An unknown name is a product**, not an error. `sinx` is `s * i * n * x`, and `f(x)` for an
  unknown `f` is `f * x`. That is what makes `a(b + c)` work, and it is why the names above are
  refused individually rather than by a general rule.
- **`%` is not the remainder.** Write `mod`.
- **Intervals use `;`**, not `,`: `[1; 2]`. `[1, 2]` is a vector.
- **`->` is implication**, not a lambda arrow.
