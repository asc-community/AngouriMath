//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Diagnostics;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// A whole power of a logarithm times a polynomial, which needs integration by parts twice.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>x * ln(x)</c> was answered and <c>x * ln(x)^2</c> was not, and it took two changes
    /// together — either alone leaves it declined.
    /// </para>
    /// <para>
    /// The rule choosing which factor to differentiate named the logarithm node and not a power
    /// of one, so <c>ln(x)^2</c> was not recognised as the factor to differentiate. And the
    /// remaining integral after one step was computed with parts switched off, so even once it
    /// was recognised, the <c>x * ln(x)</c> that one step leaves behind could not be answered —
    /// it is answered by parts and by nothing else.
    /// </para>
    /// <para>
    /// <b>The second change is why the timing test below is here.</b> Parts was switched off for
    /// the remainder because the recursion used to run away. It is allowed again only where the
    /// remainder is strictly smaller than the integrand the call started from, which is a
    /// decrease that makes the descent finite; without that measure
    /// <c>sin(x)/(x^2 + 1)^2</c> — which has no elementary antiderivative and so runs every
    /// solver to exhaustion — went from about a second to 83.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class RepeatedByPartsTest
    {
        private static readonly double[] Points = { 0.13, 0.37, 0.61, 0.82, 1.4 };

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

        /// <summary>A squared logarithm times a polynomial, which one step of parts reduces.</summary>
        [Theory]
        [InlineData("x * ln(x) ^ 2")]
        public void ASquaredLogarithmTimesAPolynomial(string integrand)
            => DifferentiatesBack(integrand);

        /// <summary>
        /// What was already answered by one step and must go on being answered — including the
        /// bare logarithm case, whose choice of factor this reuses.
        /// </summary>
        [Theory]
        [InlineData("x * ln(x)")]
        [InlineData("x ^ 2 * ln(x)")]
        [InlineData("ln(x)")]
        [InlineData("ln(x) ^ 2")]
        [InlineData("x * atan(x)")]
        [InlineData("atan(x)")]
        [InlineData("x * e ^ x")]
        [InlineData("x * sin(x)")]
        [InlineData("e ^ x * sin(x)")]
        public void WhatWasAlreadyAnswered(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// The measure that bounds the recursion, asserted where it is load-bearing.
        /// <c>sin(x)/(x^2 + 1)^2</c> has no elementary antiderivative, so every solver runs to
        /// exhaustion on it and it pays the full cost of the search.
        /// </summary>
        /// <remarks>
        /// The bound is a wall that cannot fire on a slow runner — the work is about a second —
        /// and it is a guard against the growth coming back rather than a benchmark.
        /// <c>IntegralAnswerCacheTest</c> asserts the same integrand for a different reason; this
        /// one is about the recursion rather than the answer cache, and both should keep failing
        /// independently if either regresses.
        /// </remarks>
        [Fact]
        public void AnUnanswerableIntegralDoesNotPayForTheRecursion()
        {
            var watch = Stopwatch.StartNew();
            var answer = "sin(x) / (x ^ 2 + 1) ^ 2".ToEntity().Integrate("x");
            watch.Stop();

            Assert.Contains("integral(", answer.Stringize());
            Assert.True(watch.Elapsed < TimeSpan.FromSeconds(30),
                $"took {watch.Elapsed.TotalSeconds:F1}s; without the decrease measure on the "
                + "remaining integrand it was 83s");
        }
    }
}
