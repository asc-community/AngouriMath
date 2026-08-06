//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Linq;
using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// AngouriMath's one-sided limits did not stay inside the reals: <c>lim x-&gt;0- ln(x)</c>
    /// answered <c>-oo</c>, which is the magnitude of <c>ln|x| + i*pi</c> and not the limit of
    /// anything real-valued. There was no way to say which reading was meant, and that missing
    /// information is what stops agreeing one-sided limits from being promoted to a two-sided
    /// one -- promoting them without it would also give <c>lim x-&gt;0 x^x</c> the value 1,
    /// which the suite pins as non-existent because <c>x^x</c> is not real for negative x.
    /// <see cref="MathS.Settings.Codomain"/> is that information.
    /// https://github.com/asc-community/AngouriMath/issues/719
    /// </summary>
    public sealed class RealCodomainLimitTest
    {
        private static Entity Limit(string expr, string destination, ApproachFrom side) =>
            expr.ToEntity().Limit("x", destination.ToEntity(), side).Simplify();

        private static Entity LimitOverTheReals(string expr, string destination, ApproachFrom side)
        {
            using var _ = MathS.Settings.Codomain.Set(Domain.Real);
            return Limit(expr, destination, side);
        }

        private static bool IsUnevaluated(Entity limit) => limit.Nodes.Any(node => node is Entity.Limitf);

        /// <summary>
        /// The default is what AngouriMath has always done, so that nothing changes for anyone
        /// who does not ask. This is the whole of the compatibility claim and is checked
        /// against the values themselves rather than against the setting's value.
        /// </summary>
        [Theory]
        [InlineData("ln(x)", "0", "-oo")]
        [InlineData("x * ln(x)", "0", "0")]
        [InlineData("x ^ x", "0", "1")]
        [InlineData("sqrt(x)", "0", "0")]
        public void TheDefaultReadingIsUnchanged(string expr, string destination, string expected)
        {
            Assert.Equal(Domain.Complex, MathS.Settings.Codomain.Value);
            Assert.Equal(expected.ToEntity().Evaled,
                Limit(expr, destination, ApproachFrom.Left).Evaled);
        }

        /// <summary>
        /// Over the reals each of these is approached through values the function does not
        /// take, so it has no limit rather than the one its continuation approaches.
        /// </summary>
        [Theory]
        [InlineData("ln(x)", "0")]
        [InlineData("x * ln(x)", "0")]
        [InlineData("x ^ x", "0")]
        [InlineData("sqrt(x)", "0")]
        [InlineData("ln(x) / x", "0")]
        [InlineData("sqrt(x) + 1", "0")]
        public void ALimitReachedOnlyThroughTheComplexPlaneIsWithdrawn(string expr, string destination) =>
            Assert.True(IsUnevaluated(LimitOverTheReals(expr, destination, ApproachFrom.Left)),
                $"{expr} at {destination}- still answered over the reals");

        /// <summary>
        /// The other side of the same point, where the function *is* real, is untouched. That
        /// is the whole distinction: it is the approach that is judged, not the expression.
        /// </summary>
        [Theory]
        [InlineData("ln(x)", "0", "-oo")]
        [InlineData("x * ln(x)", "0", "0")]
        [InlineData("x ^ x", "0", "1")]
        [InlineData("sqrt(x)", "0", "0")]
        public void TheSideOnWhichItIsRealStillAnswers(string expr, string destination, string expected) =>
            Assert.Equal(expected.ToEntity().Evaled,
                LimitOverTheReals(expr, destination, ApproachFrom.Right).Evaled);

        /// <summary>
        /// A function real on both sides is unaffected in either direction, which is most of
        /// them -- the setting has to be nearly invisible or it is not usable.
        /// </summary>
        [Theory]
        [InlineData("sin(x) / x", "0", "1")]
        [InlineData("(1 - cos(x)) / x ^ 2", "0", "1/2")]
        [InlineData("x ^ 2", "0", "0")]
        [InlineData("1 / x ^ 2", "0", "+oo")]
        [InlineData("e ^ x", "0", "1")]
        [InlineData("(1 + x) ^ (1/x)", "0", "e")]
        public void AFunctionRealOnBothSidesIsUnaffected(string expr, string destination, string expected)
        {
            foreach (var side in new[] { ApproachFrom.Left, ApproachFrom.Right })
                Assert.Equal(expected.ToEntity().Evaled,
                    LimitOverTheReals(expr, destination, side).Evaled);
        }

        // At infinity there is one direction of approach and it is the destination's own sign.
        [Theory]
        [InlineData("ln(x)", "+oo", "+oo")]
        [InlineData("x ^ 2", "+oo", "+oo")]
        [InlineData("1 / x", "+oo", "0")]
        [InlineData("x ^ 2", "-oo", "+oo")]
        public void ALimitAtInfinityThatStaysRealIsUnaffected(string expr, string destination, string expected) =>
            Assert.Equal(expected.ToEntity().Evaled,
                LimitOverTheReals(expr, destination, ApproachFrom.Left).Evaled);

        [Theory]
        [InlineData("ln(x)", "-oo")]
        [InlineData("sqrt(x)", "-oo")]
        public void ALimitAtInfinityThatLeavesTheRealsIsWithdrawn(string expr, string destination) =>
            Assert.True(IsUnevaluated(LimitOverTheReals(expr, destination, ApproachFrom.Left)),
                $"{expr} at {destination} still answered over the reals");

        /// <summary>
        /// The setting is restored on the way out, and it is thread-static like every other
        /// one -- worth pinning, since a codomain that leaked would change answers far from
        /// where it was set.
        /// </summary>
        [Fact]
        public void TheSettingIsRestoredWhenItGoesOutOfScope()
        {
            Assert.Equal(Domain.Complex, MathS.Settings.Codomain.Value);
            using (var _ = MathS.Settings.Codomain.Set(Domain.Real))
                Assert.Equal(Domain.Real, MathS.Settings.Codomain.Value);
            Assert.Equal(Domain.Complex, MathS.Settings.Codomain.Value);
        }

        /// <summary>
        /// The point of the setting. Two agreeing one-sided limits are a two-sided one, and
        /// the promotion is safe here for the reason it was unsafe before: over the complex
        /// plane it would give <c>lim x-&gt;0 x^x</c> the value 1, and over the reals that
        /// side has no value to agree with.
        /// <a href="https://github.com/asc-community/AngouriMath/issues/596">#596</a> is the
        /// expression that reported it.
        /// </summary>
        [Fact]
        public void AgreeingOneSidedLimitsArePromoted()
        {
            const string reported = "1 / ln(x + sqrt(x ^ 2 + 1)) - 1 / ln(x + 1)";
            // Both sides answer, and agree.
            Assert.Equal("-1/2".ToEntity().Evaled, LimitOverTheReals(reported, "0", ApproachFrom.Left).Evaled);
            Assert.Equal("-1/2".ToEntity().Evaled, LimitOverTheReals(reported, "0", ApproachFrom.Right).Evaled);

            using var _ = MathS.Settings.Codomain.Set(Domain.Real);
            var twoSided = reported.ToEntity().Limit("x", 0).Simplify();
            Assert.Equal("-1/2".ToEntity().Evaled, twoSided.Evaled);
        }

        /// <summary>
        /// The two the promotion must not reach, and the reason it could not be made at all
        /// before the codomain existed. Each is real on one side only, so under this reading
        /// there is nothing on the other side to agree with.
        /// </summary>
        [Theory]
        [InlineData("x * ln(x)")]
        [InlineData("x ^ x")]
        [InlineData("sqrt(x)")]
        [InlineData("ln(x)")]
        public void AFunctionRealOnOneSideOnlyIsNotPromoted(string expr)
        {
            using var _ = MathS.Settings.Codomain.Set(Domain.Real);
            Assert.True(IsUnevaluated(expr.ToEntity().Limit("x", 0).Simplify()),
                $"{expr} was given a two-sided limit over the reals");
        }

        /// <summary>
        /// And the ones that genuinely have no limit keep saying so: the promotion asks
        /// whether the sides *agree*, not merely whether they exist.
        /// </summary>
        [Theory]
        [InlineData("1 / x")]
        [InlineData("abs(x) / x")]
        [InlineData("e ^ (1/x)")]
        [InlineData("arctan(1/x)")]
        public void SidesThatDisagreeStillHaveNoTwoSidedLimit(string expr)
        {
            using var _ = MathS.Settings.Codomain.Set(Domain.Real);
            Assert.Equal(MathS.NaN.Evaled, expr.ToEntity().Limit("x", 0).Simplify().Evaled);
        }

        /// <summary>
        /// The promotion is opt-in with everything else. Over the complex plane these answer
        /// exactly as they always have, which is what keeps <c>LimitTest.TestNoLimit</c>
        /// meaning what it meant.
        /// </summary>
        [Theory]
        [InlineData("sqrt(x)", "0")]
        [InlineData("sin(x) / x", "1")]
        public void TheDefaultReadingIsUnchangedForTwoSidedLimitsToo(string expr, string expected) =>
            Assert.Equal(expected.ToEntity().Evaled,
                expr.ToEntity().Limit("x", 0).Simplify().Evaled);

        // The two the real reading withdraws are, by default, still the definite NaN they
        // always were -- which is what keeps LimitTest.TestNoLimit meaning what it meant.
        [Theory]
        [InlineData("x ^ x")]
        [InlineData("x * ln(x)")]
        public void TheDefaultReadingStillCallsTheseNonExistent(string expr) =>
            Assert.Equal(MathS.NaN.Evaled, expr.ToEntity().Limit("x", 0).Simplify().Evaled);

    }
}
