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
    /// Integrands holding a fractional power of something linear in the variable, which
    /// <c>u^q = a*x + b</c> turns into a rational function.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>x/sqrt(1 + x)</c> had no antiderivative, and the reason is worth keeping: the general
    /// substitution rewrites <em>sub-expressions</em>, so it found <c>1 + x</c>, replaced it, and
    /// was left holding a bare <c>x</c> it could not express in the new variable. This substitutes
    /// for <b>x itself</b>, so nothing is left behind.
    /// </para>
    /// <para>
    /// Checked by differentiating back and comparing at points, never against a printed form —
    /// these come out as polynomials in the radical and their shape says nothing about whether
    /// they differentiate back.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class LinearRadicalSubstitutionIntegralTest
    {
        /// <summary>
        /// Points where every radical below is real: each of the bases used here is positive on
        /// <c>(0, 0.6)</c>, which is what lets one set serve them all.
        /// </summary>
        private static readonly double[] Points = { 0.05, 0.17, 0.31, 0.44, 0.58 };

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
        /// A polynomial over a square root of something linear. None of these had an
        /// antiderivative, and each is a first-year exercise.
        /// </summary>
        [Theory]
        [InlineData("x/sqrt(1 + x)")]
        [InlineData("x/sqrt(2 - 3*x)")]
        [InlineData("x^2/sqrt(x + 1)")]
        [InlineData("(x + 2)/sqrt(x + 1)")]
        public void APolynomialOverASquareRootOfALinear(string integrand) => DifferentiatesBack(integrand);

        /// <summary>A root other than the square one, where <c>q</c> is the exponent's denominator.</summary>
        [Theory]
        [InlineData("x/(1 + x)^(1/3)")]
        [InlineData("1/(x + 1)^(1/3)")]
        public void ARootOtherThanTheSquare(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// Two radicals over one base, whose exponent denominators are taken together by their
        /// least common multiple, so a single substitution clears both.
        /// </summary>
        [Theory]
        [InlineData("1/(sqrt(x + 1) + (x + 1)^(1/3))")]
        public void TwoRootsOverOneBase(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// Declined, so the boundary is recorded rather than assumed.
        /// </summary>
        /// <remarks>
        /// A radical over a <em>quadratic</em> is a different substitution and is not this rule's;
        /// two radicals over <em>different</em> linear bases would need two substitutions at once,
        /// and half-rewriting is worse than declining. Should either later be answered by
        /// something else, these move rather than being deleted.
        /// </remarks>
        [Theory]
        [InlineData("sqrt(1 - x) + sqrt(1 + x)")]
        public void DifferentBasesAreDeclined(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("u_rad", integral.Stringize());
        }
    }
}
