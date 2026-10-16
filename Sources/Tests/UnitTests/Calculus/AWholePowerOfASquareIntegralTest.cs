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
    /// A whole power of a trinomial that is the square of a binomial in <c>x^2</c>,
    /// <c>(a^2 + 2 a b x^2 + b^2 x^4)^n</c>, written as <c>(a + b x^2)^(2 n)</c>. Rubi's 1.2.2.2
    /// rows beside <c>(d x)^(k/2)</c>, sampled where <c>d x</c> is positive and on both sides of
    /// the root of <c>a + b x^2</c> when there is one.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AWholePowerOfASquareIntegralTest
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
            foreach (var at in new[] { 0.3, 0.7, 1.1, 1.9 })
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
        [InlineData("1/((d*x)^(3/2)*(a^2+2*a*b*x^2+b^2*x^4)^2)")]
        [InlineData("1/((d*x)^(7/2)*(a^2+2*a*b*x^2+b^2*x^4)^2)")]
        [InlineData("1/((d*x)^(5/2)*(a^2+2*a*b*x^2+b^2*x^4)^3)")]
        public void IsAnsweredAndDifferentiatesBack(string integrand)
        {
            DifferentiatesBack(integrand, ("a", 2), ("b", 3), ("d", 0.7));
            DifferentiatesBack(integrand, ("a", -2), ("b", 3), ("d", 1.5));
        }
    }
}
