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
    /// A fractional power of a sum beside a constant times the sum's derivative is the plain
    /// substitution's, <c>u</c> = the sum, and the power of x is not taken out of it first:
    /// <c>(x - 3 sqrt(x))^(2/3) (2 sqrt(x) - 3)/sqrt(x)</c> is <c>2 u^(2/3) du</c>. 2.5.0 answered it
    /// in a fifth of a second; with the power of x taken out it was not answered in twenty. Rubi's 1.3.2.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ASumBesideItsDerivativeKeepsItsPowerOfXIntegralTest
    {
        [Theory]
        [InlineData("(x - 3*sqrt(x))^(2/3)*(-3 + 2*sqrt(x))/sqrt(x)")]
        [InlineData("(2*x + 3*b*x^2)*(x^2 + b*x^3)^(1/2)")]
        public void BySubstitutingTheSum(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 5000, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e.Substitute("b", 0.7);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { 0.3, 0.8, 1.3, 2.9, 11.3, 15.1 })
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
