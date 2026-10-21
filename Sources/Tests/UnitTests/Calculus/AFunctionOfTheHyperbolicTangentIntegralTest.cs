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
    /// A function of <c>tanh(a + b x)</c> or <c>coth(a + b x)</c> alone, not rational in it, under
    /// <c>u = tanh(a + b x)</c>. The trigonometric counterparts were answered under <c>u = tan</c>;
    /// the hyperbolic functions arrive as exponentials, and these timed out or were declined.
    /// Rubi's 6.3 and 6.4.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AFunctionOfTheHyperbolicTangentIntegralTest
    {
        [Theory]
        [InlineData("tanh(x)/sqrt(a + b*tanh(x)^2 + c*tanh(x)^4)")]
        [InlineData("coth(x)/sqrt(a + b*coth(x)^2 + c*coth(x)^4)")]
        [InlineData("tanh(x)/(a + b*tanh(x)^4)^(3/2)")]
        [InlineData("sin(tanh(a + b*x))")]
        [InlineData("cos(tanh(a + b*x))^3")]
        [InlineData("sin(coth(a + b*x))")]
        [InlineData("(b*coth(c + d*x)^4)^(1/3)")]
        public void UnderTheTangent(string integrand) => AnsweredAndCheckedBack(integrand);

        // A power of 1 +- coth alone is a binomial differential in e^(2x), which the exponential
        // substitution writes in a few hundred characters; under u = tanh it was 1,342 to 14,533.
        [Theory]
        [InlineData("(1 + coth(x))^(7/2)")]
        [InlineData("(1 + coth(x))^(1/2)")]
        [InlineData("1/(1 + coth(x))^(3/2)")]
        public void LeavesAPowerOfOnePlusCothToTheExponentialSubstitution(string integrand)
            => Assert.True(AnsweredAndCheckedBack(integrand).Length < 1000, $"{integrand} was answered at length");

        private static string AnsweredAndCheckedBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 0.4).Substitute("d", 0.3);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -2.9, -0.7, 0.3, 0.8, 1.3, 2.1 })
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
            return text;
        }
    }
}
