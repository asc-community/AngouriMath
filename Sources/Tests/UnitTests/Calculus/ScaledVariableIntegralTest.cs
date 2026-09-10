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
    /// Integrands carrying one symbolic parameter, which <c>x = c t</c> scales away.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>1/(8 + x^3)</c> is answered at once and <c>1/(a^3 + x^3)</c> was not, which is the whole
    /// shape of the gap: the rational rules read a denominator as a polynomial <b>over the
    /// rationals</b>, and <c>a^3</c> is not a rational coefficient. Scaling by the parameter puts
    /// integer coefficients back and the parameter comes out as a constant factor.
    /// </para>
    /// <para>
    /// The parameter is pinned to a value for the numeric comparison only; every antiderivative
    /// here was found symbolically, in <c>a</c>.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class ScaledVariableIntegralTest
    {
        /// <summary>
        /// Away from zero on both sides, since several of these have a pole at the origin.
        /// </summary>
        private static readonly double[] Points = { -0.5, 0.23, 0.61, 1.05, 2.3 };

        private static void DifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());

            var derivative = integral.Substitute("C", 0).Differentiate("x")
                .Substitute("a", 1.7).Substitute("b", 0.9);
            var original = integrand.ToEntity().Substitute("a", 1.7).Substitute("b", 0.9);
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

        /// <summary>A sum or difference of like powers, with the parameter in one of them.</summary>
        [Theory]
        [InlineData("1/(a^3 + x^3)")]
        [InlineData("x/(a^3 + x^3)")]
        [InlineData("1/(a^4 - x^4)")]
        [InlineData("x^2/(a^4 + x^4)")]
        public void ASumOfLikePowers(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// The same over a power of the variable, which is where the family runs long in Rubi's
        /// suites — each of these was unevaluated.
        /// </summary>
        [Theory]
        [InlineData("1/(x*(a^3 + x^3))")]
        [InlineData("1/(x^2*(a^3 + x^3))")]
        [InlineData("1/(x^3*(a^3 + x^3))")]
        [InlineData("1/(x^4*(a^3 + x^3))")]
        [InlineData("1/(x^5*(a^3 + x^3))")]
        [InlineData("1/(x*(a^4 - x^4))")]
        [InlineData("1/(x^2*(a^4 - x^4))")]
        [InlineData("1/(x^3*(a^4 - x^4))")]
        [InlineData("1/(x^4*(a^4 - x^4))")]
        public void OverAPowerOfTheVariable(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// A constant factor inside the denominator, which the two branches taking a factor out
        /// of a quotient both missed because neither side was wholly free of the variable.
        /// </summary>
        [Theory]
        [InlineData("1/(a*(1 + x^3))")]
        [InlineData("1/(a^2*(1 + x^3))")]
        [InlineData("1/(2*a*(1 + x^4))")]
        [InlineData("x/(a*(1 + x^3))")]
        [InlineData("1/(a*(1 + x^2))")]
        public void AConstantFactorInsideTheDenominator(string integrand)
            => DifferentiatesBack(integrand);

        /// <summary>
        /// What was already answered and has to stay answered. The scaling rule fires on
        /// integrands nothing is wrong with, so it runs last and these prove it does not get in
        /// front of anything.
        /// </summary>
        [Theory]
        [InlineData("a*sin(x)")]
        [InlineData("a*x^2")]
        [InlineData("a/x")]
        [InlineData("sin(a*x)")]
        [InlineData("x*e^(a*x)")]
        [InlineData("1/(1 + x^3)")]
        public void WhatWasAlreadyAnswered(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// Two parameters is a decline, and deliberately: scaling by one leaves the other, so the
        /// sub-problem would still carry a parameter and could be scaled again without end. The
        /// homogeneity check is what stops it, and this is the case that exercises the check
        /// rather than the substitution.
        /// </summary>
        [Theory]
        [InlineData("x/((a^2 + x^2)*(b^2 + x^2))")]
        [InlineData("x^2/((a^2 + x^2)*(b^2 + x^2))")]
        public void TwoParametersAreDeclined(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("u_scale", integral.Stringize());
        }

        /// <summary>
        /// An integrand that is not homogeneous in the parameter, so the scale does not separate.
        /// Declined for the same reason, and this is the case where the substitution applies
        /// cleanly and the check still has to refuse it.
        /// </summary>
        [Theory]
        [InlineData("1/(a + x^3)")]
        [InlineData("1/(a^2 + x^3)")]
        public void AnInhomogeneousParameterIsDeclined(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            // Whatever it answers must not be built on a substitution that does not hold.
            if (!integral.Stringize().Contains("integral("))
                DifferentiatesBack(integrand);
        }
    }
}
