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
    /// A written sum below the bar whose coefficients share a symbol is that symbol times a
    /// polynomial over the rationals, and is read so: <c>(a u + a)(1 - u^2)</c> is
    /// <c>a (u + 1)(1 - u^2)</c>, whose repeated factor the refactoring over the rationals then
    /// finds. Rubi's <c>tan(x)/(a + a csc(x))</c> and its kin, declined for the shared root
    /// under <c>u = sin(x)</c> and for a symbolic sextic under the half-angle.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class DenominatorContentTest
    {
        private static readonly double[] Points = { 0.4, 1.1, 2.3, 3.6, 5.2 };

        private static void DifferentiatesBack(string integrand, double constant)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Assert.DoesNotContain("NaN", integral.Stringize());

            var derivative = integral.Substitute("C", 0).Substitute("a", constant).Differentiate("x");
            var original = integrand.ToEntity().Substitute("a", constant);
            foreach (var at in Points)
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                var difference = Math.Abs((double)(got - want).RealPart)
                               + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart));
                Assert.True(difference / scale < 1e-8,
                    $"d/dx of the antiderivative of {integrand} at a = {constant} is {got} at x = {at}, "
                    + $"where the integrand is {want}");
            }
        }

        [Theory]
        [InlineData("tan(x)/(a + a*csc(x))")]
        [InlineData("sec(x)^2/(a + a*csc(x))")]
        [InlineData("x^2/((a*x + a)*(1 - x^2))")]
        [InlineData("1/((2*a*x + 4*a)*(x + 2)^2)")]
        public void TheContentComesOutOfTheSum(string integrand)
        {
            DifferentiatesBack(integrand, 2);
            DifferentiatesBack(integrand, -3);
        }
    }
}
