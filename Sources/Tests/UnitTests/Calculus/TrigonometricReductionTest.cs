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
    /// <c>sin^p cos^q</c> where no substitution leaves a polynomial, answered by the four standard
    /// recurrences instead — and, through the quadratic radical rule, every integrand that reaches
    /// that shape.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <a href="https://github.com/asc-community/AngouriMath/pull/1268">#1268</a> answers
    /// <c>sin^p cos^q</c> by substituting and integrating a polynomial term by term, which covers
    /// the cases where one exponent is odd and positive or both are even and sum to at most
    /// <c>-2</c>. The rest need a <b>logarithm</b>, and no rearrangement of a polynomial produces
    /// one: <c>sec(x)^3</c>, <c>tan(x)^2 sec(x)</c> and <c>cos(x)^2/sin(x)^3</c> were all declined.
    /// </para>
    /// <para>
    /// <a href="https://github.com/asc-community/AngouriMath/pull/1273">#1273</a> sends a square
    /// root of a quadratic through the same shape, so the same gap was there in algebraic clothes:
    /// <c>x^2/sqrt(1 + x^2)</c> is <c>sin^2 cos^(-3)</c> under the tangent substitution, and
    /// <c>sqrt(1 - x^2)/x^2</c> is <c>sin^(-2) cos^2</c> under the sine one.
    /// </para>
    /// <para>
    /// Checked by differentiating back and comparing at points. These answers hold an inverse
    /// hyperbolic tangent where a textbook writes <c>ln|sec + tan|</c>, and an arcsine or arctangent
    /// wherever the recursion bottoms out on <c>int 1 dt</c> — all of them right, and none of them
    /// comparable by shape.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class TrigonometricReductionTest
    {
        /// <summary>
        /// Inside <c>(0, 1)</c>, away from the poles of all six trigonometric functions and inside
        /// the interval where <c>sqrt(1 - x^2)</c> is real.
        /// </summary>
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
        /// The trigonometric shapes, each of them a mixed parity or a negative exponent that no
        /// substitution turns into a polynomial.
        /// </summary>
        [Theory]
        [InlineData("sec(x)^3")]
        [InlineData("csc(x)^3")]
        [InlineData("tan(x)^2*sec(x)")]
        [InlineData("cot(x)^2*csc(x)")]
        [InlineData("sin(x)^2/cos(x)^3")]
        [InlineData("cos(x)^2/sin(x)^3")]
        [InlineData("1/(sin(x)^2*cos(x)^3)")]
        [InlineData("sin(x)^2*cos(x)^2")]
        [InlineData("sin(x)^4/cos(x)^2")]
        public void WhereNoSubstitutionLeavesAPolynomial(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// The same gap in algebraic clothes, through the quadratic radical rule. These are the
        /// classic radicals of a quadratic, and each answer holds the inverse function the
        /// recursion's last step produces.
        /// </summary>
        [Theory]
        [InlineData("x^2/sqrt(1 + x^2)")]
        [InlineData("x^2/sqrt(1 - x^2)")]
        [InlineData("sqrt(1 + x^2)")]
        [InlineData("sqrt(1 - x^2)")]
        [InlineData("sqrt(1 - x^2)/x^2")]
        [InlineData("x^2*sqrt(1 + x^2)")]
        [InlineData("1/(x*sqrt(1 + x^2))")]
        [InlineData("1/(x^3*sqrt(1 + x^2))")]
        public void ARadicalOfAQuadraticThroughTheSameShape(string integrand)
            => DifferentiatesBack(integrand);

        /// <summary>
        /// With a linear term in the quadratic as well, where the shift and the recursion compose:
        /// these need completing the square <b>and</b> a step that bottoms out on an inverse
        /// function.
        /// </summary>
        [Theory]
        [InlineData("x/sqrt(1 + x + x^2)")]
        [InlineData("x^2/(1 + x + x^2)^(3/2)")]
        [InlineData("x/sqrt(5 + 4*x - x^2)")]
        [InlineData("1/sqrt(1 + x + x^2)")]
        public void AShiftedQuadraticAsWell(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// Every base case of the recursion, reached directly. Two of them are the cosecant and
        /// the secant, and the cosecant is written as <c>-artanh(cos)</c> rather than the usual
        /// <c>ln|tan(t/2)|</c> — the same function, and the only form a caller substituting the
        /// trigonometric functions back can express, since a half-angle is not one of them.
        /// </summary>
        [Theory]
        [InlineData("sin(x)")]
        [InlineData("cos(x)")]
        [InlineData("sin(x)*cos(x)")]
        [InlineData("tan(x)")]
        [InlineData("cot(x)")]
        [InlineData("1/(sin(x)*cos(x))")]
        [InlineData("sec(x)")]
        [InlineData("csc(x)")]
        public void EveryBaseCase(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// What must not move: the shapes the polynomial route already answered, which reach it
        /// first and must keep the shorter answer it gives.
        /// </summary>
        [Theory]
        [InlineData("sin(x)^3*cos(x)^2")]
        [InlineData("tan(x)^3*sec(x)^4")]
        [InlineData("cot(x)^3*csc(x)^4")]
        [InlineData("sec(x)^4")]
        [InlineData("csc(x)^6")]
        [InlineData("tan(x)^2")]
        [InlineData("sin(x)^2")]
        [InlineData("cos(x)^6")]
        [InlineData("1/(1 + x^2)^(3/2)")]
        [InlineData("x^5/sqrt(5 + x^2)")]
        [InlineData("x*sin(x)")]
        [InlineData("1/(1 + x^2)")]
        [InlineData("sqrt(x)")]
        public void TheNeighboursAreUntouched(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// A fractional exponent is still outside this: the recursion walks two <b>whole</b>
        /// exponents towards a fixed set, and there is no such walk from a half. The polynomial
        /// route still takes what it can there, so these must be answered or declined, never wrong.
        /// </summary>
        [Theory]
        [InlineData("sin(x)^(1/2)*cos(x)^(1/2)")]
        [InlineData("sqrt(tan(x))")]
        [InlineData("sin(x)^(1/2)")]
        public void AFractionalExponentIsNotThisRules(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("NaN", integral.Stringize());
            if (integral.Stringize().Contains("integral("))
                return;
            DifferentiatesBack(integrand);
        }
    }
}
