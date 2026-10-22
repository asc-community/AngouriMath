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
    /// x times a rational function of <c>e^u</c> for <c>u = a + b ln(c x^n)</c>, read off the integrand by
    /// the Euler derivative <c>x u' = b n</c>: Rubi's 6.5.3 <c>(1 - b^2 n^2) sech(u) + 2 b^2 n^2 sech(u)^3</c>
    /// is <c>(x (-sech(u) - b n tanh(u) sech(u)))'</c>, and its 6.6.3 twin in the cosecant the same way;
    /// both were declined.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class XTimesARationalFunctionOfAnExponentialOfALogarithmIntegralTest
    {
        [Theory]
        [InlineData("(1 - b^2*n^2)*sech(a + b*ln(c*x^n)) + 2*b^2*n^2*sech(a + b*ln(c*x^n))^3")]
        [InlineData("-(1 - b^2*n^2)*csch(a + b*ln(c*x^n)) + 2*b^2*n^2*csch(a + b*ln(c*x^n))^3")]
        // And a symbolic power, through x times the power two less: Rubi's 6.5.3 and 6.6.3.
        [InlineData("sech(a - ln(c*x^n)/(n*(-2 + p)))^p")]
        [InlineData("csch(a - ln(c*x^n)/(n*(-2 + p)))^p")]
        public void IsXTimesTheQuotient(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 400, $"the antiderivative of {integrand} is {text.Length} characters");
            Entity Pinned(Entity e) => e.Substitute("a", 0.4).Substitute("b", 0.7).Substitute("c", 1.3).Substitute("n", 1.6).Substitute("p", 3.4);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            foreach (var at in new[] { 0.4, 0.9, 1.3, 2.2, 3.1 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
        }
    }
}
