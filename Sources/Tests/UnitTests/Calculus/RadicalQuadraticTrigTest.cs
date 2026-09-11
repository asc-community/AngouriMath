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
    /// A power of the variable times an odd half-power of <c>a + b x^2</c>, answered through the
    /// trigonometric substitution rather than by rationalising the radical.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>1/(1 + x^2)^(3/2)</c>, <c>x^5/sqrt(5 + x^2)</c> and <c>1/(1 - x^2)^(3/2)</c> had no
    /// antiderivative. A square root of a quadratic is the largest single class the Rubi suites
    /// leave unanswered here, and this is the part of it that comes out <b>closed</b>: the
    /// substitution lands on <c>sin^p cos^q</c>, which
    /// <a href="https://github.com/asc-community/AngouriMath/pull/1268">#1268</a> integrates term
    /// by term without asking the integrator anything.
    /// </para>
    /// <para>
    /// Checked by differentiating back and comparing at points, never against a printed form —
    /// these answers come back in the radical and a textbook writes several of them with an
    /// inverse trigonometric function instead.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class RadicalQuadraticTrigTest
    {
        /// <summary>
        /// Inside <c>(0, 1)</c>, which is where <c>sqrt(1 - x^2)</c> is real and positive and
        /// every integrand here has a value.
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
        /// <c>b &gt; 0</c>, the tangent substitution. The radicand is positive everywhere, so
        /// there is no condition on the answer.
        /// </summary>
        [Theory]
        [InlineData("1/(1 + x^2)^(3/2)")]
        [InlineData("1/(4 + x^2)^(3/2)")]
        [InlineData("1/(1 + x^2)^(5/2)")]
        [InlineData("x^5/sqrt(5 + x^2)")]
        [InlineData("x^3/sqrt(1 + x^2)")]
        [InlineData("x^3/(1 + x^2)^(3/2)")]
        [InlineData("1/(x^2*sqrt(1 + x^2))")]
        public void TheTangentSubstitution(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// <c>b &lt; 0</c>, the sine substitution, where the radicand is positive only between
        /// the two roots — which is where the sample points are.
        /// </summary>
        [Theory]
        [InlineData("1/(1 - x^2)^(3/2)")]
        [InlineData("x/(1 - x^2)^(3/2)")]
        [InlineData("x^3/sqrt(1 - x^2)")]
        [InlineData("x^5/sqrt(1 - x^2)")]
        [InlineData("1/(x^2*sqrt(1 - x^2))")]
        [InlineData("1/(4 - x^2)^(3/2)")]
        public void TheSineSubstitution(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// A negative power of the variable alongside the radical, and a rational factor in
        /// front — both of which the read carries rather than declining over.
        /// </summary>
        [Theory]
        [InlineData("3/(1 + x^2)^(3/2)")]
        [InlineData("1/(2*(1 + x^2)^(3/2))")]
        [InlineData("x/(2*sqrt(1 - x^2))")]
        [InlineData("x^3/(2*sqrt(1 + x^2))")]
        public void AFactorAndANegativePower(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// A quadratic with a linear term, which is completed to a square first: <c>y = x + h</c>
        /// turns <c>a + b x + c x^2</c> into <c>A + c y^2</c>, and <c>x^m</c> into
        /// <c>(y - h)^m</c> — a sum of <c>m + 1</c> terms, each of them this rule's shape again,
        /// so the answer is their sum and the whole thing stays closed.
        /// </summary>
        [Theory]
        [InlineData("1/(1 + x + x^2)^(3/2)")]
        [InlineData("x/(1 + x + x^2)^(3/2)")]
        [InlineData("1/(x^2 + 2*x + 2)^(3/2)")]
        [InlineData("1/(4*x^2 + 4*x + 2)^(3/2)")]
        [InlineData("1/(2 + 2*x + x^2)^(3/2)")]
        public void ALinearTermIsCompletedToASquare(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// The boundary, and it is the same one <c>sin^p cos^q</c> has: with both exponents even
        /// and summing to at least zero, the substituted integrand is not a Laurent polynomial and
        /// this rule has nothing closed to offer. <c>x^2/sqrt(1 + x^2)</c> and
        /// <c>sqrt(1 - x^2)/x^2</c> are elementary — they want an inverse hyperbolic or inverse
        /// trigonometric term — and are declined rather than half-answered. So does
        /// <c>1/(x^3 sqrt(1 + x^2))</c>, where the exponents are <c>p = -3</c> and <c>q = 2</c>:
        /// odd but negative, so the sine does not go into <c>du</c>, and mixed in parity, so the
        /// tangent leaves a root behind.
        /// </summary>
        [Theory]
        [InlineData("x^2/sqrt(1 + x^2)")]
        [InlineData("sqrt(1 - x^2)/x^2")]
        [InlineData("1/(x^3*sqrt(1 + x^2))")]
        [InlineData("1/(x*sqrt(1 + x^2))")]
        // And after completing the square, where one term of the expansion falls outside even
        // though the others do not: `x/sqrt(1 + x + x^2)` leaves an `int dy/sqrt(A + c y^2)`,
        // which is an inverse hyperbolic sine. One term outside means the whole sum is.
        [InlineData("x/sqrt(1 + x + x^2)")]
        [InlineData("x^2/(1 + x + x^2)^(3/2)")]
        public void OutsideTheClosedPart(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("NaN", integral.Stringize());
            if (integral.Stringize().Contains("integral("))
                return;   // unanswered is a legitimate verdict; a wrong answer is not
            DifferentiatesBack(integrand);
        }

        /// <summary>
        /// What the read must refuse: an even half-power, which is a whole power of a polynomial
        /// and the power rule's; a quadratic whose coefficients are not decided, where the
        /// substitution cannot know which of the two it is; and one whose constant term is
        /// negative after the square is completed, which is the secant substitution and a branch
        /// this does not have.
        /// </summary>
        [Theory]
        [InlineData("x/(1 + x^2)^2")]
        [InlineData("1/(a + b*x^2)^(3/2)")]
        [InlineData("1/(x^2 - 1)^(3/2)")]
        public void WhatTheReadRefuses(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("NaN", integral.Stringize());
            if (integral.Stringize().Contains("integral("))
                return;
            // Answered by some other rule, which is fine — but it has to be right.
            var derivative = integral.Substitute("C", 0).Differentiate("x")
                                     .Substitute("a", 2).Substitute("b", 3);
            var original = integrand.ToEntity().Substitute("a", 2).Substitute("b", 3);
            foreach (var at in Points)
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                if (got.IsNaN || want.IsNaN)
                    continue;
                var difference = Math.Abs((double)(got - want).RealPart)
                               + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart));
                Assert.True(difference / scale < 1e-9,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, "
                    + $"where the integrand is {want}");
            }
        }

        /// <summary>
        /// The neighbours this must leave alone: radicals of something linear, a rational
        /// function with no radical at all, and the two shapes the general substitution already
        /// answered.
        /// </summary>
        [Theory]
        [InlineData("sqrt(x)")]
        [InlineData("1/sqrt(x)")]
        [InlineData("sqrt(1 + x)")]
        [InlineData("1/sqrt(1 - x^2)")]
        [InlineData("x/sqrt(1 + x^2)")]
        [InlineData("sqrt(1 + x^2)")]
        [InlineData("1/(1 + x^2)")]
        [InlineData("x/(1 + x^2)")]
        [InlineData("x^2")]
        [InlineData("sin(x)")]
        public void TheNeighboursAreUntouched(string integrand) => DifferentiatesBack(integrand);
    }
}
