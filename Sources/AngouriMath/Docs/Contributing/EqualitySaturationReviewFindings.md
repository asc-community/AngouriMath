# Code review on #1101/#1102, and what is fixed versus what is recorded

A review pass over [#1101](https://github.com/asc-community/AngouriMath/pull/1101) (`EGraph` and
`Transformation.EqualitySaturation`) and [#1102](https://github.com/asc-community/AngouriMath/pull/1102)
(`Transformation.SimplificationAtLevel`), run before either had a human reviewer, found fifteen
issues. Two were confirmed wrong-answer bugs and are fixed, with a regression test each
(`EGraphTest.ExtractPreservesANarrowedCodomain`, `EGraphTest.ExtractPreservesEulerIntrinsicIdentity`,
and their `TransformationTest` counterparts at the public-API level). The other thirteen are recorded
here rather than fixed, because fixing several of them is a real design question — the same shape as
[`InversePairTable.md`](InversePairTable.md): naming what is true now, so the next attempt starts
from a measured position instead of rediscovering one.

## Fixed

**`EGraph.Extract` dropped `Entity.Codomain` on every reconstructed node.** It rebuilt through
`MatchPattern.ConstructNode`, a bare constructor, where the rest of the codebase copies `Codomain`
forward through a `New(...)` helper on every `Replace`. A `Codomain`-narrowed subexpression —
`sqrt(-1)` restricted to the reals evaluates to `MathS.NaN` where the default codomain evaluates to
`i` — silently reverted to its type's default on every `EqualitySaturation` call, whether or not any
rule fired. Fixed by recording each e-node's source `Codomain` at insertion (`EGraph.AddEntity` is
the only place a real `Entity` is available to read it from) and re-applying it in `Extract` when
present.

**The e-graph's leaf key collapsed `Entity.Constant.EulerIntrinsic` into the ordinary named constant
`e`.** Both print as `"e"` and are `Equals`-equal by design — see the doc comment on
`EulerIntrinsic` — but only `EulerIntrinsic` is meant to stay outside what a binder over the name `e`
can capture. Keying a leaf by `Stringize()` alone means `Extract` re-parses `"e"` and gets back the
named constant, which is invisible to every equality check and only wrong at a binder — silently
changing what `sum(ln(x), e, 1, 2)` means after a round trip through the graph. Fixed by keying
`EulerIntrinsic` on a distinguished sentinel instead of its printed form.

## Recorded, not fixed

**The deeper pattern under both fixes above.** The e-graph's node model has nowhere to carry
anything beyond raw tree shape — no `Codomain`, no leaf reference identity beyond what the two
targeted fixes now special-case, and, per the next two items, no way to represent a conditional
equivalence at all. Both fixes here are narrow patches for the two cases a review happened to find;
a rule that produces some other kind of metadata-bearing node would reopen the same class of bug.
Whether that calls for a general "attach anything, forget nothing" e-node representation, or stays a
list of special cases extended as each is found, is not decided here.

**A `SoundUnderAssumptions` rule's `Providedf`-wrapped result is unioned onto a dead end.**
`Providedf` is absent from `EGraph`'s 14-type reconstructible whitelist, so when a rule wraps its
result in a condition — the registry's own documented convention for a rule that does not hold
unconditionally — `Union` merges the original class onto an unbuildable wrapper class instead of the
useful unwrapped payload, and the improvement never reaches extraction. Representing "equivalent
under a condition" is not a data shape the e-graph has today.

**`Extract` silently no-ops on any root type outside its 14-type whitelist.** `Providedf`,
`Piecewise`, comparisons, and function application all fall outside it, so
`EqualitySaturation.Apply` on any expression rooted at one of these returns the unchanged input —
`Changed = false` — even when the e-graph proved an improvement somewhere inside it. Indistinguishable
from "already optimal".

**A `CostModel` that returns `NaN` for one candidate corrupts every later comparison in `Extract`.**
`if (here >= bestCost) continue` is false whenever either side is `NaN` (IEEE-754), so `NaN` becomes
`bestCost` and every subsequent candidate then unconditionally overwrites `best` — silently returning
an arbitrary, not-necessarily-cheapest answer instead of an error. A one-line `double.IsNaN(here)`
guard fixes it; left unfixed here because it was outside the two findings prioritised for immediate
correctness, not because it is hard.

**`SafeRules` filters by `RewriteRuleGrowth` alone, never by the owning `RewriteRuleSet.Soundness`.**
It holds safely today only because every set currently registered happens to be
`SoundUnderAssumptions` — a fact about today's registry, not an invariant the code enforces. A future
`Heuristic`-tier set contributing a `Collects`/`Rearranges` rule would be silently incorporated while
`EqualitySaturation` keeps reporting `SoundUnderAssumptions`.

**The saturation loop's budget gate charges late, and `Rebuild` is never charged at all.**
`ChargeGrowthSinceLastCall` is called before a class's full `SafeRules` sweep runs, not after, so
the entire sweep's growth lands uncounted before the next check — a `WorkBudget { Steps = 50 }` can
already have overshot well past 50 by the time the overshoot is noticed. `Rebuild`'s own
potentially-multi-round full-graph rescan has no `BudgetLedger` interaction anywhere in it.

**Cost ties in `Extract` are broken by `HashSet<ENode>` enumeration order** — an undocumented .NET
implementation detail — where `BudgetLedger`'s own stated principle is that a bounded computation is
exactly reproducible given a defined algorithm order. `CostModelTest.OnlyTheDefaultCaresAboutARootInADenominator`
already proves exact ties are not rare in practice.

**`Extract`'s recursion has cycle protection but no explicit depth cap.** E-class dependency depth
can exceed the original expression's syntactic depth after several saturation rounds link classes
together, and under a generous or unlimited `WorkBudget` this risks an uncatchable
`StackOverflowException` rather than a graceful budget exhaustion.

**`RewriteRecording` — the library's only "what rewrote this and why" mechanism — cannot see
`EqualitySaturation` at all.** It is populated exclusively inside `RewriteRuleSet.ApplyOnce`;
`EqualitySaturationTransformation.ApplyCore` calls `rule.TryApply` directly, bypassing it entirely.
A caller who wraps a call in `RewriteRecording.Start()` — the library's own documented pattern for
introspection — gets a real rewrite with an empty derivation and no signal that introspection failed
rather than legitimately finding nothing.

**`EGraph.OperatorTypes` hand-duplicates the exact 14-type list `MatchPattern.Construct` already
hardcodes**, next to a sibling doc comment on `Construct` itself warning that "a list written twice
is a list that drifts". A future PR extending one without the other silently loses e-graph coverage
for the new type, with no compiler error.

**`RewriteRule.NodeTypes` — an existing, documented fast pre-filter — is never consulted before
`ApplyCore` calls the full `TryApply` pattern match on every `SafeRules` entry, per class, per pass.**
Purely a performance gap: `Common` alone has roughly 100 arms, and the large majority of match
attempts against any given class are wasted and uncharged against the budget.

**`NeutralClass` hand-rolls an identity-element table for `Sumf`/`Minusf`/`Mulf`/`Divf`/`Powf` that
already exists, tested, in each type's own `InnerSimplify`.** Nothing keeps the two in sync — a new
special case added to `InnerSimplify` (`Powf`'s `1 ^ x` arm already attaches a `Providedf` domain
condition `NeutralClass` has no way to learn about) would leave `EGraph` asserting an equivalence the
rest of the library no longer believes, silently and with no test to catch the divergence.

**`Entity.SimplifiedRate`'s cache can go stale across different `CostModel`s (PR #1102).** It is a
`LazyPropertyA<double>` that computes once per `Entity` instance and caches forever, so a caller who
reads `.SimplifiedRate` or calls `.Simplify()` under one `CostModel` and then runs
`SimplificationAtLevel(level, differentCostModel)` on the same or an overlapping entity can have
`Simplificator.PickSimplest` compare a stale rate against a fresh one with no error. The library
already documents this exact trap elsewhere (`MathS.Settings.ComplexityCriteria`'s own example uses
`FromString(expr, useCache: false)` to avoid it); `SimplificationTransformation.ApplyCore` does no
analogous cache-busting.

## What is not decided here

Whether the e-graph's node model should grow a general mechanism for metadata that travels with a
node (the `Codomain`/`Providedf`/root-type-whitelist cluster above), or whether each case is worth
its own targeted fix as found — same open question `InversePairTable.md` leaves for its own
mechanism, and arguably the same question either way: how much should ride along on an e-node beyond
its bare shape. The `NaN`-guard, the `Soundness` check, and the budget-timing fix are each small and
independent of that question and of each other; nothing here blocks any one of them being picked up
on its own.
