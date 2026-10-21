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
    /// A symbolic root of a constant times a power of a function, <c>(c sin(u)^n)^(1/n)</c>, as the
    /// function times a factor constant between its zeros, written back: Rubi's 4.1.7 and 4.2.0.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AFunctionOutOfASymbolicRootOfItsPowerIntegralTest
    {
        [Theory]
        [InlineData("(c*sin(a+b*x)^n)^(1/n)")]
        [InlineData("(c*cos(a+b*x)^m)^(1/m)")]
        [InlineData("x*(c*cos(x)^m)^(1/m)")]
        [InlineData("(c*sin(x)^n)^(2/n)")]
        public void IsAnsweredAndDifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Entity Pinned(Entity e) => e.Substitute("a", 0.3).Substitute("b", 0.75).Substitute("c", 1.3).Substitute("n", 2.5).Substitute("m", 0.4);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -2.3, -1.1, -0.7, 0.4, 1.9, 2.8 })
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

        [Fact]
        public void LeavesAPowerOfXToTheRulesForBinomials()
        {
            // (c x^n)^(1/n) is the rule's for a symbolic power of a monomial: `x (c x^n)^(1/n)/2`,
            // whose derivative is `(c x^n)^(1/n)/2 + x (c x^n)^(1/n)/(2 x)`, the integrand on every
            // branch, since d/dx (c x^n)^(1/n) is (c x^n)^(1/n)/x on every one.
            var integral = "(c*x^n)^(1/n)".ToEntity().Integrate("x").Stringize();
            Assert.Equal("x * (c * x ^ n) ^ (1 / n) / 2 + C", integral);
        }
    }
}
