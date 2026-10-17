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
    /// A constant common to the whole denominator after the half-angle substitution, divided out:
    /// <c>x/(k + k sin(x))^2</c> was <c>NaN</c>, and so was Rubi's 5.1.4
    /// <c>(a + b asin(c x))/((d + c d x)^(5/2) sqrt(f - c f x))</c>, which comes to it.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AConstantCommonToAHalfAngleDenominatorIntegralTest
    {
        private static void DifferentiatesBack(string integrand, params (string, double)[] pins)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Assert.DoesNotContain("NaN", integral.Stringize());

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
        [InlineData("x/(k+k*sin(x))^2")]
        [InlineData("1/(d*q+q*d*sin(x))^2")]
        [InlineData("(a+b*asin(q*x))/((d+q*d*x)^(5/2)*sqrt(f-q*f*x))")]
        public void IsAnsweredAndDifferentiatesBack(string integrand)
        {
            // |q x| below 1 at every sampled x, where the inverse sine and the roots are real.
            DifferentiatesBack(integrand, ("k", 1.3), ("a", 0.7), ("b", -0.4), ("d", 1.1), ("f", 1.5), ("q", 0.3));
            DifferentiatesBack(integrand, ("k", -0.7), ("a", -2), ("b", 1.5), ("d", 0.6), ("f", 2), ("q", -0.2));
        }
    }
}
