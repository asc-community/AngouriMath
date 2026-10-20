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
    /// Hyperbolic integrands asked as trigonometric ones by Osborn's rule, <c>L = i s</c>:
    /// <c>(b cosh(x) + c sinh(x) + sqrt(b^2 - c^2))^(1/2)</c> is
    /// <c>(b cos(s) + i c sin(s) + sqrt(b^2 - c^2))^(1/2)</c>, which the trigonometric rules answer.
    /// The hyperbolic functions arrive as exponentials, and these timed out or were declined.
    /// Rubi's 6.7.1.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AHyperbolicIntegrandAsATrigonometricOneIntegralTest
    {
        [Theory]
        [InlineData("(b*cosh(x) + c*sinh(x) + sqrt(b^2 - c^2))^(1/2)")]
        [InlineData("1/(b*cosh(x) + c*sinh(x) + sqrt(b^2 - c^2))^(3/2)")]
        [InlineData("(b*cosh(x) + c*sinh(x) - sqrt(b^2 - c^2))^(1/2)")]
        [InlineData("(cosh(x)*coth(x))^(3/2)")]
        [InlineData("1/(a + b*sinh(x) + c*sinh(x)^2)")]
        [InlineData("sinh(x)/(a + b*sinh(x) + c*sinh(x)^2)")]
        // Rubi's 6.5.3.
        [InlineData("coth(a + b*x)/sqrt(c + b*sech(a + b*x))")]
        public void AsTheTrigonometricQuestion(string integrand) => AnsweredAndCheckedBack(integrand);

        private static string AnsweredAndCheckedBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            // b > c, so that the root is real; n not whole.
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 2.7).Substitute("c", 0.4).Substitute("n", 0.6);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -1.9, -0.7, 0.3, 0.8, 1.3, 2.1 })
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

        // Wanting a sum over roots in e^x, these are asked in s only where s does not want one too:
        // there the trigonometric answer stands, in closed form.
        [Theory]
        [InlineData("1/(1 + sinh(x)^4)")]
        [InlineData("1/(1 + cosh(x)^3)")]
        [InlineData("sech(x)^2/(3 - 4*tanh(x)^3)")]
        public void KeepsTheClosedFormWhereSFactorsIntoQuadratics(string integrand)
            => Assert.DoesNotContain("sum(", AnsweredAndCheckedBack(integrand));

        // And where s wants one too, as for a cubic in sin(s) or 1 + cos(s)^8, the second pass answers
        // in e^x, as before the rule.
        [Theory]
        [InlineData("1/(1 + sinh(x)^3)")]
        [InlineData("1/(1 + cosh(x)^8)")]
        public void LeavesTheSumOverRootsToTheSecondPass(string integrand)
            => Assert.Contains("sum(", AnsweredAndCheckedBack(integrand));

        // Asked before the rules in x, the question in s took the time they answer these in.
        [Theory]
        [InlineData("x/cosh(x)^(7/2) + 3/5*x*sqrt(cosh(x))")]
        [InlineData("cosh(a + b*x)^3/(c + b*sqrt(sinh(a + b*x)))^2")]
        [InlineData("(n + c*x)/(c + i*c*sinh(a + b*x))")]
        public void LeavesTheRulesInXTheirIntegrands(string integrand)
            => Assert.DoesNotContain("integral(", integrand.ToEntity().Integrate("x").Stringize());
    }
}
