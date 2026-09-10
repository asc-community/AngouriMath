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
    /// A power whose base holds the variable, written out and integrated term by term.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>(2x + 3x^2)^3</c> had no antiderivative, and it is a polynomial. The rule for a power
    /// asks that the base <em>be</em> the variable, so a base that merely contains it matched
    /// nothing, and nothing else in the chain writes a power out. <c>(1 + x)^3</c> was answered
    /// only because the linear substitution happens to read it.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class ExpandedPowerIntegralTest
    {
        private static readonly double[] Points = { -0.5, 0.23, 0.61, 1.05, 2.3 };

        private static void DifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());

            var derivative = integral.Substitute("C", 0).Differentiate("x").Substitute("a", 1.7);
            var original = integrand.ToEntity().Substitute("a", 1.7);
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
        /// A base that is not a bare variable and not linear either, so nothing upstream reads
        /// it. Each of these is a polynomial and each was left unevaluated.
        /// </summary>
        [Theory]
        [InlineData("(2*x + 3*x^2)^3")]
        [InlineData("(2*x + 3*x^2)^2")]
        [InlineData("(1 - 2*x - 2*x^2)^3")]
        [InlineData("(x^2 + x)^4")]
        [InlineData("(1 + x^3)^2")]
        public void APolynomialBaseRaisedToAWholePower(string integrand)
            => DifferentiatesBack(integrand);

        /// <summary>
        /// What was already answered, and by what. These reach the power rule or the linear
        /// substitution first, and the expansion runs after both — so they are answered by the
        /// same rules as before rather than written out.
        /// </summary>
        [Theory]
        [InlineData("(1 + x)^3")]
        [InlineData("(x + 1)^10")]
        [InlineData("(1 + x^2)^2")]
        [InlineData("sin(x)^2")]
        public void WhatWasAlreadyAnswered(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// Exponents this must not touch. A negative one is a quotient and belongs to partial
        /// fractions; a fractional one is a radical and does not expand at all. Both are
        /// answered, and by something else.
        /// </summary>
        [Theory]
        [InlineData("(1 + x^2)^(-1)")]
        [InlineData("(1 + x^3)^(-1)")]
        [InlineData("(1 + x^2)^(1/2)")]
        [InlineData("sqrt(1 + x)")]
        public void ANegativeOrFractionalExponentIsNotExpanded(string integrand)
            => DifferentiatesBack(integrand);

        /// <summary>
        /// Still declined, so the boundary is recorded. The power is the numerator of a quotient
        /// rather than the whole integrand, and this rule reads only the whole. Should something
        /// later answer it, this moves rather than being deleted.
        /// </summary>
        [Theory]
        [InlineData("(a - b*x^2)^3/x^7")]
        public void APowerInsideAQuotientIsNotReached(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            if (!integral.Stringize().Contains("integral("))
                DifferentiatesBack(integrand);
        }
    }
}
