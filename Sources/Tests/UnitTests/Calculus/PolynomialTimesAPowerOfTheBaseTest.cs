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
    /// <c>N B^r</c>, for a fractional <c>r</c> and an <c>N</c> and a <c>B</c> that are polynomials in
    /// <c>x</c> and the functions of it in them, answered as <c>P(x) B^(r + 1)</c> where there is a
    /// polynomial <c>P</c>: <c>P' B + (r + 1) P B' = N</c> is a linear system in the coefficients of
    /// <c>P</c> once the functions of <c>x</c> are taken for indeterminates, and it is solved exactly.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// Bronstein's <c>(5x^2 + 3 (e^x + x)^(1/3) + e^x (3x + 2x^2))/(x (e^x + x)^(1/3))</c> is
    /// <c>3/x + (5x + e^x (2x + 3))/(e^x + x)^(1/3)</c>, and the second is <c>3x (e^x + x)^(2/3)</c>;
    /// nothing else read it, since the substitution <c>u = e^x + x</c> wants <c>1 + e^x</c> beside
    /// the root and finds <c>5x + e^x (2x + 3)</c>.
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class PolynomialTimesAPowerOfTheBaseTest
    {
        private static Entity DifferentiatesBack(string integrand, double[] points, params (string, double)[] pins)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Assert.DoesNotContain("NaN", integral.Stringize());

            Entity answer = integral.Substitute("C", 0);
            Entity original = integrand.ToEntity();
            foreach (var (name, value) in pins)
            {
                answer = answer.Substitute(name, value);
                original = original.Substitute(name, value);
            }
            // With the symbols pinned the piecewise's conditions are decidable, and the
            // simplification picks the arm before the derivative is taken.
            var derivative = answer.Simplify().Differentiate("x");

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
                Assert.True(difference / scale < 1e-8,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, "
                    + $"where the integrand is {want}");
            }
            Assert.True(compared >= 3,
                $"only {compared} of {points.Length} points were comparable for {integrand}");
            return integral;
        }

        /// <summary>
        /// The exponential, a logarithm and a sine in the base, the root above the bar and
        /// below it, and Bronstein's whole integrand, whose <c>3/x</c> the split takes off first.
        /// </summary>
        [Theory]
        [InlineData("(5*x + e^x*(2*x + 3))/(e^x + x)^(1/3)", new[] { -1.5, -0.4, 0.3, 0.9, 2.6 })]
        [InlineData("(5*x^2 + 3*(e^x + x)^(1/3) + e^x*(3*x + 2*x^2))/(x*(e^x + x)^(1/3))", new[] { 0.3, 0.9, 1.7, 2.6 })]
        [InlineData("(5*x/2 + ln(x) + 3/2)*sqrt(x + ln(x))", new[] { 0.7, 0.9, 1.7, 2.6 })]
        [InlineData("(3*x^3 + 2*x*sin(x) + x^2*cos(x)/2)/sqrt(x^2 + sin(x))", new[] { 0.5, 0.9, 1.7, 2.6 })]
        public void APolynomialTimesTheNextPowerOfTheBase(string integrand, double[] points)
            => DifferentiatesBack(integrand, points);

        /// <summary>
        /// The answer is the one it says: <c>3x (e^x + x)^(2/3)</c>, exactly, not a longer
        /// spelling of it.
        /// </summary>
        [Fact]
        public void TheAnswerIsTheProductItself()
            => Assert.Equal("3 * x * (e ^ x + x) ^ (2/3) + C", "(5*x + e^x*(2*x + 3))/(e^x + x)^(1/3)".ToEntity().Integrate("x").Stringize());

        /// <summary>
        /// What is not of the form is declined by the system, not answered: a numerator one
        /// coefficient off is no polynomial's derivative. Either verdict but a wrong answer.
        /// </summary>
        [Theory]
        [InlineData("(2*x*ln(x) + x + ln(x))*sqrt(x + ln(x))")]
        [InlineData("(3*x^2 + 2*x*sin(x) + x^2*cos(x))/sqrt(x^2 + sin(x))")]
        public void WhatIsNotOfTheFormIsNotAnswered(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("NaN", integral.Stringize());
            if (integral.Stringize().Contains("integral("))
                return;   // unanswered is a legitimate verdict; a wrong answer is not
            DifferentiatesBack(integrand, new[] { 0.7, 0.9, 1.7, 2.6 });
        }
    }
}
