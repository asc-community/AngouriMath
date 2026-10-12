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
    /// A root of <c>a cos(x) + b sin(x)</c> beside a half-odd power of the cosine, read as a
    /// root of <c>a + b tan(x)</c>: <c>csc(x) sqrt(cos(x) + sin(x))/cos(x)^(3/2)</c> is
    /// <c>sqrt(1 + tan(x)) csc(x) sec(x)</c>, and the sine's the same way. Checked where both
    /// roots are imaginary too, since the integrand is real there. Rubi's 4.7.7.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ARootBesideAHalfPowerOfTheCosineIntegralTest
    {
        [Theory]
        [InlineData("csc(x)*sqrt(cos(x) + sin(x))/cos(x)^(3/2)", 200)]
        [InlineData("sqrt(cos(x) + sin(x))/cos(x)^(5/2)", 100)]
        [InlineData("sqrt(2*cos(x) + 3*sin(x))/(sin(x)*cos(x)^(3/2))", 300)]
        [InlineData("sec(x)*sqrt(cos(x) - sin(x))/sin(x)^(3/2)", 200)]
        public void DifferentiatesBack(string integrand, int atMost)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < atMost, $"{text.Length} characters of answer for {integrand}");
            var derivative = integral.Substitute("C", 0).Differentiate("x");
            var original = integrand.ToEntity();
            foreach (var at in new[] { "0.35", "0.6", "3.8", "3.9" })
            {
                var want = original.Substitute("x", at.ToEntity()).EvalNumerical();
                var got = derivative.Substitute("x", at.ToEntity()).EvalNumerical();
                Assert.True(Math.Abs((double)want.ImaginaryPart) < 1e-9, $"{integrand} is not real at x = {at}");
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
        }
    }
}
