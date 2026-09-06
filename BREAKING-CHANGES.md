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

## Unreleased — since 2.4.0

### At a glance

| Silent? | What | Was | Is |
|---|---|---|---|
| **Silent** | `"domain(1/2, CC)".ToEntity()`, and every quotient of two integer literals annotated with the codomain its node type does not default to | `1/2` — the annotation dropped, equal to the unannotated literal | `1/2` carrying `Codomain = Complex`, which prints and reads back as `domain(1/2, CC)` |
| | `"a in (a / 3; 3a)".ToEntity().Simplify()`, and every denominator but 2 | `a in (a / 3; 3 * a)` — left as written | `a > 0` |
| | `MathS.Matrix(...).Determinant` on a symbolic matrix of polynomials | `a * d + -b * c`, Laplace's nested expansion | `a * d - b * c`, expanded — the same value, no larger |
| | the same on a numeric matrix past 10x10 | did not return | 11x11 in 2 ms, 30x30 in 22 ms |
| **Silent** | `MathS.Equations(...).Solve(...)` on a system neither internal path can finish | ran without a bound — cyclic-6 exceeded 20 s | `NotSufficientlySupportedException`, naming both paths |
| | the same with `MathS.Settings.Budget` set below what the solve needs | answered anyway, the fall-through having no budget | raises |
| | `"a in (a / 2; 0)".ToEntity().Simplify()`, and every interval demanding both signs | left as written | `False provided a in RR` |
| | `"x^2/(x^4 + 1)".Integrate("x")`, and every quotient whose denominator is a biquadratic irreducible over `Q` | `integral(x ^ 2 / (x ^ 4 + 1), x)` — left unevaluated | the antiderivative, over the real quadratic factors |
| | `"sqrt(x)/(1 + x^2)".Integrate("x")`, and every integrand a fractional power of the variable makes rational | `integral(sqrt(x) / (1 + x ^ 2), x)` — left unevaluated | the antiderivative |
| | `"sqrt(tan(x))".Integrate("x")`, and every integrand that is a function of `tan(x)` alone and rational in it | `integral(sqrt(tan(x)), x)` — left unevaluated | the antiderivative |
| | `"x^2/(x + 1)".Integrate("x")`, and every improper quotient of polynomials | `integral(x ^ 2 / (x + 1), x)` — left unevaluated | `x ^ 2 / 2 + -x + ln(x + 1) + C` |
| | `MathS.Equations("2*x - 4*y - 12").Solve("x", "y")`, and every linear system with fewer equations than unknowns | `WrongNumberOfArgumentsException` | `[[6 + 2 * t_1, t_1]]` — the family of all its solutions |
| **Silent** | `"not (x = 1)".ToEntity().Solve("x")`, and every negation | `{  }` — no value satisfies it | `{ x : not x = 1 }` |
| **Silent** | `"not (x > 1)".ToEntity().Solve("x")`, and every negated comparison | `{  }` | `(-oo; 1]` |
| **Silent** | `"(x = 1) implies (x = 2)".ToEntity().Solve("x")`, and every implication | `{ 2 } \/ BB` — truth values in the solution set of a numeric question | `{ x : not x = 1 }` |
| | `"domain((-oo; +oo), Any) = RR".ToEntity().Solve("x")`, and every unbounded interval widened to `Any` | `NotSufficientlySupportedException: There is no special set for domain Any` | `{  }` |
| **Silent** | `RewriteRules.DivisionPreparing.Rules[0].Name`, and every rule of the twenty-seven sets now described from their data form | `Mulf(var any1, Divf(Integer(1), var any2))` — the `switch` arm's rendered pattern | `reciprocal-factor-becomes-a-quotient` |
| | `RewriteRules.ExpandFactorialDivisions.Rules.Count`, and `FactorizeFactorialMultiplications` | `8` | `3` — the same rewrites, five of the eight arms being one commutative pattern |
| | `RewriteRules.Boolean.Rules.Count` | `36` | `20` — a commutative pattern finds a shared operand wherever it sits |
| | `RewriteRules.NumericNeat.Rules.Count`, and `Factorization` 22 -> 11 | `16` | `11` |
| | `RewriteRules.Trigonometric.Rules.Count` | `43` | `33` |
| | `RewriteRules.Power.Rules.Count` | `35` | `31` |
| | `RewriteRules.Common.Rules.Count` | `100` | `62` |
| | `RewriteRules.All.Sum(set => set.Rules.Count)` | `407` | `313` |
| **Silent** | `RewriteRules.ExpandFactorialDivisions.Rules[0].Growth` | `Collects` — guessed from string length | `Unknown`; whether it collects depends on the offsets |
| **Silent** | `RewriteStep.Soundness` on a rewrite whose rule declares a tier — `RewriteRules.SetOperator` on `A /\ A`, and every rewrite of the nineteen sets described from their data form | `SoundUnderAssumptions` — its rule set's tier, which is the minimum over every rule in the set | `Sound` — the rule's own |
| **Silent** | `DerivationStep.Soundness` | its rule set's tier | the weakest tier any rewrite that actually fired inside the step holds at |
| **Silent** | `"5 - (0; 1)".ToEntity().Simplify()`, and every interval subtracted from something | `(5; 4)` — a left end above its right, so the empty set | `(4; 5)` |
| **Silent** | `"4.5 in (5 - (0; 1))".ToEntity().Simplify()` | `False` | `True` |
| **Silent** | `"5 - [0; 1)".ToEntity().Simplify()` | `[5; 4)` — the openness left where it was | `(4; 5]` |
| | `"3 * [2; 3]".ToEntity().Simplify()`, and every interval scaled by a constant | `3 * [2; 3]` — left alone | `[6; 9]` |
| | `"[2; 3] / 2".ToEntity().Simplify()` | `[2; 3] / 2` | `[1; 3/2]` |
| **Silent** | `"[2; 3) * (-1)".ToEntity().Simplify()` | `[2; 3) * (-1)` | `(-3; -2]` — reflected, ends and openness both |
| | `"0 - [0; 1)".ToEntity().Simplify()` | `-[0; 1)` | `(-1; 0]` |
| | `"2 * x + 4 * a".ToEntity().Factorize()`, and every sum whose whole coefficients share a divisor | `2 * x + 4 * a` — left alone | `2 * (x + 2 * a)` |
| | `Transformation.Factorization.Name` | `… then polynomial-factorization` | `… then polynomial-factorization then numeric-content` |
| **Silent** | `"arccotan(-1)".ToEntity().Simplify()`, and every negative argument the inverse-trigonometric table knows | `3/4 * pi` — the textbook range, and **not equal to `arccotan(-1)`**, whose value is `-pi/4` | `-1/4 * pi` |
| | `"e ^ ln(x)".ToEntity().Simplify()`, and every exponential of a natural logarithm | `e ^ ln(x)` — left as written | `x` |
| | `"a => a + 3".ToEntity()`, and every lambda written with an arrow | `UnhandledParseException` | `lambda(a, a + 3)` |
| | `"sqrt(5 + 2 * sqrt(6))".ToEntity().Simplify()`, and every nested radical whose discriminant is a rational square | `sqrt(5 + 2 * sqrt(6))` — left as written | `sqrt(3) + sqrt(2)` |
| | `"sum(k, k, 1, n)".ToEntity().Simplify()`, and every summation whose body is a polynomial in the index | `sum(k, k, 1, n)` — carried | `piecewise((n + n ^ 2) / 2 provided n >= 0, 0)` |
| | `"sum(k, k, 1, 100000)".ToEntity().Simplify()`, and every concrete range past a hundred terms | `sum(k, k, 1, 100000)` — carried | `5000050000` |
| | `"product(k, k, 1, n)".ToEntity().Simplify()`, and every product whose body is a monomial in the index | `product(k, k, 1, n)` — carried | `piecewise(n! provided n >= 1, 1)` |
| | `"1/(x^2 + 1)^2".Integrate("x")`, and every proper rational function over a repeated irreducible quadratic | `integral(1 / (1 + x ^ 2) ^ 2, x)` — left unevaluated, after 2.9 s | `arctan(x) / 2 + C + 1/2 * x / (x ^ 2 + 1)`, in 0.11 s |
| | `"x^2/(x^2 + 2)^2".Integrate("x")`, and every polynomial numerator over one | `integral(x ^ 2 / (x ^ 2 + 2) ^ 2, x)` — left unevaluated, after 93 s | the antiderivative, in 0.57 s |
| | `"1/(x^2 - 1)^2".Integrate("x")`, and every repeated quadratic whose roots are real | `C - ln(x + -1) / 4 + ln(1 + 2 * x + x ^ 2) / 8 + -1/2 * x / (x ^ 2 + -1)` | `C - ln(1 + (-2) / (x + 1)) / 4 + -1/2 * x / (x ^ 2 + -1)` — the same value, one logarithm rather than two |
| **Silent** | `"1/x - 1/x".ToEntity().Simplify()`, and every difference of a term from itself where that term can be undefined | `0`, including at `x = 0` where neither side has a value | `0 provided not x = 0` |
| **Silent** | `"(x + 1)!/(x + 1)!".ToEntity().Simplify()`, and every cancelled quotient whose repeated part can be undefined | `1 provided not (1 + x)! = 0` | `1 provided 1 + x in RR and (1 + x >= 0 or not 1 + x in ZZ)` — the same value everywhere, a condition that says why |
| | `"ln(x)/ln(x)".ToEntity().Simplify()`, and every quotient divided out by a divisor that can be undefined | `1 provided not ln(x) = 0` — which is `1` at `x = 0`, where the quotient has no value | `1 provided not ln(x) = 0 and not x = 0` |

### A cancelled quotient says its operand is defined, not only non-zero

`a / a = 1` and `(keep * c) / c = keep` attached `provided c is not zero`. That excludes the zero of
`c` and not the points where `c` has **no value at all**, and where `c` has none the quotient has none
either — while the cancelled result does.

```
"ln(x) / ln(x)".Simplify()   was  1 provided not ln(x) = 0    at x = 0: 1, and the quotient is NaN
```

Both now conjoin the operand's own `DomainCondition`, the way `k - k = 0` does since
[#1169](https://github.com/asc-community/AngouriMath/issues/1169).
[#1174](https://github.com/asc-community/AngouriMath/issues/1174)

**Only one shape's printed answer moves, and its value does not.** `(x + 1)!/(x + 1)!` carries the
factorial's domain instead of `not (1 + x)! = 0`. Measured at x = -4, -3, -2, -1, 0, 2 and -3/2, the
old condition and the new one agree with the unsimplified quotient at every point — `NaN` at the
three gamma poles and `1` elsewhere. The old form was adequate there by accident, since `(1 + x)!` is
`NaN` at a pole and a comparison against `NaN` is not `True`; the new one states the reason rather
than relying on it. `x / x`, `sin(x) / sin(x)`, `(x * y) / (x * y)` and every other operand that
cannot be undefined fold the added clause away and are unchanged.

**And the polynomial division carries its divisor's domain too.** `n / d = quotient + remainder` is
only the quotient where `d` has a value: `ln(x) / ln(x)` divides out to `1 + 0 / ln(x)`, which is `1`
at `x = 0` while the quotient it came from is undefined there, because the remainder term carries
only `ln(x) != 0`. That candidate is the one `Simplify` rated best, so it was what a caller actually
saw. With it fixed:

```
"ln(x) / ln(x)".Simplify()   was  1 provided not ln(x) = 0    at x = 0: 1
                             is   1 provided not ln(x) = 0 and not x = 0    at x = 0: NaN
```

Ordinary divisions are untouched, because a divisor that cannot be undefined has a domain condition
of `True` and it folds away — `(x^2 - 1)/(x - 1)`, `(x^3 + 1)/(x + 1)`, `x^2 / x` and `x^3 / x^2` all
answer exactly what they did. `x!/x!` moves the same way `(x + 1)!/(x + 1)!` does, and for the same
reason; measured at x = -4, -3, -2, -1, 0, 1, 3, 1/2 and -1/2, old and new agree at every point.

### A term subtracted from itself says what it assumes

`k - k = 0` was declared `Sound`, which means it holds for every value the pattern admits with
nothing assumed. It assumed one: that `k` has a value.

```
"1/x - 1/x".Simplify()          was  0                    is  0 provided not x = 0
"ln(x) - ln(x)".Simplify()      was  0                    is  0 provided not x = 0
"x^(-2) - x^(-2)".Simplify()    was  0                    is  0 provided not x = 0
```

At `x = 0` the left side is undefined and `0` is not, so the rewrite invented an answer. Its sibling
for the same shape, `a / a = 1`, has always attached `provided a is not zero`; this one attached
nothing ([#1169](https://github.com/asc-community/AngouriMath/issues/1169)).

**Nothing else moves.** The condition is the operand's own `DomainCondition`, which is trivially
true for anything that cannot be undefined and folds away — `x - x`, `sin(x) - sin(x)`,
`(x + 1) - (x + 1)`, `x * y - x * y` and `sqrt(x) - sqrt(x)` all still answer a bare `0`, and
`x - x + y` still answers `y`. Only the three shapes above gain a condition, and each of them is one
that was wrong.

Found by `RuleEffectsMeasuredTest.NoSoundRuleChangesWhereItsResultHasAValue`, which substitutes a
real for every free variable — **zero among them** — and compares whether both sides still evaluate
to something finite.

### A repeated quadratic denominator is integrated

`1/(x^2 + 1)^2` had no antiderivative, while `1/(x^2 - 1)^2` and `1/(x^2 + 2x + 1)^2` both did — a
denominator with real roots comes apart into linear factors and never reached the gap. What was
missing was the irreducible case
([#180](https://github.com/asc-community/AngouriMath/issues/180)).

```
"1/(x^2 + 1)^2".Integrate("x")     was  integral(1 / (1 + x ^ 2) ^ 2, x)
                                    is  arctan(x) / 2 + C + 1/2 * x / (x ^ 2 + 1)
"x^2/(x^2 + 2)^2".Integrate("x")   was  integral(x ^ 2 / (x ^ 2 + 2) ^ 2, x)
                                    is  (sqrt(2) * arctan(sqrt(2) * x / 2) * 2 + (-4) * x / (x ^ 2 + 2)) / 8
                                          + C provided not x ^ 2 + 2 = 0
```

The condition on that second one is the library's own and it is true: the antiderivative has no
value where the denominator vanishes. It is attached to some of these answers and not others, and I
did not pin down what decides which — recorded as observed rather than explained.

Writing `Q` for the quadratic, `u` for its derivative `2ax + b` and `D` for `4ac - b^2`, the
identity `u^2 = 4aQ - D` turns the derivative of `u/Q^(m-1)` into an equation for the integral of
`1/Q^m`, which is unrolled down to the single power the table already answered. It is an identity in
`a`, `b` and `c` rather than a fact about signs, so one line serves both signs of the discriminant;
`D = 0` is the sole exclusion, and the division by `D` is why. A numerator of higher degree is
divided by `Q` first.

**A denominator with real roots now takes this route rather than partial fractions.** The value is
unchanged and the form is shorter — one logarithm where there were two — which is why the
node-count metric that ranks candidates selects it. No test pinned the old spelling.

**The cost, measured rather than estimated.** Where the integrand is rational this is between 26 and
165 times faster, because `TryStandardIntegrals` runs before every search and a shape answered
outright never enters the candidate exploration. Where the integrand is a transcendental function
over a repeated quadratic — `sin(x)/(x^2 + 1)^2`, `e^x/(x^2 + 1)^2`, `ln(x)/(x^2 + 1)^2`, none of
which has an elementary antiderivative — the same search now takes **3.4 to 10.8 times longer to
conclude nothing**, because the sub-integral it used to fail on immediately now succeeds and
integration by parts carries on from a larger expression. Both arms measured on one machine, master
and branch, the same way:

| | master | now |
|---|---|---|
| `1/(x^2 + 1)^2` | 2879 ms, unevaluated | **111 ms, answered** |
| `x^2/(x^2 + 2)^2` | 92 990 ms, unevaluated | **565 ms, answered** |
| `1/(x^4 + 1)^2` | 3811 ms | 3870 ms — the base is not quadratic, and nothing changes |
| `ln(x)/(x^2 + 1)^2` | 15 034 ms, unevaluated | 51 072 ms, unevaluated |
| `sin(x)/(x^2 + 1)^2` | 26 492 ms, unevaluated | 114 851 ms, unevaluated |
| `e^x/(x^2 + 1)^2` | 7944 ms, unevaluated | 85 683 ms, unevaluated |

That last group is a growth in integration by parts rather than in this rule, which those integrands
never match; it is filed separately rather than fixed here, since bounding by-parts is a change to
machinery every integral goes through.

### A nested radical comes apart

`sqrt(5 + 2*sqrt(6))` is a radical under a radical, and it is two plain ones added.

```
"sqrt(5 + 2 * sqrt(6))".Simplify()   was  sqrt(5 + 2 * sqrt(6))   is  sqrt(3) + sqrt(2)
"sqrt(7 - 4 * sqrt(3))".Simplify()   was  as written              is  2 - sqrt(3)
"sqrt(9 + 4 * sqrt(5))".Simplify()   was  as written              is  sqrt(5) + 2
"sqrt(11 + 6 * sqrt(2))".Simplify()  was  as written              is  3 + sqrt(2)
"sqrt(6 - 2 * sqrt(5))".Simplify()   was  as written              is  sqrt(5) - 1
```

Squaring `sqrt(x) + sqrt(y)` gives `x + y + 2*sqrt(x*y)`, so matching that against
`a + b*sqrt(c)` makes `x` and `y` the roots of `t^2 - a*t + b^2*c/4`. They are rational exactly
when `a^2 - b^2*c` is the square of a rational, and that is the whole test — decidable in exact
arithmetic rather than a search. The sign of `b` chooses the sum or the difference, since squaring
either gives `a + |b|*sqrt(c)`.

**No condition is attached and none is owed.** A non-negative `a` and a non-negative discriminant
are required before anything is built, and together they make the radicand, `x` and `y` all
non-negative — so what fires is an identity between real numbers with no branch chosen. A negative
`a` is refused rather than conditioned.

**A radicand with no rational split is left as written**, `sqrt(1 + sqrt(2))` among them, and so
is anything that is not `a + b*sqrt(c)`.

**Where the denesting is longer, `Simplify` keeps the nested form.** `sqrt(2 + sqrt(3))` does come
apart — to `(sqrt(6) + sqrt(2))/2` — and the nested spelling is the shorter of the two, so that is
what comes back. The rule offers the alternative; the selection is by size, as it is everywhere
else.

One radicand is a boundary rather than a rule: `sqrt(3 + 2*sqrt(2))` is answered by the rule —
applying the set gives `sqrt(2) + 1`, the shorter form — and `Simplify` nonetheless returns the
nested one. There is a test pinning what happens; the cause is not established, and the obvious
suspect was measured and ruled out.

[#717](https://github.com/asc-community/AngouriMath/issues/717).

### A polynomial summand is summed in closed form

A summation wrote itself out term by term where the bounds were concrete and there were fewer
than a hundred terms, and was carried otherwise. So a symbolic bound had no answer, and neither
did a long concrete range — both now do, where the body is a polynomial in the index.

```
"sum(k, k, 1, n)".Simplify()        was  sum(k, k, 1, n)
                                    is   piecewise((n + n ^ 2) / 2 provided n >= 0, 0)

"sum(k, k, 1, 100000)".Simplify()   was  sum(k, k, 1, 100000)
                                    is   5000050000
```

`sum(k^2, k, 1, n)` and `sum(k^3, k, 1, n)` come with it, as does any polynomial summand by
linearity, a coefficient that does not mention the index — `sum(a*k^2 + b*k + c, k, 1, n)` — and a
symbolic *lower* bound, `sum(k, k, m, n)` being `S(n) - S(m - 1)` like any other.

**The condition is the entry.** `sum(k, k, 1, n)` is **not** `(n + n^2)/2` for every `n`. At
`n = -2` the range is empty, and this library answers an empty range with the operator's identity
— `sum(k, k, 5, 1)` is `0`, which has its own test — while the polynomial there is `1`. The
identity holds exactly where `to >= from - 1`, so that is what is attached, with the empty-range
value as the other branch. Where the bounds are concrete the condition is decidable and the whole
thing collapses to a number, which is why the long range above is an integer and not a piecewise.

SymPy prints the bare polynomial for the same input and is not making a mistake: it reads a
reversed range as the negated sum over the flipped one, under which the identity needs no
condition. The condition is what this library's different convention costs, and **code that
expected a bare polynomial from `Simplify` gets a `Piecewise`.**

**A bound that is a number and not a whole one is still carried.** The index runs over the
integers, so `sum(k, k, 1, 5/2)` is `1 + 2`; the polynomial continued to `5/2` is `35/8`, which
answers a different question. `+oo` is refused the same way, so an infinite series is untouched.

**A product is unchanged, whatever its body.** `product(k, k, 1, n)` is still carried, and
`factorial(n)` would be a wrong answer for it rather than a missing one: the empty product is `1`
at every `n < 1`, and `factorial` is undefined at the negative integers. Answering it needs the
same kind of condition the sum now carries, and is not done here. *It is done in the entry below,
which gives it that condition.*

No Bernoulli numbers are involved. The sum of a degree-`d` polynomial is a polynomial of degree
`d + 1`, so `d + 2` of its values determine it, and those values are short sums computed directly;
interpolating them recovers the coefficients exactly in rational arithmetic.

[#717](https://github.com/asc-community/AngouriMath/issues/717).

### A monomial body is multiplied in closed form

The same for `product`, with the narrower reach a product has: a sum of two terms is the sum of
their sums, and a product of two terms is not the product of their products in any way that
helps. What separates is the body that **is** one term.

```
"product(k, k, 1, n)".Simplify()      was  product(k, k, 1, n)
                                      is   piecewise(n! provided n >= 1, 1)

"product(2, k, 1, 500)".Simplify()    was  product(2, k, 1, 500)
                                      is   3273390607896141870013189696827599152216642046043064789483291368096133796404674554883270092325904157150886684127560071009217256545885393053328527589376
```

`product(k^2, k, 1, n)` is `(n!)^2`, `product(2 * k, k, 1, n)` is `n! * 2^n`, and a constant body
needs no factorial at all — `product(c, k, m, n)` is `c^(n - m + 1)`, symbolic lower bound and
all.

**The condition is `to >= from`, where the sum's is `to >= from - 1`,** and the one point between
them is the whole reason. At the empty range the closed form is `c^0`, which is `1` for every `c`
except zero and undefined there, while the empty product is `1` for every `c` including zero.
Giving that point to the identity branch keeps a value from becoming an undefinedness, and costs
nothing, since both branches say `1` there.

**A lower bound that is not a concrete integer of at least one is declined** where the index is in
the body, rather than conditioned. `product(k, k, a, b)` is `b!/(a-1)!` only for `a >= 1`; below
that the range runs through zero so the product is `0`, while `(a-1)!` is undefined. That cannot
share a branch with the empty-range case, because `a < 1` does not make the range empty — a
piecewise reading "identity otherwise" would be wrong there. So `product(k, k, 0, n)` and
`product(k, k, m, n)` stay as written.

**What is not one term is carried**, `product(k + 1, k, 1, n)` included. As for the sum, a bound
that is a number and not a whole one is carried too.

[#717](https://github.com/asc-community/AngouriMath/issues/717).

### A lambda is written with an arrow as well as a call

`a => a + 3` was a parse error and is now the same entity as `lambda(a, a + 3)`. Several
parameters are the curried form the plan in
[#495](https://github.com/asc-community/AngouriMath/issues/495) specifies — `a b => a + b` is
`a => b => a + b`, which is `lambda(a, b, a + b)`.

```
"a => a + 3".ToEntity()                          was  UnhandledParseException
                                                 is   lambda(a, a + 3)
"a b => a + b".ToEntity()                        is   lambda(a, lambda(b, a + b))
"apply(apply(a b => a + b, 1), 2)".Simplify()    is   3
```

**Nothing that parsed before parses differently.** `=` followed by `>` was not a token and not a
parse, so no reading of any valid input has changed; `>=`, `->`, `=` and the rest are untouched,
and there are tests pinning them. The `Lambda` node, beta reduction and currying were all already
there — this is the syntax for them.

**The arrow is read, not printed.** A lambda still prints as `lambda(x, x + 1)`, which is what
keeps the round trip the printed form promises: several spellings may be read, exactly one is
printed.

**Every parameter must be a name**, which is what the plan says. `a 3 => 3` is refused, and so
are `2 => 3` and `x + 1 => 2`. Those raised `UnhandledParseException` before and now raise
`InvalidArgumentParseException` — still invalid, differently named. Code catching the parse
exception by type around input like that will not catch this one.

An index called `i` is the name rather than the imaginary unit, matching `lambda(i, i + 1)`,
which it gets by reading its parameters through the same `Binding` the call form uses
([#976](https://github.com/asc-community/AngouriMath/issues/976)).

**Not in this change:** the rest of that plan's syntax — `f a b` for `apply(apply(f, a), b)`,
`sin x` without brackets, and `sin (x)` with a space. Each of those changes what juxtaposition
means, which is the decision [#286](https://github.com/asc-community/AngouriMath/issues/286) is
about, and none of them is free the way the arrow is.

[#495](https://github.com/asc-community/AngouriMath/issues/495).

### The exponential of a natural logarithm folds

`"e ^ ln(x)".ToEntity().Simplify()` was `e ^ ln(x)` and is `x`; so are `e ^ ln(2 * x)`,
`e ^ ln(x + 1)` and `e ^ ln(sin(x))`. Nothing that had a value changes value.

The identity was not missing. `2 ^ log(2, x)` has simplified to `x` throughout, and the rule that
does it could not reach `e`: `ln(a)` is stored as `log(e, a)`, `e` is a `Constant` rather than a
`Number`, and the pattern binds its base with `Any<Number>`, so the base never matched however the
logarithm was written. That is
[#994](https://github.com/asc-community/AngouriMath/issues/994) — every logarithm carrying a constant
it does not mention — showing up as a missing simplification rather than as a printed one.

Nothing is assumed. `b ^ log(b, a) = a` needs `ln(b)` to be non-zero, and `e` is decidably neither
`0` nor `1`, which is exactly what the numeric arm cannot say about an arbitrary `Number` — so a
symbolic base stays refused, and `1 ^ log(1, x)` is still `NaN` rather than `x`. It holds off the
positive reals, on the principal branch: at `a = -3`, `ln(-3)` is `ln(3) + i*pi` and
`e ^ (ln(3) + i*pi)` is `-3`. At `a = 0` no definedness moves either, since this library reads
`ln(0)` as `-oo` and `e ^ (-oo)` as `0`, so both sides are `0`. It is labelled
`SoundUnderAssumptions` rather than `Sound` because that last part is a branch convention.

**No ODE changes.** The issue was filed believing this cost
[#241](https://github.com/asc-community/AngouriMath/issues/241)'s solver its integrating factor. It
does not: `OrdinaryDifferentialEquation` carries its own `ExponentialOf` helper that already folds
`e ^ ln(u)`, so the eight first-order linear equations measured across this change come back
**byte-identical**. *An earlier version of this entry went one step further and said the rule makes
that helper redundant, with removing it left as its own change. It does not, and the removal was
measured rather than done: the solver asks `InnerSimplified`, which does not carry the rewrite rules,
so `e ^ ln(x)` reaches it unfolded. Deleting the helper fails three `OrdinaryDifferentialEquationTest`
cases, deleting only its `e ^ ln(u)` arm fails two, and calling `Simplify` at the call site instead
still fails one — because nothing in the library folds `e ^ (k ln u)` to `u ^ k`, which is the shape
an antiderivative of `k/x` gives. The helper stays, and its doc comment now records why.*

[#1138](https://github.com/asc-community/AngouriMath/issues/1138).

### Every rule set describes the rules it runs

`RewriteRuleSet.Rules` is what the registry reports a set is made of, and for most of the library's
life it came from `RuleRegistryGenerator` reading the `switch` that defined the set. Twenty-seven of
the thirty sets stopped running that `switch` some releases ago — they run
`MatchedRuleSet.ApplyHere` — and went on describing it. Thirteen of them now describe what they run.

**All twenty-seven of them**, over six changes. Thirteen had **no described arm at all**, so repointing them could only add
metadata: `CollapseMultipleFractions`, the three `CommonDenominator` sets, `DivisionPreparing`,
`ExpandFactorialDivisions`, `ExpandMultipleAngle`, `ExpandTrigonometric`, `Expansion`,
`FactorizeFactorialMultiplications`, `NormalTrigonometricForm`, `PhiFunction` and
`PolynomialLongDivision`. Six more are **one arm to one rule**, so their existing descriptions carry
across unchanged and the rules that had none gain one: `CollapseTrigonometricFunctions`,
`InvertNegativeMultipliers`, `InvertNegativePowers`, `PerfectSquare`, `PolynomialGcdCancellation` and
`SetOperator`. `RationalizeDenominator` was already reading its data form. And `Boolean` is the first
where the identities were **written** rather than carried across: its comments named the laws
(De Morgan, absorption, contraposition) where an identity was wanted, so all twenty were read off the
rules' own patterns and replacements. `NumericNeat` and `Factorization` follow it, their comments
already being identities and needing only the arrow turned into an equals sign.

Nothing is left on the `switch` that does not run it. The three `CanonicalOrder` sets still *run*
theirs, so describing it is not a mismatch and they are not targets.
the `CanonicalOrder` family — still *run* their `switch`, so describing it is not a mismatch.

Four things move, and only the second changes a count:

| | Was | Is |
|---|---|---|
| `Rules[i].Name` | the arm's rendered pattern, `Mulf(var any1, Divf(Integer(1), var any2))` | the rule's name, `reciprocal-factor-becomes-a-quotient` |
| `Rules.Count`, for the two factorial sets | `8` | `3` |
| `Rules[i].Description` | `null` for 38 of these arms and set for 8 | the identity, `a * (1 / b) = a / b`, for all 59 |
| `Rules[i].Soundness` | `null` — an arm declares no tier | the rule's own tier |

**The three counts that change are not rewrites lost.** They are arms the data form writes once.
`Boolean`'s thirty-six become twenty: eight arms of distributivity are two rules, because a
commutative pattern finds the shared operand wherever it sits, and absorption's four-arms-each is one
rule twice. `ExpandFactorialDivisions` and `FactorizeFactorialMultiplications` are eight arms each
written as three, the other five being one rewrite spelled once for each side a factorial can sit on.
`NumericNeat`'s sixteen are eleven, six of them being three rules written once per side a negative
factor can sit on; `Factorization`'s twenty-two are eleven for the same reason, and
`Trigonometric`'s forty-three are thirty-three, `Power`'s thirty-five are thirty-one, and
`Common`'s hundred are sixty-two — the largest collapse, and the same cause: four orientations of a
shared-factor rule are one commutative pattern on each side of the sum. Every other repointed set is
one arm to one rule. Across the registry, 407 becomes **313** while the number of described rules
goes from **95 to 292** — which is every rule the registry now reports.

`Rules[i].Growth` also stops being a guess. `AsAddressable` used to infer it by comparing the lengths
of the two rendered pattern strings — the only thing available to a generator reading source text —
and the exact node count disagrees 23 times in 322. Mostly it corrects a wrong answer; for
`ExpandFactorialDivisions` it replaces one with `Unknown`, which is right, since a quotient of
factorials collects when the offsets are one apart and expands when they are five.

`PatternSource` changes too, from the C# the arm was written in to the pattern the matcher holds —
`Mulf(var a, Divf(1, var b))` for the same rule. Both are source text for reading; neither is
something to match against.

### A step is justified by the rule that fired, not by the set it came from

`RewriteStep.Soundness` read `RuleSet.Soundness` and nothing else, so every rewrite of a set reported
the same tier. A set's tier is the **minimum** over its rules, and one conditional rule is enough to
make a set of a hundred report as conditional: all thirty sets in the registry declare
`SoundUnderAssumptions`, while **181 of the 322 rules written as data are `Sound`** — they hold for
every complex argument, with nothing assumed.

A rewrite now reports its rule's own tier where the rule has one, and its set's where it has not:

```csharp
using var recording = RewriteRecording.Start();
RewriteRules.SetOperator.ApplyOnce(@"A /\ A".ToEntity());
recording.Dispose();
recording.Steps[0].Soundness           // was SoundUnderAssumptions, is Sound
recording.Steps[0].RuleSet.Soundness   // still SoundUnderAssumptions, and correctly so
```

The fallback is not a claim. A rule read off a `switch` declares no tier, so `RewriteRule.Soundness`
is `null` there and the set's tier is what is known — see *Nineteen rule sets describe the rules they
run* above for which sets carry per-rule tiers today.

`DerivationStep.Soundness` changes for the same reason and one more: it is now the weakest tier any
rewrite that **actually fired** inside the step holds at, rather than its set's minimum over rules
the step may never have reached. A pass of nine unconditional rewrites and one conditional one is
still a conditional pass; a pass of ten unconditional ones now says so.

### A negative `arccotan` is negative

`arccotan` here is `arctan(1/x)`, with range `(-pi/2, pi/2]` — **not** the textbook `(0, pi)`. The
inverse-trigonometric table read it as the textbook `pi/2 - arctan(x)`. The two agree on every
positive argument and on nothing negative, so a closed form came back that was not the value:

```csharp
"arccotan(-1)".ToEntity().EvalNumerical()   // -0.7853981633974483…, which is -pi/4
"arccotan(-1)".ToEntity().Simplify()        // was 3/4 * pi, is -1/4 * pi
```

`Simplify` and `EvalNumerical` disagreeing about a constant is the sharpest form this kind of defect
takes, and it reached every table value with a negative argument: `arccotan(-sqrt(3))` was `5/6 * pi`
and is `-1/6 * pi`, `arccotan(-(2 + sqrt(3)))` was `11/12 * pi` and is `-1/12 * pi`.

**The rule for `arctan(x) + arccotan(x)` had the convention right all along** — it answers `pi/2` for
a non-negative argument and `-pi/2` for a negative one, which is
[#887](https://github.com/asc-community/AngouriMath/issues/887). The table's docstring claimed to
take "the same reading of it" and took the opposite one; the comment asserting agreement is what let
the disagreement stand. The two now agree, and `ArccotanTableSignTest` measures the range at a
positive argument, a negative one and zero rather than recalling it.

`arccos` uses the same complement helper and is **unaffected**: its range is `[0, pi]` and `arcsin`'s
is `[-pi/2, pi/2]`, so `pi/2 - arcsin(x)` holds for every argument. That is asserted too, so that a
later tidy-up cannot merge the two paths back together.

### Subtracting an interval turns it round

`Minusf` slid an interval's ends without swapping them, so subtracting one produced an interval whose
left end was above its right — which is empty:

```csharp
"5 - (0; 1)".ToEntity().Simplify()          // was (5; 4), is (4; 5)
"4.5 in (5 - (0; 1))".ToEntity().Simplify() // was False, is True
```

The second line is the one that matters: a wrong answer reached through the operation an interval
exists for.

**The openness is the half that is easy to miss.** `5 - (0; 1]` is `[4; 5)` — the *excluded* 1 becomes
the excluded lower end 4, and the *included* 0 becomes the included upper end 5. Swapping the ends and
leaving the flags where they were would give `(4; 5]`: right about the width and wrong at both ends,
which no test on the printed form alone would catch. `IntervalSubtractionTest` asserts membership as
well for that reason.

An interval *minus* a number was always right and stays right — `(0; 1) - 5` is `(-5; -4)`, and
nothing turns round because nothing is being reflected.

**What this does not fix**, and the boundary is asserted rather than left implicit: `0 - x` is negated
by an earlier arm, so `0 - [0; 1)` is `-[0; 1)` and stops there. Negating an interval is multiplying
one, and `Mulf` has no interval case at all — `(0; 1) * 2` is left alone too. That is
[#322](https://github.com/asc-community/AngouriMath/issues/322)'s remaining half, and it needs a sign
analysis this does not: a negative multiplier turns the interval round exactly as subtraction does,
and a zero one collapses it to a point.

### An interval scaled by a constant is an interval

`Sumf` and `Minusf` had interval cases and `Mulf` and `Divf` had none, so `(0; 1) + 1` answered
`(1; 2)` while `(0; 1) * 2` was handed back. Three rows of `Core/Sets/Arithmetics` recorded that
asymmetry as the expected behaviour, directly beneath the two addition rows that answer.

```csharp
"3 * [2; 3]".ToEntity().Simplify()   // was 3 * [2; 3], is [6; 9]
"[2; 3] / 2".ToEntity().Simplify()   // was [2; 3] / 2, is [1; 3/2]
```

**A negative factor reflects the interval**, so its ends swap and their openness swaps with them —
exactly as subtracting one does:

| | Is |
|---|---|
| `[2; 3) * (-1)` | `(-3; -2]` |
| `(2; 3] / (-1)` | `[-3; -2)` |
| `0 - [0; 1)` | `(-1; 0]` |

The last of those is not a subtraction at all: `0 - x` is negated by an earlier arm, so it reaches
`Mulf` as `-1 * [0; 1)` and is answered there. It is the case the subtraction fix above had to leave
out, and it comes back for free.

**An unknown sign is answered by not answering.** `(0; 1) * k` for a symbolic `k` is one interval
when `k` is positive and the reflected one when it is negative, so picking either would be choosing
which; it is left unevaluated, which is what an unevaluated node means.

Two boundaries, asserted rather than left to be discovered. `(0; 1) * 0` still answers the number `0`
rather than the set `{ 0 }` — that arm is over every `Entity` and not only intervals, and moving it
would change matrices and finite sets with it. And `2 / (0; 1)` is left alone: a constant over an
interval straddling zero is two unbounded pieces rather than one interval, so there is no `Interval`
to answer with.

This is [#322](https://github.com/asc-community/AngouriMath/issues/322)'s arithmetic half. Its body
says it "will be possible once we implement quantifiers"; scaling needs none — it is monotone in the
factor's sign and in nothing else. Applying a non-monotonic function to an interval, which is the
other half, is still open: `ln((0; 1))` is `(-oo; 0)` because `ln` increases, but `sin((0; 7))` needs
to know where the turning points are.

### `Factorize` takes out a common whole factor

`2x + 2a` has been collected under plain `Simplify` for some time — the factorisation rules take out a
factor that appears *identically* in every term. A factor that is only a common **divisor** was not,
and still is not, because `2 * (x + 2 * a)` is a node larger than `4 * a + 2 * x` and
`Entity.SimplifiedRate` will not choose it.

`Factorize` now does, which is
[#195](https://github.com/asc-community/AngouriMath/issues/195)'s "forcefully… but not peacefully":

```csharp
"2 * x + 4 * a".ToEntity().Factorize()          // was 2 * x + 4 * a, is 2 * (x + 2 * a)
"4 * x + 6 * y + 10 * z".ToEntity().Factorize() // is 2 * (2 * x + 3 * y + 5 * z)
"2 * x - 4 * a".ToEntity().Factorize()          // is 2 * (x - 2 * a)
"2 * x + 4 * a".ToEntity().Simplify()           // unchanged: 4 * a + 2 * x
```

**`Simplify` is untouched**, and that is asserted rather than assumed — the peaceful behaviour is
what a caller who did not ask to factorise still gets.

Whole numbers only, and a positive content. `x/2 + a/3` has a common divisor too, but taking `1/6` out
puts a quotient outside the sum rather than a factor, which is a different rewrite. And the sign stays
inside: `-2x - 4a` is `2 * (-x - 2a)` rather than `-2 * (x + 2a)`, because which of those is wanted is
a second question.

A term whose coefficient is not a whole number stops the whole sum rather than contributing a 1: the
content of `2x + a/3` is not 1, it is a thing this does not compute, and answering 1 would say there
was nothing to take.

`Transformation.NumericContentExtraction` is the step on its own, and `Transformation.Factorization`'s
`Name` gains it — a chain names its parts.

### A negation is no longer answered with the empty set

`StatementSolver.Solve` had arms for equality, the connectives, the four comparisons, membership,
`provided` and `piecewise` — and none for `not`, so every negation fell through to `Set.Empty`.
The empty set is a positive claim, *no x satisfies this*, and it was false of all of them
([#1127](https://github.com/asc-community/AngouriMath/issues/1127)). This is the defect
[#1036](https://github.com/asc-community/AngouriMath/issues/1036) fixed for equations, left
standing for negation.

**Was** — every one of these, and 1 is not a solution of the first while 0 is:

```
"not (x = 1)".ToEntity().Solve("x")             {  }
"not (x > 1)".ToEntity().Solve("x")             {  }
"not (x >= 1)".ToEntity().Solve("x")            {  }
"not not (x = 1)".ToEntity().Solve("x")         {  }
"not (x > 1 or x < -1)".ToEntity().Solve("x")   {  }
"not (x in RR)".ToEntity().Solve("x")           {  }
```

**Is** — the negation pushed inward as far as there is an arm for it, and named as a set-builder
where there is not:

```
"not (x = 1)".ToEntity().Solve("x")             { x : not x = 1 }
"not (x > 1)".ToEntity().Solve("x")             (-oo; 1]
"not (x >= 1)".ToEntity().Solve("x")            (-oo; 1)
"not not (x = 1)".ToEntity().Solve("x")         { 1 }
"not (x > 1 or x < -1)".ToEntity().Solve("x")   [-1; 1]
"not (x in RR)".ToEntity().Solve("x")           { x : not x in RR }
```

A negated comparison is answered as the comparison it is, which is what
`RewriteRules.InequalityEquality` already says; a negated connective is pushed inward by De Morgan,
which is the direction that reaches an arm. What neither reaches is answered as written.

### An implication is solved without naming a universe

`Solve` answered `a implies b` with `Codomain \ solve(a) \/ solve(b)`, taking the complement
inside the **statement node's** codomain. That is `Boolean` for every implication, so a numeric
question came back with a solution set containing `True` and `False`
([#996](https://github.com/asc-community/AngouriMath/issues/996)). A `TODO` on the line asked for a
universal set to subtract from instead; neither is needed, because *the values of `x` where `a`
does not hold* is `{ x : not a }`, which names no universe at all.

**Was** — the domain in the answer is the codomain of the `implies` node, not anything the question
was asked over:

```
"(x = 1) implies (x = 2)".ToEntity().Solve("x")     { 2 } \/ BB
"x > 1 implies x > 0".ToEntity().Solve("x")         BB \ (1; +oo) \/ (0; +oo)
"A implies B".ToEntity().Solve("A")                 BB \ { True }
```

**Is** — a set-builder for the antecedent's complement, united with what the consequent settles:

```
"(x = 1) implies (x = 2)".ToEntity().Solve("x")     { x : not x = 1 }
"x > 1 implies x > 0".ToEntity().Solve("x")         { x : not x > 1 } \/ (0; +oo)
"A implies B".ToEntity().Solve("A")                 { A : not A }
```

The boolean row carries the same information it did before — `BB \ { True }` and `{ A : not A }`
are the same set — without asserting that `A` ranges over `BB`. The implication solver is no more
complete than it was: `solve(b, x)` is still empty where `b` does not mention `x`, so
`A implies True` is `{ A : not A }` rather than `BB`, as it was before.

### An unbounded interval over no constraint is left as written

`(-oo; +oo)` simplifies to the domain it is an interval of. Widened to `Domain.Any` there is no such
domain — `Any` is a codomain and not a set — and asking for one threw out of `Solve` on input a
caller can write ([#996](https://github.com/asc-community/AngouriMath/issues/996)).

**Was**

```
"domain((-oo; +oo), Any) = RR".ToEntity().Solve("x")
    NotSufficientlySupportedException: There is no special set for domain Any
```

**Is** — the interval is left alone, and the statement is solved:

```
"domain((-oo; +oo), Any) = RR".ToEntity().Solve("x")    {  }
"domain((-oo; +oo), Any)".ToEntity().Simplify()         domain((-oo; +oo), Any)
"(-oo; +oo)".ToEntity().Simplify()                      RR                        unchanged
"domain((-oo; +oo), CC)".ToEntity().Simplify()          CC                        unchanged
```

### An annotated rational literal keeps the annotation, `CC` included

A codomain is a property of a node rather than a node of its own, so `domain(1/2, CC)` has to become
one `Rational` carrying `Complex` — and it did not. The pass that reads a quotient of two integer
literals as the rational it denotes ([#873](https://github.com/asc-community/AngouriMath/issues/873))
ran over the **finished** tree, by which point `Complex` means two things at once: it is what an
unannotated `Divf` carries by default, and it is what `domain(x, CC)` asks for. The pass read it as
the first and dropped it.

| | before | now |
|---|---|---|
| `"domain(1/2, CC)".ToEntity().Codomain` | `Rational` — the annotation gone | `Complex` |
| `"domain(1/2, CC)".ToEntity() == "1/2".ToEntity()` | `true` | `false` |
| `(1/2).WithCodomain(Complex).Stringize().ToEntity()` | `1/2`, `Rational` — printed one node, read back another | round-trips |
| `"domain(1/2, RR)".ToEntity()` | `Real`, correct | unchanged |
| `"1/2".ToEntity()` | a `Rational`, `Codomain = Rational` | unchanged |
| `"domain(4/2, CC)".ToEntity()` | a `Divf`, `Codomain = Complex` | unchanged |
| `"domain(1/2, CC)".ToEntity().Evaled` | `1/2` — the annotation erased by evaluating | `domain(1/2, CC)`, carried through |
| the same, `.Simplify()` | `1/2` | `domain(1/2, CC)` |
| the same, `.Latexize()` | `\frac{1}{2}` | `{\left(\frac{1}{2}\right)}_{\mathbb{C}}` |
| `"domain(1/2, CC) + 1".ToEntity().Evaled` | `3/2` | unchanged |

**The fix is where the fold happens, not what it reads.** `domain(...)`'s own parser rule now folds
its argument before annotating it, so the two meanings of `Complex` never meet: an annotation lands
on a node that is already the right shape, and the sweep over the rest of the tree — which is the
only other place the fold runs — can hand back a bare `Rational` and read no codomain at all. It is
sound to do so because those two are the only routes, `domain(...)` being the one syntax that
annotates anything and the sweep meeting only what it did not annotate.

`Complex` on a rational literal is a **widening** — a rational is a complex number — so nothing that
was defined becomes `NaN` and no value changes: `domain(1/2, CC) + 1` evaluates to `3/2` as before,
and `domain(1/2, ZZ)` is still `NaN`. What changes is equality, and everything that follows from the
node genuinely carrying the annotation now — `Evaled` and `Simplify` hand it back instead of erasing
it, and `Latexize` writes the subscript. That is the point: an annotation the caller wrote is no
longer indistinguishable from one they did not.

This was the second of the two gaps
[#1048](https://github.com/asc-community/AngouriMath/issues/1048) recorded, and the last entry in
`CodomainSurvivesPrintingTest.StillUnparseable`. That dictionary is now empty and still asserted in
both directions, so every writable domain on every node type reads back as itself, and a gap added
to it later fails the day it closes.

### An interval bounded by its own element is decided, and `Common` terminates

`a in (a / 2; 2 * a)` was `a > 0` and `a in (a / 3; 3 * a)` was left as written. The difference was
not the mathematics — it was which candidate survived `Simplify`'s pruning.

`ParaphraseInterval` writes a membership out as two comparisons with zero, and the difference it
compares was left as a two-term sum: `a - a / 3` came back as `a + -a / 3`, which no rule about a
sign can read. It is collected now, and the positive factor is divided out where the comparison is
built rather than later — because `Simplify` prunes by `SimplifiedRate`, and
`2/3 * a > 0 and 2 * a > 0` rates **26** against the membership's **25**, one point worse, so it was
discarded before anything could take it to `a > 0`, which rates **8**. The `n = 2` case answered
only because `1/2 * a > 0 and a > 0` happens to rate **24**.

| | before | now |
|---|---|---|
| `"a in (a / 2; 2a)".Simplify()` | `a > 0` | unchanged |
| `"a in (a / 3; 3a)".Simplify()` | `a in (a / 3; 3 * a)` | `a > 0` |
| `"a in (a / 4; 4a)".Simplify()`, and every denominator through 8 | left as written | `a > 0` |
| `"a in (a / 2; 3a)".Simplify()` | `a in (a / 2; 3 * a)` | `a > 0` |
| `"a in (a / 7; 5a)".Simplify()` | left as written | `a > 0` |
| `"a in (a / 2; 0)".Simplify()` | `a in (a / 2; 0)` | `False provided a in RR` |
| `"a in (-2a; -a/2)".Simplify()` | left as written | `False provided a in RR` |
| `"a in (a / 2; 2a + 1)".Simplify()` | `a > 0 and 1 + a > 0` | unchanged |
| `"a in (1; 2)".Simplify()`, `"3 in (1; 5)"`, `"x in [0; 1]"`, `"a in (b; c)"` | unchanged | unchanged |

The two `False` rows are answers where there were none: `a / 2 < a < 0` wants `a > 0` and `a < 0` at
once, and so does `-2a < a < -a / 2`. The condition is there because the ordering is a claim about
reals.

**And `RewriteRules.Common` reaches a fixed point.** It was the library's only non-terminating rule
set: a three-cycle on `-x * 1/2`, `Mulf(-1/2, x)` to `Mulf(-1, Divf(x, 2))` to `Divf(Mulf(-1, x), 2)`
and back — three trees printing as two strings. Two of the three rules turning it are exact inverses
on that shape: one reads `(-1 * x) / 2` as a numeric factor to collect, giving `-1/2 * x`, and the
other reads that back as `-(x / 2)`. The first now declines a factor of `-1`, which is the sign
rather than a number to collect.

The positive case never cycled, and the reason says why the guard is where it is: `x / 2` is a
quotient over a *leaf*, so it does not re-enter the collection rule's pattern at all. The loop
existed only because a negation is spelled as a product.

`Simplify` bounded its own iteration and never hung, so no caller saw the cycle — but a rule set is
public, a caller may apply one by itself, and `Common` did not terminate when applied that way.
`RuleSetTerminationTest.NeverSettle` is now empty and still asserted in both directions.

**Nothing else moved.** `-x * 1/2` is `-1/2 * x` and `x * 1/2` is `x / 2`, both as before; the guard
changes which rewrites are available, not which answer wins. The two halves of this entry ship
together because the first is what makes the second free: the guard alone cost three interval shapes
that were being answered by coincidence, and the collection restores them along with the rest of the
family ([#1056](https://github.com/asc-community/AngouriMath/issues/1056)).

### The determinant is computed by fraction-free elimination where it can be

`Matrix.Determinant` expanded by Laplace, which is `O(n!)`. For a fully symbolic matrix that is
optimal — the determinant genuinely has `n!` terms, and no algorithm returns it smaller in expanded
form. For a numeric one it is pure waste: the answer is a single number and `O(n^3)` work suffices.

Bareiss' fraction-free elimination now runs wherever the entries are polynomials over the rationals,
and Laplace answers everything else. What decides it is not the size but whether the entries can be
read, settled per matrix by trying.

**The ceiling this removes**, both arms built from source on one machine:

| | before | now |
|---|---|---|
| numeric 8×8 | 382 ms | under 1 ms |
| numeric 10×10 | 14 415 ms | 2 ms |
| numeric 11×11 | did not return in four minutes | 2 ms |
| numeric 12×12 | did not return | 3 ms |
| numeric 20×20 | did not return | 11 ms |
| numeric 30×30 | did not return | 22 ms |

**The printed form of a symbolic determinant changes**, because an elimination produces an expanded
polynomial where Laplace produces a nested expansion. The value is the same and the expression is no
larger in any case measured:

| | before | now |
|---|---|---|
| `"[[a, b], [c, d]]"` | `a * d + -b * c` | `a * d - b * c` |
| `"[[x, 1, 0], [1, x, 1], [0, 1, x]]"` | `x * (x ^ 2 + -1) + -x` | `x ^ 3 - 2 * x` |
| `"[[a, b, 1], [c, d, 2], [1, 2, 3]]"` | `a * (d * 3 + -4) + -b * (c * 3 + -2) + c * 2 + -d` | `3 * a * d - 4 * a - 3 * b * c + 2 * b + 2 * c - d` |
| `"[[x, 1], [1, x]]"` | `x ^ 2 + -1` | unchanged |
| `"[[1/2, 1/3], [1/4, 1/5]]"` | `1/60` | unchanged |

**No condition is introduced, and that is the point.** An ordinary Gaussian elimination leaves its
pivots as literal divisions, so its answer is undefined wherever a pivot vanishes — at points where
the determinant is perfectly well defined. That was
[#992](https://github.com/asc-community/AngouriMath/issues/992), and it is why Laplace was chosen.
Bareiss divides as well, but each division is by the *previous* pivot and is exact: the quotient is a
determinant of a minor, so it is back in the ring. Here it is exact **and checked** — the arithmetic
happens in `MultivariatePolynomial`, which has no quotients to leave behind, and a division that does
not come out returns null and sends the caller to Laplace.

**What is declined**, and answered by Laplace exactly as before: an entry that is not a polynomial
over the rationals (`sin(x)`, `1 / x`, `2 ^ x`), a matrix in more than eight indeterminates, and a
matrix mentioning `e` or `pi` — a constant is a value rather than an indeterminate, and this ring
cannot hold one.

The two algorithms were compared on 300 generated matrices where both apply, as a difference
simplified to zero rather than as trees, with **no disagreements**
([#999](https://github.com/asc-community/AngouriMath/issues/999)).
### Solving a system is bounded whichever internal path takes it

`Solve` on a system tries a triangularising path first, which bounds itself, and hands what it
declines to an elimination in radicals, which had **no budget at all**. So the same call was bounded
or unbounded depending on which internal path accepted it — cyclic-6 exceeded twenty seconds with
nothing to stop it — and that is worse to debug than either extreme.

The whole call now draws on one budget. Where it runs out, `NotSufficientlySupportedException` is
raised, naming both paths and both ways to ask for more.

| | before | now |
|---|---|---|
| `solvesys[a,b,c,d,f,g]` cyclic-6 | exceeded 20 s, no bound beyond it | declines, returning in about two minutes |
| `solvesys[a,b,c,d,e]` of `a^4-1 … e^4-1` | 1024 solutions | unchanged |
| `solvesys[a,b,c,d]` cyclic-4 | 158 solutions | unchanged |
| `x + y = 3, x - y = 1` and every system that answered | unchanged | unchanged |
| the same with `MathS.Settings.Budget` set below what the solve needs | answered anyway | raises |

**Nothing that answered stops answering.** That was the risk, and it was measured before the change
rather than argued about: the 1024-solution system is one the triangularising path declines on its
quotient-dimension cap, and it declines it **in 8 milliseconds** — so bounding the whole call leaves
the elimination essentially the whole budget. "The fast path declined" does not mean "hopeless", and
this is what keeps that true.

**The default is two ceilings, and both are measured.** A step is one candidate solution the
elimination explores, which is what compounds — each elimination turns the next level's coefficients
into nested radicals. The systems that answer explore very few: a symbolic 2×2 takes 2, cyclic-4
takes 8, and the largest that answers at all takes 341. Cyclic-5 passes 100 000 without finishing. So
the step ceiling is 10 000, with thirty-fold headroom over anything known to work.

The clock is a backstop rather than the bound, and it is deliberately loose at sixty seconds, because
a step can be arbitrarily expensive — cyclic-4 spends five seconds in eight of them. A tight clock
was tried at five seconds and makes the same system answer or decline depending on what else the
machine is doing, which is a worse failure than a slow answer.

**The bound is cooperative and is checked once per branch**, so a call can overshoot by the cost of
the branch that was running when the budget ran out. That is `BudgetLedger`'s stated design: an
algorithm that does not ask cannot be bounded, and a bound enforced from outside is a thread abort in
the middle of a rewrite. It is a bound where there was none, not a tight one.

**A budget recording now sees two outcomes per solve** rather than one, named `Gröbner` and
`SolveSystem`. What stopped each is reported separately, which is the thing
[#896](https://github.com/asc-community/AngouriMath/issues/896) said a caller could not previously
find out.

## 2.4.0 — since 2.3.0

Released 2026-08-28. These entries sat under “Unreleased” while 2.4.0 was tagged and
published, so a reader on that version could not tell from this file what they had. They
are under their own heading now, split at the tag rather than from memory: an entry is
here if it was in this file at `v2.4.0` and above if it was added after.

### At a glance

| Silent? | What | Was | Is |
|---|---|---|---|
| **Silent** | `"limit(t * b, t, 0)".ToEntity().FreeVariables`, and every limit | `{ t, b }` — the variable it approaches along counted as free | `{ b }` |
| **Silent** | `MathS.Polynomials.Factor("4 * x2 - 4 * y2", "x")`, and every multivariate polynomial whose content is a bare constant | `(x + y) * (x - y)` — **not equal to what it factored** | `4 * (x + y) * (x - y)` |
| | `"2 * x3 - 2".ToEntity().Factorize()`, and every polynomial whose content the rules take out | `2 * (x ^ 3 - 1)` — the remainder left whole | `2 * (x - 1) * (x ^ 2 + x + 1)` |
| | `"3^(x+1) - 2^(x-1)".ToEntity().SolveEquation("x")`, and every equation between two powers of numeric bases | `{ ln(0.04674569822628630438865471319331845734268426895141601562 ^ (1 / ln(2))) }` — a `double` promoted to a decimal | `{ -(ln(3) + ln(2)) / (ln(3) + -ln(2)) }` |
| **Silent** | `MathS.Abs("x").WithCodomain(Domain.Any).Stringize()`, and every node widened to `Any` from a narrower default | `abs(x)` — reads back as `Real`, losing the widening | `domain(abs(x), Any)` |
| | `new Entity[0].SumAll()`, and `Sumf.Sum` on an empty list | `AngouriBugException: At least 1 child required` | `0` |
| | `new Entity[0].MultiplyAll()`, and `Mulf.Multiply` on an empty list | `AngouriBugException` | `1` |
| | `MathS.Vector()` and `new Entity[0].ToVector()` | `IndexOutOfRangeException` — outside the documented hierarchy | `InvalidMatrixOperationException` |
| | `"x3 - 1".ToEntity().Factorize()`, and every polynomial no rewrite rule has a rule for | `x ^ 3 - 1` — handed back whole | `(x - 1) * (x ^ 2 + x + 1)` |
| | `Entity.DomainConditionIn(Domain)` | did not exist | the domain of definition for a **stated** reading, through the whole tree |
| | `MathS.Polynomials.Factor("(y * x3 + 1) * (x4 - y3)", "x")`, and bivariate polynomials whose leading coefficient in the main variable is a polynomial | `null` — a refusal | `(x ^ 4 - y ^ 3) * (x ^ 3 * y + 1)` |
| | `MathS.Polynomials.Factor("x7 - y7", "x")`, and bivariate polynomials whose substituted image over-factors | `null` — a refusal | `(x - y) * (x ^ 6 + x ^ 5 * y + … + y ^ 6)` |
| | `MathS.Polynomials.Factor("x2 + y2 + z2 + w2 + 1", "x")`, and polynomials in enough variables generally | `null` — a refusal | the polynomial itself, meaning it does not factor |
| **Silent** | `"x! = 0".ToEntity().Simplify()` | `False`, including at `x = -1` where `x!` has a pole and the statement is `NaN` | `False provided x in RR and (x >= 0 or not x in ZZ)` |
| **Silent** | `"x! / x!".ToEntity().Simplify()`, and the same for any factorial over itself | `1`, including at `x = -1` where the quotient is `NaN` | `1 provided not x! = 0` |
| **Silent** | `"(y < x) or (x = y)".ToEntity().Simplify()`, and three more disjunctions of a comparison with an equality written the other way round | `x <= y` — False at `x = 3, y = 2` where the input is True | `x >= y` |
| **Silent** | `"x6 + x y + 1 = 0".ToEntity().Solve("x")`, and every equation no solver settles | `{  }` — there are no roots | `{ x : 1 + x ^ 6 + x * y = 0 }` — these are the roots, whichever they are |
| **Silent** | `"(x - 1) * (x6 + x y + 1) = 0".ToEntity().Solve("x")` | `{ 1 }` | `{ 1 } \/ { x : 1 + x ^ 6 + x * y = 0 }` |
| **Silent** | `"x6 + x y + 1 = 0 and x - 1 = 0".ToEntity().Solve("x")` | `{  }` | `{ x : x ^ 6 + x * y + 1 = 0 and x - 1 = 0 }` |
| **Silent** | `"sum(k, k, 1, n)".ToEntity().FreeVariables`, and `product` | `{ k, n }` — the bound index counted as free | `{ n }` |
| **Silent** | `"integral(t * b, t, 0, 1)".ToEntity().FreeVariables`, and every integral with limits | `{ b, t }` | `{ b }` |
| | `"x ^ 3 - x > 0".ToEntity().Solve("x")`, and every polynomial inequality of degree three or more | `NotSufficientlySupportedException: Only linear and quadratic polynomial inequalities are supported` | `(-1; 0) \/ (1; +oo)` — the solution set |
| **Silent** | `"a implies (b implies c)".ToEntity().Stringize()` | `a implies b implies c`, which reads back as `(a implies b) implies c` | `a implies (b implies c)` |
| | `"(a implies b) implies c".ToEntity().Stringize()` | `(a implies b) implies c` | `a implies b implies c` |
| **Silent** | `@"A \ (B \ C)".ToEntity().Stringize()`, and `Latexize` | `A \ B \ C` / `A \setminus B \setminus C` | `A \ (B \ C)` / `A \setminus \left(B \setminus C\right)` |
| **Silent** | `@"A \ (B \/ C)".ToEntity().Stringize()`, and `Latexize` | `A \ B \/ C` / `A \setminus B \cup C` | `A \ (B \/ C)` / `A \setminus \left(B \cup C\right)` |
| **Silent** | `@"A \/ (B \ C)".ToEntity().Stringize()`, and `Latexize` | `A \/ B \ C` / `A \cup B \setminus C` | `A \/ (B \ C)` / `A \cup \left(B \setminus C\right)` |
| **Silent** | `"(x provided p) provided q".ToEntity().Stringize()`, and `Latexize` | `x provided p provided q`, which reads back as `x provided (p provided q)` | `(x provided p) provided q` |
| **Silent** | `"a in (b in c)".ToEntity().Stringize()`, and `Latexize` | `a in b in c` / `a \in b \in c` | `a in (b in c)` / `a \in \left(b \in c\right)` |
| **Silent** | `"x * (y mod z)".ToEntity().Stringize()`, and `Latexize` | `x * y mod z` / `x y \bmod z` | `x * (y mod z)` / `x \left(y \bmod z\right)` |
| **Silent** | `"-1 * (y mod z)".ToEntity().Stringize()` | `-y mod z` | `-(y mod z)` |
| | any expression mixing a number with a `Complex` argument, `Compile`d in a NativeAOT app — `"x + 1".Compile<Complex, Complex>("x")` | `UncompilableNodeException: ... The binary operator Add is not defined for the types 'System.Numerics.Complex' and 'System.Numerics.Complex'` | the compiled function, answering as it does under the JIT |
| | `Compile` to a nullable integral return type in a NativeAOT app | `AngouriBugException: IsNaN method expected for type System.Double`, which took the process down | the compiled function |
| **Silent** | an app publishing with `PublishTrimmed` or NativeAOT | `AngouriMath.dll` was copied in whole, being unmarked | it is trimmed with the rest, since the assembly now declares `IsTrimmable` |
| **Silent** | `"domain(x, ZZ)".ToEntity().Stringize()`, and `ToString`, and `EntityJsonConverter` | `x`, which reads back with `Codomain = Any` | `domain(x, ZZ)`, which reads back narrowed |
| **Silent** | `"domain(sqrt(-1), RR)".ToEntity().Stringize()` | `sqrt(-1)`, which evaluates to `i` when read back | `domain(sqrt(-1), RR)`, which evaluates to `NaN` |
| **Silent** | `"domain(x, ZZ)".ToEntity().Latexize()` | `x` | `{\left(x\right)}_{\mathbb{Z}}` |
| **Silent** | `"x - domain(x, ZZ)".ToEntity().Simplify()`, and any sum mixing a node with a narrowed codomain and the same node without | `0` — the two were collected as one monomial | `x - domain(x, ZZ)`, left alone |
| | every node type's own `Stringize()` and `Latexize()` overrides — `Entity.Sumf.Stringize()` and 129 more | declared on each node | declared once on `Entity`; still callable on every node, and an assembly compiled against 2.3.0 keeps working without a rebuild |
| | `"x + 1 // done".ToEntity()`, and any input whose last line ends in a `//` comment | `UnhandledParseException: extraneous input '/'` | `x + 1` — the comment is skipped, as the block form already was |
| | `MathS.Polynomials.Factor("x * y + y", "x")`, and any polynomial whose coefficients in the named variable share a common divisor | `null` — a refusal | `y * (x + 1)` |
| | `MathS.Polynomials.SquareFreePart("(x - y) ^ 2 * (x + y)", "x")`, and any polynomial in more than one variable | `null` — a refusal | `x ^ 2 - y ^ 2` |
| | `MathS.Polynomials.Factor("x ^ 2 - y ^ 2", "x")`, and polynomials in two variables of small enough bidegree | `null` — a refusal | `(x + y) * (x - y)` |
| | a `switch` over `RewriteRuleGrowth` with no default arm | compiled | does not compile — there is a fourth value, `Unknown` |
| **Silent** | `RewriteRules.RationalizeDenominator.Rules` | `[]` — the registry could not read the set | its two rules, addressable and named |
| **Silent** | `RewriteRules.Power.ApplyOnce("ln(1 / x)")`, and every `log(_, 1/_)` and `log(1/_, _)` whose argument is not decidably a positive real | `-ln(x)`, which is wrong on the negative reals | `ln(1 / x)`, left alone |
| **Silent** | `RewriteRules.Boolean.ApplyOnce("a and b or a")`, and two more orientations of absorption | left alone — the arm for that orientation was never written | `a` |

### An equation nothing settled is no longer answered with the empty set

`Solve` and `SolveEquation` returned an empty `FiniteSet` for two different things: an equation
shown to have no roots, and an equation every solver in the chain declined. The empty set is a
positive claim — *no x satisfies this* — so the second of those was a wrong answer rather than a
graceful failure ([#1036](https://github.com/asc-community/AngouriMath/issues/1036),
[#746](https://github.com/asc-community/AngouriMath/issues/746) tier 4).

**Was** — indistinguishable, and false for the second column:

```
"x6 + x y + 1 = 0".ToEntity().Solve("x")            {  }        six roots for every y
"sin(x) + x + y = 0".ToEntity().Solve("x")          {  }
"e^x + x + y = 0".ToEntity().Solve("x")             {  }
"x6 + x y + 1".ToEntity().SolveEquation("x")        {  }
"e^x = 0".ToEntity().Solve("x")                     {  }        genuinely none
"abs(x) = -1".ToEntity().Solve("x")                 {  }        genuinely none
```

**Is** — the equation itself, as the set of the x that satisfy it. It names the same set and
asserts of it only what was established:

```
"x6 + x y + 1 = 0".ToEntity().Solve("x")            { x : 1 + x ^ 6 + x * y = 0 }
"sin(x) + x + y = 0".ToEntity().Solve("x")          { x : sin(x) + x = -y }
"e^x + x + y = 0".ToEntity().Solve("x")             { x : e ^ x + x = -y }
"x6 + x y + 1".ToEntity().SolveEquation("x")        { x : 1 + x ^ 6 + x * y = 0 }
"e^x = 0".ToEntity().Solve("x")                     {  }        unchanged
"abs(x) = -1".ToEntity().Solve("x")                 {  }        unchanged
```

Turning Newton's method off leaves nothing numerical either, and those answers move the same way.
A search over finitely many starting points inside a bounded region finding nothing is a fact about
the search:

```
using var _ = MathS.Settings.AllowNewton.Set(false);

"x5 + 3x + 1 = 0".ToEntity().Solve("x")             was {  }   is { x : x ^ 5 + 3 * x = -1 }
"sin(x) * x - 3 = 0".ToEntity().Solve("x")          was {  }   is { x : sin(x) * x = 3 }

"x + sqrt(x^0.1 + a) + c".ToEntity().SolveEquation("x")
    was {  }   is { x : sqrt(a + x ^ (1/10)) + c + x = 0 }
"(x + 6)^(1/6) + x + x3 + a".ToEntity().SolveEquation("x")
    was {  }   is { x : (6 + x) ^ (1/6) + x + x ^ 3 = -a }
"2 ^ (x sin(x)) + 4 ^ (x sin(x)) + c".ToEntity().SolveEquation("x")
    was {  }
    is  { x : sin(x) * x = ln(((-1 - sqrt(1 - 4 * c)) / 2) ^ (1 / ln(2)))
              or sin(x) * x = ln(((-1 + sqrt(1 - 4 * c)) / 2) ^ (1 / ln(2))) }
```

The last of those is the shape of it: the exponential solver did settle the equation in
`2 ^ (x sin(x))`, and it was the inversion of `x * sin(x)` that had nothing to say. What comes back
now is the half that was solved, with the half that was not left standing as a condition.

An answer that is partly settled keeps the part that is, so a product one of whose factors was
solved comes back as a union rather than as the factor's roots alone:

```
"(x - 1) * (x6 + x y + 1) = 0".ToEntity().Solve("x")
    was  { 1 }
    is   { 1 } \/ { x : 1 + x ^ 6 + x * y = 0 }

"x6 + x y + 1 = 0 or x - 1 = 0".ToEntity().Solve("x")
    was  { 1 }
    is   { 1 } \/ { x : 1 + x ^ 6 + x * y = 0 }

"x6 + x y + 1 = 0 implies x - 1 = 0".ToEntity().Solve("x")
    was  { 1 } \/ BB
    is   { 1 } \/ (BB \ { x : 1 + x ^ 6 + x * y = 0 })
```

A conjunction with an unsettled side is answered as the conjunction. Intersecting a finite set with
a condition keeps the elements whose membership could not be decided, so taking the intersection
here would have replaced one false claim with another — `{ 1 }` says 1 solves
`x^6 + x*y + 1 = 0`, and it does so only at `y = -2`:

```
"x6 + x y + 1 = 0 and x - 1 = 0".ToEntity().Solve("x")
    was  {  }
    is   { x : x ^ 6 + x * y + 1 = 0 and x - 1 = 0 }
```

**What this means for callers.** `Solve` has always been typed `Set` and has always been able to
return an `Interval`, a `ConditionalSet` or a union; what changes is how often it does. Code that
casts the result to `FiniteSet`, or that reads an empty result as "no solutions", has to say which
of the two it means:

```csharp
var answer = equation.Solve(x);
if (answer is FiniteSet roots)      { /* these are all of them */ }
else if (answer.IsSetEmpty)         { /* shown to have none */ }
else                                { /* not settled, or not finite */ }
```

Nothing that solved before stops solving, and no equation that was shown to have no roots gains
any. Solving `x2 - 4 = 0`, `sin(x) = 1/2`, `x2 + 1 = 0` and the system solver's answers are
unchanged, as is `Solve` on an inequality.

### A bound index is not a free variable

`FreeVariables` knew about two binders — `Lambda` and the set builder — and about no others. A
summation and a product bind their index, so `sum(k, k, 1, n)` is a function of `n` alone, and a
definite integral binds its variable between its limits. Both reported the bound name as free.

Measured on a build of 2.3.0 and a build of this branch:

| input | 2.3.0 | now |
|---|---|---|
| `sum(k, k, 1, n)` | `{ k, n }` | `{ n }` |
| `product(k, k, 1, n)` | `{ k, n }` | `{ n }` |
| `integral(t * b, t, 0, 1)` | `{ b, t }` | `{ b }` |
| `integral(t * b, t)` | `{ b, t }` | `{ b, t }` |
| `derivative(t * b, t)` | `{ b, t }` | `{ b, t }` |
| `lambda(x, x + y)` | `{ y }` | `{ y }` |
| `{ k : k > a }` | `{ a }` | `{ a }` |

The index is bound over the **bounds** as well as the body, which is what
[`Binding`](Sources/AngouriMath/Core/Binding.cs) already says of itself: the name a binder is handed
is honoured throughout it, through the summand and the bounds. So `sum(k, k, k, n)` is `{ n }` too.

**The last three rows are unchanged on purpose, and the distinction is the interesting part.** An
indefinite integral does not bind: the antiderivative of `t * b` over `t` is `b * t ^ 2 / 2 + C`,
which is still a function of `t`. Neither does a derivative — `d/dt` denotes a function of `t`. They
look like the same shape as a summation and are not, and a later sweep that completes the binder list
by adding them would make them wrong. `FreeVariablesTest` pins all of it, including the two that must
stay put.

**`Vars` and `VarsAndConsts` do not change.** They mean every name *occurring*, bound ones included —
`"sum(k, k, 1, n)".ToEntity().Vars` is still `{ k, n }` — which is what their own XML example has
always documented, listing a lambda's parameter under *variables and constants*. Occurring and free
are different questions and the three properties answer them separately.

[#1019](https://github.com/asc-community/AngouriMath/issues/1019).

### A polynomial inequality of degree three or more is answered

**Was** — every univariate polynomial inequality above degree two was refused outright, whatever its
coefficients and however easily it factored:

```
"x ^ 3 - x > 0".ToEntity().Solve("x")
    NotSufficientlySupportedException: Only linear and quadratic polynomial inequalities are
    supported; this one is of a higher degree
```

**Is** — the solution set, where the real roots can be established completely:

```
"x ^ 3 - x > 0".ToEntity().Solve("x")                   (-1; 0) \/ (1; +oo)
"x ^ 3 - x >= 0".ToEntity().Solve("x")                  { 0, 1, -1 } \/ (-1; 0) \/ (1; +oo)
"(x - 1) ^ 2 * (x + 2) > 0".ToEntity().Solve("x")       (-2; 1) \/ (1; +oo)
"x ^ 4 - 5 * x ^ 2 + 4 > 0".ToEntity().Solve("x")       (-oo; -2) \/ (-1; 1) \/ (2; +oo)
"x ^ 3 - 2 * x + 1 > 0".ToEntity().Solve("x")           ((-1 - sqrt(5)) / 2; (-1 + sqrt(5)) / 2) \/ (1; +oo)
"x ^ 3 - 2 > 0".ToEntity().Solve("x")                   (2 ^ (1/3); +oo)
"x ^ 5 - 5 * x ^ 3 + 4 * x > 0".ToEntity().Solve("x")   (-2; -1) \/ (0; 1) \/ (2; +oo)
"x ^ 4 + 1 > 0".ToEntity().Solve("x")                   (-oo; +oo)
"x ^ 4 - 2 > 0".ToEntity().Solve("x")                   (-oo; -2 ^ (1/4)) \/ (2 ^ (1/4); +oo)
"x ^ 4 - 10 * x ^ 2 + 1 > 0".ToEntity().Solve("x")      (-oo; -sqrt((10 + 4 * sqrt(6)) / 2))
                                                          \/ (-sqrt((10 - 4 * sqrt(6)) / 2); sqrt((10 - 4 * sqrt(6)) / 2))
                                                          \/ (sqrt((10 + 4 * sqrt(6)) / 2); +oo)
```

A polynomial has one sign on each open interval between consecutive real roots, so the answer is the
union of the intervals where that sign is positive. What makes it an answer rather than a guess is
that the list of real roots is *complete*: the polynomial is written as a product of powers of
irreducibles over `Q`, verified to multiply back, and the number of real roots of each irreducible
factor is read off its **discriminant** — two where a quadratic factor's is positive and none where
it is negative, three and one respectively for a cubic, and — for a quartic — the discriminant
with the two auxiliary quantities of the standard criterion, a negative discriminant meaning two
real roots and a positive one four or none. A missed root would merge two intervals of
opposite sign and report the wrong half of one as the solution, so this is the difference between
the feature and a wrong answer.

**And the refusal that remains is a different one.** The gap is no longer degree; it is an
irreducible factor of degree five or more, where there is no criterion for the number of real roots
and no formula for the roots either. So the message changed too:

```
"x ^ 5 - x - 1 > 0".ToEntity().Solve("x")
    NotSufficientlySupportedException: Only polynomial inequalities are supported, and of those
    only the ones whose real roots can be established completely: linear and quadratic with any
    coefficients, and higher degrees where the coefficients are rational and no irreducible factor
    is of degree five or more
```

Code that caught `NotSufficientlySupportedException` still catches it; code that matched on the
message text does not. Linear and quadratic inequalities are untouched, including the symbolic
coefficients and the case splits on their signs, which the sign table does not do — it takes only
rational coefficients, and only from degree three up.

[#746](https://github.com/asc-community/AngouriMath/issues/746) item 43.

### A printed operator that is not associative keeps its brackets

`Stringize` is the library's own input format: parsing what it prints has to give back the
expression it printed, and that is what [`StringizeRoundTripTest`](Sources/Tests/UnitTests/Convenience/StringizeRoundTripTest.cs)
enforces. Six operators broke it, five of them in the same way — the printer left the **right**
operand unbracketed at its own precedence level, while the grammar folds that level to the left, so
the printed text came back re-associated. The sixth, `provided`, is the mirror: it is the one infix
operator the grammar folds to the **right**, so there it is the *left* operand that mis-associates.

Four of the six are not associative, so the re-association changed the answer and not merely
the shape. Measured on a build of 2.3.0 and a build of this branch:

| input | 2.3.0 printed | 2.3.0 read that back as | value moved |
|---|---|---|---|
| `false implies (true implies false)` | `False implies True implies False` | `(False implies True) implies False` | `True` → `False` |
| `{ 1, 2, 3 } \ ({ 2, 3 } \ { 3 })` | `{ 1, 2, 3 } \ { 2, 3 } \ { 3 }` | `({ 1, 2, 3 } \ { 2, 3 }) \ { 3 }` | `{ 1, 3 }` → `{ 1 }` |
| `{ 1, 2 } \/ ({ 3 } \ { 1, 2 })` | `{ 1, 2 } \/ { 3 } \ { 1, 2 }` | `({ 1, 2 } \/ { 3 }) \ { 1, 2 }` | `{ 1, 2, 3 }` → `{ 3 }` |
| `{ 1, 2 } \ ({ 2 } \/ { 3 })` | `{ 1, 2 } \ { 2 } \/ { 3 }` | `({ 1, 2 } \ { 2 }) \/ { 3 }` | `{ 1 }` → `{ 1, 3 }` |
| `2 * (3 mod 2)` | `2 * 3 mod 2` | `(2 * 3) mod 2` | `2` → `0` |

On this branch each of those prints its brackets and reads back as itself, so the value column
no longer moves.

`implies` also changes in the other direction, because its printer had the rule the wrong way
round rather than missing: it bracketed the **assumption** — the operand that never needs it under
a left fold — and not the conclusion. So `(a implies b) implies c`, which used to print as
`(a implies b) implies c`, now prints flat as `a implies b implies c`. Both read back as the same
expression; the brackets were redundant, and printing them is what made the genuinely ambiguous
case look no different.

`\/` and `mod` are bracketed only against the operator that makes them ambiguous, because both
share a precedence level with an operator that is not associative while being associative
themselves:

- `A \/ (B \/ C)` still prints `A \/ B \/ C`; `A \/ (B \ C)` now prints `A \/ (B \ C)`.
- `x * (y * z)` still prints `x * y * z` and `x * (y / z)` still prints `x * y / z`;
  `x * (y mod z)` now prints `x * (y mod z)`.
- `-1 * (y mod z)` printed `-y mod z` and now prints `-(y mod z)`.

`provided` is bracketed on the other side, and it is the case where the value is *not* the test.
`(x provided p) provided q` and `x provided (p provided q)` are both `x` exactly when `p` and `q`
hold, so no answer moves — and they are different expressions, which is what the round trip is
about. It printed flat and read back as the right-nested one.

**What has not changed, deliberately.** An operator that *is* associative still prints flat, so
`x + (y + z)` still prints `x + y + z`, and likewise for `*`, `and`, `or`, `xor`, `unite` and
`intersect`. Those come back as a different `Entity` — a left-nested tree instead of
a right-nested one — and as the same number, because the bracketing carries no mathematics. The
alternative would print every expanded polynomial as a right-nested pile of parentheses:
`"(a + b + c + d) ^ 2".ToEntity().Expand()` prints, on both versions,
`d ^ 2 + 2 * c * d + c ^ 2 + 2 * b * d + 2 * b * c + b ^ 2 + 2 * a * d + 2 * a * c + 2 * a * b + a ^ 2`,
and its sum is right-nested throughout. `StringizeRoundTripTest` pins that decision so that it
stays one.

**LaTeX.** `Latexize` moves for the four set and `mod` cases above and **not** for `implies`.
[CSharpMath.Evaluation](https://github.com/verybadcat/CSharpMath/blob/master/CSharpMath.Evaluation/Evaluation.cs),
which reads our LaTeX back, folds `\cup`, `\setminus`, `\in`, `\cdot` and `\bmod` to the left at
the same precedences this grammar uses — so those needed the same brackets — but folds `\to` to the
**right**, which is the usual convention for implication and is what `Latexize` was already
bracketing for. The change only ever adds `\left(`/`\right)` groups, which CSharpMath already
parses, so nothing downstream needs a matching change
([#822](https://github.com/asc-community/AngouriMath/issues/822)).

### A limit binds the variable it approaches along

`FreeVariables` learned about a summation, a product and a definite integral in
[#1045](https://github.com/asc-community/AngouriMath/pull/1045), and about `limit` not at all —
that commit names the indefinite integral and the derivative as deliberate exclusions and does
not mention it.

| | before | now |
|---|---|---|
| `"limit(t * b, t, 0)".ToEntity().FreeVariables` | `{ t, b }` | `{ b }` |
| `"limit(t, t, b)".ToEntity().FreeVariables` | `{ t, b }` | `{ b }` |
| `"limitleft(t * b, t, 0)".ToEntity().FreeVariables` | `{ t, b }` | `{ b }` |
| `"limit(t, t, 0)".ToEntity().FreeVariables` | `{ t }` | `{ }` |

The reason the indefinite integral and the derivative do not bind is exactly what makes a limit
bind: an antiderivative of `t * b` over `t` is `b * t ^ 2 / 2 + C` and `d/dt` denotes a function
of `t`, both still functions of the variable — while **a limit never is**. `lim(t, t, 0)` is `0`,
and no limit's value depends on the name it approaches along. The destination is where the
dependence goes, so it is bound over too: `lim(t, t, b)` is a function of `b` alone.

`Vars` and `VarsAndConsts` are untouched, as they were in #1045 — they mean every name occurring
([#989](https://github.com/asc-community/AngouriMath/issues/989)).

### A leading coefficient that is a polynomial no longer stops the lift

The Hensel lift below declined when the leading coefficient in the main variable was not a
constant: the lifted factors' leading coefficients have to be known before the lift to keep them
polynomials rather than power series. That is Wang's leading-coefficient problem, and it is
avoided here rather than solved — `L^(n-1) f(z/L, y) = Σ a_i L^(n-1-i) z^i` is a polynomial,
because `n - 1 - i` is never negative below the leading term, and it is **monic** in `z`, because
the leading term contributes `L · L^(-1)`. A monic polynomial has a constant leading coefficient,
which is the case the lift already handled. A factor comes back as `h(L·x, y)` with the content
that substitution introduced divided out.

| | before | now |
|---|---|---|
| `Factor("(y * x3 + 1) * (x4 - y3)", "x")` | `null` | `(x ^ 4 - y ^ 3) * (x ^ 3 * y + 1)` |
| `Factor("(y * x + 1) * (x7 - y7)", "x")` | `null` | three factors |
| `Factor("(y * x2 + 1) * (x6 - y6)", "x")` | `null` | five factors |
| `Factor("(y2 * x3 + 1) * (x5 - y2)", "x")` | `null` | `null` — past the growth bound below |
| `Factor("(y * x + 1) * (x + y)", "x")` | `(x * y + 1) * (x + y)` | unchanged — the substitution already reached it |

That last-but-one row is a **bound rather than a limitation of the method**. The monic form
carries `L^(n-1-i)`, so its degree in the auxiliary variable exceeds the original's by up to
`(n-1)·deg L`. Measured: a growth of 6 costs 376 ms and a growth of 7 costs 1 ms, while a growth
of 14 costs **63 seconds** — so growth past 12 is refused. A refusal is a legitimate answer and a
sixty-three second one is not, which is the same judgement the recombination's own cap makes.

### A factorisation multiplies back to what it factored

`MathS.Polynomials.Factor` dropped a constant content when the polynomial had more than one
variable, so the product it returned was **not equal to the polynomial it factored**.

| | before | now |
|---|---|---|
| `Factor("4 * x2 - 4 * y2", "x")` | `(x + y) * (x - y)` | `4 * (x + y) * (x - y)` |
| `Factor("2 * x2 - 2 * y2", "x")` | `(x + y) * (x - y)` | `2 * (x + y) * (x - y)` |
| `Factor("6 * x4 - 6 * y4", "x")` | three factors, no `6` | `6 * (x + y) * (x ^ 2 + y ^ 2) * (x - y)` |
| `Factor("3 * x * y + 3 * y", "x")` | `y * 3 * (x + 1)` | unchanged — the content is not a constant |
| `Factor("2 * x2 - 2", "x")` | `2 * (x + 1) * (x - 1)` | unchanged — univariate |

`KroneckerFactorization.Factor` documents its result as *"each of positive degree in the main
variable"*, so a constant content is deliberately not among the factors it returns and the caller
must reinstate it. `MathS.Polynomials.Kronecker` assembled the factors into a product and never
did. Only "multivariate **and** the content is a pure number" landed in the gap; a content with a
variable in it goes down a different path that reinstates it, and a univariate polynomial never
reaches the substitution.

The content is recovered by dividing the polynomial by the assembled product, which is what the
rest of this layer does with a claim it could get wrong — a quotient that is missing or is not a
constant means the product does not account for the polynomial, and then there is no answer to
give. Everything here was already checked by exact division, but that check is on the individual
*factors*; nothing compared the assembled product against the input
([#1092](https://github.com/asc-community/AngouriMath/issues/1092)).
### `Factorize` reaches inside a product the rules already made

The change below asks the polynomial layer only where the rewrite rules said nothing, so that an
answer the rules already gave is never replaced. Declining a **product** outright was too broad:
the rules take a numeric content out and hand back `2 * (x ^ 3 - 1)`, and the remainder is exactly
the shape that change is about.

| | before | now |
|---|---|---|
| `"2 * x3 - 2".Factorize()` | `2 * (x ^ 3 - 1)` | `2 * (x - 1) * (x ^ 2 + x + 1)` |
| `"3 * x6 - 3".Factorize()` | `3 * (x ^ 6 - 1)` | five factors |
| `"5 * x7 - 5".Factorize()` | `5 * (x ^ 7 - 1)` | `5 * (x - 1) * (x ^ 6 + … + 1)` |
| `"y * (x3 - 1)".Factorize()` | `y * (x ^ 3 - 1)` | `y * (x - 1) * (x ^ 2 + x + 1)` |
| `"2 * x4 - 10 * x2 + 8".Factorize()` | `2 * (x + 1) * (x + 2) * (x - 2) * (x - 1)` | unchanged |
| `"x * y + x".Factorize()` | `x * (1 + y)` | unchanged |

Each factor of the product is asked separately instead of the product being handed over whole, so
every factor the rules found survives and only the ones they could not split are split. Found by
asking the property [#1092](https://github.com/asc-community/AngouriMath/issues/1092) is about —
that the answer multiplies back to its input — of a row that satisfied it and was still less
factored than it should be.
### An equation between two powers of numeric bases is answered exactly

`3 ^ (x+1) = 2 ^ (x-1)` has the exact root `-ln(6) / ln(3/2)`. The multiplicative solver reached it
by dividing one exponent by the other; for two different integer bases that ratio is
`ln(3)/ln(2)` — irrational — so `InnerSimplified` settled it to a decimal and everything after was
numeric. The answer agreed with the exact one to seventeen significant figures and diverged, which
is the signature of a `double` promoted to a decimal rather than a number that was computed.

| | before | now |
|---|---|---|
| `"3^(x+1) - 2^(x-1)".SolveEquation("x")` | `{ ln(0.0467456982262863043886547131933184573426842689514160156250 ^ (1 / ln(2))) }` | `{ -(ln(3) + ln(2)) / (ln(3) + -ln(2)) }` |
| `"3^(x+1) - 2^x".SolveEquation("x")` | a decimal | `{ -ln(3) / (ln(3) + -ln(2)) }` |
| `"5^(2x) - 7^(x+3)".SolveEquation("x")` | `{ }` — **no answer at all** | `{ 3 * ln(7) / (2 * ln(5) + -ln(7)) }` |
| `"3^x - 2^x".SolveEquation("x")` | `{ 0 }` | unchanged |
| `"2^(2x) - 5·2^x + 4".SolveEquation("x")` | `{ 0, 2 }` | unchanged |

Taking logarithms has no such step: `a ^ p = b ^ q` is `p ln a = q ln b` for positive real `a` and
`b`, which the analytical solver answers exactly. Both bases must be **decidably positive reals**,
which is what makes the step an equivalence rather than a branch choice; anything else declines and
the multiplicative path still gets its turn, so this only ever adds answers
([#1007](https://github.com/asc-community/AngouriMath/issues/1007)).
### A widened codomain is written out, so `Stringize` round-trips it

`Domain.Any` had no spelling: the second argument of `domain(...)` had to be one of the five
special sets, and none of them means "no restriction". So a node **widened** to `Any` from a
narrower default printed as though it had not been, and reading that back gave the default.

| | before | now |
|---|---|---|
| `MathS.Abs("x").WithCodomain(Any).Stringize()` | `abs(x)` | `domain(abs(x), Any)` |
| the same, reparsed | `Real` — the widening was lost | `Any` |
| `MathS.Abs("x").WithCodomain(Integer).Stringize()` | `domain(abs(x), ZZ)` | unchanged |
| `"Any + 1"` | parses, `Any` is a variable | unchanged |

`Any` is **not** a set literal — there is still no node for "no restriction", and
`SpecialSet.Create(Domain.Any)` still throws for it. It is read in the second argument of
`domain(...)` and nowhere else, which commits to a spelling without deciding whether there is a
universal *set*; that question is [#996](https://github.com/asc-community/AngouriMath/issues/996)
and this does not answer it.

Read rather than lexed, deliberately. A literal in a parser rule becomes a global lexer token, and
making `Any` a keyword reserved the name everywhere — `Any + 1` stopped parsing. A test pins that
it stays an ordinary variable.

`Latexize` renders the subscript as `\mathrm{Any}` rather than a `\mathbb`, since there is no set
to render ([#1048](https://github.com/asc-community/AngouriMath/issues/1048)).

### A fold over an empty sequence answers instead of throwing

A fold over a monoid has an identity, so the empty sum is `0` and the empty product is `1` — which
is what makes `xs.Concat(ys).SumAll() == xs.SumAll() + ys.SumAll()` hold for every pair, the empty
one included. These threw instead, and threw the wrong *kind* of exception: `AngouriBugException`
ends its message asking the caller to report a bug against this repository, for a list their own
`Where` happened to filter to nothing.

| | before | now |
|---|---|---|
| `new Entity[0].SumAll()` | `AngouriBugException` | `0` |
| `new Entity[0].MultiplyAll()` | `AngouriBugException` | `1` |
| `Sumf.Sum(new Entity[0])` | `AngouriBugException` | `0` |
| `Mulf.Multiply(new Entity[0])` | `AngouriBugException` | `1` |
| `MathS.Vector()` | `IndexOutOfRangeException` | `InvalidMatrixOperationException` |
| `new Entity[0].ToVector()` | `IndexOutOfRangeException` | `InvalidMatrixOperationException` |

`IndexOutOfRangeException` is not under `AngouriMathBaseException`, so a caller catching the
hierarchy `Docs/Usage/Exceptions.md` documents did not catch it at all.

`Sumf.Sum` and `Mulf.Multiply` are not named in the issue. The defect is *passing an unchecked
caller collection into `MultiHangBinary`*, whose `>= 1` precondition is genuine, and those two are
public and do exactly that. `MultiHangBinary` itself is unchanged
([#1028](https://github.com/asc-community/AngouriMath/issues/1028)).

### `Factorize` uses the polynomial layer where no rule reaches

`Entity.Factorize` was composed entirely out of `RewriteRules`, so it factored what someone had
written a rule for and handed everything else back whole — while square-free decomposition,
Zassenhaus over `Q`, Kronecker's substitution and Hensel lifting all sat in the tree unused by it.

| | before | now |
|---|---|---|
| `"x3 - 1".Factorize()` | `x ^ 3 - 1` | `(x - 1) * (x ^ 2 + x + 1)` |
| `"x4 - 5x2 + 4".Factorize()` | `x ^ 4 - 5 * x ^ 2 + 4` | `(x + 1) * (x + 2) * (x - 2) * (x - 1)` |
| `"x6 - 1".Factorize()` | `x ^ 6 - 1` | `(x + 1) * (x - 1) * (x ^ 2 + x + 1) * (x ^ 2 - x + 1)` |
| `"x7 - 1".Factorize()` | `x ^ 7 - 1` | `(x - 1) * (x ^ 6 + x ^ 5 + x ^ 4 + x ^ 3 + x ^ 2 + x + 1)` |
| `"x2 + 2x + 1".Factorize()` | `x ^ 2 + 2 * x + 1` | `(x + 1) ^ 2` |
| `"a2 - b2".Factorize()` | `(a - b) * (a + b)` | unchanged |
| `"x4 - y4".Factorize()` | `(x - y) * (x + y) * (x ^ 2 + y ^ 2)` | unchanged |
| `"x * y + x".Factorize()` | `x * (1 + y)` | unchanged |
| `"sin(x) + 1".Factorize()` | `sin(x) + 1` | unchanged |

**The layer speaks only where the rules said nothing.** An expression the rules already turned
into a product keeps their answer exactly — the order two factors come out in is arbitrary and
theirs is the one on record, so replacing it would change answers that were never the complaint.
What moves is only what came back whole.

**`Simplify` is unchanged**, and that took a second seam. It offers a factorisation as a
*candidate* and its cost model decides; the metric prefers the expanded form, so a factored
candidate wins only where the two are closest — and those are the places a factored answer is
least wanted (`x ^ 3 / 3 + x ^ 2 / 2` became `(3 + 2 * x) * x ^ 2 / 6`, an antiderivative in a
form nobody writes). `Transformation.RuleBasedFactorizationAtLevel` is what that candidate site
uses now. Offering the layer to that search is
[#746](https://github.com/asc-community/AngouriMath/issues/746) tier 2's pluggable cost model
rather than this ([#1018](https://github.com/asc-community/AngouriMath/issues/1018)).

### A polynomial in two variables is factored by lifting rather than by substituting

`MathS.Polynomials.Factor` in several variables was Kronecker's substitution alone, and its
one-variable image **over-factors**: `x7 - y7` maps to `t^7 (1 - t^49)`, whose factors are
cyclotomic, so a two-factor bivariate becomes a one-variable polynomial with many irreducibles
and the recombination is exponential in a count the substitution inflated itself. Those were
refusals.

An *evaluation* image inflates nothing — `x7 - y7` at `y = 1` is `x7 - 1`, which has the two
factors the answer has — and the factorisation of that image is lifted back one power of `y` at
a time.

| | before | now |
|---|---|---|
| `Factor("x7 - y7", "x")` | `null` | `(x - y) * (x ^ 6 + x ^ 5 * y + x ^ 4 * y ^ 2 + x ^ 3 * y ^ 3 + x ^ 2 * y ^ 4 + x * y ^ 5 + y ^ 6)` |
| `Factor("x6 - y6", "x")` | `null` | `(x + y) * (x - y) * (x ^ 2 + x * y + y ^ 2) * (x ^ 2 - x * y + y ^ 2)` |
| `Factor("x12 - y12", "x")` | `null` | six factors, the full cyclotomic split |
| `Factor("x4 - y10", "x")` | `null` | `(x ^ 2 + y ^ 5) * (x ^ 2 - y ^ 5)` |
| `Factor("x16 + 4 x8 y + 3 y2", "x")` | `null` | `(x ^ 8 + 3 * y) * (x ^ 8 + y)` |
| `Factor("x ^ 3 - (y + z) ^ 3", "x")` | `null` | `null` — three variables, and the lift goes along one |
| `Factor("(x + y) * (x - y)", "x")` | `(x + y) * (x - y)` | unchanged |

**What it will not do.** The leading coefficient in the main variable has to be a constant.
Where it is a polynomial in `y`, the lifted factors' leading coefficients must be known before
the lift to keep them polynomials rather than power series — Wang's leading-coefficient problem
— and that is a second algorithm on top of this one. It declines, and the substitution is still
tried first. Three or more variables decline for the same reason: the lift goes along one
evaluation at a time.

**Nothing is trusted.** Every candidate is checked by exact division of the original, so a bad
evaluation point, a lift that drifted, or a recombination that is not a factor costs a refusal
and cannot cost a wrong answer.

This is [#746](https://github.com/asc-community/AngouriMath/issues/746) tier 1's Hensel lifting
item, in two variables.

### A polynomial in too many variables to substitute can still be answered

`MathS.Polynomials.Factor` in more than one variable works by Kronecker's substitution, whose
one-variable image has degree `Π (d_i + 1) - 1` — a *product*. That leaves the one-variable
factoriser's reach after very few variables, and the answer was a refusal:

| | before | now |
|---|---|---|
| `Factor("x2 + y2 + z2 + w2 + 1", "x")` | `null` | `w ^ 2 + x ^ 2 + y ^ 2 + z ^ 2 + 1` |
| `Factor("x2 + y2 + z2 + 1", "x")` | `x ^ 2 + y ^ 2 + z ^ 2 + 1` | unchanged |
| `Factor("x7 - y7", "x")` | `null` | `null` — it *is* reducible, and this says nothing about it |
| `Factor("(x + y) * (x - y)", "x")` | `(x + y) * (x - y)` | unchanged |
| `Factor("y * (x + 1)", "x")` | `y * (x + 1)` | unchanged |

Where the substitution gives up, the polynomial is now evaluated at an integer point in every
variable but the main one and the one-variable image is factored. **An image that is irreducible
and has kept its degree is a proof that its source is irreducible** — substitution is a ring
homomorphism, so a factorisation would survive it, and degrees in the main variable add, so
neither part can have lost any while the total is preserved. An evaluation image has the degree
of the polynomial in the main variable however many other variables there are, so this reaches
where the substitution cannot.

It answers in one direction only. A reducible image says nothing, so `x7 - y7` is still refused;
a factor free of the main variable is invisible to it, so the content is checked and anything but
a constant declines. Since
[#1059](https://github.com/asc-community/AngouriMath/pull/1059) "it does not factor" is an
answer rather than a refusal, which is what makes this worth having
([#746](https://github.com/asc-community/AngouriMath/issues/746) tier 1).

### `x! = 0` carries the condition under which the factorial exists

A factorial is never zero **where it is defined**, and at a negative integer it is not defined.
`"x! = 0".ToEntity().Simplify()` was `False` for every `x`, so at `x = -1` it answered a question
the original declines: `(-1)! = 0` evaluates to `NaN`.

| | before | now |
|---|---|---|
| `"x! = 0".ToEntity().Simplify()` | `False` | `False provided x in RR and (x >= 0 or not x in ZZ)` |
| the same, at `x = 3` | `False` | `False` |
| the same, at `x = -1` | `False` | `NaN` |
| `RewriteRules.InequalityEquality.ApplyOnce("x! = 0")` | `False` | `False provided x in RR and (x >= 0 or not x in ZZ)` |

The rule read `Factorialf({ DomainCondition: var condition })`, which is a property pattern on the
factorial's **argument** rather than on the factorial. For a bare variable that condition is `True`,
and `Provided` drops a `True`, so the answer went out unconditioned. One character of pattern syntax
between the two, and the wrong one reads as though it were about the factorial.

Where the factorial exists the answer is still `False`, so the condition narrows the rule rather
than withdrawing it. A factorial **over itself** moves with it: `a / a = 1 provided a != 0`
produced `1 provided not x! = 0`, whose condition used to discharge itself because `x! = 0` was
`False`, and now survives. `"x! / x!".ToEntity().Simplify()` was `1` at every point including the
poles; it now agrees with the original at `x = 3`, `0`, `-1` and `-2`, where `1` disagreed at both
negative integers. Found by transcribing the set into `MatchedRules` for
[#746](https://github.com/asc-community/AngouriMath/issues/746) tier 1, as the single disagreement
out of 3,485 generated expressions
([#1081](https://github.com/asc-community/AngouriMath/issues/1081)).

### Four `or`-with-equality rules gave the opposite comparison

`a < b or a = b` is `a <= b`. Written with the comparison the other way round it answers the other
way round: `b < a or a = b` is `a >= b`. Four of the eight arms of `InequalityEqualityRules` that
say this carried their neighbour's answer, so the result was the negation of the input everywhere
off the diagonal.

| | before | now |
|---|---|---|
| `RewriteRules.InequalityEquality.ApplyOnce("(y < x) or (x = y)")` | `x <= y` | `x >= y` |
| `RewriteRules.InequalityEquality.ApplyOnce("(y > x) or (x = y)")` | `x >= y` | `x <= y` |
| `RewriteRules.InequalityEquality.ApplyOnce("(x = y) or (y < x)")` | `x <= y` | `x >= y` |
| `RewriteRules.InequalityEquality.ApplyOnce("(x = y) or (y > x)")` | `x >= y` | `x <= y` |
| `RewriteRules.InequalityEquality.ApplyOnce("(x < y) or (x = y)")` | `x <= y` | `x <= y` — this half was right |
| `RewriteRules.InequalityEquality.ApplyOnce("(x > y) or (x = y)")` | `x >= y` | `x >= y` — and so was this |

`Simplify` moves with it: `"(y < x) or (x = y)".ToEntity().Simplify()` was `x <= y` and is `x >= y`.
At `x = 3, y = 2` the input is True and the old answer is False.

**Only reachable with both operands symbolic**, which is why it survived. With a number on one side,
the `Lessf(var @const, ...)` arm further down the same set rewrites `2 < x` to `x > 2` earlier in the
pass, so the disjunction is only ever looked at with both halves written the same way round and one
of the four *correct* arms matches — `"(2 < x) or (x = 2)".ToEntity().Simplify()` was and is `x >= 2`.

Found by transcribing the set into `MatchedRules` for
[#746](https://github.com/asc-community/AngouriMath/issues/746) tier 1: writing a rule out as data
makes the correspondence between its pattern and its replacement something you have to state, and
four of these did not survive stating it
([#1077](https://github.com/asc-community/AngouriMath/issues/1077)).

### A reciprocal inside a logarithm is no longer moved out unconditionally

`ln(1/b) = -ln(b)` is false on the negative reals, because the principal argument does not negate
with its logarithm. At `b = -0.63`, `ln(1/b)` is `0.462 + πi` and `-ln(b)` is `0.462 − πi`.

Three arms of `PowerRules` applied it for every `b`, and they now carry the guard their neighbours
ten lines below already had — `ln(1/b)` is `ln(1) − ln(b)`, so a reciprocal is the *difference* case
of the logarithm gathering and the same helper answers it. Both ways of earning the rewrite carry
over: the argument is decidably a positive real, or the limit machinery is reading towards a
destination and has established the sign on the way.

| | before | now |
|---|---|---|
| `RewriteRules.Power.ApplyOnce("ln(1 / x)")` | `-ln(x)` | `ln(1 / x)` |
| `RewriteRules.Power.ApplyOnce("log(2, 1 / x)")` | `-log(2, x)` | `log(2, 1 / x)` |
| `RewriteRules.Power.ApplyOnce("log(1 / x, 1 / y)")` | `log(x, y)` | `log(1 / x, 1 / y)` |
| `RewriteRules.Power.ApplyOnce("ln(1 / 2.5)")` | `-ln(5/2)` | `-ln(5/2)` — the argument is positive |

**`Simplify` is unchanged**, and that is why this went unnoticed: `"ln(1 / x)".Simplify()` was
`ln(1 / x)` before and after, because the candidate search never picked that branch. So no answer
from the public simplifier moves. What moves is `RewriteRules.Power` applied on its own, which is
what [#746](https://github.com/asc-community/AngouriMath/issues/746) tier 2 makes a caller able to
do ([#1062](https://github.com/asc-community/AngouriMath/issues/1062)).

### `RewriteRuleGrowth` gained a fourth value, `Unknown`

A rule written as **data** builds its answer in code rather than spelling it out, so there is nothing
to count and no growth to report. The three existing values all make a claim; reporting such a rule
as `Rearranges` — the middle one, and the one that reads as harmless — would be a claim about a
rewrite nobody measured.

**Additive**, so nothing that exists changes value. What it means for a caller is that a `switch`
over `RewriteRuleGrowth` is no longer exhaustive, and code with no default arm will not compile
against the new assembly until it handles the fourth case.

It appears wherever a rule's replacement is code, which today is `RationalizeDenominator` and the
one-way rules of the sets already expressed as data.
### `RewriteRules.Boolean` absorbs in three orientations it used to miss

Absorption is written eight times in `Patterns.BooleanRules`, once for each way the shared operand
can sit inside two commutative pairs — and three of the ways were never written. Expressed as data
the law is **one commutative rule**, which covers all of them.

| | before | now |
|---|---|---|
| `RewriteRules.Boolean.ApplyOnce("a and b or a")` | `a and b or a` | `a` |
| `RewriteRules.Boolean.ApplyOnce("(a or b) and a")` | `(a or b) and a` | `a` |
| `RewriteRules.Boolean.ApplyOnce("a or b and not a")` | `a or b and not a` | `a or b` |

Each is a correct absorption: `(a ∧ b) ∨ a` is `a`, and `a ∨ (b ∧ ¬a)` is `a ∨ b`.

**`Simplify` is unchanged** — it reached all three already, because the canonical order puts the
operands into a fixed arrangement before the rules run, so the orientations the arms missed were
never the ones it was handed. What moves is `RewriteRules.Boolean` applied on its own.

The same file already carried a comment about this class of gap, on the excluded-middle rule:
*"The same law with the operands the other way round. `or` is commutative, so leaving this out made
the answer depend on which side the negation was written."* It was fixed there for one rule and left
for the rest.

### `Factor` factors a polynomial in more than one variable

After the content is taken out, what remains may still have polynomial coefficients — and it can be
factored anyway, by **Kronecker's substitution written in mixed radix**. A factor of a polynomial
has degree at most `d_i` in each variable `v_i`, because a factor divides it. So with radices
`d_i + 1` and place values `s_0 = 1`, `s_(i+1) = s_i * (d_i + 1)`, the map sending a monomial to
`t^(Σ e_i · s_i)` writes each exponent as one digit of a numeral, and is therefore injective on
every monomial that can appear in the polynomial or in any of its factors. The one-variable image
is factored by the existing factoriser, and each subset of its irreducible factors names a
candidate.

| | 2.3.0 | now |
|---|---|---|
| `Factor("x ^ 2 - y ^ 2", "x")` | `null` | `(x + y) * (x - y)` |
| `Factor("x ^ 2 + 2 * x * y + y ^ 2", "x")` | `null` | `(x + y) ^ 2` |
| `Factor("x ^ 3 - y ^ 3", "x")` | `null` | `(x - y) * (x ^ 2 + x * y + y ^ 2)` |
| `Factor("x ^ 4 - y ^ 4", "x")` | `null` | `(x + y) * (x ^ 2 + y ^ 2) * (x - y)` |
| `Factor("x ^ 2 * y ^ 2 - 1", "x")` | `null` | `(x * y + 1) * (x * y - 1)` |
| `Factor("x ^ 2 - y ^ 2 + 2 * x + 1", "x")` | `null` | `(x + y + 1) * (x - y + 1)` |
| `Factor("x ^ 2 - (y + z) ^ 2", "x")` | `null` | `(x + y + z) * (x - y - z)` |
| `Factor("x ^ 2 + 2 * x * y + y ^ 2 - z ^ 2", "x")` | `null` | `(x + y + z) * (x + y - z)` |
| `Factor("(x + y) * (x + z) * (x + w)", "x")` | `null` | `(x + y) * (w + x) * (x + z)` |
| `Factor("x * y + y * z", "x")` | `null` | `y * (x + z)` |
| `Factor("a * x + a * y + a * z", "x")` | `null` | `a * (x + y + z)` |
| `Factor("x ^ 2 * y - y ^ 3", "x")` | `null` | `y * (x + y) * (x - y)` |
| `Factor("x ^ 2 + y ^ 2", "x")` | `null` | `x ^ 2 + y ^ 2` — irreducible over ℚ |
| `Factor("x * y + z", "x")` | `null` | `x * y + z` — irreducible over ℚ |
| `Factor("x ^ 2 - a", "x")` | `null` | `x ^ 2 - a` — irreducible over ℚ |
| `Factor("x + y", "x")` | `null` | `x + y` — irreducible over ℚ |

**"It does not factor" is an answer, and the input is how it is said.** The substitution does not
merely fail to find a factorisation; it *proves* there is none. A splitting of the polynomial into
two parts of positive degree in the main variable maps to a splitting of the one-variable image,
because the substitution is a ring homomorphism on these monomials — and every splitting of the
image is one of the subsets the recombination tries. So where nothing recombines, nothing exists.

Reporting that proof as `null` threw away the content along with it: `x * y + y * z` was refused,
although `y` had already been taken out and `y * (x + z)` is a factorisation. It also disagreed with
the one-variable path, where `Factor("x ^ 2 + 1", "x")` has always been `x ^ 2 + 1`.

The proof has one precondition and it is now checked. A trial division answers `null` both for "does
not divide" and for "ran out of room", and only the first is evidence — so where a division was cut
short by a term or degree budget, the irreducibility claim is withheld and the answer stays a
refusal.

**It cannot answer wrongly.** The substitution is injective on monomials but not on factorisations,
so the image may factor further than the polynomial does and a candidate is a guess. Every one is
tested by exact division before it is kept, and the assembled factors are divided back into the
input, so the failure mode is a refusal.

**What it refuses.** The image has degree `Π (d_i + 1) - 1`, a **product** and not a sum, and the
one-variable factoriser stops at 32 — so the ceiling closes quickly as variables are added. Two
variables reach bidegrees like (2, 10), (3, 7) and (5, 4); three variables of degree 2 fit (27) and
four do not (81). `Factor("x ^ 12 - y ^ 12", "x")` and
`Factor("(x + y + z + w) * (x - y)", "x")` are both `null` for this reason, though both factor
mathematically. The recombination is over subsets, so the image's factor count is capped too.
Lifting that ceiling is Hensel lifting with an evaluation homomorphism, which is a different piece
of work.

`MathS.Polynomials.Factor` has no caller inside the library, so no simplification, solution or
integral changes with it.

### The square-free part is taken where the coefficients are polynomials

`MathS.Polynomials.SquareFreePart` refused every polynomial in more than one variable, for the same
reason `Factor` did: it was written against a representation with rational coefficients.

`p / gcd(p, dp/dx)` is the square-free part whatever ring the coefficients live in — a repeated
factor appears in the derivative one time fewer than in the polynomial, so dividing by the common
part leaves each distinct factor exactly once. The multivariate representation has all three
operations already: `DerivativeIn`, the recursive greatest common divisor that
`MathS.Polynomials.Gcd` is built from, and exact division.

| | 2.3.0 | now |
|---|---|---|
| `SquareFreePart("(x - y) ^ 2 * (x + y)", "x")` | `null` | `x ^ 2 - y ^ 2` |
| `SquareFreePart("(x - y) ^ 3", "x")` | `null` | `x - y` |
| `SquareFreePart("(x + a) ^ 2 * (x + b)", "x")` | `null` | `a * b + a * x + b * x + x ^ 2` |
| `SquareFreePart("x ^ 2 * y ^ 2", "x")` | `null` | `x` |
| `SquareFreePart("y", "x")` | `null` | `null` |

**The content is dropped, as it always was.** `SquareFreePart("4 * x ^ 2", "x")` is `x` rather than
`4 * x`, because the univariate path takes the primitive part first. `x ^ 2 * y ^ 2` is `x` for
exactly that reason, with `y ^ 2` as the content — the existing convention applied to a wider ring,
not a new one.

Reached only where the rational path declined, so nothing that already answered can change.

### `Factor` takes out the content instead of refusing

`MathS.Polynomials.Factor` works over ℚ, so a coefficient that is not a rational number stopped it
before it began and **every polynomial in more than one variable was refused**. Some of them never
needed a bigger ring: `x * y + y` is `y` times something univariate, and only the `y` was in the way.

The content in the named variable — the greatest common divisor of the coefficients, a polynomial in
the other variables — is now taken out first, using the same multivariate machinery
`MathS.Polynomials.Gcd` is already built from, and what remains goes down the ordinary path.

| | 2.3.0 | now |
|---|---|---|
| `Factor("x * y + y", "x")` | `null` | `y * (x + 1)` |
| `Factor("x ^ 2 * y + x * y", "x")` | `null` | `y * x * (x + 1)` |
| `Factor("a * x ^ 2 + a * x", "x")` | `null` | `a * x * (x + 1)` |
| `Factor("x ^ 2 * y ^ 2 - y ^ 2", "x")` | `null` | `y ^ 2 * (x + 1) * (x - 1)` |

**Only a refusal becomes an answer.** Nothing that already factorised changes, because this path
runs only where the old one returned `null`.

Taking the content out does nothing where the content is a constant, so `x ^ 2 - y ^ 2` is not
answered by this change — it needs factorisation over ℚ(y). That is what Kronecker's substitution
does, in the entry above, and the two paths are tried in that order.

The test that pinned the refusal carried a comment saying that handing `x * y + y` back *"would say
that `y * (x + 1)` does not exist, which is a wrong answer and not a graceful failure"*. It now
asserts that answer, checked numerically at twenty random points per case rather than as a string —
`Simplify` does not prove `y * x * (x + 1)` equal to `x ^ 2 * y + x * y`, and the two are equal.

### A narrowed `Codomain` is printed, so it survives being read back

`Codomain` decides evaluation — `sqrt(-1)` is `i`, and the same expression with `Codomain = Real`
is `NaN`, which is the example on `Entity.Codomain` itself. No node printed it, so the two printed
the same string and the annotation was lost the moment an expression was written out: to a file, to
a database column, through `EntityJsonConverter`, or to another process.
[#1022](https://github.com/asc-community/AngouriMath/issues/1022).

The parser already had the syntax. `domain(expr, SET)` maps onto `WithCodomain` and works for every
node, not only a variable, so only the printing half was missing. Measured on a build of 2.3.0 and a
build of this branch:

| expression | 2.3.0 printed | 2.3.0 read that back as | now prints |
|---|---|---|---|
| `"domain(x, ZZ)".ToEntity()` | `x` | a `Variable` with `Codomain = Any` | `domain(x, ZZ)` |
| `"domain(x + 1, RR)".ToEntity()` | `x + 1` | a `Sumf` with `Codomain = Complex` | `domain(x + 1, RR)` |
| `"domain(sqrt(-1), RR)".ToEntity()` | `sqrt(-1)` | a `Powf` that evaluates to `i` | `domain(sqrt(-1), RR)` |
| `"domain([1, 2], RR)".ToEntity()` | `[1, 2]` | a `Matrix` with `Codomain = Any` | `domain([1, 2], RR)` |
| `Sin(Var("x").WithCodomain(Integer)) + Var("y").WithCodomain(Real)` | `sin(x) + y` | both annotations gone | `sin(domain(x, ZZ)) + domain(y, RR)` |

`EntityJsonConverter` serialises what `Stringize` prints, so it changes with it and needed no code:
`JsonSerializer.Serialize("domain(x, ZZ)".ToEntity())` was `"x"` and is `"domain(x, ZZ)"`.

**A sum stops collecting two terms that are not the same term.** `Simplify`'s polynomial
collection keys a monomial by its base's *printed form*, so while the printed form did not
distinguish `x` from `x` narrowed to the integers, it added them up as one:

```
"x - domain(x, ZZ)".ToEntity().Simplify()      2.3.0: 0                    now: x - domain(x, ZZ)
"domain(x, ZZ) + x".ToEntity().Simplify()      2.3.0: 2 * x                now: domain(x, ZZ) + x
```

`0` is the answer only where `x` is an integer, and the annotation is what says it might not be, so
this was a wrong answer rather than a tidier one. Confirmed as the cause by putting the collision
back — keying on the base with its codomain erased brings `0` and `2 * x` straight back.

**The ordinary expression is untouched.** The wrapper is printed only where the codomain is *not*
the one that parsing the bare text would give back, and nothing inside the library narrows a
codomain — `WithCodomain` is called from the parser and from callers, and from nowhere else. So
`"x + 1".ToEntity().Stringize()` is `x + 1` on both versions, and of the 8,084 tests in the suite
the only two that moved are the one written to pin this defect and the recorded public surface.

That default is **not the same for every node**, which is why the rule is a comparison and not a
check against `Complex`: a `Variable` and a `Matrix` default to `Any`, `Absf`, `Modf`, `Minf`,
`Maxf` and `Interval` to `Real`, every boolean node to `Boolean`, each numeric literal to its own
type's domain, `Phif` to `Integer`, the set nodes and `Providedf`, `Piecewise`, `Application` and
`Lambda` to `Any`, and the rest to `Complex`. Each node now declares that default next to its
`Codomain`, and a test asserts that every freshly built node of every node type carries it.

**LaTeX** renders it as a subscripted set, `{\left(x\right)}_{\mathbb{Z}}`. The parentheses are
unconditional because a variable renders its own index as a subscript, so `x_{\mathbb{Z}}` would be
indistinguishable from a variable spelled that way. This is new output for
[CSharpMath.Evaluation](https://github.com/verybadcat/CSharpMath/blob/master/CSharpMath.Evaluation/Evaluation.cs),
which reads our LaTeX back and has no notion of a codomain; it appears only for an expression that
carries a narrowed one ([#822](https://github.com/asc-community/AngouriMath/issues/822)).

**Two annotations still do not survive, and both are the grammar's limit.** `Any` cannot be written
at all — the second argument of `domain(...)` has to be one of the five special sets, and none of
them means "no restriction" — so a node *widened* to `Any` from a narrower default still prints as
though it had not been. And no input string yields a rational literal whose codomain is `Complex`,
because the pass that reads `1/2` as a `Rational` rather than a quotient
([#873](https://github.com/asc-community/AngouriMath/issues/873)) uses `Complex` as its "nobody
annotated this" sentinel. Both are pinned by tests that fail if they start working, so neither can
outlive itself.

**The public surface.** Each node type used to declare its own `public override string Stringize()`
and `Latexize()`; the codomain wrapper is one decision and now lives once on `Entity`, with the
per-node rendering behind an internal member. All 130 overrides are therefore gone from
`PublicApi.txt`. Nothing a caller can write stops compiling — `someSumf.Stringize()` still resolves,
inherited from `Entity` — and nothing already compiled stops running either.

That second half was measured rather than assumed, because it is the kind of claim that reads
plausibly in both directions. A consumer calling `((Entity.Sumf)e).Stringize()` was compiled against
a build of 2.3.0, then run unchanged against a build of this branch: it prints what it printed
before. Reading its metadata says why — C# binds a virtual call to the type that *declares* the
method, so the emitted reference is `AngouriMath.Entity::Stringize()` whatever the static type of
the receiver, and no consumer ever names `Entity+Sumf::Stringize()` at all. The removal is
invisible except to reflection that asks a node type for its own declared members.

Devirtualising `Stringize()` cannot orphan an override outside the library either: `Entity` already
carried five `internal` or `private protected` abstract members (`Priority`, `SortHashName`,
`IntrinsicCondition`, `ToSymPy`, `InvertNode`), so no assembly but this one has ever been able to
derive a node from it.

### `Compile` works in a trimmed or NativeAOT application

The Linq compilation path found the method for each node by name —
`typeof(MathAllMethods).GetMethod(name, ...)` for the mathematical functions,
`expr.Type.GetMethod("IsNaN")` for the NaN test — and left the operators and conversions between
`Complex`, `BigInteger` and the primitives to `Expression.Add` and `Expression.Convert`, which find
them by reflecting over the operand type. A name resolved at run time is invisible to the trimmer,
so the members were removed and the lookups came back empty. All four are now tables of members
named at compile time: `MathAllMethods.Definitions` generated beside the methods it dispatches to,
and `nanChecks`, `operators` and `conversionOperators` in `CompilationProtocol`.

Nothing changes under the JIT — the same `MethodInfo` reaches the same `Expression` node — and the
44 assertions of `Sources/Tests/AotSmokeTest` give byte-identical output under the JIT, a trimmed
publish and a NativeAOT publish. What changed is that the second and third of those now run at all:
before this, six of them threw and the seventh aborted the process.

`AngouriMath` is marked `IsAotCompatible` for `net8.0` and later as a result, which is also the
silent half of the entry above: a trimmer reads that as permission to remove unused code from
inside the assembly, so an app that trims will now get a smaller `AngouriMath` rather than all of
it. That is the point of the mark, and
[`Docs/Contributing/Trimming.md`](Sources/AngouriMath/Docs/Contributing/Trimming.md) says what
keeps it true.

[#363](https://github.com/asc-community/AngouriMath/issues/363),
[#746](https://github.com/asc-community/AngouriMath/issues/746) item 79.
| **Silent** | `"derivative(y, x) + y - x".SolveEquation("y")`, and the same equation written `= 0` | `{ x }`, which is not a root of it | `{ y : derivative(y, x) + y - x = 0 }` |
| **Silent** | `"integral(y, x) + y - x".SolveEquation("y")` | `{ -(C + -x) / (x + 1) }` | `{ y : integral(y, x) + y - x = 0 }` |
| **Silent** | `"limit(y, x, 0) + y - x".SolveEquation("y")` | `{ x / 2 }` | `{ y : limit(y, x, 0) + y - x = 0 }` |
| **Silent** | `"sum(y, k, 1, 3) + y - k".SolveEquation("y")` | `{ k / 4 }` | `{ y : sum(y, k, 1, 3) + y - k = 0 }` |
| **Silent** | `"{ y : derivative(y, x) + y - x = 0 }".ToEntity().Simplify()` | `{ y : y - x = 0 }`, a different set | unchanged |

### An equation whose unknown stands under a derivative is left unsolved

`"derivative(y, x) + y - x".SolveEquation("y")` answered `{ x }`. Substituting that back —
with this library's own `Substitute` — gives `derivative(x, x) + x - x`, which is `1`. The set
named a member that is not a root.

The derivative went to zero because `y` is not `x`, and every calculus operator does the same:
`limit(y, x, 0)` is `y`, `integral(y, x)` is `x * y + C`, `sum(y, k, 1, 3)` is `3 * y`. Each is a
decision about the *name* `y`, and the root the solver then returns says that name stands for an
expression in `x` — so the answer denies the step that produced it. Every one of the operators
was affected, and so was a quadratic in the unknown: `derivative(y, x) + y ^ 2 - x` answered
`{ sqrt(x), -sqrt(x) }`, which leave `x ^ (-1/2) / 2` and `-1/2 * x ^ (-1/2)`.

The equation is not thereby unsatisfiable, so the empty set would replace one false claim with
another. What holds is the condition as written, and that is what comes back. Solving it needs a
differential-equation solver, which
[#746](https://github.com/asc-community/AngouriMath/issues/746) has as item 48 and which this
library does not yet have.

A root that does not mention the name the operator is taken over denies nothing, and is returned
as before: `"derivative(y * x, x) + y - 1".SolveEquation("y")` is `{ 1/2 }`, and
`"derivative(y ^ 2, y) - 2".SolveEquation("y")` is `{ 1 }` — there the unknown *is* the name the
derivative binds, and no independence is claimed of it.

The set builder had the same reading of its own bound name and lost it the same way:
`{ y : derivative(y, x) + y - x = 0 }` simplified to `{ y : y - x = 0 }`, which is `{ x }`. A
binder over `y` makes `y` range over values, and expressions in `x` are among them, so that
simplification settles a condition that was written to stay open. It no longer fires.

[#964](https://github.com/asc-community/AngouriMath/issues/964)

---
| | `Entity.Set.SpecialSet.Create("NN")`, and any domain name this library does not have | `AngouriBugException` | `UnrecognizedDomainException: Unrecognized domain NN` |
| | `Entity.Set.SpecialSet.Create(Domain.Any)`, and any `Domain` that is not one of the five sets | `AngouriBugException` | `NotSufficientlySupportedException: There is no special set for domain Any` |

### An unknown domain is the caller's input, not a library defect

`SpecialSet.Create(string)` and `SpecialSet.Create(Domain)` are both `public`. Given a name or a
`Domain` value they do not know, both threw `AngouriBugException`, whose message ends *"please report
about it to the official repository"* — so a caller who wrote `"NN"` was told their own typo was a
defect in this library and asked to file it.

Both are now the caller's error, and both stay under `AngouriMathBaseException`, so a `catch` for
that is unaffected. Measured on a build of each side:

| input | 2.3.0 | now |
|---|---|---|
| `Create("NN")` | `AngouriBugException: The given domain is not presented in those possible …please report about it to the official repository` | `UnrecognizedDomainException: Unrecognized domain NN` |
| `Create(Domain.Any)` | the same | `NotSufficientlySupportedException: There is no special set for domain Any` |
| `Create((Domain)99)` | the same | `NotSufficientlySupportedException: There is no special set for domain 99` |
| `Create("RR")` | `RR` | `RR` |

`UnrecognizedDomainException` has existed since it was written and nothing threw it;
[`Docs/Usage/Exceptions.md`](Sources/AngouriMath/Docs/Usage/Exceptions.md) is where the difference
between the three types is written down. `Domain.Any` is a documented member of the enum meaning *no
restriction*, which is not a set this library has a node for — `Domains.IsWithinDomain` answers it
before ever reaching `Create`, so nothing inside the library was affected.

If you catch `AngouriBugException` around a call that builds a set from a name you did not choose,
catch `MathSException` — or its parent — instead.

## 2.3.0 — since 2.2.0

### At a glance

| Silent? | What | Was | Is |
|---|---|---|---|
| | `(Entity.Number)someBigInteger` | `FormatException: Illegal character found`, for almost every value | the number |
| **Silent** | `"1/x".Integrate("x")`, and every antiderivative with a logarithm | `ln(abs(x)) + C` | `ln(x) + C` |
| **Silent** | `CostModel.FewestDivisions.Cost(y ^ (1 * (-1)) * x)` | `0.007`, cheaper than `x / y` | `1.007` |
| **Silent** | `Real.NaN > (Real)1`, and the other three operators | `true` | `false` |
| **Silent** | a rewritten expression under `domain(...)` — including via `Substitute` | the constraint was dropped, so it answered | it refuses, as it did before the rewrite |
| **Silent** | `"x!".Differentiate("x")`, and anything containing it | `NaN` | the unevaluated `derivative(x!, x)` |
| **Silent** | `"7/2".ToEntity()` and any quotient of two integer literals | a `Divf` | the `Rational` it denotes |
| **Silent** | `sum(i, i, 1, 10)`, and every binder given `i` as the name it binds | the imaginary unit in the name position, so nothing was bound | `i` is the bound name, and `55` |
| **Silent** | `sum(2i, i, 1, 3)` | `6i` | `12` |
| **Silent** | `derivative(i ^ 2, i)` | `0` | `2 * i_1` |
| **Silent** | `{ i : i > 0 }` | `NaN` | the set it describes |
| **Silent** | `limit(i, i, 0)` | unevaluated | `0` |
| **Silent** | `integral(i, i)` | `-1/2 + C` | `i_1 ^ 2 / 2 + C` |
| | `lambda(i, i + 1)` | `InvalidArgumentParseException` | the lambda |
| **Silent** | `MathS.ToSympyCode` for any set, lambda, piecewise or non-vector matrix | Python that does not run — `NameError`, `SyntaxError`, `TypeError`, or an exception out of the exporter | Python that runs |
| **Silent** | `"{ k : k > 0 }".ToEntity().FreeVariables` — and `Vars`, and `VarsAndConsts` | `{ %1 }`, a name in no expression | `{ }`, `{ k }`, `{ k }` |
| **Silent** | the symbolic determinant of a matrix, substituted where a pivot vanishes — `[[0,1,2],[3,4,5],[6,7,8]]` through `[[a,b,c],[d,e,f],[g,h,i]]` | `NaN` | `0` |
| **Silent** | `((Entity.Matrix)"[[x, 1], [2, y]]".ToEntity()).Determinant.Simplify()` | `x * y - 2 provided not x = 0` | `x * y - 2` |
| **Silent** | `[1, 2] in RR`, and a matrix or a finite set against any of `BB`, `ZZ`, `QQ`, `RR`, `CC` | `True` | `False` |
| **Silent** | `"sin(pi)".ToEntity().Differentiate(MathS.pi)`, and every derivative over `pi` or `e` | `-1`, the chain rule run over a symbol that cannot change | `0` |
| **Silent** | `"x ^ 3".ToEntity().Differentiate(x, 2)`, and every `Differentiate(x, n)` with `n >= 1` | `(0 * x ^ 2 + 2 * x ^ 1 * 1 * 3) * 1 + 0 * 3 * x ^ 2` | `2 * x * 3` |
| **Silent** | `derivative(e ^ 2, e)`, `derivative(pi ^ 2, pi)` | `0` | `2 * e_1`, `2 * pi_1` |
| **Silent** | `{ e : e > 0 }` | `{ e : True }` | the set it describes |
| **Silent** | `limit(e, e, 0).Evaled` | `2.718…` | `0` |
| **Silent** | `integral(e, e, 0, 1).Evaled` | `3.694…` | `1/2` |
| **Silent** | `integral(arccos(0), pi, 0, 1)` | `1/4` | `pi / 2` |
| **Silent** | `derivative(arccos(0) * pi, pi)` | `0` | `pi / 2` |
| **Silent** | `sum(ln(x), e, 1, 2)`, and any binder over `e` around a logarithm | `log(1, x) + log(2, x)` | `2 * ln(x)` |
| **Silent** | `"sum(pi, pi, 1, 3)".ToEntity().Vars` | empty — the index was read as a constant | `pi` |
| | `MathS.pi.GetType()` | `Entity.Variable` | `Entity.Constant`, which derives from it |
| **Silent** | `derivative(x ^ 3, 3)`, and any derivative or integral over a number | `ln(x) * x ^ 3` | the unevaluated `derivative(x ^ 3, 3)` |
| **Silent** | `derivative(x ^ 2, x + 1)`, over a subexpression that does not occur | `0` | `2 * x`, by change of variables |
| **Silent** | `derivative(x * (x + 1), x + 1)`, where the rename left a variable behind | `x` | `1 + 2 * x` |
| **Silent** | `derivative(x ^ 2, x * y)`, over several variables at once | `0` | the unevaluated derivative |
| **Silent** | `derivative(x ^ 2 + y ^ 2, [x, y])` | `0` | `[2 * x, 2 * y]`, the gradient |
| **Silent** | `integral(x, [x, y]T)` | `[[C + x ^ 2, C + x * y]]` | `[[x ^ 2 / 2 + C, x * y + C]]` |
| **Silent** | `derivative(e ^ 2, e)`, over a named constant | `0` | `2 * e` |

### `ToSympyCode` emits Python that runs

`MathS.ToSympyCode` is documented as producing code you can run in SymPy. For every set, every
lambda, every `piecewise` and every non-vector matrix, it did not — and the failure was **silent**
in the sense that matters here: the string came back looking like Python.

The preamble is `import sympy` and nothing else, so every SymPy name in the body has to be
qualified. Six exports were not, and a seventh threw before it got that far:

```python
expr = FiniteSet(1, 2)                        # was: NameError: name 'FiniteSet' is not defined
expr = Interval(0, 1, ...)                    # was: NameError: name 'Interval' is not defined
expr = Union(FiniteSet(1, 2), FiniteSet(3))   # was: NameError: name 'Union' is not defined
expr = x in S.Reals                           # was: NameError: name 'S' is not defined
expr = sympy.Lambda(x, )                      # was: TypeError: missing 1 required positional argument
```

| input | was | now |
|---|---|---|
| `{ 1, 2 }` | `FiniteSet(1, 2)` | `sympy.FiniteSet(1, 2)` |
| `ZZ` | `S.Integers` | `sympy.S.Integers` |
| `{1,2} \/ {3}` | `Union(FiniteSet(1, 2), FiniteSet(3))` | `sympy.Union(sympy.FiniteSet(1, 2), sympy.FiniteSet(3))` |
| `lambda(x, sin(x) + 1)` | `sympy.Lambda(x, )` | `sympy.Lambda(x, sympy.sin(x) + 1)` |
| `{ x : x > 0 }` | `AngouriBugException` out of the exporter | `sympy.ConditionSet(x, x > 0, sympy.S.UniversalSet)` |
| `[sin(a); b]` | `Interval(sin(a), b, ...)` | `sympy.Interval(sympy.sin(a), b, ...)` |
| `piecewise(sin(x) provided x > 0, …)` | `sympy.Piecewise((sin(x), x > 0), …)` | `sympy.Piecewise((sympy.sin(x), x > 0), …)` |
| `[[sin(a), b], [c, d]]` | `sympy.ImmutableMatrix([[sin(a), b], …])` | `sympy.ImmutableMatrix([[sympy.sin(a), b], …])` |
| `x in RR` | `x in S.Reals` | `(sympy.S.Reals).contains(x)` |

Three separate faults, all of them invisible to the tests that were there:

- **Unqualified names.** `import sympy` binds `sympy`, not `FiniteSet` or `S`.
- **Parts interpolated rather than exported.** `Interval`, `Piecewise` and the non-vector `Matrix`
  wrote their children with `{Left}` instead of `{Left.ToSymPy()}`. A bare variable spells the same
  in both languages, so it only shows once the part is a function — `sin(a)`, which Python does not
  have.
- **A set builder threw.** `ConditionalSet.Codomain` is `Domain.Any`, which `SpecialSet.Create` has
  no member for, so the exporter's cast raised `AngouriBugException` — "please report about it to
  the official repository", which is [#985](https://github.com/asc-community/AngouriMath/issues/985).
  SymPy names that set: `S.UniversalSet`, which is the one it prints `ConditionSet` *without* a
  third argument for.

`x in RR` also changes shape rather than just qualification. Python's `in` coerces its result to a
`bool`, and a membership that is not decided is not one: `x in sympy.S.Reals` raises
`TypeError: did not evaluate to a bool: (-oo < x) & (x < oo)`. `.contains` answers with the
condition, and still answers `True` or `False` where it can.

**Why the tests passed.** They asserted substring containment — `Assert.Contains("Piecewise((a, b),
(c, d))")` — which holds whether the parts were exported or interpolated, and holds on a program
that does not run at all. The new cases pin the whole emitted expression, and one of them asserts
every SymPy name carries its qualifier.

Measured with `work/sympycheck`, which executes the generated program rather than reading it: of 45
emitted programs the corpus now covers, **43 run** and 0 return an inexact value. The two that do
not are the set builders, whose preamble still declares the placeholder that
[#989](https://github.com/asc-community/AngouriMath/issues/989) is about — `%1 = sympy.Symbol('%1')`
is a `SyntaxError` whatever the body says. Their `expr` line is correct here, and with #989's fix
in as well, 45 of 45 run. Suite 7385 passed, 0 failed; corpus unchanged at 116/119 with 0 wrong.
[#985](https://github.com/asc-community/AngouriMath/issues/985).

### A set builder's internal placeholder no longer escapes into `Vars` or `FreeVariables`

`ConditionalSet.DirectChildren` is its predicate with the bound name renamed to a fresh one, so that
two builders differing only in that name compare and hash alike — `{ x : x > 0 } = { y : y > 0 }`
is `True`, and stays `True`. Every property that reports names by walking `DirectChildren` picked
that invented name up and returned it.

```csharp
"{ k : k > 0 }".ToEntity().FreeVariables   // was: { %1 }   now: { }
"{ k : k > 0 }".ToEntity().Vars            // was: { %1 }   now: { k }
"{ k : k > a }".ToEntity().FreeVariables   // was: { %1, a }      now: { a }
"{ k : k > a }".ToEntity().Vars            // was: { %1, a }      now: { k, a }
"x + { k : k > a }".ToEntity().Vars        // was: { x, %1, a }   now: { x, k, a }
```

`%1` is neither answer on anybody's definition. It is not the bound name, it is not in the
expression the caller wrote, `Variable.CreateTemp` invents a different one for a different
predicate, and it cannot be typed — the parser has no `%`. It broke `Vars`'s own promise too, which
is the variables that *occur*: `k` occurs and was missing, `%1` does not occur and was there.

A set builder binds the name it declares exactly as a lambda binds its parameter, so all three
properties now answer for `{ k : ... }` what they already answered for `lambda(k, ...)`:

| | `lambda(k, k > a)` | `{ k : k > a }` was | `{ k : k > a }` now |
|---|---|---|---|
| `FreeVariables` | `{ a }` | `{ %1, a }` | `{ a }` |
| `Vars` | `{ k, a }` | `{ %1, a }` | `{ k, a }` |

Only the reporting changed. `DirectChildren` still publishes the renamed predicate, because that is
what makes two alpha-equivalent builders equal, and `Replace` and `Substitute` never used it — both
override and work from `Var` and `Predicate` directly.

This does **not** answer the second half of [#989](https://github.com/asc-community/AngouriMath/issues/989),
which asks whether `sum`, `integral`, `limit` and `derivative` should bind their variable here too.
That is a documented choice rather than a leak, and it is still open.

Measured: suite 7376 passed, 0 failed; corpus unchanged at 116/119 with 0 wrong, and no case's
verdict or answer altered. [#989](https://github.com/asc-community/AngouriMath/issues/989).

### The symbolic determinant is a polynomial, not a quotient by its pivots

`Matrix.Determinant` was computed by Gaussian elimination, which leaves the pivots as literal
divisions. The expression it returned was therefore undefined wherever a pivot vanishes — at points
where the determinant itself is perfectly well defined.

```csharp
var m = (Entity.Matrix)"[[x, 1], [2, y]]".ToEntity();
m.Determinant             // was: x * (y * x + -2) / x           now: x * y + -2
m.Determinant.Simplify()  // was: x * y - 2 provided not x = 0   now: x * y - 2
m.Determinant.Substitute("x", 0).Substitute("y", 5).Evaled   // was: NaN   now: -2
```

`x = 0` is an ordinary point of that matrix — its determinant there is `-2` — and `NaN` asserts the
value **does not exist**. At 3×3 it stops being an edge case, because the denominator is
`a ^ 4 * (a * e - b * d)` and so two conditions have to miss:

```csharp
// [[a, b, c], [d, e, f], [g, h, i]].Determinant, substituted:
// [[0, 1, 2], [3, 4, 5], [6, 7, 8]]     was: NaN   now: 0     (the pivot a is 0)
// [[1, 2, 3], [2, 4, 6], [1, 1, 1]]     was: NaN   now: 0     (a * e = b * d)
// [[1, 2, 3], [4, 5, 6], [7, 8, 10]]    was: -3    now: -3
// [[2, 1, 0], [1, 2, 1], [0, 1, 2]]     was: 4     now: 4
```

Two of those four are ordinary matrices, and the first is the singular example every linear-algebra
course opens with. Both wrong answers were **silent**: the call succeeded and returned `NaN`, which
is exactly the answer a caller checking for singularity was looking for.

The determinant of a matrix over a commutative ring is a polynomial in its entries, so Laplace
expansion — which never divides — needs no condition at all. It is what the property's own
documentation and the comment above it already claimed was in use.

**This is not a performance trade.** Measured on the same machine, property only, both arms built
from source: Laplace returns a *smaller* expression at every size, and the elimination it replaces
was the slower of the two on numeric matrices by a wide margin.

| entries | Gaussian complexity | Laplace complexity | Gaussian, numeric | Laplace, numeric |
|---|---|---|---|---|
| 2×2 | 13 | 9 | | |
| 3×3 | 79 | 37 | | |
| 4×4 | 443 | 163 | | |
| 5×5 | 2461 | 833 | | |
| 6×6 | 13673 | 5021 | 164 ms | 126 ms |
| 7×7 | | 35173 | | |
| 8×8 | | | over 120 s | 358 ms |
| 10×10 | | | over 120 s | 4478 ms |

Laplace expansion is `O(n!)`, and a fully symbolic determinant has `n!` terms however it is
computed, so that is the size of the answer rather than an overhead. The practical ceiling on a
numeric matrix moves from 7×7 to 10×10; 11×11 does not return, where under the elimination 8×8
already did not. A fraction-free elimination (Bareiss) would raise it further and is worth having,
but it is a separate change and this one is not blocked on it.

Measured: the whole suite passes with the fix in, and the corpus is unchanged — 116/119 with 0
wrong, 0 error, 0 timeout, and no case's verdict or answer altered.
[#992](https://github.com/asc-community/AngouriMath/issues/992).

### A matrix is no longer a member of every special set at once

`SpecialSet.TryContains` decided membership by asking `MayContain`, which is deliberately permissive
about anything that is not a constant leaf — the right answer for a codomain guard, which has to let
through what it has not ruled out, and the wrong one for a membership decision.

```csharp
"[1, 2] in BB".ToEntity().Simplify()              // was: True    now: False
"[1, 2] in ZZ".ToEntity().Simplify()              // was: True    now: False
"[1, 2] in RR".ToEntity().Simplify()              // was: True    now: False
"[1, 2] in (ZZ /\\ BB)".ToEntity().Simplify()     // was: True    now: False
"{ 1, 2 } in RR".ToEntity().Simplify()            // was: True    now: False
```

The old answers were mutually contradictory — `ZZ` and `BB` share no members, and the same matrix was
in both — so nothing could have depended on all of them. `MayContain` itself is unchanged, and so is
every answer for a number or a boolean: `3 in RR` is `True`, `i in RR` is `False`, `x in RR` is still
undecided rather than answered.

Measured: the whole suite passes with the fix in, and the corpus is unchanged at 116/119 with 0 wrong.
[#995](https://github.com/asc-community/AngouriMath/issues/995).

### Differentiating over a constant is `0`, not the chain rule over a symbol that cannot change

`Entity.Differentiate(Variable)` takes a `Variable`, and `MathS.pi` and `MathS.e` are ones — so they
could be handed to it, and it differentiated as though they varied.

```csharp
"pi ^ 2".ToEntity().Differentiate(MathS.pi)        // was: 2 * pi                now: 0
"sin(pi)".ToEntity().Differentiate(MathS.pi)       // was: -1                    now: 0
"x * pi".ToEntity().Differentiate(MathS.pi)        // was: x                     now: 0
"sin(x * pi)".ToEntity().Differentiate(MathS.pi)   // was: cos(x * pi) * x       now: 0
"x!".ToEntity().Differentiate(MathS.pi)            // was: derivative(x!, pi)    now: 0
"e ^ 2".ToEntity().Differentiate(MathS.e)          // was: 2 * e                 now: 0
"pi ^ 3".ToEntity().Differentiate(MathS.pi, 2)     // was: an unsimplified 0     now: 0
```

`sin(pi)` is `0`, and its derivative with respect to anything is `0`. `-1` is `cos(pi)` — the chain
rule run over a symbol that cannot change. `x!` is the case where the library cannot take the
derivative at all, and it is `0` here regardless, because the *variable* is what settles it.

An ordinary variable is untouched, including where a constant is present as a coefficient:
`"x ^ 2 * pi".Differentiate(x)` is `2 * x * pi` on both versions. The guard is about what is
differentiated **over**, not about what appears in the expression. `Differentiate(x, 0)` returns the
input and `Differentiate(x, n)` with `n < 0` integrates; neither changes.

**The test is whether the name evaluates to a number, not whether it is spelled like a constant.**
That is what makes this compatible with
[#984](https://github.com/asc-community/AngouriMath/issues/984): a name a *binder* declares can vary
even when it is spelled `pi`, and it evaluates to itself.

**The node form is a different question and belongs to #984.** On this version
`"derivative(x * pi, pi)".ToEntity().InnerSimplified` also goes from `x` to `0`, because the node
asks the same public method. Once #984's fix lands the parser *binds* `pi` there, so it becomes
`2 * pi_1` — a derivative over the variable the binder holds — and that is the intended answer, not
a regression of this one. The two compose: measured together, `sin(pi)` differentiated by `MathS.pi`
is `0` and `derivative(pi ^ 2, pi)` is `2 * pi_1`.

`Integrate` and `Limit` over a constant are **not** changed. They have no value to give — there is
nothing to integrate with respect to a thing that cannot vary — so leaving them unevaluated is the
existing "I could not settle this", not a refusal of something settled. The derivative *is* settled,
which is why it is answered rather than declined.

Measured: suite 7383 passed, 0 failed; corpus unchanged at 116/119 with 0 wrong, no case's verdict or
answer altered; crashcheck 1834 cases, 0 crashed.
[#993](https://github.com/asc-community/AngouriMath/issues/993).

### `Differentiate(x, n)` answers what `Differentiate(x)` n times answers

The two overloads reached different code. `Differentiate(Variable)` goes through the transformation,
which ends at `DifferentiateOnce` and simplifies; `Differentiate(Variable, int)` called
`InnerDifferentiate` straight in its loop, so nothing was ever simplified — and because each pass
differentiated the *unsimplified* result of the last, every `0 *` and `* 1` the chain rule produces
was still there to be differentiated again.

```csharp
var x = MathS.Var("x");
"x ^ 3".ToEntity().Differentiate(x, 1)   // was: 3 * x ^ 2 * 1                                   now: 3 * x ^ 2
"x ^ 3".ToEntity().Differentiate(x, 2)   // was: (0 * x ^ 2 + 2 * x ^ 1 * 1 * 3) * 1 + 0 * 3 * x ^ 2   now: 2 * x * 3
```

At `n = 3` it is no longer just untidy:

```
"x ^ 4".Differentiate(x, 3)
// was: (0 * x ^ 3 + 3 * x ^ 2 * 1 * 0 + ((0 * x ^ 2 + 2 * x ^ 1 * 1 * 3) * 1 + 0 * 3 * x ^ 2) * 4
//       + 0 * 3 * x ^ 2 * 1) * 1 + 0 * (0 * x ^ 3 + 3 * x ^ 2 * 1 * 4) + 0 * 4 * x ^ 3
//       + (0 * x ^ 3 + 3 * x ^ 2 * 1 * 4) * 0
// now: 2 * x * 3 * 4
```

The value was never wrong — `Differentiate(x, 3)` and differentiating three times agree numerically
on both versions — so this is a change of *form*, and it breaks anyone matching on the shape of the
result. It also means the cost grew with the accumulated mess rather than with the derivative.

`n = 0` (returns the input) and `n < 0` (integrates) are unchanged.

**Why the two were not simply merged.** `Derivativef`'s simplification decides whether a derivative
can be taken by asking for it and keeping the node when a `Derivativef` comes back — that test is
what terminates it. Routing it through an overload that simplifies each pass makes it simplify the
very node it is deciding about, arrive back at itself, and recurse: `derivative(x!, x, 2)` overflowed
the stack after 3214 frames. So the raw loop still exists as an internal `InnerDifferentiate(Variable,
int)`, which is what that caller uses, and only the public overload simplifies. There are cases for
both in `work/crashcheck`.

Measured: suite 7378 passed, 0 failed; corpus unchanged at 116/119 with 0 wrong, no case's verdict or
answer altered; crashcheck 1834 cases, 0 crashed.
[#1002](https://github.com/asc-community/AngouriMath/issues/1002).

### A rewritten node keeps its `Codomain`, so a domain constraint no longer disappears

`Entity.Replace` rebuilds every node on the path to a change, and a rebuilt node started from its
type's default codomain rather than the one the original carried. Any rewrite therefore dropped a
`domain(...)` annotation — including `Substitute`, which is built on `Replace`.

```csharp
"domain(sqrt(x), ZZ)".ToEntity().Substitute("x", "4/9".ToEntity()).EvalNumerical()
// was: 2/3     now: NaN        — 2/3 is not an integer, so the constraint refuses it
```

The old behaviour was **silent** and one-sided: constraints were only ever weakened, never
strengthened, so a rewrite could make an undefined expression look defined but never the reverse.
An expression that answered a value where it should have refused now refuses.

Measured: the whole suite passes unchanged with the fix in, so nothing in the library depended on
the annotation being lost. [#955](https://github.com/asc-community/AngouriMath/issues/955).

### Differentiating a factorial declines instead of answering `NaN`

`d/dx x!` answered `NaN`, which asserts the derivative **does not exist**. It does — `x!` is
`Γ(x + 1)`, the library already evaluates `(1/2)!` as `0.886…`, and `d/dx x!` at 3 is an ordinary
`Γ'(4) ≈ 3.966`. It now returns the unevaluated derivative, which is what every other node the
library cannot differentiate already does.

```csharp
"x!".Differentiate("x")           // was: NaN     now: derivative(x!, x)
"x! + x^2".Differentiate("x")     // was: NaN     now: derivative(x!, x) + 2 * x
```

The second line is why it mattered: `NaN` propagates, so a single factorial destroyed an otherwise
fine derivative. Anything testing `== MathS.NaN` to detect "cannot differentiate" should look for an
`Entity.Derivativef` in the result instead.

A limit that reaches a factorial may now be **refused where it previously answered `NaN`** — the same
change of spelling, one level up.

Computing the derivative properly needs the digamma function and remains
[#171](https://github.com/asc-community/AngouriMath/issues/171).
[#958](https://github.com/asc-community/AngouriMath/issues/958).
### A quotient of two integer literals parses as a `Rational`

`"7/2".ToEntity()` was a `Divf(7, 2)`; it is now the `Rational` 7/2. The value was always the same
and the node was not, so a `Rational` did not survive being printed and read back:

```csharp
Entity original = "3.5".ToEntity();   // Rational, 7/2
original == original.Stringize().ToEntity();   // was: false     now: true
```

`Entity` equality is structural, so anything relying on that round trip — printing to text and
parsing back, which is the natural way to serialize — was quietly getting a different tree.
[#873](https://github.com/asc-community/AngouriMath/issues/873).

**A quotient that reduces to an integer is unchanged.** `"4/2".ToEntity()` is still a `Divf`, not
`2`. Parsing is not simplification, and turning it into `2` would discard what the caller wrote;
`4/2` also already round-tripped, being a `Divf` before and after. Only the non-integer case is one
a `Rational` can print as.

What changes for a caller: `"1/2".ToEntity()` now prints `1/2` rather than `1 / 2`, since `Rational`
and `Divf` space differently, and a pattern match on `Divf` no longer sees a parsed integer
quotient. Matching `Rational` catches both spellings, and `InnerSimplified` already produced a
`Rational` here, so code that simplified before matching is unaffected.

**`Complex` has the same defect and is not fixed here.** A constructed `Complex` prints `1 + 2i` and
parses back as a `Sumf`, or a `Minusf` for `3 - 4i`. It is tracked on the same issue.

### `Real`'s comparison operators refuse `NaN` instead of ordering it

`Real`'s `>`, `>=`, `<` and `<=` ordered `NaN` above every number, so `NaN > 1` was `true` and
`1 < NaN` was `true` as well. That is `EDecimal`'s total order showing through, and it is not what
`double` does, where every comparison against `NaN` is false in both directions.

```csharp
Real one = 1;
Real.NaN > one     // was: true     now: false
Real.NaN >= one    // was: true     now: false
one < Real.NaN     // was: true     now: false
Real.NaN <= one    // was: false    now: false
```

The failure it caused is one-sided and unsafe: a guard written as `if (value > threshold)` treated
an undefined value as *exceeding* the threshold. Of the two possible defaults that is the worse one,
and this library's stated position is that answering wrongly is worse than not answering.

**`CompareTo` is unchanged and still orders `NaN` above every number.** That is deliberate, not an
inconsistency left behind: sorting requires a total order — `Array.Sort` may loop or throw without
one — while an operator does not. "Where does this sort" and "is this greater" are different
questions, and only the second has no answer for a value that is not a number. Anything relying on
the old operator behaviour to sort should call `CompareTo`, which never changed.

[#947](https://github.com/asc-community/AngouriMath/issues/947).

### `FewestDivisions` and `FewestRadicals` score by value, not by spelling

Both tested the exponent's **node**: `Powf(_, Real { IsNegative: true })` and
`Powf(_, Rational and not Integer)`. In `y ^ (1 * (-1))` the exponent is a `Mulf`, so neither
fired, and the writing whose division the model could not see scored *cheapest* — under the
criterion whose whole job is to remove divisions.

| expression | `FewestDivisions` was | is |
|---|--:|--:|
| `x / y` | 1.003 | 1.003 |
| `y ^ (-1) * x` | 1.005 | 1.005 |
| `y ^ (1 * (-1)) * x` | **0.007** | **1.007** |

All three are one value written three ways, so one division each is the answer, and the written
form — being the smallest tree — is now correctly the cheapest. `FewestRadicals` had the same shape
of test and so the same blind spot: `x ^ (2 ^ (-1))` is a square root and is now counted as one.

**`Simplify`'s output does not change.** Measured on a build of each, twelve division-heavy inputs
under both models, twenty-four results, byte-identical. `Simplify` scores candidates it has already
evaluated, so it did not reach the gap; what reaches it is a caller scoring expressions it did not
build — extraction on an e-graph, where every writing of a value is a member of one class at once,
which is how this was found. Only `Cost` returns different numbers, and only for an expression with
an unevaluated exponent.

`Default` and `SmallestTree` are deliberately **unchanged**. They count how an expression is
written, and `y ^ (1 * (-1)) * x` really is a bigger tree — for them the node as written is not an
approximation of the question, it is the question.
[#950](https://github.com/asc-community/AngouriMath/issues/950).

### An antiderivative with a logarithm drops the absolute value unless the codomain is real

`abs` is not holomorphic, so `ln(abs(f))` is an antiderivative of `f'/f` **on the real line and
nowhere else** — differentiating it off the line does not return the integrand. The table produced
it unconditionally, including under the default codomain, which is the complex plane. That is
[#946](https://github.com/asc-community/AngouriMath/issues/946), and separating by codomain is the
answer given on the issue.

```csharp
"1/x".Integrate("x")        // was: ln(abs(x)) + C        now: ln(x) + C
"tan(x)".Integrate("x")     // was: -ln(abs(cos(x))) + C  now: -ln(cos(x)) + C
```

The previous answer is still available, and is now a statement about where you are working rather
than the only thing on offer:

```csharp
using var _ = MathS.Settings.Codomain.Set(Domain.Real);
"1/x".Integrate("x")        // ln(abs(x)) + C, as before
```

**Silent**: the call still succeeds and returns a different expression. If you were integrating on
the reals and relying on the default, set the codomain and nothing changes.

Eleven rules in the table introduced the absolute value themselves and all eleven now ask. The rule
for `∫ ln(abs(ax + b)) dx` is deliberately **not** among them: there the absolute value comes from
the integrand the caller wrote, not from the rule, so it is left where it is.

### A `BigInteger` converts to a `Number` instead of throwing

`Entity` and `Entity.Number` both offer an implicit conversion from
`System.Numerics.BigInteger`, and they did not agree. `Entity`'s read the two's-complement bytes;
`Number`'s passed the same bytes to `EInteger.FromString`, whose `byte[]` overload reads them as
**ASCII digits**:

```csharp
Entity        fine  = new BigInteger(123456789);   // 123456789
Entity.Number threw = new BigInteger(123456789);   // FormatException: Illegal character found
```

So the conversion failed for every value whose bytes are not digit characters — which is nearly all
of them, `1` included. The few that worked did so by accident: `12594` is the two bytes `'2'` and
`'1'`, and came out as `21`.

Nothing in the library reached it, which is why no test caught it; it is only on the public surface.
Both conversions now read the bytes, and a test requires the two to agree rather than merely to
work.

Found while documenting the members a `#pragma warning disable CS1591` was covering — the two
conversions sit in different files and had to be read side by side to look wrong.
[#585](https://github.com/asc-community/AngouriMath/issues/585).

### A binder given `i` as the name it binds reads it as that name

`i` is the imaginary unit, and the lexer decides it — `NUMBER: ... | 'i'` — so it never reaches the
rule that makes variables and could not be a bound name anywhere in the language. Every binder
handed one therefore did something other than bind: a sum bound nothing and stayed unevaluated, a
set builder answered `NaN`, a lambda threw. Naming `i` is now read as the declaration it plainly
is, throughout that binder and nowhere else.

```csharp
"sum(i, i, 1, 10)".ToEntity().Simplify()      // was: unevaluated       now: 55
"sum(2i, i, 1, 3)".ToEntity().Simplify()      // was: 6i                now: 12
"integral(i, i)".ToEntity().Simplify()        // was: -1/2 + C          now: i_1 ^ 2 / 2 + C
"limit(i, i, 0)".ToEntity().Simplify()        // was: unevaluated       now: 0
"derivative(i ^ 2, i)".ToEntity().Simplify()  // was: 0                 now: 2 * i_1
"{ i : i > 0 }".ToEntity().Simplify()         // was: NaN               now: { i : i > 0 }
"lambda(i, i + 1)".ToEntity()                 // was: threw             now: the lambda
```

`2i` is one token, so a written coefficient on the bound name arrives as a single number with
nothing in it to rename; under a binder that names `i` it is read as the product it would be with
any other name, which is what turns `6i` into `12` above.

**Only inside the binder that declares it**, which is what makes this a fix and not a new defect:

```csharp
"sum(i * k, k, 1, 3)".ToEntity().Simplify()          // 6i, unchanged
"sum(i, i, 1, 3) + i".ToEntity().Simplify()          // 6 + i, unchanged
"sum(sqrt(-1) * i, i, 1, 10)".ToEntity().Simplify()  // 55i — the sum's index, times the unit
```

The last is what SymPy answers for `Sum(sqrt(-1) * i, (i, 1, 10))`, which resolves the same
collision by naming the constant `I` and refusing to bind it.

`e` and `pi` needed nothing here, being variables that carried a value — and were not fixed by it
either, for the same reason. The entry below does that.

[#976](https://github.com/asc-community/AngouriMath/issues/976).
### A derivative or an integral over something that is not a variable

Differentiating with respect to a *subexpression* is a feature
([#230](https://github.com/asc-community/AngouriMath/issues/230)): the subexpression is given a
name and the derivative is taken over the name, so `derivative((x + 1) ^ 2, x + 1)` is `2 * (x + 1)`.
The rename has two premises that were not checked. The subexpression has to be able to **vary** —
a number in that position was renamed and differentiated over, which answers a question with no
meaning:

```csharp
"derivative(x ^ 3, 3)".ToEntity().Simplify()   // was: ln(x) * x ^ 3       now: unevaluated
"integral(x ^ 2, 2)".ToEntity().Simplify()     // was: x ^ 2 / ln(x) + C   now: unevaluated
```

And the rename has to be **exact**, which means nothing of the subexpression's own variables may be
left behind. Where something is, the leftovers were read as independent of the name they came from:

```csharp
"derivative(x * (x + 1), x + 1)".ToEntity().Simplify()  // was: x   now: 1 + 2 * x
```

`x` is what you get by renaming `x + 1` to `z` and reading the other `x` as a constant. With
`z = x + 1` the expression is `(z - 1) * z`, whose derivative is `2z - 1`, which is `2x + 1`.

**Where the rename is not exact, this is a change of variables — and a change of variables needs no
occurrence at all.** `d f / d g` is `(df/dx) / (dg/dx)`, which is the substitution without having to
invert `g`:

```csharp
"derivative(x ^ 2, x + 1)".ToEntity().Simplify()   // was: 0   now: 2 * x
"derivative(x ^ 2, 2 * x)".ToEntity().Simplify()   // was: 0   now: x
"derivative(x ^ 2, sin(x))".ToEntity().Simplify()  // was: 0   now: 2 * x / cos(x)
"integral(x ^ 2, x + 1)".ToEntity().Simplify()     // was: 0   now: x ^ 3 / 3 + C
```

What still has no answer is a subexpression of **several** variables that the rename cannot take
exactly: `d(x + y)/d(x * y)` is `1/y` through `x` and `1/x` through `y`, so there is nothing to
answer without a direction. Those are the unevaluated node — the library's way of saying it could
not settle the question — rather than `0`.

A definite integral over a subexpression is unchanged: its range is stated in the new variable, and
converting it needs `g` inverted, which is a separate question.

**A vector in that position is now the gradient.** The elementwise broadcast every binder already
has was unreachable, because the rename matched first and answered `0`:

```csharp
"derivative(x ^ 2 + y ^ 2, [x, y])".ToEntity().Simplify()   // was: 0   now: [2 * x, 2 * y]
"integral(x, [x, y]T)".ToEntity().Simplify()
// was: [[C + x ^ 2, C + x * y]]      the first component is not the integral of x
// now: [[x ^ 2 / 2 + C, x * y + C]]
```

This is componentwise and no more than that: `derivative([x * y, x + y], [x, y])` pairs index with
index and is `[y, 1]`, the diagonal of the Jacobian rather than the Jacobian. What shape a full
Jacobian or Hessian should take is a convention this does not choose.

**A named constant can be differentiated over.** `derivative(e ^ 2, e)` was `0` — `e` is a variable
carrying a value, the value arrived in the variable position, and a number there took the rename
path. It is `2 * e`, and `derivative(pi ^ 2, pi)` is `2 * pi`. Two of the four defects recorded in
[#984](https://github.com/asc-community/AngouriMath/issues/984) go with it; the set builder and
`limit(e, e, 0).Evaled` remain.

[#964](https://github.com/asc-community/AngouriMath/issues/964).

### A name a binder declares is a variable, including `pi` and `e`

A mathematical constant used to be a `Variable` whose *name* was looked up in a table, so a `pi` a
binder declared and the constant `pi` were one object and nothing downstream could tell them apart.
It is now `Entity.Constant`, a node — so a binder holds a variable while the rest of the language
holds the constant, and every binder-based operation reads the same thing.

```csharp
"derivative(e ^ 2, e)".ToEntity().Simplify()   // was: 0             now: 2 * e_1
"derivative(pi ^ 2, pi)".ToEntity().Simplify() // was: 0             now: 2 * pi_1
"{ e : e > 0 }".ToEntity().Simplify()          // was: { e : True }  now: { e : e > 0 }
"limit(e, e, 0)".ToEntity().Evaled             // was: 2.718…        now: 0
"integral(e, e, 0, 1)".ToEntity().Evaled       // was: 3.694…        now: 1/2
```

**`pi_1`, not `pi`.** Most binders consume the name they declare — a sum over `pi` answers a number,
a set builder keeps it inside itself — and those print as they were written and read back as
themselves. A derivative and an indefinite integral *return* it, and a variable called `pi` is one
the parser cannot produce, so `2 * pi` would read back as twice the constant. Renaming a bound
variable is free, so it is renamed to a name that can be written. **This applies to `i` as well**,
and changes the two answers the entry above introduced: `derivative(i ^ 2, i)` is `2 * i_1` and
`integral(i, i)` is `i_1 ^ 2 / 2 + C`. Every binder over `pi`, `e` or `i` now answers something that
parses back to itself; an ordinary name is never renamed.

Differentiation compares the node rather than its name, so a constant that simplification
**produces** inside a binder over that name is no longer differentiated as though it were the index:

```csharp
"derivative(arccos(0) * pi, pi)".ToEntity().Simplify()  // was: 0                     now: pi / 2
"derivative(arccos(0) * q, q)".ToEntity().Simplify()    // pi / 2, unchanged — the same answer now
"derivative(ln(x) * e, e)".ToEntity().Simplify()        // was: 0 provided not x = 0  now: ln(x) provided not x = 0
```

A constant that simplification **produces** inside a binder over its name is no longer caught by it,
which was a wrong answer of a different kind — `arccos(0)` is `pi / 2`, and integrating it over a
name spelled `pi` integrated that:

```csharp
"integral(arccos(0), pi, 0, 1)".ToEntity().Simplify()  // was: 1/4   now: pi / 2
"integral(arccos(0), q, 0, 1)".ToEntity().Simplify()   // pi / 2, unchanged — the same answer now
```

`ln` and `exp` are written over Euler's number in their own definitions — `Ln(a)` is `Logf(e, a)`,
and `exp(x)` is `e ^ x` — and that occurrence is the value rather than a mention of the name, so no
binder reaches it:

```csharp
"sum(ln(x), e, 1, 2)".ToEntity().Simplify()   // was: log(1, x) + log(2, x)   now: 2 * ln(x)
"sum(exp(x), e, 1, 2)".ToEntity().Simplify()  // was: 1 + 2 ^ x               now: 2 * e ^ x
"sum(ln(e), e, 1, 1)".ToEntity().Simplify()   // was: NaN, being log(1, 1)    now: 0
```

Substituting **for the constant** still reaches that base, and unchanged — `MathS.Ln("x").Substitute("e", 3)`
is `log(3, x)` before and after. A binder binds *occurrences* and a substitution replaces a *value*,
and the base of a logarithm is Euler's number by value however it got there, so replacing Euler's
number by 3 replaces it there too. `ln(x).VarsAndConsts` has said `e, x` all along.

Where the writer does name `e`, it still binds, which is the same rule read the other way:

```csharp
"sum(log(e, x), e, 1, 2)".ToEntity().Simplify()  // log(1, x) + log(2, x)
"sum(e ^ x, e, 1, 2)".ToEntity().Simplify()      // 1 + 2 ^ x
```

**Two consequences to read for.** A bound name now counts as a variable, so `Vars` reports it:

```csharp
"sum(pi, pi, 1, 3)".ToEntity().Vars   // was: empty      now: pi
"pi * x".ToEntity().Vars              // x, unchanged — a free constant is still not a variable
```

And `MathS.pi` and `MathS.e` are `Entity.Constant` rather than `Entity.Variable`. They are still
*typed* `Variable` and `Constant` derives from it, so no signature changed and nothing that compiles
today stops compiling; but `MathS.Var("pi")` is a constant, and a `pi` a binder declares is no
longer equal to it. Substituting for the constant is unchanged where it was already right — a
binder's own name was out of reach before as well — and no longer reaches into a logarithm:

```csharp
"pi + x".ToEntity().Substitute(MathS.pi, 3)             // 3 + x, unchanged
"sum(pi, pi, 1, 3)".ToEntity().Substitute(MathS.pi, 3)  // unchanged before and after
```

Free `pi` and `e` are otherwise untouched: `sin(pi)` is `0`, `ln(e)` is `1`, `arccos(0)` is `pi / 2`,
and their numeric values are what they were.

[#984](https://github.com/asc-community/AngouriMath/issues/984).

---

## 2.2.0 — since 2.1.0

### At a glance

| Silent? | What | Was | Is |
|---|---|---|---|
| **silent** | `log(x, x)` simplified | `1 provided x > 0` — `NaN` at every negative `x`, and `1` at `x = 1` where the logarithm is `NaN` | `1 provided not x = 0 and not x = 1` |
| **silent** | `DomainCondition` of a logarithm, under the default reading | the real condition, so `log(-3, -3)` was declared undefined while evaluating to `1` | `not b = 0 and not b = 1 and not a = 0` |
| **silent** | `ln(x).DomainCondition` | `x > 0` | `not x = 0` |
| **silent** | `"x ^ n".Differentiate("x").Simplify()` | `x ^ n * n / x provided x > 0` — `NaN` at every negative `x` | `x ^ n * n / x provided not x = 0` |
| **silent** | `ln(a) + ln(b)` and `ln(a) - ln(b)` for symbolic `a`, `b` | `ln(a * b)`, `ln(a / b)` — wrong by `2*pi*i` off the positive reals | left as written |
| **silent** | `lim x->+oo (x^2)^x / e^(2*x*ln(x))` and `lim x->+oo x^x / e^(x*ln(x) - ln(x))` | unevaluated, the deliberate loss 2.1.0 recorded | `1` and `+oo`, the answers from before 2.1.0 |
| **silent** | `"x^5 + 2x^3 - 2x^2 - 4".SolveEquation("x")` | three of the five roots, one of them a float | all five, exact |
| | `"x^4 + x^2 + 1".SolveEquation("x")` | `sqrt((-1 - sqrt(-3)) / 2)` and its three companions | `(-1 - sqrt(-3)) / 2` and its three, with no nested radical |
| | `"x^4 + 3x^2 + 2".SolveEquation("x")` | `{ sqrt(-2), -sqrt(-2), i, -i }` | the same four, in the order the factors are found |
| **silent** | `"1/(x^4 + 3x^2 + 2)".Integrate("x")` | unevaluated | `arctan(x) - sqrt(2) * arctan(sqrt(2) * x / 2) / 2 + C` |
| **silent** | `"1/(x^4 + 4)".Integrate("x")` | unevaluated | the antiderivative over its two quadratic factors |
| **silent** | `"sin(-x) + sin(x)".Simplify()` | `sin(-x) + sin(x)` | `0` |
| **silent** | `"cos(-x)".Simplify()` and `"abs(-x)".Simplify()` | unchanged | `cos(x)`, `abs(x)` |
| **silent** | `"cos(0 ^ y)".InnerSimplified` | `-(-1) provided ...` | `1 provided ...` |
| | `Transformation.Rationalisation` and `RewriteRules.RationaliseDenominator` | spelled `-ise`, alone on a surface that is otherwise `Factorize`, `Normalization` | spelled `-ize`; a build error rather than a wrong answer |

### `InnerSimplified` is idempotent again

An exact trigonometric value reached through a half turn — `cos(x)` read off the table as
`-cos(pi - x)` — was built as a negation over the value it turned to and handed back unfolded. Where
the answer is then *wrapped* rather than rebuilt, nothing normalises it again:

```
"cos(0 ^ y)".InnerSimplified

was  -(-1) provided y / 2 * (1 + 1 / sgn(y) ^ 2) > 0
is       1 provided y / 2 * (1 + 1 / sgn(y) ^ 2) > 0
```

Both are the same value, so nothing was wrong — but applying `InnerSimplified` twice gave a different
tree from applying it once, and a great deal of the library treats what it hands back as settled.
`Simplify` was unaffected, since it normalises again anyway.

Found by `canoncheck`, the canonical-form harness: it was the only idempotence failure in 834
generated expressions, and the count is now zero.
[#930](https://github.com/asc-community/AngouriMath/issues/930).

### The parity identities are applied

`cos(-u) = cos(u)`, `sin(-u) = -sin(u)` and the rest of the family were absent, so an expression that
cancels exactly did not:

```
"sin(-x) + sin(x)".Simplify()

was  sin(-x) + sin(x)
is   0
```

Nothing false was being asserted, so this is a coverage change rather than a wrong answer put right —
but an expression that is identically zero was left standing, and anything testing a residual against
zero saw a non-zero residual where there was none.

The one case that already folded, `cos(-2 * x)`, folded by accident: the multiple-angle expansion
fires for a coefficient of magnitude two or more, and `-x` is a coefficient of `-1`, which it skips.
That is why `cos(-2 * x)` worked and `cos(-x)`, `sin(-2 * x)`, `tan(-2 * x)` and `abs(-2 * x)` did
not. What is matched now is a product with a negative real coefficient, which `-x` and `-2 * x` both
are.

Even: `cos`, `sec`, `abs`. Odd: `sin`, `tan`, `cotan`, `cosec`, `sgn`. Each holds on the whole
complex plane, and the poles of the odd ones sit symmetrically about zero — `tan(-z)` is undefined
exactly where `tan(z)` is — so **no condition is acquired**, which is pinned by its own test. Where a
cancellation is between two reciprocal functions the condition that was already there survives:
`tan(-x) + tan(x)` is `0 provided not cos(x) = 0`, which is right, since it is undefined at the poles
rather than zero there.

**A lone `sin(-x)` still prints as `sin(-x)` rather than `-sin(x)`**, and that is a tie rather than a
failure: both rate exactly 14 under the complexity criteria, and a tie goes to whichever candidate
was generated first. The identity is applied — it is what makes the cancellation above work — but it
does not win a comparison it was never going to win. The tests assert the cancellation for that
reason, rather than pinning the tie-break.

The inverse functions are deliberately untouched: `arcsin` and `arctan` are odd and `arccos` is not,
and this library's `arccotan` has range `(-pi/2, pi/2]` rather than the textbook `(0, pi)`, so each
wants measuring before anything is written down.
[#929](https://github.com/asc-community/AngouriMath/issues/929).

### A rational function is decomposed over the factors of its denominator, not only its roots

The other consumer of the polynomial layer, and the same blindness one level along. A partial
fraction decomposition split `N/D` at a **rational root** of `D`, so a denominator with no rational
root was left whole and its integral came back unevaluated — even where every factor was one the
integrator already reads. `x^4 + 3x^2 + 2` is `(x^2 + 1)(x^2 + 2)`, and the rule for a linear
numerator over a quadratic answers both halves.

```
"1/(x^4 + 3x^2 + 2)".Integrate("x")

was  integral(1 / (x ^ 4 + 3 * x ^ 2 + 2), x)
is   arctan(x) - sqrt(2) * arctan(sqrt(2) * x / 2) / 2 + C
```

The step is a coprime split rather than a full decomposition, which is what the step at a root
already was: the denominator is factored into irreducibles, one irreducible with its multiplicity is
taken against the product of the rest, the extended Euclidean algorithm in `Q[x]` gives `U*A + V*B =
1`, and `N/(A*B)` becomes `N*V/A + N*U/B` with each numerator reduced modulo its own denominator.
Both sides are strictly smaller problems of the same kind, so the integrator recurses into them and
arrives at the full decomposition either way.

**No condition is attached, and that is a statement rather than an omission.** `A` and `B` being
coprime, `A*B` is zero exactly where one of them is, so the two sides are undefined at the same
points. A decomposition that loses a singularity is one that cancels a shared factor, and this does
not cancel anything.

**What is still declined.** A denominator irreducible over `Q` — `x^4 + 1` is, and only factors once
real coefficients are allowed — and, more generally, any denominator one of whose factors is a shape
no integration rule reads: an irreducible of degree three or more, or a quadratic repeated. So
`(x^2 + 1)^2` is declined, and the ladder that would decompose it is deliberately not built, because
every term it produces is over `(x^2 + c)^k` and would come back unevaluated in turn.

*The `x^4 + 1` half of that is no longer true: allowing those real coefficients is exactly what the
entry below does, and `x^2/(x^4 + 1)` is now answered. The rest of the paragraph stands — an
irreducible of degree three or more, and a repeated quadratic, are still declined, and `(x^2 + 1)^2`
still is.*

Deciding that from the factorisation rather than by trying is what keeps the cost of declining to the
one factorisation. Splitting regardless and recursing made `(1 - x^4)/(1 + x^4 + x^8)`, whose
factorisation holds the irreducible quartic `x^4 - x^2 + 1`, take 18s to return the same unevaluated
integral it returns in 203ms — measured, and the reason the guard is there rather than a preference.

[#919](https://github.com/asc-community/AngouriMath/issues/919).

### A biquadratic denominator is decomposed over the reals

The same blindness one level further along, and the last place it reaches. The step above factors
over `Q` and stops where `Q` does, so `x^4 + 1` — irreducible over the rationals — was left whole
and `x^2/(x^4 + 1)` came back unevaluated. Over the reals it is
`(x^2 - sqrt(2)x + 1)(x^2 + sqrt(2)x + 1)`, and both halves are read by the rule for a linear
numerator over a quadratic. Nothing was missing but a factorisation the rational step is right to
refuse.

```
"x^2/(x^4 + 1)".Integrate("x")

was  integral(x ^ 2 / (x ^ 4 + 1), x)
is   -1/2 * sqrt(2) * 1/2 / 2 * ln(x ^ 2 + sqrt(2) * x + 1)
     + 1/2 * arctan((2 * x + sqrt(2)) * 1/2 * sqrt(2)) * 1/2 * sqrt(2)
     + 1/2 * sqrt(2) * 1/2 / 2 * ln(x ^ 2 - sqrt(2) * x + 1)
     + 1/2 * arctan((2 * x + -sqrt(2)) * 1/2 * sqrt(2)) * 1/2 * sqrt(2) + C
```

This is the integral [#233](https://github.com/asc-community/AngouriMath/issues/233) names as
wanting "partial fractioning", and it is the first of that issue's list to need a factorisation
rather than a rule. `1/(x^4 + 1)`, `1/(x^4 - 2)` and `1/(x^4 + 3x^2 + 1)` come with it.

**Biquadratic only, and that is a boundary rather than a first cut.** A general quartic factors into
real quadratics through its resolvent cubic, whose roots carry Cardano's nested radicals; a
biquadratic `x^4 + px^2 + q` is the case where the resolvent is solvable by inspection and the two
factors stay inside one square root. Two shapes come out of it, by the sign of `p^2 - 4q`: negative
gives `(x^2 + ax + b)(x^2 - ax + b)` with `b = sqrt(q)` and `a = sqrt(2b - p)`, and positive gives
the even `(x^2 + u)(x^2 + v)` with `u, v = (p -+ sqrt(p^2 - 4q))/2`. Zero is `(x^2 + p/2)^2`, a
repeated quadratic, declined for the reason the step above declines one. **A quartic with an odd
power in it — `x^4 + x^3 + 1`, `x^4 + x + 1` — is still declined**, and so is everything of degree
five and up that does not factor over `Q`.

**No condition is attached**, on the same argument as the step above: the two factors are distinct,
so their product is zero exactly where the original denominator is, and nothing is cancelled.

It is tried **after** both rational steps, which is what keeps a denominator that factors over `Q`
in exact arithmetic: `x^4 + 3x^2 + 2` is decomposed by the step above and never arrives here to be
given a square root it does not need. Declining stays as cheap as it was —
`(1 - x^4)/(1 + x^4 + x^8)` returns the same unevaluated integral in the same fraction of a second,
because every guard here is rational arithmetic on coefficients already read.

`sqrt(tan(x))`, the one remaining entry on #233's list, is **not** answered by this. It reduces
under `u = sqrt(tan x)` to `2 * integral(u^2/(u^4 + 1), u)`, which is now integrable — but the
substitution that gets there is a separate capability and is not built here. *It is built in the
entry below, and `sqrt(tan(x))` is now answered.*

[#233](https://github.com/asc-community/AngouriMath/issues/233).

### A fractional power, and the tangent, are substitutions

Two substitutions, in one entry because neither reaches `sqrt(tan(x))` without the other and
without the entry above. That integral is the last of the five
[#233](https://github.com/asc-community/AngouriMath/issues/233) lists, and the one it calls "very
painful, requires different solvers" — which it is. Three capabilities in a row:

```
int sqrt(tan(x)) dx
  --- u = tan(x),  dx = du/(1 + u^2)  ------->  int sqrt(u)/(1 + u^2) du
  --- t = sqrt(u), a fractional power  ------->  int 2t^2/(1 + t^4) dt
  --- 1 + t^4 factored over the reals  ------->  logarithms and arctangents
```

Take any one away and it comes back unevaluated.

```
"sqrt(tan(x))".Integrate("x")

was  integral(sqrt(tan(x)), x)
is   a sum of two logarithms and two arctangents in sqrt(tan(x)) — the shape the
     entry above produces, with sqrt(tan(x)) where it had x
```

**The fractional power.** A power substitution rewrites the other powers of the variable into
powers of itself: for `u = x^r` the identity is `x^n = u^(n/r)`, applied wherever `n/r` is a whole
number. A whole `r` reaches only the powers it divides, which is what this did before. An `r` of
`1/2` reaches every one of them — including the bare `x`, which a whole `r` never can — so
`int sqrt(x)/(1 + x^2)` becomes `int 2u^2/(1 + u^4) du`. `1/(1 + sqrt(x))`,
`1/(sqrt(x) * (1 + x))`, `1/(sqrt(x) * (1 + x^2))` and `1/(sqrt(x) + x)` come with it.

The rewrite is made in two passes, powers first and a leftover bare `x` second, because the tree
is rewritten from the leaves up: in one pass the `x` inside `sqrt(x)` is reached before the
`sqrt(x)` node is, and `u = sqrt(x)` turns it into `sqrt(u^2)` rather than `u` — an integrand free
of `x` and no more integrable than it started.

**The tangent.** An integrand that is a function of `tan(x)` and of nothing else becomes a
rational function under `u = tan(x)`, with `dx` as `du/(1 + u^2)`. The test is the rewrite itself:
replace every `tan(x)` and see whether an `x` survives. `tan(x) + x` keeps one and is declined,
which is right — it is answered, but by linearity over the sum.

This is a step of its own rather than a candidate for the general substitution, and the reason is
worth recording. The general one divides the integrand by `du/dx` and asks what is left, which
works while the substitution survives the division. Here it does not: `sqrt(tan(x))` over the
derivative of `sqrt(tan(x))` is `2 tan(x) cos(x)^2`, which is `sin(2x)` and is simplified to it —
a correct answer to a question that has stopped being about the tangent.

**What is still declined**, each by what the rewrite hands on rather than by the rewrite:
`cotan` is its own node rather than a reciprocal of the tangent, so `sqrt(cotan(x))` never starts;
`tan(x)^2` and `tan(x)^3` become improper fractions, and dividing an improper fraction out is not
something the rational integrator does; `1/(1 + tan(x)^2)` becomes `1/(1 + u^2)^2`, a repeated
irreducible quadratic. On the other side, `sqrt(x)/(1 + x^4)` becomes `2u^2/(1 + u^8)`, whose
denominator is neither factorable over the rationals nor a biquadratic.

**The rule for the tangent itself still wins**, being reached first: `int tan(x)` is
`-ln(cos(x)) + C` and not the longer thing this would produce.

**The corpus gate now reads 40 solved of 40.** Its one unsolved problem was `int:hard`, and
`int:hard` is `sqrt(tan(x))` — chosen for that list as the standing example of an integral out of
reach. The gate reached the same verdict independently, by differentiating the answer back rather
than by comparing it with anything.

[#233](https://github.com/asc-community/AngouriMath/issues/233).

### An improper quotient is divided out before it is decomposed

Every step of the rational integrator wants a **proper** fraction — a numerator of lower degree
than the denominator — and each of the three declines an improper one rather than dividing it out.
So `x^2/(x + 1)` had no antiderivative, although it is `x - 1 + 1/(x + 1)` and every piece of that
has been integrable throughout.

```
"x^2/(x + 1)".Integrate("x")

was  integral(x ^ 2 / (x + 1), x)
is   x ^ 2 / 2 + -x + ln(x + 1) + C
```

**The division is not new code.** `TreeAnalyzer.PolynomialLongDivision` has done it all along, for
the simplifier's own `PolynomialLongDivision` rule set; the integrator simply never asked it. What
changed is one call, placed before the three decompositions rather than after them.

New with it, as written: `x^3/(1 + x^2)`, `x^4/(x^2 + 1)`, `(x^5 + 2)/(x^2 + 1)`,
`(x^2 + 3x + 5)/(x + 2)`, and the exact-division cases `(x^3 + 1)/(x + 1)` and `(x^2 - 1)/(x - 1)`,
whose proper part is zero.

New with it **through a substitution**, which is where it matters more, since the substitution
produces the improper fraction rather than the user writing one: `tan(x)^2`, `tan(x)^3` and
`sqrt(x)/(x + 1)` were all declined for this and no other reason, each having been named as such
in the tests that recorded the boundary. `int tan(x)^2` comes back as
`tan(x) - arctan(tan(x)) + C` rather than the textbook `tan(x) - x`, which is the same function on
the principal branch and is what the rewrite has to say without an assumption about which branch
`x` is on.

**A proper fraction is untouched.** The helper answers nothing for one, so the three steps below
see exactly what they saw before: `int 1/(x + 1)` is still `ln(x + 1) + C` and `int x/(x^2 + 1)`
still `ln(x^2 + 1)/2 + C`.

**A symbolic leading coefficient is still declined.** `x^2/(a + b*x)` —
[#180](https://github.com/asc-community/AngouriMath/issues/180)'s item 18 — would have the division
divide by `b`, which is not decidably non-zero, and at `b = 0` the quotient is `x^2/a`, whose
antiderivative is not the limit of the divided form. `x^2/(x + a)` is declined too, for a narrower
reason: the leading coefficient there is `1`, but the helper does not divide a polynomial whose
other coefficients are symbolic.

[#180](https://github.com/asc-community/AngouriMath/issues/180).

### A system with fewer equations than unknowns is answered, not refused

`Solve` raised `WrongNumberOfArgumentsException` for a system with fewer equations than
unknowns — a message that says the caller called it wrongly, for a caller who did nothing wrong.
`2x - 4y = 12` in `x` and `y` is a well-formed question; it simply has infinitely many answers.

```
MathS.Equations("2*x - 4*y - 12").Solve("x", "y")

was  WrongNumberOfArgumentsException: Number of equations must be equal to that of vars
is   [[6 + 2 * t_1, t_1]]
```

which is `x = 6 + 2t, y = t` — the answer the issue asks for in its body.

**The answer type did not change.** A solution has always been a row whose i-th entry is the
i-th unknown's value, and nothing said those entries may not mention a variable. The unknowns a
row reduction leaves free become parameters, named the way the constant of integration is named
in an ODE's answer, and the rest are written in terms of them. The system from the issue thread,
three equations in five unknowns, comes back with two of them:

```
{ p + 2q + 4r + s - u = 1, 2p + 4q + 8r + 3s - 4u = 2, p + 3q + 7r + 3u = -2 }

is   [[7 + 2 * t_1 + 3 * t_2, -3 + (-3) * t_1 + (-2) * t_2, t_1, 2 * t_2, t_2]]
```

**Only the short count is taken this way.** A square system that is rank-deficient still reaches
the eliminator, which has answered it with a free parameter since
[#550](https://github.com/asc-community/AngouriMath/issues/550); those answers are unchanged, as
are every determined and every overdetermined system.

**A contradictory short system is `null`**, the same as a contradictory square one, and that
continues to mean *there are no solutions* rather than *no answer was found*.

**What still raises.** The system has to be linear in the unknowns, checked rather than assumed
— `x^2 + y^2 = 1` and `x*y = 1` fail the check — and the coefficients **on the unknowns** must be
rational, so `a*x + y = 1` raises too. That last is a soundness requirement rather than a
convenience: a row reduction has to decide whether a pivot is zero, the general test available
is structural, and choosing a pivot that is zero without being written as `0` produces a wrong
family rather than no answer. The **constant** term is under no such restriction, being never a
pivot, so `2x - 4y = k` is answered with `k` symbolic.

**Code that caught `WrongNumberOfArgumentsException` around `Solve`** to detect an
underdetermined system now gets a matrix for the linear ones, and the exception only for the
rest.

[#212](https://github.com/asc-community/AngouriMath/issues/212).

### A polynomial equation that factors is solved through its factors

A polynomial of degree four or more may factor over the rationals with no rational root anywhere in
it — `x^4 + 3x^2 + 2` is `(x^2 + 1)(x^2 + 2)` — and nothing in the solver could see that. The
rational-root split that runs first is by construction blind to it, and what followed was the
general machinery for the degree: Ferrari at four, and at five and above no general solution exists
to reach for. A real polynomial layer
([#746](https://github.com/asc-community/AngouriMath/issues/746) item 43) is what was missing, and
the equation solver is its first consumer: where the polynomial factors, each factor is now solved
as a lower-degree equation of its own.

The change is confined to degree four and up, and that is not a threshold chosen for caution. A
quadratic or a cubic that factors at all has a rational root — a cubic splits as a linear factor
times a quadratic, or into three linear factors, and either way there is a linear factor to find —
so the step that runs before this one has already divided it out. Four is the first degree at which
a polynomial can factor with nothing rational to catch. A two-termed `a*x^n + b` is also left alone,
for the reason already written down where the rational-root split declines one: inverting the power
answers it whole and in polar form.

**The one that was wrong rather than merely differently written.** An incomplete solution set is not
a partial answer, it is a false one — it says these are the roots.

```
"x^5 + 2x^3 - 2x^2 - 4".SolveEquation("x")

was  { sqrt(-2), -sqrt(-2), sqrt(1.5874010519681995834417875812505371868610382080078125) }
is   { sqrt(-2), -sqrt(-2), 2 ^ (1/3),
       (-1/2 + i * 1/2 * sqrt(3)) * 2 ^ (1/3), (-1/2 + i * -1/2 * sqrt(3)) * 2 ^ (1/3) }
```

The polynomial is `(x^2 + 2)(x^3 - 2)`. Two of its five roots were missing, and the one real root of
`x^3 - 2` came back as the square root of a float where `2 ^ (1/3)` is exact — an inexact answer to a
question that has an exact one. It also took 4.6 seconds and now takes under a quarter of one.

**The others are the same numbers, written better or written in a different order.** Factoring first
means each root comes out of a quadratic or a cubic rather than out of the resolvent of a quartic,
and the forms are correspondingly plainer:

```
"x^4 + x^2 + 1".SolveEquation("x")
was  { sqrt((-1 - sqrt(-3)) / 2), -sqrt((-1 - sqrt(-3)) / 2), ... }
is   { (-1 - sqrt(-3)) / 2, (-1 + sqrt(-3)) / 2, (1 - sqrt(-3)) / 2, (1 + sqrt(-3)) / 2 }

"x^4 + 3x^3 + 6x^2 + 5x + 3".SolveEquation("x")
was  { -3/4 + (-1/2 + -sqrt(-8)) / 2, ... }
is   { (-1 - sqrt(-3)) / 2, (-1 + sqrt(-3)) / 2, (-2 - sqrt(-8)) / 2, (-2 + sqrt(-8)) / 2 }
```

Both sets are unchanged as sets — the members were checked to be equal, not merely to look it. Where
a solution set is only reordered, as for `x^4 + 3x^2 + 2` and `2x^4 + 6x^2 + 4`, code that indexed
into the result rather than searching it will see different elements at the same positions.

### A sum of logarithms is no longer gathered unless that is exact

`ln(a) + ln(b) = ln(a*b)` is false off the positive reals. At `x = -3` the two sides differ by
`2*pi*i`, the turn of the argument the principal branch discards:

```
"ln(x) + ln(x+1)" at x = -3     1.7918 + 6.2832i
gathered to ln(x*(1+x))         1.7918              — the value that was returned
```

Both rules were applied unconditionally, and this was the last disagreement `boundcheck` reported.
It now reports **none**.

**Numbers still gather.** `ln(2) + ln(3)` is `ln(6)` and `ln(6) - ln(2)` is `ln(3)`, because there
the operands are decidably positive. What no longer happens is the same rewrite on a *symbol*,
which may be anything.

**And the identity is not lost where it was doing real work.** Taking a limit states where the
expression is going, and on a stated approach the sign of each operand is decidable — so the
gathering still happens inside a limit, exactly where it is exact. A sum needs both operands
positive; a difference needs only that their signs *agree*, since `ln` of a negative is
`ln|.| + pi*i` and that cancels in a difference while it would double in a sum. No limit answer is
lost, which was measured rather than assumed: withdrawing the rule without putting it back this way
does not cost coverage, it costs **termination**, because the limit machinery's own expansion
creates the pairs that only this puts back together.

From [#721](https://github.com/asc-community/AngouriMath/issues/721).

### An exponent under a logarithm is read again, where a limit says the base holds its sign

2.1.0 withdrew `log_b(a^c) = c * log_b(a)` from an undecided argument, because it is false off
`ln`'s principal strip, and recorded two limits as the price:

| | 2.1.0 | now |
|---|---|---|
| `lim x->+oo (x^2)^x / e^(2*x*ln(x))` | unevaluated | `1` |
| `lim x->+oo x^x / e^(x*ln(x) - ln(x))` | unevaluated | `+oo` |

Both are answered again, and by the same route the logarithm gathering above takes: a stated
approach. The identity needs `Im(c * ln a)` inside `(-pi, pi]`; a base that holds a positive sign on
the way to the destination makes `ln a` real, and an exponent that is real along the approach leaves
the product real, so there is nothing for the principal branch to discard. Both halves are
answerable while a limit is being read and neither is answerable to a simplifier reading an
expression on its own account — so **outside a limit the rule still declines**, and `ln(e^x)` is
still left as written. That half now has a test of its own, because widening the guard until an
ordinary `Simplify` applies the identity would restore the wrong answer
[#902](https://github.com/asc-community/AngouriMath/issues/902) reported while every limit above
kept passing.

The exponent's realness is decided structurally and conservatively rather than read off a limit: a
positive *limit* does not make a base real on the way to it, since `x + i*sin(x)` tends to `+oo` off
the real line. A power is admitted only with a whole exponent or a decidably positive base, because
`(-2)^(1/2)` is imaginary; a second variable carries no approach and is refused. Anything unlisted
is refused too, which costs coverage and never correctness.

**What this corrects is a recorded prediction, not only a lost answer.** 2.1.0's entry said these two
wanted "an assumption travelling with the expression — `#746`'s tier 1 and the subject of
[#721](https://github.com/asc-community/AngouriMath/issues/721) — and not another pass", on the
strength of three insertion points that were each tried and each failed. That measurement was
sound and the conclusion drawn from it was not: all three were *pre-passes*, which rewrite the
expression before `Simplify` is called and cannot reach the candidate search that rebuilds it. An
ambient scope is not a pass. The rule itself asks whether an approach is being read, so it is
answered wherever the rule is asked — including 117 times inside one l'Hopital descent. No
assumption mechanism arrived, and none was needed.

`boundcheck` stays at **0** disagreements, and the suite finishes in its usual time — the failure
mode to watch for here is a hang rather than a wrong answer, since the limit machinery's own
expansion creates the shapes these rules put back.

### The logarithm's domain follows the reading, as every other node's already did

`Arcsinf`, `Arccosf`, `Arcsecantf` and `Arccosecantf` each state one condition over the reals and
another over the complex plane, and pick between them by the reading. `Logf` stated the real one as
though it were the only one, which is what
[#890](https://github.com/asc-community/AngouriMath/issues/890) reported as a contradiction: the
library gave an expression a value and declared it undefined at the same time.

```
log(-3, -3)                        1          — unchanged
log(-3, -3).DomainCondition        False  ->  True
ln(x).DomainCondition              x > 0  ->  not x = 0
```

Over the complex plane `log_b(a)` is `ln(a) / ln(b)`, which asks only that both logarithms exist
and that the denominator is not zero — no complex number has `0` as its exponential, so `a` and `b`
must be nonzero, and `ln(b) = 0` is `b = 1`. Negative and non-real arguments are admitted:
`log(2, -8)` is `3 + 4.53i` and was declared undefined.

**The real condition is still there and still reachable.** `expr.WithCodomain(Domain.Real)` gives
`x > 0 and not x = 1` for `log(x, x)` exactly as before; what changed is which of the two is the
default, and the default is now the same complex reading the evaluator has always used.

### `log_b(b) -> 1` carries the condition it has rather than one written into the rule

The rewrite asserted `provided x > 0`, and that was wrong in both directions at once — it withdrew
an answer that exists and kept one that does not:

```
"log(x, x)".Simplify()             1 provided x > 0  ->  1 provided not x = 0 and not x = 1
  at x = -3                        NaN  ->  1      (the logarithm is 1)
  at x = 1                         1    ->  NaN    (the logarithm is NaN)
```

`boundcheck` reports one disagreement where it reported two. The survivor,
`ln(x) + ln(x+1) -> ln(x*(1+x))`, needs an assumption travelling with the variable and is
untouched here.

### The derivative of a symbolic power loses a condition it never needed

`d/dx x^n` goes through `ln(x) * 0`, and `anything * 0 = 0` carries the operand's domain condition.
That condition was the logarithm's real one, so the derivative was undefined at every negative `x`:

```
"x ^ n".Differentiate("x").Simplify()   x ^ n * n / x provided x > 0
                                    ->  x ^ n * n / x provided not x = 0
  at n = 3, x = -2.5                 NaN  ->  75/4   (and 3x^2 at -2.5 is 75/4)
```

The condition does not vanish: `ln(0) * 0` is `-oo * 0`, which is `NaN`, so `not x = 0` is kept.

From [#721](https://github.com/asc-community/AngouriMath/issues/721) and
[#890](https://github.com/asc-community/AngouriMath/issues/890), in PR
[#916](https://github.com/asc-community/AngouriMath/pull/916).

### Three members spelled `-ise` are spelled `-ize`

The rest of the surface is `Factorize`, `Latexize`, `Stringize`, `Normalization`, `Factorization`.
Three members added in 2.1.0 were not, so the same operation had two spellings depending on which
one a caller reached for:

| Was | Is |
|---|---|
| `Transformation.Rationalisation` | `Transformation.Rationalization` |
| `RewriteRules.RationaliseDenominator` | `RewriteRules.RationalizeDenominator` |
| `Patterns.RationaliseDenominator` (internal) | `Patterns.RationalizeDenominator` |

**If you recompile, this breaks a build rather than an answer** — the compiler names the missing
member. Two ways it can reach you later than that:

- `AssemblyVersion` is pinned at `2.0.0.0` for the whole of 2.x precisely so that a consumer can
  drop a new DLL in without a binding redirect. Do that without recompiling, and a call to
  `Transformation.Rationalisation` throws `MissingMethodException` when it is *reached*, not when
  the assembly loads. That is the one path on which this behaves like a silent change.
- The rule set's `Name` comes from `nameof`, so a caller matching on the string
  `"RationaliseDenominator"` now sees `"RationalizeDenominator"` and simply stops matching.

Recompiling against 2.2.0 turns both into compile errors, except the string.

Documentation prose keeps British spelling throughout — `Factorize` has always sat beside the word
"factorisation" and still does. The convention is about identifiers.

Recorded here rather than left alone because a tag is public whether or not anyone has taken it. The
two canonical-form members added in this release were renamed before they ever shipped, so they are
not in the table.
In PR [#940](https://github.com/asc-community/AngouriMath/pull/940).

---

## 2.1.0 — since 2.0.0

Everything here landed **after** the 2.0.0 tag, so it is not in the package a reader of that
section already has. Almost all of it is a wrong answer becoming a right one, found by the
boundary and crash harnesses rather than reported; two answers are withdrawn deliberately and say
so, and one word — `NaN` — is reserved.

Performance against 2.0.0 is measured and published as a pair in
[`Docs/WhatsNew/version_performance_control.md`](Sources/AngouriMath/Docs/WhatsNew/version_performance_control.md):
no regression, the largest real move being `SolveMedium` at +4.6%.

### At a glance

| Silent? | What | Was | Is |
|---|---|---|---|
| loud | `NaN` as a variable name | a variable | a keyword, so the NaN value |
| loud | `MathS.ToSympyCode` of any non-integer rational | `SyntaxError` — a parenthesis was never closed | code that runs |
| **silent** | `MathS.ToSympyCode` of `1/2`, `2^(-1)` | ran, and gave the float `0.5` | `1/2`, exact |
| loud | `MathS.ToSympyCode` of `NaN`, `+oo`, `-oo` | `NameError` — the name is never bound | `sympy.nan`, `sympy.oo`, `-sympy.oo` |
| **silent** | `NaN` printed and read back | a variable of that name, which cancels and collects | the NaN value |
| loud | `Compile` of `floor`, `ceil`, `round`, `phi`, `gamma`, `!` | `AngouriBugException`, asking to be reported | `UncompilableNodeException` |
| loud | `Compile` of a boolean or lambda node as `double` | `InvalidOperationException` from Linq | `UncompilableNodeException` |
| **silent** | `abs(sgn(x))` and `sgn(abs(x))` | `1`, wrong at `x = 0` where both are `0` | left as written unless the argument's value can be read |
| **silent** | `ln(e^x)`, `log(2, 2^x)`, `ln(x^2)` | `x`, `x`, `2 * ln(x)` — wrong off the real line | left as written unless the argument is decidable |
| **silent** | two limits over `(x^2)^x` and `x^x` | answered correctly | unevaluated — a deliberate loss |
| **silent** | `DirectChildren` of a conditional set | a name off the predicate's hash, and one in 26^4 threw | `%1`, fresh by construction |
| **silent** | `-(a - b)` inside a power, a function or a matrix | left as written | `b - a`, as at the root |
| **silent** | `Expand` of a matrix | the matrix, unexpanded | expanded entry by entry |
| **silent** | `false and u`, `true or u`, `false implies u` for an undefined `u` | `NaN` | `False`, `True`, `True` — what the truth table settles |
| **silent** | a connective over a number, e.g. `true and 0`, `false and 0` | `NaN`, or `False` by short-circuit | left as written — a number is not a truth value |
| **silent** | `arctan(x) + arccotan(x)` | `pi/2`, wrong for every negative `x` | `pi/2` or `-pi/2` where the sign is known, else left as written |
| **silent** | `log(1, 1)` | `0` | `NaN`, since it is `0/0` |
| **silent** | `log(b, 1)` | `0` for any base | `0 provided not b = 1` |
| **silent** | `log(1/2, 0)` and any base below 1 | `-oo` | `+oo` |
| **silent** | `abs(-sqrt(6))`, `abs(-pi)`, `abs(1 - sqrt(2))` | left as written | `sqrt(6)`, `pi`, `sqrt(2) - 1` |

### `Compile` fails with its own exception rather than someone else's

Two families, both reported as `UncompilableNodeException` — the exception
[`Docs/Usage/Exceptions.md`](Sources/AngouriMath/Docs/Usage/Exceptions.md) documents for a node with
no compiled form. Neither the node set nor the compiled output changes; only what is thrown when
compilation is impossible.

| input, compiled as `<double, double>` | was | is |
|---|---|---|
| `floor(x)`, `ceil(x)`, `round(x)`, `phi(x)`, `gamma(x)`, `x!` | `AngouriBugException`: *An unary node seems to be not added* | `UncompilableNodeException` |
| `not x`, `x and 2`, `x or 2`, `x xor 2`, `x implies 2`, `x -> x + 1` | `InvalidOperationException` from `System.Linq.Expressions` | `UncompilableNodeException` |
| `x provided 2` | `ArgumentException`: *Argument must be boolean* | `UncompilableNodeException` |

`AngouriBugException` means "an internal error occurred, report it", and none of the first row is one
— the converter has no case for the node, which is a gap in coverage. **Four of those nodes are ones
2.0 added**: `floor`, `ceil` and `round` arrived with #809 and the compiler was never taught them, so
a caller compiling a 2.0 feature was asked to file a bug report.

The second family matters for a caller who followed the documentation: `catch (AngouriMathBaseException)`
is what the exception reference tells you to write around a library call, and Linq's exceptions are
not under it, so those escaped the handler entirely.

**What this does not do** is teach the compiler `floor`, `ceil`, `round` and the rest. That is worth
doing and is separate; this is only about the failure being honest rather than either a request for a
bug report or an exception nobody was told to expect. Issue
[#894](https://github.com/asc-community/AngouriMath/issues/894).

Found by `crashcheck`, which runs each case in a child process so that a crash is a result rather
than the end of the run. On 1652 cases it reports 0 crashes and 0 hangs, and these 16 were every
remaining finding.

### A logarithm behaves like the division it is defined as

`log_b(z)` is `ln(z) / ln(b)`, and three answers did not follow from that. Every division by zero in
this library is `NaN` — `0/0`, `2/0` and `-2/0` all are — so a logarithm whose base is `1` divides by
`ln(1) = 0` and must be `NaN` too.

| | was | is |
|---|---|---|
| `log(1, 1)` | `0` | `NaN` — it is `0/0` |
| `log(1, 2)` | `+oo` | `NaN` — dividing by `ln 1 = 0` has no signed answer |
| `log(b, 1)` | `0`, for any base including 1 | `0 provided not b = 1` |
| `log(1/2, 0)`, and any base below 1 | `-oo` | `+oo` |
| `log(2, 0)`, and any base above 1 | `-oo` | `-oo`, unchanged |
| `log(x, 0)` for symbolic `x` | `-oo` | left as written |

The first two came from a shortcut: `Number.Log` uses `EDecimal.LogN` for a positive real base, and
`LogN` answers `0` for `log_1(1)` and `+oo` for `log_1(2)` where falling through to `Ln(x)/Ln(base)`
gives `NaN` for both. A base of `1` is now excluded from the shortcut so the two paths agree.

`log(b, 1)` is `0/ln(b)`, which is `0` for every base but `1`. **A condition is right here**, unlike
the interval cases above: at `b = 1` the expression genuinely is undefined rather than merely
something else, so narrowing the domain is what the mathematics says.

`log(b, 0)` is `-oo/ln(b)`, so the sign of the answer follows the sign of `ln(b)`. It answered `-oo`
for every base, which is wrong below `1`. For a base that cannot be placed on one side of `1` there is
no signed answer to give, so the node is left as written.

Issue [#890](https://github.com/asc-community/AngouriMath/issues/890), which also records a second
half not fixed here: `DomainCondition` of `log(-3, -3)` is `False` while the expression evaluates to
`1`, so the declared domain and the evaluation disagree. That is #721's question and wants a decision.

### `arctan(x) + arccotan(x)` is not always `pi/2`, and `arccotan(cotan(x))` was guarded wrongly

Both follow from one fact about this library's `arccotan`: it is `arctan(1/x)` extended by
`arccotan(0) = pi/2`, so its **range is `(-pi/2, pi/2]`** and not the `(0, pi)` that many textbooks
use. Measured: `arccotan(1)` is `pi/4`, `arccotan(-1)` is `-pi/4`, `arccotan(0)` is `pi/2`.

| | was | is |
|---|---|---|
| `arctan(-3) + arccotan(-3)` | `pi/2` | `-1/2 * pi` — which is the value |
| `arctan(3) + arccotan(3)` | `pi/2` | `pi/2`, unchanged |
| `arctan(x) + arccotan(x)` for symbolic `x` | `pi/2` | left as written |
| `arccotan(cotan(2))` | `2` | `-1.1416...` — which is the value, `2 - pi` |
| `arccotan(cotan(-1/2))` | left as written | `-1/2` |

The sum is `pi/2` for a non-negative real argument and `-pi/2` for a negative one. `pi/2 * sgn(x)`
is the closed form and is wrong at exactly one point — at `x = 0` the sum is `pi/2` while `sgn(0)`
is `0` — so the sign is decided where it can be read and the sum is left alone otherwise. A
`Piecewise` would be total, but `Compile` throws `UncompilableNodeException` on one, so answering
that way would break expressions that compile today.

**The second row is a correction to the release before it.** 2.0.0 guarded
`arccotan(cotan(x))` with `[0, pi]`, on the assumption that `arccotan`'s range was `(0, pi)`. That
admitted `(pi/2, pi)`, where the rewrite is false, and refused `(-pi/2, 0)`, where it is true. The
interval is now `(-pi/2, pi/2]` without zero — zero excluded because `cotan` has no value there, so
the composition has none either and rewriting to `x` would invent one. The other three intervals
from that change check out: `arcsin` is `[-pi/2, pi/2]`, `arccos` is `[0, pi]`, `arctan` is
`(-pi/2, pi/2)`.

`arcsin(x) + arccos(x) -> pi/2` is **unchanged and needs no assumption**, since `arccos(x)` is
`pi/2 - arcsin(x)` by definition over the whole plane.

Three tests moved. `SimplifyTest.Patt8` and `ArctanIdentitiesTest.NeighbouringIdentitiesAreUnaffected`
asserted `pi/2` for a symbolic argument and were pinning the wrong answer; both now use numbers and
cover each sign. `SortSimplifyTest`'s `arctan(x2) + arccot(x*x)` case sorted and collected its whole
sum only because the collapse shortened it, so with the collapse gone the sum stays as written —
the sibling `arcsin`/`arccos` case still collapses and still sorts.

Found by `boundcheck`, a harness that composes every unary function node with every other and
compares against the original at points where an assumption fails rather than at sampled points.
Issue [#887](https://github.com/asc-community/AngouriMath/issues/887).

### An exponent is no longer pulled out of a logarithm over an undecided argument

`log_b(a^c) = c * log_b(a)` holds where `c * ln(a)` stays inside the strip `Im in (-pi, pi]` that `ln`
maps onto. It was applied to any argument at all, and the rewritten form is shorter, so it is what an
ordinary caller got:

| | was | is |
|---|---|---|
| `ln(e^x)` | `x` | left as written |
| `log(2, 2^x)` | `x` | left as written |
| `ln(x^2)` | `2 * ln(x)` | left as written |
| `ln(e^3)`, `log(2, 2^5)` | `3`, `5` | unchanged |
| `ln(e^x)` under `Codomain.Set(Domain.Real)` | `x` | `x`, unchanged |

`ln(e^x) -> x` is wrong wherever `Im x` leaves that strip. At `x = 3*pi*i` the expression is `pi*i`,
because `e^(3*pi*i)` is `-1`, while `x` is `9.4247...i` — the two differ by exactly the full turn the
principal branch discards. `MathS.Settings.Codomain` defaults to `Domain.Complex`, so this was unsound
on the library's own default reading. It is also unsound for a negative real base: `log(2, 64)` is `6`
where `2 * log(2, -8)` is `6 + 9.0647...i`.

The rule now asks for a base that is decidably a positive real, and an exponent that may be taken as
real — because the reading is real analysis, because the node's declared codomain says so, or because
its value is a real. A symbolic exponent under the default complex reading is none of those, so the
expression is left as written: decide, or decline, as with the four inverse-trigonometric rules above.

**Two limits are lost, and that is the cost of this entry rather than an oversight.**

| | was | is |
|---|---|---|
| `lim x->+oo (x^2)^x / e^(2*x*ln(x))` | `1` | unevaluated |
| `lim x->+oo x^x / e^(x*ln(x) - ln(x))` | `+oo` | unevaluated |

Both are right answers becoming no answer, which this file has recorded before for two integrals, and
which the ordering in [AGENTS.md](AGENTS.md) prefers to a wrong answer reachable from `ln(e^x)`. They
are unevaluated rather than `NaN`: the caller is told nothing was settled, not that the limit does not
exist.

They want the identity that was just removed. `d/dx (x^2)^x` carries `ln(x^2)`, and l'Hopital's rule
reached it through `Simplify`. On the way to `+oo` the base genuinely is positive, so the identity is
true there — the limit machinery simply has no way to say so to the simplifier. Supplying it from the
limit side was tried and does not reach: rewriting the expression before `Simplify` is called does pull
the exponent out, and `Simplify`'s own candidate search then writes `(x^2)^x` back into a logarithm and
needs the identity again. It is load-bearing *inside* the search, so what would restore these two is an
assumption travelling with the expression — `#746`'s tier 1 and the subject of
[#721](https://github.com/asc-community/AngouriMath/issues/721) — and not another pass. The two rows
have their own test asserting the unevaluated node, so a future fix flips them back deliberately.

`boundcheck` drops from four disagreements to two; the remaining two are `log(x, x)` and
`ln(x) + ln(x+1)`, both recorded elsewhere as wanting a decision rather than a guard. Issue
[#902](https://github.com/asc-community/AngouriMath/issues/902).

### A logical connective over a number has no answer

A number is not a truth value: `0` is not `False` and `1` is not `True`. The same category error was
reported three different ways, depending on which operator saw it and in which position:

| expression | was | is |
|---|---|---|
| `true and 0`, `false or 0`, `false xor 0` | `NaN` | left as written |
| `false and 0`, `true or 0` | `False`, `True` — by short-circuit | left as written |
| `true xor 0` | `not 0` | left as written |
| `not 0`, `0 xor 0` | left as written | unchanged |
| `true and 1`, `true and 1/2`, `true and i` | `NaN` | left as written |

**`NaN` was the worst of the three.** In this library it means *this does not exist*, and `true and 0`
exists perfectly well — it is not a proposition. Reporting a sort error as nonexistence tells a caller
something false about the mathematics, and it is the answer an `if` on `EvaluableBoolean` does not save
you from. The other two were an answer arrived at by not looking, and a rewrite of one non-boolean into
another.

**Two answers are withdrawn deliberately.** `false and 0` and `true or 0` used to come back `False` and
`True`, because short-circuiting settled them before the number was examined. Those are gone: whether an
operand is admissible cannot depend on whether the operator happened to need it. There is no proposition
here for `False` to be the truth of.

A caller who insists on a boolean is unaffected — `EvalBoolean()` throws `CannotEvalException` for an
unevaluated node exactly as it did for `NaN`, so the type error still surfaces at the layer where
insisting happens. `DomainCondition` stops contradicting evaluation as a side effect: `domain(a xor 0)`
says `True`, and there is no longer a `NaN` for it to disagree with.

**A number is not the same as an undefined truth value, and the two must not be treated alike.** `NaN`
is how this library spells *no truth value* — what an order comparison over the complex plane produces —
and a connective settles what it can with one, which is the entry above. So `(0/0) and False` and
`(i < 0) and False` are both still `False`, and only a genuine number makes a connective decline. A bare
variable is `Domain.Any` and may yet be a truth value, so `false and x` is still `False`.

Issue [#897](https://github.com/asc-community/AngouriMath/issues/897), which asked whether a number is
a truth value here at all. It is not; a truth value that is neither true nor false is what
`MathS.Quantum` is for.

### A logical connective is no longer strict in `NaN`

`Simplify` and evaluation disagreed about three-valued logic. `Simplify` gave the Kleene answer and
evaluation absorbed everything into `NaN`, so the two contradicted each other on the same expression:

```
"True or (True and (x < 0))".Simplify()          ->  True
the same, at x := i, evaluated as written        ->  NaN      (was)
                                                 ->  True     (is)
```

`i < 0` has no truth value — the default codomain is `Domain.Complex` and the complex numbers are not
ordered — so it evaluates to `NaN`. What changed is what a connective does with such an operand.

| expression | was | is |
|---|---|---|
| `(i < 0) and False` | `NaN` | `False` |
| `(i < 0) or True` | `NaN` | `True` |
| `False implies (i < 0)` | `NaN` | `True` |
| `(i < 0) implies True` | `NaN` | `True` |
| `(i < 0) and True` | `NaN` | `NaN`, unchanged |
| `(i < 0) or False` | `NaN` | `NaN`, unchanged |
| `not (i < 0)` | `NaN` | `NaN`, unchanged |
| `(i < 0) xor (i < 0)` | `NaN` | `NaN`, unchanged |
| `(0/0) * 0`, `(0/0) + 1` | `NaN` | `NaN`, unchanged |

The rule is the ordinary one for three-valued logic: an operand with no truth value cannot change an
answer the table settles without it, and where the answer does depend on it the result stays `NaN`.
**Arithmetic is untouched** — `NaN` still absorbs there, which is why this is opted into per node
rather than changed for everything: a rule for a zero factor exists, and `NaN * 0` must not reach it.

The tables were already three-valued. `Andf` reads `(_, Boolean(false))` as `False` and
`(Boolean(true), _)` as its right operand, which is Kleene as written; what overrode them was one line
in the shared `ExpandOnTwoArguments`, `if (left.IsNaN || right.IsNaN) return MathS.NaN;`, running
*before* the table was consulted. The connectives now get first refusal on an undefined operand and
hand back `null` where they cannot settle it, which is what still reaches `NaN`.

**One consequence to know about.** For `x < 0 and x = 0` the evaluator now settles `False` for every
`x`, since `x = 0` is decidably false at `x = i` and `False and u` is `False`. `Simplify` answers
`False provided x in RR`, whose condition
([#876](https://github.com/asc-community/AngouriMath/issues/876)) is over-strong for that row: the
reduction needs one conjunct false, not both operands real. So `Simplify` is now weaker than evaluation
there rather than stronger. It is recorded in a test rather than fixed here, because the rules #876
conditioned want going through one at a time.

Issue [#880](https://github.com/asc-community/AngouriMath/issues/880), which set this out as a fork
between Kleene and strict evaluation and left it open for want of a measurement. The measurement: one
assertion in the suite changed, and it was that issue's own guard clause.

### A negated difference is turned round wherever it sits, and `Expand` descends into a matrix

`-(a - b)` became `b - a` for a whole expression and not for the same expression inside another node,
so what a caller got depended on where the subexpression sat:

| input | was | is |
|---|---|---|
| `-(5 - sqrt(-11))` | `sqrt(-11) - 5` | unchanged |
| `-(5 - sqrt(-11)) + y` | `y - (5 - sqrt(-11))` | `sqrt(-11) - 5 + y` |
| `2 ^ (-(5 - sqrt(-11)))` | left as written | `2 ^ (sqrt(-11) - 5)` |
| `sgn(-(5 - sqrt(-11)))` | left as written | `sgn(sqrt(-11) - 5)` |
| `[[-(5 - sqrt(-11)), 1]]` | left as written | `[[sqrt(-11) - 5, 1]]` |
| `Expand` of `[[(x+1)^2, 1]]` | `[[(x + 1) ^ 2, 1]]` | `[[1 + 2 * x + x ^ 2, 1]]` |

Every one of these was already the right number, written the long way round. The step came from
`Expand`, which the simplifier offers for the **root** expression only and which does not descend into
an exponent, a function's argument or a matrix. Written as a rule instead — a unary minus parses as
`(-1) * x`, so `(-1) * (a - b)` becomes `b - a`, which is five nodes for three — it runs wherever the
shape occurs, since a rule walks the tree.

`Expand` of a matrix is the same story from the other end: it read its argument as a sum, and a matrix
is not one, so a matrix left through the "too complicated, return what came in" exit. It now expands
entry by entry. `Factorize` and `Differentiate` both already descended, being built out of rewrite
rules, so this was `Expand` being the odd one out rather than matrices being held back deliberately.

**Where a caller meets it.** `EquationSystem.Solve` returns a `Matrix`, so a solved system's entries
were left in whatever form the solver built them in:

```
MathS.Equations("x2 + y", "y - x - 3").Solve("x", "y").Simplify()

was:  [[-(-(5 - sqrt(-11)) / 2 + 3), (5 - sqrt(-11)) / 2], [-(-(5 + sqrt(-11)) / 2 + 3), (5 + sqrt(-11)) / 2]]
is:   [[-1/2 + -1/2 * sqrt(-11), 5/2 + -1/2 * sqrt(-11)], [1/2 * sqrt(-11) + -1/2, 1/2 * sqrt(-11) + 5/2]]
```

Issue [#882](https://github.com/asc-community/AngouriMath/issues/882), which
[#497](https://github.com/asc-community/AngouriMath/issues/497) names as the shape of defect to hunt:
the same input simplifying or not depending on its parent.

### A conditional set's bound variable is renamed to a temporary

`DirectChildren` of `{ x : P(x) }` renames the binder, so that the bound `x` is not read as an `x`
that may be free outside the set. The replacement name was four lowercase letters derived from the
predicate's hash code, and it was produced through `MathS.Var`, which **parses** — so a name that the
parser reads as something other than a variable did not come back as one.

| | was | is |
|---|---|---|
| `new ConditionalSet("x", "x > 0").DirectChildren[0]` | `abcd > 0`, a different name in every process | `%1 > 0` |
| one predicate in `26^4` | `CannotParseInstanceException: Cannot parse an instance of Variable from `true`` | `%1 > 0` |

Four lowercase letters can spell `true`. They can also spell a variable the predicate already uses, in
which case the rename captures it and the set means something else — silently, with no exception to
say so. The name now comes from `Variable.CreateTemp`, which reads the predicate's variables rather
than its hash and hands back `%1` upward, skipping what is taken: fresh by construction, and with no
reading as anything but a variable, since the parser does not accept `%`.

**Printed output does not change.** `Stringize` reads the node's own fields, so `{ x : x > 0 }` still
prints with its own binder, and `Solve` answers containing a conditional set are unaffected. Nothing
could have depended on the old name either, since string hash codes are randomised per process and it
was therefore different on every run — which is also why this surfaced as a CI failure that would not
reproduce.

`Variable.CreateRandom`, which had this one caller, is removed. It was `internal`.

Issue [#891](https://github.com/asc-community/AngouriMath/issues/891).

### `abs(sgn(x))` and `sgn(abs(x))` are not `1` at zero

`|sgn(z)|` and `sgn(|z|)` are `1` for every `z` except `0`, where both are `0`, because `sgn(0)` is
`0`. Both rewrites answered `1` for any argument.

| | was | is |
|---|---|---|
| `abs(sgn(x))`, `sgn(abs(x))` | `1` | left as written |
| the same at `x = 0` | `1` | `0` — which is the value |
| `abs(sgn(0))`, `sgn(abs(0))` | `0`, unchanged | |
| `abs(sgn(2))`, `sgn(abs(-3))` | `1`, unchanged | |
| `signum(abs(x/x))`, `abs(signum(x/x))` | `1 provided not x = 0` | left as written |

The rules consulted the argument's `DomainCondition`, which answers a different question from the one
they needed: a bare `x` is defined everywhere and can *be* zero, while `x / x` is defined only away
from zero and is nonzero throughout. Reading the domain conflated the two.

**A condition would not have been the fix**, for the same reason as in the two entries above: the
expression is defined at zero and equal to `0` there, so `1 provided not z = 0` would replace a wrong
value with a wrong domain. There is no closed form for "1 away from zero, 0 at it" other than the
expressions themselves, so the rules decide where the argument's value can be read and decline
otherwise.

**The last row is a coverage loss, not a correction.** `1 provided not x = 0` was right for `x / x`.
Separating it from the `x` case needs "nonzero throughout its domain", and neither test available in
`InnerSimplify` can express it — `Evaled` leaves `x / x` as `x / x`, and `DomainCondition` is what
conflated them; `Simplify` would answer it and cannot be called from there (#403).

Found by `boundcheck` after adding `x = 0` and `x = 1` to its sample points: 366 shapes had been
passing at 23 points chosen for branch cuts and principal intervals, none of which was the place
where a rule's own arithmetic degenerates. Issue
[#892](https://github.com/asc-community/AngouriMath/issues/892).

### `abs` folds where the sign of its argument is known

`|x|` is `x` for a non-negative real `x` and `-x` for a negative one. That is the definition of the
function rather than an identity with a side condition, and it was applied only when the argument
was a *number*. An argument whose value is a known real without its node being a number was left
alone, so a radical or a constant kept its `abs`:

| | was | is |
|---|---|---|
| `abs(-sqrt(6))` | left as written | `sqrt(6)` |
| `abs(-pi)`, `abs(-e)` | left as written | `pi`, `e` |
| `abs(1 - sqrt(2))` | left as written | `sqrt(2) - 1` |
| `abs(-2)` | `2` | `2`, unchanged |
| `abs(sqrt(-4))` | `2` | `2`, unchanged — the magnitude of `2i` |
| `abs(-a)` for symbolic `a` | left as written | left as written |

Where this shows up is in an answer built out of radicals. `(2x^2 - 3 > 0) and (x > 0)` solved to
`(abs(-sqrt(6)) / 2; +oo)` and now solves to `(sqrt(6) / 2; +oo)`; the endpoint was always the same
number, printed in a form that looked like unfinished work. The
[Solvers wiki page](https://github.com/asc-community/AngouriMath/wiki/Solvers) shows the old output
and wants updating with the release.

**Nothing is assumed about a symbol**, and an argument off the real line is declined rather than
guessed at: `sqrt(-4)` evaluates to `2i`, whose absolute value is `2` — neither the argument nor its
negation, so a rule that read "negative, therefore negate" would be wrong there. The sign is read
off the value, and a value that is not a finite real does not answer the question.

Issue [#881](https://github.com/asc-community/AngouriMath/issues/881).

### `MathS.ToSympyCode` emits Python that runs

Its documented purpose is code you can run in SymPy, and for two whole classes of expression it
emitted code that did not run at all.

| expression | was | is |
|---|---|---|
| `1/2`, `1/3 + 1/6` | `sympy.Rational(1, 2` — `SyntaxError: '(' was never closed` | `sympy.Rational(1, 2)` |
| `0/0`, `1/0` | `NaN` — `NameError: name 'NaN' is not defined` | `sympy.nan` |
| `+oo`, `-oo` | `+oo`, `-oo` — `NameError` | `sympy.oo`, `-sympy.oo` |

The first is a missing parenthesis, and it broke **every** expression carrying a non-integer rational,
which is most of what a computer algebra system hands back. The second is a value with no binding: the
generated preamble declares a `sympy.Symbol` for each free *variable*, and a `NaN` or an infinity is
neither a variable nor something SymPy names the same way this library does.

Both were checked by running the emitted programs against SymPy 1.14 rather than by reading them, which
is also how it was established that they now come back **exact** — `1/2` arrives as SymPy's `Half` and
not as the float `0.5`.

Nothing else about the exporter changes. `pi`, `e` and `i` were already emitted as `sympy.pi`,
`sympy.E` and `sympy.I`, and `sqrt` as `sympy.sqrt`.

Two tests now hold the properties that failed, without needing an interpreter in the suite: the
parentheses balance, and every name in the emitted body is either declared in the preamble or reached
through `sympy.`. 17 of their 23 cases fail against the old exporter.

Issue [#909](https://github.com/asc-community/AngouriMath/issues/909).

### `MathS.ToSympyCode` keeps an exact value exact

The generated program ran, and then quietly gave a different number. Python's `/`, and its `**` with a
negative exponent, are float operations on two integers:

| expression | emitted | SymPy read it as | now emitted | and reads as |
|---|---|---|---|---|
| `1/2` | `1 / 2` | `0.500000000000000`, a `Float` | `sympy.Integer(1) / 2` | `1/2`, a `Rational` |
| `2^(-1)` | `2 ** (-1)` | `0.5` | `sympy.Integer(2) ** (-1)` | `1/2` |
| `2^(-3)` | `2 ** (-3)` | `0.125` | `sympy.Integer(2) ** (-3)` | `1/8` |
| `x + 1/2` | `x + 1 / 2` | `x + 0.5` | `x + sympy.Integer(1) / 2` | `x + 1/2` |

Making one operand a SymPy integer hands the arithmetic to SymPy, which keeps it exact. **Only a pair of
integers is rewritten**, and the rest of the emitted code is unchanged: with a symbol anywhere in the
shape SymPy's own operators already take over (`x / 2`, `1 / x`, `x ** (-1)`), and `+`, `-`, `*` and a
non-negative `**` are exact on Python integers, whose precision is unbounded — `2 ** 70` was always
right.

It bit only the **unsimplified** form, which is the one a caller writes: `"1/2".ToEntity()` is a `Divf`
of two integers, because a printed rational parses back as a division
([#873](https://github.com/asc-community/AngouriMath/issues/873)), while a simplified `1/2` is a
`Rational` node and already emitted `sympy.Rational(1, 2)`.

Checked by running the emitted programs against SymPy 1.14, which is the only way this class of defect
shows itself — the code was always valid, so the earlier tests could not have caught it, and the two
added here assert the property instead: no two plain integer literals are combined with `/` or with a
negative `**`.

Issue [#911](https://github.com/asc-community/AngouriMath/issues/911).

### `NaN` is now a keyword, and the printed form of NaN reads back

`Stringize` prints the NaN value as `NaN`, and the grammar had no such token, so reading it back gave a
**variable of that name**. A variable behaves like any symbol — it cancels, collects and compares — and
nothing on the page distinguished the two, since a variable named `NaN` also prints as `NaN`:

| expression | was | is |
|---|---|---|
| `"NaN - NaN".Simplify()` | `0` | `NaN` |
| `"NaN / NaN".Simplify()` | `1 provided not NaN = 0` | `NaN` |
| `"NaN * 0".Evaled` | `0` | `NaN` |
| `"NaN * 2".Evaled`, `"NaN + NaN".Evaled` | left as written | `NaN` |
| `NaN` as a variable name | a variable | a parse of the NaN value |
| `NaNx`, `NaN_1`, `aNaN` | variables | variables, unchanged |

This is the same trade `mod` took above: a word that was an identifier becomes a keyword, so an
expression using it as a variable name now means something else. It is the narrower half of the trade —
only the exact spelling is reserved, because the lexer takes the longest match, so any longer name that
merely contains it is still a variable.

Only `NaN` is affected. Its two siblings already had tokens: `+oo` and `-oo` both print and parse, and
`Latexize` has had `\mathrm{undefined}` for all along — which is the token
[CSharpMath](https://github.com/verybadcat/CSharpMath) decodes back to `MathS.NaN`, so the LaTeX round
trip was closed already and is untouched here.

**The round-trip test now runs in both directions.** Every case in `StringizeRoundTripTest` began from a
*string*, so it could only reach expressions the parser already produces, and a value with no source
form was invisible to all of them however many cases were added. It now also enumerates the library's
named constants by reflection — `MathS` and `Entity.Number.Real` — prints each and reads it back, so a
constant added later is covered without anyone remembering the file exists. Those two cases are what
fail against the old grammar.

Issue [#906](https://github.com/asc-community/AngouriMath/issues/906).

---

## 2.0.0 — since 1.4.0

### At a glance

| Silent? | What | Was | Is |
|---|---|---|---|
| loud | `Minusf.Minuend` / `.Subtrahend` | named for the wrong operand | named for the right one |
| **silent** | `exp(x)`, `log10(x)`, `log2(x)` | products of undeclared variables | the functions |
| loud | `arcsinh(x)` and its five relatives | a product | `UnrecognizedFunctionParseException` |
| loud | `floor(x)` and ten other names the library lacks | a product, silently | `UnrecognizedFunctionParseException` |
| **silent** | `sqrt(x^2)`, `sqrt(-x)` and their kind | `x`, `i*sqrt(x)` — wrong for negative x | left as written |
| loud | `mod` as a variable name | a variable | a keyword, so a parse error |
| **silent** | `Stringize` of powers, lambdas, applications, piecewises | did not parse back | parses back |
| **silent** | `Stringize` of a complex number with a fractional imaginary part | read back as its negation, or as a power | parses back |
| **silent** | numbers below `1e-16` | rounded to `0` | kept |
| **silent** | `Real` `%` | `-7 % 3` was `-1` | `2` |
| **silent** | `Simplify` of a cancelling quotient | the quotient | a `Providedf` |
| **silent** | radicals, everywhere | `sqrt(12)` | `2 * sqrt(3)` |
| **silent** | `Expand` | left like terms uncollected | collects them |
| **silent** | an identity equation | `{ }` or `{ 0 }` | all of `CC` |
| **silent** | a quadratic inequality | wrong for one sign of a symbolic coefficient | a case split on that sign |
| **silent** | numeric root sets | one root per starting point | one root per root |
| **silent** | many limits | `NaN`, or unevaluated | a value |
| **silent** | a vanishing sine behind a constant factor | `NaN` | a value — but `sin(x)*ln(x)*2` loses its `0` |
| **silent** | a limit assembling `oo^0` or `1^oo` | that form's value, `1` | the real answer, or unevaluated |
| **silent** | a factorial under a vanishing exponent | unevaluated | a value, by Stirling |
| **silent** | `ln(x!)` in a limit | unevaluated, and `ln(x!)/ln(x)` was `NaN` | a value |
| **silent** | a boolean expression | factored, not minimised | minimised where that is shorter |
| **silent** | some integrals | not antiderivatives | closed forms |
| **silent** | two integrals | answered correctly | unevaluated — a deliberate loss |
| **silent** | `k/(a x^2 + c)` and `k/sqrt(a x^2 + c)` for symbolic `a` | `NaN` | a piecewise on the sign of the discriminant |
| loud | `Compile` over a missing variable | `KeyNotFoundException` | `UncompilableNodeException` |
| loud | `Expand` of a quotient of factorials | `AngouriBugException` | the expanded polynomial |
| loud | parsing a `provided` in a parenthesised comma list | `NullReferenceException` | `UnhandledParseException` |
| loud | `floor(x)`, `ceil(x)`, `ceiling(x)` | `UnrecognizedFunctionParseException` | the functions |
| loud | `round(x)`, `min(a, b)`, `max(a, b)`, `gcd(a, b)` | `UnrecognizedFunctionParseException` | the functions |
| loud | 28 members deprecated since 1.x | obsolete but present | removed |
| loud | `Latexise`, `ILatexiseable`, `entity_latexise` | the British spelling | `Latexize`, `ILatexizeable`, `entity_latexize` |
| loud | `MathS.Quantum.Factorise` | one letter from the unrelated `Entity.Factorize` | `MathS.Quantum.TensorFactorize` |
| loud | `MathS.Quantum.IsNormalised` | the British spelling | `MathS.Quantum.IsNormalized` |
| loud | the target frameworks | `net7.0;netstandard2.0` | `netstandard2.0;net8.0;net10.0` |
| **silent** | `abs(x) = c` for a negative `c` | a set of non-solutions | the empty set |
| loud | implicit `List<Entity>` to `Entity` | made a `FiniteSet`, and made three `params` overloads uncallable | removed |
| loud | implicit `Entity[]` to `Entity` | made a `FiniteSet`, discarding order and repeats | removed |
| loud | implicit `(Entity, Entity)` to `Entity` | made a **closed** interval, though `(a, b)` reads as the open one | removed |
| **silent** | a `MathS.Settings` scope across an `await`, or inside a task | lost, or somebody else's | follows the call |
| **silent** | a `RewriteRecording` across an `await`, or work started under it | lost, or somebody else's | follows the call |
| loud | a polynomial system with more equations than unknowns | `WrongNumberOfArgumentsException` | solved |
| loud | a known gap, e.g. a cubic inequality | `AngouriBugException`, asking to be reported | `NotSufficientlySupportedException` |
| **silent** | `arcsin(sin(x))` and three siblings | `x`, wrong wherever `x` leaves the principal interval | left as written unless `x` is a real in that interval |

---

## Target frameworks

`net7.0` is replaced by `net8.0` and `net10.0`. `netstandard2.0` is unchanged, so consumers on
.NET Framework and the older runtimes are unaffected.

| | was | is |
|---|---|---|
| `TargetFrameworks` | `net7.0;netstandard2.0` | `netstandard2.0;net8.0;net10.0` |

.NET 7 left support in May 2024, so the old list named one framework nobody should still be
targeting and no supported modern one. A consumer on net8.0 or net10.0 previously resolved the
`netstandard2.0` asset and silently lost the generic-math surface with it; they now get a real
target.

**What breaks.** Anything that pins the framework of its reference:

```xml
<ProjectReference Include="...\AngouriMath.csproj">
  <SetTargetFramework>TargetFramework=net7.0</SetTargetFramework>   <!-- no longer resolves -->
</ProjectReference>
```

fails with `NETSDK1005: Assets file ... doesn't have a target for 'net7.0'`. Change it to `net10.0`
or `net8.0`. This is not hypothetical -- every one of the nine measurement harnesses in this
workspace pinned `net7.0` and had to be repointed.

A consumer that references the NuGet package rather than the project picks its asset by its own
framework and needs no change, unless it targets `net7.0` itself, in which case it now resolves
`netstandard2.0` and loses the generic-math types.

**Generic math is unchanged in reach.** `Core/Entity/GenericMath` needs `INumber<T>` and so is
still compiled only into the net7.0-and-later targets -- now written as a compatibility test
rather than a literal `!= 'net7.0'`, so adding a framework does not silently drop it. Verified on
the built assemblies: present in `net8.0` and `net10.0`, absent from `netstandard2.0`.

---

## Types and members

### The implicit conversions from a collection are removed

`Entity` had two implicit conversions from a collection, both building a `FiniteSet`:

```csharp
public static implicit operator Entity(Entity[] elements);       // removed
public static implicit operator Entity(List<Entity> elements);   // removed
```

The second one made three public members impossible to call with a `List<Entity>`:

```csharp
var equations = new List<Entity> { "x - 1", "y - 2" };
new EquationSystem(equations);        // error CS0121: the call is ambiguous
new FiniteSet(equations);             // error CS0121
MathS.Equations(equations);           // error CS0121
```

Each of those has both an `IEnumerable<Entity>` overload and a `params Entity[]` overload. With
the conversion in place a `List<Entity>` also converts to a single `Entity`, so the `params`
overload becomes applicable in its expanded form, neither candidate is better than the other,
and the call does not compile. The array, `IEnumerable<Entity>`, and variadic forms were always
fine — only a concrete `List<Entity>` broke, which is why it survived to a 2.0 preview.

This was not three separate defects. One conversion made *every* `params Entity[]` overload in
the library uncallable with a list, including any added later.

**This one moves between previews.** The conversion is in `2.0.0-preview.1` and
`2.0.0-preview.2`, both published, and is gone in `2.0.0`. It is the only entry here that
changes between two previews rather than between releases: a reader coming from 1.3.0 or
1.4.0 can ignore the distinction, one already on a preview cannot.

**What breaks.** Assigning a list where an `Entity` is expected:

```csharp
Entity set = new List<Entity> { 1, 2, 3 };   // no longer compiles
```

Write one of these instead:

```csharp
Entity set = new FiniteSet(new List<Entity> { 1, 2, 3 });
Entity set = new List<Entity> { 1, 2, 3 }.ToSet();
```

**The conversion from `Entity[]` is gone as well.** It was kept at first, on the narrow
ground that it never produced the ambiguity — an array binds to the `params` overload in its
normal form by an identity conversion, which wins outright. That was true and beside the
point. An array carries an order and can repeat an element; a set has neither, so the
conversion silently discarded part of what it was handed, and an implicit conversion that
loses information is the wrong shape regardless of which overloads it happens to break.
Set types are built explicitly nearly everywhere for this reason.

```csharp
Entity set = new Entity[] { 1, 2, 3 };          // no longer compiles either
Entity set = new FiniteSet(1, 2, 3);            // say it
Entity set = new Entity[] { 1, 2, 3 }.ToSet();
```

Only one place in the whole repository relied on it, which was the test pinning it.

**And the conversion from a pair, which had the same fault from the other side.** A
two-element tuple became an `Interval`, and since a tuple says nothing about whether its
endpoints are included, the conversion had to supply that:

```csharp
Entity e = ((Entity)1, (Entity)5);
// was: [1; 5] — both endpoints included, and 1 is a member
```

Where the array conversion dropped information that was there, this one produced
information that was not — and chose the reading opposite to the notation, since `(1, 5)`
is the open interval in ordinary mathematical writing and `[1, 5]` the closed one. A caller
writing what looks like an open interval got a closed one, silently.

```csharp
Entity e = MathS.Interval(1, 5);                 // closed, said out loud
Entity e = MathS.Interval(1, false, 5, false);   // open
Entity e = new Interval(1, true, 5, true);
```

Nothing in the library, the tests, the F# wrapper or the utilities used it.

The pairs elsewhere in the API are unaffected, because none of them has to guess: an
integration `Range`, the arguments of `Substitute`, the cases of `MathS.Piecewise` and
`ToProvided` are all ordered pairs whose two halves have distinct, stated roles.

### An inverse trigonometric function no longer cancels its own function off the principal branch

`arcsin` is a left inverse of `sin` only on `[-pi/2, pi/2]`, and the three siblings likewise only on
their own intervals. The rewrite was unconditional, so `Simplify` returned a value that is wrong at
every real point outside the interval:

| | was | is | the value at that point |
|---|---|---|---|
| `arcsin(sin(x))` | `x` | left as written | `pi - x` for `x` in `[pi/2, 3pi/2]`, and so on |
| `arccos(cos(x))` | `x` | left as written | `2*pi - x` for `x` in `[pi, 2pi]` |
| `arctan(tan(x))` | `x` | left as written | `x - pi` for `x` in `(pi/2, 3pi/2)` |
| `arccotan(cotan(x))` | `x` | left as written | `x - pi` for `x` in `(pi, 2pi)` |

Measured on a build of `35f4ae5a` before this change and after it. The four rules are older than
1.4.0 and untouched since, so 1.4.0 answers the same way, but the numbers below are from the two
2.0.0 builds rather than from a 1.4.0 one:

```
"arcsin(sin(x))".Simplify().Substitute("x", 3).EvalNumerical()
  was  3
  is   0.14159...        which is pi - 3, and is what arcsin(sin(3)) equals

"arctan(tan(x))".Simplify().Substitute("x", 2).EvalNumerical()
  was  2
  is   -1.14159...       = 2 - pi

"arccos(cos(x))".Simplify().Substitute("x", 4).EvalNumerical()
  was  4
  is   2.28318...        = 2*pi - 4
```

**Where the argument is a real number inside the interval, the cancellation still happens and stays
exact**: `arcsin(sin(1/2))` is `1/2`, not a decimal. Where it is symbolic, the expression is left as
written, which is what SymPy and Mathematica both answer.

A condition was deliberately *not* attached. `arcsin(sin(x))` is defined for every real `x`, so
`x provided x >= -pi/2 and x <= pi/2` would say the expression is undefined outside the interval
when in fact it merely has another value — trading a wrong value for a wrong domain.

**The other direction is unchanged**, and needs no assumption: `sin(arcsin(z))`, `cos(arccos(z))`,
`tan(arctan(z))` and `cotan(arccotan(z))` are all `z`, because they compose the *right* inverse.

`CircleTest.Test8` asserted `arccotan(cotan(3x)) == 3x` and was pinning the wrong answer; it now
asserts that the expression is left alone. Issue
[#884](https://github.com/asc-community/AngouriMath/issues/884).

### A known gap no longer presents as a bug

`FutureReleaseException` is removed, and the twelve places that threw through it now throw
`NotSufficientlySupportedException`.

It worked by naming the release a feature was planned for:

```csharp
throw FutureReleaseException.Raised("Inequalities are not implemented yet", "1.2.1");
```

and turning *itself* into an `AngouriBugException` once that release had shipped — message
ending *"please report about it to the official repository"*. Every site named 1.2, 1.2.1 or
1.3. All three shipped years ago, so twelve known gaps were inviting bug reports about work
nobody had started:

```csharp
("x3 - x > 0").Solve("x");
// was: AngouriBugException — "Inequalities are not implemented yet was planned for 1.2.1
//      but hasn't been released by 1.2.1 ... please report about it"
// is:  NotSufficientlySupportedException — "Only linear and quadratic polynomial
//      inequalities are supported; this one is of a higher degree"
```

An unbuilt feature is not a bug, and a caller cannot act on being told to report one.

**What breaks.** `FutureReleaseException` is gone from the public surface; catch
`NotSufficientlySupportedException`, or `AngouriMathBaseException` for both. Code catching
`AngouriBugException` around any of these gaps no longer catches them.

The messages changed too, and deliberately: they were notes to a maintainer — *"We should
be able to return sets from invertnode"* — and are now what is not supported, since they
are the only thing a caller sees.

### A settings scope belongs to the call, not to the thread

`MathS.Settings` values were held in `[ThreadStatic]` fields — fourteen of them. A scope
therefore stopped at the first `await`:

```csharp
using var _ = MathS.Settings.MaxExpansionTermCount.Set(1);
Console.WriteLine(MathS.Settings.MaxExpansionTermCount.Value);   // 1
await Task.Delay(20).ConfigureAwait(false);
Console.WriteLine(MathS.Settings.MaxExpansionTermCount.Value);   // was 2000, the default
```

The continuation resumed on a pool thread that had never seen the scope, so the setting
silently read its default. The same mechanism ran the other way too: a thread returned to
the pool still carried whatever scope was left on it, so the *next* caller to borrow that
thread could compute under a precision or codomain it never asked for. Neither shows up as
an error; both change the answer.

The values now live in an `AsyncLocal`, which is what the cancellation token in
`MathS.Multithreading` already used.

| | was | is |
|---|---|---|
| a scope across an `await` | lost | kept |
| a scope inside `Task.Run` started under it | not inherited | **inherited** |
| a scope opened in a task, seen by a sibling | no | no |
| a scope opened in a task, seen after it ends | no | no |
| a thread reused by the pool | could carry a stale scope | cannot |

**What breaks.** The second row is the one to read. Work started inside a scope now runs
under it:

```csharp
using var _ = MathS.Settings.Codomain.Set(Domain.Real);
await Task.Run(() => expr.Solve("x"));   // now solves over R; used to solve over C
```

That is what the code says, and almost always what was meant — but if you parallelised
inside a scope and relied on the child *not* seeing it, it does now. Move the scope inside
the callback to keep the old behaviour.

**Cost.** Measured over 20 000 000 reads and 2 000 000 scopes:

| | was | is |
|---|---|---|
| read `PrecisionErrorZeroRange.Value` | 7.3–8.0 ns | **1.2 ns** |
| read `DowncastingEnabled.Value` | 0.79 ns | 0.96 ns |
| `Set()` + `Dispose()` | 388–395 ns, 32 B | **46 ns**, 112 B |
| a `Simplify` workload | 5.6–5.8 s | 5.5–5.6 s, +1.5 % allocated |

Reads got faster rather than slower: the old getter re-tested a `[ThreadStatic]` field for
null on every access, and that is dearer than the async-local lookup that replaced it.
Opening a scope got much cheaper because it no longer mints a `Guid` to identify itself,
though it allocates more, since assigning an `AsyncLocal` copies the flow's value map. Reads
outnumber scope openings by orders of magnitude in any real workload.
### A rewrite recording belongs to the call, not to the thread

`RewriteRecording` held its ambient scope in a `[ThreadStatic]` field, and documented the
consequence rather than fixing it:

> **A synchronous scope, and it has to be.** Do not `await` inside one.

It no longer has to be. The scope is an `AsyncLocal`, as `MathS.Settings` and the
cancellation token in `MathS.Multithreading` are, so it survives an `await` and work
started under it reports to it wherever it runs.

| | was | is |
|---|---|---|
| a recording across an `await` | lost, and the thread could collect a stranger's rewrites | kept |
| work started under a recording, on another thread | **not** collected | collected |
| two flows each with their own recording | separate | separate |
| a recording opened inside a task, seen after it ends | no | no |

**What breaks.** The second row. A recording no longer stops at the thread boundary:

```csharp
using var recording = RewriteRecording.Start();
var t = new Thread(() => expr.Simplify());
t.Start(); t.Join();
recording.Steps;   // used to be empty of that work; now contains it
```

That is what the code says and what the scope is for, but a caller who fanned out inside a
recording and expected only their own thread's rewrites will now see everything. Open the
recording inside the callback to keep the old behaviour.

**`Steps` is now a snapshot.** It was a live view of the underlying list; because the store
has to tolerate concurrent writers it is a `ConcurrentQueue`, and `Steps` copies out of it.
Reading it after disposing — the documented use — is unchanged. Holding the returned list
across further recording and expecting it to grow no longer works.

**Order across parallel work is not defined.** Steps from one flow keep the order they fired
in; two flows recording into the same recording interleave however they happen to run. The
single-threaded case, which is what `Simplify` is, is unaffected.

Being off is still free: no recording open still costs one ambient read per rule set and
allocates nothing, which `RewriteAllocationTest` continues to hold to.
### An over-determined polynomial system is solved rather than refused

`EquationSystem.Solve` insisted on as many equations as unknowns and threw otherwise:

```csharp
MathS.Equations("x^2 + y^2 - 25", "x + y - 7", "x*y - 12").Solve("x", "y");
// was: WrongNumberOfArgumentsException
// is:  the two solutions, (3, 4) and (4, 3)
```

The count was a consequence of how the old solver worked — it eliminated one variable per
equation — and not of the problem. A Gröbner basis has no use for the equality, and an
extra equation that happens to be a consequence of the others is not an error to report.

An inconsistent system now reports itself as one, which it also could not do before:

```csharp
MathS.Equations("x^2 + y^2 - 25", "x + y - 7", "x*y - 99").Solve("x", "y");
// was: WrongNumberOfArgumentsException
// is:  null — no solutions
```

**What breaks.** Code that catches `WrongNumberOfArgumentsException` to detect a
malformed system will no longer see it for the polynomial case. **Fewer** equations than
unknowns still throws: a free variable means infinitely many solutions, which this does
not enumerate.

The relaxation only applies where the system is a polynomial one over `Q` in at most eight
variables and its solutions are rational. Everything else reaches the previous solver
exactly as before, including the equation-count check.

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

### `Latexise` is now `Latexize`

The library serialises a node to text two ways, and spelled them in two languages:

| | was | is |
|---|---|---|
| the method | `Latexise()` | `Latexize()` |
| the interface | `ILatexiseable` | `ILatexizeable` |
| the string extension | `"x + 1".Latexise()` | `"x + 1".Latexize()` |
| the C# helper | `MathS.Latex(ILatexiseable)` | `MathS.Latex(ILatexizeable)` |
| the native entry point | `entity_latexise` | `entity_latexize` |
| the C++ method | `Entity::Latexise()` | `Entity::Latexize()` |
| the F# function | `latexise` | `latexize` |

Everything else in the public surface is `-ize` — `Stringize`, `Factorize`, `Serialize`,
`Deserialize` — so `Latexise` and its interface were the only members a caller had to remember the
other spelling for. `Stringize` and `Latexise` do the same kind of thing and sit next to each other
in every file that has either, which is what made the split spelling costly rather than merely
untidy.

**What breaks.** Every call, every override, and every implementation of the interface. There is no
forwarding member: this release is the one that removed 28 members that accumulated as forwarders,
so adding a permanent one here would undo that on the same day. The compiler reports each site, and
the fix is mechanical.

**Downstream.** [`CSharpMath.Evaluation`](https://github.com/verybadcat/CSharpMath) reads LaTeX
produced here back into an `Entity` and calls this method. It is unaffected until it moves to 2.0,
at which point it needs the new name.

[#840](https://github.com/asc-community/AngouriMath/issues/840).

### `MathS.Quantum.Factorise` is now `MathS.Quantum.TensorFactorize`

| | was | is |
|---|---|---|
| the method | `MathS.Quantum.Factorise(Entity)` | `MathS.Quantum.TensorFactorize(Entity)` |

The old name differed by one letter from `Entity.Factorize`, which does something else entirely —
algebraic factoring of an expression, against tensor factorisation of a quantum state. Both take an
`Entity` and return an `Entity`, so nothing but the name distinguished them, and the name barely
did.

Renaming it to `Factorize` would have made the spelling uniform and the API worse: two public
`Factorize` methods doing unrelated things. `MathS.Quantum` already holds the inverse operation as
`TensorExpand`, so `TensorFactorize` states the domain and the direction, pairs with its inverse,
and ends the collision. The spelling is fixed as a side effect rather than as the point.

**What breaks.** Every call to `MathS.Quantum.Factorise`. There is no forwarding member, for the
same reason as above. The compiler reports each site.

`MathS.Quantum.IsNormalised` is renamed to `IsNormalized` in the same pass. It carries no collision,
only the spelling — but it is a public member, so 2.0 is equally the last release that can change
it, and leaving it would have meant shipping one British member after two renames made expressly to
remove them.

With these two, the public surface has one spelling throughout. That is checkable rather than
asserted: `PublicApi.txt` is the recorded surface, and it now contains no `-ise`, `-ised` or
`-isation` member.

[#843](https://github.com/asc-community/AngouriMath/issues/843).

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

### Eleven function names the library does not have are refused

| | |
|---|---|
| was | `floor * x`, silently |
| is | `UnrecognizedFunctionParseException` |

`floor`, `ceil`, `ceiling`, `round`, `trunc`, `min`, `max`, `gcd`, `lcm`, `erf` and `conjugate` are
each what some other CAS calls a function, and AngouriMath has none of them. A name the grammar does
not know, followed by a bracket, is the implicit multiplication that lets `a(b + c)` mean
`a * (b + c)` — so each came back as the product of an undeclared variable with its argument, and
nothing said so. What that cost:

```csharp
"floor(x) - 3 = 0".Solve("x")     // was { 3 / floor }, a root of nothing
```

Two things are worth knowing before you read this as a loss. First, it is the **same absence** that
was already reported for two arguments, only reported the same way now: `min(x, y)` did raise, but
with `no viable alternative at input '*('` from the parser generator, so whether a missing function
was invisible or merely cryptic depended on how many arguments the caller happened to pass. Second,
none of these is being removed — none was ever there.

Refusing *every* unknown name is not an option, because that is what `a(b + c)` is. So these are
refused one at a time, as `arcsinh` above is, and everything else is untouched: each name on its own
is still an ordinary variable (`min + 1`, `floor + floor`), and a longer name beginning the same way
is still a product (`minimum(x)`, `rounded(x)`, `maxx(y)`).

`re` and `im` are deliberately **not** refused although they misparse identically. Two letters is
short enough that a caller may reasonably have a variable of that name, and refusing would break
their expression to fix nobody's.

[#733](https://github.com/asc-community/AngouriMath/issues/733), PR
[#750](https://github.com/asc-community/AngouriMath/pull/750).

### `sqrt(x^2)` is no longer `x`, and `sqrt(-x)` is no longer `i * sqrt(x)`

| | |
|---|---|
| was | `x`, and `i * sqrt(x)` |
| is | left as written |

Both were wrong, and by a whole sign rather than a rounding:

```csharp
"sqrt(x ^ 2)".Simplify()      // was x       — at x = -0.63 that is -0.63 where it is 0.63
"(x ^ 2) ^ (3/2)".Simplify()  // was x ^ 3   — at x = -2 that is -8 where it is 8
"sqrt(-x)".Simplify()         // was i*sqrt(x) — at x = -0.63 that is -0.7937, not 0.7937
```

Two rules were being applied without the condition each needs. `(a^b)^c = a^(b*c)` holds for
a positive `a`, and for any `a` when `c` is a whole number — `(a^b)^3` is `a^b` multiplied by
itself three times however `a` is signed. `(a*x)^c = a^c * x^c` holds for a positive `a`, and
again for a whole `c`. Both are now restricted to exactly that, and unchanged where they hold:
`(x^(1/2))^2` is still `x`, `(-x)^2` is still `x^2`, `(2^2)^(1/2)` is still `2`.

`sqrt(x^2)` is `abs(x)`, and the library does not write that for you — it leaves the
expression alone rather than answering something false. Writing `abs` requires knowing the
expression is real, which is what
[#719](https://github.com/asc-community/AngouriMath/issues/719)'s codomain now makes sayable
and is not yet read by the simplifier.

**One thing got worse, and it is worth stating plainly.** The inequality solver was leaning
on `sqrt(4a^2) = 2a`: `(x - a)(x + a) <= 0` answered `[a; -a]`, which is right for `a < 0`
and empty for `a > 0`. It now answers `{ a, -a } \/ (sqrt(a^2); -sqrt(a^2))`, whose interval
is empty for either sign. Neither form is right for both, because the solver has no case
split on the sign of a symbolic coefficient — that is
[#757](https://github.com/asc-community/AngouriMath/issues/757), and it was invisible while an
unsound rewrite was making it look right half the time.

Found by `work/simpsweep`, which generates the expressions it checks; its count of
disagreements over 10463 expressions goes **30 to 0**.

[#752](https://github.com/asc-community/AngouriMath/issues/752), PR
[#758](https://github.com/asc-community/AngouriMath/pull/758).

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
| `i / 2` evaluated | `1/2i` | `1 / (2i)`, which is its **negation** | `1/2 * i` |
| `2 + 3 * i / 4` evaluated | `2 + (3/4)i` | `2 + (3/4) ^ i`, a different number | `2 + 3/4 * i` |

Powers group to the right, so it is the base that needs bracketing when it is a power of its own —
the mirror of the rule the left-associative operators use. The next three have no operator spelling
in the grammar, so they now print as the function call the parser does have.

The last two are one defect. A complex number whose imaginary part is a fraction was printed with
the fraction pushed up against the `i`, and `1/2i` is read as `1 / (2i)` — the negation of what was
printed. The case with a real part bracketed the coefficient, which looks like a guard and is not:
a bracket followed by a name is read as exponentiation, so `(3/4)i` came back as `(3/4) ^ i`. Both
now multiply explicitly, which needs no bracket since `*` and `/` bind tighter than `+`. Integer
coefficients never had the problem and are unchanged — `i`, `-i` and `2i` print as before.

This one is worth reading twice if you parse printed output, because the misreading is a perfectly
ordinary number: every root of a trigonometric equation prints through this path, and
`cos(x)^2 + sin(x) = 0` returns roots that are exact as entities and were not as text.

`Latexize` is unaffected by this change. Its output is under a weaker obligation, not none:
nothing in this repository parses LaTeX back, but [`CSharpMath.Evaluation`](https://github.com/verybadcat/CSharpMath)
does, so a change to what it emits can break a downstream project with no test here to say so
([#822](https://github.com/asc-community/AngouriMath/issues/822)).

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

## Boolean simplification

### A boolean expression is minimised rather than merely factored

The rewrite rules reached absorption and stopped there, so the factoring worked and had nothing to
finish it — there is no rule taking `b or not b` to `true`:

```
a and b or a and not b                          was  a and (b or not b)   is  a
a or not a                                      was  a or not a           is  true
a and not a                                     was  a and not a          is  false
(a and b) or (a and not b) or (not a and b)      was  not a and b or a and (b or not b)
                                                 is  a or b
(not a and not b and not c) or (not a and not b and c)
                                                 was  a or b or c implies not (a or b) and c
                                                 is  not (a or b)
```

Two-level minimisation by Quine–McCluskey now runs over the truth table, covering excluded middle,
non-contradiction and every larger cover in one procedure rather than as separate rules.

**It is offered to `Simplify` as one more candidate, not as a replacement for its answer.**
Candidates are ranked by node count and the shortest is returned, so this can only change an
expression where the minimal form is shorter than everything else already on offer. That is what
keeps `not (a and b)` as it is — 4 nodes against its sum-of-products form `not a or not b` at 5 — and
it means no expression's answer gets longer.

The last row above is [#769](https://github.com/asc-community/AngouriMath/issues/769), where an
`implies` rewrite won at 12 nodes against the 16-node input. That rewrite was not misbehaving; the
4-node answer was simply never generated for it to lose to.

Bounded at **10 variables**, since the table is `2^n` rows. Beyond that it declines rather than
hangs. A cover of k terms is at least `2k-1` nodes, so where that already exceeds the input the
search is abandoned before it starts — parity over ten variables has 512 minterms, none of which
combine, and finding its 512-term minimal form took 1.5 s to produce a candidate that loses to the
input. With the check, 34 ms.

[#768](https://github.com/asc-community/AngouriMath/issues/768) and
[#769](https://github.com/asc-community/AngouriMath/issues/769), PR
[#770](https://github.com/asc-community/AngouriMath/pull/770).

## Solving

### A quadratic inequality is answered for every sign its coefficients may have

Three things decided a quadratic inequality's answer that the solver was not asking about, each of
them the sign of something it could not evaluate:

```
x^2 + 1 > 0                    was  { }                    is  RR
3*x^2 + 1 > 0                  was  (-oo; c) \/ (c; +oo)   is  RR
a*x^2 - 1 < 0     at a = 3     was  the complement of      is  (-1/sqrt(3); 1/sqrt(3))
                                    the solution set
(x + 1)(x + 2) < a             was  an interval even where it has no real roots
```

A parabola lies above zero *between* its roots when it opens downwards and *outside* them when it
opens upwards, and the test for that read `a is Real { IsNegative: true }` — which a symbol fails,
so every symbolic leading coefficient was read as positive. Where the quadratic has no real roots it
sits wholly on one side, and the empty set was returned whichever side that was. And whether "no
real roots" was noticed at all depended on how far the radical simplified: `sqrt(-4)` is the literal
`2i` and `sqrt(-12)` is a product that is not one, so `x^2 + 1` was recognised and `3*x^2 + 1` was
not. The linear branch had the same defect in miniature — `a*x + b > 0` is `x > -b/a` for a positive
`a` and `x < -b/a` for a negative one.

**What changes for callers.** A *concrete* coefficient is untouched: `(x - 2)(x + 2) <= 0` is still
`[-2; 2]` and `x^2 - 4 < 0` is still `(-2; 2)`. Where a coefficient is symbolic the answer is now a
union of conditional sets rather than a single interval — the case split written in the vocabulary
the library already has, since `Piecewise` is an `Entity` and `Solve` returns a `Set`:

```
{ x : a > 0 and x in ... or a < 0 and x in ... or a = 0 and x in ... }
```

Code that assumed `Solve` on an inequality returns an `Interval` will now sometimes get a
`ConditionalSet` or a `Unionf`. It was returning a wrong interval before.

Measured over 128 specialisations — solve with the symbol, substitute a value of each sign, and
compare membership against the inequality itself at eleven points: **81/128 correct on 1.4.0,
128/128 now**. Four cases skipped as "Piecewise required" since 1.2 pass.

[#757](https://github.com/asc-community/AngouriMath/issues/757) and
[#762](https://github.com/asc-community/AngouriMath/issues/762), PRs
[#761](https://github.com/asc-community/AngouriMath/pull/761) and
[#763](https://github.com/asc-community/AngouriMath/pull/763).

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

### A constant factor no longer decides whether a vanishing sine is seen

The first remarkable limit — a vanishing `sin(u)`, `tan(u)`, `arcsin(u)` or `arctan(u)` rewritten as
the `u` it is equivalent to — was applied at the root of the expression only. A constant factor
written on one side pushes that function a level down and out of reach, so the same product answered
two different things depending on how it was spelled:

```
lim x->+oo sin(1/x) * x * 2       was  2        is  2
lim x->+oo 2 * sin(1/x) * x       was  NaN      is  2
lim x->+oo sin(1/x) * x / 2       was  NaN      is  1/2
lim x->0   sin(x) * 1/x / 2       was  NaN      is  1/2
lim x->+oo 2 * tan(1/x) * x^2     was  NaN      is  +oo
```

The rule is now applied down the product-and-quotient spine. It stops at anything that is not a
product or a quotient, which is what keeps it sound: the equivalence licenses replacing a *factor*
of the expression as a whole, and not a term of a sum, where the difference between the two forms is
the whole answer. `lim x->0 (sin(x)/x - 1)/x^2` is still `-1/6` and `lim x->0 (sin(x) - x)/x^3` is
still `-1/6`, as they must be — a rule that descended into sums would answer both `0`.

**The loss, and it is a value becoming `NaN`.** A vanishing factor against a logarithm was being
shielded from a rewrite the library already applies to the same limit written without the constant:

```
lim x->0   sin(x) * ln(x) * 2     was  0        is  NaN
lim x->0   2 * arctan(x) * ln(x)  was  0        is  NaN
lim x->0   sin(x) * ln(x)         was  NaN      is  NaN     (unchanged, and the reason)
```

The rewrite is sound — `sin(x) * ln(x)` and `x * ln(x)` have the same limit — and what it exposes is
that `lim x->0 x * ln(x)` is `NaN` by design, for the reason given above: `ln(x)` is not real to the
left of `0`. So the previous `0` was the accident, and the two spellings of one limit now agree
instead of contradicting each other. Under `MathS.Settings.Codomain.Set(Domain.Real)` both come back
unevaluated, which is the honest answer where the function has no real left-hand neighbourhood.

Measured over 811 generated products and quotients: 196 `NaN`s became values, 13 values became this
one `NaN` — every one of them of the shape above, and in each case the constant-free spelling was
already `NaN` — and no answer changed into a different answer.

[#749](https://github.com/asc-community/AngouriMath/issues/749), PR
[#759](https://github.com/asc-community/AngouriMath/pull/759).

### An indeterminate power form is no longer read off as its value

A limit was answered, where nothing else had a reading of it, by substituting the destination and
evaluating. That is right wherever the expression is continuous there, and an indeterminate form is
exactly where it is not. Almost all of them already declined, because they evaluate to `NaN` and
every caller reads `NaN` as "no limit" — `0 * oo`, `oo - oo`, `oo / oo`, `0^0`. The two that do not
are `oo^0` and `1^oo`, which this library's arithmetic answers with `1`, so a limit that assembled
either read that `1` off as its answer:

```
lim x->+oo (e^x) ^ (1/ln(x))    was  1     is  +oo
lim x->+oo (x^2) ^ (1/ln(x))    was  1     is  e^2
lim x->+oo (x!)  ^ (1/x)        was  1     is  unevaluated   (it is +oo)
lim x->+oo (x!)  ^ (1/ln(x))    was  1     is  unevaluated   (it is +oo)
```

`(x^2)^(1/ln x)` is `e^(2*ln(x)/ln(x))`, that is `e^2` at every `x`, so the old answer was not merely
imprecise. `(x!)^(1/x)` grows like `x/e` by Stirling — `(100!)^(1/100)` is already `37.99`.

This does **not** change what `(+oo)^0` or `1^oo` evaluate to as expressions. Both are still `1`, the
same convention SymPy and IEEE 754's `pow` use; the change is that a *limit* no longer reads its
answer off one. `0^0` is deliberately untouched for the opposite reason: its `NaN` is how the library
says `lim x->0 x^x` does not exist, which `LimitTest.TestNoLimit` pins, and declining it would turn a
considered "does not exist" into "not settled".

**What it costs.** Two limits that were answered `1` are now unevaluated, because the substitution
that used to land on them is gone and nothing else has a reading:

```
lim x->0   (1/x) ^ x            was  1     is  unevaluated
lim x->0   (1/x) ^ (x^2)        was  1     is  unevaluated
```

A third, `lim x->+oo (x!)^(1/x^2)`, was withdrawn here and is answered `1` again by the entry
below — it was right by luck when this landed, since the same substitution gave `1` for
`(x!)^(1/x)` where the answer is `+oo`, and the two cannot be told apart at the point where the
value is read off. Stirling's expansion answers it by reading instead. The other two are
two-sided limits at `0` whose base has no two-sided limit at all and which are not real to the left
of `0`, so the `1` came from the complex continuation; **their one-sided readings are unchanged** —
`lim x->0+ (1/x)^x` is still `1`.

Measured over 225 generated powers at `0` and `+oo`: 2 wrong values became right ones, 3 wrong values
became unevaluated, 1 `NaN` became unevaluated, and 3 right values became unevaluated.

[#754](https://github.com/asc-community/AngouriMath/issues/754), PR
[#760](https://github.com/asc-community/AngouriMath/pull/760).

### A factorial itself is read where it has no logarithm at all

The two readings below both need the factorial to be under a logarithm already — one supplies the
logarithm, by turning `f^g` into `e^(g * ln f)`, and the other rewrites a logarithm that is written
down. Neither reaches `x! / x^x`, which has none:

```
lim x->+oo x! / x^x           was  unevaluated   is  0
lim x->+oo x^x / x!           was  unevaluated   is  +oo
lim x->+oo x! / e^x           was  unevaluated   is  +oo
lim x->+oo ln(x! / x^x) / x   was  a 20 s+ hang  is  -1
lim x->+oo ln(x! / x^x)       was  unevaluated   is  -oo
```

The logarithm is supplied instead: for a positive expression `lim H` is `e^(lim ln H)`, and `ln H` is
where the expansion applies. The guard is the power of the factorial the expression depends on, which
is exactly the coefficient `ln(f!)` carries in `ln H`, so the dropped `1/(12f)` has to vanish against
it — `(x!)^x` gives `x`, and `x/(12x)` does not, so it is refused.

Not by substituting `e^(Stirling(f))` for the factorial, which is the obvious move and measured much
worse: it puts an `e` to a large exponent into the expression, the machinery evaluates that constant
to a hundred-digit decimal, and everything downstream carries it. `lim x->+oo (x!/e^x)^(1/x)` went
from half a second to over a minute that way, on an expression the library already answered.

The rewrite that reads a *logarithm* is broadened at the same time, from `ln(f!)` to any logarithm
holding a diverging factorial, so `ln(x! / x^x)` is taken apart rather than left as one opaque node.

Over 70 generated factorial expressions, against the release before these three changes: **33
answered → 66 answered, 25 timeouts → 1, and nothing lost**. Values spot-checked numerically at up to
`x = 1e6`.

[#754](https://github.com/asc-community/AngouriMath/issues/754), PR
[#767](https://github.com/asc-community/AngouriMath/pull/767).

### A factorial's logarithm is read wherever it appears

The expansion below arrived inside the rule that turns `f^g` into `e^(g * ln f)`, so it reached a
factorial only under a vanishing exponent — the harder question answered and the easier one left:

```
lim x->+oo ln(x!) / (x * ln(x))   was  unevaluated   is  1
lim x->+oo ln(x!) / x             was  unevaluated   is  +oo
lim x->+oo ln(x!) / x^2           was  unevaluated   is  0
lim x->+oo ln(x!) - x * ln(x)     was  unevaluated   is  -oo
lim x->+oo ln((2*x)!) / (x*ln(x)) was  unevaluated   is  2
lim x->+oo ln(x!) / ln(x)         was  NaN           is  +oo
```

The last is the one that was a **wrong answer** rather than a missing one: `NaN` claims the limit
does not exist, and that quotient is asymptotic to `x` — 9.6e11 at `x = 1e12`.

Applying the expansion wherever a factorial's logarithm appears is **not** unconditionally sound, so
it is guarded. What Stirling drops is `1/(12f)`, and what that costs the answer is the rate at which
the answer moves with the logarithm — so the coefficient is found by putting a variable where the
logarithm is and differentiating, and the rewrite is refused unless `coefficient / f` vanishes.
`x * (ln(x!) - (x*ln(x) - x + ln(2*pi*x)/2))` is `1/12`, built out of the dropped term itself, and an
unguarded rewrite would answer it `0`.

**What it costs.** `lim x->+oo ln((x+1)!) / (x*ln(x))` was unevaluated in about a second and now
takes longer than 20 s to be unevaluated. The rewrite is right and the expression it hands over is
right; the machinery cannot take that limit quickly, which is demonstrable without any of this by
writing the expanded form out by hand. One of 70 in a generated factorial sweep, where 24 already
timed out before this change.

[#765](https://github.com/asc-community/AngouriMath/issues/765), PR
[#766](https://github.com/asc-community/AngouriMath/pull/766).

### A factorial under a vanishing exponent has a limit

A power whose base holds a factorial had none at all. Every route out of `ln(f)` runs through
differentiating `f`, and a factorial's derivative wants the digamma function, which this library does
not have — so the rule that reads `f^g` as `e^(g * ln f)` declined and nothing behind it had a
reading either:

```
lim x->+oo (x! / x^x) ^ (1/x)   was  unevaluated   is  1/e
lim x->+oo (x!) ^ (1/x)         was  unevaluated   is  +oo
lim x->+oo (x!) ^ (1/ln(x))     was  unevaluated   is  +oo
lim x->+oo (x!) ^ (1/x^2)       was  unevaluated   is  1
lim x->+oo (x! / e^x) ^ (1/x)   was  unevaluated   is  +oo
lim x->+oo ((x+1)! / x!) ^ (1/x)  was unevaluated  is  1
```

Stirling's expansion is stated for exactly that logarithm — `ln(f!)` is
`f*ln(f) - f + ln(2*pi*f)/2 + 1/(12f) + O(1/f^3)` — and it is applied to the **exponent** rather than
substituted for the factorial in the base. That is what makes it sound: what is dropped here
*vanishes*, where the asymptotic for `f!` itself has an error that is merely relative and survives
being raised to a power. Vanishing is still not enough on its own, since the dropped term is
multiplied by the exponent it sits under, so `power / f -> 0` is required; `(x!)^x` fails that and is
left alone.

Every one of these values was checked numerically before being claimed — `(x!/x^x)^(1/x)` is
`0.3678794453` at `x = 1e9` against `1/e = 0.3678794412`. Note that **SymPy 1.14.0 answers that one
`0`**, so it is not a usable oracle here.

Nothing without a factorial in it changes: over 225 generated powers, six results differ and all six
are one of these, every one of them an unevaluated node becoming a value. `casbench` 112/117 →
113/117.

[#754](https://github.com/asc-community/AngouriMath/issues/754), PR
[#764](https://github.com/asc-community/AngouriMath/pull/764).

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

### A quadratic denominator with a symbolic leading coefficient answered `NaN`

`1/(4 + 9*x^2)` has always come back as an arctangent. `1/(a^2 + b^2*x^2)` came back as `NaN` — the
claim that no antiderivative exists, for one of the most common integrals there is.

```
int 1/(a^2 + b^2*x^2) dx     was  NaN + C
                             is   a piecewise, the arctangent where 4*a^2*b^2 > 0
int 1/sqrt(a*x^2 + c) dx     was  NaN + C
                             is   a piecewise on the sign of a
```

Both patterns build their `a = 0` arm as though the denominator still had an `x` term —
`(k/b) ln|bx + c|` and `2k sqrt(bx + c)/b` — each dividing by a `b` that is zero whenever the
denominator is written without one. The arm is guarded afterwards by a `Providedf` on `a = 0`, so a
numeric leading coefficient makes it decidably unreachable and it is dropped before anything looks
inside; a symbolic one leaves it in place, and a `Providedf` carrying `NaN` takes the whole piecewise
with it. Every test of these shapes used numbers, where the defect cannot be reached.

The arm was also the wrong formula, not merely undefined: where `a = 0` and `b = 0` the integrand is
the constant `k/c` and its antiderivative is `kx/c`, which is what it now says.

Measured over the 1774 fair problems of Rubi's independent test suites: answered 536 -> 543, wrong
7 -> 0. Three problems that were never answered moved from unevaluated to timeout, the `NaN` having
served as an accidental early exit.

PR [#774](https://github.com/asc-community/AngouriMath/pull/774).

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

### `Expand` of a quotient of factorials threw instead of expanding

```csharp
"(x + 1)! / x!".ToEntity().Expand();
// was  AngouriBugException: SmartExpandOver must be only called of non-sum expression
// is   x + 1

"(x + 2)! / x!".ToEntity().Expand();
// was  AngouriBugException
// is   x ^ 2 + 3 * x + 2
```

Expansion cancels a quotient of factorials before it does anything else, and the cancellation can
leave a sum — `(x + 1)! / x!` is `x + 1`. The result was then handed straight back to the routine
that expands a *non*-sum, which asserts that it never receives one, so the assertion fired and an
`AngouriBugException` came out of a public method. Sending the cancelled expression through the
sum-aware entry point instead costs nothing when it is not a sum, since that is where everything
else goes anyway.

**`AngouriBugException` is the library reporting that the library is broken**, so nothing was
depending on this; the entry is here because the same call now returns a value where it used to
throw. `Simplify` was never affected — it answered `1 + x` throughout.

Issue [#817](https://github.com/asc-community/AngouriMath/issues/817).

### The deprecated members are gone

2.0 follows semantic versioning, so everything carrying `[Obsolete]` has been removed — 28 members
in all. Each obsolete message named its replacement, and those replacements are what to move to:

| removed | use instead |
|---|---|
| `MathS.Matrices.Matrix(Entity[,])` | `MathS.Matrix` |
| `MathS.Matrices.Vector(params Entity[])` | `MathS.Vector` |
| `MathS.Matrices.MatrixMultiplication`, `DotProduct` | `operator *` |
| `MathS.Matrices.ScalarProduct(a, b)` | `a.T * b` |
| `MathS.Matrices.Interval(...)` ×2 | `MathS.Interval` |
| `MathS.Compute.Derivative(expr, x)` ×2 | `expr.Differentiate(x)` |
| `MathS.Compute.Integral(expr, x)` ×2 | `expr.Integrate(x)` |
| `MathS.Compute.Limit(expr, x, to)` ×2 | `expr.Limit(x, to)` |
| `MathS.Compute.DefiniteIntegral(...)` ×3 | **`expr.DefiniteIntegral(x, from, to)` — new, see below** |
| `Entity.Derive(x)`, `"...".Derive(x)` | `Differentiate` |
| `Entity.Matrix.Shape` | `RowCount` and `ColumnCount` |
| `Setting.Global`, `Setting.RollBackToDefault` | `Setting.Set` in a `using` |
| `CompilationProtocol`'s six converter delegates | inherit and override the methods |
| `CompilationProtocolBuiltinConstantConverters` | `CompilationProtocol` |

**One of those replacements did not exist.** `MathS.Compute.DefiniteIntegral` was deprecated in
favour of "non-static methods at `Entity`", and there was no `Entity.DefiniteIntegral` — every other
member of that group had a counterpart and this one did not. Removing the group as it stood would
have taken numeric definite integration out of the library with nothing to replace it, so
`Entity.DefiniteIntegral(x, from, to, stepCount = 100)` is added here, in real and complex bound
overloads.

### `round`, `min`, `max` and `gcd` exist, so they no longer raise

```csharp
"round(5/2)".ToEntity().Simplify();   // was UnrecognizedFunctionParseException, is 2
"min(3, 5)".ToEntity().Simplify();    // was UnrecognizedFunctionParseException, is 3
"gcd(1/2, 1/3)".ToEntity().Simplify();// was UnrecognizedFunctionParseException, is 1/6
```

**`round` is half to even**, which is what Python, SymPy, Mathematica and IEEE 754 all mean by
rounding, and what .NET's `Math.Round` does by default: `round(1/2)` is `0`, `round(5/2)` is `2`.
It is deliberately **not** `floor(x + 1/2)`, which disagrees at every tie, and there is a test
pinning that they differ.

`min`, `max` and `gcd` take any number of arguments and fold, so `min(3, 5, 1)` is `1`. `min` and
`max` compare only where the arguments are ordered — an unordered pair is left as the node rather
than guessed at, as SymPy's `Min` does. `gcd` covers integers and rationals, `gcd(1/2, 1/3)` being
`1/6` exactly as SymPy gives; the polynomial case is left unevaluated for now even though this
library computes polynomial gcds elsewhere.

`trunc`, `lcm`, `erf` and `conjugate` are still refused by name.

Issue [#809](https://github.com/asc-community/AngouriMath/issues/809).

### `floor` and `ceil` exist, so they no longer raise

```csharp
"floor(x)".ToEntity();
// was  UnrecognizedFunctionParseException: floor is not a function this library has
// is   floor(x)

"floor(x) - 3 = 0".ToEntity().Solve("x");
// was  UnrecognizedFunctionParseException
// is   { 3 + t_1 provided t_1 in RR and t_1 >= 0 and t_1 < 1 }
```

`ceiling(` is accepted as well, since that is SymPy's spelling; `Stringize` prints the short
`ceil(`. Both round toward the infinities rather than toward zero — `floor(-3/2)` is `-2` — and both
are taken componentwise on a complex argument, which is what SymPy and Mathematica do. Every value
in the test file was measured against SymPy 1.14 rather than reasoned about.

**If you were catching `UnrecognizedFunctionParseException` around these names, that is now dead
code.** Nothing else moves: `round`, `trunc`, `min`, `max`, `gcd` and `lcm` are still refused by
name, and `floor` on its own — without a bracket — is still an ordinary variable.

The derivative is `0 provided not x in ZZ`: flat between the integers, undefined at each of them.
SymPy leaves that derivative unevaluated; this library states the condition instead, which is the
stance `Signumf` and `Absf` already take.

Issue [#809](https://github.com/asc-community/AngouriMath/issues/809).

### A parse failure no longer escapes as a `NullReferenceException`

```csharp
"(1 provided x > 0, 2)".ToEntity();
// was  NullReferenceException
// is   UnhandledParseException: line 1:17 no viable alternative at input '(1providedx>0,'
```

ANTLR reports a syntax error to the listener and then *recovers*, carrying on with a rule context
whose value was never assigned. The grammar's action for `provided` runs anyway and dereferences it,
so the parse came down before the error already recorded against the input was read.

**Change your `catch` clause if you were catching `NullReferenceException` around user input** — but
the point of the fix is that you should not have had to.
[`Docs/Usage/Exceptions.md`](Sources/AngouriMath/Docs/Usage/Exceptions.md) says every parse failure
arrives under `AngouriMathBaseException`, and a caller who wrote the one correct `catch` was not
catching this.

The construct is unaffected: `1 provided x > 0` parses, and so does the piecewise form
`piecewise(1 provided x > 0, 2 provided x < 0, 3)`. What is refused is the comma list with no
`piecewise` in front of it, which has no rule.

Issue [#813](https://github.com/asc-community/AngouriMath/issues/813).

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

- **The branches a simplification went down and came back from.**
  `DerivationPath.Abandoned`, beside the `Steps` it kept. The data was recorded already —
  reconstructing the path searched the recorded edges and discarded everything not on the chain —
  so this exposes it rather than collecting anything new. Each abandoned step carries the
  expression it left from, which is the branch's root. Deduplicated against itself and against
  the kept chain: the raw edges are mostly one rewrite recorded once per level of the candidate
  search, and `x^(-1)/(y/z)` produced 425 of them across 13 distinct steps.
  [#273](https://github.com/asc-community/AngouriMath/issues/273).
- **An analytical solver for first-order linear ordinary differential equations.**
  `MathS.SolveOde(equation, function, variable)`, by the integrating factor. The unknown is written
  as an application — `apply(y, x)` — rather than a bare variable, because `derivative(y, x)` is
  `0`: a variable does not depend on `x`. `y' + y = 1` comes back as `1 + C_1 * e ^ (-x)`, and
  `y' + y/x = 1` as `C_1 / x + x / 2`. It returns `null` where the equation is not linear in the
  unknown and its derivative, and where either of the two integrals has no closed form, which is
  why `y' + y = e^(x^2)` is declined. Nothing existed before it; there is no behaviour to compare.
  [#241](https://github.com/asc-community/AngouriMath/issues/241).
- **Integrals wanting a substitution by a power of the variable that occurs nowhere.**
  `int x / (x^4 + 1)` is `arctan(x^2)/2 + C`, and so for `x/(x^4 - 1)`, `x/(x^4 + 4)`,
  `x^2/(x^6 + 1)`, `x/(x^6 + 1)`, `x^3/(x^8 + 1)` and `x^3/(x^12 + 1)`. Each was previously
  returned unevaluated. Two things had to change: `x^2` is now offered as a candidate although
  it is written nowhere in `x / (x^4 + 1)`, and substituting it rewrites `x^4` as `(x^2)^2` so
  that there is something for it to replace. `int x^3 / (x^4 + 1)` worked throughout, its own
  substitution being one that occurs, which is what made the gap hard to see. No integral that
  was answered before is answered differently.
  [#233](https://github.com/asc-community/AngouriMath/issues/233).
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

## 1.4.0 — one change that was never written down

This file begins at 2.0.0, so a break introduced *by* 1.4.0 has had no home in it. One is worth
recording, because 1.4.0 is what a `dotnet add package` installs today and the change is silent
until a string is parsed.

### `integral` takes bounds, not an iteration count

| | 1.3.0 | 1.4.0 onwards |
|---|---|---|
| `integral(f, x, 1)` | accepted, read as `integral(f, x)` | `FunctionArgumentCountException` |
| `integral(x, x, 2)` | the second antiderivative, `x ^ 3 / 6` | `FunctionArgumentCountException` |
| `integral(x, x, 0, 1)` | `FunctionArgumentCountException` | the definite integral from `0` to `1` |

The third argument used to be a repetition count and is now the lower bound of a range, so three
arguments name neither form and are refused. `derivative(f, x, n)` is unaffected and still takes an
order, which is why the two functions read differently.

This was deliberate — PR [#657](https://github.com/asc-community/AngouriMath/pull/657), "change
integral node to support range instead of iterations" — and it is covered by a test. It simply went
out without a note. Both samples in this repository called `integral(f, x, 1)` and had been failing
against 1.4.0 since January without anyone seeing it, because they were pinned to 1.3.0; PR
[#846](https://github.com/asc-community/AngouriMath/pull/846) is what surfaced it.

**What to do.** `integral(f, x, 1)` becomes `integral(f, x)` — identical in meaning, since 1.3.0
turned the former into the latter anyway. A count of two or more was not a no-op and is the part
that is actually gone: nest the calls instead, `integral(integral(f, x), x)`, which 1.3.0 accepted
as well. Expect the answer to differ by the constants of integration, which the same PR began
adding — where `integral(x, x, 2)` gave `x ^ 3 / 6` on 1.3.0, the nested form gives
`C_1 + C * x + x ^ 3 / 6` from 1.4.0 onwards.

---

## If one of these hurts

Open an issue at https://github.com/asc-community/AngouriMath/issues saying what you were relying
on and what it was for. A change made because the old answer was wrong will not be reverted to the
wrong answer, but where the old behaviour was *useful* — a tolerance, a printed form, a name — there
is usually a way to have both, and the setting or the overload to add is worth knowing about.
