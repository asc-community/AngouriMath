# Code review on #1101/#1102, and what is fixed versus what is recorded

A review pass over [#1101](https://github.com/asc-community/AngouriMath/pull/1101) (`EGraph` and
`Transformation.EqualitySaturation`) and [#1102](https://github.com/asc-community/AngouriMath/pull/1102)
(`Transformation.SimplificationAtLevel`), run before either had a human reviewer, found fifteen
issues. Fourteen are now fixed, each with a regression test named beside it below. What is left is not
a defect but a design question — the same shape as [`InversePairTable.md`](InversePairTable.md):
naming what is true now, so the next attempt starts from a measured position instead of rediscovering
one.

The two sections below are the live account; whoever settles the last one moves its entry up rather
than adding a third place to look. One entry in **Fixed** also carries a correction to what this
document originally claimed, because the recorded diagnosis turned out to be wrong once it was
measured — which is why every entry here now names the test that holds it, rather than only the
reasoning that produced it.

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

**`RewriteRecording` — the library's only "what rewrote this and why" mechanism — could not see
`EqualitySaturation` at all.** It is populated inside `RewriteRuleSet.ApplyOnce`, and saturation asks
rules directly, so a caller who opened a recording round it got a real rewrite with an empty
derivation and nothing to distinguish "introspection cannot see this" from "there was nothing to
see". Fixed by noting the pass — one edge, input to output, under the transformation's own name:
`TransformationTest.EqualitySaturationIsVisibleToARecording`, with
`EqualitySaturationRecordsNothingWhenItChangesNothing` for the other half of the convention.

*Deliberately not finer than the pass, and this is a fact about e-graphs rather than a shortcut.* A
rule set records each firing because a firing there **is** the rewrite: the node it matched leaves and
the replacement takes its place. A firing in saturation is not — it adds another member to an e-class
whose members are all already believed equal, and the answer is then chosen by `Extract` from all of
them at once. Most firings contribute nothing to what extraction picked, and none of them is a step
on a route from input to output, because there is no route. Reporting them as `RewriteStep`s would
name rewrites that are not in the answer.

**`RewriteRule.NodeTypes` — a documented fast pre-filter — was never consulted, so every rule ran a
full pattern match against every class of every pass.** `MatchPattern.RequiredRootType` was already
there to be asked; a pattern requiring a root type cannot match a class holding no node of it, and
being a *necessary* condition is exactly what makes it a filter — it licenses skipping a rule, never
firing one. Fixed by gathering each class's node types once per sweep and consulting it before the
attempt. Measured over four expressions, match attempts fall by about thirteen times — 1076 steps to
78 on the largest, 87 to 3 on `x + 0` — with the same answer in every case:
`TransformationTest.EqualitySaturationDoesNotAttemptEveryRuleOnEveryClass`, asserted against the rule
count rather than a recorded number so it stays true as rules are added.

**`NeutralClass` hand-rolled an identity table that `InnerSimplify` already implements and tests.**
Nothing kept the two in step, and the divergence would have been silent in the worst direction: the
e-graph would go on asserting an equivalence the rest of the library had stopped believing, and merge
two classes that are no longer equal. Fixed by deriving the table — asking `InnerSimplified` whether
`op(x, leaf)` really is `x`, for each buildable binary operator and each of the two identities. That
settles the asymmetries without anyone having to remember them (`0 - x` is a negation, `1 / x` a
reciprocal, `1 ^ x` the constant 1), and it handles the case a written table cannot: an arm that
answers with a condition attached does not answer with the bare operand, so no fold is claimed.
`EGraphTest.TheFoldsAreExactlyWhatInnerSimplifyDoes` names what the derivation finds, so a change in
`InnerSimplify` shows up as a changed list rather than as e-graph folding quietly gaining or losing a
case.

**`Entity.SimplifiedRate` answered one cost model's question with another's cached number (PR
#1102).** The cache is one slot per `Entity` instance and the criteria is an ambient setting, so the
two do not agree about what the cached number is a rate *of*. Not merely stale:
`Simplificator.PickSimplest` compares candidates by this property, so it would weigh one model's
cached rate against another's fresh one and choose on the strength of it, with nothing anywhere to say
the comparison was meaningless. Fixed by caching only while nobody has scoped the setting — the
`IsOverriden` test `BudgetLedger.For` already applies to `MathS.Settings.Budget` — so any other
criteria is computed afresh and can neither be answered with somebody else's number nor leave one
behind for them: `CostModelTest.ARateIsNotAnsweredFromAnotherCostModelsCache` and
`EveryModelIsAnsweredWithItsOwnRate`.

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

*And narrower again, measured rather than argued.* **No written pattern can put a condition into the
graph at all.** Only 35 of the 324 rules have a right-hand side written as a pattern, and none of
them mentions `provided` — so a `Providedf` reaches an e-class only along the code-built path, where
a rule extracts a witness term, runs code on it and puts the result back. That is worth knowing
before anyone reopens this: the union that would need a meaning is not one e-matching can currently
form. `ProvidedInAnEClassTest.NoWrittenPatternIntroducesACondition` holds it at zero, so the first
two-sided conditional rule fails there rather than quietly making the question live again.

## What is not decided here

The one question above, restated as the decision it is: **how much should ride along on an e-node
beyond its bare shape.** A general "attach anything, forget nothing" representation, or a list of
special cases extended as each is found. `InversePairTable.md` leaves the same question open for its
own mechanism, and it is arguably the same question either way.

*Decided for the one live instance, by measurement rather than by preference.* `Codomain` was the
special case, kept in a side table keyed on shape alone — and that was wrong in three places at once.
`abs(x)` and `domain(abs(x), Any)` hashed to one e-node and so shared an e-class, which is the graph
asserting two unequal values are equal; extraction then returned whichever had been inserted last;
and `Rebuild` re-created every node from operator and children only, dropping the codomain on any
union. `ENodeIdentityTest` reproduced all three before anything was changed.

The rule that falls out is not "attach anything" and not "special-case each": it is that **an
e-node's identity must include everything that makes two entities unequal**, because an e-class is
an equality claim. `Codomain` distinguishes unequal entities, so it is now a field of `ENode` and
part of `Equals`, `GetHashCode` and the ordering — and the side table is gone. Anything that is
*not* part of equality (a cached hash, a cost) stays off the node. That answers the question for
any future per-node datum too: ask whether two entities differing only in it are equal, and the
answer says which side of the line it goes.

What the fourteen closed findings say about it is worth recording, because it is not what the review
expected. Only one of them turned out to depend on this decision at all — `Codomain`, which is still
special-cased. The `Providedf` case, which this document originally offered as evidence that the
e-graph had no way to represent a conditional equivalence, was the buildable-type table and nothing
deeper. The rest were ordinary defects: a missing guard, an undefined order, an absent depth cap, a
budget charging the wrong quantity, three lists written twice, a filter never consulted, a cache
keyed on nothing. **A cluster of findings around one subsystem invites a single grand explanation,
and thirteen of these fourteen did not have one.**
