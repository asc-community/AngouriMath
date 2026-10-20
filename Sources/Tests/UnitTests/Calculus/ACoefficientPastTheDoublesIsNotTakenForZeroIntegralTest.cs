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
    /// The coefficients of these antiderivatives run to <c>c^294</c>, past <c>1.3e154</c> at the
    /// symbols the lowest-terms reading pins, where the interval check took a size squared to an
    /// infinity and agreed that each was zero: two of the three terms were dropped, and the answer
    /// differed from the integral at every point.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1019">#1019</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ACoefficientPastTheDoublesIsNotTakenForZeroIntegralTest
    {
        [Theory]
        [InlineData("(b*x^(-4) + b*x^(-6) + b*x^(-8))*(1/c^2 + x^2)^(-1/2)")]
        [InlineData("(b*x^12 + b*x^10 + b*x^8)*(1/c^2 + x^2)^(-1/2)/x^16")]
        public void KeepsEveryTerm(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            if (integral.Stringize().Contains("integral("))
                return;
            Entity Pinned(Entity e) => e.Substitute("b", 0.7).Substitute("c", 1.3);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            foreach (var at in new[] { -2.1, -1.3, -0.6, 0.5, 1.2, 2.3 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
        }
    }
}
