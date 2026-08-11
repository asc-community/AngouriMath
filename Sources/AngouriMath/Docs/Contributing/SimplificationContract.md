# What a simplification rule may assume

A rewrite rule in this library is a claim: *this expression and that one are two ways of writing the
same thing.* This file says what has to be true for that claim to hold, so that a new rule can be
judged sound or unsound by reading it rather than by waiting for a bug report.

It exists because the harnesses cannot do this job. On 2026-08-11, `casbench` was at 117/119 with no
wrong answers, `propcheck` ran 1340 checks with no failures, `rootcheck` was 596/596 and `simpsweep`
agreed on 10463 comparisons — and `Simplify` was returning `3` for `arcsin(sin(3))`, whose value is
`pi - 3`. Four rules had been wrong since 2020 and shipped in every release
([#884](https://github.com/asc-community/AngouriMath/issues/884)). Sampling could not find them, for
a reason worth stating plainly: **a rule that is wrong only at a branch cut, or only off the real
line, is wrong exactly where a sweep does not look.**

Read [Transformations.md](Transformations.md) first for `TransformationRelation` and `Soundness`; this
file is the missing third of that vocabulary.

## 1. Three claims, and the one that had no home

`Transformations.md` asks a rewrite to say **what relation** it claims (`Equivalence` or `Derivation`)
and **how well justified** the claim is (`Sound`, `SoundUnderAssumptions`, `Heuristic`). Every rule set
shipped today declares `SoundUnderAssumptions`, and a test holds them there.

That declaration names no assumptions. It is the honest label — nobody has argued that any of these
rule sets is unconditional — but as a design it says only "something is assumed here, we are not
saying what". The missing piece is the **assumption set**: a predicate on the free variables and on
how the expression is being read, under which the two sides genuinely coincide.

The rest of this file is what that predicate has to account for.

## 2. What the library already knows

Four mechanisms exist. Most soundness questions can be answered with them, and a rule that ignores
them is usually reinventing one badly.

### `DomainCondition` — where an expression is defined

Every node has an `IntrinsicCondition`, the condition under which *that operation* is defined; the
public `Entity.DomainCondition` conjoins it with every child's, caches the result and inner-simplifies
it. So the whole "this is undefined here" story is one property away:

| expression | `DomainCondition` |
|---|---|
| `1/x` | `not x = 0` |
| `(x-1)/(x-1)` | `not x - 1 = 0` |
| `tan(x)` | `not cos(x) = 0` |
| `ln(x)` | `x > 0` |
| `log(b, x)` | `b > 0 and not b = 1 and x > 0` |
| `x^y` | `not x = 0 or y > 0` |
| `sqrt(x)`, `arcsin(x)`, `sgn(x)`, `arcsin(sin(x))` | `True` |

**This is the property to compare across a rewrite.** If the two sides do not have the same domain,
the rule either attaches the difference or does not fire.

### `Codomain` — what kind of value a node takes

A declared property per node: `Absf` is `Domain.Real`, `Divf` and `Logf` are `Domain.Complex`, and a
bare `Variable` is **`Domain.Any`** — a symbol is unconstrained until something says otherwise.
`Patterns.EqualityInequality.cs` reads it through `IsKnownReal`, which is the existing idiom for "may
I assume this operand is real".

### `MathS.Settings.Codomain` — how the expression is being read

A thread-static scope, **defaulting to `Domain.Complex`**. It says whether the caller is doing real
analysis or complex analysis. `Patterns.EqualityInequality.cs` pairs it with `IsKnownReal`:

```csharp
MathS.Settings.Codomain.Value is Domain.Real || IsKnownReal(entity)
```

That disjunction is the standard way to ask "is this real here", and a rule needing realness should
use it rather than invent a test.

### `Providedf` — a condition carried on the value

`expr provided condition` says: this equals `expr` where `condition` holds, and is undefined
otherwise. `Simplify` already produces it — `x/x` gives `1 provided not x = 0` — and `Entity.Provided`
drops the condition when it is `True`, so nothing is attached where nothing is needed.

Two costs to know before reaching for it. A condition **competes on complexity**, because `Simplify`
ranks candidates by node count, so attaching one can make a better form lose. And a condition
**travels**: it propagates outward and can escape a binder
([#878](https://github.com/asc-community/AngouriMath/issues/878)), and a single `NaN` condition inside
a `Piecewise` collapses the whole node.

## 3. Four things that are not the same

Conflating any two of these is the most common way a rule goes wrong. The distinction between the
first two is [#721](https://github.com/asc-community/AngouriMath/issues/721), still open.

| | says | expressed by |
|---|---|---|
| **the reading** | this expression is being read as a real-valued function, or a complex-valued one | `MathS.Settings.Codomain` |
| **a fact about a point** | *this value* is real, positive, non-zero | `Codomain`, `provided x in RR`, `IsRealPositive` |
| **a fact about a neighbourhood** | this holds *throughout the approach* to a destination | `IsEventuallyPositive`, in the limit reader only |
| **the domain of definition** | the expression has a value here at all | `DomainCondition` |

`lim x->0- (x^x)` is the case that separates the first two: `x in RR` holds at the point and tells you
nothing, while the reading is what rules the limit out. And the third is genuinely not the second —
a limit needs an identity to hold on the way to the destination, not at it, which is why
`a(x)^n / b(x)^n -> (a/b)^n` is checkable inside the limit machinery and not in the simplifier
([#802](https://github.com/asc-community/AngouriMath/issues/802)).

### A condition is not an assumption

This is the trap that looks most like a fix.

`arcsin(sin(x))` is defined for **every** real `x`. The identity `arcsin(sin(x)) = x` holds only on
`[-pi/2, pi/2]`. It is therefore tempting to write:

```
arcsin(sin(x))  ->  x provided x >= -pi/2 and x <= pi/2      // WRONG
```

That is a second wrong answer, not a fix. The rewritten form claims the expression is **undefined**
outside the interval, and it is not — it has a different value there, `pi - x`. A `Providedf` narrows
the domain; an assumption narrows the *cases in which the rule may fire*. They are opposite moves.

Use `Providedf` when the rewrite genuinely removes a singularity that the original had — `x/x -> 1`
loses the pole at zero, and the condition restores it. Use an assumption when the two sides are both
defined and merely unequal.

## 4. The obligations

A rewrite `L -> R` under assumption set `A`, claiming `Equivalence`. Numbered so a review can cite
them.

**O1 — Say which relation you claim.** `Equivalence` means `R` is another way of writing `L`;
`Derivation` means it is a different object. A test that subtracts a derivative from its integrand and
asserts zero is testing nothing.

**O2 — State `A`.** In the rule's comment if nowhere else. "None" is a strong and welcome claim;
`|-x| = |x|` needs nothing, because negation is an isometry. A rule whose comment states an identity
without qualification is asserting `A` is empty, and that assertion is checkable — the four rules of
#884 carried the comment `// arcfunc(func(x)) = x`, which was simply false.

**O3 — Under `A`, the values agree.** For every assignment satisfying `A`, `L` and `R` denote the same
value. Not "the same up to a branch", not "the same on the reals if the reading is complex".

**O4 — Under `A`, undefinedness is preserved.** `L` and `R` must be defined at the same points, so
compare `DomainCondition`. Two expressions that are both undefined at a point *agree* there — `1/(x-x)`
is undefined everywhere and `NaN` is the honest answer, not a failure. What is forbidden is turning
undefined into a value, or a value into undefined.

**O5 — Discharge `A` one of exactly three ways.**

1. **Decide it now.** The argument is a number and the predicate is computable: `WithinHalfPi` reads a
   real argument and compares it against `pi`. This keeps the exact answer where it is available —
   `arcsin(sin(1/2))` is `1/2`, not a decimal.
2. **Attach it**, if and only if `A` is a statement about the *domain* (see §3).
3. **Refuse**, leaving the node as written.

**O6 — Never fire silently under an undischarged `A`.** Refusing is a legitimate answer; a wrong value
is not. This is the whole of [AGENTS.md](../../../../AGENTS.md)'s ordering — right answer > no answer >
slow answer > wrong answer — applied to one rule.

**O7 — Refusing is the floor, not the target.** O6 is not licence to guard everything into silence. If
`A` is decidable in the common case, decide it. If `A` is checkable *somewhere else* — where there is
a destination, a neighbourhood, a declared codomain — then the rule is in the wrong place, and moving
it is the fix rather than sacrificing either correctness or coverage. #802 is the worked example: the
gathering was not deleted, it was moved into the limit reader, every limit survived and the wrong
answer went.

**O8 — A decidable specialisation proves nothing about the general rule.** `log(2, (-8)^2)` answers `6`
correctly because the base folded to a number before the rule could fire, while
`log_b(a^c) -> c*log_b(a)` is unsound for the same shape written symbolically. Test the symbolic form.

**O9 — Remember that `Simplify` *selects*.** Candidates are ranked by node count, so an unsound rewrite
only has to be **short** to win, and the same unsound rewrite exists for neighbouring expressions where
it loses invisibly. `ln(e^y)` becomes `y` because `y` is shorter; `log(2, x^2)` keeps its form because
the rewrite is longer there. **A rule that appears not to fire has not been shown to be safe.** Use
`Simplify`'s candidate list, not its answer, to find out what a rule does.

**O10 — Assumptions conjoin; soundness does not improve.** A chain of rewrites assumes the conjunction
of their assumptions and is `Equivalence` only if every link is. Nothing composes upwards.

## 5. Judged rules

Worked examples, each measured.

### Sound with no assumption

- `sin(arcsin(z)) -> z`, and the three siblings. These compose the **right** inverse, so they hold
  wherever the inner function is defined at all. `A` is empty.
- `|-x| -> |x|`. Negation is an isometry of the plane.
  ([#881](https://github.com/asc-community/AngouriMath/issues/881) — the library does not do this yet,
  which is a coverage gap and not a soundness one.)

### Unsound as written, fixed by deciding `A`

- `arcsin(sin(x)) -> x` and its three siblings. `A` is "`x` is real and in the principal interval",
  decidable for a numeric argument. Fixed in #884 by firing only there.
  **The block these lived in had a sound half and an unsound half written as though they were
  symmetric**, four lines apart, which is how it survived five years of reading. That asymmetry —
  left inverse versus right inverse — is worth looking for elsewhere.

### Unsound as written, and the assumption belongs elsewhere

- `log_b(a^c) -> c*log_b(a)`. `A` is "`a` positive real and `c` real". `ln(e^y)` simplifies to `y`,
  and at `y = 3*pi*i` the left side is `pi*i`. Guarding it in the simplifier costs two Gruntz limits
  their answers, which is O7's signal: the limit reader can check the assumption in a neighbourhood
  and the simplifier cannot. Still open on #884.
- `a(x)^n / b(x)^n -> (a/b)^n`. The same shape, already resolved this way in #802.

### Sound with a condition, because the domain really does change

- `x/x -> 1 provided not x = 0`. The rewrite removes a pole; the condition restores it.
  `DomainCondition` of both sides then agree.

### Declines, correctly

- `sqrt(x^2) -> x` does not fire, and must not: it is false for negative `x`, and `sqrt(x^2)` is
  `|x|` over the reals.

## 6. The conventions this library commits to

Branch cuts disagree between systems, so these are measured on a 2.0.0 build rather than assumed. A
rewrite that changes any of them changes answers.

| | value | convention |
|---|---|---|
| `sqrt(-4)` | `2i` | principal square root |
| `(-1)^(1/2)` | `i` | same, via the power |
| `ln(-1)` | `pi*i` | principal logarithm, `arg` in `(-pi, pi]` |
| `i^i` | `0.2078...` | principal |
| `(-8)^(1/3)` | `-2` | **the real root** |
| `(-8)^(2/3)` | `4` | the real root |
| `(-8)^(1/5)` | `-1.5157...` | the real root |
| `(-8)^(1/4)` | `1.1892 + 1.1892i` | principal — there is no real fourth root |
| `(-8)^0.3333333333` | `1.0 + 1.7320i` | **principal**, not the real root |

Read the last three rows together, because they are a trap. For a negative base and an exact
`Rational` exponent with an **odd denominator**, this library takes the real root. Write the same
exponent as a decimal and it takes the principal value instead, and the two differ by more than
rounding: `-2` against `1 + 1.732i`.

So **a rewrite that moves between an exact rational and a decimal exponent can change the value on a
negative base**, and one that reduces `2/6` to `1/3`, or fails to, changes which convention applies.
Whether this convention is the right one is
[#204](https://github.com/asc-community/AngouriMath/issues/204), open and deliberately a major-version
question; what is not in question is that a rule may not silently move an expression from one side of
it to the other.

## 7. Two inconsistencies this exposes

Recorded here because the contract makes them visible, not because they are settled.

**`ln` and `sqrt` disagree about which reading their domain describes.** With the default codomain
`Domain.Complex`, `DomainCondition` of `ln(x)` is `x > 0` — the real reading, since a complex logarithm
is defined for every `x != 0` — while `DomainCondition` of `sqrt(x)` is `True`, the complex reading.
The two cannot both be right under one setting. Likewise `arcsin(x)` reports `True`, which is the
complex reading, though over the reals it needs `|x| <= 1`.

**`DomainCondition` does not read `MathS.Settings.Codomain` at all.** It is a property of the node,
fixed at construction, so it cannot say "defined here, given that we are doing real analysis". That is
the same gap as #721, in a second place.

## 8. Checking this, and why the existing harnesses cannot

`casbench` and `propcheck` are lists of inputs somebody wrote down. `rootcheck` and `simpsweep`
generate theirs, which is stronger — and still could not reach #884, for two independent reasons:
`simpsweep` samples **real** points, so a rule wrong only off the real line never disagrees; and it
builds expressions from a grammar that never nests an inverse function around its own forward
function, so the shape was outside its space entirely.

A harness that reaches this class has to be built the other way round: **from the rules, not from a
grammar.** For each rewrite, construct the set where its assumption fails and test *there* —

- the zeros of every denominator, and the poles named by `DomainCondition`;
- branch points and points either side of a cut: negative reals for `ln` and `sqrt`, `±i` for `arctan`;
- arguments outside a principal interval, one period out in each direction;
- points off the real line, including a purely imaginary argument;
- a negative base with a rational exponent, at both odd and even denominators;

and compare `L` against `R` numerically at each, counting **both undefined** as agreement, per O4.
Such a harness would have found all four rules of #884 immediately. It does not exist yet.

## 9. Where to look an assumption set up

Obligation O2 says to state the assumption set. For a large part of classical mathematics somebody has
already stated it, machine-checked it, and published it:
**[mathlib4](https://leanprover-community.github.io/mathlib4_docs/)**. Every lemma carries its
hypotheses explicitly, because Lean will not accept it otherwise — which is the discipline a rewrite
rule needs, already applied to most of classical analysis.

The intervals this library guards the inverse-trigonometric cancellations with are the ones mathlib
states:

| mathlib4 | our guard |
|---|---|
| `Real.arcsin_sin {x : ℝ} (hx₁ : -(π / 2) ≤ x) (hx₂ : x ≤ π / 2) : arcsin (sin x) = x` | `WithinHalfPi(closed: true)` |
| `Real.arccos_cos {x : ℝ} (hx₁ : 0 ≤ x) (hx₂ : x ≤ π) : arccos (cos x) = x` | `WithinZeroAndPi(closed: true)` |
| `Real.arctan_tan {x : ℝ} (hx₁ : -(π / 2) < x) (hx₂ : x < π / 2) : arctan (tan x) = x` | `WithinHalfPi(closed: false)` |
| `@[simp] Real.tan_arctan (x : ℝ) : tan (arctan x) = x` — no hypothesis | the right-inverse direction, unguarded |

Note the open-versus-closed distinction that separates `arctan` from `arcsin`, and that
`Real.tan_arctan` is a `@[simp]` lemma with no hypothesis at all — the right-inverse direction needs
none, which is §5's point in someone else's notation. This is the first place to look when writing a
guard.

**Two warnings.**

**A hypothesis can differ because the convention differs, not because the mathematics does.**
`Real.sin_arcsin` requires `-1 ≤ x ≤ 1`, and this library needs no such condition — because mathlib's
`Real.arcsin` clamps outside `[-1, 1]` to stay a total real function, while ours goes into the complex
plane. Both are right about their own `arcsin`. Copying a hypothesis without checking which convention
it belongs to is the same error as reading a branch cut off memory, so §6 applies: measure what *this*
library does.

**And it does not cover everything.** mathlib has no `arccot`, and `arccot` is the case where this
library's convention departs furthest from the textbooks: its range is `(-pi/2, pi/2]` rather than
`(0, pi)`, so `arccotan(-1)` is `-pi/4`
([#887](https://github.com/asc-community/AngouriMath/issues/887)). Where the lookup is empty there is
no substitute for measuring the function at a positive argument, a negative one, and zero, and writing
the three values into the comment.

## 10. What this does not settle

- **Per-symbol assumptions.** SymPy carries them on the symbol (`Symbol('x', positive=True)`) with
  `refine`/`ask` to query, and an explicit `force=True` escape where the user accepts the risk. This
  library has a global reading and a per-node `Codomain`, and no way to say "this `x` is positive".
  Most of the assumption sets in §5 would be dischargeable if there were one. That is a design
  question, not an omission from this file.
- **Whether `Providedf` should be able to express a neighbourhood**, which is #721.
- **The fractional-power convention**, which is #204.
- **The tiers.** `Soundness` remains declared, not checked. Nothing here promotes a rule set to
  `Sound`; doing so still needs the argument that its assumption set is empty, written down.
