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
    /// Hyperbolic sine and cosine integrals beside a hyperbolic function of their argument, by
    /// parts beside a power of x, and a power of the cotangent beside sums in the tangent. Each was
    /// answered, after declines of five to sixteen gigabytes: the split wrote the hyperbolic
    /// function in its two exponentials, neither elementary beside the integral apart, and asked
    /// the second after the first had settled it; the scaling of the variable simplified three
    /// times to find that `x = b t` leaves `b` inside `e^(b x)`; the substitution search simplified
    /// quotients by the derivative of `Chi(a + b x)` that keep an x whatever is done to them; and
    /// the tangent's quotient was simplified to drop the `1^7` the cotangent leaves. Rubi's 8.5 and
    /// 4.3.3.1. <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class HyperbolicIntegralsBesideTheirExponentialsIntegralTest
    {
        [Theory]
        [InlineData("Shi(b*x)*sinh(b*x)/x^3")]
        [InlineData("cosh(b*x)*Shi(b*x)/x^2")]
        [InlineData("Chi(b*x)*cosh(b*x)/x^3")]
        [InlineData("Chi(b*x)*sinh(b*x)/x^2")]
        [InlineData("x*Chi(b*x)^2")]
        [InlineData("x^2*Shi(a + b*x)^2")]
        [InlineData("x^2*Chi(a + b*x)^2")]
        [InlineData("cot(x)^7*(a + b*tan(x))^4*(A + B*tan(x))")]
        public void DifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Entity Pinned(Entity e) => e.Substitute("a", 0.4).Substitute("b", 0.7).Substitute("A", 1.3).Substitute("B", 0.6);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { 0.3, 0.8, 1.3, 1.7, 2.9, 3.4 })
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

        /// <summary>
        /// The same shapes, bounded by what they allocate on this thread rather than by a clock,
        /// in a symbol no other test here uses, since an integral already answered on the thread
        /// is read from the cache and allocates nothing. Each took between three and a half and
        /// twenty-three gigabytes before.
        /// </summary>
        [Theory]
        [InlineData("Shi(w*x)*sinh(w*x)/x^3")]
        [InlineData("cosh(w*x)*Shi(w*x)/x^2")]
        [InlineData("Chi(w*x)*cosh(w*x)/x^3")]
        [InlineData("Chi(w*x)*sinh(w*x)/x^2")]
        [InlineData("x*Shi(w*x)^2")]
        [InlineData("x*Chi(w*x)^2")]
        [InlineData("cot(x)^7*(v + w*tan(x))^4*(P + Q*tan(x))")]
        public void IsAnsweredWithoutTheLongDeclines(string integrand)
        {
            var expression = integrand.ToEntity();
            var before = GC.GetAllocatedBytesForCurrentThread();
            var text = expression.Integrate("x").Stringize();
            var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            Assert.DoesNotContain("integral(", text);
            Assert.True(allocated < 2_000_000_000L, $"integrating {integrand} allocated {allocated} bytes");
        }
    }
}
