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
    /// Two powers of exponentials written apart whose logarithmic derivatives are one constant,
    /// each read as a constant multiple of the one exponential: Rubi's 2.3
    /// <c>(a + b (F^(e (c + d x)))^n)^p (G^(h (f + g x)))^(d e n ln(F)/(g h ln(G)))</c>, which the
    /// derivative-of-a-product search ran past its budget on, is
    /// <c>(G^(h (f + g x)))^(...) (a + b (F^(e (c + d x)))^n)^(p + 1)/(b d e n (p + 1) ln(F) (F^(e (c + d x)))^n)</c>.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    /// <remarks>
    /// Pinned to moderate values: with every symbol near 5 the powers pass the range the
    /// evaluation keeps, and both sides read as infinite.
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class PowersOfExponentialsOfOneRateIntegralTest
    {
        [Theory]
        [InlineData("(a + b*(F^(k*(c + d*x)))^n)^p*(G^(h*(f + g*x)))^(d*k*n*ln(F)/(g*h*ln(G)))", false)]
        [InlineData("(G^(h*(f + g*x)))^(d*k*n*ln(F)/(g*h*ln(G)))/(a + b*(F^(k*(c + d*x)))^n)^2", false)]
        [InlineData("(G^(h*(f + g*x)))^(d*k*n*ln(F)/(g*h*ln(G)))*sqrt(a + b*(F^(k*(c + d*x)))^n)", false)]
        [InlineData("(a + b*(F^(k*(c + d*x)))^n)^p*(F^(h*(f + g*x)))^(d*k*n/(g*h))", false)]
        [InlineData("(a + b*(F^(k*(c + d*x)))^n)^p*(G^(h*(f + g*x)))^(d*k*n*ln(F)/(g*h*ln(G)))", true)]
        public void IsIntegrated(string integrand, bool negativeBases)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 1500, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("F", negativeBases ? -1.9 : 1.9)
                .Substitute("k", 0.4).Substitute("c", 0.3).Substitute("d", 0.6).Substitute("n", 1.7).Substitute("p", 0.45)
                .Substitute("G", negativeBases ? -2.6 : 2.6).Substitute("h", 0.35).Substitute("f", -0.2).Substitute("g", 0.8);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -2.3, -1.1, -0.4, 0.3, 0.9, 1.7 })
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
