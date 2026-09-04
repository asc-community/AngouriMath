# Writing a rewrite rule

This is the *how*. Whether a rule is **allowed** is a different question and a harder one —
[SimplificationContract.md](SimplificationContract.md) is that document, and it comes first. A rule
that is written beautifully and holds only on the positive reals while claiming to hold everywhere is
a wrong answer with good spelling.

[#746](https://github.com/asc-community/AngouriMath/issues/746) tier 2 asks for rules that are *data*
carrying "identity, name, direction, applicability conditions, justification tier, provenance, cost
effect". This is how to supply those seven things.

> Every count in this document is asserted by `RuleAuthoringGuideTest`. If a number here is wrong, a
> test fails — which is the only way a document like this stays true. Do not edit a figure without
> re-running that test; it is measuring the library, not repeating this file.

---

## Where a rule goes

`Core/Transformations/Matching/MatchedRules.cs`, as a value in a `MatchedRuleSet`. **33** sets and
**324** rules live there today.

The `switch` statements in `Functions/Simplification/Patterns` are the older form. Twenty-seven of
the thirty registered sets no longer run theirs, and **none of those twenty-seven describes it any
more either**; the three that remain still run theirs, so describing it is honest
([#825](https://github.com/asc-community/AngouriMath/issues/825)).
**Do not add a rule to a `switch`.** A `switch` arm cannot carry a name, a tier, an identity or a
direction, and everything below is about those.

## What a rule carries

```csharp
// a * (1 / b) = a / b
new MatchedRule(
    "reciprocal-factor-becomes-a-quotient",
    MatchPattern.Node<Mulf>(
        MatchPattern.Any("a"),
        MatchPattern.Node<Divf>(MatchPattern.Exact(Integer.Create(1)), MatchPattern.Any("b"))),
    MatchPattern.Node<Divf>(MatchPattern.Any("a"), MatchPattern.Any("b")),
    Soundness.SoundUnderAssumptions,
    description: "a * (1 / b) = a / b")
```

| | |
|---|---|
| **name** | a clause in English, in kebab case — see below |
| **left** | the pattern it fires on |
| **right** | the replacement, as a pattern or as code |
| **soundness** | how well justified *this* rule is, not its set |
| `description:` | the identity, in the notation a mathematician would write |
| `when:` | a side condition on the bindings, where the pattern cannot say it |
| `growth:` | how much bigger the replacement is — only where the replacement is code |

`SourceLine` is filled in by the compiler through `[CallerLineNumber]`. Nobody maintains it.

## The name is a sentence, not an identifier

Rule names are read aloud. `DerivationPath.Explain()` renders a derivation by replacing the hyphens
in a name and putting the identity in brackets after it:

```
2. Tangent is sine over cosine (tan(a) = sin(a) / cos(a)), so tan(x) becomes sin(x) / cos(x).
```

So the name has to be a clause that survives being read that way. **All 295 distinct rule names are,
and `StepAsASentenceTest` holds them to it** — a name with a capital, a bracket or an underscore
fails that test rather than degrading the prose quietly.

Write what the rule *says*, not what it operates on:

| | |
|---|---|
| ✔ | `a-quotient-of-a-thing-by-itself-is-one-unless-it-is-zero` |
| ✔ | `two-powers-of-one-base-divide-by-subtracting-exponents` |
| ✘ | `divf-simplification-2` — not a clause, and the 2 will be wrong when somebody inserts one |
| ✘ | `DivideByItself` — reads as an identifier in the middle of a sentence |

The names run from four to sixteen words. Long is fine; a name is read once and a wrong rewrite is
debugged for an afternoon.

## The identity is not the name

`description:` is the third distinct thing, and all three are worth having:

| | |
|---|---|
| name | `dividing-by-a-quotient-multiplies-by-its-reciprocal` — what to call it |
| description | `a / (b / c) = a * c / b` — what it says |
| `Left.ToString()` | `Divf(var a, Divf(var b, var c))` — how the matcher spells it |

Write the identity with `=`, not `->`: it is an equality, and the arrow belongs to the direction the
rule happens to be applied in. **294** rules carry one today; a new rule should.

## The pattern language

| | matches |
|---|---|
| `MatchPattern.Any("a")` | anything, binding it to `a` |
| `MatchPattern.Any<Number>("c")` | anything of that node type |
| `MatchPattern.Any<Integer>("n", v => v.IsPositive)` | that, and a predicate |
| `MatchPattern.Exact(Integer.Create(1))` | one literal value |
| `MatchPattern.Node<Mulf>(l, r)` | that node type, children in that order |
| `MatchPattern.Commutative<Sumf>(l, r)` | that node type, children in either order |
| `MatchPattern.Gathered<Sumf>("rest", parts)` | those parts anywhere in an n-ary chain, the rest bound |

**A hole repeated is an equality.** `Node<Divf>(Any("a"), Any("a"))` matches a quotient of a thing by
itself, and needs no `when:` to say so. It is also strictly more specific than `Divf(a, b)`, which
the ordering knows about — see below.

**Matchable is not the same as buildable.** A pattern can be *matched* against any node type, but a
pattern used as a **replacement** must be one the library can construct: there are **44** such types.
Two families are deliberately absent, and neither is an oversight:

- **binders** — `Lambda`, `Set.ConditionalSet`. They bind a variable, and `DirectChildren` renames it
  to avoid capture, so a pattern reaching inside one would match a name that does not exist outside;
- **variable-arity nodes** — `Piecewise`, `Application`, finite `Set`s, `Matrix`. A pattern fixes an
  arity and these do not have one.

Naming an unbuildable type in a replacement throws at construction rather than producing a rule that
matches and silently builds nothing. `BuildableNodeTypesTest` is the list.

## Replacement: a pattern, or code

**35** of the 324 rules have a pattern on both sides. The rest build their answer in code, and both
are legitimate — but the choice costs two things, so make it deliberately.

A pattern replacement gets:

- **a direction.** `MatchedRule.Reversed` is the rule read the other way, and **33** of the 35
  two-sided rules have one. The other two forget something: `sin²+cos² = 1` forgets the angle, and
  `{ x : x in S } = S` forgets the name the set builder bound, so neither has anything to read back.
  See [ReversibleRules.md](ReversibleRules.md).
- **an exact growth**, counted from the two patterns rather than declared.

A code replacement gets neither, and its growth is `Unknown` unless you declare one. That is the
honest default — **182** rules sit at `Unknown` — but declare it where you can justify it:

```csharp
// The Chebyshev expansion of sin(n * a) is a sum of n terms where the pattern is one node,
// for every n this fires on.
RewriteRuleGrowth.Expands,
```

**A declaration is a claim about every expression the rule fires on, and it is checked.**
`RuleGrowthAgreesWithTheCorpusTest` runs every declaration against the generated corpus and fails on
a contradiction, so "the whole point of it is to be smaller" is not a reason — that was the stated
reason for the one declaration this caught, and the rule it was on never shrank over fourteen
hundred expressions and grew by as much as eight nodes.

Growth is not documentation. `Saturation.RulesUpTo` selects by it, so `Collects` or `Rearranges`
puts a rule among the ones equality saturation fires and `Unknown` keeps it out. Claiming a rule
shrinks when it does not tells the saturation a rewrite is safe to run when nobody established
that. A corpus can only refute — firing without contradiction is evidence, not proof — so where you
cannot argue the claim from the code, leave it `Unknown`.

**Count the pattern against the replacement in terms of the holes**, and the answer is usually one
of a few shapes. A hole matched twice and written once gives `-(1 + |a|)`, which is a `Collects`
stronger than the corpus will show you, since the corpus fills its holes with single nodes. A
replacement holding no hole at all — a constant, or `pi/2` — collects by the whole of the pattern.
Operators mapping one for one with every hole used once is `Rearranges`.

**Four shapes look declarable and are not.** Each of these measured a clean, constant delta over the
corpus, and each is false:

| | |
|---|---|
| the replacement is built with `<`, `>` and their kin | they **chain**: `(x > y) < 0` is `x > y and y < 0`, so a three-node replacement becomes seven. `EqualTo` does not chain, which is why the `equals` rules are declared and their comparison twins are not |
| the replacement attaches a `Provided` built from the operands | its size grows with them while the pattern's shrinks away, so the delta changes sign: `2 - |c|` for the shared-factor cancellation, `4 - |a| - |b|` for `(a - b) / (b - a)` |
| a hole can be filled by two spellings of one thing | `IsWholeReciprocal` takes the literal `1/3`, which is one node, and a written `1 / c`, which is three — so the delta is 0 in one and −2 in the other |
| a hole is repeated and the replacement squares it | `a * (a * b) = a^2 * b` is `1 - |a|`: zero for a leaf and negative for anything bigger |

**The comparison set is settled, and settled as `Unknown`.** Its sixty-odd rules were gone through
one at a time and all but seven fall into the shapes above: most build their replacement with `<` or
`>` and so chain; the rest either attach a `Provided` sized by their own operands — the chain
implications, the two rules about comparisons that exclude each other — or compute the answer through
a helper, as the two that push a negation through a chain do. The seven that *are* declared are the
`EqualTo` ones, for the reason in the table. Nobody need re-derive that; if you are looking for
declarable rules, look elsewhere.

**And a declaration can expose a rule whose soundness was wrong.** Writing a growth down moves the
rule into the set equality saturation runs, which may be the first time it has ever run. That is how
[#1162](https://github.com/asc-community/AngouriMath/issues/1162) was found: the boolean
distribution's growth is plainly `-(1 + |k|)`, and declaring it made saturation fire a rule marked
`Sound` that changes the value at `a = b = 0.37`. If
`EqualitySaturationNeverChangesTheValueItClaimsToPreserve` fails when you declare a growth, the
finding is about the rule and not about the number you wrote.

Take `(Entity node, Bindings bound)` rather than `(Bindings bound)` where the replacement needs the
whole matched node. Rebuilding it from the bindings makes a different object with none of the
original's cached `InnerSimplified`, `Evaled`, `Codomain` or rate — measured at **4.0–5.2 s** on one
limit, against nothing for handing the node over.

## Soundness is per rule

| | |
|---|---|
| `Sound` | holds for every value the pattern admits, with nothing assumed |
| `SoundUnderAssumptions` | holds given something the rule does not check |
| `Heuristic` | usually right |

**182** of the 324 rules are `Sound` and **142** are conditional. Every one of the thirty registered
*sets* declares `SoundUnderAssumptions`, because a set's tier is the **minimum** over its rules — so
the set grain says nothing and the rule grain says everything. A derivation reports the rule's tier
(`RewriteStep.Soundness`), which is why getting it right matters beyond the label.

The tier is **declared, not checked**. It is a claim to argue with. Tightening one needs an argument;
loosening one does not.

## Order, and when it is yours to choose

A set is first-match-wins, so where two rules fire at one node and disagree, **whichever is tried
first decides the answer**. Two cases, and only one of them is a decision:

**Where one pattern subsumes the other, it is not a choice.** `Mulf(a, Divf(b, c))` matches
everything `Mulf(Divf(a, b), Divf(c, d))` matches and more, so the general rule would swallow the
special one and the special one would never fire. `MatchPattern.Subsumes` computes this and
`MatchedRuleSet.RulesByPriority` applies it: the specific rule is tried first because of what the two
patterns *are*, not because of where you typed them. **28** rule pairs are ordered that way.

**Where neither subsumes the other, the order is a bare choice and nothing but your placement makes
it.** There are **39** such conflicts, and `RulePriorityTest` records every one by name. If your rule
adds a fortieth, that test fails and tells you — which is the moment to decide whether you meant it.

Related, and separate: `RuleSetTerminationTest` checks that a set run to a fixed point actually
reaches one, and `RuleConfluenceTest` checks whether two rules of a set that both fire agree.

## What will check your rule

Adding a rule to `MatchedRules.cs` puts it in front of all of these without any wiring:

| | |
|---|---|
| `MatchedRulesAllTest` | it is reachable, named, and its set is well-formed |
| `MatchedRulesAgreeWithTheSwitchTest` | where the set mirrors a `switch`, the two still agree over generated expressions |
| `RulePriorityTest` | the order it is tried in is the order it is written in, and any new conflict is recorded |
| `RuleSetTerminationTest` | its set still settles, alone and composed with the normalisation |
| `RuleConfluenceTest` | any new disagreement between two arms is recorded |
| `ReversibleRuleTest` | its direction is what its two sides say it is |
| `MatchedRuleGrowthTest` | its growth is what its two sides say it is |
| `StepAsASentenceTest` | its name reads as English |
| `RuleMetadataTest` | its tier and identity reach the registry |
| `Corpus` | forty problems, on every commit, reporting solved / unsolved / wrong / error / timeout |

And outside the repository, in the analysis workspace: `casbench`, `propcheck`, `simpsweep`,
`boundcheck`, `rulecheck`, `canoncheck`. A green suite is evidence about the suite.

## The whole of it, once

```csharp
// x / x = 1, wherever x is not zero
new MatchedRule(
    "a-quotient-of-a-thing-by-itself-is-one-unless-it-is-zero",
    MatchPattern.Node<Divf>(MatchPattern.Any("a"), MatchPattern.Any("a")),
    (node, bound) => new Providedf(1, !bound["a"].EqualTo(0)),
    Soundness.SoundUnderAssumptions,
    description: "a / a = 1, provided a is not zero")
```

Seven decisions, and each of them is somewhere a reader can find it: the name says what it does, the
repeated hole says the two sides must be the same expression, the replacement attaches the condition
rather than asserting an equality that is false at zero, the tier says the answer is conditional, and
the identity says it in the notation a mathematician would use.

## See also

- [SimplificationContract.md](SimplificationContract.md) — whether the rule is allowed at all
- [Transformations.md](Transformations.md) — the layer the sets sit in
- [ReversibleRules.md](ReversibleRules.md) — what a rule needs before it can be read backwards
- [CanonicalForm.md](CanonicalForm.md) — what "simplest" is and is not
- [EMatching.md](EMatching.md) — how a pattern matches an e-class rather than a term
