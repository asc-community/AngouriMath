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
    /// An inverse trigonometric function times a polynomial is integrated by parts with the
    /// inverse function differentiated — the <b>I</b> of LIATE, which comes before <b>A</b>.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// The rule already made this choice for a logarithm and for nothing else, so
    /// <c>x * ln(x)</c> was answered and <c>x * atan(x)</c> was not. Its stated reason holds for
    /// both: differentiating turns the function into something algebraic that cancels against the
    /// integrated polynomial and ends, where integrating it puts the original integral back in
    /// front of us.
    /// </para>
    /// <para>
    /// <b>Where it stops, and why.</b> <c>atan</c> and <c>acot</c> differentiate to rational
    /// functions, so what is left is rational and answered. <c>asin</c> and <c>acos</c>
    /// differentiate to <c>1/sqrt(1 - x^2)</c>, so what is left is a radical integrand and is a
    /// different gap — the choice of which factor to differentiate is right for those too, and it
    /// is the remainder that is not read.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class InverseTrigByPartsTest
    {
        /// <summary>Inside the unit interval, where every inverse function here is real.</summary>
        private static readonly double[] Points = { 0.13, 0.37, 0.61, 0.82 };

        private static void DifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var written = integral.Stringize();
            Assert.DoesNotContain("integral(", written);
            Assert.DoesNotContain("NaN", written);

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
            Assert.True(compared >= 3,
                $"only {compared} of {Points.Length} points were comparable for {integrand}");
        }

        /// <summary>
        /// The arctangent and arccotangent, whose derivatives are rational, so the remaining
        /// integral is one the rational rules read. None of these had an antiderivative.
        /// </summary>
        [Theory]
        [InlineData("x * atan(x)")]
        [InlineData("x ^ 3 * atan(x)")]
        [InlineData("x * acot(x)")]
        public void AnArctangentTimesAPolynomial(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// The logarithm, which made this choice already and must go on making it, and the bare
        /// inverse functions, which are answered by their own rules rather than by this one.
        /// </summary>
        [Theory]
        [InlineData("x * ln(x)")]
        [InlineData("x ^ 2 * ln(x)")]
        [InlineData("ln(x)")]
        [InlineData("atan(x)")]
        [InlineData("asin(x)")]
        [InlineData("acos(x)")]
        public void WhatWasAlreadyAnswered(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// Unrelated products by parts, which must be unaffected: the polynomial is the factor to
        /// differentiate in each of these, and it still is.
        /// </summary>
        [Theory]
        [InlineData("x * e ^ x")]
        [InlineData("x * sin(x)")]
        [InlineData("e ^ x * sin(x)")]
        public void OrdinaryProductsAreUnaffected(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// Still declined, and recorded rather than assumed. <c>asin</c> and <c>acos</c>
        /// differentiate to <c>1/sqrt(1 - x^2)</c>, so what is left over is a radical integrand
        /// this library does not read — a different gap from which factor to differentiate.
        /// Should something later answer them, these move rather than being deleted.
        /// </summary>
        [Theory]
        [InlineData("x * asin(x)")]
        [InlineData("x ^ 2 * acos(x)")]
        public void AnArcsineLeavesARadicalAndIsDeclined(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            // What must never happen is a wrong answer, so where one does appear it is checked.
            if (!integral.Stringize().Contains("integral("))
                DifferentiatesBack(integrand);
        }
    }
}
