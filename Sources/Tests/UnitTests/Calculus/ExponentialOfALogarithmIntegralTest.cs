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
    /// <c>e^(k ln(q))</c> is <c>q^k</c>, the definition of the principal power, and that is the
    /// spelling the parser gives every inverse hyperbolic function: <c>acoth(a x)</c> is
    /// <c>1/2 ln((a x + 1)/(a x - 1))</c>. So <c>e^acoth(a x) x^3</c> arrives as an exponential
    /// of a logarithm, which no exponential rule reads, and is <c>x^3 sqrt((a x + 1)/(a x - 1))</c>,
    /// which the radical substitution answers. Rubi's 7.4.2, exponentials of the inverse
    /// hyperbolic cotangent, where every row was declined.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ExponentialOfALogarithmIntegralTest
    {
        /// <summary>Beyond <c>a x = 1</c> for <c>a = 2</c>, where <c>acoth(2x)</c> is real.</summary>
        private static readonly double[] Points = { 0.6, 0.8, 1.1, 1.5, 2.2 };

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
                var difference = Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart));
                Assert.True(difference / scale < 1e-9,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
            Assert.True(compared >= 4, $"only {compared} of {Points.Length} points were comparable for {integrand}");
        }

        [Theory]
        [InlineData("e^(2*acoth(2*x))*(3 - 4*3*x^2)^2")]
        [InlineData("e^acoth(2*x)/(3 - 3/(2*x))")]
        [InlineData("e^(1/3*acoth(x))*x^2")]
        [InlineData("(3 - 3/(4*x^2))^3/e^(2*acoth(2*x))")]
        public void AnExponentialOfAnInverseHyperbolicCotangent(string integrand) => DifferentiatesBack(integrand);

        /// <summary>The fold is the identity it is: <c>e^(k ln q)</c> with any multiplier, nested or not.</summary>
        [Theory]
        [InlineData("e^(3*ln(x + 1))")]
        [InlineData("e^(ln(x^2 + 1)/2) * x")]
        [InlineData("e^(2*(1/2*ln(x + 2)))")]
        public void AnExponentialOfAMultipleOfALogarithm(string integrand) => DifferentiatesBack(integrand);
    }
}
