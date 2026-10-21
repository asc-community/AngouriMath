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
    /// Rubi's 4.3.4.2, <c>(a + b tan)^m (c + d tan)^n (A + B tan + C tan^2)</c>, with symbols for
    /// every coefficient. Under the tangent the integrand is a quotient of powers of sums of
    /// symbols, which the simplifier handed back unchanged after up to thirteen seconds, and
    /// beside a root of a linear the substitution search simplified each linear candidate's
    /// quotient for five seconds to refuse it for the variable it kept. Neither is done now,
    /// and the answers are the ones they were.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    /// <remarks>
    /// Checked by differentiating back with the symbols pinned after the integral is taken, at
    /// points where the tangent and every factor in it stay away from their poles and zeros and
    /// every radicand is positive. <c>K</c> stands for Rubi's <c>C</c>, which is the constant of
    /// integration here.
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class ARationalFunctionOfTheTangentInLowestTermsIntegralTest
    {
        private static readonly double[] Points = { 0.1, 0.25, 0.4, 0.55, 0.7, 0.85 };

        [Theory]
        [InlineData("(c + d*tan(g + h*x))^2*(A + B*tan(g + h*x) + K*tan(g + h*x)^2)/(a + b*tan(g + h*x))")]
        [InlineData("(c + d*tan(g + h*x))^3*(A + B*tan(g + h*x) + K*tan(g + h*x)^2)/(a + b*tan(g + h*x))^3")]
        [InlineData("(a + b*tan(g + h*x))^2*(A + B*tan(g + h*x) + K*tan(g + h*x)^2)/(c + d*tan(g + h*x))^3")]
        public void WholePowersOfLinearsInTheTangent(string integrand) => AnsweredAndDifferentiatedBack(integrand);

        [Theory]
        [InlineData("sqrt(c + d*tan(g + h*x))*(a + b*tan(g + h*x))^3*(A + B*tan(g + h*x) + K*tan(g + h*x)^2)")]
        [InlineData("(c + d*tan(g + h*x))^(3/2)*(A + B*tan(g + h*x) + K*tan(g + h*x)^2)/(a + b*tan(g + h*x))^3")]
        [InlineData("(a + b*tan(g + h*x))^3*(A + B*tan(g + h*x) + K*tan(g + h*x)^2)/(c + d*tan(g + h*x))^(3/2)")]
        public void BesideAHalfOddPowerOfALinearInTheTangent(string integrand) => AnsweredAndDifferentiatedBack(integrand);

        private static void AnsweredAndDifferentiatedBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Entity Pin(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 2.1).Substitute("d", 0.9)
                .Substitute("g", 0.3).Substitute("h", 1.1).Substitute("A", 0.5).Substitute("B", 1.7).Substitute("K", 0.4);
            var derivative = Pin(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pin(integrand.ToEntity());
            var compared = 0;
            foreach (var point in Points)
            {
                var want = original.Substitute("x", point).EvalNumerical();
                var got = derivative.Substitute("x", point).EvalNumerical();
                if (want.IsNaN)
                    continue;
                compared++;
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-8 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {point}, where the integrand is {want}");
            }
            Assert.True(compared >= 5, $"only {compared} points could be compared for {integrand}");
        }
    }
}
