module AngouriMath.FSharp.Tests.Systems

open AngouriMath.FSharp.Core
open AngouriMath.FSharp.Functions
open Xunit

// https://github.com/asc-community/AngouriMath/issues/562. Version 2 of the two shapes proposed
// there: the variables first and the equations second, which is how `solutions x expr` already
// reads, rather than a separate EquationSystem type the caller has to know about. `equationSystem`
// is still exposed for anyone who wants to build one and pass it around.

[<Fact>]
let ``A linear system is solved over its variables`` () =
    match solveSystem ["x"; "y"] ["x + y - 3"; "x - y - 1"] with
    | Some solutions ->
        Assert.Equal(1, solutions.RowCount)
        Assert.Equal(parsed "2", solutions.[0, 0])   // columns follow the order of vars
        Assert.Equal(parsed "1", solutions.[0, 1])
    | None -> failwith "the system has a solution and should not have answered None"

[<Fact>]
let ``An equality may be written out rather than moved to one side`` () =
    let asEquality = solveSystem ["x"; "y"] ["x + y = 3"; "x - y = 1"]
    let asExpression = solveSystem ["x"; "y"] ["x + y - 3"; "x - y - 1"]
    Assert.Equal(asExpression.IsSome, asEquality.IsSome)
    Assert.Equal(asExpression.Value.[0, 0], asEquality.Value.[0, 0])
    Assert.Equal(asExpression.Value.[0, 1], asEquality.Value.[0, 1])

/// An inconsistent system has no solution, and None is that rather than an empty matrix.
[<Fact>]
let ``An inconsistent system answers None`` () =
    Assert.True((solveSystem ["x"; "y"] ["x + y - 1"; "x + y - 2"]).IsNone)

[<Fact>]
let ``A system can be built and passed around`` () =
    let system = equationSystem ["x + y - 3"; "x - y - 1"]
    Assert.NotNull(system)
    Assert.True((system.Solve(symbol "x", symbol "y")) <> null)
