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
    /// Integrands that are a function of <c>tan(x)</c> and of nothing else, which the
    /// substitution <c>u = tan(x)</c> turns into a rational function — <c>dx</c> being
    /// <c>du/(1 + u^2)</c>.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>int sqrt(tan(x))</c> is the last of the five integrals that issue lists, and the one it
    /// calls "very painful, requires different solvers", which it is: the substitution here makes
    /// it <c>int sqrt(u)/(1 + u^2) du</c>, a fractional power substitution makes that
    /// <c>int 2t^2/(1 + t^4) dt</c>, and answering <em>that</em> needs the denominator factored
    /// over the reals. Three capabilities in a row, and it comes out unevaluated if any of them
    /// is missing.
    /// </para>
    /// <para>
    /// Checked by differentiating the answer back and comparing at points, never by comparing
    /// printed forms — the answers here are sums of logarithms and arctangents of radicals in
    /// <c>tan(x)</c>, and their shape says nothing about whether they differentiate back.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class TangentSubstitutionIntegralTest
    {
        /// <summary>
        /// Sample points inside a single branch of the tangent, well away from the pole at
        /// <c>pi/2</c>. <c>sqrt(tan(x))</c> is real only where the tangent is non-negative, so
        /// they stay on <c>(0, pi/2)</c> rather than straddling zero.
        /// </summary>
        private static readonly double[] Points = { 0.2, 0.45, 0.8, 1.1, 1.35 };

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

        /// <summary>The integral the issue names, and the reason this substitution exists.</summary>
        [Theory]
        [InlineData("sqrt(tan(x))")]
        public void TheIntegralTheIssueNames(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// A function of the tangent alone, whatever it is made of. Each of these is answered
        /// through the rewrite rather than by a rule of its own.
        /// </summary>
        [Theory]
        [InlineData("1/sqrt(tan(x))")]
        [InlineData("tan(x)/(1 + tan(x)^2)")]
        public void AFunctionOfTheTangentAlone(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// A power of the tangent, which the rewrite turns into an <b>improper</b> rational
        /// function — <c>u^2/(1 + u^2)</c> and <c>u^3/(1 + u^2)</c>. These were declined while
        /// nothing divided an improper fraction out; they are answered now that the rational
        /// integrator does that first.
        /// </summary>
        [Theory]
        [InlineData("tan(x) ^ 2")]
        [InlineData("tan(x) ^ 3")]
        public void APowerOfTheTangent(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// What the neighbours still answer, and must go on answering the same way: the tangent
        /// itself has a rule, which is reached before this and gives the shorter form.
        /// </summary>
        [Theory]
        [InlineData("tan(x)", "-ln(cos(x)) + C")]
        [InlineData("tan(2 * x)", "-ln(cos(2 * x)) / 2 + C")]
        public void TheRuleForTheTangentItselfStillWins(string integrand, string expected)
            => Assert.Equal(expected.ToEntity(), integrand.ToEntity().Integrate("x"));

        /// <summary>
        /// An integrand that mentions <c>x</c> outside the tangent is not a function of the
        /// tangent alone, and the rewrite leaves that <c>x</c> behind rather than pretending
        /// otherwise. <c>tan(x) + x</c> is answered, but by linearity over the sum, not by this.
        /// </summary>
        [Theory]
        [InlineData("tan(x) + x", "-ln(cos(x)) + x ^ 2 / 2 + C")]
        public void AnXOutsideTheTangentIsNotRewritten(string integrand, string expected)
            => Assert.Equal(expected.ToEntity(), integrand.ToEntity().Integrate("x"));

        /// <summary>
        /// What was declined, recorded so the boundary is visible rather than inferred from
        /// an absence, and moved here as it is answered. Each was declined by what the rewrite
        /// hands on, not by the rewrite itself:
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item><c>sqrt(cotan(x))</c> — the cotangent is written as <c>1/tan</c> on the way in,
        /// so this does start; what stopped it was the fractional power, which leaves
        /// <c>sqrt(1/u)/(1 + u^2)</c>, a radical of a reciprocal. The substitution rule offers
        /// <c>1/u</c> where it sits under a root now, and that is <c>sqrt(v)/(1 + v^2)</c> over
        /// <c>v^2</c>, the same shape as <c>sqrt(tan(x))</c> itself.</item>
        /// </list>
        /// <c>tan(x)^2</c> and <c>tan(x)^3</c> were on this list, for becoming improper rational
        /// functions that nothing divided out; they are answered above now that the rational
        /// integrator divides first. <c>1/(1 + tan(x)^2)</c> was on it too, for becoming
        /// <c>1/(1 + u^2)^2</c> — and it is answered now that the substituted integrand is
        /// combined into a single quotient rather than merely inner-simplified. That is twice
        /// this list has moved, which is why it is worth keeping as tests rather than as prose:
        /// it fails when the boundary moves.
        /// </remarks>
        [Theory]
        [InlineData("sqrt(cotan(x))")]
        public void WhatWasDeclinedAndIsAnswered(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// <c>1/(1 + tan(x)^2)</c> is <c>cos(x)^2</c>, and it used to be declined for becoming
        /// <c>1/(1 + u^2)^2</c> under the substitution. Asserted as a value, since what it comes
        /// out as says nothing about whether it is an antiderivative.
        /// </summary>
        [Fact]
        public void ARepeatedQuadraticFromTheSubstitutionIsAnswered()
        {
            var integrand = "1/(1 + tan(x)^2)".ToEntity();
            var integral = integrand.Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());

            var derivative = integral.Substitute("C", 0).Differentiate("x");
            foreach (var at in new[] { 0.35, 0.9, 2.4 })
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = integrand.Substitute("x", at).EvalNumerical();
                Assert.True(System.Math.Abs((double)(got - want).RealPart) < 1e-9,
                    $"d/dx of the antiderivative is {got} at x = {at}, where the integrand is {want}");
            }
        }
    }
}
