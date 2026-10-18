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
    /// A hyperbolic cosecant beside a power of a binomial in an even power of the sine, where
    /// <c>csch(y)</c> arrives as the difference of the exponentials with its half gathered
    /// elsewhere: Rubi's 6.1.7 <c>csch(c + d x) (a + b sinh(c + d x)^4)^n</c>, which was left
    /// unevaluated.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AnOddHyperbolicIntegrandWithAPowerOfABinomialIntegralTest
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
        [InlineData("csch(c+d*x)*(a+b*sinh(c+d*x)^4)^2")]
        [InlineData("csch(c+d*x)*(a+b*sinh(c+d*x)^4)^3")]
        public void IsAnsweredAndDifferentiatesBack(string integrand)
        {
            DifferentiatesBack(integrand, ("a", 1.3), ("b", 0.7), ("c", 0.3), ("d", 1.1));
            DifferentiatesBack(integrand, ("a", -0.5), ("b", 1.5), ("c", -0.4), ("d", 0.8));
        }
    }
}
