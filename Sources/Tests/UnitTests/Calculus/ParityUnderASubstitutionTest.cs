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
    /// A substitution whose variable may be negative, with an even power of it under a root:
    /// answered for the variable positive and extended to the other side by parity.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Charlwood's <c>sin(x)/sqrt(1 - sin(x)^6)</c> had no antiderivative. Under
    /// <c>u = cos(x)</c> -- a candidate now whenever a sine of the argument is written, since
    /// the sine that is left is the differential and its even powers are <c>1 - u^2</c> -- it
    /// is <c>-1/sqrt(u^2 (3 - 3u^2 + u^4))</c>, and <c>u</c> comes out of the root as
    /// <c>|u|</c>. The rule takes <c>|u| = u</c>, which is an antiderivative for <c>u &gt; 0</c>,
    /// and extends it by parity: the integrand is even in <c>u</c>, so the antiderivative is
    /// <c>sgn(u) F(|u|)</c>. Nothing is known of the sign of a cosine, and that is what makes
    /// it not matter.
    /// </para>
    /// <para>
    /// Every answer is differentiated back on both sides of zero, where the sine and the
    /// cosine take both signs.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class ParityUnderASubstitutionTest
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
            Assert.True(compared >= 5,
                $"only {compared} of {points.Length} points were comparable for {integrand}");
        }

        /// <summary>Away from the zeros of the sine and the cosine, on both sides of zero.</summary>
        private static readonly double[] BothSigns = { -2.5, -1.1, -0.4, 0.4, 1.1, 2.0 };

        /// <summary>
        /// Charlwood's pair, and the cosine version: the complement of the written function
        /// is the substitution, and the even power of the variable under the root is what
        /// the parity extension is for. The secant is written as the reciprocal of the cosine
        /// under its root first, where <c>sec(x)^4 - 1</c> is <c>(1 - cos(x)^4)/cos(x)^4</c> and
        /// the even power below comes out of the root whole.
        /// </summary>
        [Theory]
        [InlineData("sin(x)/sqrt(1 - sin(x)^6)")]
        [InlineData("cos(x)/sqrt(1 - cos(x)^4)")]
        [InlineData("sec(x)/sqrt(sec(x)^4 - 1)")]
        public void TheComplementOfTheWrittenFunction(string integrand) => DifferentiatesBack(integrand, BothSigns);

        /// <summary>
        /// The same collecting of powers, for a power of <c>x</c> itself: under <c>u = x^8</c>
        /// the quotient by <c>du/dx</c> writes <c>x^7</c> beside the integrand's <c>x</c>, and
        /// only collected is it <c>x^8</c>. Bronstein's, on both sides of zero.
        /// </summary>
        [Theory]
        [InlineData("(1 + 2*x^8)*sqrt(1 + x^8)/(x + 2*x^9 + x^17)")]
        public void ThePowersOfTheVariableAreCollectedFirst(string integrand)
            => DifferentiatesBack(integrand, new[] { -1.3, -0.9, -0.5, 0.4, 0.8, 1.2 });

        /// <summary>
        /// An even root is not negative, and what holds for it positive is the answer as it
        /// stands: <c>sqrt(x^(1/3))</c> here was extended by a parity it did not need, to an
        /// answer with a sign function in it that nothing could differentiate.
        /// </summary>
        [Fact]
        public void AnEvenRootIsNotExtended()
        {
            var integral = "1/(sqrt(x) - x^(-1/3))".ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Assert.DoesNotContain("sgn(", integral.Stringize());
            DifferentiatesBack("1/(sqrt(x) - x^(-1/3))", new[] { 1.3, 1.9, 2.7, 3.6, 5.2 });
        }
    }
}
