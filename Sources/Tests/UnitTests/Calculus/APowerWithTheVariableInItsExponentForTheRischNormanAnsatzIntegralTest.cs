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
    /// A power with the variable in its base and its exponent read by the Risch-Norman ansatz as the
    /// exponential it means, <c>B^E = e^(E ln(B))</c>, the part of the exponent free of x kept as a power:
    /// Rubi's 2.3 <c>x^(-2 - 1/x) (1 - ln(x))</c> is <c>(-x^(-1/x))'</c>, and as written it was declined.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class APowerWithTheVariableInItsExponentForTheRischNormanAnsatzIntegralTest
    {
        [Theory]
        [InlineData("x^(-2 - 1/x)*(1 - ln(x))")]
        [InlineData("x^(-1/x)*(1 - ln(x))/x^2")]
        public void IsAnsweredAsAPower(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 40, $"the antiderivative of {integrand} is {text}");
            var derivative = integral.Substitute("C", 0).Differentiate("x");
            var original = integrand.ToEntity();
            foreach (var at in new[] { 0.3, 0.7, 1.3, 2.2, 3.1 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
        }
    }
}
