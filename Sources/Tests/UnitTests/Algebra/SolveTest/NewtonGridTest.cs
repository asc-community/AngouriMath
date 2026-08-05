//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using PeterO.Numbers;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace AngouriMath.Tests.Algebra.PolynomialSolverTests
{
    /// <summary>
    /// The grid <see cref="Entity.SolveNt(Entity.Variable)"/> starts from was laid out with
    /// EDecimal's contextless division, which answers NaN for a quotient that does not
    /// terminate in base ten. Every step count outside 2^a * 5^b therefore produced one usable
    /// starting point and a grid of NaN, so the search covered a single corner of the region
    /// it was asked for and quietly returned fewer roots the finer it was told to look.
    /// </summary>
    public sealed class NewtonGridTest
    {
        private static HashSet<Entity.Number.Complex> Roots(string expression, int steps)
        {
            using var _ = MathS.Settings.NewtonSolver.Set(new MathS.Settings.NewtonSetting
            {
                From = (-10, -10),
                To = (10, 10),
                StepCount = (steps, steps),
                Precision = 30,
            });
            return expression.ToEntity().SolveNt("x");
        }

        private static double[] RealRoots(string expression, int steps) =>
            Roots(expression, steps)
                .Where(root => root.ImaginaryPart.EDecimal.Abs().CompareTo(EDecimal.Create(1, -8)) < 0)
                .Select(root => System.Math.Round(root.RealPart.EDecimal.ToDouble(), 6))
                .Distinct().OrderBy(value => value).ToArray();

        /// <summary>
        /// 8, 10, 16, 20 and 40 divide exactly and always worked; 3, 6, 7, 9, 12, 21 and 41 did
        /// not, and used to give back at most the one root reachable from the corner.
        /// </summary>
        [Theory]
        [InlineData(8)]
        [InlineData(9)]
        [InlineData(10)]
        [InlineData(12)]
        [InlineData(20)]
        [InlineData(21)]
        [InlineData(41)]
        public void EveryStepCountSearchesTheWholeRegion(int steps) =>
            Assert.Equal(new[] { -1.414214d, 0d, 1.414214d }, RealRoots("x^3 - 2*x", steps));

        /// <summary>
        /// Asking for a finer search must not lose roots a coarser one found. This is the
        /// property the defect broke, stated without naming any particular step count.
        /// </summary>
        [Fact]
        public void AFinerSearchKeepsWhatACoarserOneFound()
        {
            var coarse = RealRoots("x^3 - 2*x", 10);
            foreach (var steps in new[] { 11, 13, 17, 21, 33 })
                Assert.All(coarse, root => Assert.Contains(root, RealRoots("x^3 - 2*x", steps)));
        }

        /// <summary>
        /// A step count that does not divide exactly must lay out the same spread of starting
        /// points as one that does, rather than collapsing onto a corner: at 21 steps over
        /// [-10, 10] the search has to reach the roots of sin(x) - x/2 near +-1.8954942.
        /// </summary>
        [Fact]
        public void AStepCountThatDoesNotDivideExactlyStillReachesTheOutlyingRoots() =>
            Assert.Equal(new[] { -1.895494d, 0d, 1.895494d }, RealRoots("sin(x) - x / 2", 21));

        /// <summary>
        /// The grid is two-dimensional, so a step count of N lays real starting points
        /// (To - From) / N apart while costing N^2 Newton runs. Roots closer together than
        /// that fall in one interval and only one of them is reached -- which is
        /// https://github.com/asc-community/AngouriMath/issues/115. Sign changes along the
        /// real axis seed the search as well, and there are N^2 of those for the same
        /// reason there are N^2 grid points, so the spacing that matters for a real root
        /// is (To - From) / N^2: 0.2 at the default step count rather than 2.
        /// <para/>
        /// These roots are 0.5 apart, so a step count of 8 is where N^2 first resolves
        /// them and 5 is not enough -- asking for a cheaper search still gets one. The
        /// default of 10 has a factor of two in hand.
        /// </summary>
        [Theory]
        [InlineData(8)]
        [InlineData(10)]
        [InlineData(21)]
        public void RootsCloserTogetherThanTheGridSpacingAreStillFound(int steps) =>
            Assert.Equal(new[] { -0.5d, 0d, 0.5d }, RealRoots("arcsin(x) - x * pi / 3", steps));

        /// <summary>
        /// A sign change witnesses a root of odd multiplicity only, so the grid still has
        /// to be the one finding roots off the real axis and repeated roots on it. Neither
        /// may be lost to the new seeding.
        /// </summary>
        [Fact]
        public void RootsWithNoSignChangeAreStillFound()
        {
            Assert.Equal(new[] { -1.414214d, 1.414214d }, RealRoots("x^2 - 2", 10));
            Assert.Equal(new double[0], RealRoots("x^2 + 1", 10));    // both roots are +-i
            Assert.Equal(2, Roots("x^2 + 1", 10).Count);
            Assert.Contains(-1d, RealRoots("x^2 + 2*x + 1", 10));     // a double root, no sign change
        }
    }
}
