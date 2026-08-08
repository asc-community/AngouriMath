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
| `RewriteRules` | the registry: every rule set the simplifier applies, explicitly listed, enumerable through `All` in a fixed order |
| `RewriteRecording` / `RewriteStep` | a scope that collects the rewrites which fired while it was open — off unless asked for, and free when off |

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

## Recording what fired

```csharp
using var recording = RewriteRecording.Start();
var simplified = ((Entity)"a / (b / c)").Simplify();
foreach (var step in recording.Steps)
    Console.WriteLine(step);      // Common: a / (b / c) -> a * c / b
```

This is [#28](https://github.com/asc-community/AngouriMath/issues/28). Three things about it
are deliberate and worth keeping:

**Free when off.** With no recording open, applying a rule set costs one thread-static read
more than it did before — per rule set, not per node — and allocates nothing. That is why it
is a scope rather than a setting: a setting is something a caller can leave on.

**Per thread**, like `MathS.Settings`, so a parallel caller records its own work and nobody
else's. Recordings nest, and an inner one hides the outer until it closes.

**A step is a subexpression, not a snapshot.** A rewrite pass walks bottom-up and rewrites
nodes as it goes, so there is no moment at which a partly-rewritten whole expression exists
to photograph. #28's example shows whole-expression snapshots; reporting those would mean
constructing something the engine never held.

And the honest limit, which the type's own documentation states: **these are the rewrites,
not everything `Simplify` did.** Simplification also expands, factorises, divides
polynomials, minimises boolean expressions, and then *chooses* among candidates by a
complexity metric. The steps are every rewrite that fired across every candidate generated —
including candidates that lost. Reading them as a route from the input to the returned answer
would be reading in something that is not there. Making that route available is the
derivation work in #746's v5.0 tier, and it needs the candidate search to be attributable
first, not just the rewrites.

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

**Some rewrites still bypass the registry.** Every set the *simplifier* applies is registered, and
`Simplificator` reaches all of them through `RewriteRules` — that is what lets an account of what
`Simplify` did be a complete one, since a set reachable only by its method has no name to attribute a
step to. The equation and set solvers, the integrator and `TreeAnalyzer` still call `Patterns`
directly. `Patterns.TrigonometricToExponentialRules(from, to)` cannot become a registry entry as it
stands: it is parameterised by two variables, so it is a family of sets rather than one.

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
decision rather than an accident. Where a set is parameterised by something with a small fixed range,
register one entry per value and add an `internal` chooser beside them (`CanonicalOrderAt`,
`CommonDenominatorAt` are the two): the alternative is an entry that cannot be enumerated, which
defeats the point of the list.

Inside the library, apply a set with `expression.Rewrite(RewriteRules.Power)` rather than
`RewriteRules.Power.ApplyOnce(expression)` — a sequence of rule sets otherwise reads inside out.

Two things to get right:

- **State the relation honestly.** If the output is not another way of writing the input, it is
  `Derivation`, and the equivalence property test will correctly leave it alone.
- **Do not claim `Sound` without an argument.** Every rule set shipped today is
  `SoundUnderAssumptions`, and the test over `RewriteRules.All` enforces that, so promoting one means
  changing that test and saying why in the same change.

## What the next step is

The unit here is the rule *set*, not the single `pattern -> replacement` line — because that is what
has been built, **not** because the finer grain costs too much. This document used to say it would
trade one dispatch per node for one delegate call per rule per node. That was asserted, never
measured, and it is wrong: [#825](https://github.com/asc-community/AngouriMath/issues/825) measures
rules bucketed by the node type they match at **as fast as** the hand-written `switch` on a
realistic-sized set and **2.3× faster** on a small one, at identical allocation. A large `switch`
over distinct node types is compiled into that same type dispatch anyway, so an explicit registry is
not paying for anything — it is writing down what the compiler already infers from arm order.

What does stand in the way is transcription: splitting forty `switch` arms by hand is forty chances
to change a pattern silently, and the per-rule metadata has to stay in step with the pattern. Both
point at a source generator over the existing bodies, which is what #746 item 50 should decide.
Nothing here forecloses it: a set whose rewrites become individually reachable keeps its name and its
entry in the registry.

After that, in dependency order: the goal/tactic layer that `Solve` belongs in, and then derivations,
which want every step to be attributable — which is what naming the steps was for.
