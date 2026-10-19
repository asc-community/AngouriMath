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
    /// An exponential of a quadratic over a power of the linear through its vertex,
    /// <c>f^(a + b x + c x^2)/(b + 2 c x)^n</c>. Under <c>u = b + 2 c x</c> the exponent has no linear
    /// part, and the integrand is a moment about 0 of the Gaussian, which the integrator answers.
    /// They were declined. Rubi's 2.3.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AnExponentialOfAQuadraticOverTheLinearThroughItsVertexIntegralTest
    {
        [Theory]
        [InlineData("f^(a + b*x + c*x^2)/(b + 2*c*x)")]
        [InlineData("f^(a + b*x + c*x^2)/(b + 2*c*x)^2")]
        [InlineData("f^(a + b*x + c*x^2)/(b + 2*c*x)^3")]
        [InlineData("f^(b*x + c*x^2)/(b + 2*c*x)^3")]
        [InlineData("e^(x^2 + 2*x)/(x + 1)")]
        public void AMomentAboutTheVertex(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 0.4).Substitute("f", 2.3);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -2.9, -0.4, 0.3, 0.8, 1.3, 2.1 })
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

        [Fact]
        public void NotThroughTheVertexIsDeclined()
            => Assert.Contains("integral(", "e^(x^2 + 2*x)/(x + 2)".ToEntity().Integrate("x").Stringize());
    }
}
