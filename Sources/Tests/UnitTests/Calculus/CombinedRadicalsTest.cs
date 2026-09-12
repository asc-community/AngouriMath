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
    /// Several square roots of polynomials in the variable written as one before the rules
    /// that read one radical look at the integrand: <c>sqrt(1 + x^2) sqrt(1 - x^2)</c> is
    /// <c>sqrt(1 - x^4)</c>.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>x/(sqrt(1 + x^2) sqrt(1 - x^2))</c> had no antiderivative and <c>x/sqrt(1 - x^4)</c>
    /// is one substitution. It is what by parts leaves from <c>arcsin(x)/(1 + x^2)^(3/2)</c>,
    /// from <c>ln(x + sqrt(1 + x^2))/(1 - x^2)^(3/2)</c>, and from every other pairing of an
    /// inverse function's radical with the one in the power it stands over.
    /// </para>
    /// <para>
    /// <b>The identity is conditional.</b> <c>sqrt(P) sqrt(Q) = sqrt(PQ)</c> on the principal
    /// branch only where at most one of <c>P</c>, <c>Q</c> is negative; with both negative
    /// the two sides differ by sign. The rule decides that from the real roots of the bases
    /// and declines otherwise, and the decline is pinned here beside the answers. Every answer
    /// is differentiated back, including at points where one base <em>is</em> negative and
    /// the integrand is real all the same.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class CombinedRadicalsTest
    {
        private static void DifferentiatesBack(string integrand, double[] points)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Assert.DoesNotContain("NaN", integral.Stringize());

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
                var difference = Math.Abs((double)(got - want).RealPart)
                               + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart));
                Assert.True(difference / scale < 1e-8,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, "
                    + $"where the integrand is {want}");
            }
            Assert.True(compared >= 3,
                $"only {compared} of {points.Length} points were comparable for {integrand}");
        }

        /// <summary>Inside <c>(-1, 1)</c>, where <c>sqrt(1 - x^2)</c> is real.</summary>
        private static readonly double[] InsideTheUnitInterval = { -0.7, -0.3, 0.2, 0.55, 0.85 };

        /// <summary>
        /// Two radicals above or below the bar, and the shapes by parts leaves them in:
        /// Charlwood's <c>arcsin(x)/(1 + x^2)^(3/2)</c> and the two logarithms.
        /// </summary>
        [Theory]
        [InlineData("x/(sqrt(1 + x^2)*sqrt(1 - x^2))")]
        [InlineData("x*sqrt(1 + x^2)*sqrt(1 - x^2)")]
        [InlineData("arcsin(x)/(1 + x^2)^(3/2)")]
        [InlineData("ln(x + sqrt(1 + x^2))/(1 - x^2)^(3/2)")]
        [InlineData("x*ln(x + sqrt(1 - x^2))/sqrt(1 - x^2)")]
        public void TwoRadicalsWrittenAsOne(string integrand) => DifferentiatesBack(integrand, InsideTheUnitInterval);

        /// <summary>
        /// <c>sqrt(x^2 - 1)</c> is negative inside the unit interval and <c>1 + x^2</c> never
        /// is, so the identity holds on the whole line; checked past <c>1</c>, where the
        /// integrand is real.
        /// </summary>
        [Theory]
        [InlineData("x/(sqrt(1 + x^2)*sqrt(x^2 - 1))")]
        [InlineData("ln(x + sqrt(x^2 - 1))/(1 + x^2)^(3/2)")]
        public void OneBaseMayBeNegative(string integrand) => DifferentiatesBack(integrand, new[] { 1.2, 1.6, 2.3, 3.1 });

        /// <summary>
        /// A root of a quotient of polynomials written as a quotient of roots, where that is
        /// exact: <c>sqrt(P/Q) = sqrt(P)/sqrt(Q)</c> unless <c>Q &lt; 0 &lt; P</c> somewhere on the
        /// reals. Charlwood's <c>arcsin(x/sqrt(1 - x^2))</c> by parts against one leaves
        /// <c>x (1 - x^2)^(-3/2)/sqrt((1 - 2x^2)/(1 - x^2))</c>, which no rule reads and which
        /// split is <c>x/((1 - x^2) sqrt(1 - 2x^2))</c>. Checked where the arcsine is real,
        /// <c>|x| &lt; 1/sqrt(2)</c>, on both sides of zero.
        /// </summary>
        [Theory]
        [InlineData("arcsin(x/sqrt(1 - x^2))")]
        [InlineData("x*(1 - x^2)^(-3/2)/sqrt((1 - 2*x^2)/(1 - x^2))")]
        public void ARootOfAQuotientIsSplit(string integrand) => DifferentiatesBack(integrand, new[] { -0.6, -0.3, 0.2, 0.45, 0.65 });

        /// <summary>
        /// <c>sqrt((1 + x)/(3 + 2x))</c> is not answered: <c>3 + 2x &lt; 0 &lt; 1 + x</c> nowhere, so
        /// the split is exact, but the two roots it leaves are both negative below
        /// <c>-3/2</c> and no rule takes them further; and <c>x/((1 - x^2) sqrt((1 - 2x^2)/(1 - x^2)))</c>
        /// splits and gathers to <c>x/(sqrt(1 - x^2) sqrt(1 - 2x^2))</c>, two roots both
        /// negative past <c>1</c>, where the integrand is real, so those are not combined
        /// either. Either verdict but a wrong answer.
        /// </summary>
        [Theory]
        [InlineData("sqrt((1 + x)/(3 + 2*x))")]
        [InlineData("sqrt((1 + x^2)/(1 - x^2))")]
        [InlineData("x/((1 - x^2)*sqrt((1 - 2*x^2)/(1 - x^2)))")]
        public void ARootOfAQuotientTheRulesBehindCannotUse(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("NaN", integral.Stringize());
            if (integral.Stringize().Contains("integral("))
                return;
            DifferentiatesBack(integrand, new[] { -0.7, -0.3, 0.2, 0.55, 0.85 });
        }

        /// <summary>
        /// A small power of a sum of radicals, as a factor, is written out first:
        /// Bondarenko's <c>1/(sqrt(1 - x) + sqrt(1 + x))^2</c> is <c>1/(2 + 2 sqrt(1 - x^2))</c>,
        /// which Euler's substitution answers.
        /// </summary>
        [Theory]
        [InlineData("1/(sqrt(1 - x) + sqrt(1 + x))^2")]
        [InlineData("(sqrt(1 - x) + sqrt(1 + x))^2")]
        public void APowerOfASumOfRadicalsIsWrittenOut(string integrand) => DifferentiatesBack(integrand, InsideTheUnitInterval);

        /// <summary>
        /// <c>sqrt(x - 1) sqrt(x - 2)</c> is <c>-sqrt((x - 1)(x - 2))</c> below <c>1</c>, where both
        /// bases are negative, so the rule must not combine them. Either verdict but a wrong
        /// answer: whatever comes back is differentiated back below <c>1</c> as well as above
        /// <c>2</c>.
        /// </summary>
        [Theory]
        [InlineData("sqrt(x - 1)*sqrt(x - 2)")]
        [InlineData("1/(sqrt(x - 1)*sqrt(x - 2))")]
        [InlineData("x/(sqrt(x - 1)*sqrt(x - 2))")]
        public void TwoNegativeBasesAreNotCombined(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("NaN", integral.Stringize());
            if (integral.Stringize().Contains("integral("))
                return;   // unanswered is a legitimate verdict; a wrong answer is not
            DifferentiatesBack(integrand, new[] { -1.5, -0.4, 0.3, 2.4, 3.1, 4.7 });
        }

        /// <summary>
        /// The identity itself, at a point where both bases are negative, so that the
        /// condition the rule checks is pinned as a fact and not as a convention:
        /// <c>sqrt(-1) sqrt(-2)</c> is <c>-sqrt(2)</c> and <c>sqrt(2)</c> is not.
        /// </summary>
        [Fact]
        public void TheIdentityFailsWithTwoNegativeBases()
        {
            var left = "sqrt(x - 1)*sqrt(x - 2)".ToEntity().Substitute("x", 0).EvalNumerical();
            var right = "sqrt((x - 1)*(x - 2))".ToEntity().Substitute("x", 0).EvalNumerical();
            Assert.True(Math.Abs((double)(left + right).RealPart) < 1e-12, $"{left} against {right}");
            Assert.True(Math.Abs((double)left.RealPart + Math.Sqrt(2)) < 1e-12, $"{left}");
        }
    }
}
