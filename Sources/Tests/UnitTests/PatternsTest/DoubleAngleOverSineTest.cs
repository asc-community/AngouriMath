//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.PatternsTest
{
    /// <summary>
    /// sin(2u) csc(u) is 2 cos(u), which nothing reduced. Opening sin(2u) up leaves
    /// 2 sin(u) cos(u) csc(u), whose sine and cosecant are no longer adjacent in the
    /// product, and the rules that cancel those are pairwise -- so
    /// (sin(2t) csc(t))^2/4 - cos(2t) - sin(t)^2 stopped one step short of zero, which is
    /// what https://github.com/asc-community/AngouriMath/issues/557 asks for.
    /// </summary>
    public sealed class DoubleAngleOverSineTest
    {
        private static Entity Bare(string expression)
        {
            var simplified = expression.ToEntity().Simplify();
            while (simplified is Entity.Providedf(var inner, _)) simplified = inner;
            return simplified;
        }

        [Theory]
        [InlineData("sin(2 * x) * cosec(x)", "2 * cos(x)")]
        [InlineData("cosec(x) * sin(2 * x)", "2 * cos(x)")]
        [InlineData("sin(2 * x) / sin(x)", "2 * cos(x)")]
        [InlineData("sin(2 * y) * cosec(y)", "2 * cos(y)")]
        [InlineData("(sin(2 * x) * cosec(x)) ^ 2 / 4 - cos(2 * x) - sin(x) ^ 2", "0")]
        public void TheDoubleAngleOverTheSingleOne(string expression, string expected) =>
            Assert.Equal(expected.ToEntity().Simplify(), Bare(expression));

        /// <summary>
        /// 2cos(u) is a number where sin(u) is zero and sin(2u) csc(u) is not, so the
        /// cosecant's own condition has to be carried rather than dropped. It is the same
        /// condition the cancellation of sin(u) csc(u) already comes back with.
        /// </summary>
        [Theory]
        [InlineData("sin(2 * x) * cosec(x)")]
        [InlineData("sin(2 * x) / sin(x)")]
        public void TheCosecantsConditionIsCarried(string expression) =>
            Assert.Contains("sin(x) = 0", expression.ToEntity().Simplify().Stringize());

        // Nothing else is claimed: a different argument, and a multiplier the identity
        // does not cover.
        [Theory]
        [InlineData("sin(2 * x) * cosec(y)")]
        [InlineData("sin(3 * x) * cosec(x)")]
        [InlineData("cos(2 * x) * cosec(x)")]
        public void OutsideTheIdentityNothingIsClaimed(string expression) =>
            Assert.Contains("csc", Bare(expression).Stringize());

        // The rewrite has to be the same number wherever both sides are defined.
        [Fact]
        public void TheRewriteIsTheSameNumber()
        {
            var original = "sin(2 * x) * cosec(x)".ToEntity();
            var simplified = Bare("sin(2 * x) * cosec(x)");
            foreach (var at in new[] { 0.3, 0.9, 1.4, 2.2, -0.7, -1.9 })
            {
                var before = original.Substitute("x", at).EvalNumerical().RealPart.EDecimal.ToDouble();
                var after = simplified.Substitute("x", at).EvalNumerical().RealPart.EDecimal.ToDouble();
                Assert.Equal(before, after, 9);
                Assert.Equal(2 * Math.Cos(at), after, 9);
            }
        }

        // The cancellations next to this one have to keep working.
        [Theory]
        [InlineData("sin(x) * cosec(x)", "1")]
        [InlineData("cos(x) * sec(x)", "1")]
        [InlineData("tan(x) * cotan(x)", "1")]
        [InlineData("sin(x) ^ 2 + cos(x) ^ 2", "1")]
        [InlineData("sin(2 * x) - 2 * sin(x) * cos(x)", "0")]
        [InlineData("cos(2 * x) - (1 - 2 * sin(x) ^ 2)", "0")]
        public void NeighbouringCancellationsAreUnaffected(string expression, string expected) =>
            Assert.Equal(expected.ToEntity().Simplify(), Bare(expression));
    }
}
