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
    /// A denominator that is a symbolic multiple of <c>x^2</c> has an antiderivative, and the
    /// integrator used to answer <c>NaN</c> — which says the opposite.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>1/(a*x^2)</c> came back as <c>NaN + C</c>, where it is <c>-1/(a x)</c>. The rule for a
    /// constant over a quadratic returns a piecewise over the discriminant with a branch for
    /// <c>a = 0</c>, and with <c>b</c> and <c>c</c> both zero that branch computes <c>k/0</c>. A
    /// symbolic <c>a</c> makes it undecidable, so the branch stays, and one <c>NaN</c> case takes
    /// the whole piecewise with it.
    /// </para>
    /// <para>
    /// <b>Unevaluated and <c>NaN</c> are different answers.</b> The first says the library could
    /// not settle it; the second says the antiderivative does not exist. These exist, so
    /// <c>NaN</c> was a wrong answer rather than a missing one — which is why this is asserted
    /// as a value and not merely as "not <c>NaN</c>".
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class QuadraticDenominatorNaNTest
    {
        private static void DifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var written = integral.Stringize();
            Assert.DoesNotContain("integral(", written);
            Assert.DoesNotContain("NaN", written);

            var derivative = integral.Substitute("C", 0).Differentiate("x")
                .Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 2.1);
            var original = integrand.ToEntity()
                .Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 2.1);
            var compared = 0;
            foreach (var at in new[] { 0.4, 0.9, 1.6, -0.7 })
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
                $"only {compared} points were comparable for {integrand}");
        }

        /// <summary>
        /// The denominator is a symbolic constant times <c>x^2</c>, however that constant is
        /// written — one symbol, a sum of them, or the sum the terms collect into.
        /// </summary>
        [Theory]
        [InlineData("1/(a*x^2)")]
        [InlineData("1/((a + b)*x^2)")]
        [InlineData("1/(a*x^2 + b*x^2)")]
        [InlineData("1/(a*x^2 + b*x^2 + c*x^2)")]
        public void ASymbolicMultipleOfXSquared(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// The neighbours, which were right and have to stay right. A numeric coefficient never
        /// reached the bad branch — the <c>a = 0</c> case is decidably false and dropped — and a
        /// denominator with a linear or constant term takes a different arm of the piecewise.
        /// </summary>
        [Theory]
        [InlineData("1/x^2")]
        [InlineData("1/(2*x^2)")]
        [InlineData("1/(a*x^2 + b*x)")]
        [InlineData("1/(a*x^2 + b)")]
        [InlineData("1/(a*x + b*x)")]
        [InlineData("1/(x^2 + 1)")]
        [InlineData("1/(x^2 - 1)")]
        [InlineData("1/(2*x^2 + 3*x + 1)")]
        [InlineData("1/(a*x^2 + b*x + c)")]
        public void TheNeighbouringShapes(string integrand) => DifferentiatesBack(integrand);
    }
}
