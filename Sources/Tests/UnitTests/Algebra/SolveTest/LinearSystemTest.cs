//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Algebra
{
    /// <summary>
    /// Systems whose elimination order matters. Substituting one variable leaves
    /// occurrences of the next that cancel, and the solver used to commit to the first
    /// equation those occurrences appeared in.
    /// </summary>
    public sealed class LinearSystemTest
    {
        /// <summary>
        /// Solves and checks by substitution, so that the assertion does not depend on the
        /// form the solver returns the values in.
        /// </summary>
        private static void AssertSolves(params string[] equations)
        {
            var variables = new Entity.Variable[equations.Length];
            for (var i = 0; i < equations.Length; i++)
                variables[i] = $"x_{i + 1}";

            var solution = MathS.Equations(System.Array.ConvertAll(equations, e => (Entity)e.ToEntity()))
                .Solve(variables);
            Assert.NotNull(solution);

            foreach (var equation in equations)
            {
                var substituted = equation.ToEntity();
                for (var i = 0; i < variables.Length; i++)
                    substituted = substituted.Substitute(variables[i], solution![0, i]);
                var residual = substituted.EvalNumerical();
                Assert.True(residual.Abs().EDecimal.ToDouble() < 1e-9,
                    $"{equation} left {residual.Stringize()} at the returned solution");
            }
        }

        // https://github.com/asc-community/AngouriMath/issues/608
        // Returned null. Eliminating x_4 from the first equation leaves the second reading
        // x_1 + 2*x_2 + x_3 - (x_1 + x_2 + x_3 - 4) - 5, in which x_3 is written twice and
        // cancels; solving that for x_3 has no answer, and the search stopped there.
        [Fact]
        public void Issue608_DenseFourByFourSolves() => AssertSolves(
            "2 * x_1 * (-66) - 6 * x_2 + 24 * x_3 - 12 * x_4 + 270",
            "-6 * x_1 - 2 * x_2 * 74 - 8 * x_3 + 4 * x_4 - 440",
            "24 * x_1 - 8 * x_2 - 2 * x_3 * 59 - 16 * x_4 - 190",
            "-12 * x_1 + 4 * x_2 - 16 * x_3 - 2 * x_4 * 71 + 20");

        [Fact]
        public void Issue608_SmallestFailingCase() => AssertSolves(
            "x_1 + x_2 + x_3 + x_4 - 4",
            "x_1 + 2 * x_2 + x_3 + x_4 - 5",
            "x_1 + x_2 + 3 * x_3 + x_4 - 6",
            "x_1 + x_2 + x_3 + 4 * x_4 - 7");

        [Fact]
        public void Issue608_FiveByFiveSolves() => AssertSolves(
            "2 * x_1 + x_2 + x_3 + x_4 + x_5 - 6",
            "x_1 + 3 * x_2 + x_3 + x_4 + x_5 - 7",
            "x_1 + x_2 + 4 * x_3 + x_4 + x_5 - 8",
            "x_1 + x_2 + x_3 + 5 * x_4 + x_5 - 9",
            "x_1 + x_2 + x_3 + x_4 + 6 * x_5 - 10");

        // The sizes that already worked have to keep working.
        [Fact]
        public void SparseSystemsStillSolve() => AssertSolves(
            "x_1 + x_2 - 3",
            "x_2 + x_3 - 5",
            "x_3 + x_4 - 7",
            "x_1 - x_4 + 1");

        [Fact]
        public void DenseThreeByThreeStillSolves() => AssertSolves(
            "2 * x_1 + x_2 + x_3 - 4",
            "x_1 + 3 * x_2 + x_3 - 5",
            "x_1 + x_2 + 4 * x_3 - 6");

        // Trying further equations must not invent a solution where there is none.
        [Fact]
        public void InconsistentSystemStillReturnsNothing() =>
            Assert.Null(MathS.Equations("x_1 + x_2 - 1".ToEntity(), "x_1 + x_2 - 2".ToEntity())
                .Solve("x_1", "x_2"));
    }
}
