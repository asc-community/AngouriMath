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
    /// A polynomial times a function of <c>atanh(c x)</c> or <c>acoth(c x)</c> over a power of
    /// <c>d (1 +- c x)</c>, written in powers of the linear first: <c>x (a + b atanh(c x))^2/(d + c d x)^3</c>
    /// is two terms answered under <c>t = (1 + c x)/(1 - c x)</c>, where by parts on the whole ran out of
    /// time. Rubi's 7.3.4.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class APolynomialOverAPowerOfALinearOfTheLogarithmIntegralTest
    {
        [Theory]
        [InlineData("x*(a+b*atanh(c*x))^2/(d+c*d*x)^3")]
        [InlineData("x*(a+b*atanh(c*x))^3/(1+c*x)^4")]
        [InlineData("x^2*(a+b*atanh(c*x))^2/(1+c*x)^4")]
        [InlineData("(2+x)*(a+b*atanh(c*x))^2/(1+c*x)^3")]
        [InlineData("x*(a+b*atanh(c*x))^2/(d-c*d*x)^3")]
        [InlineData("x*(a+b*acoth(c*x))^2/(d+c*d*x)^3")]
        public void WrittenInPowersOfTheLinear(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 20000, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 0.4).Substitute("d", 0.3);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -2.9, -1.2, -0.7, 0.3, 0.8, 1.3, 2.9 })
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
