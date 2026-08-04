//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Linq;
using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Algebra
{
    /// <summary>
    /// https://github.com/asc-community/AngouriMath/issues/550. A system whose equations do
    /// not pin every unknown down still has solutions; there are just infinitely many of them.
    /// The solver used to answer either null or a single tuple that is not the whole answer.
    /// </summary>
    public sealed class UnderdeterminedSystemTest
    {
        private const string h =
            "0.000100000000000000004792173602385929598312941379845142364501953125";
        private const string g =
            "9789.0960964999997808359069040306866750797257845497032630044387246925907675176858901977539062500000017547";

        /// <summary>
        /// Puts a solution row back into the equations it came from. A parametric answer has to
        /// satisfy them for every value of the parameter, so the residual must simplify to zero
        /// rather than merely evaluate to zero at a point.
        /// </summary>
        private static void AssertSatisfies(Entity[] equations, Entity.Variable[] vars, Entity.Matrix solutions)
        {
            Assert.Equal(vars.Length, solutions.ColumnCount);
            for (var row = 0; row < solutions.RowCount; row++)
                foreach (var equation in equations)
                {
                    var residual = equation;
                    for (var column = 0; column < vars.Length; column++)
                        residual = residual.Substitute(vars[column], solutions[row, column]);
                    Assert.Equal(Entity.Number.Integer.Create(0), residual.Simplify());
                }
        }

        // Two equations saying the same thing leave one unknown free. Before, x - y together
        // with 2x - 2y answered { (0, 0) }, which is a solution but not the solution set.
        [Theory]
        [InlineData("x - y", "2 * x - 2 * y")]
        [InlineData("x + y - 1", "2 * x + 2 * y - 2")]
        [InlineData("x - y", "3 * x - 3 * y")]
        public void ARepeatedEquationLeavesAFreeParameter(string first, string second)
        {
            Entity[] equations = { first.ToEntity(), second.ToEntity() };
            Entity.Variable[] vars = { "x", "y" };
            var solutions = MathS.Equations(equations).Solve(vars);
            Assert.NotNull(solutions);
            AssertSatisfies(equations, vars, solutions!);
            // A parameter, not a number: the answer stands for infinitely many tuples.
            Assert.NotEmpty(solutions![0, 0].Vars);
        }

        // A system that contradicts itself has no solutions at all, and null still says so.
        [Fact]
        public void AContradictorySystemHasNoSolutions() =>
            Assert.Null(MathS.Equations("x + y - 1", "x + y - 2").Solve("x", "y"));

        // A system that does pin every unknown down must keep answering with numbers.
        [Theory]
        [InlineData("x + y - 3", "x - y - 1")]
        [InlineData("2 * x + y", "x - y - 3")]
        public void ADeterminedSystemStillAnswersWithNumbers(string first, string second)
        {
            Entity[] equations = { first.ToEntity(), second.ToEntity() };
            Entity.Variable[] vars = { "x", "y" };
            var solutions = MathS.Equations(equations).Solve(vars);
            Assert.NotNull(solutions);
            AssertSatisfies(equations, vars, solutions!);
            Assert.Empty(solutions![0, 0].Vars);
        }

        /// <summary>
        /// The system from the report itself. Rewriting one equation from -(a + b + c) to
        /// -a - b - c used to decide whether it solved at all; both forms must solve, in either
        /// of the two orders the reporter tried them in.
        /// </summary>
        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void TheReportedSystemSolvesWhicheverWayItIsWritten(bool parenthesised, bool numericOrder)
        {
            Entity y1 = $"-Y_3 - v_3 * {h} * Y_4";
            Entity y2 = $"-Y_8 - v_1 * {h} * Y_1";
            Entity y3 = $"-Y_8 - v_5 * {h} * Y_7";
            Entity y4 = "Y_7 - Y_6 + Y_1";
            Entity y5 = $"-Y_2 - v_2 * {h} * Y_6";
            Entity y6 = $"-Y_5 - v_4 * {h} * Y_6";
            Entity y7 = "Y_4 - Y_6 + (-40743.6654315252017113380134105682373046875) * Y_3";
            Entity y8 = "Y_9 - Y_3 + Y_2 - Y_8";
            Entity y9 = parenthesised ? "-(Y_5 + Y_12 + Y_9)" : "-Y_9 - Y_5 - Y_12";
            Entity y10 = "Y_6 - Y_10 - Y_11";
            Entity y11 = "Y_11 + 80000 * Y_12 * n + (-800000) * n * n";
            Entity y12 = $"Y_12 - v_6 * {h} * Y_13";
            Entity y13 = $"Y_13 - {g} * v_7 + Y_10";

            var equations = numericOrder
                ? new[] { y1, y2, y3, y4, y5, y6, y7, y8, y9, y10, y11, y12, y13 }
                : new[] { y2, y5, y1, y7, y6, y4, y3, y8, y9, y10, y11, y12, y13 };
            var vars = Enumerable.Range(1, 13)
                .Select(i => (Entity.Variable)$"Y_{i}").ToArray();

            var solutions = MathS.Equations(equations).Solve(vars);
            Assert.NotNull(solutions);
            Assert.Equal(13, solutions!.ColumnCount);
        }
    }
}
