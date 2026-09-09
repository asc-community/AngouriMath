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
    /// Integrands built from <c>sin(x)</c> and <c>cos(x)</c> by the field operations, which the
    /// half-angle substitution <c>t = tan(x/2)</c> turns into a rational function —
    /// <c>sin(x) = 2t/(1 + t^2)</c>, <c>cos(x) = (1 - t^2)/(1 + t^2)</c>, <c>dx = 2/(1 + t^2) dt</c>.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// These are first-year table entries and none of them had an antiderivative. Measured against
    /// Rubi's suite, the band it rates as a <em>single table lookup</em> was the worst-served
    /// difficulty we had — 58%, barely above the 52% for problems needing two or three rules — and
    /// this family is a large part of why.
    /// </para>
    /// <para>
    /// Checked by differentiating back and comparing at points, never against a printed form: the
    /// answers come out in terms of <c>tan(x/2)</c> and their shape says nothing about whether
    /// they differentiate back. <c>int 1/cos(x)</c> in particular comes out as a logarithm of a
    /// quotient of two linear expressions in <c>tan(x/2)</c>, which is correct and is not what a
    /// textbook prints.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class HalfAngleSubstitutionIntegralTest
    {
        /// <summary>
        /// Points inside one branch of <c>tan(x/2)</c>, which has poles at odd multiples of pi.
        /// They stay on <c>(0, pi)</c> and away from both ends, since the substitution produces an
        /// antiderivative per branch rather than one across a pole.
        /// </summary>
        private static readonly double[] Points = { 0.4, 0.9, 1.3, 1.9, 2.4 };

        private static void DifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());

            var derivative = integral.Substitute("C", 0).Differentiate("x");
            var original = integrand.ToEntity();
            var compared = 0;
            foreach (var at in Points)
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                if (got.IsNaN || want.IsNaN)
                    continue;
                compared++;
                var difference = Math.Abs((double)(got - want).RealPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart));
                Assert.True(difference / scale < 1e-9,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, "
                    + $"where the integrand is {want}");
            }
            Assert.True(compared >= 4,
                $"only {compared} of {Points.Length} points were comparable for {integrand}, "
                + "so this asserts almost nothing");
        }

        /// <summary>
        /// The cosecant and the secant. Both are table entries in every calculus text and neither
        /// had an antiderivative; <c>1/sin(x)^2</c> did, through the rule for the cotangent, which
        /// is what made the gap easy to miss.
        /// </summary>
        [Theory]
        [InlineData("1/sin(x)")]
        [InlineData("1/cos(x)")]
        public void TheCosecantAndTheSecant(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// A quotient whose denominator is linear in one of the two, where the substitution
        /// produces something the rational integrator answers directly. <c>1/(1 + cos(x))</c>
        /// becomes <c>int 1 dt</c> exactly, so the answer is <c>tan(x/2)</c>.
        /// </summary>
        [Theory]
        [InlineData("1/(1 + cos(x))")]
        public void ALinearDenominatorInCosine(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// Declined, and each for its own reason, so that the boundary is recorded rather than
        /// assumed. <c>sin(x) + x</c> is not a function of the sine alone. <c>sin(x)*sin(2*x)</c>
        /// leaves an <c>x</c> behind because <c>sin(2x)</c> is not <c>sin(x)</c> — the right
        /// outcome, since a product of sines wants the product-to-sum identity rather than a
        /// rational function in <c>t</c>.
        /// </summary>
        /// <remarks>
        /// These assert only that the rule does not fire wrongly. Should either later be answered
        /// by something else, this test moves to the group above rather than being deleted.
        /// </remarks>
        [Theory]
        [InlineData("sin(x) + x")]
        [InlineData("sin(x) * sin(2*x)")]
        public void NotAFunctionOfSineAndCosineAlone(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            // `sin(x) + x` is answered by linearity, so what is asserted is that the answer does
            // not come from this substitution — no half-angle appears in it.
            Assert.DoesNotContain("tan(x / 2)", integral.Stringize());
        }
    }
}
