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
    /// A rational function of <c>x</c> and one square root of a palindromic quartic
    /// <c>a x^4 + b x^2 + a</c>, integrated by <c>u = x - 1/x</c> or <c>u = x + 1/x</c>.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Charlwood's <c>(1 + x^2)/((1 - x^2) sqrt(1 + x^4))</c> had no antiderivative: a root of a
    /// quartic is nothing Euler's substitutions read. But <c>1 + x^4 = x^2 ((x - 1/x)^2 + 2)</c>,
    /// and under <c>u = x - 1/x</c> the integrand is <c>-du/(u sqrt(u^2 + 2))</c>.
    /// </para>
    /// <para>
    /// <c>sqrt(1 + x^4) = |x| sqrt(u^2 + 2)</c>, and the rule takes <c>|x| = x</c>, so what it
    /// finds is an antiderivative for <c>x &gt; 0</c>; it is extended to the other side by
    /// parity -- <c>F(|x|)</c> for an odd integrand, <c>sgn(x) F(|x|)</c> for an even one. So
    /// every answer here is differentiated back on <b>both</b> sides of zero, which is the
    /// claim the extension makes.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class ReciprocalSubstitutionTest
    {
        /// <summary>Both sides of zero, away from the poles at <c>+-1</c>.</summary>
        private static readonly double[] Points = { -2.7, -1.6, -0.6, -0.3, 0.4, 0.7, 1.5, 2.3 };

        private static void DifferentiatesBack(string integrand, params (string symbol, double value)[] pinned)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Assert.DoesNotContain("NaN", integral.Stringize());

            var derivative = integral.Substitute("C", 0).Differentiate("x");
            var original = integrand.ToEntity();
            foreach (var (symbol, value) in pinned)
            {
                derivative = derivative.Substitute(symbol, value);
                original = original.Substitute(symbol, value);
            }
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
                Assert.True(difference / scale < 1e-8,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, "
                    + $"where the integrand is {want}");
            }
            Assert.True(compared >= 6,
                $"only {compared} of {Points.Length} points were comparable for {integrand}");
        }

        /// <summary>
        /// Charlwood's pair, one for each sign of the substitution, and Welz's
        /// <c>sqrt(1 + x^4)/(1 - x^4)</c> with the root above the bar.
        /// </summary>
        [Theory]
        [InlineData("(1 + x^2)/((1 - x^2)*sqrt(1 + x^4))")]
        [InlineData("(1 - x^2)/((1 + x^2)*sqrt(1 + x^4))")]
        [InlineData("sqrt(1 + x^4)/(1 - x^4)")]
        [InlineData("(x^2 + 1)/(x*sqrt(x^4 + 3*x^2 + 1))")]
        public void ARootOfAPalindromicQuartic(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// The odd terms too: <c>a x^4 + d x^3 + b x^2 + s d x + a</c> is <c>x^2</c> times
        /// <c>a u^2 + d u + b - 2as</c> under <c>u = x + s/x</c>, and the odd terms fix which
        /// sign it is. Such an integrand has no parity, so the answer for <c>x &gt; 0</c> is
        /// extended by the sign of <c>x</c> instead, which is exact: the root is
        /// <c>|x| sqrt(q(u))</c> on both sides. Both sides are checked.
        /// </summary>
        [Theory]
        [InlineData("(1 - x^2)/((1 + x + x^2)*sqrt(1 + x + 3*x^2 + x^3 + x^4))")]
        [InlineData("(1 + x^2)/(x*sqrt(1 - x + 3*x^2 + x^3 + x^4))")]
        public void APalindromicQuarticWithOddTerms(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// Timofeev's, with symbols for coefficients: the quartic is read with <c>2a - 2a</c>
        /// taken as zero, and the bracket <c>-x/(1 + 2ax + x^2)</c> is written as
        /// <c>-1/(u + 2a)</c> by a solve that pins the symbols first to find which
        /// coefficients there are, and solves for those symbolically.
        /// </summary>
        [Fact]
        public void TheCoefficientsMayBeSymbols()
            => DifferentiatesBack("(1 - x^2)/((1 + 2*a*x + x^2)*sqrt(1 + 2*a*x + 2*b*x^2 + 2*a*x^3 + x^4))", ("a", 0.3), ("b", 0.5));

        /// <summary>
        /// A hyperbolic function under the root is a palindromic quartic under it once
        /// <c>u = e^x</c> is in: <c>1 - sinh(x)^2</c> is <c>(6u^2 - u^4 - 1)/(4u^2)</c>, and
        /// <c>u - 1/u</c> is <c>2 sinh(x)</c>. The exponential substitution asks this rule
        /// directly, saying that <c>u</c> is positive, so no parity extension is needed; and
        /// the quartic here is to the three halves, read as the quartic beside its root.
        /// </summary>
        [Theory]
        [InlineData("sinh(x)^2*sinh(2*x)/(1 - sinh(x)^2)^(3/2)")]
        public void AHyperbolicFunctionUnderTheRoot(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// The extension by parity is the claim, so it is pinned as a fact: the even integrand's
        /// antiderivative is odd, and the answer carries a <c>sgn(x)</c> for it.
        /// </summary>
        [Fact]
        public void AnEvenIntegrandGetsAnOddAntiderivative()
        {
            var integral = "(1 + x^2)/((1 - x^2)*sqrt(1 + x^4))".ToEntity().Integrate("x").Substitute("C", 0);
            var atPositive = integral.Substitute("x", 0.4).EvalNumerical();
            var atNegative = integral.Substitute("x", -0.4).EvalNumerical();
            Assert.True(Math.Abs((double)(atPositive + atNegative).RealPart) < 1e-9, $"{atPositive} against {atNegative}");
        }

        /// <summary>
        /// What the rule declines rather than guessing at: a rational part that is not a
        /// function of <c>x -+ 1/x</c> -- <c>(1 + x^2)/sqrt(1 + x^4)</c> is elliptic -- and a
        /// quartic that is not palindromic. Either verdict but a wrong answer.
        /// </summary>
        [Theory]
        [InlineData("(1 + x^2)/sqrt(1 + x^4)")]
        [InlineData("1/sqrt(1 + x^4)")]
        [InlineData("(1 + x^2)/((1 - x^2)*sqrt(2 + x^4))")]
        public void WhatIsNotAFunctionOfTheReciprocalVariable(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("NaN", integral.Stringize());
            if (integral.Stringize().Contains("integral("))
                return;   // unanswered is a legitimate verdict; a wrong answer is not
            DifferentiatesBack(integrand);
        }

        /// <summary>
        /// A power of the variable below the bar beside a root of a quadratic, by the same
        /// reciprocal: <c>1/(x^n sqrt(q0 + q1 x + q2 x^2))</c> is
        /// <c>-sgn(t) t^(n - 1)/sqrt(q0 t^2 + q1 t + q2)</c>, a polynomial over the root of the
        /// quadratic read the other way round. <c>1/(x sqrt(1 - (a + b x)^2))</c> was answered
        /// and the repeated factor was not, which is what a round of parts against
        /// <c>acos(a + b x)/x^4</c> leaves.
        /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
        /// </summary>
        [Theory]
        [InlineData("1/(x^2*sqrt(1 + x^2))")]
        [InlineData("1/(x^3*sqrt(4 + 3*x + 2*x^2))")]
        [InlineData("1/(x^2*sqrt(1 + x + x^2))")]
        [InlineData("1/(x^4*sqrt(2 - x + x^2))")]
        public void APowerOfTheVariableBesideARootOfAQuadratic(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// And with symbols among the coefficients, which is the shape Rubi's 5.2 rows leave:
        /// <c>acos(a + b x)/x^4</c> by parts is <c>1/(x^3 sqrt(1 - (a + b x)^2))</c> up to the
        /// constants.
        /// </summary>
        [Theory]
        [InlineData("1/(x^2*sqrt(1 - (a + b*x)^2))")]
        [InlineData("1/(x^3*sqrt(1 - (a + b*x)^2))")]
        [InlineData("acos(a + b*x)/x^4")]
        public void TheSameWithSymbolicCoefficients(string integrand)
            => DifferentiatesBack(integrand, ("a", 0.3), ("b", 0.4));
    }
}
