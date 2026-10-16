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
    /// Two linears to one power that is not whole, integrated as one quadratic to that power with
    /// <c>L1^p L2^p/(L1 L2)^p</c> in front, which is constant on each interval it is continuous on.
    /// Rubi's 1.1.1.3, which declined the quarter powers and took forty-five seconds over the
    /// powers of <c>5/2</c>.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class TwoLinearsToOnePowerIntegralTest
    {
        private static void DifferentiatesBack(string integrand, Entity[] points, params (string, double)[] pins)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());

            var pinned = integral.Substitute("C", 0);
            Entity original = integrand.ToEntity();
            foreach (var (name, value) in pins)
            {
                pinned = pinned.Substitute(name, value);
                original = original.Substitute(name, value);
            }
            var derivative = pinned.Differentiate("x");
            foreach (var at in points)
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                var difference = Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart));
                Assert.True(difference / scale < 1e-8,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
        }

        private static readonly Entity[] TheLine = { "-23/10", "-7/10", "-3/10", "2/5", "9/10", "5/2" };

        // Where 1 - 2x and 3 + 5x are positive: outside, the integrand is complex and the answer's
        // arcsin is past its real interval, which the evaluator reads as NaN at its default
        // precision; the answer agrees with the integrand there to 2000 digits.
        private static readonly Entity[] BetweenTheRoots = { "-11/20", "-3/10", "0", "1/5", "9/20" };

        [Theory]
        [InlineData("(e*x)^(3/2)/((1-x)^(1/4)*(1+x)^(1/4))")]
        [InlineData("1/((1-x)^(1/4)*(e*x)^(1/2)*(1+x)^(1/4))")]
        public void TwoQuarterPowers(string integrand)
        {
            DifferentiatesBack(integrand, TheLine, ("e", 1.5));
            DifferentiatesBack(integrand, TheLine, ("e", -0.75));
        }

        [Theory]
        [InlineData("(2+3*x)^4/((1-2*x)^(5/2)*(3+5*x)^(5/2))")]
        [InlineData("1/((1-2*x)^(5/2)*(2+3*x)^2*(3+5*x)^(5/2))")]
        public void TwoPowersOfFiveHalves(string integrand) => DifferentiatesBack(integrand, BetweenTheRoots);
    }
}
