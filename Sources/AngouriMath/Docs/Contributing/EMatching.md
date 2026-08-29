# A real e-matcher: matching a pattern against an e-class, not a term

`Transformation.EqualitySaturation` (#1101) says plainly, in its own doc comment, what it is not:

> The harness this is built from enumerates a class's terms and rewrites each, which finds the same
> equalities e-matching would but by materialising terms a real e-matcher never builds ... A production
> e-matcher over `MatchPattern` is not this; it is what tier 2 still names as its production caller's
> other missing half.

This is that other half. Read [`ReversibleRules.md`](ReversibleRules.md) and
[`InversePairTable.md`](InversePairTable.md) first — the second is the reason this is scoped the way
it is: the inverse-pair-table attempt found that most of the registry's addressable rules have no
`MatchPattern` at all, only compiled C# lambda text, and that finding is load-bearing here too.

## What this is not trying to fix

**This does not make `Growth.Expands` rules safe to include.** That was the original motivation
raised for this work, and it was checked against the measurement before committing to a design: the
`work/egraph` harness's 7,147× blowup happened with hash-consing and congruence closure already in
place, discovered through term enumeration. An expand rule mostly produces genuinely new shapes each
time it fires, so congruence closure — which catches *exact* repeats — does not bound that growth,
and switching how a match is *found* does not change how many new shapes a rule *produces*. If
`Expands` rules are ever included safely, that is a scheduling policy (deferring them until
`Collects`/`Rearranges` reach a local fixed point, capping how often one rule fires per class, or
using the reversibility work to avoid firing a rule right next to its own inverse) layered on top of
whichever matcher is underneath — not a consequence of this document. This document is scoped to what
e-matching actually buys: not materialising a term to find out whether a rule's *shape* matches.

## Where the patterns actually are

`RewriteRules.All` — the public registry `Transformation.EqualitySaturation` currently draws
`SafeRules` from — is mostly `RuleRegistryGenerator` output: a `RewriteRule`'s `PatternSource` and
`ReplacementSource` are rendered *text*, produced from a `switch` arm's Roslyn syntax after the fact.
There is no `MatchPattern` behind most of them to match a class against — see
`InversePairTable.md`'s measurement, which found `AsAddressable()` (the one place a `RewriteRule` is
built from a real `MatchPattern`) called exactly once, for `RationalizeDenominator`.

The real patterns live in `AngouriMath.Core.Transformations.Matching.MatchedRules`, the internal
catalogue `MatchedRuleSet`/`MatchedRule` declarations, whether or not a given set is wired into the
public registry's `Rules` property. **This is the source `EqualitySaturationTransformation` moves to.**
It changes what "the rules `EqualitySaturation` uses" means: not `RewriteRuleGrowth ∈
{Collects, Rearranges}` over the public surface, but the same classification computed over real
pattern trees (below) for every set in `MatchedRules` — which may be a larger or smaller list than
today's 313 depending on which sets `MatchedRules` currently holds; this document does not guess the
number, the implementation measures it.

`EqualitySaturationTransformation` already lives in `AngouriMath.Core.Transformations`, the same
assembly as `Matching.MatchedRules` — referencing it needs no visibility widening, unlike
`MatchPattern.Construct` → `ConstructNode` in #1101.

## The four pattern shapes, and which one does not e-match

`MatchPattern`'s four concrete kinds are `private sealed` classes nested inside it — there is no way
to add matching capability from outside; it goes into `MatchPattern.cs` directly, next to `Match` /
`TryMatchOnce` / `TryMatchChoice`, for the same reason those three agree with each other: a single
place holds the invariant.

| Kind | What it matches | E-matches? |
|---|---|---|
| `AnyPattern` | anything, or anything of a type, or anything of a type satisfying a predicate | yes |
| `ExactPattern` | one literal value exactly | yes |
| `NodePattern` | a fixed-arity node, optionally commutative | yes |
| `GatheredPattern` | a flattened sum/product chain, parts assigned to some operands | **no** |

`GatheredPattern` is already, by its own documentation, "the one shape that has to be enumerated" —
true against a single concrete `Entity`. Against an e-*class*, which can bundle many equivalent tree
shapes into one, the same assignment search has no obvious bound the way `NodePattern`'s does, and
attempting one is a second research problem this document does not take on. A pattern containing a
`GatheredPattern` anywhere falls back to today's extract-a-term-then-`TryApply` path, unchanged.

## New members on `MatchPattern`

```csharp
internal abstract bool CanEMatch { get; }
internal abstract IEnumerable<EBindings> EMatch(EGraph graph, int classId, EBindings bindings);
internal abstract bool ETryBuild(EGraph graph, EBindings bindings, out int classId);
```

`CanEMatch` is computed structurally, the same way `IsBuildable` and `IsDeterministic` already are:
`false` for `GatheredPattern`, and for any `NodePattern` with a non-`CanEMatch` child; `true`
otherwise. Checked **once per pattern, up front** — a rule decides which path it takes before
searching, not by discovering mid-search that a shape is unsupported. `Left` and `Right` are checked
**independently**: a rule may e-match its `Left` and still need one extraction to build a
`Gathered`-shaped `Right`, rather than falling back on the whole rule because one side cannot go all
the way.

`EBindings` mirrors `Bindings` exactly — the same cons-list, immutable, sharing a tail per the same
reasoning `Bindings`'s own doc comment gives — except it maps a name to an e-class id (`int`) instead
of an `Entity`. One concrete win falls out of this for free: a repeated name (`x - x -> 0`'s `AnyPattern`
binding the same name twice) becomes `graph.Find(a) == graph.Find(b)`, an O(1) check, where matching
against a concrete term needs `Entity.Equals`. Not the main point of this work, but a real one.

### Matching, per kind

- **`ExactPattern`**: does the class already contain a leaf e-node equal to the literal? A class
  membership lookup — the same check `EGraph`'s neutral-folding already does — no extraction.
- **`AnyPattern`**: for every e-node in the class satisfying the required type, bind the name to the
  *class*. A non-leaf e-node's `Op` is already its type name (`EGraph`'s existing `OperatorTypes` map,
  read in the type→name direction too); a leaf e-node's `Op` is its printed form, so checking a
  required type means reparsing that one string (`"2".ToEntity() is Number`) — real work, but **O(1)
  per leaf candidate**, not the recursive, depth-scaling cost of materialising a whole subtree. Where
  the pattern also carries a `where` predicate, that candidate needs a witness — see lazy extraction,
  below.
- **`NodePattern`**: for every e-node in the class whose `Op` matches the required type, recurse into
  each child *pattern* against the corresponding child *class*; `commutative` tries both child-class
  orders, exactly as `MatchCore` does for concrete children today.
- **`GatheredPattern`**: `CanEMatch` is `false`; `EMatch` is not implemented and is never called.

### Lazy extraction — exactly three triggers, nowhere else

1. An `AnyPattern`'s inline `where` predicate, once a candidate is otherwise structurally matched.
2. A `MatchedRule`'s own `when` condition, checked once after a complete structural match (at the
   `MatchedRule` level — `MatchPattern` itself has no `when`).
3. A code-built replacement (`Right is null`), which needs real `Entity` bindings to call.

In each case: `EGraph.Extract` (already built, #1101) one witness for the specific class the check
needs, under the transformation's own `CostModel`. A failed check backtracks to the next `EMatch`
candidate — it does not abort the rule, the same way a concrete-term match failing on one candidate
does not stop `Match` from trying the rest.

### Building the replacement without materialising it either

`ETryBuild` mirrors `TryBuild` node for node, but calls `graph.Add(op, childClassIds)` in place of
`new Entity.Sumf(...)`. A `NodePattern` replacement built this way runs through `EGraph.Add`
unchanged, which means **it gets #1101's neutral-folding for free** — a replacement that structurally
builds `x + 0` folds into `x`'s class on insertion, no second pass needed. Where `Right` cannot
`ETryBuild` (a `Gathered` shape, or code) but `Left` did e-match, only the classes the match actually
bound are extracted — narrower than falling back on the whole rule.

## What changes in `EqualitySaturationTransformation`

- Rule source: `Matching.MatchedRules`'s sets, not `RewriteRules.All`.
- Growth classification: a real node-count walk over each `MatchedRule.Left`/`.Right` pattern tree,
  computed once at startup — the same three-way `Collects`/`Rearranges`/`Expands` split
  `RewriteRuleGrowth` already names, now exact rather than the public surface's string-length proxy.
  `SafeRules` is still `Collects ∪ Rearranges` — this document does not reopen the `Expands` question,
  per the first section.
- The saturation loop: for each class, for each safe rule — `EMatch` the `Left` if `CanEMatch`, else
  extract a representative and match it the current way; check `when` lazily on a structural hit;
  build via `ETryBuild` if `Right.CanEMatch`, else extract the bound classes and use `TryBuild` or the
  code delegate; `Union` the result into the graph.
- `Name`/doc comment: state which fraction of `MatchedRules`'s rules actually took the e-match path
  in a given run (a `Gathered`-containing `Left` is common — `two-added-fractions-take-a-common-denominator`
  and its kin are exactly this shape), so the report is honest about how much of the corpus this
  reaches rather than claiming the mechanism covers everything it is offered.

## Verification

- `DeterministicMatchingAgreesWithEnumeration` and `BoundedMatchingAgreesWithEnumeration` already hold
  `Match`/`TryMatchOnce`/`TryMatchChoice` together over generated expressions; a fourth test in the
  same shape — `EMatchingAgreesWithMatching` — builds a single-entity e-graph (one class per subterm,
  no sharing) from a generated corpus and checks that `EMatch` against the whole-expression class
  yields the same set of bindings `Match` does against the entity itself, translating each binding's
  class back to the entity it was built from. Single-entity is deliberate for this test: it isolates
  "does e-matching find what term-matching finds" from "does congruence change what a class denotes",
  which is a different, already-tested question (`EGraphTest`, #1101).
- A second test builds a real, shared e-graph (several equal-by-rule expressions unioned together) and
  checks `EMatch` finds a match that requires crossing a union — the case term-matching against one
  representative cannot reach at all, which is the actual capability this adds over #1101's extraction
  approach for `CanEMatch`-eligible patterns.
- `ETryBuild` output is checked against `TryBuild` output: build the same bindings both ways (as
  classes and as the entities those classes were extracted from) and confirm the extracted result of
  the class-built version equals the directly-built entity.
- The existing `EqualitySaturationNeverChangesTheValueItClaimsToPreserve` (#1101,
  `TransformationTest.cs`) continues to hold under the new rule source and matching path unchanged —
  it is a claim about the transformation's output, not about which matcher produced it.

## What is deliberately not decided here

**Whether `Growth.Expands` rules ever join `SafeRules`**, and under what scheduling policy — a
separate document's question, once one exists to answer it.

**Whether the local fallback (extract-then-`TryApply`, kept for `Gathered`-containing rules) is worth
removing later** by also solving e-matching for `GatheredPattern`, or stays permanently as the honest
boundary of what this mechanism covers. Not decided because it does not need to be yet: the fallback
is cheap to keep and correct as written, and removing it is a strict improvement whenever it happens,
not a decision with a deadline.

**Whether `RewriteRules.All`'s public registry should also move its `Rules` source to
`MatchedRuleSet.AsAddressable()`** — `InversePairTable.md`'s "Option B", named there as the
architecturally right but expensive fix for a different problem (public addressability). This
document's rule-sourcing change is internal to `EqualitySaturationTransformation` and does not touch
the public registry at all, so it neither requires nor blocks that decision.
