# Reading a rewrite rule backwards

[#746](https://github.com/asc-community/AngouriMath/issues/746) tier 2 names this as the first thing
missing from the rewrite graph:

> a rule that can be *matched* rather than only run — its pattern is a C# source string today and its
> replacement is a closure, so **no rule can be read backwards**

This file says what a rule has to carry before it can be read backwards, which rules can carry it,
which cannot and why not, and what the mechanism costs the fast path. Read
[`Transformations.md`](Transformations.md) first for the layer it sits in and
[`SimplificationContract.md`](SimplificationContract.md) for what a rewrite may assume.

## Where the library stands

Measured on this checkout, and the commands are in the tests named below.

| | |
|---|--:|
| rule sets registered in `RewriteRules.All` | 30 |
| of those, addressable at rule grain | 29 |
| addressable rules in total | 405 |
| **of those, readable backwards** | **0** |
| rule sets expressed as data in `Core/Transformations/Matching` | 5 |
| rules in them | 14 |
| **of those, readable backwards** | **13** |
| registered rule sets that use the matcher at run time | 0 |

The 405 cannot be read backwards for a reason no amount of care about the rules would fix. They are
generated from the `switch` arms that define them, and an arm is a C# pattern and a C# expression:
`RewriteRule.PatternSource` and `RewriteRule.ReplacementSource` are the *text* of those, named
`Source` for exactly this reason. Text is something to print in a report, not something to match an
expression against.

## What a rule has to carry

Four things, and the first two are the whole of it.

**Both sides have to be patterns.** A `MatchPattern` is a term with named holes; it can be matched
against an expression, which takes the expression apart into `Bindings`, and — since
`MatchPattern.TryBuild` — it can be read as a template, which puts an expression together out of
bindings. A rule whose two sides are both patterns is a pair of terms rather than a pattern and a
procedure, and swapping them is the whole of reversal.

**Both sides have to bind the same holes.** This is the condition that decides which rewrites have a
backward reading and it is decidable from the two sides. If the replacement drops a hole the pattern
binds, the backward direction would have to invent what the forward direction discarded:
`sin(x)^2 + cos(x)^2 -> 1` cannot be read backwards because `1` does not say which angle, and
`x - x -> 0` cannot for the same reason. The other direction — a replacement naming a hole the
pattern never binds — is a typo, and the constructor refuses it. That check is only possible at all
because the replacement is data: a builder over the bindings fails on the first expression that
reaches it, or never.

**A hole's constraint is written once, on the side that states it.**
`(a/b)^c = a^c/b^c` needs a positive whole `c`, and that is written on the left, where the rule is
written. Read backwards the rule matches `a^c/b^c` for any `c` at all — and then refuses to *build*
`(a/b)^c` unless `c` passes. `MatchPattern.TryBuild` checks a hole's node type and its predicate on
the way out as well as on the way in, so no constraint is lost by reversing and none is written
twice.

**Both sides have to be constructible.** `MatchPattern.Construct` builds a node from its children,
written out as a switch over the node types rather than reflected, because
[`Trimming.md`](Trimming.md) forbids reflection in the kernel and `IUnaryNode`/`IBinaryNode` expose
the children and no constructor. A pattern over a node type absent from that switch is matchable and
not writable, which is a gap in the mechanism rather than a fact about the mathematics — so it is
reported as its own reason.

## What the type says

`MatchedRule.Reversal` is **derived from the two sides, never declared**, so the difference between a
one-way rewrite and a two-way one is something a test fails on:

| `RuleReversal` | means |
|---|---|
| `Reversible` | both sides are patterns over the same holes, and both can be built. `Reversed` is the rule with the sides swapped |
| `ReplacementIsCode` | the replacement is a builder over the bindings, so there is nothing to match against |
| `ReplacementDropsHoles` | the replacement does not mention every hole the pattern binds |
| `PatternCannotBeBuilt` | one side is over a node type `Construct` does not build |

`MatchedRule.Reversed` is `null` for every value but the first, and `MatchedRuleSet.Reversed` is the
set of those of its rules that have one — smaller than the set it came from wherever some do not,
and empty rather than absent where none do.

## Why the reversal is licensed

The forward rule claims `left = right` under its side condition. Two things follow, and they are the
argument for carrying each over unchanged:

- **The side condition carries over verbatim.** It is a predicate on the bindings, and matching the
  right-hand side produces the same bindings that matching the left-hand side does. So the reversed
  rule is guarded by exactly what the forward rule was guarded by, and `(a^b)^c = a^(b*c)` read
  backwards still refuses `x^(y*z)`.
- **`Soundness` carries over unchanged**, because what the rule claims is an equality and an equality
  is symmetric. A rule sound wherever both sides are defined is sound wherever both sides are defined
  read either way; the set of points is the same set.

**This is a property of a rule, not of a transformation.** `Expand` and `Factorize` are still not
inverses, `Unsolve` is still not well defined, and nothing here invents a symmetry the mathematics
does not have — see [AGENTS.md](../../../../AGENTS.md). What it says is narrower and checkable:
`k*p + k*q -> k*(p + q)` and `k*(p + q) -> k*p + k*q` are one rule read two ways, and the library can
now say so.

## What does not carry over

**Termination.** A rule that collects becomes one that expands. Composing a rule with its own
reversal does not reach a fixed point — `k*p + k*q` and `k*(p + q)` rewrite to each other forever —
so `RuleConfluenceTest`, which asserts that every set in `RewriteRules.All` reaches one, must not be
pointed at a reversed set. A reversed set is a thing to ask questions of, not one to run to
stability.

**Round-tripping through a *set*.** A reversed rule undoes the rule it came from, checked by value
over generated expressions in `ReversibleRuleTest`. A reversed *set* need not undo the set it came
from: both are first-match-wins and they order their rules independently, so a different rule may
answer first.

**The written order of a commutative operand.** Reversing a rule whose pattern is commutative writes
the operands in the order the rule was written in, so `a*x + b*x` comes back as `x*a + x*b` — the
same number written differently. The tests compare values, not trees, for exactly this reason.

**Usefulness.** `a/b -> a*(1/b)` is reversible and is not a rewrite anyone wants to run. Whether a
backward reading is worth taking is the caller's question; whether it exists is this file's.

## What it costs the fast path

Nothing, because nothing on the fast path reaches it. No type in
`Core/Transformations/Matching` is referenced anywhere in `Sources/AngouriMath` outside that
directory: 0 of the 30 registered rule sets use the matcher at run time, and the rule sets expressed
as data are a statement *about* the `switch` sets rather than a replacement for them. That is the
answer to #746's standing condition that the fast path survives as a path, and it is why the
`switch` stays the thing the simplifier calls.

The measurement, because "it should not cost anything" is not a measurement. Three copies of the
kernel in one process — the commit before this work, this work, and a byte-identical copy of the
first as a control — asked to `Simplify` seven expressions, sixty rounds, arm order rotating:

| arm | median | min | vs before | allocated | distinct values |
|---|--:|--:|--:|--:|--:|
| before | 347.5 ms | 344.1 ms | — | 658,407,960 B | 1 |
| after | 347.1 ms | 344.0 ms | −0.13% | 658,407,960 B | 1 |
| control (a byte-identical copy of *before*) | 345.9 ms | 343.3 ms | −0.47% | 658,407,960 B | 1 |

**Allocation is identical to the byte**, one distinct value per arm across all sixty rounds, and that
is the column to read: it is deterministic where a timing on an ordinary machine is not. The control
arm differs from the code it is a copy of by 0.47%, so half a percent is what this machine's noise
looks like and the 0.13% is not a signal. For contrast, the one exchange that put a rule set
expressed as data on the simplifier's path was measured at **+5% of `Simplify`** for that one set of
thirty, and was reverted.

## What is deliberately not built

**No production caller.** [`Packaging.md`](Packaging.md) and #746's own counterweight both say
speculative code without a consumer is not an asset, so this is one mechanism and its tests rather
than a framework. The consumer tier 2 names next is the inverse-pair table an e-graph needs: equality
saturation keeps both results where a pipeline keeps one, so it has to be told which rewrites undo
each other or it grows without bound.

`RewriteRuleGrowth` is what that question has today, and it does not answer it. Growth says a rule
was written smaller or larger than its pattern; it does not say *which* rule is the inverse of
*which*, and two rules can both collect without being related at all. A reversible rule is the
answer, because its inverse is the rule itself read the other way.

**No reversal for the 405.** Nothing here changes what `RewriteRules.All` can do. Making one of those
sets reversible means writing it as data, which puts it on the matcher — and that is the exchange
measured at 5%, so it is a decision about that set rather than a mechanical migration.

**No `Sound` promotions and no per-rule tiers argued.** Reversal carries the tier over; it does not
justify one. Per-rule soundness is tier 2's next item and wants an argument per rule, which is
writing rather than plumbing.
