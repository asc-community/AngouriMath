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
    /// A whole power of a constant times an exponential, <c>(k e^u)^n = k^n e^(n u)</c>. The terms of
    /// <c>sinh(a + b x)^3 sinh(c + d x)^3</c>, written in exponentials, are products of two of them,
    /// <c>(-e^(-b x))^2 (-e^(-d x))^2</c>, which nothing else read: 2.5.0 answered the product, and
    /// 2.6.0's development had stopped. Rubi's 6.7.1.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AWholePowerOfAMultipleOfAnExponentialIntegralTest
    {
        [Theory]
        [InlineData("(-e^(-b*x))^2*(-e^(-d*x))^2")]
        [InlineData("(-e^(-b*x))^2")]
        [InlineData("(2*e^x)^3")]
        [InlineData("sinh(b*x)^3*sinh(d*x)^3")]
        [InlineData("sinh(a + b*x)^3*sinh(c + d*x)^3")]
        public void AsAnExponentialOfTheMultiple(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 5000, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 0.4).Substitute("d", 0.3);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -1.2, -0.7, 0.3, 0.8, 1.3, 2.9 })
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
