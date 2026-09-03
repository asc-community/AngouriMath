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

A lambda is now also *readable* as `x => x + 1`, and it still **prints** as `lambda(x, x + 1)`.
More than one spelling may be read; exactly one is printed, and it is the one this contract is
about.

A **codomain** is part of that. A node whose `Codomain` is not the one its type carries by default
prints inside `domain(...)`, so `domain(x, ZZ)` prints as `domain(x, ZZ)` and reads back narrowed
([#1022](https://github.com/asc-community/AngouriMath/issues/1022)); it is not decoration, since
`sqrt(-1)` is `i` and `domain(sqrt(-1), RR)` is `NaN`. The wrapper is printed only where it says
something, so the ordinary expression is unchanged — and the default is not the same for every node:
a variable and a matrix default to `Any`, `abs` and an interval to `RR`, every boolean node to `BB`,
each numeric literal to its own type's domain, and everything else to `CC`.

**`Any` is written too, and it is not a set.** `domain(abs(x), Any)` is how a node widened from a
narrower default prints, and it reads back widened
([#1048](https://github.com/asc-community/AngouriMath/issues/1048)). `Any` is a keyword in that one
position and nowhere else — `Any + 1` is arithmetic on a variable called `Any`, and there is no
`Entity` it denotes: `SpecialSet.Create(Domain.Any)` throws, deliberately. It says that a node
imposes no restriction, which is a fact about the node and not a collection of values
([#996](https://github.com/asc-community/AngouriMath/issues/996)).

One annotation still does not survive, and it is the parser's rather than the printer's: no input
at all yields a rational literal whose codomain is `CC`, because the pass that reads `1/2` as a
rational rather than a quotient
([#873](https://github.com/asc-community/AngouriMath/issues/873)) treats `CC` as "nobody annotated
this", which is what an un-annotated `/` carries.

**LaTeX output has a round trip too, and it is checked in someone else's repository.**
[CSharpMath.Evaluation](https://github.com/verybadcat/CSharpMath/blob/master/CSharpMath.Evaluation/Evaluation.cs)
reads LaTeX back into an `Entity` and says so in its own source — *"CSharpMath must handle all
LaTeX coming from AngouriMath or a bug is present!"* — so `Latexize` is free to use `\frac`,
`\bmod` and the rest, but changing what it emits can break a downstream project and no test here
will say so ([#822](https://github.com/asc-community/AngouriMath/issues/822)).

## Operators, loosest first

Everything on one line has the same precedence and groups to the left, except `^` and `provided`,
which group to the right.

Grouping is not decoration: an operator that is not associative prints the brackets it has, so
`a implies (b implies c)` prints with them and `(a implies b) implies c` prints without. Where the
operator *is* associative the brackets carry no mathematics and are not printed, so `x + (y + z)`
prints as `x + y + z` and reads back as `(x + y) + z` — the same number, a differently shaped tree.

| | operators | notes |
|---|---|---|
| 1 | `provided` | **groups to the right**: `a provided b provided c` is `a provided (b provided c)` |
| 2 | `implies` `->` | |
| 3 | `or` `\|` | |
| 4 | `xor` | |
| 5 | `and` `&` | |
| 6 | `not` | prefix |
| 7 | `=` `<>` `>` `>=` `<` `<=` | chained: `a < b < c` means `a < b and b < c` |
| 8 | `in` | |
| 9 | `unite` `\/`, `setsubtract` `\` | one level, so `A \/ B \ C` is `(A \/ B) \ C` |
| 10 | `intersect` `/\` | |
| 11 | `+` `-` | |
| 12 | `*` `/` `mod` | one level, so `x * y mod z` is `(x * y) mod z` |
| 13 | `+` `-` | prefix |
| 14 | `^` | **groups to the right**: `a ^ b ^ c` is `a ^ (b ^ c)` |
| 15 | `!` | postfix factorial |

`%` is **not** an operator. It is left free to mean percent; the remainder is written `mod`.

`mod` is the floored remainder, `a - b * floor(a / b)`, which takes the sign of the **divisor**:
`(-7) mod 3` is 2 and `7 mod (-3)` is -2. This is the convention SymPy, Mathematica and Maxima
use, and the one under which the residues modulo n are the numbers from 0 to n - 1. C's `%`
truncates instead; it is a different operation and the library does not inherit it.

## Whitespace and comments

Newlines are skipped, so an expression may be written over several lines. `//` runs to the end of
the line and `/* … */` spans lines; both are dropped before parsing. A `//` comment is ended by a
newline or by the end of the input, so `x + 1 // done` and `x + 1 /* done */` are both `x + 1`. Until
[#1039](https://github.com/asc-community/AngouriMath/issues/1039) the newline was required and the
first of those was a parse error.

## Numbers and constants

`1`, `1.5`, `.5`, `1.`, `1/2`, `1e-9`, `i`, `e`, `pi`, `+oo`, `-oo`, `NaN`, `true`, `false`.

A trailing `i` makes the literal imaginary, exponent and all: `3i`, `1.5e3i`. `i` on its own is the
imaginary unit, and it is a **number** rather than a name — which is why `x i` is `x ^ i` and not a
product (below), why `ix` is an ordinary variable called `ix`, and why an operator that declares
`i` as an index has to say so.

Booleans are written `true` or `false`; `True` and `False` are read as well, because that is what
`Stringize` prints.

`NaN` is the value an undefined computation gives — `0/0` and `1/0` both reach it — and it is
spelled exactly that way, since only the exact word is reserved. A longer name containing it, such
as `NaNx` or `NaN_1`, is still a variable, because the lexer takes the longest match.

## Names

A name is **one or more letters**, optionally followed by `_` and one or more letters or digits:
`x`, `xy`, `x_1`, `x_a`, `α`, `ω_1`, `Θ_1`, `альфа`, `x_ω`. Letters are `a`–`z`, `A`–`Z`, Greek
(U+0370–U+03FF and U+1F00–U+1FFF) and Cyrillic (U+0400–U+04FF); no other script is one, so `ﬁ` is
a lexer error.

Four things that look like names and are not:

| written | what it is |
|---|---|
| `_x`, `x_` | a lexer error. `_` may neither begin a name nor end one |
| `x_1_2` | a lexer error. At most one `_`, so subscripts do not nest ([#524](https://github.com/asc-community/AngouriMath/issues/524)) |
| `x1` | `x ^ 1`. A digit is not a letter, and a name beside a number is a power — see below |
| `sinx` | one name, spelled `sinx`. Letters written together are a single name, never a product of one-letter ones |

## Juxtaposition

Two things written next to each other have an operator inserted between them, and **which
operator depends on what comes second**:

| | inserted | |
|---|---|---|
| number, name or `)`, then a **name**, a function or `(` | `*` | `2x` is `2 * x`, `a(b + c)` is `a * (b + c)`, `x sin(x)` is `x * sin(x)`, `x(2)` is `x * 2` |
| number, name or `)`, then a **number** | `^` | `x2` is `x ^ 2`, `(x + 1)2` is `(x + 1) ^ 2`, `3 2` is `3 ^ 2` |

So `x2` is a square and `x(2)` is a product, and since `i` is a number, `x i` is `x ^ i` while
`2i x` is `2i * x`.

`MathS.Settings.ExplicitParsingOnly` turns the insertion off. Under it every row above is a
`MissingOperatorParseException` naming the two tokens it will not join, and only what is written
with an operator parses.

## Sets

| | |
|---|---|
| finite | `{ 1, 2, 3 }`, `{}` |
| interval | `[a; b]` closed, `(a; b)` open, `[a; b)` and `(a; b]` half-open |
| conditional | `{ x : x > 0 }` — the name before the `:` is declared, as under `sum` below |
| special | `RR` `CC` `ZZ` `QQ` `BB` |
| operations | `unite` `/\` … see the table above |

There is **no universal set** and no literal for one. A set that constrains nothing is the
conditional set `{ x : True }`, which prints, reads back, compares and answers membership like any
other; `Domain.Any` is a codomain and not a candidate for the job
([#996](https://github.com/asc-community/AngouriMath/issues/996)).

## Matrices and vectors

`[1, 2, 3]` is a column vector (3 by 1), `[[1, 2], [3, 4]]` a matrix, `[1, 2, 3]T` a transpose
(1 by 3). There is no empty vector: `[]` is not a value this library has
([#1028](https://github.com/asc-community/AngouriMath/issues/1028)), while the empty set `{}` is.

`(|x|)` is the absolute value of `x`, and prints back as `abs(x)`.

## Functions

Everything below is written `name(argument, ...)`.

**Trigonometric** — `sin` `cos` `tan` `cotan` `cot` `sec` `cosec` `csc`, and the inverses
`arcsin` `arccos` `arctan` `arccotan` `arccot` `arcsec` `arccosec` `arccsc`, each also spelled
`asin` `acos` `atan` `acotan` `acot` `asec` `acosec` `acsc`.

**Hyperbolic** — `sinh` `sh` `cosh` `ch` `tanh` `th` `cotanh` `coth` `cth` `sech` `sch`
`cosech` `csch`.

**Inverse hyperbolic** — the inverse of a hyperbolic function is an *area*, not an arc, so it is
`ar-` and not `arc-`. Each has a long spelling, an `a-` spelling and at least one short one:

| | | |
|---|---|---|
| sine | `arsinh` `asinh` `arsh` | `arcsinh` is **refused** |
| cosine | `arcosh` `acosh` `arch` | `arccosh` is **refused** |
| tangent | `artanh` `atanh` `arth` | `arctanh` is **refused** |
| cotangent | `arcotanh` `acotanh` `arcoth` `acoth` `arcth` | `arccotanh` is **refused** |
| secant | `arsech` `asech` `arsch` | `arcsech` is **refused** |
| cosecant | `arcosech` `acosech` `arcsch` `acsch` | `arccosech` is **refused** |

A refused spelling raises a parse error that names the accepted ones. It is refused rather than
ignored because an unknown name followed by a bracket is a product (above), so `arcsinh(x)` left
alone would quietly be a variable times `x`.

Both hyperbolic families are **rewritten as they are parsed** and are not nodes of their own:
`sinh(x)` becomes `(e ^ x - e ^ (-x)) / 2` and `arsinh(x)` becomes `ln(x + sqrt(x ^ 2 + 1))`.
So they do not print back as themselves — what round-trips is the expression, not the spelling.
The same goes for `cbrt(x)`, which is `x ^ (1/3)`, `sqr(x)`, which is `x ^ 2`, and `exp(x)`,
which is `e ^ x`.

**Powers and logarithms** — `sqrt` `cbrt` `sqr` `pow(a, b)` `exp` `ln` `log(base, x)`. `log` with
one argument is base 10, so `log(100)` is 2; `log10` and `log2` say it in the name.

**Other** — `abs` `signum` `sgn` `sign` `phi` `gamma` `factorial` (or postfix `!`).

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

A lambda also has an arrow form, `param => body`, with several parameters written next to each
other: `a b => a + b` is `a => b => a + b`, which is `lambda(a, b, a + b)`. The arrow binds
looser than everything else and its body runs to the end, so `a => a + 3` is `a => (a + 3)`.
Every parameter must be a name — `a 3 => 3` is refused — and the body is read exactly as
`lambda(...)` reads it, so an index called `i` means the name and not the imaginary unit.

`domain` takes a special set — `CC` `RR` `QQ` `ZZ` `BB` — or the keyword `Any`, and sets the
*codomain* of whatever node it wraps, which is what makes the expression evaluate to `NaN` outside
it. It applies to any node, not only a variable, and it is what `Stringize` prints for one. `Any`
is the one spelling here that is not a set: it removes the restriction rather than naming values.

**Refused by name** — `trunc` `lcm` `erf` `conjugate`. AngouriMath has none of these, and each is
what some other CAS calls a function, so a caller reaches for it. Left alone they would be read as
products under the rule above and answer silently and wrongly; they raise a parse error naming the
function instead. `re` and `im` are the same case and are *not* refused, being short enough to be
somebody's variable.

## `sum` and `product`, which declare a name

`sum(body, name, from, to)` and `product(body, name, from, to)`. The **second** argument is the
name being declared and the first is the body it runs over, so `sum(i, i, 1, 10)` is
1 + 2 + … + 10 = 55, and `sum(k, k, 1, 10)` is the same number written over a different name.

Both bounds are inclusive and the step is one. An empty range is the operator's identity —
`sum(k, k, 5, 1)` is `0` and `product(k, k, 5, 1)` is `1` — and the range is written out only when
both bounds are integers and there are not too many terms.

A `sum` whose body is a **polynomial in the index** is answered in closed form instead of being
written out, so `sum(k, k, 1, n)` is `(n + n^2)/2` and `sum(k, k, 1, 100000)` is `5000050000`
rather than a hundred thousand terms. The answer carries the condition it needs — `to >= from - 1`
— because below that the range is empty and the polynomial is not zero; where the bounds are
concrete that condition is decidable and the answer is a number. A bound that is a number and not
a whole one still stays as written, since the index runs over the integers and
`sum(k, k, 1, 5/2)` is `1 + 2` rather than the polynomial at `5/2`.

A `product` gets the same treatment over the narrower class its shape allows: a **monomial** in
the index, since a product has no linearity to take a sum of terms apart with. So
`product(k, k, 1, n)` is `factorial(n)`, `product(k ^ 2, k, 1, n)` is `factorial(n) ^ 2`, and
`product(c, k, m, n)` is `c ^ (n - m + 1)`. Its condition is `to >= from` rather than
`to >= from - 1`, because at the empty range itself the closed form would be `c ^ 0` — undefined
at `c = 0`, where the empty product is `1`. Where the index appears in the body the lower bound
must be a concrete integer of at least one, `factorial` having no value at the negative integers;
`product(k, k, 0, n)` therefore stays as written.

**The declared name means the name, and only inside the operator that declares it.** Two names
show this, because both mean something else in this language:

- `i` is the imaginary unit, so `sum(i, i, 1, 10)` is `55` and not a sum of imaginary units.
  Outside, it is the unit again: `sum(i, i, 1, 3) + i` is `6 + i`, and `sum(sqrt(-1) * i, i, 1, 10)`
  is `55i` — the factor is the unit, the index is the name. `2i` is a single number token, so
  `sum(2i, i, 1, 3)` is `12`: two times the index, three times over.
- `pi` and `e` are constants, so `product(pi, pi, 1, 4)` is `24` and `sum(e, e, 1, 3)` is `6`.

A declared name that outlives the operator declaring it is renamed, since `i` and `pi` are not
names the parser can produce: `derivative(pi ^ 2, pi)` is `2 * pi_1`.

Every binder in the language reads its name this way — `integral`, `derivative`, `limit`, `lambda`
and the set builder `{ x : … }` alike. `lambda` is the one that will not take a number: its
parameter is typed as a name, so `lambda(2, x)` is a parse error where `sum(k, 2, 1, 3)` is not.

## Where it is easy to be caught out

- **`^` groups to the right.** `2 ^ 2 ^ 3` is 256, not 64. Write `(2 ^ 2) ^ 3` for the other.
- **A name beside a number is a power.** `x2` is `x ^ 2`; `x(2)` is `x * 2`.
- **Letters run together are one name.** `sinx` is a variable called `sinx`, not `sin(x)` and not
  a product. Write the bracket.
- **An unknown name with a bracket is a product**, not an error: `f(x)` for an unknown `f` is
  `f * x`. That is what makes `a(b + c)` work, and it is why the names above are refused
  individually rather than by a general rule.
- **`%` is not the remainder.** Write `mod`.
- **Intervals use `;`**, not `,`: `[1; 2]`. `[1, 2]` is a vector.
- **`->` is implication**, not a lambda arrow. The lambda arrow is `=>`, and the two are
  different operators rather than spellings of one.
