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
    /// A power of x times a whole power of hyperbolic functions of a linear in <c>x^n</c>, under
    /// <c>t = x^n</c>, where <c>x^m dx</c> is <c>t^((m + 1)/n - 1) dt/n</c>. The hyperbolic functions
    /// arrive as exponentials of <c>a + b x^n</c>, and in x the rules spent the budget on each of
    /// these. Rubi's 6.1.3, 6.2.3, 6.5.2, 6.6.2 and 6.7.1.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class APowerOfXTimesAHyperbolicFunctionOfAPowerIntegralTest
    {
        [Theory]
        [InlineData("sinh(a + b*x^2)^3/x^3")]
        [InlineData("cosh(a + b*x^2)^3/x^3")]
        [InlineData("x*sech(a + b*x^2)^7")]
        [InlineData("x*csch(a + b*x^2)^7")]
        [InlineData("x/(a + b*sech(c + d*x^2))^2")]
        [InlineData("x/(a + b*csch(c + d*x^2))^2")]
        [InlineData("x*(a + b*sech(c + d*x^2))^2")]
        [InlineData("x^5*cosh(a + b*x^3)^7*sinh(a + b*x^3)")]
        [InlineData("sinh(a + b*x^2)^4/x^5")]
        [InlineData("x/(cosh(a + b*x^2)^2 + 1)")]
        public void UnderThePower(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 5000, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 0.4).Substitute("d", 0.3);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -1.3, -0.8, -0.4, 0.35, 0.9, 1.4 })
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
