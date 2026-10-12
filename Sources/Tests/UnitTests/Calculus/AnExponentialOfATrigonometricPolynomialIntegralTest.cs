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
    /// An exponential of a polynomial in <c>x</c>, <c>sin(x)</c> and <c>cos(x)</c> beside a
    /// rational function of the three, closed by the ansatz <c>e^h P/Q</c>:
    /// <c>e^sin(x) sec(x)^2 (x cos(x)^3 - sin(x))</c> is <c>(e^sin(x) (x - sec(x)))'</c>, though
    /// neither of its two terms has an elementary antiderivative. Rubi's 4.7.7.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AnExponentialOfATrigonometricPolynomialIntegralTest
    {
        [Theory]
        [InlineData("e^sin(x)*sec(x)^2*(x*cos(x)^3 - sin(x))", 100)]
        [InlineData("e^cos(x)*(x*sin(x) - 1)", 100)]
        [InlineData("e^sin(x)*(x*cos(x) + 1)", 100)]
        public void DifferentiatesBack(string integrand, int atMost)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < atMost, $"{text.Length} characters of answer for {integrand}");
            var derivative = integral.Substitute("C", 0).Differentiate("x");
            var original = integrand.ToEntity();
            foreach (var at in new[] { "0.35", "1.1", "2.3", "4.4" })
            {
                var want = original.Substitute("x", at.ToEntity()).EvalNumerical();
                var got = derivative.Substitute("x", at.ToEntity()).EvalNumerical();
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
        }
    }
}
