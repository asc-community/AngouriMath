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
    /// An integrand holding an inverse trigonometric function of the variable, integrated by
    /// the substitution that undoes it: <c>x = sin(u)</c> for <c>arcsin(x)</c>, and likewise for
    /// the cosine, the tangent and the secant.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>e^(arcsin(x)) x^3/sqrt(1 - x^2)</c> had no antiderivative. Under <c>x = sin(u)</c> it is
    /// <c>e^u sin(u)^3</c>, and <c>x arcsec(x)/sqrt(x^2 - 1)</c> is <c>u sec(u)^2</c> under
    /// <c>x = sec(u)</c>. The general substitution does not find these because what is left
    /// beside the substituted subtree is <c>x</c> itself, which only the inverse substitution
    /// removes -- the same reason the logarithm has one.
    /// </para>
    /// <para>
    /// The radical goes by construction: <c>sqrt(1 - x^2)</c> is <c>cos(u)</c> on the branch the
    /// arcsine is defined on, where the cosine is not negative. Every answer is differentiated
    /// back at points on that branch.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class InverseTrigonometricSubstitutionTest
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

        /// <summary>Inside <c>(-1, 1)</c>, where the arcsine, the arccosine and their radical are real.</summary>
        private static readonly double[] InsideTheUnitInterval = { -0.7, -0.3, 0.2, 0.55, 0.85 };

        /// <summary>Past <c>1</c>, where the arcsecant and <c>sqrt(x^2 - 1)</c> are real.</summary>
        private static readonly double[] PastOne = { 1.3, 1.8, 2.5, 3.7 };

        /// <summary>
        /// The sine and the cosine: Charlwood's <c>e^(arcsin(x)) x^3/sqrt(1 - x^2)</c>, which is
        /// <c>e^u sin(u)^3</c>, and its neighbours.
        /// </summary>
        [Theory]
        [InlineData("e^(arcsin(x))*x^3/sqrt(1 - x^2)")]
        [InlineData("e^(arcsin(x))*x/sqrt(1 - x^2)")]
        [InlineData("e^(arccos(x))*x^2/sqrt(1 - x^2)")]
        [InlineData("arcsin(x)/(1 + sqrt(1 - x^2))")]
        [InlineData("arcsin(x)^2*x/sqrt(1 - x^2)")]
        [InlineData("x^3*arcsin(x)/(1 - x^2)^(3/2)")]
        public void TheSineAndTheCosine(string integrand) => DifferentiatesBack(integrand, InsideTheUnitInterval);

        /// <summary>
        /// The tangent, whose radical is <c>sqrt(1 + x^2)</c> and becomes the secant.
        /// </summary>
        [Theory]
        [InlineData("e^(arctan(x))/(1 + x^2)^(3/2)")]
        [InlineData("arctan(x)/(1 + x^2)^(3/2)")]
        public void TheTangent(string integrand) => DifferentiatesBack(integrand, new[] { -1.6, -0.4, 0.3, 1.1, 2.4 });

        /// <summary>
        /// The secant: Charlwood's <c>x arcsec(x)/sqrt(x^2 - 1)</c>, which is <c>u sec(u)^2</c>.
        /// </summary>
        [Theory]
        [InlineData("x*arcsec(x)/sqrt(x^2 - 1)")]
        [InlineData("x*arcsec(x)/sqrt(x^2 - 1)^3")]
        public void TheSecant(string integrand) => DifferentiatesBack(integrand, PastOne);

        /// <summary>
        /// The radical in both of its spellings: <c>(1 - x^2)^(3/2)</c> and <c>sqrt(1 - x^2)^3</c>
        /// are one function on the principal branch, and the second was declined -- by this rule
        /// for the spelling, and then by a by-parts search that did not return.
        /// </summary>
        [Theory]
        [InlineData("arcsin(x)/sqrt(1 - x^2)^3", "arcsin(x)/(1 - x^2)^(3/2)")]
        [InlineData("arctan(x)/sqrt(1 + x^2)^3", "arctan(x)/(1 + x^2)^(3/2)")]
        public void BothSpellingsOfTheRadical(string nested, string flat)
        {
            DifferentiatesBack(nested, new[] { -0.7, -0.3, 0.2, 0.55 });
            DifferentiatesBack(flat, new[] { -0.7, -0.3, 0.2, 0.55 });
        }

        /// <summary>
        /// What the substitution is not for, and declines at once rather than searching: no
        /// radical to remove and no exponential of the inverse function -- <c>x arcsin(x)</c> is
        /// by parts' and its answer stays in <c>x</c> -- a radical the construction does not
        /// reach, two inverse functions, and an inverse function of something other than the
        /// bare variable. Each is either answered correctly by another rule or declined; a wrong
        /// answer is what is pinned against.
        /// </summary>
        [Theory]
        [InlineData("x*arcsin(x)", new[] { -0.7, -0.3, 0.2, 0.55, 0.85 })]
        [InlineData("x^3*arcsin(x)/sqrt(1 - x^4)", new[] { -0.7, -0.3, 0.2, 0.55, 0.85 })]
        [InlineData("arcsin(x)*arctan(x)", new[] { -0.7, -0.3, 0.2, 0.55, 0.85 })]
        [InlineData("e^(arcsin(2*x))*x/sqrt(1 - 4*x^2)", new[] { -0.4, -0.2, 0.1, 0.3, 0.45 })]
        public void WhatItIsNotFor(string integrand, double[] points)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("NaN", integral.Stringize());
            if (integral.Stringize().Contains("integral("))
                return;   // unanswered is a legitimate verdict; a wrong answer is not
            DifferentiatesBack(integrand, points);
        }

        /// <summary>
        /// <c>x arcsin(x)</c> is answered in <c>x</c>, not in <c>sin(2 arcsin(x))</c>: the
        /// substitution declines it, and by parts gives the answer the reader expects.
        /// </summary>
        [Fact]
        public void APolynomialTimesTheInverseStaysInX()
        {
            var integral = "x*arcsin(x)".ToEntity().Integrate("x").Stringize();
            Assert.DoesNotContain("integral(", integral);
            Assert.DoesNotContain("sin(arcsin", integral);
            Assert.DoesNotContain("cos(arcsin", integral);
        }
    }
}
