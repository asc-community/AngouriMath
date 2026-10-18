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
    /// dividing by it: <c>(a (1 + x^2))^2 - (a (1 - x^2))^2</c> is <c>4 a^2 x^2</c>, its fourth power
    /// cancels, and over its cube the answer was <c>NaN + C</c>. The radical substitution writes it for
    /// Rubi's 7.4.2 <c>1/(e^(arccoth(a x)) (c - a^2 c x^2)^2)</c>.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ALeadingCoefficientThatVanishesIntegralTest
    {
        [Theory]
        [InlineData("1/((a*(1+x^2))^2-(a*(1-x^2))^2)^3")]
        [InlineData("x^2/((a*(1+x^2))^2-(a*(1-x^2))^2)^2")]
        [InlineData("1/((-1/a^2+((1+x^2)/(a*(1-x^2)))^2)^2*(1-x^2)^2)")]
        public void IsNotANaN(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("NaN", integral.Stringize());
            Assert.DoesNotContain("integral(", integral.Stringize());
            var pinned = integral.Substitute("C", 0).Substitute("a", 1.3);
            Entity original = integrand.ToEntity().Substitute("a", 1.3);
            var derivative = pinned.Differentiate("x");
            foreach (var at in new[] { -2.3, 0.4, 1.9 })
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
