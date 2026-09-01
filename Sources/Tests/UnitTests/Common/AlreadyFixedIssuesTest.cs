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
    [Trait("Area", "Common")]
    public sealed class AlreadyFixedIssuesTest
    {
        private static readonly System.TimeSpan Budget = LimitTermination.Guard;

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
                "InnerSimplify of the quotient from https://github.com/asc-community/AngouriMath/issues/263");

        // The limit is what the quotient was written for, so its value is asserted rather
        // than only that it comes back. Reported on the issue, this is 15*14/2 * 3^13.
        [Fact]
        public void Issue263_LimitIsComputed() =>
            Assert.Equal(Entity.Number.Integer.Create(167403915),
                Terminates(() => Issue263Quotient.ToEntity().Limit("x", 3, ApproachFrom.Left).Simplify(),
                    "the limit from https://github.com/asc-community/AngouriMath/issues/263"));

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
        // Piecewise was to become compileable. These are piecewise() nodes: the earlier
        // version of this test used `provided`, which is a Providedf and a different node
        // altogether, so it was not testing the issue at all.
        [Theory]
        [InlineData("piecewise(2 * x provided x > 0, -x provided true)", 3.0, 6.0)]
        [InlineData("piecewise(2 * x provided x > 0, -x provided true)", -4.0, 4.0)]
        [InlineData("piecewise(1 provided x > 10, 2 provided x > 5, 3 provided true)", 12.0, 1.0)]
        [InlineData("piecewise(1 provided x > 10, 2 provided x > 5, 3 provided true)", 7.0, 2.0)]
        [InlineData("piecewise(1 provided x > 10, 2 provided x > 5, 3 provided true)", 1.0, 3.0)]
        public void Issue424_PiecewiseCompiles(string expression, double argument, double expected) =>
            Assert.Equal(expected, expression.ToEntity().Compile<double, double>("x")(argument), 9);

        // https://github.com/asc-community/AngouriMath/issues/415
        // The example in the issue's screenshot, which is what it asks for. It needed two
        // things: an intersection has to distribute over a union, and endpoints have to be
        // compared by what they are worth rather than by whether they are written as bare
        // numbers -- (sqrt(33) - 3) / 6 is a division and never a Real.
        [Fact]
        public void Issue415_TheScreenshottedExample() =>
            Assert.Equal(@"(-1; (sqrt(33) - 3) / 6)".ToEntity().Simplify(),
                @"(-1; 1) /\ (((-(sqrt(33) + 3) / 6; (sqrt(33) - 3) / 6) \/ (1; +oo)))"
                    .ToEntity().Simplify());

        [Theory]
        [InlineData(@"[1; 5] /\ [3; 8]", "[3; 5]")]
        [InlineData(@"[1; 5] \/ [3; 8]", "[1; 8]")]
        [InlineData(@"(-1; 1) /\ ((0; 3) \/ (5; 7))", "(0; 1)")]
        [InlineData(@"(-1; 1) /\ (-2; sqrt(2) / 4)", "(-1; sqrt(2) / 4)")]
        [InlineData(@"(-1; 1) /\ (1; +oo)", "{ }")]
        public void Issue415_IntervalsSimplify(string input, string expected) =>
            Assert.Equal(expected.ToEntity().Simplify(), input.ToEntity().Simplify());

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
