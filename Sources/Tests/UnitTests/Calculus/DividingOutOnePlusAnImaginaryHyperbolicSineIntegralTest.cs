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
    /// A polynomial in <c>sinh(y)</c> and <c>cosh(y)</c> over <c>a ± i a sinh(y)</c> beside a power
    /// of a linear, divided out: <c>cosh(y)^2 = (1 + i sinh(y))(1 - i sinh(y))</c>. Rubi's 6.1.1.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    /// <remarks>Compared as complex numbers: the integrand is complex wherever it is defined.</remarks>
    [Trait("Area", "Calculus")]
    public sealed class DividingOutOnePlusAnImaginaryHyperbolicSineIntegralTest
    {
        [Theory]
        [InlineData("(g + f*x)*cosh(c + d*x)^2/(a + i*a*sinh(c + d*x))")]
        [InlineData("(g + f*x)^3*cosh(c + d*x)^3/(a + i*a*sinh(c + d*x))")]
        [InlineData("cosh(c + d*x)^2/((g + f*x)^2*(a + i*a*sinh(c + d*x)))")]
        [InlineData("cosh(c + d*x)^3/((g + f*x)*(a + i*a*sinh(c + d*x)))")]
        [InlineData("(g + f*x)*sinh(c + d*x)^3/(a + i*a*sinh(c + d*x))")]
        [InlineData("(g + f*x)^2*cosh(c + d*x)^3/(a - i*a*sinh(c + d*x))")]
        [InlineData("x^2*cosh(2*x + 1)^4/(3 - 3*i*sinh(2*x + 1))")]
        public void IsAPolynomialInTheHyperbolicFunctionsAndAQuotient(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("c", 0.4).Substitute("d", 0.3).Substitute("g", 0.6).Substitute("f", 1.1);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -1.9, -1.1, -0.2, 0.4, 1.1, 1.9 })
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
