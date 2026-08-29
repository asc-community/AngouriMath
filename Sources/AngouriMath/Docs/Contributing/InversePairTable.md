# The inverse-pair table, and why it is not a small extension of `Growth`

[#746](https://github.com/asc-community/AngouriMath/issues/746) tier 2 names this as what remains
once a rule can be read backwards ([`ReversibleRules.md`](ReversibleRules.md)):

> the consumer tier 2 names next is the inverse-pair table an e-graph needs: equality saturation
> keeps both results where a pipeline keeps one, so it has to be told which rewrites undo each
> other or it grows without bound. `RewriteRuleGrowth` is what that question has today, and it does
> not answer it. Growth says a rule was written smaller or larger than its pattern; it does not say
> *which* rule is the inverse of *which* ... A reversible rule is the answer, because its inverse is
> the rule itself read the other way.

This reads like the same move `RewriteRuleGrowth` already made: an internal `MatchedRule` computes
something (there, `Growth`; here, `Reversal`/`Reversed`), and `RewriteRule.AsAddressable()` renders
it onto the public, addressable type. That move was tried. It does not work, and the reason is worth
recording so the next attempt spends its effort on the real obstacle rather than rediscovering this
one.

## What was measured

`AsAddressable()` is called exactly once in the whole registry:

```
$ grep -n "AsAddressable()" Sources/AngouriMath/Core/Transformations/RewriteRules.cs
260:            Matching.MatchedRules.RationalizeDenominator.AsAddressable());
```

Every other entry in `RewriteRules.All` — `Common`, `Power`, `Trigonometric`, `CollapseMultipleFractions`,
all thirty — is built like this instead:

```csharp
public static RewriteRuleSet CollapseMultipleFractions { get; } = new(
    nameof(CollapseMultipleFractions),
    ...
    Matching.MatchedRules.CollapseMultipleFractions.ApplyHere,   // how the set runs
    Patterns.CollapseMultipleFractionsArms);                     // where Rules comes from
```

Two arguments, two different jobs. `ApplyHere` is the exchange #825 measured at ~5% and shipped:
the set *runs* against the matcher. `Patterns.CollapseMultipleFractionsArms` is the original
`switch` method, kept **only** so `RuleRegistryGenerator` can still read its arms and produce the
`Rules` property — name, pattern text, node types, `Growth` — the same way it always has. The
"exchange" replaced how a set is *applied*. It did not touch where `Rules` comes from for any set
that still has a `switch` to read, which today is every set except the one whose shape the generator
declined outright.

So `RewriteRuleGrowth` was exposable by extending `AsAddressable()` and `RuleRegistryGenerator`
*both* — the generator already computes `Growth` from Roslyn syntax (`Growth(arm.Pattern,
arm.Replacement)` in `RuleRegistryGenerator.cs`), comparing rendered text lengths, which needs
nothing a syntax tree doesn't already have. `Reversed` is not that: it is a **behaviour** —
`MatchPattern.TryBuild` run against `Bindings` a match produced — not a string comparison, and nothing
about a `switch` arm's syntax gives a generator a `MatchPattern` to build with. Measured directly:
adding `reversed:` wiring to `AsAddressable()` alone, then asking every set in the live registry
whether it has one reversible rule, found **zero** — including `CollapseMultipleFractions`, whose
internal `MatchedRuleSet` provably has one (`ReversibleRuleTest.AReversedRuleSaysWhatAnExpressionCameFrom`
uses `MatchedRules.SharedFactor` directly and it works) — because the public `RewriteRule` for that
set is still generator output, never touched by the change at all.

## Why the two options that look easiest do not work

**Extend `RuleRegistryGenerator` to compute `Reversed` from syntax**, matching how it already
computes `Growth`. This is the wrong shape of problem for a generator: `Growth` is a comparison
between two pieces of text, decidable without knowing what either side means. Reversibility needs
to construct the actual reversed rule — swap the sides, check every hole is still bound, check both
are buildable — which means either re-embedding `MatchPattern`'s construction logic a second time at
the syntax level (two implementations of the same check, guaranteed to drift exactly the way
`RuleRegistryGenerator`'s whole design was chosen to avoid — see the "generated from the `switch`"
remark on `RewriteRule` itself) or having the generator emit code that builds a `MatchPattern` from
the arm's syntax at compile time, which is most of writing the rule as data in the first place.

**Point every converted set's `Rules` at `MatchedRuleSet.AsAddressable()` instead of the generator**,
since the set already runs on the matcher and the two are proven to agree
(`RuleSetTerminationTest`-adjacent agreement tests, per set, before an exchange ships). This is the
architecturally right fix, and it is not small: `AddressableRulesTest.cs`'s own
`Addressable()` fixture pairs every converted set with the `switch` method its arms are checked
against, and a set of tests — `PatternSource` text, `NodeTypes`, rule `Name` including the `#2`/`#3`
suffixing for a pattern written twice — are specified against **generator output**, not
`MatchPattern.ToString()` output. The two renderings are not obliged to agree today (nothing has
ever compared them, because nothing has ever needed to), and finding out where they differ, for
every converted set, is the actual size of this option — not a redirect, a re-verification of
addressability itself for 29 sets.

## What is true regardless of which option is picked

- **The reversibility mechanism itself is sound and already tested.** `MatchedRule.Reversal` /
  `.Reversed` and `ReversibleRuleTest`'s corpus-checked round trips are not in question; this
  document is about the last mile from that mechanism to the public, addressable `RewriteRule` most
  of the registry actually uses.
- **`RationalizeDenominator` is not a useful example to build the first version against.** It is
  the one set that already flows through `AsAddressable()`, which makes it tempting, but both its
  rules are code-built (`RuleReversal.ReplacementIsCode`) — the set was written that way specifically
  *because* the registry declined its shape as a `switch`. A first cut proven against it would prove
  nothing, since it would show `IsReversible == false` whether the wiring works or not.
- **Speculative code without a consumer is not an asset** — this document's own opening quote is
  from `ReversibleRules.md`, which took the same position about reversibility itself before this was
  written. Wiring `Reversed` onto `RewriteRule` for the one set where it changes nothing is exactly
  that: infrastructure that cannot be exercised by the registry as it stands, which is why the first
  attempt was reverted rather than shipped.

## What is not decided here

Which of the two real options — generator-level reversibility, or redirecting `Rules` to
`AsAddressable()` for converted sets — is worth its cost, and whether either is worth doing before
tier 2's other named gap, a real e-matcher over `MatchPattern` (matching e-classes without
materialising terms, which is what `Transformation.EqualitySaturation` still does not do — see its
own doc comment). Both are estimation-sized questions this file does not answer; what it answers is
that "expose `Reversed` the way `Growth` is exposed" is not the small step it looks like from the
outside, and the next person reaching for it should start from the two options above rather than
the one that was tried and measured not to work.
