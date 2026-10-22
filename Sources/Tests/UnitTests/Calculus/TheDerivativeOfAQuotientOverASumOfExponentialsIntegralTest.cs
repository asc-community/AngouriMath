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
    /// The derivative of a sum of exponentials over another, read off the integrand over the other's
    /// square: <c>(b - a sinh(x))/(a + b sinh(x))^2</c> is the derivative of <c>-cosh(x)/(a + b sinh(x))</c>,
    /// and beside x, by parts, Rubi's 6.7.1 <c>x (b - a sinh(x))/(a + b sinh(x))^2</c> is
    /// <c>-x cosh(x)/(a + b sinh(x)) + ln(a + b sinh(x))/b</c>. The split and the substitution in
    /// <c>e^x</c> answered the first with a piecewise on a discriminant, and the second ran past the budget.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class TheDerivativeOfAQuotientOverASumOfExponentialsIntegralTest
    {
        [Theory]
        [InlineData("x*(b - a*sinh(x))/(a + b*sinh(x))^2")]
        [InlineData("x*(b + a*cosh(x))/(a + b*cosh(x))^2")]
        [InlineData("x*(b - a*sinh(c + d*x))/(a + b*sinh(c + d*x))^2")]
        [InlineData("(b - a*sinh(x))/(a + b*sinh(x))^2")]
        [InlineData("(b + a*cosh(c + d*x))/(a + b*cosh(c + d*x))^2")]
        public void IsAnsweredWithoutCases(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.DoesNotContain("piecewise", text);
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 0.4).Substitute("d", 0.9);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            foreach (var at in new[] { -1.7, -0.6, 0.4, 1.3, 2.2 })
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
