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
    /// The terms of a sum free of the variable that come to a number multiplied out, folded to it
    /// before the integral is asked: Rubi's 1.2.2.2 and 1.1.1.2 spell a zero coefficient
    /// <c>2 + 2 a - 2 (1 + a)</c> and an exponent of <c>-1</c> as <c>1 + 2 n - 2 (1 + n)</c>.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ConstantSumsFoldedToTheirNumbersIntegralTest
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
        [InlineData("1/sqrt(a+(2+2*c-2*(1+c))*x^4)")]
        [InlineData("1/sqrt(2+2*a-2*(1+a)+c*x^4)")]
        [InlineData("1/sqrt(2-2*(1+a)+c*x^4+2*a)")]
        [InlineData("(c+d*x)^(1+2*n-2*(1+n))/(a+b*x)^2")]
        public void IsAnsweredAndDifferentiatesBack(string integrand)
        {
            DifferentiatesBack(integrand, ("a", 0.6), ("b", 1.25), ("c", 1.5), ("d", -0.6), ("n", 1.0 / 3));
            DifferentiatesBack(integrand, ("a", -0.75), ("b", -0.7), ("c", 2.5), ("d", 0.4), ("n", 2.3));
        }

        [Fact]
        public void LeavesASumThatIsNotANumber()
        {
            // `m + 2 (1 + n)` is no number multiplied out, and the power is integrated as written.
            var integral = "x^(m+2*(1+n))".ToEntity().Integrate("x").Stringize();
            Assert.Contains("1 + n", integral);
        }
    }
}
