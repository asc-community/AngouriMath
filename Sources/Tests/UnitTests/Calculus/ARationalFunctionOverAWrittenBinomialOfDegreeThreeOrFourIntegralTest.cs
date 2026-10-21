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
    /// A rational function with symbols in it over a product written with a binomial of degree three
    /// or four in it, split over the factors as written by undetermined coefficients: each piece is a
    /// polynomial over a binomial, or over a linear or a quadratic, which the rules for those answer.
    /// The split read only linear and quadratic factors, and `1/((a + b u^3) (1 + u^2))`, which the
    /// tangent makes of Rubi's 4.3.7 `1/(a + b tan(c + d x)^3)`, was declined.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ARationalFunctionOverAWrittenBinomialOfDegreeThreeOrFourIntegralTest
    {
        [Theory]
        [InlineData("1/((a+b*x^3)*(1+x^2))")]
        [InlineData("1/((a+b*x^4)*(c+x^2))")]
        [InlineData("1/(a+b*tan(c+d*x)^3)")]
        [InlineData("1/(a+b*tan(c+d*x)^3)^2")]
        public void DifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 0.4).Substitute("d", 0.9);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -1.3, -0.6, -0.2, 0.3, 0.8, 1.4 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                if (want.IsNaN || got.IsNaN)
                    continue;
                compared++;
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
            Assert.True(compared >= 5, $"only {compared} points could be compared for {integrand}");
        }

        /// <summary>
        /// Not split beside a power of the variable alone, nor beside a second such binomial: the cube
        /// root's substitution makes Rubi's 1.1.3.4 rows rational functions over <c>u^3 (c + k u^3)</c>
        /// and over <c>(b u^3 - 1)(c + k u^3)</c>, and split, they came back in 12,377 and 27,590
        /// characters with <c>c^36</c> and <c>c^43</c> in them, the second wrong, where the rules for
        /// the cube root write 1,964 and 2,861.
        /// </summary>
        [Theory]
        [InlineData("(a+b*x^3)^(1/3)/(x^5*(c+d*x^3))")]
        [InlineData("x^7*(a+b*x^3)^(1/3)/(c+d*x^3)")]
        public void IsNotSplitBesideAPowerOfXOrASecondBinomial(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 5000, $"the antiderivative of {integrand} is {text.Length} characters");
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 0.4).Substitute("d", 0.9);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { 0.3, 0.6, 0.9, 1.2, 1.5, 1.8 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                if (want.IsNaN || got.IsNaN)
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
