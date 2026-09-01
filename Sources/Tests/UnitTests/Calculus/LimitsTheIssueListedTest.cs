//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// The limits <a href="https://github.com/asc-community/AngouriMath/issues/231">#231</a> asked
    /// for, and enough others to say the capability is there rather than the three examples.
    /// </summary>
    /// <remarks>
    /// <para>
    /// All three of the issue's cases answer, including <c>lim e^x - x</c>, which its checklist
    /// still shows unticked. They are pinned here because an issue's list is not a test: closing
    /// it leaves nothing that fails if one of them stops working.
    /// </para>
    /// <para>
    /// The wider sweep is the point of the file. Three cases passing says almost nothing about a
    /// limit solver — it is a search over several methods, and which one answers depends on the
    /// shape. These cover the standard forms: the two remarkable limits, an indeterminate power,
    /// a difference of infinities, polynomial against exponential, a root cancelling a linear
    /// term, and an oscillation damped by its argument.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class LimitsTheIssueListedTest
    {
        private static Entity At(string expression, Entity destination)
            => expression.ToEntity().Limit("x", destination).Simplify();

        private static Entity Infinity => Entity.Number.Real.PositiveInfinity;

        /// <summary>The three the issue names.</summary>
        [Fact]
        public void TheSecondRemarkableLimitIsE()
            => Assert.Equal("e".ToEntity(), At("(1 + 1/x)^x", Infinity));

        [Fact]
        public void TheFirstRemarkableLimitIsOne()
            => Assert.Equal(Entity.Number.Integer.One, At("sin(x)/x", 0));

        /// <summary>
        /// The one still unticked on the issue. An exponential beats a linear term, so the
        /// difference is unbounded rather than indeterminate.
        /// </summary>
        [Fact]
        public void AnExponentialLessALinearTermIsUnbounded()
            => Assert.Equal(Infinity, At("e^x - x", Infinity));

        /// <summary>
        /// The remarkable limits with the constants moved, which is what says a rule was applied
        /// rather than a value recognised.
        /// </summary>
        [Theory]
        [InlineData("(1 + 2/x)^x", "e ^ 2")]
        [InlineData("(1 + 1/x)^(2*x)", "e ^ 2")]
        [InlineData("ln(x)/x", "0")]
        [InlineData("x^2 / e^x", "0")]
        [InlineData("(x^2 + 1)/(2*x^2 - 3)", "1/2")]
        [InlineData("sqrt(x^2 + x) - x", "1/2")]
        [InlineData("sin(1/x) * x", "1")]
        [InlineData("arctan(x)", "pi / 2")]
        [InlineData("x - ln(x)", "+oo")]
        [InlineData("e^x - x^2", "+oo")]
        [InlineData("e^x / x^100", "+oo")]
        [InlineData("(1 + 1/x)^(x^2)", "+oo")]
        public void AtInfinity(string expression, string expected)
            => Assert.Equal(expected.ToEntity().Simplify(), At(expression, Infinity));

        [Theory]
        [InlineData("tan(x)/x", "1")]
        [InlineData("(1 - cos(x))/x^2", "1/2")]
        [InlineData("(e^x - 1)/x", "1")]
        [InlineData("ln(1 + x)/x", "1")]
        [InlineData("(sin(x) - x)/x^3", "-1/6")]
        [InlineData("(2^x - 1)/x", "ln(2)")]
        public void AtZero(string expression, string expected)
            => Assert.Equal(expected.ToEntity().Simplify(), At(expression, 0));

        /// <summary>
        /// <c>x * ln(x)</c> and <c>x^x</c> at zero are <see cref="MathS.NaN"/> two-sided and
        /// answer one side at a time, which is a decision rather than a gap: <c>ln(x)</c> is not
        /// real to the left of zero, so there is no two-sided limit to report. Pinned in both
        /// directions so that "it does not answer" and "it answers correctly" cannot be confused
        /// for one another later.
        /// </summary>
        [Theory]
        [InlineData("x * ln(x)", "0")]
        [InlineData("x^x", "1")]
        public void OneSidedWhereTwoSidedDoesNotExist(string expression, string expected)
        {
            Assert.Equal(MathS.NaN, At(expression, 0));
            Assert.Equal(
                expected.ToEntity(),
                expression.ToEntity().Limit("x", 0, ApproachFrom.Right).Simplify());
            Assert.Equal(
                expected.ToEntity(),
                expression.ToEntity().Limit("x", 0, ApproachFrom.Left).Simplify());
        }
    }
}
