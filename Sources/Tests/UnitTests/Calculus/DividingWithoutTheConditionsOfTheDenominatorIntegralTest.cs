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
    /// A polynomial over a polynomial beside a power of a quadratic, divided, and the part over
    /// the denominator asked without the condition the division writes on it, that the quadratic
    /// is not zero: <c>x^2/((1 + x^2) Q^(3/2))</c> is <c>1/Q^(3/2) - 1/((1 + x^2) Q^(3/2))</c>, and
    /// with the condition nothing read the second. Rubi's 4.3.9
    /// <c>tan^2/(a + b tan + c tan^2)^(3/2)</c>, under the tangent, was declined while the tangent
    /// and its cube over the same were answered.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class DividingWithoutTheConditionsOfTheDenominatorIntegralTest
    {
        [Theory]
        [InlineData("tan(d + f*x)^2/(a + b*tan(d + f*x) + c*tan(d + f*x)^2)^(3/2)")]
        [InlineData("x^2/((1 + x^2)*(a + b*x + c*x^2)^(3/2))")]
        [InlineData("x^2/((2 + x^2)*(a + b*x + c*x^2)^(3/2))")]
        public void IsAnswered(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 5000, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e.Substitute("a", 0.7).Substitute("b", 1.1).Substitute("c", 1.4).Substitute("d", 0.3).Substitute("f", 0.9);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -2.9, -1.7, -0.35, 0.25, 0.8, 1.4, 2.2 })
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
