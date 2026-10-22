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
    /// Powers of two linears beside a negative power of a logarithm of their quotient, which under
    /// <c>t = (a + b x)/(c + d x)</c> are a power of t and a polynomial beside the logarithm alone,
    /// onto the exponential integral: Rubi's 3.2.1 rows with the powers of the linears summing to
    /// <c>-2</c> or <c>-3</c>, which ran past the budget. And the square below the bar the substitution
    /// <c>u = ln(k t)</c> writes, whose discriminant is zero: its two roots were one, and the answer had
    /// no value at any point.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class TheQuotientOfTheLogarithmsLinearsOntoTheExponentialIntegralTest
    {
        private static readonly (string Symbol, double Value)[] Pins =
        {
            ("a", 1.3), ("b", 0.7), ("c", 0.4), ("d", 0.3), ("f", 0.9), ("g", 1.1), ("k", 0.6), ("h", 0.5),
            ("m", 1.7), ("n", 2.3), ("p", 1.6), ("q", 2.2), ("A", 0.8), ("B", 1.2), ("F", 2.4),
        };

        [Theory]
        [InlineData("1/((a*g + b*g*x)^2*(A + B*ln(k*((a + b*x)/(c + d*x))^n))^2)")]
        [InlineData("1/((a*g + b*g*x)^2*(A + B*ln(k*(a + b*x)/(c + d*x)))^2)")]
        [InlineData("1/((a*g + b*g*x)^2*(A + B*ln(k*(a + b*x)^2/(c + d*x)^2))^2)")]
        [InlineData("1/((a*g + b*g*x)^2*(A + B*ln(k*(c + d*x)^2/(a + b*x)^2)))")]
        [InlineData("1/((a*g + b*g*x)^3*(A + B*ln(k*(a + b*x)/(c + d*x)))^2)")]
        public void OntoTheExponentialIntegral(string integrand) => AssertAnswered(integrand);

        [Theory]
        [InlineData("1/(x^2*(A + B*ln(k*x))^2)")]
        [InlineData("1/(x^2*(A + B*ln(k*x^2))^2)")]
        public void OverASquareWithAZeroDiscriminant(string integrand) => AssertAnswered(integrand);

        private static void AssertAnswered(string integrand, string variable = "x")
        {
            var integral = integrand.ToEntity().Integrate(variable);
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 5000, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e)
            {
                foreach (var (symbol, value) in Pins)
                    if (symbol != variable)
                        e = e.Substitute(symbol, value);
                return e;
            }
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate(variable);
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { 0.3, 0.8, 1.3, 1.9, 2.9, 3.7 })
            {
                var want = original.Substitute(variable, at).EvalNumerical();
                var got = derivative.Substitute(variable, at).EvalNumerical();
                if (want.IsNaN)
                    continue;
                compared++;
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/d{variable} of the antiderivative of {integrand} is {got} at {variable} = {at}, where the integrand is {want}");
            }
            Assert.True(compared >= 5, $"only {compared} points could be compared for {integrand}");
        }
    }
}
