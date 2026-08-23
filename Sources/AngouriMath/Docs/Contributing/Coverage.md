# Coverage

Four legs of CI run tests, and they do not all measure the same thing. This says what each one
measures, so that a missing number is read as the boundary it is rather than as a leg nobody got
round to instrumenting.

| leg | workflow | what it runs | how coverage is collected | codecov flag |
|---|---|---|---|---|
| C# | `CSharpTest.yml` | `dotnet test Sources/Tests/UnitTests` | coverlet, both as `/p:CollectCoverage=true` and as the VSTest `XPlat Code Coverage` collector | `csharp` |
| F# | `FSharpTest.yml` | `dotnet test Sources/Tests/FSharpWrapperUnitTests` | VSTest `XPlat Code Coverage` collector | `fsharp` |
| Interactive | `InteractiveTest.yml` | `dotnet test` over the Interactive wrapper and the terminal | VSTest `XPlat Code Coverage` collector | `interactive` |
| C++ | `CPPTest.yml` | `ctest` over a native executable | `gcov`, reported by `gcovr` as Cobertura | `cpp` |

The first three all take the same route: `dotnet test` hosts managed assemblies, and a data
collector instruments them in that host. The fourth cannot, and the reason is worth writing down
because it decides what the `cpp` number means.

## The C++ leg does not carry managed code into CI

`AngouriMath.CPP.Exporting` sets `PublishAot`, and `CPPTest.yml` publishes it with
`-p:NativeLib=Shared -p:SelfContained=true`. What arrives in
`AngouriMath.CPP.Importing/out-x64` is one shared object and a symbol file; there is no
`AngouriMath.dll` and no `AngouriMath.CPP.Exporting.dll` in it. The tests are then a C++ binary
that ctest runs, not a test host.

So neither half of the mechanism the other three legs use is present. There is no `dotnet test` for
a collector to attach to, and there is no managed assembly in the deployed artefact for a collector
to instrument. Published Native AOT output is a native binary, and .NET documents the diagnostic
tooling that follows managed code into it as
[partial](https://learn.microsoft.com/dotnet/core/deploying/native-aot/diagnostics) — the managed
debugger does not attach, and managed heap analysis is unsupported.

That is a statement about this route, not a proof that no route exists. What it settles is that
copying a step from `CSharpTest.yml` into `CPPTest.yml` would upload an empty report under a flag
claiming the C++ leg was covered, which is worse than the flag being absent.

## What the `cpp` flag does cover

The C++ leg has C++ of its own, and until now nothing measured it:
`Sources/Wrappers/AngouriMath.CPP.Importing` is the handle cache, the error-code translation and
the exception boundary that every C++ caller passes through, and a defect there is a defect no
managed test can see. It is ordinary C++, so `gcov` covers it.

The Linux job in the matrix configures cmake with `--coverage -O0 -g`, runs the same ctest suite as
the other two, and reports with

```
gcovr --root <repo root> --filter '.*AngouriMath\.CPP\.Importing.*' --cobertura coverage-cpp.xml
```

The filter is the honest part. Without it the report is mostly vendored googletest, and it includes
`RunTests.cpp`, which is fully covered by construction because it is the thing being run.

**So `cpp` is coverage of the C++ wrapper, not of AngouriMath through the C++ wrapper.** A line of
`Simplify` exercised only from a C++ test appears in no report at all. Read the flag as answering
"how much of the wrapper do these tests reach", and read `csharp` for the kernel.

To reproduce it locally, publish the exporting project as `CPPTest.yml` does, then configure the
tests with the two cmake flags above and run `gcovr` from the build directory.
