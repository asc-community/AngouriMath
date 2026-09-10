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
    /// A denominator whose factorisation holds an irreducible <em>biquadratic</em> quartic is
    /// split, because that quartic is a shape an integration rule reads.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// The coprime split declined any irreducible factor past degree two, on the stated grounds
    /// that nothing read one. That stopped being true when
    /// <c>TrySplitBiquadraticOverTheReals</c> arrived: it factors <c>x^4 + p x^2 + q</c> into two
    /// real quadratics. So <c>1/(1 + x^6)</c> was declined although the library factors
    /// <c>x^6 + 1</c> into <c>(x^2 + 1)(x^4 - x^2 + 1)</c> and integrates each of those on its
    /// own.
    /// </para>
    /// <para>
    /// The guard is a claim about the rule set rather than about polynomials, which is why it
    /// went stale; these tests are what would catch it going stale again.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class BiquadraticFactorIntegralTest
    {
        private static readonly double[] Points = { -0.5, 0.23, 0.61, 1.05, 2.3 };

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
        /// <c>x^6 + 1</c> is <c>(x^2 + 1)(x^4 - x^2 + 1)</c>, and the quartic is biquadratic.
        /// None of these had an antiderivative.
        /// </summary>
        [Theory]
        [InlineData("1/(1 + x^6)")]
        [InlineData("x^2/(1 + x^6)")]
        [InlineData("1/(x*(1 + x^6))")]
        public void ADenominatorWithABiquadraticFactor(string integrand)
            => DifferentiatesBack(integrand);

        /// <summary>
        /// The case the guard's own comment names as the reason it exists —
        /// <c>1 + x^4 + x^8</c> factors with the irreducible quartic <c>x^4 - x^2 + 1</c> in it.
        /// It used to decline; it now answers, which is the trade this change makes.
        /// </summary>
        [Theory]
        [InlineData("(1 - x^4)/(1 + x^4 + x^8)")]
        public void TheCaseTheGuardWasWrittenFor(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// What was already answered and is answered by the same route. The widening admits one
        /// shape and must admit no others.
        /// </summary>
        [Theory]
        [InlineData("1/(1 + x^4)")]
        [InlineData("1/(x^4 + 3*x^2 + 2)")]
        [InlineData("1/(1 + x^2)")]
        [InlineData("1/(x^4 - x^2 + 1)")]
        [InlineData("1/((1 + x)^3*(2 + x)^3)")]
        [InlineData("1/(1 + x^3)")]
        public void WhatWasAlreadyAnswered(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// Still declined, and each for its own reason, so the boundary is recorded rather than
        /// assumed. <c>x^6 + 2</c> is irreducible over the rationals, so there is nothing to
        /// split. <c>x^12 + 1</c> factors as <c>(x^4 + 1)(x^8 - x^4 + 1)</c>, and the degree-8
        /// factor has no rule — biquadratic reads a quartic and nothing higher. <c>x^5 + 1</c>
        /// leaves a general quartic, which is not biquadratic.
        /// </summary>
        [Theory]
        [InlineData("1/(2 + x^6)")]
        [InlineData("1/(1 + x^12)")]
        [InlineData("1/(1 + x^5)")]
        public void WhatIsStillDeclined(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            // If something else later answers one of these, it must answer it correctly.
            if (!integral.Stringize().Contains("integral("))
                DifferentiatesBack(integrand);
        }
    }
}
