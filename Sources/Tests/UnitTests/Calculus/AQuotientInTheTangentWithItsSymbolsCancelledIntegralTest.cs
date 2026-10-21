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
    /// A quotient of sines and cosines with symbols in it, written in the tangent with the factors alike
    /// on both sides of the bar cancelled rather than simplified: Rubi's 4.1.7 and 4.5.7, whose
    /// simplification ran past the budget.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AQuotientInTheTangentWithItsSymbolsCancelledIntegralTest
    {
        [Theory]
        [InlineData("sin(c+d*x)^6/(a+b*sin(c+d*x)^2)^2")]
        [InlineData("csc(c+d*x)^2/(a-b*sin(c+d*x)^4)^2")]
        [InlineData("cot(c+d*x)^2/(a+b*sec(c+d*x)^2)^2")]
        [InlineData("sin(x)^4/(a+b*sin(x)^2)^2")]
        public void IsAnsweredAndDifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 0.3).Substitute("d", 0.75);
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
    }
}
