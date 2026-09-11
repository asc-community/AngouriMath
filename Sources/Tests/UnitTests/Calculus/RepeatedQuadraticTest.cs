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
    /// A repeated irreducible quadratic beside a negative power of the variable, which the
    /// rational split does not take apart and the tangent substitution does.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>1/(x^3 (1 + x^2)^2)</c> came out and <c>1/(x (1 + x^2)^2)</c> did not. Under
    /// <c>x = tan(t)</c> the second is <c>sin^(-1) cos^3</c> — both exponents whole, which the
    /// recurrences close — and the same substitution that
    /// <a href="https://github.com/asc-community/AngouriMath/pull/1273">#1273</a> uses for a
    /// radical works for a whole power of the same quadratic.
    /// </para>
    /// <para>
    /// <b>Ordered below partial fractions rather than beside it.</b> A radical is nobody else's
    /// and is taken straight away; a rational function is the rational machinery's, and everything
    /// it answers keeps the form it gives. This rule only ever sees what that declined, which is
    /// what makes the extension safe rather than a competition between two right answers.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class RepeatedQuadraticTest
    {
        private static readonly double[] Points = { 0.21, 0.35, 0.52, 0.71, 0.83 };

        private static void DifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("NaN", integral.Stringize());
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
                var difference = Math.Abs((double)(got - want).RealPart)
                               + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart));
                Assert.True(difference / scale < 1e-9,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, "
                    + $"where the integrand is {want}");
            }
            Assert.True(compared >= 4,
                $"only {compared} of {Points.Length} points were comparable for {integrand}, "
                + "so this asserts almost nothing");
        }

        /// <summary>The ones that were declined: the quadratic repeated, over a power of x.</summary>
        [Theory]
        [InlineData("1/(x*(1 + x^2)^2)")]
        [InlineData("1/(x^2*(1 + x^2)^2)")]
        [InlineData("1/(x*(1 + x^2)^3)")]
        [InlineData("1/(x*(4 + x^2)^2)")]
        [InlineData("1/(x^2*(1 + x^2)^3)")]
        public void ARepeatedQuadraticOverAPowerOfTheVariable(string integrand)
            => DifferentiatesBack(integrand);

        /// <summary>
        /// The same family where the rational split already coped, which must keep the answer it
        /// gives — these reach partial fractions first and never see this rule at all.
        /// </summary>
        [Theory]
        [InlineData("1/(x^3*(1 + x^2)^2)")]
        [InlineData("1/(1 + x^2)^2")]
        [InlineData("1/(1 + x^2)^3")]
        [InlineData("x/(1 + x^2)^2")]
        [InlineData("x^2/(1 + x^2)^2")]
        [InlineData("x^3/(1 + x^2)^2")]
        [InlineData("x^4/(1 + x^2)^3")]
        [InlineData("1/(x^2*(1 + x^2))")]
        [InlineData("1/(x^3*(1 + x^2))")]
        [InlineData("1/(x^4*(1 + x^2))")]
        public void WhatTheRationalSplitAlreadyAnswers(string integrand)
            => DifferentiatesBack(integrand);

        /// <summary>
        /// A quadratic with a negative leading coefficient, which is the sine substitution rather
        /// than the tangent one.
        /// </summary>
        [Theory]
        [InlineData("1/(x^2*(1 - x^2)^2)")]
        [InlineData("1/(x^2*(4 - x^2)^2)")]
        public void TheOtherSubstitution(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// A <b>negative</b> power of the variable beside a quadratic with a linear term is
        /// outside this: completing the square turns <c>x^m</c> into <c>(y - h)^m</c>, which is a
        /// finite sum only for a non-negative whole <c>m</c>. Declined rather than guessed at.
        /// </summary>
        [Theory]
        [InlineData("1/(x*(2 + 2*x + x^2)^2)")]
        [InlineData("1/(x*sqrt(1 + x + x^2))")]
        public void ANegativePowerBesideAShiftedQuadratic(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("NaN", integral.Stringize());
            if (integral.Stringize().Contains("integral("))
                return;   // unanswered is a legitimate verdict; a wrong answer is not
            DifferentiatesBack(integrand);
        }

        /// <summary>
        /// The radicals, which reach this rule from the earlier call site and must be unaffected
        /// by the second one existing.
        /// </summary>
        [Theory]
        [InlineData("1/(1 + x^2)^(3/2)")]
        [InlineData("x^5/sqrt(5 + x^2)")]
        [InlineData("x^2/sqrt(1 + x^2)")]
        [InlineData("sqrt(1 - x^2)/x^2")]
        [InlineData("x/sqrt(1 + x + x^2)")]
        public void TheRadicalsAreUnaffected(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// The ordinary rational functions, which must be answered exactly as before — this is
        /// the risk the ordering exists to remove, so it is pinned by value rather than by shape.
        /// </summary>
        [Theory]
        [InlineData("1/(1 + x^2)")]
        [InlineData("x/(1 + x^2)")]
        [InlineData("1/(x^2 - 1)")]
        [InlineData("1/(x*(1 + x))")]
        [InlineData("x^3/(1 + x)")]
        [InlineData("1/(x^2 + 2*x + 2)")]
        public void TheOrdinaryRationalFunctionsAreUnchanged(string integrand)
            => DifferentiatesBack(integrand);
    }
}
