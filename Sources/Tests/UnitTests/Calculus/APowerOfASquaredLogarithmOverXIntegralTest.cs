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
    /// A symbolic power of a power of the variable, asked below the question: <c>u = ln(x)</c> makes
    /// Rubi's 3.5 <c>(ln(x)^2)^p/x</c> the question <c>(u^2)^p</c>, which is <c>u (u^2)^p/(2 p + 1)</c>
    /// on every branch and was answered only where it was the question asked.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class APowerOfASquaredLogarithmOverXIntegralTest
    {
        [Theory]
        [InlineData("(ln(x)^2)^p/x")]
        [InlineData("(ln(a*x^n)^2)^p/x")]
        [InlineData("(ln(a*x)^3)^p/x")]
        public void DifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("n", 2.0).Substitute("p", 0.35);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            // Both sides of ln = 0, where (ln^2)^p is real and ln^(2p) is not.
            foreach (var at in new[] { 0.2, 0.5, 0.8, 1.4, 2.6, 4.1 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                if (want.IsNaN || got.IsNaN)
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
