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
    /// The Risch-Norman ansatz on <c>ln(x)</c> alone where a sum below the bar holds x beside it to two
    /// or more powers, with the logarithmic integral among its generators: <c>(1 - ln(x))/(x (x + ln(x)))</c>
    /// is <c>(ln(x + ln(x)) - ln(x))'</c>, and <c>(1 + x)/(ln(x) (x + ln(x)))</c> is
    /// <c>(li(x) + ln(ln(x)) - ln(x + ln(x)))'</c>. Rubi's 3.5. And the error function among them for an
    /// exponential of a quadratic: <c>e^((a + x)^2)/x^2 - 2 a e^((a + x)^2)/x</c> is
    /// <c>(-e^((a + x)^2)/x + sqrt(pi) erfi(a + x))'</c>, Rubi's 2.3, whose terms are not elementary apart.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ARischNormanAnsatzBesideTheLogarithmOfXIntegralTest
    {
        private static readonly (string Symbol, double Value)[] Pins =
        {
            ("a", 1.3), ("b", 0.7), ("c", 0.4), ("d", 0.3), ("f", 0.9), ("g", 1.1), ("k", 0.6), ("h", 0.5),
            ("m", 1.7), ("n", 2.3), ("p", 1.6), ("q", 2.2), ("A", 0.8), ("B", 1.2), ("F", 2.4),
        };

        [Theory]
        [InlineData("(1 - ln(x))/(x*(x + ln(x)))")]
        [InlineData("(1 + x)/(ln(x)*(x + ln(x)))")]
        [InlineData("e^((a + x)^2)/x^2 - 2*e^((a + x)^2)*a/x")]
        public void BesideTheLogarithmOfX(string integrand) => AssertAnswered(integrand);

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
