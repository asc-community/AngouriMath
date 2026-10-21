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
    /// <c>cosh(y)^(2m)</c> over <c>(1 ± i sinh(y))^(2m)</c>, or the other way up, cancelled through
    /// <c>cosh(y)^2 = (1 + i sinh(y))(1 - i sinh(y))</c>: <c>1/(sech(x) + i tanh(x))^4</c> is
    /// <c>(1 - i sinh(x))^2/(1 + i sinh(x))^2</c>. Rubi's 6.7.1.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    /// <remarks>Compared as complex numbers: the integrand is complex wherever it is defined.</remarks>
    [Trait("Area", "Calculus")]
    public sealed class CancellingOnePlusAnImaginaryHyperbolicSineAgainstTheCosineSquaredIntegralTest
    {
        [Theory]
        [InlineData("1/(sech(x) + i*tanh(x))^4")]
        [InlineData("1/(sech(x) - i*tanh(x))^4")]
        [InlineData("1/(sech(x) + i*tanh(x))^6")]
        [InlineData("1/(sech(a + b*x) - i*tanh(a + b*x))^4")]
        [InlineData("cosh(x)^4/(1 + i*sinh(x))^4")]
        [InlineData("3*cosh(a + b*x)^6/(a - i*a*sinh(a + b*x))^6")]
        [InlineData("(sech(x) + i*tanh(x))^6")]
        [InlineData("cosh(x)^2/(i + sinh(x))^2")]
        public void IsTheQuotientTheCancellationLeaves(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Entity Pinned(Entity e) => e.Substitute("a", -0.4).Substitute("b", 1.3);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            foreach (var at in new[] { -1.9, -1.1, -0.2, 0.4, 1.1, 1.9 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
        }
    }
}
