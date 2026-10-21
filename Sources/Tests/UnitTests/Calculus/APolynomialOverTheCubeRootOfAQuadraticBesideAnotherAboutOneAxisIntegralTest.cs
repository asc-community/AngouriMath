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
    /// A linear over the cube root of a quadratic beside another quadratic about the same axis, in
    /// the variable that takes the linear terms away: Rubi's pseudo-elliptic 1.2.1.6:232 is its 231
    /// there, with compound coefficients.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class APolynomialOverTheCubeRootOfAQuadraticBesideAnotherAboutOneAxisIntegralTest
    {
        [Theory]
        [InlineData("(g + h*x)/((1/9*(-c^2*g^2 + b*c*g*h + 2*b^2*h^2)/(c*h^2) + b*x + c*x^2)^(1/3)*(f*(b^2 + 1/3*(c^2*g^2 - b*c*g*h - 2*b^2*h^2)/h^2)/c^2 + b*f*x/c + f*x^2))")]
        public void InTheShiftedVariable(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 12000, $"{text.Length} characters of answer for {integrand}");
            // Odd roots of negative reals are real here, as the library evaluates them.
            foreach (var (b, c, f, g, h) in new[] { (1.3, 0.6, 0.9, 1.7, 0.8), (0.5, -0.8, 1.2, -1.3, 0.7) })
            {
                Entity Pinned(Entity e) => e.Substitute("b", b).Substitute("c", c).Substitute("f", f).Substitute("g", g).Substitute("h", h);
                var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
                var original = Pinned(integrand.ToEntity());
                var compared = 0;
                foreach (var at in new[] { -3.1, -1.7, -0.6, 0.31, 0.83, 1.77, 3.2 })
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
}
