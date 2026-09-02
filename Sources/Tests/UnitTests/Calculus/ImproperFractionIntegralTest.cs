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
    /// Quotients of polynomials whose numerator is of no lower degree than the denominator, which
    /// every step of the rational integrator wanted divided out first and none of them did.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>x^2/(x + 1)</c> is <c>x - 1 + 1/(x + 1)</c>, and each piece of that has been integrable
    /// throughout; the quotient itself had no antiderivative because the split at a rational root,
    /// the split at a coprime pair, and the split over the reals all require a <b>proper</b>
    /// fraction and decline otherwise.
    /// </para>
    /// <para>
    /// The division was not written for this — <c>TreeAnalyzer.PolynomialLongDivision</c> has done
    /// it all along for the simplifier's own <c>PolynomialLongDivision</c> rule set. The integrator
    /// never asked it.
    /// </para>
    /// <para>
    /// Checked by differentiating the answer back and comparing at points, never by comparing
    /// printed forms.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class ImproperFractionIntegralTest
    {
        private static void DifferentiatesBack(string integrand, params double[] points)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());

            var derivative = integral.Substitute("C", 0).Differentiate("x");
            var original = integrand.ToEntity();
            var compared = 0;
            foreach (var at in points)
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
                $"only {compared} of {points.Length} points were comparable for {integrand}");
        }

        /// <summary>The degree of the numerator above the denominator's, by one and by more.</summary>
        [Theory]
        [InlineData("x^2/(x + 1)", new[] { 0.3, 1.7, 5.0, -2.4 })]
        [InlineData("x^2/(1 + x)", new[] { 0.3, 1.7, 5.0, -2.4 })]
        [InlineData("x^3/(1 + x^2)", new[] { 0.3, 1.7, 5.0, -2.4 })]
        [InlineData("x^4/(x^2 + 1)", new[] { 0.3, 1.7, 5.0, -2.4 })]
        [InlineData("(x^5 + 2)/(x^2 + 1)", new[] { 0.3, 1.7, 5.0, -2.4 })]
        [InlineData("(x^2 + 3*x + 5)/(x + 2)", new[] { 0.3, 1.7, 5.0, -2.4 })]
        public void AnImproperQuotientIsDividedOutFirst(string integrand, double[] points)
            => DifferentiatesBack(integrand, points);

        /// <summary>
        /// A numerator the denominator divides exactly, where the proper part is zero and the
        /// answer is the polynomial alone.
        /// </summary>
        [Theory]
        [InlineData("(x^3 + 1)/(x + 1)", new[] { 0.3, 1.7, 5.0, -2.4 })]
        [InlineData("(x^2 - 1)/(x - 1)", new[] { 0.3, 1.7, 5.0, -2.4 })]
        public void ADenominatorThatDividesExactly(string integrand, double[] points)
            => DifferentiatesBack(integrand, points);

        /// <summary>
        /// What reaches this through a substitution rather than as written: the tangent
        /// substitution and the fractional power both produce improper fractions, and both were
        /// declined for it.
        /// </summary>
        [Theory]
        [InlineData("tan(x) ^ 2", new[] { 0.3, 0.9, 1.3 })]
        [InlineData("tan(x) ^ 3", new[] { 0.3, 0.9, 1.3 })]
        [InlineData("sqrt(x)/(x + 1)", new[] { 0.3, 1.7, 5.0 })]
        public void ReachedThroughASubstitution(string integrand, double[] points)
            => DifferentiatesBack(integrand, points);

        /// <summary>
        /// A proper fraction must be untouched by this: the division declines one, so the steps
        /// below it see exactly what they saw before and answer it the same way.
        /// </summary>
        [Theory]
        [InlineData("1/(x + 1)", "ln(x + 1) + C")]
        [InlineData("x/(x^2 + 1)", "1/2 * ln(x ^ 2 + 1) + C")]
        public void AProperFractionIsUnchanged(string integrand, string expected)
            => Assert.Equal(expected.ToEntity(), integrand.ToEntity().Integrate("x"));

        /// <summary>
        /// A denominator whose leading coefficient is symbolic is still declined. The division
        /// would have to divide by <c>b</c>, which is not decidably non-zero — and at <c>b = 0</c>
        /// the quotient is <c>x^2/a</c>, whose antiderivative is not the limit of the divided
        /// form. <see href="https://github.com/asc-community/AngouriMath/issues/180"/> item 18.
        /// </summary>
        [Theory]
        [InlineData("x^2/(a + b*x)")]
        [InlineData("x^2/(x + a)")]
        public void ASymbolicLeadingCoefficientIsDeclined(string integrand)
            => Assert.Contains("integral(", integrand.ToEntity().Integrate("x").Stringize());
    }
}
