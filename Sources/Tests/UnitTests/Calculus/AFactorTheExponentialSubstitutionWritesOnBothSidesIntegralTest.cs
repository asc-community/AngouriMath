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
    /// A sum the exponential substitution writes on both sides of the bar, cancelled as it is
    /// written in place of simplifying the quotient: <c>csch(x)^2/(a + b coth(x))^2</c> is
    /// <c>4 (u^2 - 1)^2 u^2/((u^2 - 1)^2 (a (u^2 - 1) + b (u^2 + 1))^2 u)</c> under <c>u = e^x</c>,
    /// and simplifying that with the symbols in it ran past twenty seconds, so that
    /// <c>x csch(x)^2/(a + b coth(x))^2</c>, one step of parts, timed out. Rubi's 6.4.2.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AFactorTheExponentialSubstitutionWritesOnBothSidesIntegralTest
    {
        [Theory]
        [InlineData("x*csch(x)^2/(a + b*coth(x))^2", 1.3, 0.7)]
        [InlineData("x*csch(x)^2/(a + b*coth(x))^2", 0.4, 1.1)]
        [InlineData("x/(sinh(x)^2*(a + b*coth(x))^2)", 1.3, 0.7)]
        [InlineData("x/(sinh(x)^2*(a + b*coth(x))^2)", 0.4, 1.1)]
        [InlineData("csch(x)^2/(a + b*coth(x))^2", 1.3, 0.7)]
        public void ByPartsAgainstTheCancelledQuotient(string integrand, double a, double b)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 5000, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e.Substitute("a", a).Substitute("b", b);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -2.9, -1.3, 0.3, 0.8, 1.3, 2.9 })
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
