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

        private static void DifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Assert.DoesNotContain("NaN", integral.Stringize());

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
    }
}
