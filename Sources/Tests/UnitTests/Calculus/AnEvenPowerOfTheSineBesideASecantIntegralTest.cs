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
    /// An even power of the sine beside a rational function of the secant, written in the cosine,
    /// <c>sin(z)^(2k)</c> as <c>(1 - cos(z)^2)^k</c>: the half-angle substitution wrote Rubi's 4.5.1.3
    /// <c>sin(c + d x)^2/(a + b sec(c + d x))</c> in 117,751 characters and past the corpus's budget,
    /// where in the cosine it is a polynomial over a linear. The cosine beside the cosecant the same way.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AnEvenPowerOfTheSineBesideASecantIntegralTest
    {
        [Theory]
        // Rubi 4.5.1.3:238.
        [InlineData("sin(c + d*x)^2/(a + b*sec(c + d*x))")]
        [InlineData("sin(x)^4/(a + b*sec(x))")]
        [InlineData("cos(x)^2/(a + b*csc(x))")]
        public void IsAnsweredInTheCosine(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 3000, $"{integrand} came back in {text.Length} characters");
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 0.4).Substitute("d", 0.9);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            foreach (var at in new[] { -2.2, -0.9, 0.3, 1.4, 2.1 })
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
