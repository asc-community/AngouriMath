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
    /// Half-odd powers of a hyperbolic sine beside an exponential of another linear, read as the
    /// derivative of the same kind of sum: Rubi's 6.7.1
    /// <c>e^(a + b x) sinh(c + d x)^(3/2) - 3/4 e^(a + b x) d^2/((b^2 - 9/4 d^2) sqrt(sinh(c + d x)))</c>
    /// is <c>(e^(a + b x) (4 b sinh^(3/2) - 6 d cosh sinh^(1/2))/(4 b^2 - 9 d^2))'</c>, and each term alone
    /// is not elementary, so the split declined both.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class HalfOddPowersOfAHyperbolicSineBesideAnExponentialIntegralTest
    {
        [Theory]
        // Rubi 6.7.1:1141.
        [InlineData("e^(a + b*x)*sinh(c + d*x)^(3/2) - 3/4*e^(a + b*x)*d^2/((b^2 - 9/4*d^2)*sqrt(sinh(c + d*x)))")]
        public void IsAnsweredAsADerivative(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Entity Pinned(Entity e) => e.Substitute("a", 0.4).Substitute("b", 1.3).Substitute("c", 0.2).Substitute("d", 0.7);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            foreach (var at in new[] { 0.3, 0.9, 1.4, 2.1, 3.3 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                Assert.False(got.IsNaN, $"d/dx of the antiderivative of {integrand} has no value at x = {at}, where the integrand is {want}");
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
        }
    }
}
