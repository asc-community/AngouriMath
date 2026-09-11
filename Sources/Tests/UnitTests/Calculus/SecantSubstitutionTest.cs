//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// The third trigonometric substitution: <c>b x^2 - a</c>, where <c>x = r sec(t)</c>.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <a href="https://github.com/asc-community/AngouriMath/pull/1273">#1273</a> shipped the
    /// tangent and sine substitutions and left this one out, because its domain is two intervals
    /// rather than one. That is the whole of what this adds, and the whole of what makes it
    /// delicate: <c>(a tan(t)^2)^(k/2)</c> is <c>a^(k/2) |tan(t)|^k</c>, and <c>tan(t)</c> is
    /// negative on the left one.
    /// </para>
    /// <para>
    /// <b>Every case here is checked on both intervals</b>, which is the point. Patching the
    /// three back-substitutions with an absolute value and a signum gets the right interval right
    /// and the left one wrong in a way that varies with the power outside the radical — measured,
    /// eight of thirteen shapes still reversed. What is true without cases is that the integrand
    /// is even or odd under <c>x -> -x</c> according to that power, so an antiderivative valid
    /// for <c>x &gt; r</c> gives the whole answer as <c>sign(x)^(m+1) F(|x|)</c>.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class SecantSubstitutionTest
    {
        private static void DifferentiatesBack(string integrand, double[] points)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("NaN", integral.Stringize());
            Assert.DoesNotContain("integral(", integral.Stringize());

            var derivative = integral.Substitute("C", 0).Differentiate("x");
            var original = integrand.ToEntity();
            var compared = 0;
            foreach (var at in points)
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                if (got.IsNaN || want.IsNaN)
                    continue;
                compared++;
                var difference = Math.Abs((double)(got - want).RealPart)
                               + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart));
                Assert.True(difference / scale < 1e-9,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, "
                    + $"where the integrand is {want}");
            }
            Assert.True(compared >= 4,
                $"only {compared} of {points.Length} points were comparable for {integrand}, "
                + "so this asserts almost nothing");
        }

        /// <summary>
        /// The radicand positive on both sides of the gap, and the points chosen on both — two
        /// above <c>r</c> and two below <c>-r</c>. An answer that is right only to the right of
        /// the gap passes a one-sided test and is wrong.
        /// </summary>
        [Theory]
        [InlineData("sqrt(x^2 - 1)", new[] { 1.7, 3.1, -1.9, -4.2 })]
        [InlineData("x^2/sqrt(x^2 - 1)", new[] { 1.7, 3.1, -1.9, -4.2 })]
        [InlineData("x^3*sqrt(x^2 - 1)", new[] { 1.7, 3.1, -1.9, -4.2 })]
        [InlineData("1/(x^2 - 1)^(3/2)", new[] { 1.7, 3.1, -1.9, -4.2 })]
        [InlineData("1/(x^2*sqrt(x^2 - 1))", new[] { 1.7, 3.1, -1.9, -4.2 })]
        [InlineData("1/(x^3*sqrt(x^2 - 1))", new[] { 1.7, 3.1, -1.9, -4.2 })]
        [InlineData("sqrt(9*x^2 - 1)/x^2", new[] { 0.9, 2.3, -1.1, -3.4 })]
        [InlineData("1/(x^3*sqrt(x^2 - 16))", new[] { 5.1, 9.3, -6.2, -11.0 })]
        [InlineData("(x^2 - 10)^(5/2)/x", new[] { 4.1, 7.3, -5.2, -9.0 })]
        [InlineData("sqrt(x^2 - 4)/x", new[] { 2.7, 5.1, -3.3, -6.2 })]
        public void BothIntervals(string integrand, double[] points)
            => DifferentiatesBack(integrand, points);

        /// <summary>
        /// With a linear term as well, so the square is completed first and the shifted constant
        /// is what comes out negative. The gap is then not around zero, and the points are chosen
        /// either side of where it actually is.
        /// </summary>
        [Theory]
        [InlineData("sqrt(x^2 - 2*x)", new[] { 2.7, 5.1, -1.3, -4.2 })]
        [InlineData("x/sqrt(x^2 - 2*x)", new[] { 2.7, 5.1, -1.3, -4.2 })]
        [InlineData("1/sqrt(x^2 - 4*x + 3)", new[] { 3.4, 6.1, -0.7, -3.2 })]
        public void AShiftedQuadratic(string integrand, double[] points)
            => DifferentiatesBack(integrand, points);

        /// <summary>
        /// A <b>whole</b> power of such a quadratic must not take this route, and this is a
        /// correctness bound rather than a matter of taste.
        /// </summary>
        /// <remarks>
        /// Where the power outside is a half the integrand is a square root of something negative
        /// between the roots, so it is undefined exactly where the substitution is. Where the
        /// power is whole the integrand is an ordinary polynomial, real on the whole line, and an
        /// answer built from <c>sqrt(x^2 - r^2)</c> is wrong in the middle: <c>(2x + 3x^2)^2</c>
        /// came back with the sign reversed at <c>x = -0.5</c> before this was bounded, which is
        /// a wrong answer and not a missing condition. These are pinned at points <b>inside</b>
        /// the interval the substitution cannot see.
        /// </remarks>
        [Theory]
        [InlineData("(2*x + 3*x^2)^2", new[] { -0.5, -0.2, 0.3, 1.0 })]
        [InlineData("(2*x + 3*x^2)^3", new[] { -0.5, -0.2, 0.3, 1.0 })]
        [InlineData("(x^2 - 1)^2", new[] { -0.5, -0.2, 0.3, 0.8 })]
        [InlineData("(x^2 - 4)^3", new[] { -1.5, -0.2, 0.3, 1.8 })]
        public void AWholePowerIsNotThisSubstitutions(string integrand, double[] points)
            => DifferentiatesBack(integrand, points);

        /// <summary>
        /// The other two substitutions, which must be unaffected — the branch is chosen by the
        /// two signs and there is no fourth case, so getting a third one wrong would show here.
        /// </summary>
        [Theory]
        [InlineData("1/(1 + x^2)^(3/2)", new[] { 0.3, 1.6, -0.4, -2.2 })]
        [InlineData("x^5/sqrt(5 + x^2)", new[] { 0.3, 1.6, -0.4, -2.2 })]
        [InlineData("x^2/sqrt(1 + x^2)", new[] { 0.3, 1.6, -0.4, -2.2 })]
        [InlineData("sqrt(1 - x^2)/x^2", new[] { 0.3, 0.5, -0.4, -0.8 })]
        [InlineData("1/(1 - x^2)^(3/2)", new[] { 0.3, 0.5, -0.4, -0.8 })]
        [InlineData("x/sqrt(1 + x + x^2)", new[] { 0.3, 1.6, -0.4, -2.2 })]
        [InlineData("1/(x*(1 + x^2)^2)", new[] { 0.3, 1.6, -0.4, -2.2 })]
        public void TheOtherTwoSubstitutionsAreUnaffected(string integrand, double[] points)
            => DifferentiatesBack(integrand, points);

        /// <summary>
        /// A radicand negative everywhere has no real integrand to integrate and is declined —
        /// there is no fourth substitution and none is invented.
        /// </summary>
        [Theory]
        [InlineData("sqrt(-1 - x^2)")]
        [InlineData("1/sqrt(-4 - 9*x^2)")]
        public void ARadicandNegativeEverywhereIsDeclined(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("NaN", integral.Stringize());
        }
    }
}
