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
    /// A product of powers times a sum that is the derivative of the product with some of
    /// the powers raised by one, read off the sum: <c>e^x x^2 ln(x)^2 (3 + (3 + x) ln(x))</c>
    /// is <c>(e^x x^3 ln(x)^3)'</c>, for symbolic exponents too.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// The product rule on <c>G = K prod f_i^(e_i)</c> gives <c>G' = G sum e_i f_i'/f_i</c>, so
    /// the sum in such an integrand names the powers to raise; every subset of them is tried,
    /// the quotient of the integrand by the candidate's derivative must be a constant, decided
    /// at sampled points and then read off one monomial, and the answer is differentiated
    /// back. Nothing else reads a symbolic exponent, and splitting the sum loses it, since
    /// <c>x^m ln(x)^n</c> on its own is not elementary.
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class ProductOfPowersDerivativeTest
    {
        private static readonly double[] Points = { 0.3, 0.9, 1.7, 2.6 };

        /// <summary>
        /// <see cref="DifferentiatesBack"/> with every symbol but <c>x</c> pinned to a distinct
        /// value off the integers, so that a symbolic base or slope is a number on both sides.
        /// </summary>
        private static void DifferentiatesBackWithParametersPinned(string integrand, double[] points)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Assert.DoesNotContain("NaN", integral.Stringize());
            var derivative = integral.Substitute("C", 0).Differentiate("x");
            var original = integrand.ToEntity();
            var pinned = 0;
            foreach (var parameter in original.Vars)
            {
                if (parameter.Name == "x")
                    continue;
                var value = new[] { 1.3, 2.1, 0.7, 1.9, 3.1, 0.4 }[pinned++ % 6] + pinned / 6;
                derivative = derivative.Substitute(parameter, value);
                original = original.Substitute(parameter, value);
            }
            var compared = 0;
            foreach (var at in points)
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                if (got.IsNaN || want.IsNaN)
                    continue;
                compared++;
                var difference = Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart));
                Assert.True(difference / scale < 1e-9,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
            Assert.True(compared >= 3, $"only {compared} of {points.Length} points were comparable for {integrand}");
        }

        /// <summary>
        /// Rubi's, with symbols for the exponents: <c>x^m ln(x)^n (1 + n + (1 + m) ln x)</c> is
        /// <c>(x^(m + 1) ln(x)^(n + 1))'</c>, and
        /// <c>F^(c (a + b x)) x^m ln(d x)^n (p + p n + p (1 + m + b c x ln F) ln(d x))</c> is
        /// <c>(p F^(c (a + b x)) x^(m + 1) ln(d x)^(n + 1))'</c>, with a power of <c>x</c> below
        /// the bar, or raised to nothing, in the others.
        /// </summary>
        [Theory]
        [InlineData("x^m*ln(x)^n*(1 + n + (1 + m)*ln(x))")]
        [InlineData("F^(c*(a + b*x))*x^m*ln(d*x)^n*(p + p*n + p*(1 + m + b*c*x*ln(F))*ln(d*x))")]
        [InlineData("F^(c*(a + b*x))*ln(d*x)^n*(p + p*n + b*c*p*x*ln(F)*ln(d*x))/x")]
        [InlineData("F^(c*(a + b*x))*ln(d*x)^n*(p + p*n + p*(-2 + b*c*x*ln(F))*ln(d*x))/x^3")]
        public void ASymbolicExponentOnEachPower(string integrand)
            => DifferentiatesBackWithParametersPinned(integrand, Points);

        /// <summary>Whole exponents, which the tower ansätze do not read either: the sum is not elementary apart.</summary>
        [Theory]
        [InlineData("e^x*x^2*ln(x)^2*(3 + (3 + x)*ln(x))")]
        [InlineData("F^(c*(a + b*x))*x^2*ln(d*x)^2*(3 + (3 + b*c*x*ln(F))*ln(d*x))")]
        public void AWholeExponentOnEachPower(string integrand)
            => DifferentiatesBackWithParametersPinned(integrand, Points);

        /// <summary>
        /// The constant in front is read off the answer, not assumed: <c>p</c> above, and here
        /// a symbol that does not cancel.
        /// </summary>
        [Fact]
        public void TheConstantIsRead()
            => Assert.Equal("p * x ^ (m + 1) * ln(x) ^ (n + 1) + C",
                "p*x^m*ln(x)^n*(1 + n + (1 + m)*ln(x))".ToEntity().Integrate("x").Stringize());

        /// <summary>
        /// A sum that is not the derivative's is declined, not answered: <c>x^m ln(x)^n (1 + ln x)</c>
        /// with symbols for <c>m</c> and <c>n</c> is not elementary.
        /// </summary>
        [Fact]
        public void WhatIsNotADerivativeIsDeclined()
            => Assert.Contains("integral(", "x^m*ln(x)^n*(1 + ln(x))".ToEntity().Integrate("x").Stringize());
    }
}
