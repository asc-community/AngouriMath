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
    /// Powers that are not whole of polynomials that are constant multiples of powers of one
    /// linear, gathered into one power of it with the constant quotient in front: Rubi's 1.1.1.2
    /// <c>1/(sqrt(-2 - b x) sqrt(2 + b x))</c>, 1.1.1.3 <c>(a + b x)^n/(x (-a - b x)^n)</c> and 1.3.1
    /// <c>(a^3 + 3 a^2 b x + 3 a b^2 x^2 + b^3 x^3)^p</c>. Sampled on both sides of the linear's root.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class PowersOfOneLinearAsOnePowerIntegralTest
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
        [InlineData("1/(sqrt(-2-b*x)*sqrt(2+b*x))")]
        [InlineData("sqrt(a+b*x)/(x*sqrt(-a-b*x))")]
        [InlineData("(a+b*x)^n/(x*(-a-b*x)^n)")]
        [InlineData("(a^3+3*a^2*b*x+3*a*b^2*x^2+b^3*x^3)^n")]
        public void WithSymbolsIsAnsweredAndDifferentiatesBack(string integrand)
        {
            DifferentiatesBack(integrand, ("a", 0.6), ("b", 1.5), ("n", 1.0 / 3));
            DifferentiatesBack(integrand, ("a", -0.75), ("b", -0.7), ("n", 2.3));
        }

        [Theory]
        [InlineData("((a+b*x)^3)^(1/3)", 40)]
        [InlineData("x*(-a-b*x)^(3/2)/(a+b*x)^(3/2)", 80)]
        public void LeavesWhatTheRulesBeforeItAnswer(string integrand, int length)
        {
            // Asked before them, the rule wrote these in 111 and 106 characters, where they write
            // 25 and 61.
            var integral = integrand.ToEntity().Integrate("x").Stringize();
            Assert.DoesNotContain("integral(", integral);
            Assert.True(integral.Length < length, integral);
        }
    }
}
