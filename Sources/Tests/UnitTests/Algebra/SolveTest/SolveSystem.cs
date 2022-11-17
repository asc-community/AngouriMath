﻿//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Collections.Generic;
using Xunit;
using AngouriMath;
using static AngouriMath.Entity.Number;

namespace AngouriMath.Tests.Algebra
{
    public sealed class SolveSystem
    {
        internal static void AssertSystemSolvable(Entity[] equations, Entity.Variable[] vars, int rootCount, Integer? ToSub = null)
        {
            ToSub ??= 3;
            var sol = MathS.Equations(equations).Solve(vars);
            if (sol is null)
                if (rootCount == 0)
                    return;
                else throw new Xunit.Sdk.XunitException($"{nameof(sol)} is null but {nameof(rootCount)} is {rootCount}");
            if (rootCount != -1)
                Assert.Equal(rootCount, sol.RowCount);
            var substitutions = new Dictionary<Entity.Variable, Entity>();
            for (int i = 0; i < sol.RowCount; i++)
                foreach (var equation in equations)
                {
                    var eqCopy = equation;
                    Assert.Equal(sol.ColumnCount, vars.Length);
                    substitutions.Clear();
                    for (int rootid = 0; rootid < sol.ColumnCount; rootid++)
                        substitutions.Add(vars[rootid], sol[i, rootid]);
                    eqCopy = eqCopy.Substitute(substitutions);

                    substitutions.Clear();
                    foreach (var uniqvar in eqCopy.Vars)
                        substitutions.Add(uniqvar, ToSub);
                    eqCopy = eqCopy.Substitute(substitutions);
                    var error = eqCopy.EvalNumerical().Abs();
                    Assert.True(error.IsFinite && error < 0.0001,
                        $"\n{nameof(equation)}: {equation.InnerSimplified.Stringize()}\n{nameof(i)}: {i}\n{nameof(error)}: {error.Stringize()}\n{nameof(sol)}: {sol.Stringize()}");
                }
        }

        [Fact]
        public void TestLinSystem2() => AssertSystemSolvable(new Entity[] {
            "x + y - 3",
            "x + 2y - 4"
        }, new Entity.Variable[] { "x", "y" }, 1);

        [Fact]
        public void TestLinSystem3() => AssertSystemSolvable(new Entity[] {
            "x + y - 3",
            "x + 2y - 4z",
            "z + 3x - 0.1y - 1"
        }, new Entity.Variable[] { "x", "y", "z" }, 1);

        [Fact]
        public void TestPolySystem3() => AssertSystemSolvable(new Entity[] {
            "x2 + y3 + z + 4",
            "3y3 - z - 2",
            "x2 - 0.1z + 4x2 + 4y3"
        }, new Entity.Variable[] { "x", "y", "z" }, 6, 5);

        [Fact]
        public void TestTrigSystem2() => AssertSystemSolvable(new Entity[] {
            "cos(x2 + 1)^2 + 3y",
            "y * (-1) + 4cos(x2 + 1)"
        }, new Entity.Variable[] { "x", "y" }, 8); // TODO: Should be 6 solutions according to Wolfram Alpha

        [Fact]
        public void TestTrigSystem3() => AssertSystemSolvable(new Entity[] {
            "a+b-c",
            "3a+3b-2c",
            "3a+4b-4c"
        }, new Entity.Variable[] { "a", "b", "c" }, 1);

        [Fact]
        public void TestTrigSystem4() => AssertSystemSolvable(new Entity[] {
            "a - 1",
            "b * 0"
        }, new Entity.Variable[] { "a", "b" }, 0);

        // https://www.youtube.com/watch?v=dVs26SSUJSA
        [Fact]
        public void TestSystemFromGermanOlympiad() => AssertSystemSolvable(new Entity[] {
            "x3 + 9 x2 y - 10",
            "y3 + x y2 - 2"
        }, new Entity.Variable[] { "x", "y" }, 9);

        [Fact] // Hint: The above test :)
        public void TestSystemFromSomewhereHmmAnalytical() => AssertSystemSolvable(new Entity[] {
            "x3 - 9 x2 y - f",
            "y3 + x y2 - a"
        }, new Entity.Variable[] { "x", "y" }, 9);

        [Fact]
        public void EquationWithDivisionIsSolved() => AssertSystemSolvable(new Entity[] {
            "x2 + y",
            "y - x - 3"
        }, new Entity.Variable[] { "x", "y" }, 2);

        [Fact]
        public void EquationLinear() => AssertSystemSolvable(new Entity [] {
            "2 * x_1 * (-66) - 6 * x_2 + 24 * x_3 - 12 * x_4 + 270",
            "-6 * x_1 - 2 * x_2 * 74 - 8 * x_3 + 4 * x_4 - 440",
            "24 * x_1 - 8 * x_2 - 2 * x_3 * 59 - 16 * x_4 - 190",
            "-12 * x_1 + 4 * x_2 - 16 * x_3 - 2 * x_4 * 71 + 20"
        }, new Entity.Variable[] { "x", "y" }, 1);
    }
}
