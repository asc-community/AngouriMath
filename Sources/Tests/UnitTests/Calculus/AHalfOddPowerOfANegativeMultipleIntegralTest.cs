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
    /// A power of a product that is not whole is not the product of the powers: <c>(b u)^(5/2)</c>
    /// is <c>-b^(5/2) u^(5/2)</c> at <c>b &lt; 0</c>, <c>u &lt; 0</c>. The long division read it so,
    /// and the integrator answered <c>(b u)^(5/2) (u^2 - 1)/u</c>, and through it Rubi's 4.3.0
    /// <c>(b sec(x))^m tan(x)^3</c> at a half-odd m, wrongly wherever <c>b sec(x) &gt; 0</c> with
    /// <c>b &lt; 0</c>. A negative number did the same: <c>sqrt(-x) (x + 1)/x</c> was integrated, and
    /// simplified, as <c>i (sqrt(x) + x^(-1/2))</c>, the negative of it for every <c>x &lt; 0</c>.
    /// Checked at both signs of b and of the variable.
    /// https://github.com/asc-community/AngouriMath/issues/233
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AHalfOddPowerOfANegativeMultipleIntegralTest
    {
        private static readonly double[] AlgebraicPoints = { -2.1, -1.3, -0.6, 0.5, 1.2, 2.3 };
        // Both signs of the secant and of the cosecant.
        private static readonly double[] TrigonometricPoints = { 0.4, 0.9, 2.0, 2.5, 3.0, 3.5, 4.0, 4.4 };

        [Theory]
        [InlineData("(b*x)^(5/2)*(x^2 - 1)/x", -0.4)]
        [InlineData("(b*x)^(5/2)*(x^2 - 1)/x", 0.4)]
        [InlineData("(b*x)^(5/2)*(x + 1)/x", -0.4)]
        [InlineData("(b*x)^(5/2)*(x + 1)/x", 0.4)]
        [InlineData("(b*x)^(3/2)*(x^2 + 1)/x", -0.4)]
        [InlineData("(b*x)^(5/2)*x", -0.4)]
        [InlineData("sqrt(b*x)*(1 + x)", -0.4)]
        [InlineData("sqrt(-x)*(x + 1)/x", 0.0)]
        [InlineData("(-x)^(3/2)*(x^2 - 1)/x", 0.0)]
        [InlineData("(-3*x)^(5/2)*(x + 1)/x", 0.0)]
        public void APowerOfAMultipleOfTheVariable(string integrand, double b)
            => DifferentiatesBack(integrand, b, AlgebraicPoints);

        [Theory]
        [InlineData("(b*sec(x))^(5/2)*tan(x)^3", -0.4)]
        [InlineData("(b*sec(x))^(5/2)*tan(x)^3", 0.4)]
        [InlineData("(b*sec(x))^(3/2)*tan(x)^3", -0.4)]
        [InlineData("(b*sec(x))^(1/2)*tan(x)^5", -0.4)]
        [InlineData("(b*sec(x))^(-1/2)*tan(x)^3", -0.4)]
        [InlineData("cot(x)^3*(b*csc(x))^(3/2)", -0.4)]
        [InlineData("cot(x)^3*(b*csc(x))^(3/2)", 0.4)]
        [InlineData("cot(x)^5*(b*csc(x))^(1/2)", -0.4)]
        public void APowerOfAMultipleOfTheSecantOrTheCosecant(string integrand, double b)
            => DifferentiatesBack(integrand, b, TrigonometricPoints);

        [Theory]
        [InlineData("sqrt(-x)*(x + 1)/x")]
        [InlineData("(b*x)^(5/2)*(x + 1)/x")]
        [InlineData("(b*x)^(3/2)*(x^2 + 1)/x")]
        public void TheSimplificationKeepsTheValue(string written)
        {
            var expression = written.ToEntity();
            var simplified = expression.Simplify();
            foreach (var at in AlgebraicPoints)
            {
                Entity Pinned(Entity e) => e.Substitute("b", -0.4).Substitute("x", at);
                var want = Pinned(expression).EvalNumerical();
                var got = Pinned(simplified).EvalNumerical();
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"{written} simplified to {simplified} is {got} at x = {at}, b = -0.4, where it is {want}");
            }
        }

        [Fact]
        public void TheLongDivisionKeepsTheMultipleUnderThePower()
        {
            var u = MathS.Var("u");
            var dividend = "(b*u)^(5/2)*(u + 1)".ToEntity();
            var divisor = (Entity)u;
            if (Functions.TreeAnalyzer.PolynomialLongDivision(dividend, divisor, genericCase: true, inTermsOf: u) is not var (quotient, remainder))
                return;
            foreach (var (b, at) in new[] { (-0.4, -1.3), (-0.4, 0.7), (0.4, -1.3), (0.4, 0.7) })
            {
                Entity Pinned(Entity e) => e.Substitute("b", b).Substitute(u, at);
                var got = Pinned(quotient + remainder).EvalNumerical();
                var want = Pinned(dividend / divisor).EvalNumerical();
                Assert.True(((got - want).Abs() as Entity.Number.Real)!.EDecimal.ToDouble() < 1e-9 * Math.Max(1, (want.Abs() as Entity.Number.Real)!.EDecimal.ToDouble()),
                    $"the quotient and remainder are {got} at b = {b}, u = {at}, where the division is {want}");
            }
        }

        private static void DifferentiatesBack(string integrand, double b, double[] points)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Entity Pinned(Entity e) => e.Substitute("b", b);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in points)
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                if (want.IsNaN)
                    continue;
                compared++;
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/dx of {text}, the antiderivative of {integrand}, is {got} at x = {at}, b = {b}, where the integrand is {want}");
            }
            Assert.True(compared >= 5, $"only {compared} points could be compared for {integrand}");
        }
    }
}
