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
    /// The binomial differential <c>x^m (a + b x^n)^(p/q)</c>, in the two of Chebyshev's three
    /// cases that are not a whole power: where the substitution leaves a polynomial or a
    /// rational function of <c>u</c>, and the third case taken to those through <c>x = 1/y</c>.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>x^2 sqrt(1 + x^3)</c> came out and <c>x^5 sqrt(1 + x^3)</c> did not, under the same
    /// substitution. <c>u = (a + b x^n)^(1/q)</c> leaves a monomial in <c>u</c> for the first —
    /// which the general substitution finds, because <c>x^(n-1)</c> is <c>du</c> up to a constant
    /// — and a polynomial for the second, which it does not.
    /// </para>
    /// <para>
    /// The condition is <c>s = (m + 1)/n</c> being a whole number of at least one. Checked by
    /// differentiating back and comparing at points.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class BinomialDifferentialTest
    {
        private static readonly double[] Points = { 0.21, 0.35, 0.52, 0.71, 0.83 };

        private static void DifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("NaN", integral.Stringize());
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
                var difference = Math.Abs((double)(got - want).RealPart)
                               + Math.Abs((double)(got - want).ImaginaryPart);
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
        /// <c>s = 2</c> and above, where the substitution leaves a polynomial rather than a
        /// monomial. These are the ones that were declined.
        /// </summary>
        [Theory]
        [InlineData("x^5*sqrt(1 + x^3)")]
        [InlineData("x^5*(1 + x^3)^(2/3)")]
        [InlineData("x^5/(1 + x^3)^(1/3)")]
        [InlineData("x^3*(1 + x^2)^(1/3)")]
        [InlineData("x^7*(1 + x^4)^(1/2)")]
        [InlineData("x^8*sqrt(1 + x^3)")]
        public void WhereTheSubstitutionLeavesAPolynomial(string integrand)
            => DifferentiatesBack(integrand);

        /// <summary>
        /// <c>s = 1</c>, the monomial case, which the general substitution already answered. It
        /// reaches this rule first now, so the answers have to be right from here too.
        /// </summary>
        [Theory]
        [InlineData("x^2*sqrt(1 + x^3)")]
        [InlineData("x^2*(1 + x^3)^(1/3)")]
        [InlineData("x^2/(1 + x^3)^(1/3)")]
        [InlineData("x*sqrt(1 + x^2)")]
        [InlineData("x*(1 + x^2)^(1/3)")]
        [InlineData("x^3/sqrt(1 + x^4)")]
        [InlineData("x^2*(4 - x^3)^(1/3)")]
        public void TheMonomialCaseIsUnchangedInSubstance(string integrand)
            => DifferentiatesBack(integrand);

        /// <summary>
        /// A rational factor in front and a negative coefficient inside, both of which the read
        /// carries rather than declining over.
        /// </summary>
        [Theory]
        [InlineData("3*x^5*sqrt(1 + x^3)")]
        [InlineData("x^5*sqrt(2 - 3*x^3)/2")]
        [InlineData("x^5*(1 + 2*x^3)^(1/3)")]
        public void AFactorAndACoefficient(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// <c>s = (m + 1)/n</c> whole and at most zero, where the substitution leaves a rational
        /// function of <c>u</c> rather than a polynomial: <c>sqrt(1 + x^3)/x</c> is
        /// <c>(2/3) int u^2/(u^2 - 1) du</c>. Charlwood's, Bronstein's and four of Welz's.
        /// </summary>
        [Theory]
        [InlineData("sqrt(1 + x^3)/x")]
        [InlineData("1/(x*sqrt(1 - x^3))")]
        [InlineData("sqrt(1 + x^4)/(x*(1 + x^4))")]
        [InlineData("(1 - x^3)^(1/3)/x")]
        [InlineData("(1 - x^3)^(2/3)/x")]
        [InlineData("1/(x*(1 - x^2)^(1/3))")]
        [InlineData("1/(x*(1 + x^3)^(1/3))")]
        [InlineData("sqrt(1 + x^2)/x^3")]
        public void WhereTheSubstitutionLeavesARationalFunction(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// Chebyshev's third case, <c>s + p/q</c> whole, taken to the second by <c>x = 1/y</c>:
        /// <c>x^6 (3 + 4x^4)^(1/4)</c> and <c>(x^3 - 1)/(2 + x^3)^(1/3)</c> are Timofeev's, and the
        /// second is a sum whose terms are each this shape.
        /// </summary>
        [Theory]
        [InlineData("x^6*(3 + 4*x^4)^(1/4)")]
        [InlineData("x^3/(2 + x^3)^(1/3)")]
        [InlineData("1/(2 + x^3)^(1/3)")]
        [InlineData("(x^3 - 1)/(2 + x^3)^(1/3)")]
        [InlineData("x^2*(1 + x^2)^(-3/2)")]
        public void TheThirdCaseThroughTheReciprocal(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// Outside all three of Chebyshev's cases there is — and this is the part worth knowing
        /// before anyone goes looking — no elementary antiderivative at all, which he proved.
        /// These must be declined or answered by another rule, never wrong.
        /// </summary>
        [Theory]
        [InlineData("sqrt(1 + x^3)")]
        [InlineData("x*sqrt(1 + x^3)")]
        [InlineData("x^3*sqrt(1 + x^3)")]
        public void OutsideTheFirstCase(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("NaN", integral.Stringize());
            if (integral.Stringize().Contains("integral("))
                return;   // unanswered is a legitimate verdict; a wrong answer is not
            DifferentiatesBack(integrand);
        }

        /// <summary>
        /// What the read must refuse: a whole exponent on the bracket, which is a polynomial and
        /// wants expanding; a linear bracket, which the linear radical rule answers more shortly;
        /// and a quadratic one, which the trigonometric substitution answers more shortly still.
        /// </summary>
        [Theory]
        [InlineData("x^2*(1 + x^3)^2")]
        [InlineData("x*sqrt(1 + x)")]
        [InlineData("sqrt(1 + 2*x)")]
        [InlineData("x^3*sqrt(1 + x^2)")]
        [InlineData("1/(1 + x^2)^(3/2)")]
        public void TheNeighboursAreUntouched(string integrand) => DifferentiatesBack(integrand);
    }
}
