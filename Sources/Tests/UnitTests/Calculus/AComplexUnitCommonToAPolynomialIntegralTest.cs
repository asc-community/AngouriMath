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
    /// A complex unit common to a polynomial's coefficients, taken out before the polynomial is expanded,
    /// and a whole power of a quotient distributed under a root-of-a-quotient substitution: Rubi's 5.3.6
    /// <c>1/(e^(5/2 i arctan(a x)) x^3)</c>, which was left unevaluated, and 7.4.2's
    /// <c>1/(e^(arccoth(a x)) (c - a^2 c x^2)^2)</c>, which was answered wrongly.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AComplexUnitCommonToAPolynomialIntegralTest
    {
        private static void DifferentiatesBack(string integrand, double[] points, params (string, double)[] pins)
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
            foreach (var at in points)
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
        [InlineData("1/(e^(5/2*i*atan(a*x))*x^3)")]
        [InlineData("1/(e^(5/2*i*atan(a*x))*x^4)")]
        public void AnExponentialOfAnArctangentOverAPowerIsAnswered(string integrand)
        {
            DifferentiatesBack(integrand, new[] { -2.3, -0.7, 0.4, 1.9 }, ("a", 1.3));
            DifferentiatesBack(integrand, new[] { -2.3, -0.7, 0.4, 1.9 }, ("a", -0.5));
        }

        [Fact]
        public void APolynomialWithACommonUnitIsAnswered()
            => DifferentiatesBack("1/(x^2*(i+i*x^4)^3)", new[] { -2.3, -0.7, 0.4, 1.9 });

        [Theory]
        [InlineData("1/(e^acoth(a*x)*(c-a^2*c*x^2)^2)")]
        [InlineData("1/(e^acoth(a*x)*(c-a^2*c*x^2)^3)")]
        public void AnExponentialOfAnArccotangentIsAnsweredRightly(string integrand)
        {
            DifferentiatesBack(integrand, new[] { -2.3, -1.6, 1.4, 1.9 }, ("a", 1.3), ("c", 0.7));
            DifferentiatesBack(integrand, new[] { -4.5, -2.6, 2.4, 3.9 }, ("a", -0.5), ("c", 2.5));
        }
    }
}
