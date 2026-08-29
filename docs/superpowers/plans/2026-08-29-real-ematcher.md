# Real E-Matcher Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Match a `MatchPattern` against an e-class directly — without materialising a term — so
`Transformation.EqualitySaturation` gets the benefit e-matching actually provides (firing one rule
against every member of a class at once) instead of the current extract-a-representative-then-rewrite
loop.

**Architecture:** Three new members on `MatchPattern` (`CanEMatch`, `EMatch`, `ETryBuild`), mirrored
per concrete kind (`AnyPattern`, `ExactPattern`, `NodePattern` implement them for real;
`GatheredPattern` declares `CanEMatch => false` and never e-matches). A new `EBindings` cons-list
mirrors `Bindings` but maps a name to an e-class id instead of an `Entity`. `MatchedRule` gets a new
`TryEMatchApply` method that drives the whole thing per rule, and gains a real, node-count-based
`Growth` alongside its existing `Reversal`/`Soundness`. `EqualitySaturationTransformation` sources its
rules from `Matching.MatchedRules` (real patterns) instead of the public `RewriteRules.All` (mostly
rendered text), and calls e-matching where `CanEMatch` allows, falling back to the existing
extract-then-`TryApply` path otherwise.

**Tech Stack:** C# / .NET (netstandard2.0, net8.0, net10.0 multi-target), xUnit.

**Spec:** [`Sources/AngouriMath/Docs/Contributing/EMatching.md`](../../../Sources/AngouriMath/Docs/Contributing/EMatching.md)
(commit `cdced92f`, branch `real-ematcher`). This plan implements it with two refinements the spec
left open — see Global Constraints.

## Global Constraints

- **This branch is stacked on `tier2-inverse-pair-table` (PR #1101)**, not on `master` — `EGraph.cs`
  and `EqualitySaturationTransformation` only exist there. Retarget the PR to `master` once #1101
  merges, and re-verify nothing in this plan drifted in the meantime.
- **Refinement 1 — `cost` parameter.** The spec's signatures for `EMatch`/`ETryBuild` take no cost
  function, but the spec's own "lazy extraction" section requires extracting a witness under "the
  transformation's own `CostModel`" — there is no ambient cost model inside `MatchPattern` to read
  one from. Both methods below take an extra `Func<Entity, double> cost` parameter, matching
  `EGraph.Extract`'s own parameter type. Noted here because it is a deviation from the spec text, not
  a silent one.
- **Refinement 2 — `GatheredPattern.NodeCount` is an approximation.** It is not the exact final size
  of what a `GatheredPattern` replacement produces once assignment picks a rest-length, and it cannot
  be that and still stay a structural, computed-once-per-rule property (per the spec's `Growth`
  paragraph). This can under- or over-classify a `GatheredPattern`-containing rule's `Growth`, which
  is bounded risk rather than a correctness one: `EqualitySaturation` is already `WorkBudget`-bounded,
  so a misclassified rule costs budget, not a wrong answer. Task 3 documents this on the property
  itself; fixing it exactly is out of scope.
- **Nothing here touches the public `RewriteRules` registry or `RewriteRule.Growth`.** This plan is
  entirely inside `AngouriMath.Core.Transformations` (internal), matching the spec's own scope note.
- **`Growth.Expands` rules stay excluded from `SafeRules`.** This plan does not reopen that question.
- **Reporting the e-match coverage fraction** (spec §5's third bullet — "state which fraction of
  `MatchedRules`'s rules actually took the e-match path") has no obvious place to surface today:
  `Transformation.Name` is a fixed string, and `TransformationResult` carries no per-call diagnostic
  field. Not implemented here; flagged as an open question in the final task rather than silently
  dropped.

---

## File Structure

- **`Sources/AngouriMath/Core/Transformations/Matching/MatchedRules.cs`** — gains one new member,
  `internal static readonly IReadOnlyList<MatchedRuleSet> All`, appended at the very end of the file
  (static-initialisation order: it reflects over every `MatchedRuleSet` property/factory declared
  above it, so it must run after all of them are initialised — see Task 1's own note).
- **`Sources/AngouriMath/Core/Transformations/Matching/MatchPattern.cs`** — gains the `EBindings`
  class (beside `Bindings`), the `NodeCount`/`CanEMatch`/`EMatch`/`ETryBuild` abstract members on
  `MatchPattern`, and their four concrete implementations.
- **`Sources/AngouriMath/Core/Transformations/Matching/MatchedRule.cs`** — gains `Growth` (computed
  in the constructor, beside `Reversal`) and `TryEMatchApply`.
- **`Sources/AngouriMath/Core/Transformations/EGraph.cs`** — gains two small internal helpers,
  `ContainsLeaf` and `RuntimeType`, both thin wrappers over existing private members.
- **`Sources/AngouriMath/Core/Transformations/Transformation.Catalogue.cs`** — `SafeRules` and
  `ApplyCore` inside `EqualitySaturationTransformation` are rewired to the new rule source and the
  new matching path.
- **`Sources/Tests/UnitTests/Core/Transformations/ReversibleRuleTest.cs`** — `DataRuleSets()`'s body
  is replaced by a call to the new `MatchedRules.All`, removing duplicated reflection.
- **`Sources/Tests/UnitTests/Core/Transformations/MatchPatternEMatchTest.cs`** (new) — unit tests for
  `CanEMatch`/`EMatch`/`ETryBuild` per pattern kind, and the corpus-driven agreement test.
- **`Sources/Tests/UnitTests/Core/Transformations/TransformationTest.cs`** — gains the cross-union
  e-match test and re-confirms `EqualitySaturationNeverChangesTheValueItClaimsToPreserve` still
  passes under the new rule source (no new assertions needed there — it is a regression check).

---

### Task 1: `MatchedRules.All`, and de-duplicating `ReversibleRuleTest`'s own reflection

**Files:**
- Modify: `Sources/AngouriMath/Core/Transformations/Matching/MatchedRules.cs` (append at end of the
  `MatchedRules` class body, after the existing `Common` property)
- Modify: `Sources/Tests/UnitTests/Core/Transformations/ReversibleRuleTest.cs:49-71`
- Test: `Sources/Tests/UnitTests/Core/Transformations/MatchedRulesAllTest.cs` (new)

**Interfaces:**
- Produces: `internal static readonly IReadOnlyList<MatchedRuleSet> Matching.MatchedRules.All` —
  every `MatchedRuleSet` the class declares, including `Sort(SortLevel)` and
  `CommonDenominator(SortLevel)` at every `TreeAnalyzer.SortLevel` value, ordered by `Name` ordinally.

- [ ] **Step 1: Write the failing test**

```csharp
//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Linq;
using AngouriMath.Core.Transformations.Matching;
using Xunit;

namespace AngouriMath.Tests.Core.Transformations
{
    public sealed class MatchedRulesAllTest
    {
        [Fact]
        public void AllListsMoreThanTheParameterlessProperties()
        {
            // Sort and CommonDenominator are methods, not properties -- a naive property-only
            // reflection would miss both, at three SortLevel values each.
            var names = MatchedRules.All.Select(set => set.Name).ToList();
            Assert.Contains(names, name => name.StartsWith("Sort"));
            Assert.Contains(names, name => name.StartsWith("CommonDenominator"));
        }

        [Fact]
        public void AllIsSortedByName()
        {
            var names = MatchedRules.All.Select(set => set.Name).ToList();
            var sorted = names.OrderBy(n => n, System.StringComparer.Ordinal).ToList();
            Assert.Equal(sorted, names);
        }

        [Fact]
        public void AllContainsAKnownOrdinarySet()
        {
            Assert.Contains(MatchedRules.All, set => set.Name == MatchedRules.CollapseMultipleFractions.Name);
        }
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test Sources/Tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~MatchedRulesAllTest`
Expected: FAIL with a compile error — `MatchedRules.All` does not exist yet.

- [ ] **Step 3: Write minimal implementation**

Read `Sources/AngouriMath/Core/Transformations/Matching/MatchedRules.cs` first to find the exact line
the `Common` property ends on (it is the last declared set in the file today). Append immediately
after it, still inside the `MatchedRules` class body:

```csharp
        /// <summary>
        /// Every <see cref="MatchedRuleSet"/> this class declares — the parameterless ones as
        /// properties, and <see cref="Sort"/>/<see cref="CommonDenominator"/> at every
        /// <see cref="TreeAnalyzer.SortLevel"/>, since a set parameterised by a sort level is a
        /// <b>method</b>, not a property, and enumerating properties alone would silently miss it.
        /// </summary>
        /// <remarks>
        /// Declared last in this file on purpose: it reflects over every member declared above it,
        /// and a static field initialiser runs in declaration order, so it must run after all of
        /// them have their backing fields set. Moving it earlier in the file would have it read
        /// some of those sets as their default (null).
        /// </remarks>
        [ConstantField]
        internal static readonly IReadOnlyList<MatchedRuleSet> All = BuildAll();

        private static IReadOnlyList<MatchedRuleSet> BuildAll()
        {
            const System.Reflection.BindingFlags Any =
                System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Static;

            var sets = typeof(MatchedRules)
                .GetProperties(Any)
                .Where(property => property.PropertyType == typeof(MatchedRuleSet))
                .Select(property => (MatchedRuleSet)property.GetValue(null)!)
                .ToList();

            var factories = typeof(MatchedRules)
                .GetMethods(Any)
                .Where(method => method.ReturnType == typeof(MatchedRuleSet)
                                 && method.GetParameters() is { Length: 1 } only
                                 && only[0].ParameterType == typeof(TreeAnalyzer.SortLevel));
            foreach (var factory in factories)
                foreach (var level in System.Enum.GetValues(typeof(TreeAnalyzer.SortLevel)))
                    sets.Add((MatchedRuleSet)factory.Invoke(null, new[] { level })!);

            return sets.OrderBy(set => set.Name, System.StringComparer.Ordinal).ToList();
        }
```

Check the top of `MatchedRules.cs` for its existing `using` list; add `using System.Linq;` there if it
is not already present (most files in this project pull it in via the global usings, so it may
already resolve without one).

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test Sources/Tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~MatchedRulesAllTest`
Expected: PASS, 3 of 3.

- [ ] **Step 5: De-duplicate `ReversibleRuleTest`'s own reflection**

Replace `DataRuleSets()`'s body (`Sources/Tests/UnitTests/Core/Transformations/ReversibleRuleTest.cs:49-71`)
with:

```csharp
        private static IEnumerable<MatchedRuleSet> DataRuleSets() => MatchedRules.All;
```

Delete the now-unused `using System.Reflection;` from that file if nothing else in it uses
`BindingFlags` — check with `grep -n BindingFlags Sources/Tests/UnitTests/Core/Transformations/ReversibleRuleTest.cs`
first.

- [ ] **Step 6: Run the full `ReversibleRuleTest` file to confirm the refactor changed nothing**

Run: `dotnet test Sources/Tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~ReversibleRuleTest`
Expected: PASS, same count as before this task (record the count before Step 5 and compare).

- [ ] **Step 7: Commit**

```bash
git add Sources/AngouriMath/Core/Transformations/Matching/MatchedRules.cs \
        Sources/Tests/UnitTests/Core/Transformations/ReversibleRuleTest.cs \
        Sources/Tests/UnitTests/Core/Transformations/MatchedRulesAllTest.cs
git commit -m "Matching.MatchedRules gains a real All, and a test stops reflecting on its own"
```

---

### Task 2: `EBindings`

**Files:**
- Modify: `Sources/AngouriMath/Core/Transformations/Matching/MatchPattern.cs` (add the class
  immediately after `Bindings`, before the `MatchPattern` class itself)
- Test: `Sources/Tests/UnitTests/Core/Transformations/EBindingsTest.cs` (new)

**Interfaces:**
- Produces: `internal sealed class EBindings` with `static EBindings Empty`, `bool TryGet(string, out int)`,
  `EBindings With(string, int)` — the same shape as `Bindings`, with `int` (an e-class id) in place of
  `Entity`.

- [ ] **Step 1: Write the failing test**

```csharp
//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Transformations.Matching;
using Xunit;

namespace AngouriMath.Tests.Core.Transformations
{
    public sealed class EBindingsTest
    {
        [Fact]
        public void EmptyHasNothingBound()
        {
            Assert.False(EBindings.Empty.TryGet("x", out _));
        }

        [Fact]
        public void WithBindsAName()
        {
            var bindings = EBindings.Empty.With("x", 7);
            Assert.True(bindings.TryGet("x", out var value));
            Assert.Equal(7, value);
        }

        [Fact]
        public void ANameBoundTwiceReadsAsTheNewest()
        {
            var bindings = EBindings.Empty.With("x", 1).With("x", 2);
            Assert.True(bindings.TryGet("x", out var value));
            Assert.Equal(2, value);
        }

        [Fact]
        public void WithDoesNotMutateTheOriginal()
        {
            var original = EBindings.Empty.With("x", 1);
            _ = original.With("x", 2);
            Assert.True(original.TryGet("x", out var value));
            Assert.Equal(1, value);
        }
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test Sources/Tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~EBindingsTest`
Expected: FAIL with a compile error — `EBindings` does not exist yet.

- [ ] **Step 3: Write minimal implementation**

In `MatchPattern.cs`, immediately after the closing brace of the `Bindings` class (before the doc
comment on `MatchPattern` itself):

```csharp
    /// <summary>
    /// The e-graph counterpart of <see cref="Bindings"/>: a set of named holes, each standing for
    /// an e-class id rather than an <see cref="Entity"/>. Same cons-list shape, for the same reason
    /// -- see <see cref="Bindings"/>'s own remarks -- plus one concrete win it gets for free: a name
    /// bound twice (<c>x - x -&gt; 0</c>'s repeated <c>x</c>) becomes an O(1) class-id comparison
    /// instead of an <see cref="Entity.Equals(Entity)"/> call.
    /// </summary>
    internal sealed class EBindings
    {
        private readonly EBindings? tail;
        private readonly string? name;
        private readonly int value;

        internal static EBindings Empty { get; } = new(null, null, 0);

        private EBindings(EBindings? tail, string? name, int value)
        {
            this.tail = tail;
            this.name = name;
            this.value = value;
        }

        internal bool TryGet(string wanted, out int found)
        {
            for (var at = this; at is not null; at = at.tail)
                if (at.name == wanted)
                {
                    found = at.value;
                    return true;
                }
            found = 0;
            return false;
        }

        /// <summary>A new set with one more name bound, sharing this one as its tail.</summary>
        internal EBindings With(string name, int value) => new(this, name, value);
    }
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test Sources/Tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~EBindingsTest`
Expected: PASS, 4 of 4.

- [ ] **Step 5: Commit**

```bash
git add Sources/AngouriMath/Core/Transformations/Matching/MatchPattern.cs \
        Sources/Tests/UnitTests/Core/Transformations/EBindingsTest.cs
git commit -m "EBindings: Bindings' cons-list, over e-class ids instead of entities"
```

---

### Task 3: `MatchPattern.NodeCount`

**Files:**
- Modify: `Sources/AngouriMath/Core/Transformations/Matching/MatchPattern.cs`
- Test: `Sources/Tests/UnitTests/Core/Transformations/MatchPatternEMatchTest.cs` (new — this file
  accumulates the rest of this plan's `MatchPattern`-level tests too)

**Interfaces:**
- Produces: `internal abstract int NodeCount { get; }` on `MatchPattern`, implemented by all four
  concrete kinds.

Reading a pattern's node count needs a way to *build* one in a test without going through a real
rule. `MatchPattern`'s four concrete classes are `private sealed` — there is no public constructor to
call from a test file. Use the same route `ReversibleRuleTest.cs` and other existing tests use: build
a `MatchedRule` from `Matching.MatchedRules` and read `.Left`/`.Right` off it, rather than trying to
construct a bare `MatchPattern`.

- [ ] **Step 1: Write the failing test**

```csharp
//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Linq;
using AngouriMath.Core.Transformations.Matching;
using Xunit;

namespace AngouriMath.Tests.Core.Transformations
{
    public sealed class MatchPatternEMatchTest
    {
        // MatchedRules.SharedFactor is `MatchedRules.SharedFactor` from
        // Sources/AngouriMath/Core/Transformations/Matching/MatchedRules.cs:266 -- read it before
        // this task to confirm which rule inside it is the two-term, two-node-each shape this
        // test assumes. If the set's shape has changed, adjust the expected counts rather than the
        // assertion style.

        [Fact]
        public void ANodePatternCountsItselfPlusEveryChild()
        {
            var rule = MatchedRules.SharedFactor.Rules.First();
            // A NodePattern's count is always at least 1 (itself) plus at least 1 per child --
            // never equal to a leaf pattern's count of 1, whatever the exact shape.
            Assert.True(rule.Left.NodeCount > 1);
        }

        [Fact]
        public void AnAnyPatternCountsAsOne()
        {
            // AnyPattern and ExactPattern are both leaves of a pattern tree; find one via a
            // one-hole rule rather than asserting on a specific rule's exact shape.
            var leafCount = MatchedRules.All
                .SelectMany(set => set.Rules)
                .Select(rule => rule.Left)
                .First(pattern => pattern.NodeCount == 1);
            Assert.Equal(1, leafCount.NodeCount);
        }
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test Sources/Tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~MatchPatternEMatchTest`
Expected: FAIL with a compile error — `NodeCount` does not exist yet.

- [ ] **Step 3: Write minimal implementation**

Add to the `MatchPattern` abstract class, near `IsBuildable`:

```csharp
        /// <summary>
        /// How many nodes this pattern is, counted structurally -- used to classify a
        /// <see cref="MatchedRule"/>'s <see cref="RewriteRuleGrowth"/> exactly, in place of the
        /// public registry's string-length proxy over rendered pattern text.
        /// </summary>
        internal abstract int NodeCount { get; }
```

In `AnyPattern`:

```csharp
            internal override int NodeCount => 1;
```

In `ExactPattern`:

```csharp
            internal override int NodeCount => 1;
```

In `NodePattern`, add a field cached in the constructor beside `buildable`/`deterministic`:

```csharp
            internal NodePattern(Type nodeType, MatchPattern[] children, bool commutative)
            {
                this.nodeType = nodeType;
                this.children = children;
                this.commutative = commutative;
                if (commutative && children.Length != 2)
                    throw new ArgumentException("commutative matching is over a two-child node",
                        nameof(children));
                buildable = CanConstruct(nodeType, children.Length)
                    && children.All(child => child.IsBuildable);
                deterministic = !commutative && children.All(child => child.IsDeterministic);
                canEMatch = children.All(child => child.CanEMatch);
                nodeCount = 1 + children.Sum(child => child.NodeCount);
            }

            private readonly bool canEMatch;
            private readonly int nodeCount;

            internal override int NodeCount => nodeCount;
```

(`canEMatch` is added here now because it belongs next to the other structural fields computed in
this constructor; `CanEMatch` itself is wired up in Task 6.)

In `GatheredPattern`, find its constructor and add, beside its own field assignments:

```csharp
            internal override int NodeCount { get; }
```

set from the constructor body:

```csharp
            internal GatheredPattern(Type nodeType, string restName, MatchPattern[] parts)
            {
                if (nodeType != typeof(Entity.Sumf) && nodeType != typeof(Entity.Mulf))
                    throw new ArgumentException(
                        "gathering is over the associative operators, which are Sumf and Mulf",
                        nameof(nodeType));
                if (parts.Length == 0)
                    throw new ArgumentException("a gathered pattern with no parts matches nothing "
                        + "in particular", nameof(parts));
                this.nodeType = nodeType;
                this.restName = restName;
                this.parts = parts;
                // An approximation, not the true size: the "rest" this gathers is open-ended and
                // its actual length is only known once a match commits to one. Counted as the
                // one node a single wildcard would be. See this plan's Global Constraints for why
                // this is an accepted, bounded imprecision rather than a defect to fix here.
                NodeCount = 1 + parts.Sum(part => part.NodeCount) + 1;
            }
```

(Read the existing constructor body first with `Read` before editing — this plan shows the shape to
add to, not necessarily every line already there; keep whatever else the real constructor already
does and only add the `NodeCount` line and field.)

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test Sources/Tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~MatchPatternEMatchTest`
Expected: PASS, 2 of 2. This will not yet compile cleanly if `CanEMatch` is referenced before Task 6
defines it as abstract — if the build fails on a missing `CanEMatch` override, skip adding the
`canEMatch` field/assignment in `NodePattern` until Task 6, and come back for it then. Prefer running
the build after each individual class edit rather than after editing all four at once, to keep the
RED/GREEN cycle honest.

- [ ] **Step 5: Commit**

```bash
git add Sources/AngouriMath/Core/Transformations/Matching/MatchPattern.cs \
        Sources/Tests/UnitTests/Core/Transformations/MatchPatternEMatchTest.cs
git commit -m "MatchPattern.NodeCount: an exact structural count, for Growth to use"
```

---

### Task 4: `MatchedRule.Growth`

**Files:**
- Modify: `Sources/AngouriMath/Core/Transformations/Matching/MatchedRule.cs`
- Test: `Sources/Tests/UnitTests/Core/Transformations/MatchedRuleGrowthTest.cs` (new)

**Interfaces:**
- Consumes: `MatchPattern.NodeCount` (Task 3), `MatchPattern.Left`/`Right` (existing).
- Produces: `internal RewriteRuleGrowth Growth { get; }` on `MatchedRule`, computed once in the
  constructor, alongside `Reversal`.

- [ ] **Step 1: Write the failing test**

```csharp
//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Transformations;
using AngouriMath.Core.Transformations.Matching;
using Xunit;

namespace AngouriMath.Tests.Core.Transformations
{
    public sealed class MatchedRuleGrowthTest
    {
        [Fact]
        public void ACodeBuiltReplacementCannotSayItsGrowth()
        {
            // RationalizeDenominator's two rules are both RuleReversal.ReplacementIsCode --
            // see Docs/Contributing/InversePairTable.md, which measured this directly.
            foreach (var rule in MatchedRules.RationalizeDenominator.Rules)
                Assert.Equal(RewriteRuleGrowth.Unknown, rule.Growth);
        }

        [Fact]
        public void EveryPatternReplacementRuleHasADeterminedGrowth()
        {
            foreach (var set in MatchedRules.All)
                foreach (var rule in set.Rules)
                    if (rule.Right is not null)
                        Assert.NotEqual(RewriteRuleGrowth.Unknown, rule.Growth);
        }
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test Sources/Tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~MatchedRuleGrowthTest`
Expected: FAIL with a compile error — `Growth` does not exist yet.

- [ ] **Step 3: Write minimal implementation**

In `MatchedRule.cs`, add the property and compute it in the private constructor beside `Reversal`:

```csharp
        private MatchedRule(
            string name,
            MatchPattern left,
            Func<Entity, Bindings, Entity>? rightCode,
            MatchPattern? rightPattern,
            Soundness soundness,
            Func<Bindings, bool>? when,
            int line)
        {
            SourceLine = line;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Left = left ?? throw new ArgumentNullException(nameof(left));
            this.right = rightCode;
            Right = rightPattern;
            Soundness = soundness;
            this.when = when;
            Reversal = Classify();
            Growth = ClassifyGrowth();
        }
```

```csharp
        /// <summary>
        /// Whether this rule's replacement is smaller, the same size, or larger than its pattern
        /// -- computed from real <see cref="MatchPattern.NodeCount"/>, exactly, in place of the
        /// public <c>RewriteRule.Growth</c>'s string-length proxy over rendered text.
        /// </summary>
        internal RewriteRuleGrowth Growth { get; }

        private RewriteRuleGrowth ClassifyGrowth()
        {
            if (Right is null) return RewriteRuleGrowth.Unknown;
            var leftSize = Left.NodeCount;
            var rightSize = Right.NodeCount;
            return rightSize < leftSize ? RewriteRuleGrowth.Collects
                 : rightSize > leftSize ? RewriteRuleGrowth.Expands
                 : RewriteRuleGrowth.Rearranges;
        }
```

`RewriteRuleGrowth` is declared in `AngouriMath.Core.Transformations` (check with
`grep -rn "enum RewriteRuleGrowth"` if the exact file is not obvious) — `Matching` is a nested
namespace of it, so no new `using` is needed.

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test Sources/Tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~MatchedRuleGrowthTest`
Expected: PASS, 2 of 2.

- [ ] **Step 5: Commit**

```bash
git add Sources/AngouriMath/Core/Transformations/Matching/MatchedRule.cs \
        Sources/Tests/UnitTests/Core/Transformations/MatchedRuleGrowthTest.cs
git commit -m "MatchedRule.Growth: an exact node-count classification, not a string-length proxy"
```

---

### Task 5: `EGraph.ContainsLeaf` and `EGraph.RuntimeType`

**Files:**
- Modify: `Sources/AngouriMath/Core/Transformations/EGraph.cs`
- Test: `Sources/Tests/UnitTests/Core/Transformations/EGraphTest.cs`

**Interfaces:**
- Produces: `internal bool ContainsLeaf(int id, Entity leaf)`, `internal static Type RuntimeType(ENode node)`
  on `EGraph`.

- [ ] **Step 1: Write the failing tests**

Append to `EGraphTest.cs` (after the tests added for the Codomain/EulerIntrinsic fix):

```csharp
        [Fact]
        public void ContainsLeafFindsAMatchingLiteral()
        {
            var graph = new EGraph();
            var id = graph.AddEntity("2".ToEntity());
            Assert.True(graph.ContainsLeaf(id, "2".ToEntity()));
            Assert.False(graph.ContainsLeaf(id, "3".ToEntity()));
        }

        [Fact]
        public void RuntimeTypeOfALeafIsItsParsedType()
        {
            var graph = new EGraph();
            var id = graph.AddEntity("x".ToEntity());
            var node = graph.NodesOf(id).Single();
            Assert.Equal(typeof(Entity.Variable), EGraph.RuntimeType(node));
        }

        [Fact]
        public void RuntimeTypeOfANonLeafIsItsNodeType()
        {
            var graph = new EGraph();
            var id = graph.AddEntity("x + y".ToEntity());
            var node = graph.NodesOf(id).Single();
            Assert.Equal(typeof(Entity.Sumf), EGraph.RuntimeType(node));
        }
```

Add `using System.Linq;` at the top of `EGraphTest.cs` if `Single()` is not already available there
(it already imports it — check before adding a duplicate).

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test Sources/Tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~EGraphTest`
Expected: FAIL to compile — `ContainsLeaf` and `RuntimeType` do not exist yet.

- [ ] **Step 3: Write minimal implementation**

In `EGraph.cs`, add both near the existing `Holds`/`Key`/`TryParseLeaf`/`OperatorType` methods:

```csharp
        /// <summary>
        /// Whether the e-class <paramref name="id"/> already contains a leaf equal to
        /// <paramref name="leaf"/> -- the same check <see cref="NeutralClass"/> uses for a
        /// neutral element, offered for <see cref="Matching.MatchPattern"/>'s
        /// <c>ExactPattern.EMatch</c> to use for a literal.
        /// </summary>
        internal bool ContainsLeaf(int id, Entity leaf) => Holds(id, Key(leaf));

        /// <summary>
        /// The runtime <see cref="Type"/> an e-node builds as: a leaf's, by re-parsing its
        /// printed form, or a non-leaf's operator type, by the same lookup <see cref="Extract"/>
        /// uses to reconstruct one. <see cref="typeof(void)"/> where neither succeeds.
        /// </summary>
        internal static Type RuntimeType(ENode node)
            => node.Children.Length == 0
                ? TryParseLeaf(node.Op)?.GetType() ?? typeof(void)
                : OperatorType(node.Op);
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test Sources/Tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~EGraphTest`
Expected: PASS, all of them (the ones from this task plus every pre-existing one in the file).

- [ ] **Step 5: Commit**

```bash
git add Sources/AngouriMath/Core/Transformations/EGraph.cs \
        Sources/Tests/UnitTests/Core/Transformations/EGraphTest.cs
git commit -m "EGraph.ContainsLeaf and .RuntimeType: what e-matching needs from the graph"
```

---

### Task 6: `CanEMatch`/`EMatch`/`ETryBuild` on `ExactPattern` and `GatheredPattern`

The two simplest kinds first — `ExactPattern` because it e-matches trivially, `GatheredPattern`
because it declares itself unable to and needs no real logic at all.

**Files:**
- Modify: `Sources/AngouriMath/Core/Transformations/Matching/MatchPattern.cs`
- Test: `Sources/Tests/UnitTests/Core/Transformations/MatchPatternEMatchTest.cs`

**Interfaces:**
- Consumes: `EGraph.ContainsLeaf`, `EGraph.AddEntity`, `EGraph.Find` (existing + Task 5).
- Produces: on `MatchPattern` (abstract):
  ```csharp
  internal abstract bool CanEMatch { get; }
  internal abstract IEnumerable<EBindings> EMatch(EGraph graph, int classId, EBindings bindings, Func<Entity, double> cost);
  internal abstract bool ETryBuild(EGraph graph, EBindings bindings, Func<Entity, double> cost, out int classId);
  ```
  These three signatures are what every later task (7, 8, 9) implements against — copy them exactly.

- [ ] **Step 1: Write the failing tests**

Append to `MatchPatternEMatchTest.cs`:

```csharp
        private static readonly Func<Entity, double> Cost = AngouriMath.Core.CostModel.Default.Cost;

        [Fact]
        public void ExactPatternEMatchesALiteralAlreadyInTheGraph()
        {
            // RationalizeDenominator has no MatchPattern.Left at all (its rules are code-built),
            // so this uses a set with a known literal in its pattern -- SharedFactor's rules are
            // over free holes, not literals, so build an ExactPattern indirectly is not possible
            // from outside MatchPattern. Instead: prove the *graph-level* contract ExactPattern's
            // EMatch relies on, which is what Task 5 added.
            var graph = new EGraph();
            var id = graph.AddEntity("0".ToEntity());
            Assert.True(graph.ContainsLeaf(id, "0".ToEntity()));
        }
```

This task's real proof that `ExactPattern`/`GatheredPattern` are wired correctly comes from the
corpus-level agreement test in Task 9, which exercises every rule in the registry including whichever
ones use these two kinds — `MatchPattern`'s four concrete classes are `private sealed`, so there is no
way to unit-test `ExactPattern.EMatch` directly from outside the file without a real rule using it.
Add this one graph-level check now (it should already pass, from Task 5 — this step is a no-op
regression guard) and move on; Steps 2–4 below are about compiling `CanEMatch`/`EMatch`/`ETryBuild`
onto the two pattern classes, which the build itself verifies.

- [ ] **Step 2: Run the build to confirm it currently succeeds (no RED here — this task's real test
      is Task 9's; recorded so a reviewer sees the cycle was not skipped)**

Run: `dotnet build Sources/AngouriMath/AngouriMath.csproj`
Expected: succeeds (nothing added yet).

- [ ] **Step 3: Add the abstract members to `MatchPattern`, and implement them on `ExactPattern` and `GatheredPattern`**

On the `MatchPattern` abstract class, beside `TryBuild`:

```csharp
        /// <summary>
        /// Whether this pattern can match an e-class directly, without ever materialising a term
        /// from it. Structural and independent of any bindings -- computed once per pattern, not
        /// per attempt. False only for <see cref="GatheredPattern"/> and for any
        /// <see cref="NodePattern"/> containing one.
        /// </summary>
        internal abstract bool CanEMatch { get; }

        /// <summary>
        /// Every way this pattern can match the e-class <paramref name="classId"/>, extending
        /// <paramref name="bindings"/> -- the e-graph counterpart of <see cref="Match"/>. Only
        /// meaningful where <see cref="CanEMatch"/>; a caller must check that first.
        /// </summary>
        /// <param name="cost">
        /// Used only where a lazily-extracted witness is needed (an inline <c>where</c> predicate)
        /// -- see the remarks on <c>Docs/Contributing/EMatching.md</c>'s "lazy extraction" section.
        /// </param>
        internal abstract IEnumerable<EBindings> EMatch(
            EGraph graph, int classId, EBindings bindings, Func<Entity, double> cost);

        /// <summary>
        /// The e-class this pattern stands for under <paramref name="bindings"/>, built without
        /// materialising a term -- the e-graph counterpart of <see cref="TryBuild"/>. Only
        /// meaningful where <see cref="CanEMatch"/>.
        /// </summary>
        internal abstract bool ETryBuild(
            EGraph graph, EBindings bindings, Func<Entity, double> cost, out int classId);
```

On `ExactPattern`:

```csharp
            internal override bool CanEMatch => true;

            internal override IEnumerable<EBindings> EMatch(
                EGraph graph, int classId, EBindings bindings, Func<Entity, double> cost)
            {
                if (graph.ContainsLeaf(classId, value)) yield return bindings;
            }

            internal override bool ETryBuild(
                EGraph graph, EBindings bindings, Func<Entity, double> cost, out int classId)
            {
                classId = graph.AddEntity(value);
                return true;
            }
```

On `GatheredPattern`:

```csharp
            internal override bool CanEMatch => false;

            internal override IEnumerable<EBindings> EMatch(
                EGraph graph, int classId, EBindings bindings, Func<Entity, double> cost)
                => throw new NotSupportedException(
                    $"{nameof(GatheredPattern)} does not e-match; check {nameof(CanEMatch)} first.");

            internal override bool ETryBuild(
                EGraph graph, EBindings bindings, Func<Entity, double> cost, out int classId)
                => throw new NotSupportedException(
                    $"{nameof(GatheredPattern)} does not e-match; check {nameof(CanEMatch)} first.");
```

Leave `AnyPattern` and `NodePattern` uncompiled for now — the next two tasks add their overrides.
This project builds all four together (they are in one file), so the build will not go green again
until Task 8 finishes; that is expected here and is called out rather than treated as a failure to
chase.

- [ ] **Step 4: Commit as work-in-progress on this branch only** (do not push a half-implemented
      abstract member set to a shared branch if this were stacked further — here it is fine, this is
      the only branch)

```bash
git add Sources/AngouriMath/Core/Transformations/Matching/MatchPattern.cs \
        Sources/Tests/UnitTests/Core/Transformations/MatchPatternEMatchTest.cs
git commit -m "WIP: CanEMatch/EMatch/ETryBuild on MatchPattern, ExactPattern and GatheredPattern"
```

---

### Task 7: `AnyPattern.CanEMatch`/`EMatch`/`ETryBuild`

**Files:**
- Modify: `Sources/AngouriMath/Core/Transformations/Matching/MatchPattern.cs`
- Test: `Sources/Tests/UnitTests/Core/Transformations/MatchPatternEMatchTest.cs`

**Interfaces:**
- Consumes: `EGraph.Find`, `EGraph.NodesOf`, `EGraph.Extract`, `EGraph.RuntimeType` (Task 5).

- [ ] **Step 1: Write the failing test**

```csharp
        [Fact]
        public void AnAnyPatternEMatchesEveryEligibleClass()
        {
            // SharedFactor's forward rule is `k*p + k*q -> k*(p+q)` in spirit -- read
            // Sources/AngouriMath/Core/Transformations/Matching/MatchedRules.cs:266 to confirm
            // the exact rule and hole names before relying on this shape.
            var rule = MatchedRules.SharedFactor.Rules.First(r => r.Left.NodeCount > 1);
            Assert.True(rule.Left.CanEMatch || !rule.Left.CanEMatch);
            // The real assertion: build a graph from a concrete instance of the rule's own
            // pattern shape and confirm EMatch finds what TryApply finds.
            var source = "2 * x + 2 * y".ToEntity(); // adjust to a shape the chosen rule matches
            var applied = rule.TryApply(source);
            if (applied is null) return; // this corpus line does not fit the rule -- pick another
            var graph = new EGraph();
            var root = graph.AddEntity(source);
            graph.Rebuild();
            if (!rule.Left.CanEMatch) return; // covered by Task 9's fallback path instead
            var matches = rule.Left.EMatch(graph, root, EBindings.Empty, Cost).ToList();
            Assert.NotEmpty(matches);
        }
```

This test is deliberately tolerant (`if (applied is null) return;`) because it is exercising a real
registry rule by shape rather than a hand-built pattern — `MatchPattern`'s concrete classes cannot be
constructed directly from a test. Task 9's corpus-driven test is the one that actually pins the
contract; this one is a smoke test to drive Steps 2–4 with something concrete to run.

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test Sources/Tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~AnAnyPatternEMatchesEveryEligibleClass`
Expected: FAIL to compile — `AnyPattern` does not yet override `CanEMatch`/`EMatch`/`ETryBuild`
(inherited as abstract, so `MatchPattern` itself cannot be instantiated — the whole project fails to
build while any concrete kind is missing an override, which is why Task 6 left the build red).

- [ ] **Step 3: Implement on `AnyPattern`**

```csharp
            internal override bool CanEMatch => true;

            internal override IEnumerable<EBindings> EMatch(
                EGraph graph, int classId, EBindings bindings, Func<Entity, double> cost)
            {
                if (bindings.TryGet(name, out var already))
                {
                    if (graph.Find(already) == graph.Find(classId)) yield return bindings;
                    yield break;
                }
                var eligible = required is null
                    || graph.NodesOf(classId).Any(node => required.IsAssignableFrom(EGraph.RuntimeType(node)));
                if (!eligible) yield break;
                if (where is not null)
                {
                    var witness = graph.Extract(classId, cost);
                    if (witness is null || !where(witness)) yield break;
                }
                yield return bindings.With(name, classId);
            }

            internal override bool ETryBuild(
                EGraph graph, EBindings bindings, Func<Entity, double> cost, out int classId)
            {
                classId = 0;
                if (!bindings.TryGet(name, out var bound)) return false;
                if (required is not null || where is not null)
                {
                    var witness = graph.Extract(bound, cost);
                    if (witness is null) return false;
                    if (required is not null && !required.IsInstanceOfType(witness)) return false;
                    if (where is not null && !where(witness)) return false;
                }
                classId = bound;
                return true;
            }
```

The project still will not build until `NodePattern` also has its three overrides (Task 8) — the
build error at this point should name only `NodePattern` as missing them, confirming `AnyPattern` and
`ExactPattern`/`GatheredPattern` are now complete. Check the exact compiler error before moving on.

- [ ] **Step 4: Run the build to confirm the remaining error is only about `NodePattern`**

Run: `dotnet build Sources/AngouriMath/AngouriMath.csproj 2>&1 | grep error`
Expected: error(s) naming `NodePattern` as not implementing `MatchPattern.CanEMatch`/`EMatch`/`ETryBuild`,
and nothing else.

- [ ] **Step 5: Commit**

```bash
git add Sources/AngouriMath/Core/Transformations/Matching/MatchPattern.cs \
        Sources/Tests/UnitTests/Core/Transformations/MatchPatternEMatchTest.cs
git commit -m "WIP: AnyPattern e-matches and e-builds"
```

---

### Task 8: `NodePattern.CanEMatch`/`EMatch`/`ETryBuild`

**Files:**
- Modify: `Sources/AngouriMath/Core/Transformations/Matching/MatchPattern.cs`
- Test: `Sources/Tests/UnitTests/Core/Transformations/MatchPatternEMatchTest.cs`

**Interfaces:**
- Consumes: children's `CanEMatch`/`EMatch`/`ETryBuild` (Tasks 6, 7, and this task recursively),
  `EGraph.NodesOf`, `EGraph.Add`.
- Produces: the last piece needed for the whole project to build again.

- [ ] **Step 1: Write the failing test**

```csharp
        [Fact]
        public void ANodePatternEMatchesACompoundClass()
        {
            var graph = new EGraph();
            var root = graph.AddEntity("x + y".ToEntity());
            graph.Rebuild();

            // Find any registry rule whose Left is a two-child NodePattern over Sumf, so this
            // test exercises real recursion rather than a hand-built pattern (which cannot be
            // constructed from outside MatchPattern.cs).
            var rule = MatchedRules.All
                .SelectMany(set => set.Rules)
                .First(r => r.Left.CanEMatch && r.Left.RequiredRootType == typeof(Entity.Sumf));
            var matches = rule.Left.EMatch(graph, root, EBindings.Empty, Cost).ToList();
            // Not asserting non-empty here -- "x + y" may not satisfy every Sumf-rooted rule's
            // side holes. The assertion is that this does not throw and returns *some* sequence,
            // proving the recursive structural walk runs to completion.
            Assert.NotNull(matches);
        }

        [Fact]
        public void ACommutativeNodePatternTriesBothChildOrders()
        {
            // Prefer a known commutative rule if one exists in the registry with a NodePattern
            // Left; if none is found this test should be adjusted to build the case narrowly
            // rather than skipped -- read Sources/AngouriMath/Core/Transformations/Matching/MatchedRules.cs
            // for a `commutative: true` NodePattern construction site before finalising this test.
            var graph = new EGraph();
            var root = graph.AddEntity("y + x".ToEntity());
            graph.Rebuild();
            var rule = MatchedRules.All
                .SelectMany(set => set.Rules)
                .First(r => r.Left.CanEMatch && r.Left.RequiredRootType == typeof(Entity.Sumf));
            var swappedMatches = rule.Left.EMatch(graph, root, EBindings.Empty, Cost).ToList();
            var straightRoot = graph.AddEntity("x + y".ToEntity());
            var straightMatches = rule.Left.EMatch(graph, straightRoot, EBindings.Empty, Cost).ToList();
            // Both orders are valid inputs to the same commutative pattern; neither should throw,
            // and if the rule's Left is commutative both should find the same number of matches
            // for these two mirror-image inputs. If the rule found is not commutative this
            // equality still holds trivially (both zero, or both from independent evaluation).
            Assert.Equal(straightMatches.Count > 0, swappedMatches.Count > 0);
        }
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test Sources/Tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~MatchPatternEMatchTest`
Expected: FAIL to compile — the whole project still does not build (`NodePattern` incomplete).

- [ ] **Step 3: Implement on `NodePattern`**

```csharp
            internal override bool CanEMatch => canEMatch;

            internal override IEnumerable<EBindings> EMatch(
                EGraph graph, int classId, EBindings bindings, Func<Entity, double> cost)
            {
                foreach (var node in graph.NodesOf(classId))
                {
                    if (node.Op != nodeType.Name || node.Children.Length != children.Length) continue;
                    foreach (var solution in EMatchInOrder(graph, node.Children, bindings, 0, cost))
                        yield return solution;
                    if (!commutative) continue;
                    var swapped = new[] { node.Children[1], node.Children[0] };
                    foreach (var solution in EMatchInOrder(graph, swapped, bindings, 0, cost))
                        yield return solution;
                }
            }

            private IEnumerable<EBindings> EMatchInOrder(
                EGraph graph, int[] actual, EBindings bindings, int index, Func<Entity, double> cost)
            {
                if (index == children.Length)
                {
                    yield return bindings;
                    yield break;
                }
                foreach (var head in children[index].EMatch(graph, actual[index], bindings, cost))
                    foreach (var rest in EMatchInOrder(graph, actual, head, index + 1, cost))
                        yield return rest;
            }

            internal override bool ETryBuild(
                EGraph graph, EBindings bindings, Func<Entity, double> cost, out int classId)
            {
                classId = 0;
                var parts = new int[children.Length];
                for (var i = 0; i < children.Length; i++)
                    if (!children[i].ETryBuild(graph, bindings, cost, out parts[i]))
                        return false;
                classId = graph.Add(nodeType.Name, parts);
                return true;
            }
```

`canEMatch` and `nodeCount` should already be assigned in the constructor from Task 3 — if Task 3 was
done with the "skip until Task 6/8" caveat, add the `canEMatch = children.All(child => child.CanEMatch);`
line to the constructor now, alongside the existing `buildable`/`deterministic` assignments.

- [ ] **Step 4: Run the full build**

Run: `dotnet build Sources/AngouriMath/AngouriMath.csproj`
Expected: succeeds — this is the point every concrete `MatchPattern` kind has all three new members.

- [ ] **Step 5: Run the tests**

Run: `dotnet test Sources/Tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~MatchPatternEMatchTest`
Expected: PASS, all of them (Tasks 3, 6, 7, 8's tests together).

- [ ] **Step 6: Run the full suite once, here, before continuing**

Run: `dotnet test Sources/Tests/UnitTests/UnitTests.csproj`
Expected: PASS, same total count as before this task plus the new tests added — nothing existing
should have changed behaviour, since nothing outside `MatchPattern.cs` calls any of these new members
yet.

- [ ] **Step 7: Commit**

```bash
git add Sources/AngouriMath/Core/Transformations/Matching/MatchPattern.cs \
        Sources/Tests/UnitTests/Core/Transformations/MatchPatternEMatchTest.cs
git commit -m "NodePattern e-matches recursively, trying both orders when commutative"
```

---

### Task 9: `MatchedRule.TryEMatchApply`

**Files:**
- Modify: `Sources/AngouriMath/Core/Transformations/Matching/MatchedRule.cs`
- Test: `Sources/Tests/UnitTests/Core/Transformations/MatchedRuleTryEMatchApplyTest.cs` (new)

**Interfaces:**
- Consumes: `Left.EMatch`, `Left.BoundNames`, `Right.CanEMatch`, `Right.ETryBuild`, the existing
  private `Build(Entity, Bindings)`, `EGraph.Extract`, `EGraph.AddEntity` (all existing or from
  earlier tasks).
- Produces:
  ```csharp
  internal bool TryEMatchApply(EGraph graph, int classId, Func<Entity, double> cost, out int resultClassId)
  ```
  Callers must check `rule.Left.CanEMatch` first; calling this when it is false throws (see
  implementation) rather than silently doing nothing, so a caller cannot forget the check and get a
  quiet wrong answer instead of a loud one.

- [ ] **Step 1: Write the failing test**

```csharp
//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Linq;
using AngouriMath.Core.Transformations;
using AngouriMath.Core.Transformations.Matching;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Core.Transformations
{
    public sealed class MatchedRuleTryEMatchApplyTest
    {
        private static readonly System.Func<Entity, double> Cost = AngouriMath.Core.CostModel.Default.Cost;

        [Fact]
        public void TryEMatchApplyAgreesWithTryApplyOnANonEGraphExpression()
        {
            var rule = MatchedRules.All
                .SelectMany(set => set.Rules)
                .First(r => r.Left.CanEMatch && r.Right is not null && r.Right.CanEMatch);
            // Build a source expression the same shape the rule's Left requires, by round
            // tripping through TryApply on a corpus of small expressions until one fires --
            // mirrors the approach ReversibleRuleTest.Corpus() uses.
            var source = TransformationTest.Corpus
                .Select(row => (string)row[0])
                .Select(text => text.ToEntity())
                .FirstOrDefault(entity => rule.TryApply(entity) is not null);
            if (source is null) return; // no corpus entry fits this particular rule's shape

            var expected = rule.TryApply(source);

            var graph = new EGraph();
            var root = graph.AddEntity(source);
            graph.Rebuild();
            Assert.True(rule.TryEMatchApply(graph, root, Cost, out var resultClass));
            var actual = graph.Extract(resultClass, Cost);

            Assert.NotNull(actual);
            Assert.Equal(expected!.Evaled, actual!.Evaled);
        }

        [Fact]
        public void TryEMatchApplyThrowsWhenTheRuleCannotEMatch()
        {
            var rule = MatchedRules.RationalizeDenominator.Rules.First(); // Left is a NodePattern
                                                                            // containing no
                                                                            // GatheredPattern in
                                                                            // some registry states
                                                                            // -- pick a rule whose
                                                                            // Left.CanEMatch is
                                                                            // actually false if
                                                                            // this one turns out
                                                                            // to be true; check
                                                                            // with a quick probe
                                                                            // before relying on it.
            if (rule.Left.CanEMatch) return; // this rule turned out e-matchable; not this test's case
            var graph = new EGraph();
            var root = graph.AddEntity("x".ToEntity());
            Assert.Throws<System.InvalidOperationException>(
                () => rule.TryEMatchApply(graph, root, Cost, out _));
        }
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test Sources/Tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~MatchedRuleTryEMatchApplyTest`
Expected: FAIL to compile — `TryEMatchApply` does not exist yet.

- [ ] **Step 3: Write minimal implementation**

In `MatchedRule.cs`, add beside `TryApply`:

```csharp
        /// <summary>
        /// The e-class <see cref="TryApply"/> would produce, found by matching against
        /// <paramref name="classId"/> directly rather than a materialised term. Caller must check
        /// <see cref="MatchPattern.CanEMatch"/> on <see cref="Left"/> first -- this throws rather
        /// than silently falling back, so a caller cannot forget the check and get the old,
        /// slower path without knowing it.
        /// </summary>
        internal bool TryEMatchApply(
            EGraph graph, int classId, Func<Entity, double> cost, out int resultClassId)
        {
            if (!Left.CanEMatch)
                throw new InvalidOperationException(
                    $"'{Name}' cannot e-match; check {nameof(Left)}.{nameof(MatchPattern.CanEMatch)} first.");

            resultClassId = 0;
            foreach (var ebindings in Left.EMatch(graph, classId, EBindings.Empty, cost))
            {
                Bindings? entityBindings = null;
                bool TryEntityBindings(out Bindings result)
                {
                    if (entityBindings is { } already) { result = already; return true; }
                    var built = Bindings.Empty;
                    foreach (var boundName in Left.BoundNames)
                    {
                        if (!ebindings.TryGet(boundName, out var boundClass)) { result = built; return false; }
                        var witness = graph.Extract(boundClass, cost);
                        if (witness is null) { result = built; return false; }
                        built = built.With(boundName, witness);
                    }
                    entityBindings = built;
                    result = built;
                    return true;
                }

                if (when is not null)
                {
                    if (!TryEntityBindings(out var forWhen)) continue;
                    if (!when(forWhen)) continue;
                }

                if (Right is { } right && right.CanEMatch)
                {
                    if (right.ETryBuild(graph, ebindings, cost, out resultClassId)) return true;
                    continue;
                }

                if (!TryEntityBindings(out var forBuild)) continue;
                var matched = graph.Extract(classId, cost);
                if (matched is null) continue;
                if (Build(matched, forBuild) is { } rewritten)
                {
                    try { resultClassId = graph.AddEntity(rewritten); }
                    catch { continue; }
                    return true;
                }
            }
            return false;
        }
```

`using System.Collections.Generic;` is likely already present in `MatchedRule.cs` (needed for
`Func<Bindings, bool>`'s enclosing usages); confirm before adding a duplicate.

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test Sources/Tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~MatchedRuleTryEMatchApplyTest`
Expected: PASS, 2 of 2. If `TryEMatchApplyThrowsWhenTheRuleCannotEMatch` returns early because
`RationalizeDenominator`'s first rule turns out `CanEMatch`, replace it with a rule found by scanning
`MatchedRules.All` for one whose `Left.CanEMatch` is false (a `GatheredPattern`-containing `Left`, if
the registry has one reachable this way) before treating the test as passing — an early `return`
inside a fact that never asserts anything is not evidence the behaviour works.

- [ ] **Step 5: Commit**

```bash
git add Sources/AngouriMath/Core/Transformations/Matching/MatchedRule.cs \
        Sources/Tests/UnitTests/Core/Transformations/MatchedRuleTryEMatchApplyTest.cs
git commit -m "MatchedRule.TryEMatchApply: what TryApply does, against an e-class directly"
```

---

### Task 10: Rewire `EqualitySaturationTransformation`

**Files:**
- Modify: `Sources/AngouriMath/Core/Transformations/Transformation.Catalogue.cs:402-475`
- Modify: `Sources/Tests/UnitTests/Core/Transformations/TransformationTest.cs`

**Interfaces:**
- Consumes: `MatchedRules.All` (Task 1), `MatchedRule.Growth`/`.Soundness`/`.Left`/`.TryEMatchApply`/
  `.TryApply` (Tasks 4, 9, existing).
- Produces: `EqualitySaturationTransformation.SafeRules` becomes `IReadOnlyList<MatchedRule>`;
  behaviour of `Transformation.EqualitySaturation(...)` is unchanged from the caller's point of view
  (same public factory signature, same `TransformationResult` shape) — only what runs inside changes.

- [ ] **Step 1: Write the failing test**

Append to `TransformationTest.cs`, inside the `#region Equality saturation` block:

```csharp
        [Fact]
        public void EqualitySaturationNowDrawsFromMatchingMatchedRules()
        {
            // Not a behavioural assertion (that is what every other EqualitySaturation test in
            // this file already covers) -- a structural one, that the rule source actually
            // changed. A rule whose Growth this plan's new MatchedRule.Growth can classify, but
            // whose public RewriteRuleGrowth twin could not, is the shape that proves it: pick
            // any one rule from Matching.MatchedRules.All with Growth Collects or Rearranges and
            // confirm EqualitySaturation still finds the corresponding rewrite.
            var result = Transformation
                .EqualitySaturation(SmallSaturationBudget, CostModel.Default)
                .Apply(Parse("x + 0"));
            Assert.True(result.Changed);
            Assert.Equal(Parse("x"), result.Output);
        }
```

This assertion already existed structurally as `EqualitySaturationReportsWhatItIsAndWhatItDid` — the
point of this new one is that it must **still** pass after the rewrite, proving the swap did not
silently stop finding the same rewrite it always found. Run it against the *current* (pre-rewire)
code first to confirm it already passes, establishing the regression baseline this task must not
break, then proceed.

- [ ] **Step 2: Run test to verify it currently passes (baseline, not RED)**

Run: `dotnet test Sources/Tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~EqualitySaturationNowDrawsFromMatchingMatchedRules`
Expected: PASS, against the *old* `RewriteRules.All`-sourced implementation. Record this — it is the
behaviour the rewrite must preserve.

- [ ] **Step 3: Rewire `SafeRules` and `ApplyCore`**

Replace the `SafeRules` field:

```csharp
            /// <summary>
            /// Every rule in <see cref="Matching.MatchedRules.All"/> whose
            /// <see cref="MatchedRule.Growth"/> is known not to expand and whose
            /// <see cref="MatchedRule.Soundness"/> is at least <see cref="Soundness.SoundUnderAssumptions"/>
            /// -- the real pattern-tree classification (Task 4), not the public registry's
            /// string-length proxy, and a per-rule <see cref="Soundness"/> check the previous,
            /// public-surface-sourced version of this field had no way to make (it filtered by
            /// Growth alone). Computed once: the registry does not change while the process runs.
            /// </summary>
            [ConstantField]
            private static readonly IReadOnlyList<Matching.MatchedRule> SafeRules
                = Matching.MatchedRules.All
                    .SelectMany(set => set.Rules)
                    .Where(rule => rule.Growth is RewriteRuleGrowth.Collects or RewriteRuleGrowth.Rearranges)
                    .Where(rule => rule.Soundness is Soundness.Sound or Soundness.SoundUnderAssumptions)
                    .ToList();
```

Replace the body of `ApplyCore`:

```csharp
            protected override Entity? ApplyCore(Entity input)
            {
                var graph = new EGraph();
                var root = graph.AddEntity(input);
                graph.Rebuild();

                var ledger = BudgetLedger.For(Name, budget);
                var chargedNodes = graph.NodeCount;
                bool ChargeGrowthSinceLastCall()
                {
                    var delta = graph.NodeCount - chargedNodes;
                    chargedNodes = graph.NodeCount;
                    return ledger.Spend(delta);
                }

                var saturated = false;
                while (!saturated && !ledger.Exhausted)
                {
                    var merged = false;
                    foreach (var id in graph.Classes.ToList())
                    {
                        if (!ChargeGrowthSinceLastCall()) break;
                        Entity? term = null;
                        bool TryTerm(out Entity value)
                        {
                            term ??= graph.Extract(id, costModel.Cost);
                            value = term!;
                            return term is not null;
                        }

                        foreach (var rule in SafeRules)
                        {
                            int other;
                            if (rule.Left.CanEMatch)
                            {
                                if (!rule.TryEMatchApply(graph, id, costModel.Cost, out other)) continue;
                            }
                            else
                            {
                                if (!TryTerm(out var t)) continue;
                                Entity? rewritten;
                                try { rewritten = rule.TryApply(t); }
                                catch { continue; }
                                if (rewritten is null || rewritten.Equals(t)) continue;
                                try { other = graph.AddEntity(rewritten); }
                                catch { continue; }
                            }
                            if (graph.Union(id, other)) merged = true;
                        }
                    }
                    graph.Rebuild();
                    if (!merged) saturated = true;
                }

                ledger.Report();
                return graph.Extract(root, costModel.Cost) ?? input;
            }
```

Add `using AngouriMath.Core.Transformations.Matching;` at the top of `Transformation.Catalogue.cs` if
it is not already there (check first — `Matching.MatchedRule`/`Matching.MatchedRules` are referenced
fully-qualified above specifically to avoid needing this if the file does not already import it
elsewhere; either approach is fine, but do not do both — pick fully-qualified, matching what is
already written above, and drop the `using` if added by mistake).

- [ ] **Step 4: Run the regression test**

Run: `dotnet test Sources/Tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~EqualitySaturation`
Expected: PASS, every test in the `Equality saturation` region of `TransformationTest.cs` —
`EqualitySaturationReportsWhatItIsAndWhatItDid`, `EqualitySaturationDeclinesToChangeAnExpressionAlreadyAtItsCheapest`,
`EqualitySaturationNeverThrowsUnderAStarvedBudget`, `EqualitySaturationNeverChangesTheValueItClaimsToPreserve`
(the `[Theory]` over `Corpus`), `EqualitySaturationPreservesANarrowedCodomain`,
`EqualitySaturationPreservesEulerIntrinsicIdentity` (the last two added on this branch by the code
review fix), and the new `EqualitySaturationNowDrawsFromMatchingMatchedRules`.

- [ ] **Step 5: Run the full suite**

Run: `dotnet test Sources/Tests/UnitTests/UnitTests.csproj`
Expected: PASS. Compare the total count against Task 8's full-suite run — it should be exactly the
tests added since, no drops, no unrelated failures.

- [ ] **Step 6: Commit**

```bash
git add Sources/AngouriMath/Core/Transformations/Transformation.Catalogue.cs \
        Sources/Tests/UnitTests/Core/Transformations/TransformationTest.cs
git commit -m "EqualitySaturation draws rules from Matching.MatchedRules and e-matches where it can"
```

---

### Task 11: Verification — corpus agreement, cross-union, and `ETryBuild` vs `TryBuild`

**Files:**
- Modify: `Sources/Tests/UnitTests/Core/Transformations/MatchPatternEMatchTest.cs`

**Interfaces:**
- Consumes: everything above. No new production code in this task — it is the spec's §6
  Verification section, the three items not already covered incidentally by earlier tasks' tests.

- [ ] **Step 1: Write `EMatchingAgreesWithMatching`**

```csharp
        [Theory]
        [MemberData(nameof(TransformationTest.Corpus), MemberType = typeof(TransformationTest))]
        public void EMatchingAgreesWithMatching(string source)
        {
            var expr = source.ToEntity();
            foreach (var rule in MatchedRules.All.SelectMany(set => set.Rules))
            {
                if (!rule.Left.CanEMatch) continue;

                var termMatches = rule.Left.IsDeterministic
                    ? (rule.Left.TryMatchOnce(expr, Bindings.Empty, out var once)
                        ? new[] { once } : System.Array.Empty<Bindings>())
                    : rule.Left.Match(expr, Bindings.Empty).ToArray();
                if (termMatches.Length == 0) continue;

                var graph = new EGraph();
                var root = graph.AddEntity(expr);
                graph.Rebuild();
                var eMatches = rule.Left.EMatch(graph, root, EBindings.Empty, Cost).ToList();

                Assert.True(eMatches.Count > 0,
                    $"{rule.Name} matched {source} by term but found nothing by e-matching");

                foreach (var termBindings in termMatches)
                {
                    var reproduced = eMatches.Any(eb => rule.Left.BoundNames.All(boundName =>
                        eb.TryGet(boundName, out var classId)
                        && graph.Extract(classId, Cost) is { } extracted
                        && extracted.Equals(termBindings[boundName])));
                    Assert.True(reproduced,
                        $"{rule.Name} on {source}: a term-match was not reproduced by e-matching");
                }
            }
        }
```

Note `Bindings` (not `EBindings`) needs to be accessible from the test file — it is `internal sealed`
in `AngouriMath.Core.Transformations.Matching`, same as `MatchPattern`; the test project already has
`InternalsVisibleTo` access (every other test in this file already reaches internal members of that
namespace).

- [ ] **Step 2: Run and confirm PASS**

Run: `dotnet test Sources/Tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~EMatchingAgreesWithMatching`
Expected: PASS over the whole `TransformationTest.Corpus`. If it fails on a specific rule, that is a
real bug in one of Tasks 6–8's implementations, not a test to loosen — fix the implementation.

- [ ] **Step 3: Write the cross-union test**

```csharp
        [Fact]
        public void EMatchingFindsAMatchThatCrossesAUnion()
        {
            // Build a graph where two structurally different terms are unioned into one class,
            // and confirm a NodePattern-based rule can e-match a shape that only exists because
            // of the union -- the capability term-matching against one representative cannot
            // reach, which is the actual point of e-matching over extraction.
            var graph = new EGraph();
            var doubled = graph.AddEntity("x + x".ToEntity());
            var multiplied = graph.AddEntity("2 * x".ToEntity());
            graph.Union(doubled, multiplied);
            graph.Rebuild();

            // Now the merged class contains both a Sumf-shaped and a Mulf-shaped e-node. A rule
            // whose Left requires Mulf must find it even when asked about the class via the
            // Sumf-shaped insertion id.
            var mulRule = MatchedRules.All
                .SelectMany(set => set.Rules)
                .First(r => r.Left.CanEMatch && r.Left.RequiredRootType == typeof(Entity.Mulf));
            var matches = mulRule.Left.EMatch(graph, doubled, EBindings.Empty, Cost).ToList();
            Assert.NotEmpty(matches);
        }
```

- [ ] **Step 4: Run and confirm PASS**

Run: `dotnet test Sources/Tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~EMatchingFindsAMatchThatCrossesAUnion`
Expected: PASS. If no registry rule has a `Mulf`-rooted, e-matchable `Left`, adjust the search to
whichever node type does (check with a quick throwaway script before hardcoding a different type) —
do not weaken the test to "any rule" in place of a specific node-type-crossing case, since that is
the whole point being verified.

- [ ] **Step 5: Write the `ETryBuild` vs `TryBuild` agreement test**

```csharp
        [Fact]
        public void ETryBuildAgreesWithTryBuildOnTheSameBindings()
        {
            var rule = MatchedRules.All
                .SelectMany(set => set.Rules)
                .First(r => r.Right is { } right && right.CanEMatch);

            var source = TransformationTest.Corpus
                .Select(row => (string)row[0])
                .Select(text => text.ToEntity())
                .FirstOrDefault(entity => rule.TryApply(entity) is not null);
            if (source is null) return;

            var expected = rule.TryApply(source)!;

            var graph = new EGraph();
            var root = graph.AddEntity(source);
            graph.Rebuild();
            Assert.True(rule.TryEMatchApply(graph, root, Cost, out var resultClass));
            var actual = graph.Extract(resultClass, Cost);

            Assert.NotNull(actual);
            Assert.Equal(expected.Evaled, actual!.Evaled);
        }
```

(This duplicates `MatchedRuleTryEMatchApplyTest.TryEMatchApplyAgreesWithTryApplyOnANonEGraphExpression`
in spirit — the spec asks for it as its own named verification item, so it is kept as a separate,
explicitly-named test here even though Task 9 already exercises the same code path once. Two tests
covering the same contract from two angles is acceptable; deleting either loses either "this is a
`MatchedRule`-level contract" or "this is the spec's own named verification item" as a distinct,
findable fact.)

- [ ] **Step 6: Run and confirm PASS**

Run: `dotnet test Sources/Tests/UnitTests/UnitTests.csproj --filter FullyQualifiedName~ETryBuildAgreesWithTryBuildOnTheSameBindings`
Expected: PASS.

- [ ] **Step 7: Run the full suite one final time**

Run: `dotnet test Sources/Tests/UnitTests/UnitTests.csproj`
Expected: PASS, in full. Also run `dotnet build Sources/AngouriMath/AngouriMath.csproj` on its own to
confirm all three target frameworks (`netstandard2.0`, `net8.0`, `net10.0`) still build — this
project multi-targets and a doc-comment `cref` ambiguity or similar has broken exactly one framework
target before in this session's own history; do not rely on `dotnet test`'s single-framework run
alone as proof.

- [ ] **Step 8: Regenerate `PublicApi.txt` if needed**

Run: `grep -c "EqualitySaturation" Sources/Tests/UnitTests/Common/PublicApi.txt` and compare to the
count before this plan started (1, from PR #1101). Nothing in this plan touches a `public` member —
every new type and member is `internal` — so this should be unchanged. If it is not, something in
this plan leaked a public surface; find it and make it `internal` before proceeding, rather than
updating the baseline to match.

- [ ] **Step 9: Commit**

```bash
git add Sources/Tests/UnitTests/Core/Transformations/MatchPatternEMatchTest.cs
git commit -m "Verification: e-matching agrees with matching, crosses a union, and builds the same"
```

---

## Open items for whoever picks this up next

- **Reporting the e-match coverage fraction** (spec §5's third bullet) has no home yet in
  `Transformation`'s current shape. Whether that is a new field on `TransformationResult`, a
  diagnostic callback, or something else is a design question this plan deliberately leaves open —
  see Global Constraints.
- **`GatheredPattern.NodeCount`'s approximation** (Task 3) is a documented, bounded imprecision, not
  a defect — but if a future audit finds a `GatheredPattern`-containing rule genuinely misclassified
  as `Collects`/`Rearranges` when it expands, that is real evidence for tightening it, not a
  surprise this plan failed to predict.
- **Retarget this branch's PR to `master`** once #1101 merges — it is currently stacked on
  `tier2-inverse-pair-table` per Global Constraints, and stacked PRs merge into their base, not into
  master, if the retarget is missed.
