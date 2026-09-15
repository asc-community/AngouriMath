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
    /// Bioche's first two rules: a rational function of <c>sin(x)</c> and <c>cos(x)</c> that is
    /// odd in the sine is a rational function of <c>u = cos(x)</c> with <c>sin(x) dx = -du</c>,
    /// and one odd in the cosine the mirror of it -- in front of the half-angle substitution,
    /// which answers any rational function of the two at greater length.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// Bondarenko's <c>1/(cos(x) + cos(3x))^5</c> is <c>1/(4 cos(x)^3 - 2 cos(x))^5</c>, odd in the
    /// cosine; under <c>u = sin(x)</c> it is a rational function with a denominator of degree
    /// sixteen, where the half-angle tangent makes one of degree thirty and did not return
    /// within the budget. Every answer is differentiated back and compared to the integrand
    /// numerically.
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class BiocheOddSubstitutionTest
    {
        private static readonly double[] Points = { 0.3, 0.9, 1.7, 2.6, -0.7 };

        private static Entity DifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Assert.DoesNotContain("NaN", integral.Stringize());

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
                var difference = Math.Abs((double)(got - want).RealPart)
                               + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart));
                Assert.True(difference / scale < 1e-8,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, "
                    + $"where the integrand is {want}");
            }
            Assert.True(compared >= 3,
                $"only {compared} of {Points.Length} points were comparable for {integrand}");
            return integral;
        }

        /// <summary>
        /// Odd in the sine, odd in the cosine, with a denominator that is even, odd, or
        /// neither in the function substituted for -- each a case of its own in the reading
        /// -- and with the other four functions in the writing.
        /// </summary>
        [Theory]
        [InlineData("sin(x)/(1 + cos(x)^2)")]
        [InlineData("cos(x)/(2 + sin(x))")]
        [InlineData("sin(x)^3/(1 + cos(x))")]
        [InlineData("1/(sin(x)*(2 + cos(x)))")]
        [InlineData("tan(x)/(1 + cos(x))")]
        [InlineData("cos(x)^3/(sin(x)^2 + sin(x)^4)")]
        [InlineData("sec(x)*tan(x)/(3 + sec(x))")]
        [InlineData("1/(4*cos(x)^3 - 2*cos(x))^5")]
        public void OddInOneOfTheTwo(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// With a radical of a polynomial in the two, even in the function the integrand is
        /// odd in, admitted as a coefficient: Moses's <c>sqrt(A^2 + B^2 sin(x)^2)/sin(x)</c>
        /// with <c>A</c> and <c>B</c> pinned, and the numeric spelling of it.
        /// </summary>
        [Theory]
        [InlineData("sqrt(3 + 2*sin(x)^2)/sin(x)")]
        [InlineData("sqrt(1.7^2 + 2.3^2*sin(x)^2)/sin(x)")]
        [InlineData("sin(x)^3*sqrt(1 + cos(x)^2)")]
        [InlineData("cos(x)/sqrt(1 + sin(x)^2)")]
        public void ARadicalEvenInTheFunction(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// The answer is in the function substituted for and not in the half-angle tangent,
        /// which is the point of the rule's place in front of that substitution.
        /// </summary>
        [Theory]
        [InlineData("sin(x)/(1 + cos(x)^2)", "arctan(")]
        [InlineData("cos(x)/(2 + sin(x))", "ln(sin(x) + 2)")]
        public void TheAnswerIsInTheOtherFunction(string integrand, string expected)
        {
            var integral = DifferentiatesBack(integrand).Stringize();
            Assert.Contains(expected, integral);
            Assert.DoesNotContain("tan(x / 2)", integral);
        }

        /// <summary>
        /// Even in both, or with x bare beside the two: not this rule's, and the half-angle
        /// substitution or the rest of the chain has them as before.
        /// </summary>
        [Theory]
        [InlineData("1/(2 + cos(x))")]
        [InlineData("1/(sin(x) + cos(x))")]
        [InlineData("x*sin(x)")]
        public void TheNeighboursAreUntouched(string integrand) => DifferentiatesBack(integrand);
    }
}
