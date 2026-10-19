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
    /// <c>x^m artanh(sqrt(k) x/sqrt(d + k x^2))</c>, whose derivative is <c>sqrt(k)/sqrt(d + k x^2)</c>,
    /// by parts. Differentiated as written, the logarithm is a quotient of two sums with a root in
    /// each, and the search ran past its budget on what was left. Rubi's 7.3.7.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class APowerTimesAnArtanhWhoseDerivativeIsARootIntegralTest
    {
        [Theory]
        [InlineData("atanh(x*sqrt(k)/sqrt(d + k*x^2))")]
        [InlineData("x*atanh(x*sqrt(k)/sqrt(d + k*x^2))")]
        [InlineData("x^4*atanh(x*sqrt(k)/sqrt(d + k*x^2))")]
        [InlineData("atanh(x*sqrt(k)/sqrt(d + k*x^2))/x^2")]
        [InlineData("atanh(x*sqrt(k)/sqrt(d + k*x^2))/x^5")]
        [InlineData("atanh(x/sqrt(1 + x^2))")]
        public void ByPartsAgainstTheRoot(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Entity Pinned(Entity e) => e.Substitute("d", 1.3).Substitute("k", 0.7);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -2.1, -0.4, 0.3, 0.8, 1.3, 2.9 })
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
