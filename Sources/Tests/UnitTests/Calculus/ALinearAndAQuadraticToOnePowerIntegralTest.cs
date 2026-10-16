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
    /// A linear and a quadratic to one power whose product is a binomial in <c>x^3</c>,
    /// integrated over the binomial: <c>(1 + x) (1 - x + x^2)</c> is <c>1 + x^3</c>. Rubi's
    /// 1.2.1.2 rows, sampled on both sides of the linear's root, where the cube roots are complex.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ALinearAndAQuadraticToOnePowerIntegralTest
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
        [InlineData("1/((1+x)^(1/3)*(1-x+x^2)^(1/3))")]
        [InlineData("1/((1-x)^(1/3)*(1+x+x^2)^(1/3))")]
        public void WithNumbersIsAnsweredAndDifferentiatesBack(string integrand)
            => DifferentiatesBack(integrand);

        [Fact]
        public void WithSymbolsIsAnsweredAndDifferentiatesBack()
        {
            const string integrand = "1/((b*k-c*k*x)^(1/3)*(b^2+b*c*x+c^2*x^2)^(1/3))";
            DifferentiatesBack(integrand, ("b", 2), ("c", 3), ("k", 0.7));
            DifferentiatesBack(integrand, ("b", -2), ("c", 3), ("k", -1.5));
        }
    }
}
