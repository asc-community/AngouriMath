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
    /// A root of <c>1 - cos(x)</c> beside a function of the cosine, under <c>u = cos(x)</c> with the sine
    /// written as its sign times <c>sqrt(1 - u) sqrt(1 + u)</c>: Rubi's 4.2.2.1
    /// <c>sqrt(1 - cos(x))/sqrt(a - cos(x))</c>, which is <c>-2 sgn(sin(x)) arctan(sqrt(1 + cos(x))/sqrt(a - cos(x)))</c>.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class TheRootOfOneMinusTheCosineOverTheSignOfTheSineIntegralTest
    {
        [Theory]
        [InlineData("sqrt(1-cos(x))/sqrt(a-cos(x))")]
        [InlineData("sqrt(1+cos(c+d*x))/sqrt(a+cos(c+d*x))")]
        public void IsAnsweredAndDifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("c", 0.3).Substitute("d", 0.75);
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
