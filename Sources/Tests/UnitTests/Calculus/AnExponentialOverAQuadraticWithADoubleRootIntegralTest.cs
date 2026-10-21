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
    /// An exponential over a quadratic whose discriminant is zero as a value and not as written,
    /// <c>a x^2 + 2 a x + a</c>, or <c>a^2 + 2 a b t + b^2 t^2</c>, which is what
    /// <c>1/(x^2 (a + b ln(c x))^2)</c> is under <c>t = ln(c x)</c>. Split over two roots that
    /// coincide, the answer carried <c>sqrt((2 a b)^2 - 4 a^2 b^2)</c>, and its derivative had no
    /// value at any point. Each is answered with an antiderivative that differentiates back
    /// wherever the integrand is real, or left unevaluated, and never answered with no value.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1879">#1879</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AnExponentialOverAQuadraticWithADoubleRootIntegralTest
    {
        private static readonly double[] Positive = { 0.31, 0.57, 0.83, 1.29, 1.77, 2.41 };
        private static readonly double[] BothSides = { -2.3, -1.7, -0.6, -0.3, 0.4, 1.9 };

        private static void HasAValueAndDifferentiatesBack(string integrand, Entity integral, double[] points, params (string, double)[] pins)
        {
            using var _ = MathS.Settings.DowncastingEnabled.Set(false);
            var derivative = integral.Substitute("C", 0).Differentiate("x");
            Entity original = integrand.ToEntity();
            foreach (var (name, value) in pins)
            {
                derivative = derivative.Substitute(name, value);
                original = original.Substitute(name, value);
            }
            var compared = 0;
            foreach (var at in points)
            {
                var want = original.Substitute("x", at).EvalNumerical();
                if (want.IsNaN)
                    continue;
                var got = derivative.Substitute("x", at).EvalNumerical();
                Assert.False(got.IsNaN,
                    $"d/dx of the antiderivative of {integrand} has no value at x = {at}, where the integrand is {want}");
                compared++;
                var difference = Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart));
                Assert.True(difference / scale < 1e-8,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
            Assert.True(compared >= 3, $"only {compared} of {points.Length} points were comparable for {integrand}");
        }

        [Theory]
        [InlineData("1/(x^2*(a+b*ln(c*x))^2)", true)]
        [InlineData("1/(x^3*(a+b*ln(c*x))^2)", true)]
        [InlineData("1/(x^4*(a+b*ln(c*x))^2)", true)]
        [InlineData("1/(x^2*(a+b*ln(c*x))^3)", true)]
        [InlineData("1/(x^2*(a+b*ln(c*x^2))^2)", true)]
        [InlineData("1/(x^2*(a+b*ln(c*x^n))^2)", true)]
        [InlineData("1/(x^(3/2)*(a+b*ln(c*x))^2)", true)]
        [InlineData("1/(x^2*(a+b*ln(c/x))^2)", true)]
        [InlineData("ln(c*x)/(x^2*(a+b*ln(c*x))^2)", true)]
        [InlineData("e^x/(a*x^2+2*a*x+a)", false)]
        [InlineData("cosh(x)/(a*x^2+2*a*x+a)", false)]
        [InlineData("(e^x+e^(2*x))/(a*x^2+2*a*x+a)", false)]
        [InlineData("e^x/(a*x^2+2*a*b*x+a*b^2)", false)]
        [InlineData("e^x/((c+d)*x^2+2*(c+d)*x+(c+d))", false)]
        [InlineData("e^x/(x*(a*x^2+2*a*x+a))", false)]
        [InlineData("sinh(x)/(x^2*(a*x^2+2*a*x+a))", false)]
        [InlineData("e^x/((x-1)*(a*x^2+2*a*x+a)*(x+2))", false)]
        public void IsAnsweredWithAValueOrLeftUnevaluated(string integrand, bool logarithmic)
        {
            var integral = integrand.ToEntity().Integrate("x");
            if (integral.Stringize().Contains("integral("))
                return;
            var points = logarithmic ? Positive : BothSides;
            HasAValueAndDifferentiatesBack(integrand, integral, points, ("a", 0.7), ("b", 1.3), ("c", 1.1), ("d", 0.9), ("n", 2));
            HasAValueAndDifferentiatesBack(integrand, integral, points, ("a", 0.7), ("b", -1.3), ("c", 1.1), ("d", 0.9), ("n", 2));
            HasAValueAndDifferentiatesBack(integrand, integral, points, ("a", -0.7), ("b", 1.3), ("c", 2.5), ("d", -0.4), ("n", 3));
            HasAValueAndDifferentiatesBack(integrand, integral, points, ("a", -0.7), ("b", -1.3), ("c", 2.5), ("d", -0.4), ("n", 3));
        }
    }
}
