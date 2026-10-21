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
    /// A product whose logarithmic derivative is a constant, or whose exponentials' logarithmic derivative
    /// is the rest of it up to a constant -- with <c>f^g = e^(g ln f)</c> for x in the base and the exponent:
    /// <c>(a F^(c + d x))^m (b F^(k + f x))^n</c>, <c>x^(-2 - 1/x) (1 - ln(x))</c>, <c>cosh(x)^x (ln(cosh(x)) + x tanh(x))</c>.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class TheExponentialsAloneAsTheirOwnDerivativeIntegralTest
    {
        private static readonly (string Symbol, double Value)[] Pins =
        {
            ("a", 1.3), ("b", 0.7), ("c", 0.4), ("d", 0.3), ("f", 0.9), ("g", 1.1), ("k", 0.6), ("h", 0.5),
            ("m", 1.7), ("n", 2.3), ("p", 1.6), ("q", 2.2), ("A", 0.8), ("B", 1.2), ("F", 2.4),
        };

        [Theory]
        [InlineData("(a*F^(c + d*x))^m*(b*F^(k + f*x))^n")]
        [InlineData("F^(a + b*x)*(cosh(c + d*x) + sinh(c + d*x))^n")]
        [InlineData("F^(a + b*x)*(cosh(c + d*x) - sinh(c + d*x))^n")]
        [InlineData("x^(-2 - 1/x)*(1 - ln(x))")]
        [InlineData("cosh(x)^x*(ln(cosh(x)) + x*tanh(x))")]
        [InlineData("x^x*(1 + ln(x))")]
        public void TheirOwnDerivative(string integrand) => AssertAnswered(integrand);

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
