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
    /// A linear below the cube root of a quadratic, <c>1/((d + e x) (a + b x + c x^2)^(1/3))</c>,
    /// where the shifted quadratic and the linear satisfy <c>c d^2 = 3 a e^2</c>: Rubi's 1.2.1.2
    /// closed form, an arctangent and two logarithms. Sampled on both sides of the linear's root,
    /// and where the quadratic is negative, so the cube root is complex.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ALinearBelowACubeRootOfAQuadraticIntegralTest
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
        [InlineData("1/((1+x^2)^(1/3)*(x+sqrt(3)))")]
        [InlineData("1/((1+x^2)^(1/3)*(-x+sqrt(3)))")]
        [InlineData("1/((2+3*i*x)*(4-27*x^2)^(1/3))")]
        public void WithNumbersIsAnsweredAndDifferentiatesBack(string integrand)
            => DifferentiatesBack(integrand);

        [Theory]
        [InlineData("1/((d+k*x)*(d^2+3*k^2*x^2)^(1/3))")]
        [InlineData("1/((d+k*x)*(c^2*d^2-b*c*d*k+b^2*k^2+3*b*c*k^2*x+3*c^2*k^2*x^2)^(1/3))")]
        public void WithSymbolsIsAnsweredAndDifferentiatesBack(string integrand)
        {
            DifferentiatesBack(integrand, ("b", 2), ("c", 3), ("d", 0.7), ("k", 0.5));
            DifferentiatesBack(integrand, ("b", -2), ("c", 3), ("d", -0.7), ("k", 1.5));
        }

        [Fact]
        public void IsNotAnsweredInClosedFormWithoutTheRelation()
        {
            // c d^2 = 2 a e^2: the integral is elliptic, and no closed form is claimed.
            var integral = "1/((1+x)*(1+2*x^2)^(1/3))".ToEntity().Integrate("x");
            Assert.DoesNotContain("arctan", integral.Stringize());
        }
    }
}
