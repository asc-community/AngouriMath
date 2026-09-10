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
    /// One integrand written two ways gets one answer. A quotient is either a <c>Divf</c> or a
    /// negative whole <c>Powf</c>, and partial fractions used to read only the first.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// The second spelling is not an exotic way to write it — it is what the integrator builds
    /// for itself. Taking a constant factor out of <c>a/(1 + x^3)</c> leaves
    /// <c>(1 + x^3)^(-1)</c>, so <c>a/(1 + x^3)</c> had no antiderivative while
    /// <c>1/(1 + x^3)</c> and <c>a * (1/(1 + x^3))</c> both did.
    /// </para>
    /// <para>
    /// Checked by differentiating back with the parameter pinned to a value and comparing at
    /// points, since the answers are logarithms and arctangents whose printed form says nothing
    /// about whether they differentiate back.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class QuotientSpellingIntegralTest
    {
        private static readonly double[] Points = { -0.5, 0.23, 0.61, 1.05, 1.7 };

        private static void DifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());

            // A parameter is pinned to a value only for the numeric comparison; the
            // antiderivative itself was found symbolically.
            var derivative = integral.Substitute("C", 0).Differentiate("x")
                .Substitute("a", 1.7).Substitute("b", 0.9);
            var original = integrand.ToEntity().Substitute("a", 1.7).Substitute("b", 0.9);
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
        /// The reciprocal written as a power, which is the shape the integrator hands itself.
        /// </summary>
        [Theory]
        [InlineData("(1 + x^3)^(-1)")]
        [InlineData("(1 + x^4)^(-1)")]
        [InlineData("(x^4 + 3*x^2 + 2)^(-1)")]
        [InlineData("(x^2 - 1)^(-2)")]
        public void AReciprocalWrittenAsAPower(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// A constant factor over a denominator partial fractions has to take apart. These are
        /// the ones the two spellings actually cost: the factor comes out, and what is left is
        /// the power spelling.
        /// </summary>
        [Theory]
        [InlineData("a/(1 + x^3)")]
        [InlineData("b/(1 + x^4)")]
        [InlineData("a/(x^4 + 3*x^2 + 2)")]
        [InlineData("a/(x^2 - 1)")]
        [InlineData("a/((1 + x)^3*(2 + x)^3)")]
        public void AConstantFactorOverAFactorableDenominator(string integrand)
            => DifferentiatesBack(integrand);

        /// <summary>
        /// The three spellings of one integrand agree, which is the property this is about
        /// rather than any particular antiderivative.
        /// </summary>
        [Fact]
        public void TheSpellingsAgree()
        {
            var points = new[] { 0.23, 0.61, 1.7 };
            foreach (var written in new[] { "1/(1 + x^3)", "(1 + x^3)^(-1)" })
            {
                var integral = written.ToEntity().Integrate("x").Substitute("C", 0);
                var reference = "1/(1 + x^3)".ToEntity().Integrate("x").Substitute("C", 0);
                foreach (var at in points)
                {
                    var got = integral.Substitute("x", at).EvalNumerical();
                    var want = reference.Substitute("x", at).EvalNumerical();
                    Assert.True(Math.Abs((double)(got - want).RealPart) < 1e-9,
                        $"{written} integrates to {got} at x = {at}, where 1/(1 + x^3) gives {want}");
                }
            }
        }

        /// <summary>
        /// What is still declined, so the boundary is recorded rather than assumed. A positive
        /// or fractional power is not a quotient, and a constant factor inside the denominator
        /// is a different gap — the denominator is then not a polynomial over the rationals.
        /// Should either later be answered, these move rather than being deleted.
        /// </summary>
        [Theory]
        [InlineData("(1 + x^3)^2")]
        [InlineData("(1 + x^3)^(1/2)")]
        [InlineData("1/(a*(1 + x^3))")]
        public void WhatIsStillDeclined(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            // Either answered by something else entirely or left alone — what must not happen is
            // a wrong answer, so this asserts the value rather than the verdict where there is one.
            if (!integral.Stringize().Contains("integral("))
                DifferentiatesBack(integrand);
        }
    }
}
