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
    /// A whole power of <c>a + b u</c> for <c>u</c> the inverse sine, cosine or hyperbolic cosine of
    /// <c>c + d x^2</c> with <c>c^2 = 1</c>, or the inverse hyperbolic sine of one with <c>c^2 = -1</c>,
    /// by the recurrence that closes at the first two powers: Rubi's 5.1.5, 5.2.5 and 7.2.5. Pinned
    /// with <c>d</c> of either sign, on the real domain of each and off it.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class APowerOfAnInverseFunctionOfAQuadraticIntegralTest
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

        // d with the sign that keeps the argument real at every sampled x, and |d| below 2/2.3^2
        // so that the inverse sine and cosine stay inside [-1, 1]; off it the exact checks hold too.
        [Theory]
        [InlineData("(a+b*acos(1+d*x^2))^4", -1)]
        [InlineData("(a+b*acos(-1+d*x^2))^2", 1)]
        [InlineData("(a+b*asin(1+d*x^2))^3", -1)]
        [InlineData("(a+b*asin(-1+d*x^2))", 1)]
        [InlineData("(a+b*acosh(-1+d*x^2))^2", -1)]
        [InlineData("(a+b*acosh(1+d*x^2))^3", 1)]
        [InlineData("(a+b*asinh(i+d*x^2))^2", 1)]
        public void IsAnsweredAndDifferentiatesBack(string integrand, int sign)
        {
            DifferentiatesBack(integrand, ("a", 0.6), ("b", 1.25), ("d", 0.3 * sign));
            DifferentiatesBack(integrand, ("a", -0.75), ("b", -0.7), ("d", 0.15 * sign));
        }

        [Fact]
        public void WithNumbersIsAnswered()
            => DifferentiatesBack("asin(1-2*x^2)^2");
    }
}
