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
    /// A rational function of a sine or a cosine over a quadratic in it with a symbol among
    /// the coefficients, split over the quadratic's two roots: Rubi's
    /// <c>sin(x)/(a + b sin(x) + c sin(x)^2)</c> and its neighbours.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// Every answer is differentiated back with the coefficients pinned twice: once where the
    /// roots are real and once where they are a conjugate pair, since the closed form for
    /// <c>1/(sin(x) - r)</c> holds for any complex <c>r</c> and a piecewise on the sign of a
    /// root would have no value in the second case.
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class SineOverASymbolicQuadraticTest
    {
        private static readonly double[] Points = { 0.5, 1.2, 2.0, 3.0, 4.0, 5.5 };

        private static void DifferentiatesBack(string integrand, double a, double b, double c)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Assert.DoesNotContain("NaN", integral.Stringize());
            Assert.DoesNotContain("piecewise", integral.Stringize());

            var derivative = integral.Substitute("C", 0).Substitute("a", a).Substitute("b", b).Substitute("c", c).Differentiate("x");
            var original = integrand.ToEntity().Substitute("a", a).Substitute("b", b).Substitute("c", c);
            foreach (var at in Points)
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                var difference = Math.Abs((double)(got - want).RealPart)
                               + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart));
                Assert.True(difference / scale < 1e-8,
                    $"d/dx of the antiderivative of {integrand} at a = {a}, b = {b}, c = {c} is {got} at x = {at}, "
                    + $"where the integrand is {want}");
            }
        }

        [Theory]
        [InlineData("1/(a+b*sin(x)+c*sin(x)^2)")]
        [InlineData("sin(x)/(a+b*sin(x)+c*sin(x)^2)")]
        [InlineData("sin(x)^2/(a+b*sin(x)+c*sin(x)^2)")]
        [InlineData("sin(x)^4/(a+b*sin(x)+c*sin(x)^2)")]
        [InlineData("cos(x)^2/(a+b*sin(x)+c*sin(x)^2)")]
        [InlineData("csc(x)^2/(a+b*sin(x)+c*sin(x)^2)")]
        [InlineData("sec(x)^2/(a+b*sin(x)+c*sin(x)^2)")]
        [InlineData("1/(a+b*cos(x)+c*cos(x)^2)")]
        [InlineData("cos(x)^3/(a+b*cos(x)+c*cos(x)^2)")]
        [InlineData("sin(x)^2/(a+b*cos(x)+c*cos(x)^2)")]
        [InlineData("1/(a+b*sin(2*x+1)+c*sin(2*x+1)^2)")]
        public void OverTheTwoRoots(string integrand)
        {
            DifferentiatesBack(integrand, 1, 5, 2);
            DifferentiatesBack(integrand, 3, 1, 2);
        }
    }
}
