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
    /// A power of a secant over a whole power of <c>a cos(y) + b sin(y)</c>, the two powers together
    /// odd and at least seven, in powers of <c>a + b tan(y)</c>. The half-angle ran out of time on
    /// Rubi's 4.7.2 <c>sec(c + d x)^4/(a cos(c + d x) + b sin(c + d x))^3</c>.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ASecantPowerOverAPowerOfACosineAndASineInPowersOfABinomialInTheTangentIntegralTest
    {
        [Theory]
        [InlineData("sec(c + d*x)^4/(a*cos(c + d*x) + b*sin(c + d*x))^3", 1.3, 0.7)]
        [InlineData("sec(c + d*x)^3/(a*cos(c + d*x) + b*sin(c + d*x))^4", 0.6, -1.9)]
        [InlineData("sec(c + d*x)^6/(a*cos(c + d*x) + b*sin(c + d*x))", 1.3, 0.7)]
        [InlineData("sec(c + d*x)^2/(a*cos(c + d*x) + b*sin(c + d*x))^5", -0.6, 1.9)]
        public void InPowersOfTheBinomialInTheTangent(string integrand, double a, double b)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 8000, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e.Substitute("a", a).Substitute("b", b).Substitute("c", 0.4).Substitute("d", 0.3).Substitute("f", 0.6).Substitute("k", 0.2);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { 0.3, 0.8, 1.3, 2.9, 4.1, 5.3 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                if (want.IsNaN)
                    continue;
                compared++;
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-8 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
            Assert.True(compared >= 5, $"only {compared} points could be compared for {integrand}");
        }
    }
}
