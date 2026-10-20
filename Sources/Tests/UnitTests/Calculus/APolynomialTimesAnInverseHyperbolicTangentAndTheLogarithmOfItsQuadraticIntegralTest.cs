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
    /// A polynomial times an inverse hyperbolic tangent or cotangent of <c>c x</c> and the logarithm of
    /// <c>1 - c^2 x^2</c>, in closed form: Rubi's 7.3.4 and 7.4.1
    /// <c>x^m (a + b artanh(c x))(d + e ln(1 - c^2 x^2))</c>, which timed out.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class APolynomialTimesAnInverseHyperbolicTangentAndTheLogarithmOfItsQuadraticIntegralTest
    {
        private static void DifferentiatesBack(string integrand, params (string, double)[] pins)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 5000, $"{text.Length} characters of answer for {integrand}");

            var pinned = integral.Substitute("C", 0);
            Entity original = integrand.ToEntity();
            foreach (var (name, value) in pins)
            {
                pinned = pinned.Substitute(name, value);
                original = original.Substitute(name, value);
            }
            var derivative = pinned.Differentiate("x");
            var compared = 0;
            foreach (var at in new[] { -2.3, -0.7, 0.4, 1.1, 1.9, 3.2 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                if (want.IsNaN)
                    continue;
                var got = derivative.Substitute("x", at).EvalNumerical();
                var difference = Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart));
                Assert.True(difference / scale < 1e-8,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
                compared++;
            }
            Assert.True(compared >= 5, $"only {compared} points could be compared for {integrand}");
        }

        [Theory]
        [InlineData("x^4*(a+b*atanh(c*x))*(d+k*ln(1-c^2*x^2))")]
        [InlineData("x^3*(a+b*atanh(c*x))*(d+k*ln(1-c^2*x^2))")]
        [InlineData("x^2*(a+b*atanh(c*x))*(d+k*ln(1-c^2*x^2))")]
        [InlineData("x*(a+b*atanh(c*x))*(d+k*ln(1-c^2*x^2))")]
        [InlineData("(a+b*atanh(c*x))*(d+k*ln(1-c^2*x^2))")]
        [InlineData("x^5*(a+b*acoth(c*x))*(d+k*ln(1-c^2*x^2))")]
        [InlineData("x^4*(a+b*acoth(c*x))*(d+k*ln(1-c^2*x^2))")]
        [InlineData("x^3*(a+b*acoth(c*x))*(d+k*ln(1-c^2*x^2))")]
        [InlineData("x^2*(a+b*acoth(c*x))*(d+k*ln(1-c^2*x^2))")]
        [InlineData("x*(a+b*acoth(c*x))*(d+k*ln(1-c^2*x^2))")]
        [InlineData("(x^2+1)*atanh(c*x)*ln(1-c^2*x^2)")]
        [InlineData("x^7*atanh(c*x)*ln(1-c^2*x^2)")]
        [InlineData("atanh(c*x)*ln(2-2*c^2*x^2)")]
        public void IsAnsweredAndDifferentiatesBack(string integrand)
        {
            DifferentiatesBack(integrand, ("a", 1.3), ("b", 0.7), ("c", 0.3), ("d", 2), ("k", -0.4));
            DifferentiatesBack(integrand, ("a", -2), ("b", 1.5), ("c", -1.2), ("d", 0.5), ("k", 3));
        }
    }
}
