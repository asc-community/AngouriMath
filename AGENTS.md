# AGENTS.md

For AI agents working on AngouriMath. Humans: [CONTRIBUTING.md](CONTRIBUTING.md) is yours, and
everything below applies to you too.

AngouriMath is a computer algebra system. The thing being built is *mathematics*, and the code is
how it is expressed. Read this as instructions for doing mathematics well, using C# and F#.

## The one rule everything else follows from

**Be a mathematician first.** When mathematical correctness and backward compatibility disagree,
correctness wins. A published API that returns the wrong answer is not an asset to preserve; it is
a bug with users. Say so in the changelog, and change it.

Saying so is not optional, and it has a place: [BREAKING-CHANGES.md](BREAKING-CHANGES.md). Anything
that makes the same input give a different answer goes there before the branch merges, with the old
value, the new one, and why — measured on a build of each, not read off the diff. A user whose code
depended on the wrong answer deserves to find out from us why it moved, rather than from their own
test suite.

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

The syntax the parser accepts is written down in
[`Docs/Usage/Syntax.md`](Sources/AngouriMath/Docs/Usage/Syntax.md). Keep it true when you change the
grammar — and change the grammar the way
[`Docs/Contributing/ImproveParser.md`](Sources/AngouriMath/Docs/Contributing/ImproveParser.md) sets
out, by editing `AngouriMath.g` and regenerating. Never hand-edit the generated files. Regenerate the
*unmodified* grammar first and check the diff is empty, so that what you then commit is your rule and
not a toolchain version.

## Verify the mathematics, not the string

Never assert on printed form unless the printed form *is* the bug. Assert the property:

- an integral: differentiate it back and compare to the integrand
- a root: substitute it into the equation
- an identity: subtract the two sides and simplify to zero
- a limit: check it against the value at nearby points, or a series

A string comparison passes for `2*x` and fails for `x*2`, which tells you nothing about whether the
answer is right.

Issue regressions go in `Sources/Tests/UnitTests/Common/`, in the file for the area and named
`IssueNNN_WhatItAsserts` after the issue —
[`SimplificationRegressionTest.cs`](Sources/Tests/UnitTests/Common/SimplificationRegressionTest.cs),
[`SolverRegressionTest.cs`](Sources/Tests/UnitTests/Common/SolverRegressionTest.cs) and
[`NumericsRegressionTest.cs`](Sources/Tests/UnitTests/Common/NumericsRegressionTest.cs).
[`AlreadyFixedIssuesTest.cs`](Sources/Tests/UnitTests/Common/AlreadyFixedIssuesTest.cs) is the
separate case: an open issue that turns out to work today, pinned so it can be closed without
leaving whatever closed it unprotected.

**Reproduce on a stock `master` build before claiming anything is broken.** More than one confident
report here has turned out to describe behaviour that was never broken.

The same measurement is worth taking before you plan a fix at all. An open issue names the version
its reporter was on, and the tracker is older than the code: on 2026-08-05, eleven of the open
issues turned out to be already fixed, and re-measuring them cost minutes where planning fixes for
them would have cost days. Run the reporter's expression first. Close it with the measurement if it
answers, and say which build you measured.

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

**A plan belongs in an issue, not in a comment.** When work splits into separable pieces — or when
a discussion settles on a design bigger than the thread it is in — open a new issue per piece and
then execute on them. A plan left in a comment is not work anybody can pick up, schedule or argue
with separately, and the thread it sits in closes over it.

Split by what can be landed and measured on its own, not by topic. Each new issue says what the
problem is, what the plan is, and what has to be re-measured rather than assumed; then link them
from the issue they came out of, and say which one closes it.

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
5. If the same input now gives a different answer, add it to
   [BREAKING-CHANGES.md](BREAKING-CHANGES.md) — including when the old answer was wrong. A test you
   had to change is the usual sign that you owe an entry.
6. Open a PR. State what was wrong, why the fix is right, and what you measured.

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

## Where things are written down

Before writing a paragraph explaining something, check whether it already has a home. Most of these
are short, and a stale one is worse than none — if you change what a file describes, change the file.

| | |
|---|---|
| [BREAKING-CHANGES.md](BREAKING-CHANGES.md) | every input whose answer has changed, with both values and why |
| [CHANGELOG.md](CHANGELOG.md) | points at the published release notes on the site |
| [CONTRIBUTING.md](CONTRIBUTING.md) | for humans; how to get set up and what a PR should look like |
| [`Docs/Usage/Syntax.md`](Sources/AngouriMath/Docs/Usage/Syntax.md) | what the parser accepts. The grammar was the only statement of it until [#706](https://github.com/asc-community/AngouriMath/pull/706) |
| [`Docs/Usage/Exceptions.md`](Sources/AngouriMath/Docs/Usage/Exceptions.md) | the exception hierarchy under `AngouriMathBaseException` |
| [`Docs/Contributing/`](Sources/AngouriMath/Docs/Contributing/README.md) | the index of the contributor docs |
| [`Contributing/General.md`](Sources/AngouriMath/Docs/Contributing/General.md) | the `Entity` hierarchy, in a paragraph |
| [`Contributing/AddingNode.cs`](Sources/AngouriMath/Docs/Contributing/AddingNode.cs) | every place a new node has to be taught about. Read it *before* adding one |
| [`Contributing/ImproveParser.md`](Sources/AngouriMath/Docs/Contributing/ImproveParser.md) | how to change the grammar and regenerate |
| [`Contributing/coding_rules.md`](Sources/AngouriMath/Docs/Contributing/coding_rules.md) | sealed-or-abstract, and immutability of `Entity` |
| [`WhatsNew/version_performance_control.md`](Sources/AngouriMath/Docs/WhatsNew/version_performance_control.md) | the inter-version performance table, and how to add a column |
| `Sources/Analyzers/` | the custom analyzers, including the static-field one behind `[ConstantField]` |

`Docs/Contributing/RS1617Errors.md` is the one exception: it describes adding public members to a
`PublicApi.*.txt`, and neither those files nor the analyzer that wanted them are in the tree any
more. Do not follow it; delete or rewrite it if you are in there anyway.

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
