//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace AngouriMath.Tests.Discrete
{
    using static AngouriMath.Entity;
    [Trait("Area", "Discrete")]
    public sealed class BooleanSolver
    {
        [Theory]
        [InlineData(10, "A & B -> C xor D -> not B")]
        [InlineData(5, "A | C -> E")]
        [InlineData(5, "A | C implies not E")]
        [InlineData(1, "B & C and A")]
        [InlineData(9, "C and not A & B -> C xor D -> not B")]
        [InlineData(6, "C and not A | C -> E")]
        [InlineData(6, "C and not A | C implies not E")]
        [InlineData(1, "C and not B & C and A")]
        [InlineData(1, "A & B")]
        [InlineData(1, "A")]
        [InlineData(3, "A | B")]
        [InlineData(2, "A xor B")]
        [InlineData(3, "A implies B")]
        [InlineData(1, "not A")]
        public void Test(int rootNumber, string exprString)
        {
            Entity expr = exprString;
            var vars = expr.Vars.ToArray();
            var res = MathS.SolveBooleanTable(expr, vars);
            if (res is null && rootNumber == 0)
                return; // success

            // null case should explicitly quit the scope
            if (res is null)
            {
                Assert.True(false, $"0 roots instead of {rootNumber}");
                return;
            }

            Assert.Equal(rootNumber, res.RowCount);

            var dict = new Dictionary<Variable, Entity>();
            var count = vars.Length;
            for (int i = 0; i < res.RowCount; i++)
            {
                for (int j = 0; j < count; j++)
                    dict[vars[j]] = res[i, j];
                Assert.True(expr.Substitute(dict).EvalBoolean());
            }
        }

        static Variable[] Vars(int count) =>
            Enumerable.Range(0, count).Select(i => (Variable)$"p_{i}").ToArray();

        /// <summary>
        /// Sixty variables is 2^60 assignments. Enumerating them was what this did, so this
        /// test cannot pass by accident: it either prunes or it never returns.
        /// </summary>
        [Fact]
        public void AConjunctionIsSolvedWithoutEnumeratingTheTable()
        {
            var vars = Vars(60);
            Entity expr = vars[0];
            for (var i = 1; i < vars.Length; i++)
                expr = expr & vars[i];

            var solutions = MathS.SolveBooleanTable(expr, vars);

            Assert.NotNull(solutions);
            Assert.Equal(1, solutions.RowCount);
            for (var j = 0; j < vars.Length; j++)
                Assert.True((bool)solutions[0, j].EvalBoolean());
        }

        /// <summary>Forty variables, forty solutions, out of 2^40 assignments.</summary>
        [Fact]
        public void ExactlyOneTrueIsFoundWithoutEnumeratingTheTable()
        {
            var vars = Vars(40);
            Entity expr = vars[0];
            for (var i = 1; i < vars.Length; i++)
                expr = expr | vars[i];
            for (var i = 0; i < vars.Length; i++)
                for (var j = i + 1; j < vars.Length; j++)
                    expr = expr & !(vars[i] & vars[j]);

            var solutions = MathS.SolveBooleanTable(expr, vars);

            Assert.NotNull(solutions);
            Assert.Equal(vars.Length, solutions.RowCount);
            for (var row = 0; row < solutions.RowCount; row++)
            {
                var trues = 0;
                for (var j = 0; j < vars.Length; j++)
                    if ((bool)solutions[row, j].EvalBoolean()) trues++;
                Assert.Equal(1, trues);
            }
        }

        /// <summary>
        /// An unsatisfiable expression has no rows, and the contract for that is a null
        /// rather than an empty matrix.
        /// </summary>
        [Fact]
        public void AContradictionHasNoSolutions()
        {
            var vars = Vars(30);
            Entity expr = vars[0] & !vars[0];
            for (var i = 1; i < vars.Length; i++)
                expr = expr & (vars[i] | !vars[i]);

            Assert.Null(MathS.SolveBooleanTable(expr, vars));
        }

        /// <summary>
        /// Pruning must not reorder the answer. The rows are still the satisfying rows of the
        /// truth table, in the order the table would have produced them.
        /// </summary>
        [Fact]
        public void RowsKeepTruthTableOrder()
        {
            var vars = Vars(4);
            Entity expr = (vars[0] | vars[1]) & (vars[2] | vars[3]);

            var solutions = MathS.SolveBooleanTable(expr, vars);
            Assert.NotNull(solutions);

            var expected = new List<int>();
            for (var assignment = 0; assignment < 16; assignment++)
            {
                var bits = Enumerable.Range(0, 4)
                    .Select(i => ((assignment >> (3 - i)) & 1) == 1).ToArray();
                if ((bits[0] || bits[1]) && (bits[2] || bits[3])) expected.Add(assignment);
            }

            Assert.Equal(expected.Count, solutions.RowCount);
            for (var row = 0; row < solutions.RowCount; row++)
            {
                var actual = 0;
                for (var j = 0; j < 4; j++)
                    actual = (actual << 1) | ((bool)solutions[row, j].EvalBoolean() ? 1 : 0);
                Assert.Equal(expected[row], actual);
            }
        }

        [Theory]
        [InlineData("(x implies a) = b", "{ False provided a and b, True provided a and b, False provided not a and b, True provided not a and not b }")]
        [InlineData("(x and a) = b", "{ True provided b and a, False provided a and not b, True provided not a and not b, False provided not a and not b }")]
        [InlineData("(x or a) = b", "{ True provided a and b, False provided a and b, True provided not a and b, False provided not a and not b }")]
        [InlineData("(x xor a) = b", "{ b provided not a, not b provided a }")]
        public void TestSymbolicSolver(string statementRaw, string expectedRaw)
        {
            Entity expected = expectedRaw;
            Entity statement = statementRaw;
            var solSet = statement.Solve("x");
            Assert.Equal(expected, solSet.InnerSimplified);
        }
    }
}
