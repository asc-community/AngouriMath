# How AngouriMath compares to other systems

This page is what happened when the library was put beside systems it did not write: two other
.NET symbolic libraries, SymPy, and Rubi's integration test-suite. The tables are the measurements
themselves.

They are here because [#184](https://github.com/asc-community/AngouriMath/issues/184) asked for
a comparison against another CAS on a fixed corpus, and item 40 of
[#746](https://github.com/asc-community/AngouriMath/issues/746) asks for the result to be
**published** — the runs had been made and nothing in this repository said what they found.

Two rules govern the writing. **The rows where this library loses are in the tables**, because a
comparison that shows only the wins measures its author rather than its subject. And **every
sentence that draws a conclusion is a count taken from the table directly above it**, so that a
sentence cannot go on being true after its table has stopped being.

## What was measured, against what, and when

| comparison | the other side | this library | measured | harness |
|---|---|---|---|---|
| capability, output and speed | Math.NET Symbolics 0.25.0 and Symbolism 1.0.4, on .NET 10.0.10 | `6b93b401`, the 2.3.0 release commit | 2026-08-23 | `libcompare` |
| 80 feature probes over 23 areas | SymPy 1.14.0 | `6b93b401` | 2026-08-23 | `sympyparity` |
| 1,774 integration problems | Rubi's test-suite, twelve of its files | `e4eefde0` | 2026-08-23 | `intbench` |

`e4eefde0` is later than the 2.3.0 tag the other two rows measure: it is `master` after nine pull
requests landed, so the Rubi figures are the newer ones. That is deliberate — the suite has to be
downloaded rather than vendored, so this harness is re-run on its own schedule, and the run behind
this table is the first taken on merged `master`.

**What the reports do not carry.** Only `sympyparity` writes a commit into its own output, taken
from the project reference it built against, so a moved checkout cannot make it lie. The other two
record neither a commit nor a date of their own; the ones above come from the analysis workspace's
history. `intbench` does not record its invocation either — what fixes its slice is the twelve
source files in its table, which are Rubi's *independent test suites*, the problems taken from
textbooks, and not its systematic families.

`comparison.md` also prints nuget.org download counts for the three packages "at time of writing",
with no date on them. Downloads measure adoption and change daily, so they are not reproduced here.

## 1. Math.NET Symbolics and Symbolism: what each library has

The Math.NET column is filled in by reflecting over its assembly rather than by reading its
documentation.

| Capability | AngouriMath | Math.NET Symbolics | Symbolism |
|---|---|---|---|
| Parse infix string | yes | yes | no (expression trees only) |
| Print infix | yes | yes | yes |
| LaTeX output | yes | yes | no |
| MathML output | no | yes | no |
| Differentiate | yes | yes | yes |
| Integrate (symbolic) | yes | **no** | **no** |
| Limits | yes | **no** | **no** |
| Solve equations | yes | **no** | yes (basic) |
| Solve inequalities | yes | no | no |
| Systems of equations | yes | no | no |
| Expand | yes | yes | yes |
| Rational simplify / cancel | yes | yes | yes |
| Polynomial GCD, division, partial fractions | partial | yes | no |
| Taylor series | yes | yes | no |
| Matrices / linear algebra | yes | no | no |
| Sets, intervals, set algebra | yes | no | no |
| Boolean algebra and predicates | yes | no | no |
| Piecewise functions | yes | no | no |
| Compile expression to delegate | yes | yes | no |
| Arbitrary-precision arithmetic | yes (PeterO.Numbers) | no (BigRational + double) | no (double) |
| Complex numbers | yes, first class | partial | partial |
| Multithreading / cancellation controls | yes | no | no |
| F# wrapper | yes (AngouriMath.FSharp) | native F# | no |
| Interactive/notebook integration | yes (.NET Interactive) | no | no |

Of the 24 rows, **eleven** are `yes` here and `no` in both of the others: symbolic integration,
limits, inequalities, systems of equations, matrices, sets, boolean algebra, piecewise functions,
arbitrary-precision arithmetic, cancellation controls and notebook integration. **Two** go the
other way, and both are Math.NET's: MathML output, which this library does not have at all, and
polynomial GCD, division and partial fractions, where this library is `partial`. No row is one
Symbolism has and this library does not.

That compares surfaces. A `yes` here says a public entry point exists and answers; it does not say
it answers as well as another system's, and the rest of this page is where that gets tested.

## 2. The same task, put to both

| Task | Input | AngouriMath | Math.NET Symbolics |
|---|---|---|---|
| differentiate | `x^3 + 2*x^2 + x + 1` | `1 + 3 * x ^ 2 + 4 * x` | `1 + 4*x + 3*x^2` |
| differentiate | `sin(x)*cos(x)` | `cos(2 * x)` | `-(sin(x))^2 + (cos(x))^2` |
| differentiate | `ln(x)/x` | `(1 - ln(x)) / x ^ 2 provided not x = 0` | `1/x^2 - ln(x)/x^2` |
| differentiate | `(x^2+1)/(x-1)` | `1 + (-2) / (x - 1) ^ 2` | `(2*x)/(-1 + x) - (1 + x^2)/(-1 + x)^2` |
| differentiate | `x^x` | `(1 + ln(x)) * x ^ x provided not x = 0` | `x^x + x^x*ln(x)` |
| expand | `(x + 1)^3` | `1 + 3 * x + 3 * x ^ 2 + x ^ 3` | `1 + 3*x + 3*x^2 + x^3` |
| expand | `(a + b)*(c + d)` | `a * c + a * d + b * c + b * d` | `a*c + b*c + a*d + b*d` |
| expand | `(x + 1)^2*(x + 2)^2` | `4 + 12 * x + 13 * x ^ 2 + 6 * x ^ 3 + x ^ 4` | `4 + 12*x + 13*x^2 + 6*x^3 + x^4` |
| simplify | `(x^2 - 1)/(x - 1)` | `x + 1 provided not x - 1 = 0` | `1 + x` |
| simplify | `x/x + 0*y` | `1 provided not x = 0` | `1` |
| simplify | `2*x + 3*x - x` | `4 * x` | `4*x` |
| simplify | `a*c + a*d + b*c + b*d` | `(a + b) * (c + d)` | `a*c + b*c + a*d + b*d` |

Of the twelve tasks the two libraries give the same answer on **3**. That comparison ignores
spacing, because this library prints `3 * x` where Math.NET prints `3*x`, and comparing raw
characters measures the spacing rather than the agreement. Of the remaining nine, Math.NET's answer
is shorter on **4**, this library's is shorter on **3**, and **2** are the same length and differ
only in the order of the operands.

**All four rows where Math.NET is shorter are rows where this library attached a `provided`
clause**, which is the trade the table is really showing. For `(x^2 - 1)/(x - 1)` Math.NET answers
`1 + x` with no condition, and `1 + x` has a value at `x = 1` where the input has none; this
library answers `x + 1 provided not x - 1 = 0`. Same for `x/x + 0*y`. Carrying a removable
singularity costs the shorter output on those rows and is deliberate — see
[SimplificationContract.md](../Contributing/SimplificationContract.md), which is where a rewrite's
right to attach or drop such a condition is set out.

The two equal-length rows differ by collection and ordering: `1 + 3 * x ^ 2 + 4 * x` against
`1 + 4*x + 3*x^2` is the same polynomial written in a different order.

## 3. Speed, on the four operations both can do

| Operation | AngouriMath (us) | Math.NET Symbolics (us) | Verdict |
|---|--:|--:|---|
| parse a fresh polynomial | 729.5 | 9.0 | Math.NET 81.1x faster |
| differentiate `sin(x)*cos(x)` | 9.3 | 1.8 | Math.NET 5.2x faster |
| expand `(x + 1)^5` | 83.7 | 9.6 | Math.NET 8.7x faster |
| simplify `(x^2 - 1)/(x - 1)` | 2782.1 | 25.2 | Math.NET 110.4x faster |

Math.NET is faster on every operation measured, by between 5.2x and 110.4x. The method is a median
of 15 runs after 5 warm-up runs, with both libraries timed in the same process. The microsecond
columns therefore describe one machine and should not be compared against a number from any other
run; the ratio within a row is an A/B taken in a single process and travels better.

Four operations are not a performance profile. They are the operations *both* libraries have, so
they say nothing about the eleven capabilities in §1 that only one of them has. This library's own
release-to-release timings are in
[`WhatsNew/version_performance_control.md`](../WhatsNew/version_performance_control.md).

## 4. SymPy 1.14.0

Eighty probes over 23 areas, each side run independently — SymPy in the harness's Python half, this
library in its C# half — with neither half seeing the other's answer.

| verdict | probes |
|---|--:|
| answers | 28 |
| declines | 9 |
| internal | 3 |
| errors | 1 |
| absent | 35 |
| unclear | 4 |

**The verdict is computed from this library's answer alone, not from agreement.** `answers` means
something other than the input came back; `declines` means the input came back unchanged, or an
unevaluated `integral(...)`, `sum(...)` or `gcd(...)` node; `internal` means a member of that name
exists but is not public; `absent` means no exported member's name contains the capability's word.

So **`answers` does not mean `agrees`**. Several rows have both sides answering and the answers
differing, and nothing in this page adjudicates which is right.

The one `errors` row is `x^2 - 4 > 0`: SymPy returns two open intervals and this library raises
`NotSufficientlySupportedException`. That row and §1's `Solve inequalities: yes` are both true at
once, which is what makes a capability table a comparison of surfaces.

And `absent` is a keyword search of the public surface, not an inventory: a capability exported
under a name sharing no word with the probe would read as absent. It is a lower bound on what is
present. The search is what turned up `MathS.Series.Taylor`, `MathS.NumberTheory.Factorize` and
`SolveDiophantineEquation`, which a feature list written from memory had as missing.

Long answers are truncated in the table below at 64 characters, with `…`.

| area | capability | SymPy 1.14 | AngouriMath | |
|---|---|---|---|---|
| simplify | trigonometric simplification | `1` | `1` | answers |
| simplify | rational function cancellation | `x + 1` | `x + 1 provided not x - 1 = 0` | answers |
| simplify | radical denesting | `sqrt(2) + sqrt(3)` | `sqrt(5 + 2 * sqrt(6))` | declines |
| simplify | combine logarithms | `log(x*y)` | `ln(x) + ln(y)` | declines |
| simplify | hypergeometric simplification | `x` | `x` | answers |
| simplify | trigsimp of a product | `sin(2*x)` | `sin(2 * x)` | answers |
| polys | factor over the rationals | `(x - 1)*(x + 1)*(x**2 + 1)` | `x ^ 4 - 1` | declines |
| polys | multivariate gcd | `x*y` | `gcd(x ^ 2 * y + x * y ^ 2, x * y)` | declines |
| polys | resultant | `9` | `internal only, matching "Resultant": PolynomialResultant, Resul…` | internal |
| polys | square-free decomposition | `(x**2 - 1)**2` | `internal only, matching "SquareFree": FactorSquareFree, IsSquar…` | internal |
| polys | Groebner basis | `GroebnerBasis([x - y, 2*y**2 - 1], x, y, domain='ZZ', order='le…` | `internal only, matching "Groebner": GroebnerBudget, GroebnerSys…` | internal |
| polys | factor over a finite field | `(x + 1)**2` | `public members matching "modulus": op_Modulus` | unclear |
| polys | partial fractions | `-1/(2*(x + 1)) + 1/(2*(x - 1))` | `<no member at all matching "Apart">` | absent |
| polys | minimal polynomial of an algebraic number | `x**4 - 10*x**2 + 1` | `<no member at all matching "MinimalPolynomial">` | absent |
| solvers | polynomial equation | `{-2, 2}` | `{ 2, -2 }` | answers |
| solvers | transcendental equation | `{log(2)}` | `{ ln(2) }` | answers |
| solvers | linear system | `{(2, 1)}` | `[[2, 1]]` | answers |
| solvers | nonlinear system | `{(-sqrt(2)/2, -sqrt(2)/2), (sqrt(2)/2, sqrt(2)/2)}` | `[[1/2 * sqrt(2), 1/2 * sqrt(2)], [-1/2 * sqrt(2), -1/2 * sqrt(2…` | answers |
| solvers | ODE | `Eq(f(x), C1*exp(-x) + x - 1)` | `<no member at all matching "Dsolve">` | absent |
| solvers | PDE | `pdsolve exists: True` | `<no member at all matching "Pdsolve">` | absent |
| solvers | diophantine equation | `{(-3, 4), (3, 4), (4, -3), (4, 3), (-4, -3), (-5, 0), (-4, 3), …` | `(0, 25)` | answers |
| solvers | recurrence relation | `C0 + n` | `<no member at all matching "Rsolve">` | absent |
| solvers | inequality | `Union(Interval.open(-oo, -2), Interval.open(2, oo))` | `!!! NotSufficientlySupportedException: Inverting this node woul…` | errors |
| solvers | numeric root | `0.739085133215161` | `{ -47.02744973977255682484610588289797306060791015625 - 4.54842…` | answers |
| integrals | indefinite integral | `x**3/3` | `x ^ 3 / 3 + C` | answers |
| integrals | definite integral | `1/3` | `1/3` | answers |
| integrals | integral needing Risch | `sqrt(pi)*erf(x)/2` | `integral(e ^ (-x ^ 2), x)` | declines |
| integrals | integration by parts | `(x - 1)*exp(x)` | `e ^ x * (x - 1) + C` | answers |
| integrals | trigonometric substitution | `asin(x)` | `C - arcsin(-x)` | answers |
| integrals | Laplace transform | `(1/(s + 1), -1, True)` | `<no member at all matching "Laplace">` | absent |
| integrals | Fourier transform | `sqrt(pi)*exp(-pi**2*k**2)` | `<no member at all matching "Fourier">` | absent |
| integrals | multiple integral | `1/4` | `<no member at all matching "multiple integral">` | absent |
| series | limit | `1` | `1` | answers |
| series | one-sided limit | `oo` | `+oo` | answers |
| series | Taylor series | `x - x**3/6 + x**5/120 + O(x**6)` | `x ^ 5 / 120 - x ^ 3 / 6 + x` | answers |
| series | asymptotic expansion | `exp(x)/x` | `internal only, matching "Asymptotic": AsymptoticSeries` | internal |
| series | residue | `1` | `<no member at all matching "Residue">` | absent |
| concrete | symbolic summation | `n**2/2 + n/2` | `sum(k, k, 1, n)` | declines |
| concrete | infinite series | `pi**2/6` | `sum(1 / k ^ 2, k, 1, +oo)` | declines |
| concrete | symbolic product | `factorial(n)` | `product(k, k, 1, n)` | declines |
| matrices | determinant | `x*y - 2` | `x * y - 2` | answers |
| matrices | inverse | `Matrix([[-2, 1], [3/2, -1/2]])` | `[[-2, 1], [3/2, -1/2]]` | answers |
| matrices | eigenvalues | `{3: 1, -1: 1}` | `<no member at all matching "Eigen">` | absent |
| matrices | row echelon form | `(Matrix([ [1, 2], [0, 0]]), (0,))` | `internal only, matching "Rref": rref` | internal |
| matrices | characteristic polynomial | `lambda**2 - 2*lambda - 3` | `<no member at all matching "CharacteristicPolynomial">` | absent |
| matrices | Jordan form | `Matrix([[1, 1], [0, 1]])` | `<no member at all matching "Jordan">` | absent |
| ntheory | integer factorisation | `{2: 3, 3: 2, 5: 1}` | `2^3 * 3^2 * 5^1` | answers |
| ntheory | primality | `True` | `True` | answers |
| ntheory | Euler totient | `12` | `12` | answers |
| ntheory | Moebius | `-1` | `<no member at all matching "Mobius">` | absent |
| ntheory | Chinese remainder | `(23, 105)` | `<no member at all matching "Crt">` | absent |
| ntheory | continued fraction | `[4, 2, 6, 7]` | `<no member at all matching "ContinuedFraction">` | absent |
| logic | boolean simplification | `a` | `a` | answers |
| logic | satisfiability | `False` | `<no member at all matching "Satisfiable">` | absent |
| logic | conjunctive normal form | `(a \| c) & (b \| c)` | `<no member at all matching "Cnf">` | absent |
| sets | interval intersection | `Interval(1, 2)` | `[1; 2]` | answers |
| sets | set-builder | `ConditionSet(x, x > 0, Reals)` | `{ x : x > 0 }` | answers |
| sets | image of a set | `Interval(0, 4)` | `<no member at all matching "ImageSet">` | absent |
| functions | gamma | `sqrt(pi)` | `sqrt(pi)` | answers |
| functions | zeta | `pi**2/6` | `<no member at all matching "Zeta">` | absent |
| functions | error function | `0` | `<no member at all matching "ErrorFunction">` | absent |
| functions | Bessel | `1` | `<no member at all matching "Bessel">` | absent |
| functions | Lambert W | `0` | `<no member at all matching "LambertW">` | absent |
| functions | polylogarithm | `pi**2/6` | `<no member at all matching "Polylog">` | absent |
| assumptions | assumption-driven simplification | `p` | `sqrt(p ^ 2)` | declines |
| assumptions | ask a predicate | `True` | `public members matching "Assumptions": SoundUnderAssumptions` | unclear |
| printing | LaTeX output | `\int x^{2}\, dx` | `\int {x}^{2}\,\mathrm{d}x` | answers |
| printing | C code generation | `pow(x, 2) + sin(x)` | `<no member at all matching "CCode">` | absent |
| printing | MathML output | `mathml exists: True` | `<no member at all matching "MathML">` | absent |
| discrete | convolution | `[3, 10, 8]` | `<no member at all matching "Convolution">` | absent |
| discrete | fast Fourier transform | `[10, -2 - 2*I, -2, -2 + 2*I]` | `<no member at all matching "Fft">` | absent |
| combinatorics | permutation group | `PermutationGroup exists: True` | `<no member at all matching "PermutationGroup">` | absent |
| stats | probability of an event | `1/2` | `<no member at all matching "Probability">` | absent |
| geometry | line intersection | `[Point2D(1/2, 1/2)]` | `<no member at all matching "Geometry">` | absent |
| physics | units | `units exists: True` | `<no member at all matching "Units">` | absent |
| vector | vector calculus | `CoordSys3D exists: True` | `<no member at all matching "CoordSys">` | absent |
| tensor | tensor algebra | `tensor exists: True` | `<no member at all matching "TensorIndex">` | absent |
| diffgeom | differential geometry | `Manifold exists: True` | `<no member at all matching "Manifold">` | absent |
| holonomic | holonomic functions | `expr_to_holonomic exists: True` | `<no member at all matching "Holonomic">` | absent |
| codegen | compile to a callable | `9` | `9` | answers |

### One row has moved since the run

**concrete / symbolic summation** was measured as `declines` at `6b93b401` and is answered now:
`sum(k, k, 1, n)` is `(n + n^2)/2`, carrying the condition `n >= 0` that this library's
empty-range convention requires and SymPy's reversed-range convention does not. The row is left
as it was measured rather than edited, because the table is a record of one run at one commit and
a hand-corrected cell would be a measurement nobody took; it will read `answers` when
`sympyparity` is next run.

The two rows either side of it have **not** moved. *Infinite series* is still declined —
`sum(1 / k ^ 2, k, 1, +oo)` needs the zeta function, which the row four below records as absent.
*Symbolic product* is still declined, and deliberately: `factorial(n)` is undefined at the
negative integers where the empty product is `1`, so answering it needs a condition of the same
kind the sum now carries.

### Implemented, but not reachable from outside

The gap in these five is a public entry point, not an algorithm.

- **polys / resultant** — `internal only, matching "Resultant": PolynomialResultant, Resultant`
- **polys / square-free decomposition** — `internal only, matching "SquareFree": FactorSquareFree, IsSquareFree, SquareFreeDecomposition, SquareFreePart…`
- **polys / Groebner basis** — `internal only, matching "Groebner": GroebnerBudget, GroebnerSystemSolver`
- **series / asymptotic expansion** — `internal only, matching "Asymptotic": AsymptoticSeries`
- **matrices / row echelon form** — `internal only, matching "Rref": rref`

### Absent, by area

No exported member of this library's public surface matched these capabilities' words.

- **combinatorics** — permutation group
- **diffgeom** — differential geometry
- **discrete** — convolution, fast Fourier transform
- **functions** — Bessel, Lambert W, error function, polylogarithm, zeta
- **geometry** — line intersection
- **holonomic** — holonomic functions
- **integrals** — Fourier transform, Laplace transform, multiple integral
- **logic** — conjunctive normal form, satisfiability
- **matrices** — Jordan form, characteristic polynomial, eigenvalues
- **ntheory** — Chinese remainder, Moebius, continued fraction
- **physics** — units
- **polys** — minimal polynomial of an algebraic number, partial fractions
- **printing** — C code generation, MathML output
- **series** — residue
- **sets** — image of a set
- **solvers** — ODE, PDE, recurrence relation
- **stats** — probability of an event
- **tensor** — tensor algebra
- **vector** — vector calculus

### Where the two sides were not asked the same question

The probe list writes each side's input separately, so a row can compare two different questions,
and the verdict column cannot see that. The diophantine row is one: SymPy is asked for
`x^2 + y^2 = 25` and this library a linear equation, which is why one answer is a set of pairs on
a circle and the other a single pair. The numeric-root row is another: SymPy's `nsolve` is given
a starting point and this library's search is given none, which is why one returns a real root
near it and the other a set reaching into the complex plane. Read a row's two cells before reading
its verdict.

## 5. Rubi's integration test-suite

Albert Rich's suite, downloaded rather than vendored — it carries no licence statement. Every
problem comes with an antiderivative Rubi found, which is the point of using it: where that
antiderivative is elementary, an unevaluated answer from us is a **gap** and not an impossible
integral.

- 1,892 problems read, 0 lines not parseable as an entry
- 2 excluded: the integrand uses a function this library has no node for
- 116 excluded: Rubi's antiderivative is non-elementary, so the problem would be unfair
- 1,774 fair, all of which ran, at a 5-second budget

**Answered 604 of 1,774 (34.0%)**: 604 where `Integrate`'s own answer checks out and 0 more that
only checked out after `Simplify()`. 1,118 unevaluated, **0 wrong**, 8 unverifiable on the reals,
0 errors, 44 timeouts. Restricted to the 1,726 problems for which Rubi records a positive step
count — the ones it answers optimally itself — the rate is 604/1,726 (35.0%).

**The timeout column is not reproducible, and the solved column inherits that.** Two runs of *this
same commit*, same slice and same 5-second budget, answered **604 with 44 timeouts** and **602 with
46**. Every problem that differed between them moved into or out of the timeout bucket — none became
unevaluated and none became wrong — and timed on their own against this build the three take
1,744 ms, 1,579 ms and 835 ms, none close to the budget. `intbench` cannot abort a thread, so a case
that does time out leaks one and the leaked threads slow every problem after it; how many leak
depends on what else the machine is doing. So read this rate as **±3 problems**, and read the
*wrong* answer count, which is 0 in both runs, as the number that means something.

| Source | Run | Solved | +Simplify | Unevaluated | Wrong | Unverifiable | Error | Timeout | Rate |
|---|--:|--:|--:|--:|--:|--:|--:|--:|--:|
| Apostol Problems | 159 | 88 | 0 | 68 | 0 | 2 | 0 | 1 | 55% |
| Bondarenko Problems | 22 | 0 | 0 | 22 | 0 | 0 | 0 | 0 | 0% |
| Bronstein Problems | 10 | 1 | 0 | 9 | 0 | 0 | 0 | 0 | 10% |
| Charlwood Problems | 50 | 1 | 0 | 49 | 0 | 0 | 0 | 0 | 2% |
| Hearn Problems | 259 | 111 | 0 | 141 | 0 | 1 | 0 | 6 | 43% |
| Hebisch Problems | 4 | 1 | 0 | 2 | 0 | 0 | 0 | 1 | 25% |
| Jeffrey Problems | 9 | 0 | 0 | 9 | 0 | 0 | 0 | 0 | 0% |
| Moses Problems | 107 | 54 | 0 | 47 | 0 | 4 | 0 | 2 | 50% |
| Stewart Problems | 376 | 183 | 0 | 189 | 0 | 0 | 0 | 4 | 49% |
| Timofeev Problems | 666 | 162 | 0 | 478 | 0 | 1 | 0 | 25 | 24% |
| Welz Problems | 104 | 3 | 0 | 96 | 0 | 0 | 0 | 5 | 3% |
| Wester Problems | 8 | 0 | 0 | 8 | 0 | 0 | 0 | 0 | 0% |

Rubi records how many rule applications each problem took it. That is a difficulty measure produced
by a system that is not ours, so it says where this library's ceiling is rather than where we chose
to put it.

| Rubi steps | Run | Solved | Rate |
|---|--:|--:|--:|
| 1 (table lookup) | 268 | 153 | 57% |
| 13+ | 48 | 3 | 6% |
| 2-3 | 851 | 346 | 41% |
| 4-6 | 432 | 85 | 20% |
| 7-12 | 127 | 17 | 13% |
| negative (Rubi's own answer is not the optimal one) | 48 | 0 | 0% |

**How an answer is graded, and what `0 wrong` means.** Not against Rubi's answer: two
antiderivatives of one integrand differ by a constant and by arbitrarily much rewriting. Our answer
is differentiated back and compared to the integrand numerically at positive real sample points.
`0 wrong` therefore means no answer failed *that* check. The 8 unverifiable are cases where neither
the raw nor the simplified answer could be evaluated on the reals at two or more points — the
harness's symbolic parameters are bound to one fixed set of positive reals, so an integrand whose
radicand is negative throughout has nowhere real to be compared. They are listed by name in the
report rather than folded into either column, because a silent bucket reads as a clean one. The 44
timeouts count against the rate, not out of the denominator.

**The number to hold this against is the library's own corpus.** On the same 2.3.0 commit and the
same day, `casbench` — another harness in the same workspace, over 121 problems written here —
solves 116 of the 119 that have an elementary answer, 97.5%. The Rubi rate is 34.0%. Both are
correct measurements of different things: the first says that the problems we chose are solved,
the second says what happens to a list somebody else wrote down. Only the second is a measurement
of the library rather than of the corpus.

## What none of this establishes

- **A corpus is a list somebody wrote down.** 604 of 1,774 is a statement about Rubi's textbook
  problems at a 5-second budget. It is not a statement about integration in general, and a
  different suite would give a different number without the integrator changing.
- **Nothing here adjudicates a disagreement.** Where this library and another give different
  answers, this page reports both and stops. The exceptions are narrow and named: the Rubi run
  grades against the integrand, and §2's two rational rows have an argument attached to them.
  Everywhere else, a difference is unadjudicated — including every SymPy row marked `answers`.
- **Three .NET libraries picked by hand are not a survey of .NET**, and a capability matrix built
  by reflection is a surface comparison in both directions: it can miss what the other library
  calls by another name exactly as `sympyparity` can miss ours.
- **Four microbenchmarks on one machine are not a performance profile**, and none of them covers
  solving, integration or limits.
- **None of these runs is a test.** They are measurements a person reads. Nothing here fails a
  build.

## Reproducing it

Plainly: **you cannot, from a clone of this repository.** The harnesses are not part of this
repository, are not in the solution, are not run by `dotnet test`, and no CI workflow runs them.
They live in a separate analysis workspace next to a checkout of the library. Each also needs
something fetched — Rubi's suite from <https://rulebasedintegration.org/testProblems.html>, `sympy`
installed for the Python half, and `MathNet.Symbolics` and `Symbolism` from nuget for the .NET
comparison.

What *is* in this repository and does run on every commit is
`Sources/Tests/UnitTests/Corpus`: a small gate over problems with known answers, which fails on any
wrong answer and on any case that stops matching its record — including a case that gets better, so
the record cannot drift away from the library. It is a gate, not a harness, and it is not the
source of any figure on this page.

Every figure here carries the commit and the date it was taken at. A figure without one has not
been measured, whoever is quoting it.
