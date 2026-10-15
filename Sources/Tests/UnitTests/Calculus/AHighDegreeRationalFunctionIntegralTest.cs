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
    /// A rational function with numbers for coefficients over a denominator past the twelfth
    /// degree, by Hermite's reduction: Rubi's 1.3.1 derivatives over powers of
    /// <c>x^4 + x + 3</c>, written as sums of quotients over different powers of it.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AHighDegreeRationalFunctionIntegralTest
    {
        [Theory]
        [InlineData("3*(-47+228*x+120*x^2+19*x^3)/(3+x+x^4)^4+(42-320*x-75*x^2-8*x^3)/(3+x+x^4)^3+30*x/(3+x+x^4)^2")]
        [InlineData("(-3+10*x+4*x^3-30*x^5)/(3+x+x^4)^3-3*(1+4*x^3)*(2-3*x+5*x^2+x^4-5*x^6)/(3+x+x^4)^4")]
        public void IsAnsweredAndDifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            var derivative = integral.Substitute("C", 0).Differentiate("x");
            foreach (var at in new[] { -2.3, -0.7, 0.4, 1.9 })
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = integrand.ToEntity().Substitute("x", at).EvalNumerical();
                var difference = Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart));
                Assert.True(difference / scale < 1e-8, $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
        }
    }
}
