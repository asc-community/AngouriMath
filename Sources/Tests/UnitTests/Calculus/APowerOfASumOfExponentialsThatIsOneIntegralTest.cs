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
    /// A power of a sum of exponentials that is one exponential, written as it: the parser writes
    /// <c>cosh(u) + sinh(u)</c> as <c>(e^u + e^(-u))/2 + (e^u - e^(-u))/2</c>, which nothing collected, and
    /// Rubi's 6.7.1 <c>F^(a + b x) (cosh(c + d x) + sinh(c + d x))^n</c> was declined.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class APowerOfASumOfExponentialsThatIsOneIntegralTest
    {
        [Theory]
        [InlineData("F^(a + b*x)*(cosh(c + d*x) + sinh(c + d*x))^n", 400)]
        [InlineData("F^(a + b*x)*(cosh(c + d*x) - sinh(c + d*x))^n", 400)]
        [InlineData("(cosh(x) + sinh(x))^3", 40)]
        [InlineData("x*(cosh(x) - sinh(x))^(5/2)", 120)]
        public void IsOneExponential(string integrand, int characters)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < characters, $"the antiderivative of {integrand} is {text.Length} characters: {text}");
            Entity Pinned(Entity e) => e.Substitute("F", 1.7).Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 0.4)
                .Substitute("d", 0.9).Substitute("n", 0.6);
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
