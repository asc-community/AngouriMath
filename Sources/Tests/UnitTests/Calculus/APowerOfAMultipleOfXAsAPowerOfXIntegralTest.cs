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
    /// A factor <c>(c x)^t</c> written as <c>x^t</c>, with <c>(c x)^t x^(-t)</c> in front, whose
    /// derivative is zero: Rubi's 1.1.3.2 and 1.2.3.2 with a symbol for the multiple of x.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class APowerOfAMultipleOfXAsAPowerOfXIntegralTest
    {
        [Theory]
        [InlineData("(c*x)^(-1 - 2*n - n*p)*(a + b*x^n)^p")]
        [InlineData("(c*x)^(-1 - 3*n - n*p)*(a + b*x^n)^p")]
        [InlineData("(c*x)^(-1 - 4*n - n*p)*(a + b*x^n)^p")]
        [InlineData("(d*x)^(-1 - 2*n*(1 + p))*(a^2 + 2*a*b*x^n + b^2*x^(2*n))^p")]
        public void WrittenAsAPowerOfX(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 5000, $"{text.Length} characters of answer for {integrand}");
            // n past the wrap, so that x^n at a negative x is not confined to a half-plane.
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", -0.4).Substitute("d", 0.3)
                .Substitute("n", 2.3).Substitute("p", 0.6);
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
