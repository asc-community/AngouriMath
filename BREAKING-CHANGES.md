# Behavioural and breaking changes

AngouriMath puts mathematical correctness ahead of backward compatibility. An API that returns the
wrong answer is not an asset to preserve, so when the two disagree the answer changes — see
[AGENTS.md](AGENTS.md). This file is the other half of that bargain: every place where the same
input now gives a different result, what it gives instead, and why.

Most of what follows is a wrong answer becoming a right one. That is still a breaking change if you
built on the wrong one, and the entries say so plainly rather than filing it under "fixed".

Each entry gives the value on the previous release and the value now, both measured on a build
rather than taken from the diff, with the issue and the pull request it came from. **Silent** marks
a change where the call still succeeds and quietly returns something else — those are the ones to
read first.

---

## Unreleased — since 1.4.0

### At a glance

| Silent? | What | Was | Is |
|---|---|---|---|
| loud | `Minusf.Minuend` / `.Subtrahend` | named for the wrong operand | named for the right one |
| **silent** | `exp(x)`, `log10(x)`, `log2(x)` | products of undeclared variables | the functions |
| loud | `arcsinh(x)` and its five relatives | a product | `UnrecognizedFunctionParseException` |
| loud | `mod` as a variable name | a variable | a keyword, so a parse error |
| **silent** | `Stringize` of powers, lambdas, applications, piecewises | did not parse back | parses back |
| **silent** | numbers below `1e-16` | rounded to `0` | kept |
| **silent** | `Real` `%` | `-7 % 3` was `-1` | `2` |
| **silent** | `Simplify` of a cancelling quotient | the quotient | a `Providedf` |
| **silent** | radicals, everywhere | `sqrt(12)` | `2 * sqrt(3)` |
| **silent** | `Expand` | left like terms uncollected | collects them |
| **silent** | an identity equation | `{ }` or `{ 0 }` | all of `CC` |
| **silent** | numeric root sets | one root per starting point | one root per root |
| **silent** | many limits | `NaN`, or unevaluated | a value |
| **silent** | some integrals | not antiderivatives | closed forms |
| **silent** | two integrals | answered correctly | unevaluated — a deliberate loss |
| loud | `Compile` over a missing variable | `KeyNotFoundException` | `UncompilableNodeException` |

---

## Types and members

### `Minusf`'s two operands exchanged names

In `a - b`, `a` is the minuend and `b` is the subtrahend. The record declared them the other way
about, so the property called `Subtrahend` held the operand on the left of the sign:

```csharp
var m = (Minusf)"a - b".ToEntity();
m.Minuend      // was b, is a
m.Subtrahend   // was a, is b
```

The operand order in the constructor and in every compiled form is unchanged — the objects are the
same and only the names moved. Code that reads either property **by name** now gets the other
operand and will not fail to compile.

This is not cosmetic. It is what caused the sign error below: the inverter was written to what the
names say rather than to which operand is which, so `a = 1 / (b - c)` solved to the negation of the
right answer. Code written against the old names was wrong in the same way.

[#632](https://github.com/asc-community/AngouriMath/issues/632), PR
[#664](https://github.com/asc-community/AngouriMath/pull/664).

---

## Parsing

Four names that looked like functions were not in the grammar, and none of them failed. Each fell
through to implicit multiplication — the rule that lets `a(b + c)` mean `a * (b + c)` — and came back
as a product with an undeclared variable, silently, propagating into an answer that looked like one.

### `exp(x)` is the exponential

| | |
|---|---|
| was | `exp * x`, a product with an undeclared variable named `exp` |
| is | `e ^ x` |

`exp(x) - 3x = 0` answered `{ 0 }`, which is a root of `exp * x - 3x` and of nothing else. It now
answers the equation's actual roots, the real ones near `0.6191` and `1.5121` among them.

It maps to `MathS.Pow(MathS.e, arg)` rather than to a node of its own, so `exp(x) * exp(y)` comes
out `e ^ (x + y)` with nothing further added. Only the exact name followed by a bracket is the
function: `expr(x)`, `expo(x)` and a bare `exp` are the products they were.

[#730](https://github.com/asc-community/AngouriMath/issues/730), PR
[#731](https://github.com/asc-community/AngouriMath/pull/731).

### `log10(x)` and `log2(x)` are logarithms

| | |
|---|---|
| was | `log ^ 10 * 100` for `log10(100)` — `log10` lexes as the variable `log` followed by `10`, and `x2` means `x^2` by design |
| is | `2` |

Both map to the `log` the library already had, so `log10(x)` and `log(10, x)` parse to one tree.
Without a bracket nothing changed: `log2x` is still `log ^ 2 * x`, and `logx(y)` and `log3(x)` are
still the implicit products they were.

[#733](https://github.com/asc-community/AngouriMath/issues/733), PR
[#734](https://github.com/asc-community/AngouriMath/pull/734).

### `arcsinh` and its five relatives are refused

| | |
|---|---|
| was | `arcsinh * x`, silently |
| is | `UnrecognizedFunctionParseException` |

The inverse hyperbolic functions are *area* functions, not arc functions, so `arcsinh` is not
another name for `arsinh` — it is not a name for anything. The same for `arccosh`, `arctanh`,
`arccotanh`, `arcsech` and `arccosech`. The exception names the spellings that do work:

> there is no function arcsinh: the inverse hyperbolic functions are area functions, not arc
> functions, so the inverse hyperbolic sine is arsinh, asinh or arsh

This one refuses where `exp` was adopted, because `exp` names something the library has and
`arcsinh` names nothing. The twenty-one spellings the grammar already accepted still parse, `arcsin`
and its five relatives are correct and untouched, and a name that merely begins the same way —
`arcs(x)`, `arcsinhh(x)` — is still the product it was.

PR [#687](https://github.com/asc-community/AngouriMath/pull/687).

### `mod` is now a keyword

| | |
|---|---|
| was | `mod + 1` parsed, `mod` being an ordinary variable; `7 mod 3` parsed as `7 * mod ^ 3` |
| is | `mod` is the remainder operator; `mod + 1` is an `UnhandledParseException` |

There was no remainder in the library at all before this. **An expression using `mod` as a variable
name no longer parses** — rename the variable.

`%` is deliberately *not* the parser's spelling, so that it stays free to mean percent, which is
what it means in mathematical writing. `"7 % 3"` is still a parse error. The `%` operator on
`Entity` in C# and F# is a different thing and does exist; see below.

[#402](https://github.com/asc-community/AngouriMath/issues/402),
[#618](https://github.com/asc-community/AngouriMath/issues/618), PR
[#703](https://github.com/asc-community/AngouriMath/pull/703).

### `pow(a, b)` parses

`pow(2, 3)` raised `UnhandledParseException`: `pow` was lexed as the implicit product `p*o*w`, after
which `(2, 3)` was not a valid operand. It is now `8`. Additive.

[#625](https://github.com/asc-community/AngouriMath/issues/625), PR
[#663](https://github.com/asc-community/AngouriMath/pull/663).

---

## Printing

`Stringize`'s contract is that parsing what it prints gives back what was printed. Four kinds of
expression broke it, and broke it silently, since a wrong reading is usually still a valid
expression. **If you compare `Stringize` output against fixed strings, these four changed.**

| expression | was printed | reads back as | is printed |
|---|---|---|---|
| `(2 ^ 3) ^ 2` | `2 ^ 3 ^ 2` | `512`, where the expression is `64` | `(2 ^ 3) ^ 2` |
| `lambda(x, x + 1)` | `x -> x + 1` | `x implies x + 1` | `lambda(x, x + 1)` |
| `apply(f, 2)` | `f 2` | a power | `apply(f, 2)` |
| `piecewise(1 provided x > 0)` | `(1 if x > 0)` | a product, `if` being an undeclared variable | `piecewise(1 provided (x > 0))` |

Powers group to the right, so it is the base that needs bracketing when it is a power of its own —
the mirror of the rule the left-associative operators use. The other three have no operator spelling
in the grammar, so they now print as the function call the parser does have.

`Latexise` is under no such obligation, since nothing parses LaTeX, and is unaffected.

The syntax the parser accepts is now written down in
[`Sources/AngouriMath/Docs/Usage/Syntax.md`](Sources/AngouriMath/Docs/Usage/Syntax.md).

PR [#706](https://github.com/asc-community/AngouriMath/pull/706).

---

## Numbers and precision

### Small numbers are no longer rounded to zero

Downcasting rounded a computed value onto a nearby integer whenever the two were within
`PrecisionErrorZeroRange`, whose default is `1e-16`. Numbers are evaluated to
`DecimalPrecisionContext` digits — 100 by default — so `1e-16` sat nowhere near the noise floor, and
being an *absolute* threshold it swallowed every value whose integer part was zero.

| | was | is |
|---|---|---|
| `"1e-20".EvalNumerical()` | `0` | `1E-20` |
| `"e^(-40)".EvalNumerical()` | `0` | `4.248...E-18` |

The tolerance is now derived from the working precision — half its digits — whenever nobody has set
`PrecisionErrorZeroRange` explicitly. Residuals genuinely left over by exact cancellation still round
away with room to spare: `sin(pi)` is `0`, and such residuals measure around `1e-99`.

**If you depended on tiny results collapsing to zero**, set the setting explicitly and it is honoured
verbatim, as it was before:

```csharp
MathS.Settings.PrecisionErrorZeroRange.As(1e-16m, () => "e^(-40)".EvalNumerical());  // 0
```

That restores the old rounding for *computed* values. A parsed literal such as `1e-20` is exact and
never went through the rounding, so it stays `1e-20` either way.

[#602](https://github.com/asc-community/AngouriMath/issues/602), PR
[#663](https://github.com/asc-community/AngouriMath/pull/663).

### Logarithms of extreme powers

`ln(2^1000) / ln(2^(-1000))` answered `0`; it is `-1`. A `Rational` holds both its exact ratio and a
decimal form, the decimal is rounded into `DecimalPrecisionContext` when the number is built, and
that context runs from `10^(-100)` to `10^1000` — so `2^(-1000)` flushed to zero and `2^10000`
saturated to `+oo`. Both logarithms were computed off the decimal and reported `-oo` and `+oo` for
answers that are perfectly ordinary numbers, after which the quotient of two infinities collapsed.

Where the decimal has lost the ratio this way, `Ln` and `Log` now work from the ratio itself.
Numbers whose decimal form is intact take exactly the path they took before.

[#210](https://github.com/asc-community/AngouriMath/issues/210), PR
[#663](https://github.com/asc-community/AngouriMath/pull/663).

### `MathS.ToBaseN`

| | was | is |
|---|---|---|
| `ToBaseN(13.125m, 5)` | never returned | `23.0303030303…`, bounded by the configured precision |
| `ToBaseN(0.5m, 2)` | `".1"` | `"0.1"` |
| `ToBaseN(0m, 2)` | `""` | `"0"` |

The digit loop ran until the fractional part hit zero, which for a repeating expansion it never
does. It is now bounded by the number of base-N digits that carry the configured decimal precision,
so terminating expansions stay exact and repeating ones stop. A zero integer part rendered as the
empty string; it renders as `0`.

[#584](https://github.com/asc-community/AngouriMath/issues/584), PR
[#663](https://github.com/asc-community/AngouriMath/pull/663).

### Vector norms evaluate numerically

`MathS.FromString("(|[12,15] - [0,0]|)").EvalNumerical()` threw `CannotEvalException` on 1.4.0 where
1.3.0 returned a value — the difference being whether the norm came out a perfect square. The
vector branch of `Absf` always finished with `InnerSimplified`, ignoring the `isExact` flag its
sibling branches respect, so `sqrt(369)` survived as a `Powf` and `EvalNumerical` rejected it. The
exact path still returns a symbolic answer and the numeric path now evaluates it.

[#662](https://github.com/asc-community/AngouriMath/issues/662), PR
[#664](https://github.com/asc-community/AngouriMath/pull/664).

---

## Remainder

The three numeric types answered three different ways, and two of the answers were wrong. Which one
applied depended on the static type at the call site rather than on the values, since `Integer` and
`Rational` are both `Real`.

| | was | is |
|---|---|---|
| `(Integer)(-7) % (Integer)3` | `2` | `2` |
| `(Integer)7 % (Integer)(-3)` | `ArithmeticException: Divisor is negative` | `-2` |
| `(Real)(-7) % (Real)3` | `-1` | `2` |
| `(Rational)(-7/2) % (Integer)(-3)` | `-7/2` — larger in magnitude than the divisor, a remainder under no convention at all | `-1/2` |

All three are now **floored**: the remainder takes the sign of the divisor, and every one of them
computes `a - b*floor(a/b)`. This is the convention under which the residues modulo `n` are the
numbers from `0` to `n - 1`, which is the point of the operation in number theory, and it is what
SymPy 1.14, Mathematica and Maxima all answer — checked on all four sign pairs rather than reasoned
about. C's `%` truncates, but C's `%` is an operation on machine integers.

**`Real` is the one to watch**: `-7 % 3` was `-1` and is `2`, on ordinary input, with no exception
to notice. For `Integer` and `Rational` only the cases that were broken moved.

[#708](https://github.com/asc-community/AngouriMath/issues/708), PR
[#709](https://github.com/asc-community/AngouriMath/pull/709).

---

## Simplification

`Simplify`, `InnerSimplified`, `Expand` and `Factorize` all reach further than they did. Each entry
below is an expression whose printed form changed; **assertions on printed form will need
rewriting**, which is why the house rule is to assert the mathematics instead.

### Radicals are reduced where the power is built

Not in `Simplify` — at construction, so this reaches every path that builds a radical.

```
sqrt(12)           was  sqrt(12)           is  2 * sqrt(3)
sqrt(1/2)          was  sqrt(1/2)          is  sqrt(2) / 2
1 / sqrt(2)        was  1 / sqrt(2)        is  sqrt(2) / 2
1 / sqrt(12)                               is  sqrt(3) / 6
1 / cbrt(2)                                is  4^(1/3) / 2
(2^3 * 5^7)^(1/3)                          is  50 * 5^(1/3)
12^(5/2)                                   is  288 * sqrt(3)
sqrt(369)          was  sqrt(369)          is  3 * sqrt(41)
```

The reduced form was previously generated as a `Simplify` candidate and lost to the shorter one on
node count, so it only ever appeared where two like radicals had to be collected. That is the wrong
judge for this: a reduced radicand is what makes two surds comparable without simplifying their
difference, and it is what SymPy, Mathematica and Maxima all print.

A negative radicand is declined — the principal cube root of `-8` is `1 + i*sqrt(3)`, not `-2`, and
reducing under an even root would settle that branch by accident.

[#281](https://github.com/asc-community/AngouriMath/issues/281),
[#205](https://github.com/asc-community/AngouriMath/issues/205), PR
[#716](https://github.com/asc-community/AngouriMath/pull/716).

### Constant equality is decided by the difference

`Equalsf` compared the two sides' separately evaluated values for exact digit equality, and
evaluating each rounds independently, so two spellings of one number differed in the last ulps.

```
sqrt(i) = (1 + i) / sqrt(2)     was  False    is  True
sin(pi/4) = sqrt(2)/2           was  False    is  True
sqrt(2) * sqrt(3) = sqrt(6)     was  False    is  True
```

It now falls back to testing whether the difference is zero, which reuses the library's existing
notion of zero rather than introducing a new tolerance. The exact-equality fast path is kept, so
infinities compare as before.

[#442](https://github.com/asc-community/AngouriMath/issues/442), PR
[#664](https://github.com/asc-community/AngouriMath/pull/664).

### `Simplify` returns conditions it did not return before

A quotient of polynomials is now put into lowest terms, and the condition that the cancelled factor
is nonzero travels with the answer — where it vanishes the quotient was `0/0` and the reduced form
is something definite. **The returned node is a `Providedf`, not a `Divf`**, so code that pattern-
matches on the node type or reads `Stringize` sees something new.

```
(x + y)^2 / ((x - y)(x + y))
    was  1 + (2y^2 + 2xy) / (x^2 - y^2)   — long-divided, still carrying the factor
    is   (x + y) / (x - y) provided not x + y = 0

(x - y)^2 / (x^2 - y^2)
    was  unchanged
    is   (x - y) / (x + y) provided not x - y = 0

sin(2u) * csc(u)
    was  csc(u) * sin(2u)
    is   2 * cos(u) provided not sin(u) = 0
```

The missing piece was a multivariate GCD (Knuth, TAOCP vol. 2 §4.6.1, algorithms C and E). Nothing
trusts its result: the divisor is divided out of both sides and multiplied back, and the quotient is
used only when both come out equal to what they started as.

[#55](https://github.com/asc-community/AngouriMath/issues/55), PR
[#713](https://github.com/asc-community/AngouriMath/pull/713);
[#557](https://github.com/asc-community/AngouriMath/issues/557), PR
[#693](https://github.com/asc-community/AngouriMath/pull/693).

### `Expand` collects like terms

```
3x + 2x                  was  3 * x + 2 * x            is  5 * x
(x+1)^2 * (x+2-1)^2      was  sixteen terms            is  1 + 4x + 6x^2 + 4x^3 + x^4
((x-1)*(x-2))^3                                        is  8 - 36x + 66x^2 - 63x^3 + 33x^4 - 9x^5 + x^6
```

A term whose coefficients cancel is written out rather than dropped, which looks wasteful and is the
whole of the correctness here: the monomial may carry a domain condition, and
`(4a - 2)/(2x) + (1 - 2a)/x` is zero only where `x` is not.

[#164](https://github.com/asc-community/AngouriMath/issues/164), PR
[#676](https://github.com/asc-community/AngouriMath/pull/676).

### `Factorize` finishes, and stops leaving its workings in the answer

```
x^2 - y^2
    was  (sqrt(x) - sqrt(y)) * (sqrt(x) + sqrt(y)) * (x^1 + y^1)   — correct, but not a factorisation
    is   (x - y) * (x + y)

a*c + a*d + b*c + b*d
    was  a*(c + d) + b*c + b*d
    is   (c + d) * (b + a)

4*x^2 - 4*y^2                                          is  4*(x - y)*(x + y)
```

The difference-of-squares rule halved both exponents without checking they were even, so it fired
again on the linear factors it had just made. Separately, every rule matched two *adjacent* terms of
the sum tree, so a sum of four could never be grouped twice over; sums of three or more now go
through a pass that flattens them.

[#531](https://github.com/asc-community/AngouriMath/issues/531),
[#178](https://github.com/asc-community/AngouriMath/issues/178), PR
[#665](https://github.com/asc-community/AngouriMath/pull/665).

### Polynomials with whole roots factor

`x^2 + 2x + 1` was `1 + x^2 + 2x` and is `(1 + x)^2`. Deliberately narrow on two counts, both
load-bearing: rational roots only — factoring through every root would answer `(x - i)(x + i)` for
`x^2 + 1`, which is not what anyone means by factoring that — and only where the polynomial splits
completely, so `x^2 - 1` stays as it is and the antiderivative of `x^2 + x` reads `x^3/3 + x^2/2`
rather than `x^2 * (x + 3/2) / 3`.

[#177](https://github.com/asc-community/AngouriMath/issues/177), PR
[#669](https://github.com/asc-community/AngouriMath/pull/669).

### The trigonometric tables are read backwards, and the Pythagorean identity in more arrangements

```
arcsin(1/2)                    was  arcsin(1/2)          is  pi / 6
arcsin(sqrt(2) / 2)                                      is  pi / 4
arccos(-1/2)                                             is  2/3 * pi
arctan(1)                      was  arctan(1)            is  pi / 4
arctan(sqrt(3))                                          is  pi / 3
arctan(1/2) + arctan(1/3)                                is  pi / 4

1 - sin(t)^2                   was  unchanged            is  cos(t)^2
1 + tan(t)^2                   was  unchanged            is  sec(t)^2
1 + cotan(t)^2                 was  unchanged            is  csc(t)^2
1 - sin(t)^2 - cos(t)^2        was  unchanged            is  0
sec(t)^2 - tan(t)^2            was  unchanged            is  1 provided not cos(t) = 0
```

Note the last one: the identity divided through by `cos^2` is only `1` where `cos(t)` is not zero,
and the condition comes back with the answer rather than being dropped. That is a `Providedf`, as in
the section above.

The tables were only ever read forwards. `arcsin(0.4999999)` still comes back unanswered: a double
comparison picks the candidate and the exact value confirms it.

The Pythagorean rewrites are one-way on purpose — `1 - sin(:)^2` becomes `cos(:)^2` and not the
reverse, since stating it both ways would undo each rewrite as fast as it fired.

[#569](https://github.com/asc-community/AngouriMath/issues/569),
[#179](https://github.com/asc-community/AngouriMath/issues/179), PR
[#716](https://github.com/asc-community/AngouriMath/pull/716);
[#725](https://github.com/asc-community/AngouriMath/issues/725), PR
[#726](https://github.com/asc-community/AngouriMath/pull/726).

### Interval intersections distribute over unions

`(-1; 1) /\ ((-(sqrt(33) + 3) / 6; (sqrt(33) - 3) / 6) \/ (1; +oo))` came back exactly as written and
is now `(-1; (sqrt(33) - 3) / 6)`. `IntersectIntervalAndInterval` also gave up unless every endpoint
was a bare `Real` node; endpoints are now compared by what they evaluate to, while the bounds of the
answer are taken from the original expressions, so the result stays exact rather than becoming a
hundred decimal places.

[#415](https://github.com/asc-community/AngouriMath/issues/415), PR
[#677](https://github.com/asc-community/AngouriMath/pull/677).

---

## Solving

### An identity equation is answered with every value, not with none

An equation that puts no condition on the variable is satisfied by every value of it. The library
said the opposite in two different ways, one of them a wrong answer rather than a missing one:

```
(0 = 0).Solve(x)                       was  { }        is  CC
(x - x = 0).Solve(x)                   was  { 0 }      is  CC
```

The polynomial solver never checked whether a monomial's coefficient had cancelled, so `x - x` was
read as one times `x`, of degree one, with the root `0`.

A system that no longer constrains a variable is now answered with a free parameter rather than with
a single tuple that is *a* solution but not *the* solution set:

```
{ x - y = 0, 2x - 2y = 0 }.Solve(x, y)     was  [[0, 0]]     is  [[t_1, -t_1 / (-1)]]
{ x + y - 1, 2x + 2y - 2 }.Solve(x, y)                       is  [[t_1, -(t_1 + -1)]]
```

Those are `[[t_1, t_1]]` and `[[t_1, 1 - t_1]]` as numbers; the parameter arrives unsimplified.

`null` still means there are no solutions, which is the answer for a system that contradicts itself.
**Code that assumed a `FiniteSet` back from `Solve` now has `CC` or a parametric family to handle.**

[#550](https://github.com/asc-community/AngouriMath/issues/550), PR
[#679](https://github.com/asc-community/AngouriMath/pull/679).

### A sign error inverting a subtraction

`2 = 1/(5 - c)` solved for `c` answered `-9/2` where the answer is `9/2`. See the `Minusf` entry
above for the cause.

[#632](https://github.com/asc-community/AngouriMath/issues/632), PR
[#664](https://github.com/asc-community/AngouriMath/pull/664).

### Roots are checked against the equation they came from

Rewrites such as `log(a) + log(b) = log(a * b)` widen the domain, and the conditions cannot each
survive the chain of substitutions after them. Answers are now checked once, at the top, against the
equation as the caller wrote it. **Some root sets got smaller.**

```
ln(x) + ln(x+1) = 0
    was  { (-1 - sqrt(5)) / 2, (-1 + sqrt(5)) / 2 }
    is   { (-1 + sqrt(5)) / 2 }
```

`(-1 - sqrt(5))/2` is `-1.618…`, where both logarithms are taken off the negative reals and the
left-hand side comes to `2*pi*i`. A root is dropped only on positive evidence — anything that will
not evaluate to a number, a parametric family like `pi + 2*pi*n_1` among them, is kept.

The check also ran only when the caller wrote the equation out as `expr = 0`, so the two entry
points disagreed. Both check now:

```
"2^x + 2^(2x) - 6".SolveEquation(x)
    was  { ln((-3) ^ (1 / ln(2))), 1 }     is  { 1 }
```

A root found numerically is only as accurate as the search that found it, so the residual is read
against the largest of the terms it is the sum of rather than on its own size. Without that, all
four roots of `1/210 - 17x/210 + 101x^2/210 - 247x^3/210 + x^4` were dropped and the quartic came
back empty.

PRs [#664](https://github.com/asc-community/AngouriMath/pull/664) and
[#685](https://github.com/asc-community/AngouriMath/pull/685).

### `x + ln(x) = 0` no longer throws out of `Solve`

It raised `UncompilableNodeException` through the public `Solve`: the Newton solver compiles the
simplified expression, simplification leaves `Providedf` nodes behind, and the compiler has no way
to represent one. The conditions come off before compilation and the roots are verified after. It
answers `0.5671…`.

PR [#664](https://github.com/asc-community/AngouriMath/pull/664).

### Numeric root sets are deduplicated, and the numeric search finds more

The search starts from a grid, and the same root reached from different starting points came back
agreeing to about sixteen significant digits and differing after that, so **each starting point
contributed a root of its own**:

```
x^5 + 3x + 1        was  28 roots     is  5
x^6 + x + 1         was  23 roots     is  6
```

Four of the 28 were `-0.83907243306660750`, `…61`, `…73` and `…84`. Candidates closer together than
the iteration can tell apart are now one root, and the one kept from each group is whichever leaves
the equation closest to zero.

Three separate faults were also making the search miss roots that were inside the region:

- The grid divided each index by the step count with `EDecimal`'s own operator, which carries no
  context and answers `NaN` wherever the quotient does not terminate in base ten. Only step counts
  of the form `2^a * 5^b` divided exactly, so at 3, 6, 7, 9, 12, 21 and most other values the search
  covered a single corner. **Asking for a finer search made the answer worse**; the default divides
  exactly, which is why nothing caught it.
  ([#115](https://github.com/asc-community/AngouriMath/issues/115), PR
  [#695](https://github.com/asc-community/AngouriMath/pull/695))
- The grid is two-dimensional, so a step count of `N` lays real starting points only `(To-From)/N`
  apart — 2 at the default. The real axis is now scanned separately at `StepCount.Re * StepCount.Im`
  points and Newton runs from the brackets a sign change finds, making the spacing that matters for
  a real root `(To-From)/N^2`. `arcsin(x) - x*pi/3` went from `{ 0 }` to `{ -1/2, 0, 1/2 }`.
  ([#115](https://github.com/asc-community/AngouriMath/issues/115), PR
  [#729](https://github.com/asc-community/AngouriMath/pull/729))
- The `FastExpression` compiler conjugated arcsine unconditionally, which is right on the two branch
  cuts and wrong everywhere else, so Newton walked away from roots it started at.
  (PR [#696](https://github.com/asc-community/AngouriMath/pull/696))

**Do not depend on the order roots come back in.** `TestFormula8` asserted which of the two roots of
`x^2 + 1` a `HashSet` handed back first; that was never the solver's to decide, and candidates are
now ordered before being collapsed so that which one is compared against which does not depend on
the order the grid produced them in.

Near-rational roots are also no longer presented as exact ones
([#235](https://github.com/asc-community/AngouriMath/issues/235), PR
[#668](https://github.com/asc-community/AngouriMath/pull/668)).

PR [#686](https://github.com/asc-community/AngouriMath/pull/686).

### Dense linear systems

Elimination gave up where it stalled instead of trying another equation, so a reporter's small
system did not solve. [#608](https://github.com/asc-community/AngouriMath/issues/608), PR
[#667](https://github.com/asc-community/AngouriMath/pull/667).

---

## Limits

More limits are answered, and — this is the part to read — **some that were answered are answered
differently, and some that were unevaluated are now `NaN`**. The three results are three different
claims and they are not interchangeable:

| result | means |
|---|---|
| unevaluated (`Limitf`, the expression back) | "I could not settle this" |
| `NaN` | "**this does not exist**" |
| a value | "this is the value" |

### Wrong values that became right ones

```
lim x->0   (1+x)^(1/x)          was  1        is  e
lim x->0+  (1+x)^(1/x)          was  1        is  e
lim x->+oo (x-5)^x / x^x        was  1 *      is  e^(-5)
lim x->0-  sin(x)/x             was  NaN      is  1
lim x->0+  cos(x)/sin(x)        was  NaN      is  +oo
lim x->0   1/x^2 - 1/sin(x)^2   was  NaN      is  -1/3
```

\* unevaluated on 1.4.0 and `1` once the descent reached it; the wrong answer was reachable from
`Simplify`.

The second remarkable limit required the exponent to have an infinite *two-sided* limit, where at
`x -> 0` the exponent `1/x` goes to `-oo` on the left and `+oo` on the right — no two-sided limit,
even though the magnitude diverges, which is all the rule needs. Separately, the rule ran once at
the top of `ComputeLimit`, so a `1^oo` *created* by a later simplification was judged by rules that
ran before it existed.

`sin(x)/x` from one side is the one worth naming on its own: the library said the most familiar
limit in the subject did not exist. The one-sided path skipped everything the two-sided path has,
both in front of the descent and behind it.

PRs [#664](https://github.com/asc-community/AngouriMath/pull/664),
[#697](https://github.com/asc-community/AngouriMath/pull/697),
[#700](https://github.com/asc-community/AngouriMath/pull/700),
[#728](https://github.com/asc-community/AngouriMath/pull/728),
[#739](https://github.com/asc-community/AngouriMath/pull/739).

### Unevaluated that became `NaN` — a stronger claim, not a weaker one

```
lim x->+oo sin(x)               was  unevaluated    is  NaN
lim x->0   sin(1/x)             was  unevaluated    is  NaN
```

A sine whose argument grows without bound takes every value in its range infinitely often and
settles on none, so the limit genuinely does not exist and `NaN` says so where an unevaluated node
said only that no rule found one. Two tests changed rather than broke, one of which says in its own
summary that the answer must not become `NaN` "since NaN asserts that the limit does not exist
rather than that it was not found" — the principle is right, and a sine's range is what establishes
non-existence.

`lim x->+oo (x + sin(x))/x`, `x * sin(x)` and `x * cos(x)` are still unevaluated.

[#723](https://github.com/asc-community/AngouriMath/issues/723), PR
[#724](https://github.com/asc-community/AngouriMath/pull/724).

### Unevaluated that became a value

A large family, from l'Hôpital's rule at infinity, Gruntz's algorithm, the squeeze theorem,
differences of divergent parts, indeterminate powers and products, and limits of piecewise
expressions. A sample:

```
lim x->+oo sin(x) / x                     unevaluated  ->  0
lim x->0   x * sin(1 / x)                 unevaluated  ->  0
lim x->+oo ln(x) / x                      unevaluated  ->  0
lim x->+oo e^(x + e^(-x)) - e^x           unevaluated  ->  1
lim x->+oo sqrt(x^2 + 3x) - sqrt(x^2+1)   unevaluated  ->  3/2
lim x->+oo x^20 / e^x                     unevaluated  ->  0
lim x->+oo arcsin(x)                      unevaluated  ->  pi/2 - +ooi
lim x->0   csc(x)^2 - 1/x^2               unevaluated  ->  1/3
```

`lim x->+oo sin(a*x)/x` is still unevaluated, correctly — a free variable reads as complex here, and
`sin(i*t)` is `i*sinh(t)`, which grows without bound, so it is not `0` for every `a`.

`lim x->0 x * ln(x)` and `lim x->0 x^x` are still `NaN` deliberately: `x^x` is not real to the left
of `0`, so the `1` from that side is the complex continuation and not a limit to agree with.

PRs [#680](https://github.com/asc-community/AngouriMath/pull/680),
[#682](https://github.com/asc-community/AngouriMath/pull/682),
[#694](https://github.com/asc-community/AngouriMath/pull/694),
[#710](https://github.com/asc-community/AngouriMath/pull/710),
[#714](https://github.com/asc-community/AngouriMath/pull/714),
[#724](https://github.com/asc-community/AngouriMath/pull/724).

### `lim x->2 signum(x)` no longer kills the process

`Signumf` was the one node whose limit override handed back an unevaluated limit of the very
expression it was asked about. That is a cycle, not a failure to answer: the two-sided path compares
its two one-sided results by evaluating them, evaluating a limit computes it, and computing arrives
back at the same override. Some four thousand frames later the stack ran out — which kills the
process rather than raising anything catchable. It answers `1`.

[#704](https://github.com/asc-community/AngouriMath/issues/704), PR
[#705](https://github.com/asc-community/AngouriMath/pull/705).

---

## Differentiation and integration

### `sgn` has a derivative

`derivative(sgn(x), x)` came back unevaluated, with a note that the delta function would be needed
first. Leaving it unevaluated does not represent the delta either — it only means the derivative of
anything containing `sgn` cannot be evaluated at all, and that reached callers: the antiderivative
of `abs(x)` is `sgn(x) * x^2 / 2`, and differentiating it back stopped there.

`sgn` is flat either side of zero and has no derivative at zero, so the derivative is `0` wherever
it exists, and the condition says where: `0 provided not x = 0`. This is the stance `abs` already
took right below it.

PR [#672](https://github.com/asc-community/AngouriMath/pull/672).

### Some antiderivatives were not antiderivatives

`CreateUnique` handed back a variable name already in use, because it looked for names to avoid by
cutting at the *first* underscore — so for the prefix `u_sub`, which is the one integration uses, it
read an existing `u_sub_1` as the prefix `u` with the index `sub_1`, parsed no number, and returned
`u_sub_1` as though it were free. That only bites when integration substitutes twice, and the result
was a nonsense expression that passed the test for a successful substitution.

```
int x(x^2+1)^3 dx      was the expanded polynomial      is  (x^2 + 1)^4 / 8
int x(x^2+1)^10 dx     was the expanded polynomial      is  (x^2 + 1)^11 / 22
int 3x^2(x^3+2)^2 dx   was not an antiderivative        is  (x^3 + 2)^3 / 3
```

The first two were correct — differentiating either back gives the integrand exactly — and only the
form changed. The third did not: differentiating the old answer gives
`4.5x^8 + 10.5x^5 + 12x^2` against the integrand `3x^8 + 12x^5 + 12x^2`, and the two are already
0.0035 apart at `x = 0.3` and 506 apart at `x = 2.1`.

Those are the values. The antiderivative table does not fold its own arithmetic, so the printed form
is `(x ^ 2 + 1) ^ (3 + 1) / (3 + 1) / 2 + C` and the like — pass it through `Simplify` if you want it
tidy, and do not assert on the string.

The third is the one to check for: `d/dx` of the old answer is `4.5x^8 + 10.5x^5 + 12x^2`, against
the integrand `3x^8 + 12x^5 + 12x^2`.

PR [#670](https://github.com/asc-community/AngouriMath/pull/670).

### An integral that came back holding a `NaN`

Substitution candidates were not required to depend on `x`, and `ln(e)` is picked up from any
integrand carrying a logarithm — its derivative is `0`, the integrand divided by it is `NaN`, and
`NaN` contains no `x`, so it passed the test for a successful substitution and was returned as the
antiderivative.

```
int (sin(x)^2 + cos(x)^2) dx
    was  NaN * (sin(x)^2 + cos(x)^2) + C
    is   an antiderivative, via the power-reduction entry added later
```

The result of the division is now checked for `NaN` directly, which catches every way of arriving at
one.

PRs [#665](https://github.com/asc-community/AngouriMath/pull/665) and
[#675](https://github.com/asc-community/AngouriMath/pull/675).

### Two integrals that were answered are now unevaluated

This one is a loss, and it is deliberate. Both were correct on 1.4.0 — differentiating either back
gives the integrand exactly — and both are `integral(…)` today:

```
int x(x^2 + x + 1)^2 dx        was answered (2968 ms)    is unevaluated
int x^2(x^2 + 1)^2 dx          was answered (1414 ms)    is unevaluated
```

They reached an answer through integration by parts re-entering itself, which is the same path that
overflowed the stack on `int x*ln(x)` and killed the process. Bounding the recursion closed the
route these two arrived by along with it. No answer is a limit; a crash is not.

**If you were getting an expression for either, you now get an unevaluated node** — check for one
rather than assuming an antiderivative comes back. Both are worth reopening as coverage, and neither
was ever tracked by an issue.

PRs [#665](https://github.com/asc-community/AngouriMath/pull/665) and
[#670](https://github.com/asc-community/AngouriMath/pull/670).

### `int x*ln(x)` no longer overflows the stack

`ComputeIndefiniteIntegral` takes an `integrateByParts` flag so that integration by parts can call
back into it without re-entering itself, and four of the recursive solvers did not hand the flag on
— so by parts was switched back on one level below the call that had switched it off.

`int sin(x)^2 dx` was the same fault without the crash: it had not returned after forty seconds on
1.4.0, and answers instantly now.

PR [#665](https://github.com/asc-community/AngouriMath/pull/665).

### Many integrals that had no antiderivative now have one

Additive; listed because an unevaluated `integral(...)` you were branching on may now be an
expression. The families added are `k / sqrt(ax^2 + bx + c)`, an exponential times a sine or cosine,
the inverse trigonometric functions, any whole power of sine times any whole power of cosine,
`sqrt(ax^2 + bx + c)`, a linear numerator over a linear or quadratic denominator, `1/cos(u)^2` and
`1/sin(u)^2`, `k / (x^2 sqrt(ax^2 + c))`, and rational functions split at a rational root of the
denominator — `1/(x^3 + 1)`, `1/(x^4 - 1)`, `1/((x-1)(x-2)(x-3))` and the rest.

[#233](https://github.com/asc-community/AngouriMath/issues/233), PRs
[#675](https://github.com/asc-community/AngouriMath/pull/675),
[#681](https://github.com/asc-community/AngouriMath/pull/681),
[#690](https://github.com/asc-community/AngouriMath/pull/690),
[#691](https://github.com/asc-community/AngouriMath/pull/691).

---

## Compilation

### A missing variable is named

```csharp
"a * x".ToEntity().Compile("x");
// was  KeyNotFoundException: The given key 'a' was not present in the dictionary.
// is   UncompilableNodeException: a is not among the variables the expression is being
//      compiled over, which are x
```

Both compilers did it, each by reaching into its own dictionary of arguments and letting the lookup
fail, naming neither the variable, nor the compilation, nor anything a caller of `Compile` could act
on. **Change your `catch` clause.** Constants are unaffected: `pi` and `e` are substituted before any
variable is looked up, so `"pi * x"` still compiles over `x` alone.

PR [#688](https://github.com/asc-community/AngouriMath/pull/688).

### Compiled arcsine was the conjugate of the arcsine

The `FastExpression` compiler conjugated arcsine unconditionally, which is right on the two branch
cuts — the real arguments outside `[-1, 1]`, where the library does take the lower side and where
the only test of it looked — and wrong everywhere else.

```
arcsin(0.5 + 0.1i) compiled
    was  0.5198083869450859 - 0.11496532217013944i
    is   0.5198083869450859 + 0.11496532217013944i
```

Measured against `Evaled` over a grid of 63 points spanning both cuts, the `FastExpression` compiler
agreed at 15 and the Linq compiler, which does not conjugate, at 59 — neither right, wrong in
complementary places. Both now go through one helper that conjugates on the cut and nowhere else and
agree at all 63. **Nothing changes on the real axis.** Arccosecant is the same, being an arcsine of
a reciprocal.

PR [#696](https://github.com/asc-community/AngouriMath/pull/696).

### A compiled expression may be called from more than one thread

`FastExpression` kept its working stack and its subexpression cache as instance fields, so two
threads calling one compiled expression interleaved their pushes and pops. Measured with sixteen
threads over 400000 calls: 399328 threw "Unused values remain in the stack", 418 threw "Stack
empty", and one returned `-0.483` where the answer is `14884.07`. The damage was permanent — the
count check throws before anything is popped, so a single racing call left its leftovers behind and
every later call failed on them, on one thread or on many.

Both buffers are now per thread. Nothing is slower for it.

[#637](https://github.com/asc-community/AngouriMath/issues/637), PR
[#698](https://github.com/asc-community/AngouriMath/pull/698).

### Expressions whose matrices cancel out compile

And what cannot be compiled is named rather than failing obscurely.
[#425](https://github.com/asc-community/AngouriMath/issues/425),
[#526](https://github.com/asc-community/AngouriMath/issues/526), PR
[#673](https://github.com/asc-community/AngouriMath/pull/673).

---

## Additions

Not behavioural changes, listed so that a reader working through this file has the whole picture.

- **A modulus node.** `Modf`, `MathS.Mod`, the `%` operator on `Entity` in C# and F#, the `mod`
  keyword in the parser, evaluation, simplification, differentiation, limits, both compilers, LaTeX
  and SymPy export. `a % b` on `Entity` is *not* `int`'s `%` — it is floored, and its documentation
  says so. [#402](https://github.com/asc-community/AngouriMath/issues/402),
  [#618](https://github.com/asc-community/AngouriMath/issues/618), PR
  [#703](https://github.com/asc-community/AngouriMath/pull/703).
- **Taking part of a matrix by range.** `a[1.., ..]`, `a[..2, 1..3]`, `a[^1.., ^1..]`. Both extents
  have to be written out. An empty range is refused; a range reaching past the matrix throws from the
  range itself. [#443](https://github.com/asc-community/AngouriMath/issues/443), PR
  [#683](https://github.com/asc-community/AngouriMath/pull/683).
- **A syntax reference**, at [`Sources/AngouriMath/Docs/Usage/Syntax.md`](Sources/AngouriMath/Docs/Usage/Syntax.md).
  The grammar was previously the only statement of what the language accepts, and it is ANTLR source.
- **MyGet is gone.** Releases still publish to NuGet; the per-push master feed does not exist any
  more, so remove it from your `NuGet.Config` if you had it. PR
  [#701](https://github.com/asc-community/AngouriMath/pull/701).

---

## If one of these hurts

Open an issue at https://github.com/asc-community/AngouriMath/issues saying what you were relying
on and what it was for. A change made because the old answer was wrong will not be reverted to the
wrong answer, but where the old behaviour was *useful* — a tolerance, a printed form, a name — there
is usually a way to have both, and the setting or the overload to add is worth knowing about.
