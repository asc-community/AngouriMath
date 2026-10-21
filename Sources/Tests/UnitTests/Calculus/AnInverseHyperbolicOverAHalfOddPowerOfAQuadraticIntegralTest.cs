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
    /// <c>A + B F(a x)</c>, <c>F</c> an inverse hyperbolic sine, cosine or tangent, over a half-odd
    /// power of <c>c + d x^2</c>, by parts against <c>F</c>, the remainder asked a term at a time under
    /// <c>w = x^2</c>: from <c>(c + d x^2)^(7/2)</c> on, by parts in the search was declined or ran out of
    /// time. Rubi's 7.1.4, 7.2.4, 7.2.5 and 7.3.4.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AnInverseHyperbolicOverAHalfOddPowerOfAQuadraticIntegralTest
    {
        [Theory]
        [InlineData("atanh(a*x)/(c+d*x^2)^(9/2)")]
        [InlineData("acosh(a*x)/(c+d*x^2)^(9/2)")]
        [InlineData("acosh(a*x)/(c+d*x^2)^(7/2)")]
        [InlineData("(a+b*asinh(c*x))/(d+k*x^2)^(7/2)")]
        [InlineData("(a+b*acosh(c*x))/(d+k*x^2)^(7/2)")]
        [InlineData("atanh(a*x)/(c+d*x^2)^(3/2)")]
        public void ByPartsOntoTermsInTheSquare(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 20000, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 0.4).Substitute("d", 0.3).Substitute("k", 0.9);
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
