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
    /// A polynomial times powers of polynomials, one at least not whole, whose antiderivative is
    /// another polynomial times each power raised by one: Rubi's
    /// <c>(a g + e x - b g x^4)/(a + b x^4)^(3/2)</c> and
    /// <c>(d + f x^2)^p (2 c d f + 2 b f^2 (3 + 2 p) x + 2 c f^2 (3 + 2 p) x^2)</c>, which were declined.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class APolynomialTimesPowersOfPolynomialsByRaisingThePowersIntegralTest
    {
        [Theory]
        // Rubi's 1.1.3.8 and 1.2.2.5: a quartic under the root.
        [InlineData("(a*g + k*x - b*g*x^4)/(a + b*x^4)^(3/2)")]
        [InlineData("(a*g + f*x^3 - b*g*x^4)/(a + b*x^4)^(3/2)")]
        [InlineData("(a*g + k*x + f*x^3 - b*g*x^4)/(a + b*x^4)^(3/2)")]
        [InlineData("(a*g + k*x - c*g*x^4)/(a + b*x^2 + c*x^4)^(3/2)")]
        [InlineData("(a*g + f*x^3 - c*g*x^4)/(a + b*x^2 + c*x^4)^(3/2)")]
        [InlineData("(a*g + k*x + f*x^3 - c*g*x^4)/(a + b*x^2 + c*x^4)^(3/2)")]
        // Rubi's 1.2.1.9: a symbolic exponent.
        [InlineData("(d + f*x^2)^p*(2*c*d*f + 2*b*f^2*(3 + 2*p)*x + 2*c*f^2*(3 + 2*p)*x^2)")]
        [InlineData("(d + k*x + f*x^2)^p*(-2*c*k^2 + 2*c*d*f - c*k^2*p + 2*c*f^2*(3 + 2*p)*x^2)")]
        [InlineData("(d + k*x + f*x^2)^p*(-2*c*k^2 + 2*c*d*f + 3*b*k*f - c*k^2*p + 2*b*k*f*p + 2*b*f^2*(3 + 2*p)*x + 2*c*f^2*(3 + 2*p)*x^2)")]
        // Rubi's 1.1.2.3: a cube root.
        [InlineData("(3*a + b*x^2)^2/(a - b*x^2)^(7/3)")]
        // Rubi's 1.1.1.3: two linears, each raised.
        [InlineData("x^2*(1 - a*x)^(-1 - 1/2*n*(1 + n))*(1 + a*x)^(-1 - 1/2*(-1 + n)*n)")]
        public void AsAPolynomialTimesThePowersRaised(string integrand) => AssertAnAntiderivative(integrand);

        [Theory]
        // Rubi's 1.1.3.8 and 1.2.3.5: the same under u = x^(n/4) or u = x^(n/2), which the
        // substitution for a symbolic power of x asks.
        [InlineData("(-a*h*x^(-1 + 1/4*n) + b*f*x^(-1 + 1/2*n) + b*g*x^(-1 + n) + b*h*x^(-1 + 5/4*n))/(a + b*x^n)^(3/2)")]
        [InlineData("(-a*h*x^(-1 + 1/2*n) + c*f*x^(-1 + n) + c*g*x^(-1 + 2*n) + c*h*x^(-1 + 5/2*n))/(a + b*x^n + c*x^(2*n))^(3/2)")]
        [InlineData("x^(-1 + 1/4*n)*(-a*h + c*f*x^(1/4*n) + c*g*x^(3/4*n) + c*h*x^n)/(a + c*x^n)^(3/2)")]
        [InlineData("x^(-1 + 1/2*n)*(-a*h + c*f*x^(1/2*n) + c*g*x^(3/2*n) + c*h*x^(2*n))/(a + b*x^n + c*x^(2*n))^(3/2)")]
        public void UnderAPowerOfX(string integrand) => AssertAnAntiderivative(integrand);

        [Theory]
        // Rubi's 1.2.3.5 and 1.1.3.2: a power of a multiple of x beside the powers of x, read as
        // the power of x with the ratio, constant on each interval, in front -- for a negative
        // multiple too, where both are complex and their ratio is still a constant.
        [InlineData("(d*x)^(-1 + 1/4*n)*(-a*h + c*f*x^(1/4*n) + c*g*x^(3/4*n) + c*h*x^n)/(a + c*x^n)^(3/2)")]
        [InlineData("(d*x)^(-1 + 1/2*n)*(-a*h + c*f*x^(1/2*n) + c*g*x^(3/2*n) + c*h*x^(2*n))/(a + b*x^n + c*x^(2*n))^(3/2)")]
        [InlineData("(c*x)^(-1 - n)/(a + b*x^n)")]
        [InlineData("(c*x)^(-1 - 2/3*n)/(a + b*x^n)")]
        [InlineData("(c*x)^(-1 - 5/4*n)/(a + b*x^n)")]
        [InlineData("(-c*x)^(-1 - n)/(a + b*x^n)")]
        public void BesideAPowerOfAMultipleOfX(string integrand) => AssertAnAntiderivative(integrand);

        private static void AssertAnAntiderivative(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 2000, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 0.4).Substitute("d", 0.3)
                .Substitute("f", 0.9).Substitute("g", 1.1).Substitute("h", 0.6).Substitute("k", 0.5).Substitute("p", 0.35).Substitute("n", 2.3);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { 0.3, 0.55, 0.8, 1.3, 1.7, 2.9 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                if (want.IsNaN)
                    continue;
                compared++;
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
            Assert.True(compared >= 5, $"only {compared} points could be compared for {integrand}");
        }
    }
}
