//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Common
{
    /// <summary>
    /// Issues that are open in the tracker but no longer reproduce. Nothing here is a
    /// fix: each test pins behaviour that already works, so that its issue can be closed
    /// without leaving the fix that closed it unprotected. Every case is the reporter's
    /// own, taken from the issue.
    /// </summary>
    public sealed class AlreadyFixedIssuesTest
    {
        private static readonly System.TimeSpan Budget = System.TimeSpan.FromSeconds(30);

        /// <summary>
        /// Runs work that used to hang or overflow the stack, and fails if it does not
        /// finish. Asserted as termination rather than as a duration, so the test does not
        /// turn flaky on a slow machine, and so a regression fails the run instead of
        /// hanging it.
        /// </summary>
        private static T Terminates<T>(System.Func<T> work, string what)
        {
            var task = System.Threading.Tasks.Task.Run(work);
            Assert.True(task.Wait(Budget), $"{what} did not finish within {Budget.TotalSeconds}s");
            return task.Result;
        }

        // https://github.com/asc-community/AngouriMath/issues/263
        // InnerSimplify never returned on the reporter's quotient, and neither did the
        // limit it was reached from.
        private const string Issue263Quotient =
            "(x ^ 15 - 3 ^ 15 - 15 * 3 ^ 14 * (x - 3)) / (x - 3) ^ 2";

        [Fact]
        public void Issue263_InnerSimplifyTerminates() =>
            Terminates(() => Issue263Quotient.ToEntity().InnerSimplified,
                "InnerSimplify of the #263 quotient");

        [Fact]
        public void Issue263_LimitTerminates() =>
            Terminates(() => Issue263Quotient.ToEntity().Limit("x", 3, ApproachFrom.Left),
                "the #263 limit");

        // https://github.com/asc-community/AngouriMath/issues/347
        // Taking a limit of a boolean expression overflowed the stack. It is still not
        // evaluated -- there is nothing to evaluate -- but it comes back rather than
        // taking the process down.
        [Fact]
        public void Issue347_LimitOfABooleanDoesNotCrash() =>
            Terminates(() => "a and x".ToEntity().Limit("x", "+oo"), "lim (a and x)");

        // https://github.com/asc-community/AngouriMath/issues/362
        // Simplified to (-3/2 * d - d / 2) * t, which is the same number written worse.
        [Fact]
        public void Issue362_CoefficientsAreCollected() =>
            Assert.Equal("(-2) * d * t", "(-1/2d-3/2d)t".ToEntity().Simplify().Stringize());

        // https://github.com/asc-community/AngouriMath/issues/424
        // Piecewise was to become compileable.
        [Theory]
        [InlineData("5 provided x = 3", 3.0, 5.0)]
        [InlineData("x provided true", 7.0, 7.0)]
        [InlineData("2 * x provided x > 0", 4.0, 8.0)]
        public void Issue424_PiecewiseCompiles(string expression, double argument, double expected) =>
            Assert.Equal(expected, expression.ToEntity().Compile<double, double>("x")(argument), 9);

        // https://github.com/asc-community/AngouriMath/issues/415
        // Intersections and unions of intervals were to be simplified.
        [Fact]
        public void Issue415_IntervalsIntersect() =>
            Assert.Equal(@"[3; 5]".ToEntity(), @"[1; 5] /\ [3; 8]".ToEntity().Simplify());

        [Fact]
        public void Issue415_IntervalsUnite() =>
            Assert.Equal(@"[1; 8]".ToEntity(), @"[1; 5] \/ [3; 8]".ToEntity().Simplify());

        // https://github.com/asc-community/AngouriMath/issues/550
        // The reporter's small system did not solve. The 25x26 one from the same report
        // is a question of scale and is tracked separately at
        // https://github.com/asc-community/AngouriMath/issues/608.
        [Fact]
        public void Issue550_SmallLinearSystemSolves()
        {
            var equations = new[]
            {
                "x - y + 2 * z - 6",
                "2 * x + 3 * y + 2 * z - 11",
                "x + 2 * y + z - 8"
            };
            var solution = MathS.Equations(equations[0], equations[1], equations[2]).Solve("x", "y", "z");
            Assert.NotNull(solution);
            var (x, y, z) = (solution![0, 0], solution[0, 1], solution[0, 2]);
            // Checked by substitution rather than against printed values, so the test does
            // not depend on which form the solver hands the roots back in.
            foreach (var equation in equations)
            {
                var residual = equation.ToEntity()
                    .Substitute("x", x).Substitute("y", y).Substitute("z", z)
                    .EvalNumerical();
                Assert.True(residual.Abs().EDecimal.ToDouble() < 1e-9,
                    $"{equation} left {residual.Stringize()} at the reported solution");
            }
        }

        // https://github.com/asc-community/AngouriMath/issues/164
        // Reported as leaving sixteen uncollected terms.
        [Fact]
        public void Issue164_RepeatedFactorCollects() =>
            Assert.Equal("(1 + x) ^ 4".ToEntity(), "(x+1)^2*(x+2-1)^2".ToEntity().Simplify());
    }
}
