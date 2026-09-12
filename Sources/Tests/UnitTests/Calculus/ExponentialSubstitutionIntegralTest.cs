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
    /// Integrands that are rational in <c>e^(k x)</c>, which <c>u = e^(k x)</c> turns into
    /// rational functions of one variable.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// The hyperbolic functions are here because they are not nodes in this library —
    /// <c>tanh(x)</c> is built as a quotient of exponentials — so this is the rule that
    /// integrates them, and none of the four had an antiderivative before it.
    /// </para>
    /// <para>
    /// Checked by differentiating back and comparing at points, never against a printed form.
    /// These come out in the exponential rather than as <c>ln(cosh(x))</c> or
    /// <c>2 arctan(e^x)</c>, and comparing shapes would fail on answers that are correct.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class ExponentialSubstitutionIntegralTest
    {
        /// <summary>
        /// Real points either side of zero. Every integrand here is defined on the whole real
        /// line except the two with a zero at <c>x = 0</c>, which is not among these.
        /// </summary>
        private static readonly double[] Points = { -0.5, 0.23, 0.61, 1.05, 1.7 };

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
        /// The four hyperbolic functions that had no antiderivative at all, and the two spellings
        /// of the secant, since one is a helper over the other and only one of them is what the
        /// parser produces.
        /// </summary>
        [Theory]
        [InlineData("tanh(x)")]
        [InlineData("coth(x)")]
        [InlineData("sech(x)")]
        [InlineData("csch(x)")]
        [InlineData("1/cosh(x)")]
        [InlineData("tanh(x)^2")]
        public void TheHyperbolicFunctions(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// Quotients written in the exponential directly. <c>e^x/(1 + e^x)</c> was already
        /// answered — its numerator is the derivative of its denominator, which is what the
        /// general substitution looks for — and is here so that the rule cannot break it.
        /// </summary>
        [Theory]
        [InlineData("1/(1 + e^x)")]
        [InlineData("1/(1 - e^x)")]
        [InlineData("1/(e^x - 1)")]
        [InlineData("1/(e^x + e^(-x))")]
        [InlineData("e^x/(1 + e^x)")]
        [InlineData("e^x/(1 + e^(2*x))")]
        public void AQuotientOfExponentials(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// Slopes that differ, taken together by their greatest common divisor so that one
        /// substitution serves the whole integrand.
        /// </summary>
        [Theory]
        [InlineData("1/(1 + e^(2*x))")]
        [InlineData("e^(2*x)/(1 + e^x)")]
        [InlineData("e^(3*x)/(e^x + 1)")]
        [InlineData("1/(e^(2*x) - e^(4*x))")]
        public void SlopesTakenTogether(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// A fractional slope: the base is <c>e^(k x)</c> with <c>k</c> the greatest common
        /// divisor of the slopes as rationals, so <c>e^(x/2)</c> beside <c>e^x</c> is read as
        /// <c>u</c> beside <c>u^2</c>. Timofeev's <c>e^(x/2)/sqrt(e^x - 1)</c> is
        /// <c>2/sqrt(u^2 - 1)</c> that way, and was declined for the half.
        /// </summary>
        [Theory]
        [InlineData("e^(x/2)/sqrt(e^x - 1)")]
        [InlineData("e^(x/3)/(1 + e^x)")]
        [InlineData("1/(e^(x/2) + e^(x/3))")]
        public void AFractionalSlope(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// The sign of the base is chosen for the radicals: with every exponential under a root
        /// of negative slope, <c>u = e^(-x)</c> makes <c>sqrt(1 + e^(-x))</c> a root of something
        /// linear in <c>u</c>, where <c>u = e^x</c> would make it a root of a quotient that
        /// nothing rationalises. Bondarenko's two.
        /// </summary>
        [Theory]
        [InlineData("sqrt(1 + e^(-x))/(e^x - e^(-x))")]
        [InlineData("sqrt(1 + e^(-x))/sinh(x)")]
        [InlineData("sqrt(1 + e^x)")]
        public void TheBaseTakesTheSignOfTheRadicand(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// <c>u = e^(k x)</c> is positive, and a root holding a power of it gives that power up:
        /// <c>sqrt(e^(2x) + e^(3x))</c> is <c>sqrt(u^2 (1 + u))</c> under <c>u = e^x</c>, and the
        /// simplifier is right not to call that <c>u sqrt(1 + u)</c> for a <c>u</c> it knows
        /// nothing about -- the substitution knows exactly this about its own. The identity
        /// <c>(a b)^r = a^r b^r</c> is exact whenever <c>a</c> is a non-negative real.
        /// </summary>
        [Theory]
        [InlineData("sqrt(e^(2*x) + e^(3*x))")]
        [InlineData("e^x/sqrt(e^(2*x) - e^x)")]
        [InlineData("e^(2*x)/sqrt(e^(4*x) + e^(2*x))")]
        [InlineData("sqrt(1 + tanh(4*x))")]
        public void APowerOfThePositiveBaseLeavesTheRoot(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// What the rule must not disturb: exponentials the other rules already answer, and which
        /// are not rational in <c>e^(k x)</c> at all.
        /// </summary>
        [Theory]
        [InlineData("e^x")]
        [InlineData("x*e^x")]
        [InlineData("e^(2*x)")]
        public void WhatWasAlreadyAnswered(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// Declined, so the boundary is recorded rather than assumed. <c>e^(x^2)</c> has no
        /// elementary antiderivative at all, and its exponent is not linear, which is the check
        /// that stops it — a rewrite would leave an <c>x</c> standing beside the new variable.
        /// Should it later be answered by something else, this moves rather than being deleted.
        /// </summary>
        [Theory]
        [InlineData("e^(x^2)")]
        [InlineData("e^(1/x)")]
        public void WhatIsNotRationalInOneExponentialIsDeclined(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("u_exp", integral.Stringize());
        }
    }
}
