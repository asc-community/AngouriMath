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
    /// A polynomial over a power of a linear times <c>A + B ln(C x^n)</c>, by parts with the
    /// antiderivative of the rational function that vanishes at zero written as
    /// <c>x M(x)/(d + k x)^(q - 1)</c>: the remainder is then a polynomial over a power of the
    /// linear. Rubi's 3.1.4 <c>x^5 (a + b ln(c x^n))/(d + e x)^7</c> handed the substitution
    /// search a quotient of degree twenty-two and ran past forty seconds.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ARationalFunctionOverAPowerOfALinearTimesALogarithmIntegralTest
    {
        [Theory]
        [InlineData("x^5*(a + b*ln(c*x^n))/(d + k*x)^7")]
        [InlineData("x^6*(a + b*ln(c*x^n))/(d + k*x)^8")]
        [InlineData("x^5*(a + b*ln(x))/(d + k*x)^7")]
        [InlineData("x^4*(a + b*ln(c*x^n))/(d + k*x)^7")]
        [InlineData("x^5*(a + b*ln(c*x^n))/(d + k*x)^8")]
        public void IsIntegrated(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 1500, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 0.6)
                .Substitute("d", 1.1).Substitute("k", -0.9).Substitute("n", 2);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -2.3, -0.7, 0.3, 0.8, 2.9, 4.1 })
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
