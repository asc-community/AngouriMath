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
    /// Exponentials of linears in different bases over a power of x: <c>a^x b^x/x</c> is
    /// <c>Ei((ln(a) + ln(b)) x)</c>. The rule for a product of exponentials left every product
    /// without a quadratic to the rules for one exponential, and beside a negative power of x none
    /// of them reads two. Rubi's 2.3.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ExponentialsOfTwoBasesOverAPowerOfXIntegralTest
    {
        [Fact]
        public void TheExponentialIntegralOfTheSumOfTheLogarithms()
            => Assert.Equal("Ei((ln(a) + ln(b)) * x)".ToEntity(), "a^x*b^x/x".ToEntity().Integrate("x").Substitute("C", 0).InnerSimplified);

        [Theory]
        [InlineData("a^x*b^x/x^2")]
        [InlineData("a^x*b^x/x^3")]
        [InlineData("a^x*b^x*(1 + x)/x^2")]
        [InlineData("a^(c + d*x)*b^(f + g*x)/x")]
        [InlineData("2^x*3^x/x")]
        // e^(b x) e^(-b x) beside 1/x is 1/x, its rate b - b decided as a value: written, the rate is
        // not zero, and these were answered with the exponential integral of it divided by it.
        [InlineData("x*Shi(f*x)*sinh(f*x)")]
        [InlineData("x*Chi(f*x)*cosh(f*x)")]
        [InlineData("sinh(f*x)^2/x")]
        public void ByPartsDownToTheExponentialIntegral(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 0.4).Substitute("d", 0.3)
                .Substitute("f", 2.3).Substitute("g", -0.6);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -0.9, -0.4, 0.3, 0.8, 1.3, 2.9 })
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
