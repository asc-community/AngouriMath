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
    /// Integrals that need a substitution by a <em>power of the variable</em> which does not occur
    /// anywhere in the integrand.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>int x / (x^4 + 1)</c> wants <c>u = x^2</c>, and <c>x^2</c> is written nowhere in it. Two
    /// things followed from that. The candidate was never offered, because candidates were taken
    /// from the subexpressions that occur; and had it been, substituting it would have replaced
    /// nothing, because <c>x^4</c> is not written as <c>(x^2)^2</c> and a substitution matches
    /// what is written. So the integrand kept its <c>x</c> and the candidate was rejected.
    /// </para>
    /// <para>
    /// <c>int x^3 / (x^4 + 1)</c> worked throughout, which is what made this hard to see: its
    /// substitution, <c>u = x^4</c>, does occur.
    /// </para>
    /// <para>
    /// Every case here is checked by differentiating the answer and comparing it with the
    /// integrand at sampled points, not by comparing printed forms. An antiderivative is only
    /// correct if it differentiates back, and these come out with radicals and arctangents whose
    /// printed shape says nothing about whether they do.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class PowerSubstitutionIntegralTest
    {
        /// <summary>
        /// The sample points, chosen away from the poles of the integrands below and on both
        /// sides of zero, since a substitution by an even power is where a sign is most easily
        /// lost.
        /// </summary>
        private static readonly double[] Points = { 0.3, 0.7, 1.3, 1.9, 2.6, 3.4, -0.4, -1.7, -2.9 };

        private static void DifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());

            var derivative = integral.Differentiate("x");
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
            Assert.True(compared >= 5,
                $"only {compared} of {Points.Length} points were comparable for {integrand}, "
                + "so this asserts almost nothing");
        }

        /// <summary>
        /// A <em>fractional</em> power of the variable as the substitution, which is the same
        /// rewrite read the other way: <c>u = x^r</c> means <c>x^n = u^(n/r)</c>, and where a
        /// whole <c>r</c> can only rewrite the powers it divides, an <c>r</c> of <c>1/2</c>
        /// rewrites every one of them — the bare <c>x</c> included, which a whole <c>r</c> never
        /// can.
        /// </summary>
        /// <remarks>
        /// <c>int sqrt(x)/(1 + x^2)</c> becomes <c>int 2u^2/(1 + u^4) du</c> under
        /// <c>u = sqrt(x)</c>, which is answered — so this reaches what it does partly because
        /// the denominator now factors over the reals.
        /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
        /// </remarks>
        [Theory]
        [InlineData("sqrt(x)/(1 + x^2)")]
        [InlineData("1/(1 + sqrt(x))")]
        [InlineData("1/(sqrt(x) * (1 + x))")]
        [InlineData("1/(sqrt(x) * (1 + x^2))")]
        [InlineData("1/(sqrt(x) + x)")]
        public void AFractionalPowerIsAlsoASubstitution(string integrand)
            => DifferentiatesBack(integrand);

        /// <summary>
        /// Where a fractional substitution reaches and the rest of the chain does not, recorded
        /// so the boundary is visible. <c>sqrt(x)/(1 + x^4)</c> becomes <c>2u^2/(1 + u^8)</c>,
        /// whose denominator is neither factorable over the rationals nor a biquadratic; and
        /// <c>sqrt(x)/(x + 1)</c> becomes <c>2u^2/(1 + u^2)</c>, which is improper, and dividing
        /// an improper fraction out is not something the rational integrator does.
        /// </summary>
        [Theory]
        [InlineData("sqrt(x)/(1 + x^4)")]
        [InlineData("sqrt(x)/(x + 1)")]
        public void WhereTheChainStops(string integrand)
            => Assert.Contains("integral(", integrand.ToEntity().Integrate("x").Stringize());

        /// <summary>
        /// The shape the issue is about: an odd power over an even one, where the substitution is
        /// the root of the denominator's power.
        /// </summary>
        [Theory]
        [InlineData("x/(x^4 + 1)")]
        [InlineData("x/(x^4 - 1)")]
        [InlineData("x/(x^4 + 4)")]
        [InlineData("x^2/(x^6 + 1)")]
        [InlineData("x/(x^6 + 1)")]
        [InlineData("x^3/(x^8 + 1)")]
        [InlineData("x^3/(x^12 + 1)")]
        public void APowerOfTheVariableThatOccursNowhereIsStillASubstitution(string integrand)
            => DifferentiatesBack(integrand);

        /// <summary>
        /// <c>int x / (x^4 + 1)</c> is <c>arctan(x^2)/2</c>, which is worth asserting as a form
        /// and not only as a derivative: it is the answer the issue's link gives, and getting a
        /// constant factor wrong would still differentiate back to something proportional.
        /// </summary>
        [Fact]
        public void TheAnswerIsTheOneTheIssueNames()
        {
            var integral = "x/(x^4 + 1)".ToEntity().Integrate("x").Simplify();
            Assert.Equal("arctan(x ^ 2) / 2 + C".ToEntity().Simplify(), integral);
        }

        /// <summary>
        /// Nothing that integrated before integrates differently. These take the other paths —
        /// a substitution that does occur, partial fractions, by parts, a logarithm — so a
        /// change in the candidate list would show here.
        /// </summary>
        [Theory]
        [InlineData("x^3/(x^4 + 1)")]
        [InlineData("cos(x^2) * x")]
        [InlineData("sin(x) * e^x")]
        [InlineData("1/(x^2 + 1)")]
        [InlineData("1/(x^4 - 1)")]
        [InlineData("1/(x^2 - 1)")]
        [InlineData("x * ln(x)")]
        [InlineData("ln(x)")]
        [InlineData("x^2")]
        [InlineData("sin(x) * cos(x)")]
        [InlineData("e^x * x")]
        public void WhatIntegratedBeforeStillDoes(string integrand)
            => DifferentiatesBack(integrand);

        /// <summary>
        /// And the rewrite does not make a candidate succeed that should not: under
        /// <c>u = x^2</c> each of these leaves a bare <c>x</c> behind, so the substitution
        /// rejects them.
        /// </summary>
        /// <remarks>
        /// The witness used to be <c>x^2/(x^4 + 1)</c>, which is now answered — not by this
        /// substitution, which still rejects it for the same reason, but by the partial fraction
        /// step learning to factor a biquadratic denominator over the reals. A test that the
        /// substitution declines something needs an integrand nothing else answers either, or it
        /// stops testing the substitution the moment a neighbouring capability arrives. These
        /// carry an odd power, which puts them out of reach of that step as well.
        /// <see href="https://github.com/asc-community/AngouriMath/issues/233"/>.
        /// </remarks>
        [Theory]
        [InlineData("x^2/(x^4 + x + 1)")]
        [InlineData("x^2/(x^4 + x^3 + 1)")]
        public void AnIntegralThisDoesNotReachIsStillDeclined(string integrand)
            => Assert.Contains("integral(", integrand.ToEntity().Integrate("x").Stringize());
    }
}
