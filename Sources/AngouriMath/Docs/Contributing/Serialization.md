## Serialization

An `Entity` written out and read back is the expression that was written, as exactly as the
printed form is — which is the whole of the design, and *What the printed form does not carry* below
is where that is not an identity. The format is the expression itself, in the library's own syntax:
what `Stringize` prints and `MathS.FromString` reads.

```csharp
public sealed record Problem(string Title, Entity Body);

JsonSerializer.Serialize(new Problem("quadratic", "x ^ 2 - 3 * x + 2"));
// {"Title":"quadratic","Body":"x ^ 2 - 3 * x + 2"}
```

Nothing has to be configured. `Core/Serialization/Entity.Serialization.cs` puts
`EntityJsonConverter` on every public node type, and `EntityJsonConverter` is public so that a caller
who builds `JsonSerializerOptions` by hand can add it there too.

### Why there is no structural format

The question this answers is not "how does an `Entity` serialize" but "what does a caller need that
the printed form does not already give them". The printed form is an exact serialization already, and
it is one the library has to keep exact whatever else it does, because it is also the *input* format:
parsing what `Stringize` prints gives back the expression printed, and
`EveryNodeSurvivesEveryPipelineTest` enumerates the node types by reflection and fails the day a new
one stops satisfying that.

A structural schema — a JSON object per node, with its children — would be a second description of
the same tree. It would need per-node code, a place in `AddingNode.cs`, a version story of its own,
and it would be free to drift from the first description in a way nothing detects. Two exact formats
for one tree is one more than the tree needs.

What it would buy is speed, and the number is worth writing down rather than guessing at. For a
textbook-sized expression — 43 nodes, 97 characters,
`(x ^ 3 - 3 * x ^ 2 + 2 * x) / (x ^ 2 - 1) + sin(2 * x) * cos(x / 2) - ln(x ^ 2 + 1) / sqrt(x + 3)` —
measured on `net10.0`, release, 2000 iterations after warm-up:

| | time | allocated |
|---|---|---|
| `Stringize()` | 45 us | 115 kB |
| `JsonSerializer.Serialize` | 41 us | 115 kB |
| `MathS.FromString` (a fresh string, so the parse cache misses) | 430 us | 793 kB |
| `JsonSerializer.Deserialize<Entity>` | 420 us | 883 kB |
| the same tree built from constructors, no text at all | 6.5 us | 25 kB |

So the JSON layer costs nothing over printing and parsing — it *is* printing and parsing — and
reading costs about 65 times what building the tree directly costs. That last row is the floor a
structural format could aim at, and it is a real gap. It is an argument for a faster parser, which
every caller who writes `Entity e = "..."` would also get, and not for a second representation that
only serialization uses.

`MathS.FromString` caches by string *instance*, so deserializing the same `string` object twice is
free and a benchmark that reuses one measures nothing.

### What the printed form does not carry

One, and it is a property of printing rather than of the converter, so fixing it there fixes it
here:

- **`Number.Complex` with both parts non-zero.** It prints as a sum and reads back as `Sumf` — the
  same number, a different node. Already recorded in `EveryNodeSurvivesEveryPipelineTest`.

**`Codomain` used to be on that list and no longer is.** A node narrowed with `WithCodomain` — or
written `domain(x, ZZ)`, which the parser has always accepted — came back with the default, because
nothing printed the annotation ([#1022](https://github.com/asc-community/AngouriMath/issues/1022)).
It prints as `domain(inner, SET)` now, and the converter needed no change at all: it serialises what
`Stringize` prints. `EntitySerializationTest.ACodomainSurvivesBecauseThePrintedFormCarriesIt` is
where that is held. Two corners of it are still the *grammar*'s limit rather than the printer's:
`Domain.Any` has no special set to name it, and no input yields a `Rational` whose codomain is
`Complex`, since the pass that reads `1/2` as a rational treats `Complex` as "nobody annotated
this".

And one that is not about a node type but about how operands nest. An **associative** operator whose
operands nest to the right comes back nested to the left, because the printed form does not bracket a
right operand at its own precedence: `1 + (2 + 3)` prints as `1 + 2 + 3` and reads back as
`(1 + 2) + 3`, and the same for `*`, `and`, `or` and `xor`. The node changes and the value does not,
which is what makes it an output choice rather than a defect — and it is the objection on #323 that
this format still has, and all that is left of it. An operator that is *not* associative does keep
its brackets, since [#1009](https://github.com/asc-community/AngouriMath/pull/1009).

Everything else round trips: 112 of the 115 node shapes measured, including `Lambda` and
`Application` with bound names, `Provided`, `ConditionalSet`, matrices, the `sum` and `product`
binders, and the bound constants `pi`, `e` and `i`.

### Why the attribute is on every node type and not only on `Entity`

`System.Text.Json` looks a converter attribute up on the declared type with `inherit: false`. An
attribute on `Entity` alone therefore leaves a member declared as `Entity.Variable` to the reflecting
object converter — which walks `Nodes`, a node's enumeration of *itself*, and reports an object
cycle after 64 levels. That is what `JsonSerializer.Serialize(entity)` did before this existed, for
every entity including `(Entity)3`.

`EntitySerializationTest.EveryPublicNodeTypeCarriesItsConverter` enumerates the public node types and
names any that is missing, so a node added later cannot join the list silently.

### Trimming and NativeAOT

Nothing here reflects and nothing scans assemblies. `JsonConverterAttribute` would construct the
converter with `Activator.CreateInstance`; `EntityJsonConverterAttribute` overrides `CreateConverter`
and returns `new EntityJsonConverter()` instead, so the type is reached statically.

### Frameworks

`netstandard2.0` has no `System.Text.Json` in the box and `AngouriMath.csproj` takes no package
reference for it, so the folder is compiled for `net8.0` and later. A `netstandard2.0` caller writes
`Stringize` and reads `MathS.FromString`, which is what the converter does.

### `BinaryFormatter`, `[Serializable]`, `DataContract`

None of them, deliberately. `BinaryFormatter` is
[obsolete and removed](https://github.com/dotnet/designs/blob/main/accepted/2020/better-obsoletion/binaryformatter-obsoletion.md),
and the rest reflect over fields — which here means the memoisation fields behind `DirectChildren`,
`Nodes`, `Vars` and the rest, none of which is state anybody would want written to a file. Writing
the expression as text is what makes that question not arise: no field of a node is ever read.
