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
    /// A power of x times <c>ln(A + B sqrt(Q))</c>, by parts, with the logarithm's derivative written
    /// over the conjugate, <c>D = A^2 - B^2 Q</c>: a rational function plus one over the root. The
    /// search differentiated it as written and ran past its budget. Rubi's 3.5.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class APowerTimesALogarithmOfARadicalSumIntegralTest
    {
        [Theory]
        [InlineData("ln(-1 + 4*x + 4*sqrt((x - 1)*x))")]
        [InlineData("x^3*ln(-1 + 4*x + 4*sqrt((x - 1)*x))")]
        [InlineData("x*ln(2*x + sqrt(x^2 + 3))")]
        public void ByPartsOverTheConjugate(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            var derivative = integral.Substitute("C", 0).Differentiate("x");
            var original = integrand.ToEntity();
            var compared = 0;
            // Where the root is real and the logarithm's argument positive.
            foreach (var at in new[] { 1.2, 1.7, 2.3, 3.1, 4.4, 6.9 })
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
