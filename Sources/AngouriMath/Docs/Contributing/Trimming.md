# Trimming and NativeAOT

The kernel is published with `IsAotCompatible`, which is a claim to consumers: a trimmer takes it
as licence to remove code *inside* `AngouriMath.dll`, and the NativeAOT compiler keeps no
reflection metadata for a member nothing referenced. The claim is only as good as what checks it,
and what checks it is `Sources/Tests/AotSmokeTest`.

Run it the way CI does:

```
dotnet publish Sources/Tests/AotSmokeTest/AotSmokeTest.csproj -c Release -r linux-x64 -o artifacts/trimmed
./artifacts/trimmed/AotSmokeTest

dotnet publish Sources/Tests/AotSmokeTest/AotSmokeTest.csproj -c Release -r linux-x64 -p:AngouriMathPublishAot=true -o artifacts/aot
./artifacts/aot/AotSmokeTest
```

`-r` is required for both — trimming and ILC are per-runtime-identifier. On Linux the NativeAOT
half also needs `clang` and `zlib1g-dev`; on Windows the MSVC build tools; on macOS the Xcode
command line tools.

## Two things it checks, and they are not the same

**Publishing.** The project sets `TrimmerSingleWarn=false` so a warning names a line rather than an
assembly, and `ILLinkTreatWarningsAsErrors` / `IlcTreatWarningsAsErrors` so any IL2xxx or IL3xxx
from the kernel fails the publish. `TreatWarningsAsErrors` alone does not cover either of those:
Roslyn, the trimmer and the NativeAOT compiler each have their own switch.

**Running.** The binary is then executed, and its exit code is the gate. This half is not
redundant. Everything that goes wrong here goes wrong *silently at publish time*: a member removed
because only a string named it produces no warning at all, and the failure is a `null` from
`Type.GetMethod` or a "no coercion operator is defined between types" from `Expression.Convert`
some number of calls later. Publishing proves nothing about that; running does.

The smoke test covers parse, `Simplify`, `Solve`, `Differentiate`, `Integrate`, `Limit` and
`Compile`, asserts values rather than printed forms, and returns non-zero on any mismatch.

## What breaks a trimmed build

Anything that names a member at run time.

- `Type.GetMethod("Name")`, `Activator.CreateInstance(type)`, assembly scanning. Use a table
  generated beside what it dispatches to; `MathAllMethods.Definitions` is the worked example, and
  its entries are expression trees precisely because the compiler emits an `ldtoken` for the call
  inside one, which is how C# names a method without a string.
- `Type.MakeGenericType` over a value type, which NativeAOT cannot promise (IL3050). Where the set
  of types is closed, write the closed set down: `CompilationProtocol.nullableForms`.
- Letting `System.Linq.Expressions` find an operator or a conversion for you.
  `Expression.Add(l, r)` reflects over the operand type looking for `op_Addition`, and
  `Expression.Convert(e, t)` reflects over the target type looking for `op_Implicit`. For `double`
  and `long` those are IL instructions and there is nothing to find, but for
  `System.Numerics.Complex` and `BigInteger` they are methods, and a NativeAOT build had no
  metadata for them: `x + 1` compiled over a complex argument threw "the binary operator Add is not
  defined for the types 'System.Numerics.Complex' and 'System.Numerics.Complex'". Pass the method
  in — `CompilationProtocol.operators` and `CompilationProtocol.conversionOperators`.

`Expression.Compile()` itself is fine: under NativeAOT it falls back to the interpreter rather than
emitting IL, so a compiled expression is slower there than under the JIT but it is not broken.
See [#363](https://github.com/asc-community/AngouriMath/issues/363).

## When you add a node, a function or a protocol

Add the case to the smoke test as well. It is a few lines, it is the only place the trimmed and
NativeAOT behaviour of the new path is measured, and the publish will not tell you it is missing.
