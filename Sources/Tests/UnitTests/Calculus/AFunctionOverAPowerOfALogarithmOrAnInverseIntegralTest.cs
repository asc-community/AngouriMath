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
    /// The derivative of <c>U/V^k</c> for <c>V</c> a logarithm or an inverse trigonometric function, whose
    /// terms are not elementary apart: <c>U = -B/(k V')</c> for the terms <c>B</c> over <c>V^(k + 1)</c>; and
    /// for <c>V</c> a sum with a logarithm in it, <c>U</c> a monomial of the numerator, read monomial by
    /// monomial. Rubi's 3.2.3, 3.5 and 5.3.4 rows ran past the budget.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AFunctionOverAPowerOfALogarithmOrAnInverseIntegralTest
    {
        private static readonly (string Symbol, double Value)[] Pins =
        {
            ("a", 1.3), ("b", 0.7), ("c", 0.4), ("d", 0.3), ("f", 0.9), ("g", 1.1), ("k", 0.6), ("h", 0.5),
            ("m", 1.7), ("n", 2.3), ("p", 1.6), ("q", 2.2), ("A", 0.8), ("B", 1.2), ("F", 2.4),
        };

        [Theory]
        [InlineData("1/((c + d*x)*(-a + c + (-b + d)*x)*ln((a + b*x)/(c + d*x))) + ln(1 + (-a - b*x)/(c + d*x))/((a + b*x)*(c + d*x)*ln((a + b*x)/(c + d*x))^2)")]
        [InlineData("(-1)/((a + b*x)*(a - c + (b - d)*x)*ln((a + b*x)/(c + d*x))) + ln(1 + (-c - d*x)/(a + b*x))/((a + b*x)*(c + d*x)*ln((a + b*x)/(c + d*x))^2)")]
        [InlineData("x^3/((1 + a^2*x^2)*atan(a*x)^3) - 3/2*x^2/(a*atan(a*x)^2)")]
        [InlineData("(a*d*n*x^m - a*d*m*x^m*ln(c*x^n) - b*d*n*(q - 1)*ln(c*x^n)^q)/(x*(a*x^m + b*ln(c*x^n)^q)^2)")]
        public void AsTheDerivativeOfAQuotient(string integrand) => AssertAnswered(integrand);

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
