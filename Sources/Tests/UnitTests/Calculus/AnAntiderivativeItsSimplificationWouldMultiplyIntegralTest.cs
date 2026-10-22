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
    /// An antiderivative is inner-simplified before it is returned, but not where that multiplies it:
    /// Rubi's 1.1.2.2 <c>x^19/(a + b x^2)^10</c> was answered in 11,811 characters, a sum of ten piecewises
    /// of four arms each whose conditions differ as written, and combined case by case it came to
    /// 5,051,391, which the corpus spent seventeen seconds differentiating back.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AnAntiderivativeItsSimplificationWouldMultiplyIntegralTest
    {
        [Theory]
        // Rubi 1.1.2.2:208.
        [InlineData("x^19/(a + b*x^2)^10", 50_000)]
        [InlineData("x^7/(a + b*x^2)^4", 10_000)]
        public void IsReturnedAtTheSizeItWasFound(string integrand, int mostCharacters)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < mostCharacters, $"{integrand} came back in {text.Length} characters");
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7);
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
