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
    /// A power of a power of x with a symbolic inner exponent, <c>(c x^m)^p</c>, is
    /// <c>K x^(m p)</c> with <c>K = (c x^m)^p/x^(m p)</c> constant, not <c>c^p x^(m p)</c>: a
    /// symbolic <c>m</c> can be even, and then the integrand is real for a negative x as well,
    /// where <c>x^(m p)</c> is not it. Rubi's 3.5 <c>(ln(a x^n)^m)^p/x</c> was answered
    /// <c>ln(a x^n)^(m p + 1)/(n (m p + 1))</c>, with the wrong sign wherever the logarithm is
    /// negative at <c>m = 2</c>, <c>p = 1/2</c>.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    /// <remarks>
    /// Checked by differentiating back at points on both sides of 0, with <c>m = 2</c> and
    /// <c>p = 1/2</c>, where the integrand is real there, and with <c>m = 2.3</c> and
    /// <c>p = 1.44</c>, past the wrap of the principal argument, where it is complex and
    /// compared as a complex number.
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class APowerOfAPowerWithASymbolicExponentIntegralTest
    {
        [Theory]
        [InlineData("(x^m)^p")]
        [InlineData("(b*x^m)^p")]
        [InlineData("x^k*(x^m)^p")]
        [InlineData("1/(x*(b*x^m)^p)")]
        [InlineData("x^2/(b*x^m)^p")]
        [InlineData("(ln(x)^m)^p/x")]
        [InlineData("(ln(a*x^n)^m)^p/x")]
        [InlineData("(b*ln(a*x^n)^m)^p/x")]
        [InlineData("(sin(x)^m)^p*cos(x)")]
        public void DifferentiatesBackOnBothSides(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            foreach (var (m, p) in new[] { (2.0, 0.5), (2.3, 1.44) })
            {
                Entity Pinned(Entity e) => e.Substitute("m", m).Substitute("p", p).Substitute("a", 0.5)
                    .Substitute("b", 1.5).Substitute("n", 1.3).Substitute("k", 1.2);
                var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
                var original = Pinned(integrand.ToEntity());
                foreach (var at in new[] { 0.31, 0.57, 1.29, 2.41, -0.43, -1.7, -3.3 })
                {
                    var want = original.Substitute("x", at).EvalNumerical();
                    var got = derivative.Substitute("x", at).EvalNumerical();
                    var difference = got - want;
                    Assert.True(Math.Abs((double)difference.RealPart) + Math.Abs((double)difference.ImaginaryPart)
                                < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                        $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, m = {m}, p = {p}, where the integrand is {want}");
                }
            }
        }

        // The constant needs no sign: the factor in front holds for every c.
        [Fact]
        public void ASymbolicConstantOwesNoCondition()
            => Assert.DoesNotContain("provided", "(b*x^m)^p".ToEntity().Integrate("x").Stringize());
    }
}
