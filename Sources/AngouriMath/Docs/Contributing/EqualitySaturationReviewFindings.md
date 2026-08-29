# Code review on #1101/#1102, and what is fixed versus what is recorded

A review pass over [#1101](https://github.com/asc-community/AngouriMath/pull/1101) (`EGraph` and
`Transformation.EqualitySaturation`) and [#1102](https://github.com/asc-community/AngouriMath/pull/1102)
(`Transformation.SimplificationAtLevel`), run before either had a human reviewer, found fifteen
issues. Ten are now fixed, each with a regression test named beside it below. The other five are
recorded here rather than fixed, because fixing several of them is a real design question — the same
shape as [`InversePairTable.md`](InversePairTable.md): naming what is true now, so the next attempt
starts from a measured position instead of rediscovering one.

The two sections below are the live account; whoever closes one of the remaining five moves its entry
up rather than adding a third place to look. One entry in **Fixed** also carries a correction to what
this document originally claimed, because the recorded diagnosis turned out to be wrong once it was
measured.

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

**`SafeRules` filtered by `RewriteRuleGrowth` alone, never by the owning set's `Soundness`.** It held
safely only because every set then registered happened to be `SoundUnderAssumptions` — a fact about
that day's registry, not an invariant. Fixed when `SafeRules` moved onto `MatchedRules.All`, which
carries a per-rule `Soundness` the public-surface-sourced version had no way to read.

**A `CostModel` answering `NaN` corrupted every later comparison in `Extract`.** `here >= bestCost` is
false whenever either side is `NaN` (IEEE-754), so `NaN` became the incumbent cheapest and every
candidate after it then won unconditionally — the answer stopped being the cheapest and became
whichever member the enumeration reached last. Fixed by declining an unranked candidate, which is
already what a cost model that *throws* gets: `EGraphTest.ExtractDeclinesACandidateItsCostModelCannotRank`
and `ExtractPicksTheCheapestPastACandidateItCannotRank`.

**Cost ties in `Extract` were settled by `HashSet<ENode>` enumeration order.** An unspecified
implementation detail, and string hashing is randomised per process, so one run's tie-break was not
the next run's. Fixed with a total order on `ENode` — ordinal on the operator, then children — so a
tie goes to the ordinally-first e-node: `EGraphTest.ExtractSettlesACostTieOnADefinedOrder`.

**`Extract`'s recursion had cycle protection but no depth cap.** Its cycle guard bounds the chain only
by the number of distinct classes, which unions grow past the input's own syntactic depth, so a deep
enough graph exhausted the stack — and a `StackOverflowException` cannot be caught, so it takes the
process down rather than failing one call. Fixed with a `MaxExtractionDepth` of 256 that declines to
build, the answer the cycle case already gives and the shape `Gruntz.MaxDepth` already uses:
`EGraphTest.ExtractDeclinesToBuildPastItsDepthCap`, with
`ExtractStillBuildsAnExpressionOfOrdinaryDepth` holding the cap to being a crash guard rather than a
quality knob.

**`WorkBudget.Steps` bounded nothing at all — and the review said something weaker and wrong about
why.** It was recorded here as a *timing* defect, that `ChargeGrowthSinceLastCall` ran before a
class's sweep rather than after, so a sweep's growth landed uncounted. Measuring it found the timing
is not the problem: `Steps` charged the e-graph's *node-count growth*, and `SafeRules` is by
construction the rules whose `Growth` does not expand — so on ordinary input the ledger was charged
nothing whatever. Under `Steps = 0`, three of five varied expressions ran the entire sweep to
saturation and then reported that they had `Completed`, never having reached a ceiling. `Time` was the
only bound that was really holding. Fixed by charging what every other bounded computation in the
library charges — one step per unit of work attempted, as Buchberger, FGLM and `MatchPattern` all do —
before the attempt, with the growth charge kept alongside it and moved after the sweep, and with
`Rebuild`'s full-graph rescan charged for the first time:
`TransformationTest.EqualitySaturationStopsWhenItHasNoStepsToSpend` and
`EqualitySaturationSpendsNoMoreThanOneStepPastItsCeiling`.

*Worth keeping as a note about reviews rather than about this code: the finding was real and the
mechanism given for it was wrong, and the wrong mechanism was the more flattering one — a bound that
is slightly late reads as a rounding error, where a bound that never fires is the feature missing.
The probe that separated them was four lines and had not been run.*

**`EGraph` kept its own copy of the node-type list `MatchPattern.Construct` hardcodes.** Two lists of
the same fourteen types, next to a doc comment on `Construct` itself warning that a list written twice
is a list that drifts — and the drift would have been silent, since a type added to one and not the
other simply stops being reachable from the e-graph with no compiler error. Fixed by making one table
hold all three facts at once — the type, its arity, and the constructor call — with the list, the
name lookup and `CanConstruct` all derived from its keys, so there is no second list left to drift.
Being a table rather than a chain of `nodeType == typeof(T)` tests also makes the lookup O(1), which
matters now that the list is three times longer: `BuildableNodeTypesTest` holds it to `Construct` by
reflecting over every concrete node type, and builds each declared one for real, since a lookup that
says "yes" does not prove a constructor runs.

**`Extract` silently no-opped on any root type outside that list, and a `Providedf`-wrapped result was
unioned onto a dead end.** Two findings with one cause. Fourteen types is what `Construct` had
accumulated, not a principled boundary: comparisons, connectives, the inverse trigonometric functions,
`floor`/`ceil`/`round`, `mod`, `gcd`, `min`/`max`, the set operations and `Providedf` were all absent,
so `EqualitySaturation.Apply` on an expression rooted at any of them returned its input reporting
`Changed = false` — indistinguishable from "already at its cheapest" — and a rule following the
registry's own convention of wrapping a conditional result in `Providedf` had that result merged onto
a class nothing could build. Fixed by widening the table to 44 types, which is every node type whose
constructor takes one or two `Entity` children, less two binders. Six rules gain a reverse direction
as a result (`a-sine-of-an-arcsine` and its three siblings, `a-union-with-itself-is-itself`,
`an-intersection-with-itself-is-itself`); each reversed rule is `Expands`, so none of them joins
`SafeRules`.

*What stays out, and why, is now a statement rather than an accident.* **Binders** — `Lambda`,
`ConditionalSet` — because the e-graph has no notion of a bound variable's scope and would union a
bound occurrence with a free one, and because `DirectChildren` hands out a capture-avoidingly renamed
body rather than the written one; rebuilding either would produce a term meaning something else.
**Variable-arity nodes** — `Piecewise`, `Application`, the finite `Set`s, `Matrix` — because the table
keys on an arity of one or two; widening it to n children is a separate piece of work rather than a
principle. `EGraphTest.ExtractStillDeclinesWhatItCannotFaithfullyRebuild` pins both, including the
case where the root is buildable and its operands are not.

## Recorded, not fixed

**The deeper pattern: an e-node carries raw tree shape and nothing else.** `Codomain` and
`EulerIntrinsic`'s reference identity are each special-cased, and a rule producing some other kind of
metadata-bearing node would reopen the same class of bug. Whether that calls for a general "attach
anything, forget nothing" e-node representation, or stays a list of special cases extended as each is
found, is still not decided.

*Narrower than it was, though, and worth saying how.* This entry used to name the `Providedf` case as
evidence that the e-graph had "no way to represent a conditional equivalence at all". That was the
whitelist rather than the node model: `Providedf` is an ordinary two-child node, and giving
`Construct` a line for it was the whole fix. What remains is genuinely about the node model —
per-node data that is not a child, of which `Codomain` is the only instance the library has today.

**`RewriteRecording` — the library's only "what rewrote this and why" mechanism — cannot see
`EqualitySaturation` at all.** It is populated exclusively inside `RewriteRuleSet.ApplyOnce`;
`EqualitySaturationTransformation.ApplyCore` calls `rule.TryApply` directly, bypassing it entirely.
A caller who wraps a call in `RewriteRecording.Start()` — the library's own documented pattern for
introspection — gets a real rewrite with an empty derivation and no signal that introspection failed
rather than legitimately finding nothing.

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
its bare shape. That question is still open, and it is what the `Providedf` and root-type-whitelist
entries above are both waiting on.

The five closed since — the `Soundness` check, the `NaN` guard, the tie order, the depth cap and the
budget — were each independent of it and of each other, which is why they went first. Of the eight
that remain, only two (`Providedf`, the root-type whitelist) actually depend on that decision; the
other six are ordinary work that nothing here blocks.
