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
    /// A sum of powers of one inverse trigonometric or error function beside cofactors free of
    /// it, by parts down the powers from the top. Its terms are not elementary apart, so the
    /// split declined each, and the sum was declined or ran out of time. Rubi's 5.1.4, 5.3.4
    /// and 8.1.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ASumOfPowersOfAnInverseOrErrorFunctionByPartsDownThePowersIntegralTest
    {
        [Theory]
        // The parts cancel every power below the top: one step, and two.
        [InlineData("x^3/((1+a^2*x^2)*arctan(a*x)^3)-3/2*x^2/(a*arctan(a*x)^2)")]
        [InlineData("x*arcsin(x)^(3/2)/(1-x^2)^2-3/8*x/((1-x^2)*sqrt(arcsin(x)))")]
        [InlineData("1/((1-x^2)*arcsin(x)^2)-x/((1-x^2)^(3/2)*arcsin(x))")]
        [InlineData("2*x/arccos(x)+x^2/(sqrt(1-x^2)*arccos(x)^2)")]
        [InlineData("erf(x)^2+4*x*erf(x)*e^(-x^2)/sqrt(pi)")]
        // What is left is free of the function, and the cofactors integrate together only.
        [InlineData("erf(b*x)/(e^(b^2*x^2)*x^3)+b^2*erf(b*x)/(e^(b^2*x^2)*x)")]
        [InlineData("erfc(b*x)/(e^(b^2*x^2)*x^3)+b^2*erfc(b*x)/(e^(b^2*x^2)*x)")]
        [InlineData("erfi(b*x)/(e^(b^2*x^2)*x^3)+b^2*erfi(b*x)/(e^(b^2*x^2)*x)")]
        public void ByPartsDownThePowers(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 2000, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            // Inside (0, 1), where the arcsine and the arccosine are real and positive.
            foreach (var at in new[] { 0.15, 0.3, 0.45, 0.6, 0.75, 0.9 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                if (want.IsNaN)
                    continue;
                compared++;
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
            Assert.True(compared >= 5, $"only {compared} points could be compared for {integrand}");
        }
    }
}
