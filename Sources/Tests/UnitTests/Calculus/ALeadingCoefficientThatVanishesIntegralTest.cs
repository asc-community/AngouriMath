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
    /// A power of a polynomial whose leading coefficient is zero spelled otherwise is not made monic by
    /// dividing by it: the radical substitution of Rubi's 7.4.2 <c>1/(e^(arccoth(a x)) (c - a^2 c x^2)^2)</c>
    /// writes <c>(a (1 + u^2))^2 - (a (1 - u^2))^2</c>, whose fourth power cancels, and the answer was
    /// <c>NaN + C</c>.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ALeadingCoefficientThatVanishesIntegralTest
    {
        [Theory]
        [InlineData("1/(e^acoth(a*x)*(c-a^2*c*x^2)^2)")]
        [InlineData("1/(e^acoth(a*x)*(c-a^2*c*x^2)^5)")]
        public void IsNotANaN(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("NaN", integral.Stringize());
            if (integral.Stringize().Contains("integral("))
                return;
            var pinned = integral.Substitute("C", 0).Substitute("a", 1.3).Substitute("c", 0.7);
            Entity original = integrand.ToEntity().Substitute("a", 1.3).Substitute("c", 0.7);
            var derivative = pinned.Differentiate("x");
            foreach (var at in new[] { -2.3, 1.4, 1.9 })
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                var difference = Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart));
                Assert.True(difference / scale < 1e-8,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
        }
    }
}
