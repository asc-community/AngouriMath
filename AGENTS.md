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

### But "no answer" is the floor, not the target

Read that ordering forwards. *Right answer* comes first, and refusing is what you do when you have
established there is nothing better — not when the right answer looks like more work than you
wanted. Difficulty is not an argument, and neither is "I cannot promise this lands cleanly": the
way to find out whether a fix lands is to write it and measure it, and a failure you measured is a
finding worth having. These *are* reasons to prefer one fix over another — it degrades output
callers depend on, it rests on an assumption that is not true in general, it cannot be validated by
anything you can run. These are not: it touches more files, it might break tests you would then
have to understand, a smaller change exists that suppresses the wrong answer without producing the
right one.

The worked example is [#757](https://github.com/asc-community/AngouriMath/issues/757).
`(x - a)(x + a) <= 0` was answered with an interval whose endpoints are ordered for one sign of `a`
only, and the two candidate fixes were a case split on that sign, or declining to answer a symbolic
coefficient at all. Refusing is not a fix that works — it is a stopgap — and choosing it would have
been choosing the smaller diff over the answer.

**And before building a case analysis, look for the closed form.** That same issue looked like it
needed three branches on the sign of `a`; the answer is the single interval `[-|a|; |a|]`, because
`min(p, q)` is `(p + q - |p - q|)/2` and `max(p, q)` is `(p + q + |p - q|)/2`. One interval, right
for either sign and for `a = 0`, and it collapses to exactly the old output when the roots are
concrete. Enumerating cases is usually a sign that an identity has been missed.

## Output has a contract too

**Parsing what `Stringize` prints must give back the expression it printed.** Anything else makes
the printed form a lie, and a silent one, since a wrong reading is usually still a valid
expression: `(2^3)^2` printed as `2^3^2` is 512 where the expression is 64, and a piecewise
printed with `if` came back as a product with `if` read as an undeclared variable. If a node's
usual notation is not in the grammar, print the function call the parser does have.
`StringizeRoundTripTest` is where that is enforced; add to it when you add a node.

**`Latexize` has a round trip too, and it is enforced in someone else's repository.**
[CSharpMath.Evaluation](https://github.com/verybadcat/CSharpMath/blob/master/CSharpMath.Evaluation/Evaluation.cs)
reads LaTeX back into an `Entity`, and says so in its own source: *"CSharpMath must handle all LaTeX
coming from AngouriMath or a bug is present!"* So a change to what `Latexize` emits — a new node, a
different command, a changed bracketing — can break a downstream project, and no test here will say
so.

That is weaker than `Stringize`'s contract, not stronger. `StringizeRoundTripTest` fails in this
repository the moment the printed form stops parsing; the LaTeX contract fails as a bug report from
someone else, months later. When you change `Latexize` output, check it against CSharpMath and open
a PR there as well ([#822](https://github.com/asc-community/AngouriMath/issues/822)).

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

### Extend the harness rather than writing a throwaway

The measurement tools are infrastructure, not scaffolding. When the one you reach for cannot
express the measurement you need — no operation for the call you want to make, no coverage of the
shape you are chasing — **add the operation and keep it**. A scratch project answers the question
once and is deleted; the next person to ask it writes the same thing again.

Learned the wrong way round: closing
[#629](https://github.com/asc-community/AngouriMath/issues/629) needed a system solve, the probe had
no operation for one, and a throwaway project got written to answer it. The durable version was ten
lines in the probe. Same measurement, and only one of the two can be re-run.

Listed inputs and generated inputs fail differently, and both are worth having. A corpus of recorded
problems tells you about the corpus. A harness that builds its own inputs — sampling negative points,
checking a simplification against the expression it came from — finds what nobody thought to write
down: [#744](https://github.com/asc-community/AngouriMath/issues/744),
[#751](https://github.com/asc-community/AngouriMath/issues/751) and
[#752](https://github.com/asc-community/AngouriMath/issues/752) were all found that way while every
listed corpus stayed green. When you add one, sample negative points, make the tolerance relative,
and treat a `NaN` as a failure only where the original had a value somewhere.

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

### The corpus runs on every commit, and it records what it found

`Sources/Tests/UnitTests/Corpus` is that measurement, in the suite, so it runs on every commit. Each
problem carries the verdict it currently earns and the gate fails on two things: **any wrong answer**,
and **any case that stops matching its record** — including one that gets *better*, so the record
cannot quietly drift away from the library.

So when a change makes the corpus solve something it did not:

1. the gate fails and names the case;
2. **update its `Expect` in `Corpus.cs` in the same change**, which is how the improvement gets
   recorded rather than absorbed;
3. and if it is worth a line to a user, `BREAKING-CHANGES.md` too.

Answers are checked, not compared against stored text — a root is substituted back into its equation,
an antiderivative is differentiated back, a simplification is evaluated against the expression it came
from. A change of *form* is therefore not a failure and only a change of *value* is, which is what
lets the corpus stay useful while printed output moves.

**It is a gate and not a harness.** It is small and takes about a second, because everything in the
suite is paid for on every commit. The harnesses in `work/` are where a measurement generates its own
inputs, takes minutes, and gets read by a person; the two are not substitutes, and a finding from a
harness that is worth keeping belongs in the corpus as a new problem.

## One structure under several features

Four things in this library are the same shape, and three of them were written separately before
anyone noticed: a **finitely-supported map from a basis into a coefficient semiring**.

| feature | basis | its monoid operation | coefficients | where |
|---|---|---|---|---|
| polynomials | exponent vectors | addition of exponents | `Entity` | `PolynomialSolver.GatherMonomialInformation` |
| asymptotic series | rational exponents | addition | `Entity` | `AsymptoticSeries.Terms` |
| boolean minterms | assignments | concatenation | the boolean semiring | `Functions/Boolean/Minimiser` |
| quantum states | basis kets | tensor product | `Entity` | `Functions/Quantum` *(being added)* |

The basis is a **monoid**, so the whole is a monoid algebra `R[M]`. That is not decoration: it makes
the operation these features all want into a single one. Factoring is *dividing out the monoid GCD
of the support*, and these two lines are the same computation —

```
x^2*y + x^2      =  x^2 * (y + 1)                  gcd of {(2,1),(2,0)} is (2,0)
|001> + |011>    =  |0> (x) (|0> + |1>) (x) |1>    common prefix |0>, common suffix |1>
```

— which is why "factor out a common monomial" and "detect tensor separability" do not need two
implementations.

**Idempotence is a property of the semiring, not of the algorithm.** `a or a = a` holds in the
boolean semiring and `|x> + |x> = 2|x>` says it fails in the complex one, and that single difference
is what separates *covering* from *superposition*. Quine-McCluskey's merge step is absorption; it is
correct only because a minterm may be covered twice for free. Write that as data on the semiring and
the classical minimiser becomes the idempotent special case of the general thing, rather than a
separate subsystem.

**What does not share, and must not be made to.** Cover selection is boolean only, for exactly the
reason above. Factorisation into irreducibles is polynomials only — only the monomial content is
common. Truncation and order tracking are series only. A shared engine that swallowed these would be
wrong in a way that type-checks.

**Two representations, deliberately.** The coefficients are `Entity` throughout; the choice is what
the *basis* is, and it is made differently on purpose:

| | basis | why |
|---|---|---|
| quantum | `Entity`-backed | the state is then an ordinary expression, so `Simplify`, `Substitute`, `Latexize` and the rest work on it without anything being taught about quantum |
| polynomials, series | a generic `TBasis` struct | exponent vectors never enter the expression tree, and the internal layers stay allocation-free and type-safe |

One generic spine, instantiated twice. That is less tidy than picking a single answer, and the
untidiness is the point: making quantum states first-class expressions is what puts the
classical/quantum boundary inside one algebra instead of between two subsystems, while an exponent
vector has no business being an `Entity` and would cost allocation on the hottest path in the
library. If you are tempted to unify these later, price the polynomial side first — that is the one
with the measurements against it.

**What is not this shape at all**, and is worth saying so that nobody tries: sets and intervals
(union is not coefficient addition), piecewise (a condition is not a basis element), matrices (the
product is contraction, not convolution), and operators or gates — those act *on* states rather than
being states, and they belong to a quantum computing library rather than to a CAS.

## An operation is a value, not only a method

`Simplify`, `Expand`, `Factorize`, `Differentiate`, `Integrate` and `Limit` are adapters over
`AngouriMath.Core.Transformations` — a `Transformation` is the operation itself, carrying a name,
what it claims about its output, and how well justified the claim is. The algorithms underneath are
untouched; what changed is that a step can be named, composed and enumerated rather than only
called. See [`Contributing/Transformations.md`](Sources/AngouriMath/Docs/Contributing/Transformations.md),
and [#746](https://github.com/asc-community/AngouriMath/issues/746) for where it is going.

Three habits it asks for, and each is the honesty rule above in a different place:

**Say which relation you are claiming.** `Equivalence` means the output is another way of writing the
input; `Derivation` means it is a different object. A derivative and an antiderivative are
`Derivation`, and a test that subtracts one from its input and asserts zero is testing nothing.

**Do not label a rewrite `Sound` without an argument.** Every rule set shipped today is
`SoundUnderAssumptions`, and a test over `RewriteRules.All` holds it there. The tier is declared, not
verified — so promoting one means changing that test, and saying in the same change why the rewrite
needs no assumptions. Loosening a tier needs nothing.

**A recording is a scope, not a setting.** `RewriteRecording.Start()` collects the rewrites that
fire while it is open, and costs one thread-static read per rule set — not per node — when nobody
opened one. Anything else added to this layer has to keep that shape: the common path may not pay
for machinery it is not using, and a switch a caller can leave on is a way of making it pay.

**Say "no answer" with `null`, here too.** `ApplyCore` returning `null` is the layer's way of saying
"I could not settle this", and it is the one place where the new layer is *more* honest than the 1.x
method it backs: `Transformation.Integration` has no answer where `Entity.Integrate` returns an
unevaluated `Integralf`. Both are the same claim; neither is `NaN`.

And what not to do with it. `Solve` is not a transformation — it consumes a goal and produces a
solution set, and it belongs in a tactic layer that does not exist yet. `Entity.Set` being an
`Entity` means it would type-check as `Entity -> Entity`, which is the reason to keep it out rather
than a reason to put it in. Nor is there any inverse machinery: `Expand` and `Factor` are not
inverses and `Unsolve` is not well defined, so the API does not invent a symmetry the mathematics
does not have.

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
- **[mathlib4](https://leanprover-community.github.io/mathlib4_docs/)** — for the *hypotheses* of an
  identity. Every lemma there carries its side conditions explicitly and machine-checked, which is
  exactly what a rewrite rule needs and exactly what our rules have repeatedly been missing. See
  [`Contributing/SimplificationContract.md`](Sources/AngouriMath/Docs/Contributing/SimplificationContract.md)
  for how to use it and where it does not help.

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
7. **Read the PR's own thread before it merges**, and answer what is on it. Comments arrive after
   the checks go green, so a PR that was clear when it was opened need not still be; and a review
   merged over does not go away — it comes back as an issue somebody else had to file. Both places
   count, and the API shows them separately: `gh pr view <n> --comments` for the thread, and
   `gh api repos/{owner}/{repo}/pulls/<n>/comments` for comments left on the diff.

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
| [`Contributing/Transformations.md`](Sources/AngouriMath/Docs/Contributing/Transformations.md) | the transformation layer the 1.x entry points sit on, and how to add the next rule set |
| [`Contributing/SimplificationContract.md`](Sources/AngouriMath/Docs/Contributing/SimplificationContract.md) | what a rewrite may assume, and the ten obligations one has to meet. Read it *before* adding or changing a rule |
| [`Contributing/CanonicalForm.md`](Sources/AngouriMath/Docs/Contributing/CanonicalForm.md) | canonical versus simplest, why no canonical form exists for the whole language, and what one means per node class. Read it before comparing two expressions for equality |
| [`Contributing/coding_rules.md`](Sources/AngouriMath/Docs/Contributing/coding_rules.md) | sealed-or-abstract, and immutability of `Entity` |
| [`WhatsNew/version_performance_control.md`](Sources/AngouriMath/Docs/WhatsNew/version_performance_control.md) | the inter-version performance table, and how to add a column |
| `Sources/Analyzers/` | the custom analyzers, including the static-field one behind `[ConstantField]` |

Anything added for the library's own purposes is not `public` — see
[`Contributing/coding_rules.md`](Sources/AngouriMath/Docs/Contributing/coding_rules.md). Nothing
checks that any more: the `PublicApiAnalyzers` package that required every public member to be
listed in a `PublicApi.*.txt` is gone from the tree, so it is a rule to follow rather than one to
be caught by.

## Read the roadmap before you release anything

[#746](https://github.com/asc-community/AngouriMath/issues/746) is the ten-year technical vision, and
it is **not optional reading before a release, a version number, or anything that lands in the kernel
package**. It was written to be argued with, not obeyed — but it has to be read first, because two
things in it are easy to break by accident and impossible to undo afterwards.

**Its `v1.0`–`v9.0` are capability tiers, not versions.** `v1.0` is "a symbolic engine worth building
on" — a real polynomial layer, a *written* canonical-form specification, pattern matching as data
rather than a `switch`, and assumptions that travel with a node. `v2.0` is "the rewrite graph". A
published package version does **not** mean the tier of the same name has been reached, and choosing
one spends a label the roadmap is using: check #746 before picking a number, and say on the issue
which tier the release does and does not advance.

**Three conditions cut across every tier**, and #746 says a tier that violates one has failed
whatever else it delivered:

1. **The common case pays for nothing it does not use.** Package boundaries are decided deliberately
   and early, because a published one cannot be moved. Anything large landing in the kernel wants that
   decision first — #746's item 78.
2. **Speed and memory on popular use cases are measured, not hoped for.** Parse, `Simplify`, `Solve`
   and `Differentiate` on textbook-sized input, recorded in
   [`WhatsNew/version_performance_control.md`](Sources/AngouriMath/Docs/WhatsNew/version_performance_control.md).
   Measure the previous column again on the same machine and publish the pair: columns taken on
   different hardware cannot be read as a ratio, and a uniform factor across every row is the machine
   rather than the code.
3. **Correctness coverage grows with the surface.** Each new layer adds ways to be wrong that the one
   below could not express.

So the release checklist is: the suite and the harnesses in `work/` green, a `BREAKING-CHANGES.md`
entry for every changed answer measured on real builds, **a performance column measured against the
previous one on the same machine**, and a version number that does not contradict #746.

## Where the work is

Good entry points, roughly by depth. **Checked against the tracker on 2026-08-08** — the list this
replaces had gone stale, with eight of its ten issues closed, so it was pointing at finished work.

- **Missing functions** — [#809](https://github.com/asc-community/AngouriMath/issues/809). `floor`, `ceil`, `round`, `min`, `max`, `gcd`. Each wants
  a design decision rather than a grammar line, and `min`/`max` are the cheapest thing here.
- **More solvers** — [#231](https://github.com/asc-community/AngouriMath/issues/231) for limits, [#233](https://github.com/asc-community/AngouriMath/issues/233) for integrals. Both accepted, both
  open-ended, and both measurable one problem at a time.
- **Solving** — [#475](https://github.com/asc-community/AngouriMath/issues/475), [#381](https://github.com/asc-community/AngouriMath/issues/381). Diophantine equations and characteristic
  polynomials both want the polynomial layer.
- **A wrong answer** — [#812](https://github.com/asc-community/AngouriMath/issues/812). `abs(x) = -1` returns a non-empty set whose members do not
  satisfy it. Highest priority here by the rule at the top of this file: not answering is
  legitimate, answering wrongly is not.
- **Decisions only a major version may take** — [#204](https://github.com/asc-community/AngouriMath/issues/204) roots versus fractional powers,
  [#326](https://github.com/asc-community/AngouriMath/issues/326) the syntax for piecewise, [#721](https://github.com/asc-community/AngouriMath/issues/721) unifying `Codomain` with
  `provided ... in RR`. Cheap now; after 2.0 ships each waits for 3.0. Note that
  [#318](https://github.com/asc-community/AngouriMath/issues/318) is *not* one of these despite looking like it: `Invert` and `InvertNode` are
  `internal` and `private protected`, so changing what they return breaks nobody, and the
  parametric sets it asks for already work — what is left of it is the guard in #812.
- **Structural** — [#286](https://github.com/asc-community/AngouriMath/issues/286) and [#495](https://github.com/asc-community/AngouriMath/issues/495), functions and lambdas as entities;
  [#248](https://github.com/asc-community/AngouriMath/issues/248), n-ary operators. All three add node types, and whether *that* is a breaking
  change is itself undecided — see #248.
- **The polynomial layer itself** — multivariate GCD, resultants, factorisation. Large, and most of
  the above sits behind it. Its representation is already a monoid algebra in all but name; see
  *One structure under several features* for the shape it wants, and note that
  `GatherMonomialInformation` is on the hot path for solving, long division *and* simplification, so
  nothing there moves without a measured proof it did not regress.
- **The goals** — [#717](https://github.com/asc-community/AngouriMath/issues/717) parity with sympy, [#718](https://github.com/asc-community/AngouriMath/issues/718) competition and textbook
  problems, [#746](https://github.com/asc-community/AngouriMath/issues/746) the ten-year one. Long-horizon, and each names its own measurement.

[#497](https://github.com/asc-community/AngouriMath/issues/497), the AngouriMath 2.0 design paper, is **closed**: the decision recorded there on
2026-08-04 was to evolve the existing design rather than rewrite it in F#. So 2.0 is now a version
of this codebase, not a successor to it, and structural proposals belong in their own issue.
