# Harnesses

Console programs that measure the library against properties a unit test cannot list case by
case: each generates its inputs, runs them, verifies every answer it gets, and writes a report.
Each exits non-zero when it finds a defect, and `.github/workflows/Harnesses.yml` runs them on
every change to the library.

| harness | asks | fails on |
|---|---|---|
| `BoundCheck` | does `Simplify` keep the value **at the boundary** — across a branch cut, off the real line, outside a principal interval, at a pole | any disagreement |
| `RootCheck` | is the root set **complete**, on polynomials built from known factors | a missing root, a returned value that is not a root, an exception |
| `CasBench` | does a corpus of problems with known answers still solve, every answer verified — an integral by differentiating it back | a wrong answer, an exception |
| `SimpSweep` | does a simplification keep the value of the expression it came from, at sampled real points | any disagreement |
| `RuleCheck` | does a rewrite rule set do what it **declares**: terminate alone and composed with the normalisation, and keep the value where it claims `Equivalence` | a set that does not settle, a `Sound` declaration a value falsifies |
| `CrashCheck` | does anything take the process down: every case in a child process of its own, so a stack overflow is a result rather than the end of the run | a crash, an exception that is not the library declining |
| `PropCheck` | does each transformation satisfy a property it must: `Simplify`, `Expand` and `Factorize` keep the value, `Differentiate` agrees with a difference quotient, `Integrate` differentiates back | any property that does not hold |

A timeout fails none of them: a shared runner is slower than the machine a budget was set on.

Run one by hand with `dotnet run -c Release --project Sources/Tests/Harnesses/BoundCheck`. Its
report goes to this folder, where it is ignored by git, or to the directory `HARNESS_REPORTS`
names. A report says which commit it measured: a report records a build, not a branch.

More harnesses exist, and come here as each gains a criterion that fails on a defect and not on a
known finding or on the machine:
[#1256](https://github.com/asc-community/AngouriMath/issues/1256) lists them.
