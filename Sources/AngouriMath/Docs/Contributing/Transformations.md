# Transformations

`AngouriMath.Core.Transformations` is the layer the 1.x mathematical entry points sit on. It exists
so that an operation is a **value** — something with a name, a stated claim, and a way to be
composed — instead of only a method you call.

It is the first step of the rewrite-engine layer in
[#746](https://github.com/asc-community/AngouriMath/issues/746). It is deliberately small, and it is
**experimental**: the three concepts are meant to last, the catalogue will grow, and the signatures
may still move. The stable surface is `MathS` and the methods on `Entity`.

## Why it is not just an interface with `Apply`

Three things have to be said about a mathematical operation before it can be composed with another
one, and none of them fit in `Entity -> Entity`:

**What it claims.** `Simplify` returns another way of writing the same value. `Differentiate` returns
a different object. Both are `Entity -> Entity` and confusing them is how you get a test that
subtracts a derivative from its integrand and asserts zero. `TransformationRelation` is `Equivalence`
or `Derivation`, and a chain is an equivalence only if every link is.

**How well justified the claim is.** `Soundness` is `Sound`, `SoundUnderAssumptions` or `Heuristic`,
and composing takes the weaker of two — nothing composes upwards. The tier is **declared, not
checked**: it is a claim to argue with, not a guarantee, which is why the registry starts
conservative and why tightening a label needs an argument while loosening one does not.

**Whether it answered at all.** `ApplyCore` returns `null` for "I could not settle this", which is
the same distinction [AGENTS.md](../../../../AGENTS.md) draws between an unevaluated node and `NaN`.
`Transformation.Integration("x")` applied to `e ^ (x ^ 2)` has no answer; `Entity.Integrate` makes the
same claim in the shape its callers expect, by handing back an unevaluated `Integralf`.

## The pieces

| | |
|---|---|
| `Transformation` | the operation: `Name`, `Relation`, `Soundness`, `Apply`, and the static catalogue |
| `TransformationResult` | input, output-or-nothing, and which transformation ran. A struct, so routing an ordinary call through this layer allocates nothing |
| `RewriteRuleSet` | a named, attributed group of rewrites — `Name`, `Description`, `Relation`, `Soundness`, `ApplyOnce` |
| `RewriteRules` | the registry: every shipped set, explicitly listed, enumerable through `All` in a fixed order |

Composition is `Then`, `Repeat(n)` and `UntilStable(max)`. All three take their bound from the
caller: there is no unbounded rewrite loop anywhere in the layer, and `UntilStable` reports hitting
its bound as **no answer** rather than as the value it happened to be holding, so that a rule set
which does not converge is visible instead of silently truncated.

Registration is static and explicit. No assembly scanning, no `Activator`, no reflection — the layer
stays trimmable and NativeAOT-publishable, and `RewriteRules.All` is in an order that does not depend
on hashing or on which type loaded first.

## What already uses it

```
Entity.Simplify(level)     ->  Transformation.SimplificationAtLevel(level)  ->  Simplificator.Simplify
Entity.Expand(level)       ->  Transformation.ExpansionAtLevel(level)       ->  Entity.ExpandOverSum
Entity.Factorize(level)    ->  Transformation.FactorizationAtLevel(level)   ->  RewriteRules, composed
Entity.Differentiate(x)    ->  Transformation.Differentiation(x)            ->  Entity.DifferentiateOnce
Entity.Integrate(x)        ->  Transformation.Integration(x)                ->  Integration.ComputeIndefiniteIntegral
Entity.Limit(x, to, side)  ->  Transformation.LimitAt(x, to, side)          ->  LimitFunctional.ComputeLimit
Simplificator.SimplifyChildren  ->  a composed chain of four registry entries
```

Two of these are real ports rather than wrappers. `Factorize` is no longer a method that names its
own rules: it is `PerfectSquare`, then `Factorization`, then a tidying pass, repeated `level` times,
built out of the registry. `SimplifyChildren` — which every stage of the simplification pipeline runs
— is a chain composed once, statically, out of four registry entries instead of a hand-written run of
`Replace` calls. Both produce exactly what they produced before.

Everything else is a thin adapter over the algorithm that was already there. **Nothing that worked
was rewritten to make the architecture tidier.**

## What is deliberately not here

**`Solve` is not a transformation.** It consumes a *goal* — an equation, with a variable to solve for
— and produces a solution set, and the honest place for it is a tactic layer where a goal can become
subgoals. `Entity.Set` being an `Entity` means `Solve` would type-check as `Entity -> Entity`; that
is exactly why forcing it in would be a mistake, since it would compile while saying nothing true
about what the operation does. Same for `Isolate`, `Eliminate` and `Parametrize` when they arrive.

**There are no inverse transformations.** `Expand` and `Factor` are not inverses, and `Unsolve` is not
a well-defined operation. Where an inverse is mathematically meaningful there is room to add one; the
API does not invent symmetry that the mathematics does not have.

**`Heuristic` currently has no instance in the catalogue.** That is a statement about what is
registered so far, not a claim that nothing in the library guesses.

**Most of the pattern table is still only reachable from `Simplificator`.** `RewriteRules` registers
the ten sets the catalogue is built from. The rest are unchanged and unregistered.

## Adding the next one

A new transformation built from rules that already exist is one line:

```csharp
public static Transformation Rationalisation { get; }
    = Rewriting(RewriteRules.RationaliseDenominator).Then(InnerSimplification);
```

A new rule set is five, and registering it gets it enumeration, an identity, a soundness label, and
the tests that run over `RewriteRules.All` — including the one that asserts it reaches a fixed point
rather than rewriting in a cycle:

```csharp
public static RewriteRuleSet Power { get; } = new(
    nameof(Power),
    "Gathers and splits powers, roots and logarithms.",
    TransformationRelation.Equivalence,
    Soundness.SoundUnderAssumptions,
    Patterns.PowerRules);
```

Add it to `RewriteRules.All` in the same change — the list is explicit so that its order is a
decision rather than an accident.

Two things to get right:

- **State the relation honestly.** If the output is not another way of writing the input, it is
  `Derivation`, and the equivalence property test will correctly leave it alone.
- **Do not claim `Sound` without an argument.** Every rule set shipped today is
  `SoundUnderAssumptions`, and the test over `RewriteRules.All` enforces that, so promoting one means
  changing that test and saying why in the same change.

## What the next step is

The unit here is the rule *set*, not the single `pattern -> replacement` line, because every rewrite
in this library is a case of one `switch` and the compiler turns that into a type test and a jump —
splitting each case into an object would trade one dispatch per node for one delegate call per rule
per node, on the hottest path there is. Making the individual rewrites addressable without paying
that is the next piece of work, and nothing here forecloses it: a set whose rewrites become
individually reachable keeps its name and its entry in the registry.

After that, in dependency order: the goal/tactic layer that `Solve` belongs in, and then derivations,
which want every step to be attributable — which is what naming the steps was for.
