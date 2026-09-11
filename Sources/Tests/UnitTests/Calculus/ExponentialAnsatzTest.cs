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
    /// <c>e^h R</c>, with <c>h</c> and <c>R</c> rational in the variable, integrated by the
    /// ansatz <c>F = e^h N/D</c>; and with no exponential, the Hermite reduction
    /// <c>R = (N/D)' + M/D_1</c> for a denominator with a repeated factor.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Liouville's theorem says the elementary antiderivative of <c>e^h R</c>, where there is
    /// one, is <c>e^h</c> times a rational function; so an ansatz of that shape, solved as a
    /// linear system in the coefficients of <c>N</c>, finds the antiderivative or shows there
    /// is none of that shape. <c>e^x x/(1 + x)^2</c>, <c>e^(x^2)(1 + 2x^2)</c> and
    /// <c>(4x^5 - 1)/(1 + x + x^5)^2</c> had no antiderivative; each is the derivative of
    /// something of its own shape.
    /// </para>
    /// <para>
    /// Every answer is differentiated back and compared to the integrand numerically; the
    /// non-elementary neighbours — <c>e^x/x</c>, <c>e^(x^2)</c> — are pinned as declined, since
    /// answering them would be a wrong answer and not a missing one.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class ExponentialAnsatzTest
    {
        private static readonly double[] Points = { 0.3, 0.9, 1.7, 2.6 };

        private static void DifferentiatesBack(string integrand)
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
                Assert.True(difference / scale < 1e-9,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, "
                    + $"where the integrand is {want}");
            }
            Assert.True(compared >= 3,
                $"only {compared} of {Points.Length} points were comparable for {integrand}");
        }

        /// <summary>
        /// An exponential of the variable, of its square, of its reciprocal and of a rational
        /// function, each times a rational function: Moses's, Hearn's and Hebisch's from the
        /// Rubi suite. The sum is Moses's too, and is read whole.
        /// </summary>
        [Theory]
        [InlineData("e^x*x/(1 + x)^2")]
        [InlineData("e^x*(x^2 + 1)/(x + 1)^2")]
        [InlineData("e^(x^2)*(1 + 2*x^2)")]
        [InlineData("e^(x^2) + 2*e^(x^2)*x^2")]
        [InlineData("e^(1/x)*(1 + x)/x^4")]
        [InlineData("(1 - 3*x - x^2 + x^3)*e^(1/(x^2 - 1))/(1 - x - x^2 + x^3)")]
        [InlineData("e^(-x)/(1 - x)^2 - e^(-x)/(1 - x)")]
        public void AnExponentialTimesARationalFunction(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// The exact forms, so that the ansatz is seen to find the short answer and not a
        /// longer equivalent.
        /// </summary>
        [Theory]
        [InlineData("e^x*x/(1 + x)^2", "e ^ x / (1 + x) + C")]
        [InlineData("(-1 + 4*x^5)/(1 + x + x^5)^2", "-x / (1 + x + x ^ 5) + C")]
        public void TheAnswerIsTheShortOne(string integrand, string expected)
            => Assert.Equal(expected, integrand.ToEntity().Integrate("x").Stringize());

        /// <summary>
        /// No exponential: the Hermite reduction for a repeated factor, whose logarithmic part
        /// is then the splits' to answer. Apostol's, Stewart's and two of Timofeev's; the last
        /// took seventeen seconds of peeling and re-factoring to reach the same answer before.
        /// </summary>
        [Theory]
        [InlineData("(-1 + 4*x^5)/(1 + x + x^5)^2")]
        [InlineData("(1 + x^2 + x^4)/((1 + x^2)*(4 + x^2)^2)")]
        [InlineData("x^5/(1 + x^4)^3")]
        [InlineData("(1 + x^2)/(x*(1 + x^3)^2)")]
        [InlineData("1/((x + 1)^3*(x + 2)^4)")]
        public void TheHermiteReduction(string integrand) => DifferentiatesBack(integrand);

        [Fact]
        public void TheHermiteReductionIsQuick()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            DifferentiatesBack("(1 + x^2)/(x*(1 + x^3)^2)");
            Assert.True(watch.Elapsed < TimeSpan.FromSeconds(10), $"took {watch.Elapsed}");
        }

        /// <summary>
        /// What has no elementary antiderivative is declined, not answered: the ansatz finds
        /// no <c>N</c>, which by Liouville is the proof.
        /// </summary>
        [Theory]
        [InlineData("e^x/x")]
        [InlineData("e^(x^2)")]
        [InlineData("e^x/(1 + x)")]
        [InlineData("e^(1/x)")]
        public void ANonElementaryOneIsDeclined(string integrand)
            => Assert.Contains("integral(", integrand.ToEntity().Integrate("x").Stringize());

        /// <summary>
        /// What by parts answered before keeps its form: a polynomial times an exponential is
        /// still integrated by parts, since the ansatz runs after it.
        /// </summary>
        [Fact]
        public void APolynomialTimesAnExponentialKeepsItsForm()
            => Assert.Equal("x * e ^ x + -e ^ x + C", "x*e^x".ToEntity().Integrate("x").Stringize());
    }
}
