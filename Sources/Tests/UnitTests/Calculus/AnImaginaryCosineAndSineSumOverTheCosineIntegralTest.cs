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
    /// <c>a cos(y) + i a sin(y)</c> beside powers of the secant is written <c>a cos(y) (1 + i tan(y))</c>,
    /// which the rules for <c>a + i a tan</c> take. Rubi's 4.7.2.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AnImaginaryCosineAndSineSumOverTheCosineIntegralTest
    {
        [Theory]
        [InlineData("sec(x)^5/(a*cos(x) + i*a*sin(x))^2")]
        [InlineData("sec(x)^6/(a*cos(x) - i*a*sin(x))")]
        public void DifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Entity Pinned(Entity e) => e.Substitute("a", 3);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            foreach (var at in new[] { -1.1, -0.4, 0.3, 1.2 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                var scale = Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart));
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart) < 1e-9 * scale,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
        }
    }
}
