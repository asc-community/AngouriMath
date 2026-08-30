//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Common
{
    /// <summary>
    /// <see cref="Entity.Simplify(int)"/> and <c>EvalNumerical</c> have to agree about a
    /// closed-form constant, and for a negative <c>arccotan</c> they did not.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b><c>arccotan</c> here is <c>arctan(1/x)</c>, with range <c>(-pi/2, pi/2]</c>.</b> The
    /// inverse-trigonometric table read it as the textbook <c>pi/2 - arctan(x)</c>, whose range is
    /// <c>(0, pi)</c>: the two agree on every positive argument and on nothing negative, so
    /// <c>arccotan(-1)</c> simplified to <c>3/4 * pi</c> while the function's own value there is
    /// <c>-pi/4</c>.
    /// </para>
    /// <para>
    /// The convention is measured here rather than recalled, at the three arguments that settle a
    /// range — a positive one, a negative one and zero. That is the discipline this defect exists
    /// for: the identity is right in a textbook and wrong in this library, and nothing but running
    /// the function says which convention it keeps.
    /// </para>
    /// </remarks>
    [Trait("Area", "Common")]
    public sealed class ArccotanTableSignTest
    {
        /// <summary>
        /// The range, at the three arguments that pin it. <c>arccotan</c> is odd away from zero and
        /// lands on <c>pi/2</c> there, which is <c>(-pi/2, pi/2]</c> and not <c>(0, pi)</c>.
        /// </summary>
        [Theory]
        [InlineData("arccotan(1)", 0.7853981633974483)]
        [InlineData("arccotan(-1)", -0.7853981633974483)]
        [InlineData("arccotan(0)", 1.5707963267948966)]
        public void TheRangeIsTheOneThisLibraryKeeps(string expression, double expected)
            => Assert.Equal(expected,
                (double)expression.ToEntity().EvalNumerical().RealPart, 12);

        /// <summary>
        /// Every table value, positive and negative, and the closed form has to be the value.
        /// </summary>
        [Theory]
        [InlineData("arccotan(1)")]
        [InlineData("arccotan(-1)")]
        [InlineData("arccotan(0)")]
        [InlineData("arccotan(sqrt(3))")]
        [InlineData("arccotan(-sqrt(3))")]
        [InlineData("arccotan(1 / sqrt(3))")]
        [InlineData("arccotan(-1 / sqrt(3))")]
        [InlineData("arccotan(2 + sqrt(3))")]
        [InlineData("arccotan(-(2 + sqrt(3)))")]
        [InlineData("arccotan(sqrt(2) - 1)")]
        [InlineData("arccotan(1 - sqrt(2))")]
        public void TheClosedFormIsTheValue(string expression)
        {
            var entity = expression.ToEntity();
            Assert.Equal(
                (double)entity.EvalNumerical().RealPart,
                (double)entity.Simplify().EvalNumerical().RealPart,
                12);
        }

        /// <summary>
        /// The named case, exactly as it was wrong.
        /// </summary>
        [Fact]
        public void ANegativeArccotanIsNegative()
        {
            Assert.Equal("-1/4 * pi".ToEntity(), "arccotan(-1)".ToEntity().Simplify());
            Assert.Equal("pi / 4".ToEntity(), "arccotan(1)".ToEntity().Simplify());
        }

        /// <summary>
        /// And the sum rule, which had the convention right all along, still does — so the two
        /// readings of <c>arccotan</c> in the library now agree instead of contradicting.
        /// </summary>
        [Theory]
        [InlineData("arctan(1) + arccotan(1)", "pi / 2")]
        [InlineData("arctan(-1) + arccotan(-1)", "-1/2 * pi")]
        [InlineData("arctan(0) + arccotan(0)", "pi / 2")]
        public void TheSumRuleAndTheTableAgree(string expression, string expected)
            => Assert.Equal(expected.ToEntity().Simplify(), expression.ToEntity().Simplify());

        /// <summary>
        /// <c>arccos</c> uses the same complement helper and is <b>not</b> affected: its range is
        /// <c>[0, pi]</c> and <c>arcsin</c>'s is <c>[-pi/2, pi/2]</c>, so <c>pi/2 - arcsin(x)</c>
        /// holds for every argument. Asserted so that a later tidy-up cannot merge the two paths
        /// back together.
        /// </summary>
        [Theory]
        [InlineData("arccos(1/2)")]
        [InlineData("arccos(-1/2)")]
        [InlineData("arccos(sqrt(3) / 2)")]
        [InlineData("arccos(-sqrt(2) / 2)")]
        public void ArccosIsUnaffected(string expression)
        {
            var entity = expression.ToEntity();
            Assert.Equal(
                (double)entity.EvalNumerical().RealPart,
                (double)entity.Simplify().EvalNumerical().RealPart,
                12);
        }
    }
}
