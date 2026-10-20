//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Globalization;
using System.Linq;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// <c>x^m (d + k x^2)^p (a + b F(c x))</c> with <c>m + 2p + 3 = 0</c> and <c>F</c> an inverse
    /// trigonometric function, by parts against <c>v = x^(m + 1) (d + k x^2)^(p + 1)/(d (m + 1))</c>.
    /// By parts against the integrator's own antiderivative, a piecewise on <c>k = 0</c>, ran out of
    /// time. Rubi's 5.1.4, 5.3.4, 5.5.1 and 5.6.1.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class APowerOfXOverAPowerOfAQuadraticTimesAnInverseTrigonometricFunctionByPartsIntegralTest
    {
        private const string Inside = "-1.3 -0.9 -0.4 0.3 0.8 1.2";
        private const string Outside = "-4.1 -2.9 -2.1 2.1 2.9 4.1";

        [Theory]
        [InlineData("x^3*(a + b*arcsin(c*x))/(d + k*x^2)^3", Inside)]
        [InlineData("x^3*(a + b*arctan(c*x))/(d + k*x^2)^3", Inside)]
        [InlineData("x^3*(a + b*arcsec(c*x))/(d + k*x^2)^3", Outside)]
        [InlineData("x^3*(a + b*arccsc(c*x))/(d + k*x^2)^3", Outside)]
        [InlineData("x^3*(a + b*arccos(c*x))/(d + k*x^2)^3", Inside)]
        [InlineData("x^3*(a + b*arccot(c*x))/(d + k*x^2)^3", Inside)]
        [InlineData("x^3*arcsin(c*x)/(d + k*x^2)^3", Inside)]
        [InlineData("x^5*(a + b*arctan(c*x))/(d + k*x^2)^4", Inside)]
        public void ByPartsAgainstTheOneTermAntiderivative(string integrand, string points)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 20000, $"{text.Length} characters of answer for {integrand}");
            foreach (var (d, k) in new[] { (2.1, 0.9), (-1.7, 1.3), (1.9, -0.8) })
            {
                Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 0.6)
                    .Substitute("d", d).Substitute("k", k);
                var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
                var original = Pinned(integrand.ToEntity());
                var compared = 0;
                foreach (var at in points.Split(' ').Select(point => double.Parse(point, CultureInfo.InvariantCulture)))
                {
                    var want = original.Substitute("x", at).EvalNumerical();
                    var got = derivative.Substitute("x", at).EvalNumerical();
                    if (want.IsNaN)
                        continue;
                    compared++;
                    Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                                < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                        $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, d = {d}, k = {k}, where the integrand is {want}");
                }
                Assert.True(compared >= 5, $"only {compared} points could be compared for {integrand} at d = {d}, k = {k}");
            }
        }
    }
}
