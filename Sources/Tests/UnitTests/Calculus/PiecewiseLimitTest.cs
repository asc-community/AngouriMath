//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Extensions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// https://github.com/asc-community/AngouriMath/issues/536 -- a limit of a piecewise
    /// expression came back unevaluated wherever it was taken, including at points nowhere
    /// near a seam.
    /// </summary>
    public sealed class PiecewiseLimitTest
    {
        private const string Sign = "piecewise(-1 provided x < 0, 1 provided x > 0, 0)";

        private static Entity Limit(string expression, string destination, ApproachFrom side) =>
            expression.ToEntity().Limit("x", destination.ToEntity(), side).Simplify();

        private static void AssertLimit(string expression, string destination, ApproachFrom side, string expected) =>
            Assert.Equal(expected.ToEntity().Evaled, Limit(expression, destination, side).Evaled);

        /// <summary>
        /// Which case a piecewise agrees with near the destination is decided by the sign its
        /// predicates keep on the way in, so the seam matters only where the destination is on
        /// it. Away from one, the limit is the limit of the single case that holds there.
        /// </summary>
        [Theory]
        [InlineData(Sign, "0", ApproachFrom.Left, "-1")]
        [InlineData(Sign, "0", ApproachFrom.Right, "1")]
        [InlineData(Sign, "-456", ApproachFrom.BothSides, "-1")]
        [InlineData(Sign, "123", ApproachFrom.BothSides, "1")]
        [InlineData(Sign, "-oo", ApproachFrom.BothSides, "-1")]
        [InlineData(Sign, "+oo", ApproachFrom.BothSides, "1")]
        public void ThePieceThatHoldsNearTheDestinationDecides(string expression, string destination, ApproachFrom side, string expected) =>
            AssertLimit(expression, destination, side, expected);

        /// <summary>
        /// The case that holds gets the whole of the machinery, not only the substitution: the
        /// piecewise below is sin(x)/x everywhere the limit looks, and that is an indeterminate
        /// form in its own right.
        /// </summary>
        [Theory]
        [InlineData("piecewise(sin(x) / x provided not (x = 0), 0)", "0", ApproachFrom.Right, "1")]
        [InlineData("piecewise(sin(x) / x provided not (x = 0), 0)", "0", ApproachFrom.Left, "1")]
        [InlineData("piecewise(sin(x) / x provided not (x = 0), 0)", "0", ApproachFrom.BothSides, "1")]
        public void TheCaseThatHoldsIsAnsweredInFull(string expression, string destination, ApproachFrom side, string expected) =>
            AssertLimit(expression, destination, side, expected);

        /// <summary>
        /// The value at the point is not the limit and does not have to agree with it. Both
        /// cases here tend to 1 while the piecewise is 5 at the destination itself.
        /// </summary>
        [Theory]
        [InlineData("piecewise(x + 1 provided x < 0, x + 1 provided x > 0, 5)", "0", ApproachFrom.Left, "1")]
        [InlineData("piecewise(x + 1 provided x < 0, x + 1 provided x > 0, 5)", "0", ApproachFrom.Right, "1")]
        [InlineData("piecewise(x + 1 provided x < 0, x + 1 provided x > 0, 5)", "0", ApproachFrom.BothSides, "1")]
        public void TheValueAtThePointIsNotTheLimit(string expression, string destination, ApproachFrom side, string expected) =>
            AssertLimit(expression, destination, side, expected);

        /// <summary>
        /// A jump: the two one-sided limits exist and disagree, so the two-sided one does not
        /// exist. That is a claim about the mathematics, and NaN is how it is made.
        /// </summary>
        [Fact]
        public void AJumpHasNoTwoSidedLimit() =>
            Assert.Equal(MathS.NaN, Sign.ToEntity().Limit("x", 0, ApproachFrom.BothSides).Evaled);

        /// <summary>
        /// Where no case holds at all the expression is undefined on the whole of the way in,
        /// so there is nothing for it to tend to.
        /// </summary>
        [Fact]
        public void NoCaseHoldingMeansNoLimit() =>
            Assert.Equal(MathS.NaN, "piecewise(1 provided x > 5)".ToEntity().Limit("x", 0, ApproachFrom.Right).Evaled);

        /// <summary>
        /// A predicate whose truth near the destination cannot be read off leaves it open which
        /// case the limit is of, and an unevaluated limit is how that is said.
        /// </summary>
        [Fact]
        public void AnUnreadablePredicateIsLeftUnevaluated() =>
            Assert.IsType<Entity.Limitf>(
                "piecewise(1 provided x > a, 2)".ToEntity().Limit("x", 0, ApproachFrom.Right));

        /// <summary>
        /// The predicates are asked about one at a time and each of those questions is a limit,
        /// so a piecewise of several cases must not fan out. Termination is the assertion.
        /// </summary>
        [Fact]
        public void ManyCasesTerminate()
        {
            var task = Task.Run(() =>
                "piecewise(1 / x provided x < -1, x ^ 2 provided x < 0, sin(x) / x provided x < 1, ln(x), 7)"
                    .ToEntity().Limit("x", 0, ApproachFrom.Right));
            Assert.True(task.Wait(TimeSpan.FromSeconds(30)), "the limit of a piecewise did not terminate");
        }
    }
}
