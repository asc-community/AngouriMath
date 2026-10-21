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
    /// The derivative of a product of powers written out as a sum whose terms do not all hold the
    /// same bases: <c>(1 + e^x) x/sqrt(e^x + x) + 2 sqrt(e^x + x)</c> is <c>(2 x sqrt(e^x + x))'</c>,
    /// with <c>x</c> and <c>1 + e^x</c> in the first term only. Neither term is elementary on its
    /// own, and the sum was declined. Rubi's 2.3:866-877.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ADerivativeOfAProductOfPowersWrittenAsASumIntegralTest
    {
        [Theory]
        [InlineData("(1 + e^x)*x/sqrt(e^x + x) + 2*sqrt(e^x + x)")]
        [InlineData("x/sqrt(e^x + x) + e^x*x/sqrt(e^x + x) + 2*sqrt(e^x + x)")]
        [InlineData("1/5*x^2*(5*e^x + 3*x^2)/sqrt(5*e^x + x^3) + 4/5*x*sqrt(5*e^x + x^3)")]
        [InlineData("(-1)/(e^x + x)^(1/3) + x/(e^x + x)^(1/3) - (e^x + x)^(2/3)")]
        [InlineData("2*x/(e^x + x)^(1/3) + 2*e^x*x/(e^x + x)^(1/3) + 3*(e^x + x)^(2/3)")]
        [InlineData("x*(a*e^x + b)/sqrt(a*e^x + b*x) + 2*sqrt(a*e^x + b*x)")]
        public void IsTheProductRaised(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 200, $"{text.Length} characters of answer for {integrand}: {text}");
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -0.4, 0.3, 0.8, 1.3, 2.1, 2.9 })
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
