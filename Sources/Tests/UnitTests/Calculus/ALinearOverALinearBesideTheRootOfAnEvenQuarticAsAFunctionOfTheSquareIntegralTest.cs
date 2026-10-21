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
    /// A linear over a linear beside the root of an even quartic where its integral is a constant
    /// times the inverse tangent, or its hyperbolic one, of the numerator's square over the root. Rubi's 1.2.2.3.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ALinearOverALinearBesideTheRootOfAnEvenQuarticAsAFunctionOfTheSquareIntegralTest
    {
        [Theory]
        [InlineData("(1 + x - sqrt(3))/((1 + x + sqrt(3))*sqrt(-4 + x^4 + 4*x^2*sqrt(3)))")]
        [InlineData("(1 + x + sqrt(3))/((1 + x - sqrt(3))*sqrt(-4 + x^4 - 4*x^2*sqrt(3)))")]
        [InlineData("(1 + 2*x - sqrt(3))/((1 + 2*x + sqrt(3))*sqrt(-1 + 4*x^4 + 4*x^2*sqrt(3)))")]
        [InlineData("(1 + 2*x + sqrt(3))/((1 + 2*x - sqrt(3))*sqrt(-1 + 4*x^4 - 4*x^2*sqrt(3)))")]
        public void AnInverseTangentOfTheSquareOverTheRoot(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 2000, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e;
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -2.1, -1.4, 0.3, 0.8, 1.3, 2.9 })
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
