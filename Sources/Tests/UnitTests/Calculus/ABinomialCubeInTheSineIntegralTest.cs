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
    /// A rational function of the sine over <c>a + b sin^3</c>, the cube written
    /// <c>b (s + r)(s^2 - r s + r^2)</c> with <c>r^3 = a/b</c> and split over its factors. Rubi's 4.1.7.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ABinomialCubeInTheSineIntegralTest
    {
        [Theory]
        [InlineData("csc(c + d*x)/(a + b*sin(c + d*x)^3)")]
        [InlineData("csc(c + d*x)^2/(a + b*sin(c + d*x)^3)")]
        [InlineData("csc(c + d*x)^3/(a + b*sin(c + d*x)^3)")]
        [InlineData("sec(c + d*x)^2/(a + b*sin(c + d*x)^3)")]
        public void DifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Entity Pinned(Entity e) => e.Substitute("a", 2).Substitute("b", 3).Substitute("c", "1/4".ToEntity()).Substitute("d", "3/2".ToEntity());
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            foreach (var at in new[] { "0.35", "0.6", "1.1", "1.4" })
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
