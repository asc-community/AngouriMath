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
    /// A power of x below the bar beside a root of a binomial in <c>x^n</c> and a rational function
    /// of <c>x^n</c>, rationalised by <c>u = x/(a + b x^n)^(1/n)</c>: <c>x^(-2)</c> is <c>x</c>
    /// times <c>x^(-3)</c>. Rubi's 1.1.3.4.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class APowerOfXBelowTheBarBesideARootOfABinomialIntegralTest
    {
        [Theory]
        [InlineData("(a + b*x^3)^(1/3)/(x^2*(c + d*x^3))", "0.35 0.6 0.85 1.4")]
        [InlineData("(a + b*x^3)^(1/3)/(x^5*(c + d*x^3))", "0.35 0.6 0.85 1.4")]
        [InlineData("(a + b*x^3)^(4/3)/(x^2*(c + d*x^3))", "0.35 0.6 0.85 1.4")]
        [InlineData("1/(x^2*(a + b*x^3)^(2/3)*(c + d*x^3))", "0.35 0.6 0.85 1.4")]
        [InlineData("(a + b*x^3)^(1/3)/(x^2*(a*d - b*d*x^3))", "0.35 0.6 1.4 1.8")]
        [InlineData("1/(x^2*(1 - x^3)^(2/3)*(1 + x^3))", "0.35 0.6 0.8 0.95")]
        public void DifferentiatesBack(string integrand, string points)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Entity Pinned(Entity e) => e.Substitute("a", 2).Substitute("b", 3).Substitute("c", 5).Substitute("d", 7);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            foreach (var at in points.Split(' '))
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
