# What a node type has to implement, and who may write one

[#1026](https://github.com/asc-community/AngouriMath/issues/1026) asks a question the tree does not
answer anywhere: **can a node type be defined outside `AngouriMath.dll`, and if not, what would it
take?** `Entity` is a `public abstract partial record` with a `protected` constructor, which reads
as an invitation to derive from it. It is not one.

This file is the decision. It is upstream of
[#746](https://github.com/asc-community/AngouriMath/issues/746) item 56 (the first extensibility
seam), item 53 (groups, rings and fields as first-class,
[#440](https://github.com/asc-community/AngouriMath/issues/440)) and the whole v9.0 knowledge-package
tier, and it is the blocker [`Packaging.md`](Packaging.md) §5 records as *"blocked, not decided"* for
domain packages.

Every figure below was measured on `9b6fb5b0`; where one comes from a command, a reflection query or
a build, that is next to it. Run them again before quoting a figure at a later commit.

---

## 1. The contract as it is

`Entity` declares **twelve abstract members**. Reflection over the shipped assembly, so this counts
what a subclass actually has to satisfy rather than what the source appears to say:

```csharp
typeof(Entity).GetMethods(Instance | Public | NonPublic | DeclaredOnly).Where(m => m.IsAbstract)
```

| member | accessibility | read by | how many of the 68 node types write their own |
|---|---|---|--:|
| `Codomain` | `public` (`protected init`) | `InnerSimplifyWithCheck`, domain machinery | 62 |
| `DefaultCodomain` | **`internal`** | `Entity.Stringize()` and `Entity.Latexize()`, to decide whether the codomain has to be printed | 62 |
| `LatexizeNode()` | **`private protected`** | `Entity.Latexize()`, hence the LaTeX printer and CSharpMath downstream | 62 |
| `Replace(Func<Entity, Entity>)` | `public` | every traversal | 58 |
| `StringizeNode()` | **`private protected`** | `Entity.Stringize()`, hence the text printer and the parser round trip | 67 |
| `InitDirectChildren()` | `protected` | `DirectChildren`, hence everything | 58 |
| `InnerSimplify(bool)` | `protected` | `InnerSimplified`, `Evaled` | 59 |
| `Priority` | **`internal`** | `Functions/Output/` **only** | 35 |
| `ToSymPy()` | **`internal`** | `Functions/Output/ToSympy/` and `MathS.ToSympyCode` | 67 |
| `IntrinsicCondition` | **`private protected`** | `Entity.DomainCondition`, once | 58 |
| `InvertNode(Entity, Entity)` | **`private protected`** | `Entity.Invert`, hence the analytical solver | 58 |
| `SortHashName(SortLevel)` | **`private protected`** | `Functions/Simplification/Patterns/` **only** | 58 |

The last column counts node types that declare the member themselves; the rest inherit it from one
of the nine intermediate abstract records. `Entity.Function` gives 27 node types their `Priority`,
`Entity.Set.SpecialSet` gives five, `Entity.Variable` gives `Constant` one. **A shared default is
already the normal case for `Priority`**, which matters in §4.

Six of the eleven are reachable from another assembly. Five are not, and that is the whole of the
problem. One more sits below `Entity`: `Entity.Set.SpecialSet.ToDomain()` is `internal abstract`, so
a domain package cannot contribute a new special set either.

68 concrete node types, 10 abstract ones:

```
typeof(Entity).Assembly.GetTypes().Where(t => typeof(Entity).IsAssignableFrom(t))   // 78, incl. Entity
```

### The twelfth obligation, which is written nowhere

All 68 concrete node types carry the line `public override string ToString() => Stringize();`, and
it appears 69 times in the kernel — the extra one on an abstract base. Nothing requires it,
`AddingNode.cs` does not mention it, and it is not optional: without it the record compiler
synthesizes `ToString`, which calls the synthesized `Entity.PrintMembers`, which appends
`Entity`'s own public `Entity`-valued properties to the builder, each of which calls `ToString` on
the node again. The recursion is unbounded and terminates as
`InsufficientExecutionStackException` — §3 has the stack.

`EveryNodeSurvivesEveryPipelineTest` runs a `ToString` pipeline, so a *kernel* node that forgets the
line fails the suite. Nothing catches it for a node defined anywhere else.

---

## 2. What an external assembly can write today

Not a node. A class library referencing the built `AngouriMath.dll` and deriving from `Entity`,
implementing all six reachable members, does not compile:

```
GeoPoint.cs(7,30): error CS0534: 'GeoPointf' does not implement inherited abstract member 'Entity.IntrinsicCondition.get'
GeoPoint.cs(7,30): error CS0534: 'GeoPointf' does not implement inherited abstract member 'Entity.InvertNode(Entity, Entity)'
GeoPoint.cs(7,30): error CS0534: 'GeoPointf' does not implement inherited abstract member 'Entity.Priority.get'
GeoPoint.cs(7,30): error CS0534: 'GeoPointf' does not implement inherited abstract member 'Entity.SortHashName(TreeAnalyzer.SortLevel)'
GeoPoint.cs(7,30): error CS0534: 'GeoPointf' does not implement inherited abstract member 'Entity.ToSymPy()'
```

Exactly five, and no other error: the `protected` constructor and the six reachable members are fine.
`InternalsVisibleTo` is not an escape — a published extension package cannot be on that list.

**But the hierarchy is not closed.** Four node types are concrete and not `sealed`, by three
deliberate `#pragma warning disable SealedOrAbstract` exemptions — `Number.Complex`, `Number.Real`,
`Number.Rational` and `Variable`. A subclass of one of those inherits every implementation, so it has
nothing abstract left to satisfy, and it compiles from outside the assembly today:

```csharp
public sealed record MyReal(EDecimal D) : Entity.Number.Real(D);   // compiles against the nupkg
```

and it works: `Stringize` gives `3`, `3 + x` simplifies to `3 + x`, and `SolveEquation("x")` on it
gives `{ -3 }`. So "`Entity` is closed, therefore exhaustive matching is sound" is already only true
by convention, and `EveryNodeSurvivesEveryPipelineTest` — which enumerates
`typeof(Entity).Assembly.GetTypes()` — could not see such a type. What is genuinely closed today is
the set of node **kinds**, not the set of `Entity` subtypes.

---

## 3. Which of the five are contract and which are engine

The distinguishing question is not what a member looks like. It is: **is there an answer only the
node's author can give, or is there an answer the kernel can give on their behalf that is never
wrong?** Where a correct default exists, the member is engine machinery and the kernel should supply
it. Where it does not, the member is contract and the author must be made to state it.

### `Priority` — engine. Default: `Priority.Func`

Every read of `Priority` and `LatexPriority` in the kernel is under `Functions/Output/`; a grep for
`.Priority` outside that directory returns only declarations and `AddingNode.cs`. It is not a
semantic property of a node at all — it is the bracketing precedence its *parent* consults when
printing. Evaluation, simplification, solving and parsing never read it.

A correct default exists and is already the majority case: `Entity.Function` returns `Priority.Func`,
which 27 node types inherit. It is correct for exactly the shape an external node is obliged to have
anyway, because `AGENTS.md` requires a new node to print as a function call the parser already has
when its notation is not in the grammar. A node printed `point(x, y)` brackets like a function call,
and there is nothing else it could be: an external assembly cannot add operator syntax to the
grammar, so it can never need any other value.

### `SortHashName` — engine. Default: a name distinctive to the type

Read only from `Functions/Simplification/Patterns/`, where `SortHash` groups like terms. The values
are not per-node facts; they are a **deduplication table over kernel node kinds**: `Sumf` and `Minusf`
both hash to `summinus_` at the middle level, `Mulf`/`Divf`/`Modf` to `divmul_`, `Powf`/`Logf` to
`logpow_`. That table encodes which of *this library's* operators are the same up to inverse, and an
external node has no partner in it.

So the default is the node's own type name at every level. The direction matters: under-grouping
costs a simplification the engine might have found, wrong grouping costs correctness, and by
`AGENTS.md`'s ordering the conservative one wins.

### `IntrinsicCondition` — **contract**. No safe default exists

The only one of the five read by a `public` member: `Entity.DomainCondition` folds it over the
children, and `SimplificationContract.md` rests on the result — it is why `(x - 1)/(x - 1)` is
`1 provided x ≠ 1` rather than `1`.

39 of the 60 implementations are a bare `Boolean.True`, which invites a default. It must not become
one. `True` is not "unknown"; it is the positive claim *this operation is defined everywhere
its children are*, and for an unknown node that claim is the unsafe direction — it licenses a rewrite
straight over a singularity the kernel cannot see. Nobody but the node's author knows a new
operation's natural domain, so the author has to say. Its signature is `Entity`, which is already
public, so making it `protected abstract` adds no type to the compatibility surface at all.

### `ToSymPy` — engine. Default: `NotSufficientlySupportedException`

A printer for a foreign system, judged against SymPy's grammar rather than this library's. The kernel
already has the honest answer for a node with no SymPy form, and already supplies it from a base
class: `Entity.Set.SpecialSet` throws `NotSufficientlySupportedException` for any special set SymPy
has no name for, and `Application` throws it for the application of an undeclared lambda. So the
default is "no SymPy form", which is true of every node SymPy has not got.

An external node that *does* have a SymPy form should get it through a registry keyed by node type
rather than through a member on `Entity`. That is the same change [`Packaging.md`](Packaging.md) §6
prices as *"SymPy export out of the kernel"*, and it is separate work.

### `InvertNode` — engine. Default: `Enumerable.Empty<Entity>()`

Read once, by `Entity.Invert`, which the analytical solver drives. The default is what 17 of the 60
implementations already return: an empty sequence, meaning *this node cannot be inverted*. That is
the honest "no answer", which `AGENTS.md` ranks above every wrong one, and the solver already handles
it for over a quarter of the node types it meets. As with `ToSymPy`, an external node that *can* be
inverted wants a registry, not a member.

### The defaults, measured rather than argued

The five defaults above were given to a node kind the kernel has never seen — `GeoPointf(X, Y)`,
printing as `point(x, y)` — and it was put through the seventeen pipelines
`EveryNodeSurvivesEveryPipelineTest` uses, on five shapes. (Compiled against a local build of this
tree with `InternalsVisibleTo` added and signing off, which is the only way to compile such a type
today; not a build of `master`.)

Everything held. `point(x, x) + point(x, x)` simplified to `2 * point(x, x)` — the pattern layer
grouped an unknown node correctly through the default sort hash. `point(x, x) / point(x, x)`
simplified to `1 provided not point(x, x) = 0` — the `provided` machinery reached an unknown node
through `DomainCondition`. `Solve` refused with the library's own `UncompilableNodeException` where
it had nothing to say; `ToSymPy` refused with `NotSufficientlySupportedException`. Structural
equality, `Substitute`, `Differentiate`, `Integrate`, `Expand`, `Factorize`, `Vars`, `Nodes` and
`Complexity` all returned.

One pipeline did not, and it is the obligation from §1 that no document records:

```
   ToString         !! InsufficientExecutionStackException
        at System.Runtime.CompilerServices.RuntimeHelpers.EnsureSufficientExecutionStack()
        at AngouriMath.Entity.PrintMembers(StringBuilder builder)
        at DomainPack.GeoPointf.PrintMembers(StringBuilder builder)
        at DomainPack.GeoPointf.ToString()
        at System.Text.StringBuilder.Append(Object value)
        at AngouriMath.Entity.PrintMembers(StringBuilder builder)          [repeats]
```

`Entity` should declare `public sealed override string ToString() => Stringize();` once. That deletes
69 identical lines, removes the trap for every future node inside the kernel and outside it, and is
worth doing whether or not anything else here is.

---

## 4. The two options, and what each costs

### Option A — open the node contract

Four members get a kernel-supplied default and stay `internal` / `private protected`;
`IntrinsicCondition` becomes `protected abstract`. Nothing becomes `public`.

| member | becomes | default |
|---|---|---|
| `Priority` | `internal virtual` | `Priority.Func` |
| `SortHashName(SortLevel)` | `private protected virtual` | a name distinctive to the type, at every level |
| `ToSymPy()` | `internal virtual` | `throw new NotSufficientlySupportedException(...)` |
| `InvertNode(Entity, Entity)` | `private protected virtual` | `Enumerable.Empty<Entity>()` |
| `IntrinsicCondition` | **`protected abstract`** | none — the author states it |
| `ToString()` | `public sealed override` on `Entity` | `Stringize()`, and the obligation disappears |

**What it costs.** `Entity` stops being closed for new node kinds. Exhaustive `switch` over node
types inside the kernel is no longer total for an expression that came from elsewhere, and every such
site needs a default arm that is *correct* rather than `throw`. `EveryNodeSurvivesEveryPipelineTest`
keeps sweeping the kernel and cannot sweep what it has never seen, so an external node's coverage has
to be its own author's problem — a published test project the kernel builds, compiling one external
node and putting it through the same pipeline list, is what makes the promise checkable at all.

**What it does not cost, which is the point.** No new public API. `internal enum Priority` (30 named
values, one of them `LatexCalculusOperation`, a LaTeX-only concern in a general enum),
`internal enum SortLevel` and the `InvertNode` signature all stay internal and stay changeable.
`IntrinsicCondition` is `Entity`-valued, so it adds nothing either.

### Option A′ — publish the five members as they are

The reading the issue reaches for, and it is strictly worse than A for the same benefit. Making
`Priority` and `SortHashName` `public` or `protected` drags `Priority` and `SortLevel` into the public
API with them, because a member's signature cannot be more accessible than its types. That freezes
30 enum names *and their numeric values* — they are `|`-composed bitfields with deliberate ties, so
they are not free to renumber — plus a three-level sort enum that exists to serve one grouping pass,
plus `IEnumerable<Entity> InvertNode(Entity, Entity)`. Every later change to how this library brackets
its output or groups like terms becomes a `BREAKING-CHANGES.md` entry. That is a permanent cost for a
capability Option A delivers without it.

### Option B — keep `Entity` closed, extend by data

A domain package contributes a node *kind* — a name plus an operation table — rather than a subclass,
against one kernel node type that carries them.

**What it buys.** Exhaustive matching stays sound. The reflection sweep keeps meaning what it says.
`BREAKING-CHANGES.md` never has to record a node-contract change, because there is no node contract
outside the kernel.

**What it costs, and it is not small.** `InnerSimplify` is a function, not a table row: any node worth
adding needs behaviour, so the "data" acquires delegates immediately and the only thing actually
gained is that the delegate lives outside a subclass. The kernel then pays an indirection on a path
`Simplify` walks constantly, which is the (B) clause of [`Packaging.md`](Packaging.md) §4 and has to
be measured before it merges. A domain package loses the type system: its own rules match on a string
name rather than on `GeoPointf`, so its internal correctness is checked by nothing. And §5's own
review rule for tier 6 — *"a domain package is only accepted if it uses the shared infrastructure and
contributes to it; a statistics package that ships its own private expression type has failed the
review"* — is a rule about `Entity` being the shared type, which a name-and-table encoding satisfies
only in letter.

**Option B is also not the status quo.** §2 measured that `Number.Real`, `Number.Rational`,
`Number.Complex` and `Variable` are already externally derivable, so choosing B to preserve closedness
would mean sealing those four first, which is a breaking change of its own.

### The decision

**Option A.** It reaches the same capability as A′ at no cost in public API, and the thing Option B
was buying — a closed hierarchy — is measurably already gone. B's indirection lands on `Simplify`'s
default path, which is the one cost this repository has decided in advance it will not pay by
default.

Two things are *not* part of Option A and should not be smuggled into it: the SymPy and inversion
**registries**, which are what would let an external node do more than refuse, and which are also the
change that would let the SymPy exporter leave the kernel. Neither is needed to make a node type
compile, and each is worth its own issue.

---

## 5. Trimming and NativeAOT

Structural, not a preference. The kernel declares `IsAotCompatible`, `Sources/Tests/AotSmokeTest`
gates it on three operating systems, and [`Trimming.md`](Trimming.md) says what keeps it true. **Any
design that needs runtime assembly scanning is disqualified**, because a trimmer removes what only a
string names and the failure is a `null` from `Type.GetMethod` some calls later, with no warning at
publish time.

This is not a new constraint here — #746 already settled it: *"prefer source generators and explicit
registration over runtime type lookup"*, and v9.0 requires **statically declared** package contents
*"so that a packaged application can still be trimmed and AOT-published"*.

It bites one of the two issues #746 item 56 names.
[#338](https://github.com/asc-community/AngouriMath/issues/338) proposes, in its own words, that the
parser *"look up the types, inherited from `FunctionEntity`"* in the assembly. That mechanism cannot
be built. The capability behind it can: a node type registers its parsable name explicitly, or a
source generator emits the table beside the types it dispatches to, the way
`MathAllMethods.Definitions` already does.

Option A itself needs no reflection at all — it is virtual dispatch, which trims. Option B's table is
also fine if it is populated by explicit registration. The disqualification is specific to discovery.

---

## 6. Against `AddingNode.cs`

[`AddingNode.cs`](AddingNode.cs) is the closest thing to a written contract today, and it is a
different kind of document: a **checklist for a contributor inside the kernel**, ordered by what to do
first, naming an example to copy for each step. It covers twelve steps numbered 0 to 11, and steps
1, 4, 5, 8 and 11 —
numeric evaluation, pattern rules, `MathS` exposure, the parser, the serialization name — are places
the kernel has to be *taught about* a node, which an external assembly cannot reach at all.

The two files answer different questions and both should exist. What `AddingNode.cs` owes this one:

- **Step 2d says "add `Priority`", step 3e "hash for sorting", step 9 `InvertNode`, step 10
  `ToSymPy`** — all four listed as things to write, with no statement that not writing them is an
  option. Under Option A they become optional, and the checklist should say which default applies and
  when a node needs to override it.
- **`ToString` is absent from the checklist**, and §1 shows what its absence does. It should be
  removed from the contract entirely rather than added to the list.
- **`IntrinsicCondition` is absent too**, though it is required today and is the one member this file
  concludes is genuinely the author's to state. Step 3 should name it next to `InnerSimplify`.
- **`Codomain` is step 3f** and `Replace` is step 2c, so the reachable half is covered.

---

## 7. What this file does not settle

- **Whether an external node is in scope for `EveryNodeSurvivesEveryPipelineTest`.** The measurement
  in §3 covered five shapes of one node kind. It is evidence that the defaults hold, not a proof over
  the pipeline set, and the sweep's whole value is that it enumerates rather than lists.
- **What a kernel `switch` over node types should do with an unknown kind.** Every such site needs a
  default arm, and whether the honest answer is "return the node unchanged" or
  `NotSufficientlySupportedException` differs per site. They have not been enumerated.
- **Whether `Number.Real`, `Number.Rational`, `Number.Complex` and `Variable` being externally
  derivable is intended.** The `SealedOrAbstract` exemptions say what is exempt and not why external
  derivation is acceptable, and sealing them would be a breaking change either way.
- **What a registry for `ToSymPy` and `InvertNode` looks like**, and whether it is the same mechanism
  as #338's parsable-name lookup. Both are keyed by node type and both must be explicitly populated;
  whether that makes them one table is a design question this file does not open.
