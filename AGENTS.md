# AGENTS.md

For AI agents working on AngouriMath. Humans: [CONTRIBUTING.md](CONTRIBUTING.md) is yours, and
everything below applies to you too.

AngouriMath is a computer algebra system. The thing being built is *mathematics*, and the code is
how it is expressed. Read this as instructions for doing mathematics well, using C# and F#.

## The one rule everything else follows from

**Be a mathematician first.** When mathematical correctness and backward compatibility disagree,
correctness wins. A published API that returns the wrong answer is not an asset to preserve; it is
a bug with users. Say so in the changelog, and change it.

The same goes for convention. If mathematicians write it one way and the library writes it another,
the library is wrong — even where the library's way is defensible in isolation. `arcsinh` is not a
thing; the inverse of `sinh` is an *area*, not an arc, so it is `arsinh` (#687). Follow the notation
of the people who use the subject, not the notation that was easiest to parse.

When you have to choose a convention, **check what the other systems answer** — SymPy,
Mathematica, Maxima — and match the mathematics rather than the language you are writing in.
`mod` takes the sign of the divisor because that is what a mathematician means by mod and what
all three of those give; C's `%` truncates, but C's `%` is an operation on machine integers.
Check it, do not reason about it from memory: this exact case was got wrong first time round.

**Consistency is the point.** [#497](https://github.com/asc-community/AngouriMath/issues/497), the
2.0 paper, names inconsistency as the central defect: *"one may find it inconsistent in a lot of
places in API, behaviour, and internal structure of code."* A rule that fires for `sin` but not
`tan`, a limit that works from both sides but not from one, an evaluation that holds precision for
large numbers and drops it for small — each is a bug even when every individual case is defensible.
When you fix something, ask what else is the same shape, and fix that too, or write down why not.

## Not answering is a legitimate answer. Answering wrongly is not.

The most important distinction in this codebase:

| result | means |
|---|---|
| unevaluated (`Limitf`, the original expression back) | "I could not settle this" |
| `NaN` | "**this does not exist**" |
| a value | "this is the value" |

These are three different claims and they are not interchangeable. Returning `NaN` for a limit you
merely failed to compute is a *wrong answer*, not a graceful failure — it tells the caller the limit
does not exist. Returning an unevaluated node is honest.

So, in order of preference: right answer > no answer > slow answer > wrong answer. A wrong answer is
worse than a hang, because a hang is visible.

Say "no answer" by returning **`null`**, not by handing back an unevaluated node of the expression
you were asked about. `Limitf(this, ...)` looks like the honest answer and is in fact a cycle: the
caller evaluates it to compare, evaluating computes the limit, and computing arrives back where it
started. That overflows the stack, which kills the process rather than raising anything catchable.
`null` reads the same to the caller and terminates.

## Output has a contract too

**Parsing what `Stringize` prints must give back the expression it printed.** Anything else makes
the printed form a lie, and a silent one, since a wrong reading is usually still a valid
expression: `(2^3)^2` printed as `2^3^2` is 512 where the expression is 64, and a piecewise
printed with `if` came back as a product with `if` read as an undeclared variable. If a node's
usual notation is not in the grammar, print the function call the parser does have.
`StringizeRoundTripTest` is where that is enforced; add to it when you add a node.

`Latexise` is under no such obligation — nothing parses LaTeX.

The syntax the parser accepts is written down in `Sources/AngouriMath/Docs/Usage/Syntax.md`.
Keep it true when you change the grammar.

## Verify the mathematics, not the string

Never assert on printed form unless the printed form *is* the bug. Assert the property:

- an integral: differentiate it back and compare to the integrand
- a root: substitute it into the equation
- an identity: subtract the two sides and simplify to zero
- a limit: check it against the value at nearby points, or a series

A string comparison passes for `2*x` and fails for `x*2`, which tells you nothing about whether the
answer is right. `Sources/Tests/UnitTests/Common/IssueRegressionTest.cs` is where issue regressions
go, named for the issue.

**Reproduce on a stock `master` build before claiming anything is broken.** More than one confident
report here has turned out to describe behaviour that was never broken.

## Aim high

The ambition is to answer the hard problems: olympiad and competition algebra, calculus and number
theory; the integrals and limits that need real machinery rather than pattern tables; the questions
where the honest current answer is "unevaluated". Concretely, that means things like Risch for
integration, a proper polynomial layer (multivariate GCD, resultants, factorisation) which most of
the open simplification issues sit behind, and quantifier elimination for inequalities.

Measure against a corpus of problems with known answers, and report **wrong / error / timeout**
counts alongside the solved count. A change that solves one more problem and introduces one wrong
answer is a regression. Compare against SymPy, Mathematica, or a textbook — being different from
SymPy is not automatically being wrong, but it is always worth explaining.

## Keep up with the mathematics

Algorithms here are decades of literature deep, and the good ones are written down. Before inventing
a procedure, find out whether it has a name:

- **SymPy** (`sympy/`) — readable reference implementations of Risch, Gruntz, Gröbner, and much else,
  with the papers cited in the docstrings. The single most useful cross-check.
- **Mathematica / Wolfram Alpha, Maxima, SageMath** — for deciding what the right answer *is*.
- **DLMF** (dlmf.nist.gov) — the authority on special-function identities and branch cuts.
- **OEIS** — for recognising an integer sequence.
- **arXiv math.AC / cs.SC**, the *Journal of Symbolic Computation*, and the ISSAC proceedings — where
  new symbolic-computation algorithms appear.
- **Gruntz's thesis** for limits, **Bronstein's _Symbolic Integration I_** for integration,
  **_Modern Computer Algebra_** (von zur Gathen & Gerhard) for the polynomial layer.

Branch cuts deserve a specific warning: `arcsin`, `log`, and fractional powers disagree between
conventions, and C99, .NET, Python and Mathematica do not all agree. Decide deliberately, cite the
convention, and test the disagreeing points.

## Working practice

**Branch first, always.** Never commit a feature or a fix straight to `master`, even with write
access. One branch per change, each independently mergeable, cut from `master`.

```
fix/<what-was-wrong>        fix/vanishing-denominator
feat/<what-is-new>          feat/gruntz
perf/<what-got-faster>      perf/limit-memoisation
docs/<what-is-documented>   docs/agents-md
chore/<housekeeping>        chore/drop-myget
```

Then:

1. Write the failing test first, and check it fails for the reason you think.
2. Fix it. Prefer the smaller change with the smaller blast radius.
3. Run everything: `dotnet test Sources/Tests/UnitTests`,
   `dotnet test Sources/Tests/FSharpWrapperUnitTests`.
4. If an existing test now fails, decide honestly which it is — the answer got better, the test was
   pinning a fudge, or you broke something — and say which in the commit message. Never loosen an
   assertion without writing down why.
5. Open a PR. State what was wrong, why the fix is right, and what you measured.

`TreatWarningsAsErrors` is on and there are custom analyzers; a static field needs
`[ConstantField]`, `[ThreadStatic]` or `[ConcurrentField]`.

## Write for the reader, briefly

Comments explain **why**, not what — the code says what. The reader is a mathematician six months
from now wondering whether a line can be deleted. Tell them what breaks if it is.

```csharp
// (x - a)^k is positive on the right whatever k is, and on the left takes the sign of
// (-1)^k, so approaching from the left at an odd order turns the sign around.
```

Be concise. Do not narrate your own process, and **do not record your mistakes, wrong turns or
retracted diagnoses in code, commit messages or documentation.** Those belong in the issue tracker,
where they are searchable and where someone hitting the same wall will find them. What belongs in
the code is the conclusion and the reason it holds. A measurement that justifies a constant is a
reason; a story about how you arrived at it is not.

Cite issues by full URL in code comments (`https://github.com/asc-community/AngouriMath/issues/557`),
since a bare `#557` means nothing outside GitHub. `#557` is fine in PR titles and bodies.

## Where the work is

Good entry points, roughly by depth:

- **Simplification consistency** — [#531](https://github.com/asc-community/AngouriMath/issues/531),
  [#557](https://github.com/asc-community/AngouriMath/issues/557),
  [#415](https://github.com/asc-community/AngouriMath/issues/415). Rules that fire in one arrangement
  and not another.
- **Limits** — [#596](https://github.com/asc-community/AngouriMath/issues/596),
  [#536](https://github.com/asc-community/AngouriMath/issues/536). `0^0` and `oo^0` have no rule
  where `1^oo` has one; piecewise has none at all.
- **Numerics** — [#602](https://github.com/asc-community/AngouriMath/issues/602). Precision that
  holds at one scale and not another.
- **Solving** — [#629](https://github.com/asc-community/AngouriMath/issues/629),
  [#475](https://github.com/asc-community/AngouriMath/issues/475),
  [#381](https://github.com/asc-community/AngouriMath/issues/381). Diophantine equations and
  characteristic polynomials both want the polynomial layer.
- **The polynomial layer itself** — multivariate GCD, resultants, factorisation. Large, and most of
  the above sits behind it.
- **[#497](https://github.com/asc-community/AngouriMath/issues/497), AngouriMath 2.0** — the design
  paper. If you are proposing something structural, propose it there.
