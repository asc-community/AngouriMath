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
    /// A sum of powers of x times one power with a symbol in its exponent of a linear in a root of
    /// x, or of its square written out, whose terms are not elementary apart: the derivative of that
    /// power times the linear and a sum of powers of the root. Rubi's 1.2.3.2:621.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ASumOfPowersOfXTimesAPowerOfALinearInARootOfXIntegralTest
    {
        [Theory]
        [InlineData("(a^2 + 2*a*b*x^(1/3) + b^2*x^(2/3))^p/x^2 - 2/3*b^3*(1 - 2*p)*(1 - p)*p*(a^2 + 2*a*b*x^(1/3) + b^2*x^(2/3))^p/(a^3*x)")]
        [InlineData("(a + b*x)^p/x^4 - b^3*p*(p - 1)*(p - 2)/(6*a^3)*(a + b*x)^p/x")]
        [InlineData("b*(p + 1)/2*x^(-1/2)*(a + b*sqrt(x))^p + b*p/4*x^(-1)*(a + b*sqrt(x))^p - a/4*x^(-3/2)*(a + b*sqrt(x))^p")]
        [InlineData("(b*(p + 1)/4)*x^(-3/4)*(a + b*x^(1/4))^p + (3*b*p/8)*x^(-1)*(a + b*x^(1/4))^p - 3*a/8*x^(-5/4)*(a + b*x^(1/4))^p")]
        public void AsTheDerivativeOfThePowerTimesTheLinear(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 1000, $"{text.Length} characters of answer for {integrand}");
            foreach (var (a, b, p) in new[] { (1.3, 0.7, 0.37), (-1.1, 0.9, -1.7) })
            {
                Entity Pinned(Entity e) => e.Substitute("a", a).Substitute("b", b).Substitute("p", p);
                var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
                var original = Pinned(integrand.ToEntity());
                var compared = 0;
                foreach (var at in new[] { -1.2, 0.3, 0.8, 1.3, 2.9, 5.1 })
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

        [Fact]
        public void NotWhereTheRemaindersDoNotCancel()
            => Assert.Contains("integral(", "(a + b*x)^p/x^4 - c*(a + b*x)^p/x".ToEntity().Integrate("x").Stringize());
    }
}
