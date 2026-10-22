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
    /// A transcendental function of one shifted linear alone, written in <c>u = c + d x</c>, where a
    /// power of the linear stands below the bar inside a sum or the linear is written two ways. Rubi's
    /// <c>F^(a + b/(c + d x)^3)/(c + d x)^19</c> ran past the budget in x, and is 20 ms in u.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ATranscendentalFunctionOfOneShiftedLinearIntegralTest
    {
        private static readonly (string Symbol, double Value)[] Pins =
        {
            ("a", 1.3), ("b", 0.7), ("c", 0.4), ("d", 0.3), ("f", 0.9), ("g", 1.1), ("k", 0.6), ("h", 0.5),
            ("m", 1.7), ("n", 2.3), ("p", 1.6), ("q", 2.2), ("A", 0.8), ("B", 1.2), ("F", 2.4),
        };

        [Theory]
        [InlineData("F^(a + b/(c + d*x)^3)/(c + d*x)^19")]
        [InlineData("F^(a + b/(c + d*x)^3)/(c + d*x)^16")]
        [InlineData("F^(a + b/(c + d*x)^2)/(c + d*x)^10")]
        [InlineData("ln(c*(d + k/(f + g*x)^3)^q)")]
        [InlineData("(k*g + f*g*x)^(p - 1)/ln(d*(k + f*x)^p)")]
        public void WrittenInTheShiftedLinear(string integrand) => AssertAnswered(integrand);

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
