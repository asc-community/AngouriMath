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
    /// <c>sqrt(c x^2 + d sqrt(a + b x^4))/(sqrt(a + b x^4) (p + q x)^k)</c> with <c>c^2 = b d^2</c>,
    /// written as <c>sqrt(d/(2 sqrt(b)))</c> times <c>1/sqrt(x^2 + i beta) + 1/sqrt(x^2 - i beta)</c>
    /// over the power, <c>beta = sqrt(a/b)</c>: Rubi's 1.3.2
    /// <c>sqrt(2 x^2 + sqrt(3 + 4 x^4))/((c + d x) sqrt(3 + 4 x^4))</c> and its square, declined before.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    /// <remarks>
    /// The answer is checked by differentiating back and also against Simpson's rule on an
    /// interval to the left of the pole: the logarithm of a quotient, which the table writes
    /// for a real root, jumps there for an imaginary one, and only its value says so.
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class ARootOverTheRootInsideItBesideAPowerOfALinearIntegralTest
    {
        [Theory]
        [InlineData("sqrt(2*x^2 + sqrt(3 + 4*x^4))/((c + d*x)*sqrt(3 + 4*x^4))")]
        [InlineData("sqrt(2*x^2 + sqrt(3 + 4*x^4))/((c + d*x)^2*sqrt(3 + 4*x^4))")]
        [InlineData("sqrt(2*x^2 + sqrt(3 + 4*x^4))/((c + d*x)^3*sqrt(3 + 4*x^4))")]
        [InlineData("sqrt(3*x^2 + sqrt(2 + 9*x^4))/((c + d*x)*sqrt(2 + 9*x^4))")]
        [InlineData("sqrt(2*x^2 + sqrt(3 + 4*x^4))/((1 + 2*x)*sqrt(3 + 4*x^4))")]
        public void IsIntegrated(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 4000, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e.Substitute("c", 0.7).Substitute("d", 1.3);
            var antiderivative = Pinned(integral.Substitute("C", 0));
            var derivative = antiderivative.Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -3.1, -1.7, -0.9, 0.2, 1.3, 4.6 })
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

            // On [-3, -1], left of every pole here, F(-1) - F(-3) is the integral.
            double At(Entity e, double point) => (double)e.Substitute("x", point).EvalNumerical().RealPart;
            const int steps = 200;
            var (from, to) = (-3.0, -1.0);
            var width = (to - from) / steps;
            var simpson = At(original, from) + At(original, to);
            for (var step = 1; step < steps; step++)
                simpson += (step % 2 == 1 ? 4 : 2) * At(original, from + step * width);
            simpson *= width / 3;
            var difference = antiderivative.Substitute("x", to).EvalNumerical() - antiderivative.Substitute("x", from).EvalNumerical();
            Assert.True(Math.Abs((double)difference.ImaginaryPart) < 1e-9, $"F(-1) - F(-3) is {difference} for {integrand}");
            Assert.True(Math.Abs((double)difference.RealPart - simpson) < 1e-6 * Math.Max(1, Math.Abs(simpson)),
                $"F(-1) - F(-3) is {difference} for {integrand}, where Simpson's rule gives {simpson}");
        }
    }
}
