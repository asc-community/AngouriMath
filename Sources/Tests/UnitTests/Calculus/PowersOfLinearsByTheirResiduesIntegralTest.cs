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
    /// Rational functions over written powers of linears, split by the residues at their roots.
    /// A rational function of the sine over powers of linears in it, one with a symbolic root
    /// cubed, is split in the sine itself, where under the half-angle that linear is a symbolic
    /// quadratic whose cube took the budget; and a repeated quadratic over the rationals with Gaussian
    /// roots, the tangent's <c>1 + u^2</c>, is split by its residue at <c>i</c>. Rubi's 4.1.1.3,
    /// 4.1.2.1, 4.1.2.2 and 4.3.1.3.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class PowersOfLinearsByTheirResiduesIntegralTest
    {
        [Theory]
        [InlineData("tan(c + d*x)^4/(a + b*sin(c + d*x))^3")]
        [InlineData("cot(c + d*x)^6/(a + b*sin(c + d*x))^3")]
        [InlineData("cos(c + d*x)^6*csc(c + d*x)^8/(a + b*sin(c + d*x))^3")]
        [InlineData("csc(c + d*x)^3*sec(c + d*x)^2/(a + b*sin(c + d*x))^3")]
        [InlineData("1/((a + b*sin(k + f*x))^3*(c + d*sin(k + f*x))^3)")]
        public void OverPowersOfLinearsInTheSine(string integrand)
        {
            AntiderivativeHolds(integrand, (1.3, 0.7));
            AntiderivativeHolds(integrand, (0.6, 1.7));
        }

        [Theory]
        [InlineData("sin(c + d*x)^6/(a + b*tan(c + d*x))^2")]
        [InlineData("sin(c + d*x)^6/(a + b*tan(c + d*x))^3")]
        [InlineData("sin(c + d*x)^4/(a + b*tan(c + d*x))^4")]
        [InlineData("x^6/((1 + x^2)^4*(a + b*x)^2)")]
        [InlineData("x^3/((2 + 2*x + x^2)^3*(a + b*x)^2)")]
        public void OverARepeatedQuadraticWithGaussianRoots(string integrand)
            => AntiderivativeHolds(integrand, (1.3, 0.7));

        private static void AntiderivativeHolds(string integrand, (double A, double B) pins)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 40000, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e.Substitute("a", pins.A).Substitute("b", pins.B)
                .Substitute("c", 0.4).Substitute("d", 0.3).Substitute("k", 0.2).Substitute("f", 0.9);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -0.5, 0.3, 0.8, 1.3, 2.2, 3.1 })
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
    }
}
