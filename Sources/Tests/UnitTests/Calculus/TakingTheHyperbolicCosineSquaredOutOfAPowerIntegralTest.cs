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
    /// <c>cosh(y)^2</c> taken out of a fractional power of a quotient: <c>cosh(x) coth(x)</c> and
    /// <c>csch(x) + sinh(x)</c> are both <c>cosh(x)^2/sinh(x)</c>, whose power is then odd in the
    /// hyperbolic cosine. Rubi's 6.7.1.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    /// <remarks>
    /// Compared as complex numbers on both sides of zero: for <c>x &lt; 0</c> the radicand is
    /// negative, and the constant the power was written apart with is <c>-1</c> there.
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class TakingTheHyperbolicCosineSquaredOutOfAPowerIntegralTest
    {
        [Theory]
        [InlineData("(cosh(x)*coth(x))^(1/2)")]
        [InlineData("(cosh(x)*coth(x))^(5/2)")]
        [InlineData("(csch(x) + sinh(x))^(3/2)")]
        [InlineData("1/(csch(x) + sinh(x))^(1/2)")]
        [InlineData("(cosh(a + b*x)^2/(c + d*sinh(a + b*x)))^(3/2)")]
        public void IsOddInTheCosineOnceItIsOut(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Entity Pinned(Entity e) => e.Substitute("a", 0.4).Substitute("b", 1.3).Substitute("c", 0.7).Substitute("d", 1.1);
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
