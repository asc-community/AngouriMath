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
    /// Integrands holding a fractional power of something linear in the variable, which
    /// <c>u^q = a*x + b</c> turns into a rational function.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>x/sqrt(1 + x)</c> had no antiderivative, and the reason is worth keeping: the general
    /// substitution rewrites <em>sub-expressions</em>, so it found <c>1 + x</c>, replaced it, and
    /// was left holding a bare <c>x</c> it could not express in the new variable. This substitutes
    /// for <b>x itself</b>, so nothing is left behind.
    /// </para>
    /// <para>
    /// Checked by differentiating back and comparing at points, never against a printed form —
    /// these come out as polynomials in the radical and their shape says nothing about whether
    /// they differentiate back.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class LinearRadicalSubstitutionIntegralTest
    {
        /// <summary>
        /// Points where every radical below is real: each of the bases used here is positive on
        /// <c>(0, 0.6)</c>, which is what lets one set serve them all.
        /// </summary>
        private static readonly double[] Points = { 0.05, 0.17, 0.31, 0.44, 0.58 };

        private static void DifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());

            var derivative = integral.Substitute("C", 0).Differentiate("x");
            var original = integrand.ToEntity();
            var compared = 0;
            foreach (var at in Points)
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                if (got.IsNaN || want.IsNaN)
                    continue;
                compared++;
                var difference = Math.Abs((double)(got - want).RealPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart));
                Assert.True(difference / scale < 1e-9,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, "
                    + $"where the integrand is {want}");
            }
            Assert.True(compared >= 4,
                $"only {compared} of {Points.Length} points were comparable for {integrand}, "
                + "so this asserts almost nothing");
        }

        /// <summary>
        /// For an even <c>q</c> the principal root <c>u = (a x + b)^(1/q)</c> is not negative
        /// wherever it is real, and a root holding a power of <c>u</c> gives that power up:
        /// <c>1/sqrt(x + x^(3/2))</c> under <c>u = sqrt(x)</c> is <c>2u/sqrt(u^2 + u^3)</c>, a
        /// root of a cubic that nothing reads, and is <c>2/sqrt(1 + u)</c>. Apostol's
        /// <c>x/sqrt(1 + x^2 + (1 + x^2)^(3/2))</c> is the same one step further in.
        /// </summary>
        [Theory]
        [InlineData("1/sqrt(x + x^(3/2))")]
        [InlineData("sqrt(x)/sqrt(x + x^2)")]
        [InlineData("x/sqrt(1 + x^2 + (1 + x^2)^(3/2))")]
        public void APowerOfTheRootLeavesTheRadical(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// A polynomial over a square root of something linear. None of these had an
        /// antiderivative, and each is a first-year exercise.
        /// </summary>
        [Theory]
        [InlineData("x/sqrt(1 + x)")]
        [InlineData("x/sqrt(2 - 3*x)")]
        [InlineData("x^2/sqrt(x + 1)")]
        [InlineData("(x + 2)/sqrt(x + 1)")]
        public void APolynomialOverASquareRootOfALinear(string integrand) => DifferentiatesBack(integrand);

        /// <summary>A root other than the square one, where <c>q</c> is the exponent's denominator.</summary>
        [Theory]
        [InlineData("x/(1 + x)^(1/3)")]
        [InlineData("1/(x + 1)^(1/3)")]
        public void ARootOtherThanTheSquare(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// Two radicals over one base, whose exponent denominators are taken together by their
        /// least common multiple, so a single substitution clears both.
        /// </summary>
        [Theory]
        [InlineData("1/(sqrt(x + 1) + (x + 1)^(1/3))")]
        public void TwoRootsOverOneBase(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// Two bases, when every radical is a square root: <c>sqrt(x)/(x sqrt(1 + x))</c> is what
        /// <c>sqrt(1 + tanh(4x))</c> becomes under <c>u = e^(8x)</c>, and with <c>s = sqrt(x)</c>
        /// the other root is <c>sqrt(1 + s^2)</c>, a root of a quadratic, which the rules for
        /// those answer. <c>sqrt(1 - x) + sqrt(1 + x)</c> was pinned here as declined for the
        /// second base; it is answered now, by linearity before this rule and by this rule
        /// when the sum is not at the top. What the rule hands on still has to be answered:
        /// <c>1/(sqrt(1 - x) + sqrt(1 + x))</c> becomes a root of <c>2 - u^2</c>, whose Euler
        /// form has <c>sqrt(2)</c> in its coefficients, and the rational integrator stops at
        /// those; <c>3 + x</c> beside <c>1 - x</c> becomes a root of <c>4 - u^2</c> and comes out.
        /// </summary>
        [Theory]
        [InlineData("sqrt(x)/(x*sqrt(1 + x))")]
        [InlineData("sqrt(x)/sqrt(1 + x)")]
        [InlineData("1/(sqrt(x) + sqrt(x + 1))")]
        [InlineData("1/(sqrt(1 - x) + sqrt(3 + x))")]
        public void ASecondBaseUnderASquareRoot(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// Declined, so the boundary is recorded rather than assumed: three bases, or a cube
        /// root beside a second base, would need more than the one substitution and the
        /// quadratic-radical rules behind it; and two bases that are both negative somewhere
        /// on the reals, where the integrand is real and the answer built through an imaginary
        /// <c>u</c> is not its antiderivative -- <c>sqrt(x - 1) sqrt(x - 2)</c> below <c>1</c>,
        /// measured by quadrature at <c>-3.66</c> against an answer's <c>-4.68</c>. Should any
        /// later be answered by something else, these move rather than being deleted.
        /// </summary>
        /// <summary>
        /// A root of a quotient of two linears is a base of its own kind: under
        /// <c>u = sqrt((1 + x)/(3 + 2x))</c>, <c>x = (3u^2 - 1)/(1 - 2u^2)</c> is rational, and
        /// the answer holds wherever the root is real -- below <c>-3/2</c> too, where both
        /// linears are negative and the root of each is not, which is why the split into a
        /// root over a root is declined for it and rightly. Differentiated back on both sides.
        /// </summary>
        [Theory]
        [InlineData("sqrt((1 + x)/(3 + 2*x))", new[] { -4.0, -2.5, -1.8, -0.5, 0.7, 2.1 })]
        [InlineData("sqrt((x - 1)/(x + 1))/x", new[] { -4.0, -2.5, -1.8, 1.4, 2.6, 5.0 })]
        [InlineData("((2*x + 1)/(x - 3))^(1/3)", new[] { -4.0, -2.5, -1.8, 4.0, 5.5, 9.0 })]
        public void ARootOfAQuotientOfLinears(string integrand, double[] points)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            var derivative = integral.Substitute("C", 0).Differentiate("x");
            var original = integrand.ToEntity();
            foreach (var at in points)
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                var difference = Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart));
                Assert.True(difference / scale < 1e-9,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
        }

        /// <summary>
        /// A whole power below the bar one level down: under <c>u = sqrt(1 - x)</c> this is
        /// <c>2u/(u^7 (u^2 - 1)^5)</c>, and the normalisation writes the power below as
        /// <c>(u^2 - 1)^(5 * (-1))</c>, an exponent that is a product of numbers and not a
        /// number, which the rational readers read evaluated now and read as nothing before.
        /// </summary>
        [Theory]
        [InlineData("1/((1 - x)^(7/2)*x^5)")]
        public void AWholePowerBelowTheBarOneLevelDown(string integrand) => DifferentiatesBack(integrand);

        [Theory]
        [InlineData("1/(sqrt(x) + sqrt(x + 1) + sqrt(x + 2))")]
        [InlineData("x^(1/3)/sqrt(x + 1)")]
        [InlineData("x/(sqrt(x - 1)*sqrt(x - 2))")]
        public void ThreeBasesOrACubeRootBesideASecondAreDeclined(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("u_rad", integral.Stringize());
        }
    }
}
