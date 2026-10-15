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
    /// A power of an even quadratic <c>w = p + q x^2</c> over a binomial in it,
    /// <c>w^m/(a + b w^n)</c>, split over the <c>n</c> roots of <c>a + b w^n</c> into
    /// <c>1/(p - r + q x^2)</c>. Rubi's 1.3.1 <c>(1 - x^2)/(a + b (1 - x^2)^4)</c>, and an odd
    /// <c>n</c>, whose roots of unity are written as <c>cos + i sin</c>.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class APowerOfAnEvenQuadraticOverABinomialInItIntegralTest
    {
        private static void DifferentiatesBack(string integrand, params (string, double)[] pins)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());

            var pinned = integral.Substitute("C", 0);
            Entity original = integrand.ToEntity();
            foreach (var (name, value) in pins)
            {
                pinned = pinned.Substitute(name, value);
                original = original.Substitute(name, value);
            }
            var derivative = pinned.Differentiate("x");
            foreach (var at in new[] { -2.3, -0.7, 0.4, 1.9 })
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                var difference = Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart));
                Assert.True(difference / scale < 1e-8,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
        }

        [Theory]
        [InlineData("(1-x^2)/(a+b*(1-x^2)^4)")]
        [InlineData("(1-x^2)/(a+b*(-1+x^2)^4)")]
        [InlineData("1/(a+b*(2+3*x^2)^3)")]
        [InlineData("(2+3*x^2)^2/(a+b*(2+3*x^2)^3)")]
        public void IsAnsweredAndDifferentiatesBack(string integrand)
        {
            DifferentiatesBack(integrand, ("a", 2), ("b", 3));
            DifferentiatesBack(integrand, ("a", -1.5), ("b", 2));
            DifferentiatesBack(integrand, ("a", 5), ("b", -0.5));
        }
    }
}
