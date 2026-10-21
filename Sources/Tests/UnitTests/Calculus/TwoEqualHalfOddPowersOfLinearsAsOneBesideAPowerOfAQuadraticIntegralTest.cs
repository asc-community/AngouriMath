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
    /// The same half-odd power of two linears beside a squared quadratic, as one power of their
    /// product with the two roots written back. Rubi's 1.2.1.4 <c>1/((a + b x + c x^2)^2 sqrt(1 - d x) sqrt(1 + d x))</c>.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class TwoEqualHalfOddPowersOfLinearsAsOneBesideAPowerOfAQuadraticIntegralTest
    {
        [Theory]
        [InlineData("1/((a + b*x + c*x^2)^2*sqrt(1 - d*x)*sqrt(1 + d*x))")]
        public void AsOnePowerOfTheProduct(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 60000, $"{text.Length} characters of answer for {integrand}");
            // By central differences of the answer: its derivative, taken symbolically with the
            // numbers pinned, carries a condition that is false at these pins, though the answer and
            // its derivative agree with the integrand to every digit there.
            Entity Pinned(Entity e) => e.Substitute("a", "13/10".ToEntity()).Substitute("b", "7/10".ToEntity()).Substitute("c", "2/5".ToEntity()).Substitute("d", "3/10".ToEntity());
            var antiderivative = Pinned(integral.Substitute("C", 0));
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            const double h = 1e-6;
            foreach (var at in new[] { -1.2, -0.7, 0.3, 0.8, 1.3, 2.9 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                if (want.IsNaN)
                    continue;
                var got = (antiderivative.Substitute("x", at + h).EvalNumerical() - antiderivative.Substitute("x", at - h).EvalNumerical()) / (2 * h);
                compared++;
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-6 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"the antiderivative of {integrand} changes at the rate {got} at x = {at}, where the integrand is {want}");
            }
            Assert.True(compared >= 5, $"only {compared} points could be compared for {integrand}");
        }
    }
}
