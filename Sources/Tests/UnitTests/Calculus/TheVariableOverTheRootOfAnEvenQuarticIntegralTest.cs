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
    /// A rational function beside a half-odd power of an even quartic P, as a rational function
    /// of <c>t = x/sqrt(P)</c>: the pseudo-elliptic integrals of Rubi's 1.2.2.7 and 1.3.2, which
    /// no rule wrote in t.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class TheVariableOverTheRootOfAnEvenQuarticIntegralTest
    {
        [Theory]
        [InlineData("(a - c*x^4)/((a*d + a*k*x^2 + c*d*x^4)*sqrt(a + b*x^2 + c*x^4))")]
        [InlineData("(a - c*x^4)/((a*d + a*k*x^2 + c*d*x^4)*sqrt(a - b*x^2 + c*x^4))")]
        [InlineData("(a - c*x^4)/((a*k + c*d*x^2)*(d + k*x^2)*sqrt(a + b*x^2 + c*x^4))")]
        [InlineData("sqrt(a + b*x^2 + c*x^4)/(a*d - c*d*x^4)")]
        [InlineData("sqrt(a + b*x^2 - c*x^4)/(a*d + c*d*x^4)")]
        [InlineData("sqrt(a + b*x^4)/(a*c - b*c*x^4)")]
        [InlineData("sqrt(a - b*x^4)/(a*c + b*c*x^4)")]
        [InlineData("x^2/((1 + x^4)*sqrt(-1 + x^4))")]
        public void WrittenInTheVariableOverTheRoot(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 5000, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 0.4).Substitute("d", 0.3).Substitute("k", 0.9);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -2.9, -1.2, -0.7, 0.3, 0.8, 1.7, 2.9 })
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

        [Theory]
        [InlineData("1/sqrt(a + b*x^2 + c*x^4)")]
        [InlineData("x^2/sqrt(1 + x^4)")]
        public void AnEllipticIntegralIsLeftAlone(string integrand)
            => Assert.Contains("integral(", integrand.ToEntity().Integrate("x").Stringize());
    }
}
