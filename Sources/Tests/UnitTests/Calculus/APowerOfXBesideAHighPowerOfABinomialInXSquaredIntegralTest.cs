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
    /// A power of x past the second beside a high power of a binomial in <c>x^2</c> is the binomial rules', even or odd:
    /// the reduction over a power of a symbolic quadratic took the power of x off Rubi's 1.1.2.2
    /// <c>1/(x^4 (a + b x^2)^10)</c> four times over ten levels each, and came back in 75,607 characters
    /// with its like terms never gathered, where the binomial rules write 8,559.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class APowerOfXBesideAHighPowerOfABinomialInXSquaredIntegralTest
    {
        [Theory]
        [InlineData("1/(x^4*(a+b*x^2)^10)", 12000)]
        [InlineData("1/(x^6*(a+b*x^2)^8)", 9000)]
        public void IsAnsweredInFewCharacters(string integrand, int characters)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < characters, $"the antiderivative of {integrand} is {text.Length} characters");
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            foreach (var at in new[] { -1.7, -0.6, 0.4, 1.3, 2.2 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                Assert.False(got.IsNaN, $"d/dx of the antiderivative of {integrand} has no value at x = {at}, where the integrand is {want}");
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
        }
    }
}
