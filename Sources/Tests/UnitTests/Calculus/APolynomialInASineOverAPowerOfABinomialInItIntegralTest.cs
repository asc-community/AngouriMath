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
    /// A polynomial in a sine or a cosine over a whole power of a binomial in it, written in powers of
    /// the binomial: Rubi's 4.1.4 and 4.2.4 <c>(a + b cos)^m (A + B cos + C cos^2)</c> at <c>m = -4</c>,
    /// which timed out. Pinned on the real domain.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class APolynomialInASineOverAPowerOfABinomialInItIntegralTest
    {
        private static void DifferentiatesBack(string integrand, params (string, double)[] pins)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());

            var pinned = integral.Substitute("C", 0);
            Entity original = integrand.ToEntity();
            foreach (var (name, value) in pins)
            {
                pinned = pinned.Substitute(name, value);
                original = original.Substitute(name, value);
            }
            var derivative = pinned.Differentiate("x");
            foreach (var at in new[] { -2.3, -0.7, 0.4, 1.9 })
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                var difference = Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart));
                Assert.True(difference / scale < 1e-8,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
        }

        // The parameters are p, q, r rather than Rubi's A, B, C: the helper takes C for the constant.
        [Theory]
        [InlineData("(p+q*cos(c+d*x)+r*cos(c+d*x)^2)/(a+b*cos(c+d*x))^4")]
        [InlineData("cos(c+d*x)^3*(p+q*cos(c+d*x)+r*cos(c+d*x)^2)/(a+b*cos(c+d*x))^4")]
        [InlineData("cos(c+d*x)^2*(p+r*cos(c+d*x)^2)/(a+b*cos(c+d*x))^4")]
        [InlineData("(p+q*sin(c+d*x)+r*sin(c+d*x)^2)/(a+b*sin(c+d*x))^3")]
        public void IsAnsweredAndDifferentiatesBack(string integrand)
        {
            DifferentiatesBack(integrand, ("a", 1.3), ("b", 0.7), ("c", 0.7), ("d", 1.07), ("p", 0.43), ("q", -0.45), ("r", 0.22));
            DifferentiatesBack(integrand, ("a", 2), ("b", -1.5), ("c", -0.3), ("d", 0.6), ("p", -1), ("q", 2), ("r", 0.5));
        }

        /// <summary>
        /// An even power of the other function is a power of <c>1 - f^2</c>, so the numerator is a
        /// polynomial in the one: Rubi's 4.1.2.2 <c>cos(c + d x)^6 sin(c + d x)^3/(a + b sin(c + d x))^2</c>
        /// and its kin, which timed out.
        /// </summary>
        [Theory]
        [InlineData("cos(c+d*x)^4*sin(c+d*x)^3/(a+b*sin(c+d*x))^2")]
        [InlineData("cos(c+d*x)^4*sin(c+d*x)^3/(a+b*sin(c+d*x))^3")]
        [InlineData("cos(c+d*x)^6*sin(c+d*x)^2/(a+b*sin(c+d*x))^2")]
        [InlineData("cos(c+d*x)^6*sin(c+d*x)^3/(a+b*sin(c+d*x))^3")]
        [InlineData("sin(c+d*x)^2*cos(c+d*x)^2/(a+b*cos(c+d*x))^2")]
        public void WithAnEvenPowerOfTheOther(string integrand)
        {
            DifferentiatesBack(integrand, ("a", 1.3), ("b", 0.7), ("c", 0.7), ("d", 1.07));
            DifferentiatesBack(integrand, ("a", 2), ("b", -1.5), ("c", -0.3), ("d", 0.6));
        }
    }
}
