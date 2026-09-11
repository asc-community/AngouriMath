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
        /// A denominator whose <b>leading coefficient</b> is symbolic is divided out too, for the
        /// integrator — and this test recorded the opposite until it was.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The verdict here was that dividing by <c>b</c> loses <c>b = 0</c>, where the quotient
        /// is <c>x^2/a</c> and the divided answer is not its limit, so no answer was the honest
        /// one. The first half is true. The second was measured against the wrong neighbours:
        /// <c>int 1/(a x + b) dx</c> is <c>ln(a x + b)/a</c>, <c>int sin(a x) dx</c> is
        /// <c>-cos(a x)/a</c>, <c>int x^n dx</c> is <c>x^(n+1)/(n+1)</c> — each undefined at one
        /// value of its parameter and each given by this integrator without a condition. Long
        /// division was the one rule holding out, and the first of those divides by the very
        /// coefficient it refused. It now asks the division for the generic case; the simplifier,
        /// whose rewrite has to be an equivalence, gets the old answer. The cases live in
        /// <see cref="SymbolicParameterIntegralTest"/>.
        /// </para>
        /// <para>
        /// Item 18 of <see href="https://github.com/asc-community/AngouriMath/issues/180"/> is
        /// this integral. What it asked for is answered; the piecewise with the degenerate branch
        /// beside it is a further step no neighbouring rule takes either.
        /// </para>
        /// </remarks>
        [Theory]
        [InlineData("x^2/(a + b*x)")]
        [InlineData("x^2/(2 + b*x)")]
        [InlineData("x^3/(a + b*x)")]
        public void ASymbolicLeadingCoefficientIsDividedOut(string integrand)
            => Assert.DoesNotContain("integral(", integrand.ToEntity().Integrate("x").Stringize());

        /// <summary>
        /// A symbolic coefficient that is <b>not</b> the leading one carries none of that
        /// argument, and these are answered.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>x^2/(x + a)</c> was on the list above, and it did not belong there: its leading
        /// coefficient is <c>1</c>, nothing is divided by a symbol, and the quotient
        /// <c>x - a + a^2/(x + a)</c> holds for every <c>a</c>. It was declined for an unrelated
        /// reason — the division replaced each variable by its smallest enclosing subtree, which
        /// for <c>a</c> is the whole denominator, so the divisor became one opaque symbol and no
        /// longer shared a variable with the dividend.
        /// </para>
        /// <para>
        /// Recorded here rather than deleted, because the reason written against the old verdict
        /// was true of one of its two cases and not of the other, and the difference is exactly
        /// what decides whether a quotient like this may be given.
        /// </para>
        /// </remarks>
        [Theory]
        [InlineData("x^2/(x + a)", new[] { 0.3, 1.7, 5.0 })]
        [InlineData("x^2/(a + x)", new[] { 0.3, 1.7, 5.0 })]
        [InlineData("x^3/(x + a)", new[] { 0.3, 1.7, 5.0 })]
        [InlineData("x^4/(x + a)", new[] { 0.3, 1.7, 5.0 })]
        [InlineData("(x^2 + 1)/(x + a)", new[] { 0.3, 1.7, 5.0 })]
        [InlineData("x^3/(x^2 + a)", new[] { 0.3, 1.7, 5.0 })]
        [InlineData("x*ln(a + x)", new[] { 0.3, 1.7, 5.0 })]
        [InlineData("x^2*ln(a + x)", new[] { 0.3, 1.7, 5.0 })]
        public void ASymbolicCoefficientThatIsNotTheLeadingOne(string integrand, double[] points)
        {
            var pinned = integrand.ToEntity().Substitute("a", 2);
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Assert.DoesNotContain("NaN", integral.Stringize());

            var derivative = integral.Substitute("C", 0).Differentiate("x").Substitute("a", 2);
            var compared = 0;
            foreach (var at in points)
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = pinned.Substitute("x", at).EvalNumerical();
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
            Assert.True(compared >= 3,
                $"only {compared} of {points.Length} points were comparable for {integrand}");
        }

        /// <summary>
        /// What the division has always been able to do and must keep doing: a base that is not a
        /// variable, which only the variable replacement reaches.
        /// </summary>
        [Theory]
        [InlineData("sin(x)^2/sin(x)", new[] { 0.3, 0.9, 1.3 })]
        [InlineData("x^(6/10)/x^(3/10)", new[] { 0.3, 1.7, 5.0 })]
        public void ABaseThatIsNotAVariable(string integrand, double[] points)
            => DifferentiatesBack(integrand, points);
    }
}
