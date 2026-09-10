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
    /// A quotient by an exponential is the product with its reciprocal, and only that spelling
    /// reached integration by parts.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>sin(x) * e^(-x)</c> had an antiderivative and <c>sin(x)/e^x</c> did not — the same
    /// integrand, differing only in which node is on top. Integration by parts matches a
    /// <c>Mulf</c> and nothing else, and the simplifier keeps <c>sin(x)/e^x</c> as written, so
    /// nothing closed the gap.
    /// </para>
    /// <para>
    /// The two spellings are asserted to agree numerically rather than to print alike, since
    /// which form an antiderivative comes out in says nothing about whether it is one.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class DivisionByAnExponentialTest
    {
        private static readonly double[] Points = { 0.23, 0.61, 1.05, 1.7 };

        private static void DifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var written = integral.Stringize();
            Assert.DoesNotContain("integral(", written);
            Assert.DoesNotContain("NaN", written);

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
            Assert.True(compared >= 3,
                $"only {compared} of {Points.Length} points were comparable for {integrand}");
        }

        /// <summary>
        /// The family, which is wider than the trigonometric case that exposed it — a polynomial
        /// numerator and a base other than <c>e</c> were declined for the same reason.
        /// </summary>
        [Theory]
        [InlineData("sin(x)/e^x")]
        [InlineData("cos(x)/e^x")]
        [InlineData("x/e^x")]
        [InlineData("x^3/e^x")]
        [InlineData("x^2/e^(2*x)")]
        [InlineData("sin(2*x)/e^(3*x)")]
        [InlineData("sin(x)/2^x")]
        public void AQuotientByAnExponential(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// The product spelling of the same integrand, which was always answered and must go on
        /// being answered by the same route.
        /// </summary>
        [Theory]
        [InlineData("sin(x)*e^(-x)")]
        [InlineData("e^x*sin(x)")]
        [InlineData("x*e^x")]
        public void TheProductSpellingIsUnaffected(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// The two spellings agree, which is the property this is about rather than any
        /// particular antiderivative.
        /// </summary>
        [Fact]
        public void TheTwoSpellingsAgree()
        {
            var overExponential = "sin(x)/e^x".ToEntity().Integrate("x").Substitute("C", 0);
            var timesReciprocal = "sin(x)*e^(-x)".ToEntity().Integrate("x").Substitute("C", 0);
            foreach (var at in Points)
            {
                var one = overExponential.Substitute("x", at).EvalNumerical();
                var other = timesReciprocal.Substitute("x", at).EvalNumerical();
                // Antiderivatives may differ by a constant, so the difference is what is compared
                // against its value at another point rather than against zero.
                var here = (double)(one - other).RealPart;
                var there = (double)(overExponential.Substitute("x", 0.5).EvalNumerical()
                                     - timesReciprocal.Substitute("x", 0.5).EvalNumerical()).RealPart;
                Assert.True(Math.Abs(here - there) < 1e-9,
                    $"the two spellings differ by {here} at x = {at} and by {there} at x = 0.5, "
                    + "so they are not the same antiderivative up to a constant");
            }
        }

        /// <summary>
        /// A rational denominator must not be turned into a negative power: <c>x^n</c> under a
        /// quotient is partial fractions' to read as written, and rewriting it would take these
        /// away from the rules that answer them.
        /// </summary>
        [Theory]
        [InlineData("1/(1 + x^3)")]
        [InlineData("x/(1 + x^2)")]
        [InlineData("x^2/(x + 1)")]
        [InlineData("1/x^2")]
        [InlineData("x/(x^4 + 1)")]
        public void ARationalDenominatorIsLeftAlone(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// Still declined, and rightly: <c>∫e^(-x)/x dx</c> is the exponential integral and
        /// <c>∫atan(x) e^(-x) dx</c> is likewise not elementary, so what is left after parts has
        /// no antiderivative to find. Recorded so that a change making them answer is noticed.
        /// </summary>
        [Theory]
        [InlineData("ln(x)/e^x")]
        [InlineData("atan(x)/e^x")]
        public void WhatIsNotElementaryIsStillDeclined(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            if (!integral.Stringize().Contains("integral("))
                DifferentiatesBack(integrand);
        }
    }
}
