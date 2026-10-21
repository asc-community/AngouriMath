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
    /// A constant with a minus written in it, taken out of a fractional power of a trigonometric
    /// function, for either sign: <c>(c g)^b</c> is <c>c^b g^b</c> for a positive <c>c</c> and
    /// <c>(-c)^b (-g)^b</c> for a negative one. Rubi's 4.3.2.1 <c>(a + b tan(x))/sqrt(-k tan(x))</c>
    /// was answered <c>provided -k &gt; 0</c> alone, and so said nothing for a positive <c>k</c>,
    /// where the integrand is real wherever the tangent is negative.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ANegatedConstantTakenOutOfAFractionalPowerIntegralTest
    {
        [Theory]
        [InlineData("(a + b*tan(c + d*x))/sqrt(-k*tan(c + d*x))", 0.9)]
        [InlineData("(a + b*tan(c + d*x))/sqrt(-k*tan(c + d*x))", -0.9)]
        [InlineData("sqrt(-k*tan(x))", 1.3)]
        [InlineData("sqrt(-k*tan(x))", -1.3)]
        [InlineData("1/sqrt(-k*tan(c + d*x))", 0.7)]
        [InlineData("(-k*cot(x))^(3/2)", 2.1)]
        [InlineData("(-k*cot(x))^(3/2)", -2.1)]
        [InlineData("sqrt(-k*sin(x))*cos(x)", 0.6)]
        public void HoldsForEitherSign(string integrand, double k)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 3000, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e.Substitute("a", 0.7).Substitute("b", 1.3).Substitute("c", 0.2).Substitute("d", 1.1).Substitute("k", k);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -2.1, -1.3, -0.6, 0.4, 1.0, 1.7, 2.5 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                if (want.IsNaN)
                    continue;
                compared++;
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/dx of the antiderivative of {integrand} at k = {k} is {got} at x = {at}, where the integrand is {want}");
            }
            Assert.True(compared >= 5, $"only {compared} points could be compared for {integrand}");
        }
    }
}
