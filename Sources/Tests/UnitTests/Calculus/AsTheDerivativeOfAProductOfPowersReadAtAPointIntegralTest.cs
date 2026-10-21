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
    /// A product of powers with symbolic exponents that is the derivative of another, where the
    /// quotient of the two brackets is a constant only once the coefficients of <c>x^n</c> and
    /// <c>x^(2 n)</c> cancel: Rubi's 1.1.3.3 <c>(a - b x^(n/2))^p (a + b x^(n/2))^p (c + d x^n)^q</c>
    /// at <c>c = -a^2 d n (p + 1)/(b^2 (1 + n + n p))</c> and <c>q = -(1 + 2 n + n p)/n</c>, which is
    /// <c>(x (a - b x^(n/2))^(p + 1) (a + b x^(n/2))^(p + 1) (c + d x^n)^(q + 1))'</c> up to a constant.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AsTheDerivativeOfAProductOfPowersReadAtAPointIntegralTest
    {
        [Theory]
        [InlineData("(a - b*x^(n/2))^p*(a + b*x^(n/2))^p*(a^2*d*(1 + p)/(b^2*(1 + (-1 - 2*n - n*p)/n)) + d*x^n)^((-1 - 2*n - n*p)/n)")]
        [InlineData("(a - b*x^(n/2))^p*(a + b*x^(n/2))^p*(-a^2*d*n*(1 + p)/(b^2*(1 + n + n*p)) + d*x^n)^((-1 - 2*n - n*p)/n)")]
        [InlineData("(a^2 - b^2*x^n)^p*(-a^2*d*n*(1 + p)/(b^2*(1 + n + n*p)) + d*x^n)^((-1 - 2*n - n*p)/n)")]
        public void TheConstantReadAtAPoint(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 1000, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e.Substitute("a", 1.1).Substitute("b", 0.6).Substitute("d", 0.9)
                .Substitute("n", 2.3).Substitute("p", 0.4);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { 0.3, 0.7, 1.1, 1.6, 2.3, 3.1 })
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
