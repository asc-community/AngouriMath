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
    /// Whole powers of the secant and the cosecant, which the downward recurrence brings to the
    /// first power or to the zeroth.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Before this, <c>1/cos(x)^2</c> was answered and <c>1/cos(x)^6</c> was not: the rule for
    /// powers of sine and cosine reads a positive exponent, and a secant is a negative one. The
    /// square came out only because it is a standard integral in its own right.
    /// </para>
    /// <para>
    /// <b>Three spellings, one integrand.</b> <c>sec(u)^n</c>, <c>cos(u)^(-n)</c> and
    /// <c>1/cos(u)^n</c> all reach the integrator, and answering one and declining the others is
    /// the defect this file's neighbours have carried repeatedly. Each case below is written out
    /// in every spelling that parses to a different tree.
    /// </para>
    /// <para>
    /// Checked by differentiating back and comparing at points, never against a printed form:
    /// <c>∫sec(x)</c> comes out as an inverse hyperbolic tangent here rather than as
    /// <c>ln|sec + tan|</c>, and both are right.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class SecantPowerIntegralTest
    {
        /// <summary>
        /// Points inside one branch, away from the poles of both the secant and the cosecant —
        /// which sit at multiples of <c>pi/2</c> — so that every integrand here has a value.
        /// </summary>
        private static readonly double[] Points = { 0.35, 0.61, 0.9, 1.15, 1.35 };

        private static void DifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
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
        /// Even powers, which the recurrence takes down to the zeroth — so the last term is
        /// <c>x</c> and the answer is a polynomial in the tangent.
        /// </summary>
        [Theory]
        [InlineData("sec(x)^2")]
        [InlineData("sec(x)^4")]
        [InlineData("sec(x)^6")]
        [InlineData("1/cos(x)^4")]
        [InlineData("1/cos(x)^6")]
        [InlineData("1/cos(x)^12")]
        [InlineData("cos(x)^(-4)")]
        [InlineData("cos(x)^(-6)")]
        public void EvenPowersOfTheSecant(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// Odd powers, which land on the first — where the antiderivative is no longer
        /// elementary in the tangent alone and the inverse hyperbolic tangent appears.
        /// </summary>
        [Theory]
        [InlineData("sec(x)")]
        [InlineData("sec(x)^3")]
        [InlineData("sec(x)^5")]
        [InlineData("1/cos(x)^3")]
        [InlineData("1/cos(x)^5")]
        [InlineData("cos(x)^(-3)")]
        public void OddPowersOfTheSecant(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// The cosecant, which is the same recurrence with the cotangent in place of the tangent
        /// and the first term the other sign. It is a separate node here, so it is separate
        /// coverage rather than a symmetry anyone may assume.
        /// </summary>
        [Theory]
        [InlineData("csc(x)^2")]
        [InlineData("csc(x)^3")]
        [InlineData("csc(x)^4")]
        [InlineData("csc(x)^6")]
        [InlineData("1/sin(x)^4")]
        [InlineData("1/sin(x)^6")]
        [InlineData("sin(x)^(-4)")]
        public void PowersOfTheCosecant(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// A linear argument, which divides the whole answer by the rate. The constant term is
        /// there because a rule that reads only the coefficient gets <c>sec(3x + 1)</c> right by
        /// accident and <c>sec(3x)</c> wrong in the same way.
        /// </summary>
        [Theory]
        [InlineData("sec(2*x)^4")]
        [InlineData("sec(3*x + 1)^3")]
        [InlineData("csc(2*x + 1)^4")]
        [InlineData("1/cos(2*x)^6")]
        public void ALinearArgument(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// The neighbouring integrands this rule must not take over. Each of these is answered
        /// without it, and a reciprocal power that swallowed them would be a regression that no
        /// wrong answer reveals.
        /// </summary>
        [Theory]
        [InlineData("sin(x)^2")]
        [InlineData("cos(x)^3")]
        [InlineData("tan(x)^2")]
        [InlineData("cot(x)^2")]
        [InlineData("sin(x)*cos(x)")]
        public void TheNeighboursAreUntouched(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// A power of the secant multiplied by something else is <b>not</b> this rule's, and it
        /// must still be declined quickly rather than searched. The rule fires only on the
        /// integrand the caller asked about, for exactly this reason:
        /// <a href="https://github.com/asc-community/AngouriMath/issues/1265">#1265</a> — a rule
        /// that answers a sub-integral which used to come back unanswered lets the search that
        /// asked carry on, and the cost turns up on an integrand the rule never fires on.
        /// </summary>
        /// <remarks>
        /// Bounded by the integrator's own budget rather than by a wall clock, which measures the
        /// runner. <c>sec(x)^6*tan(x)^3</c> is declined in about a quarter of a second here and
        /// did not return in four hundred seconds when the rule answered sub-integrals too.
        /// </remarks>
        [Theory]
        [InlineData("sec(x)^6*tan(x)^3")]
        [InlineData("cot(x)^3*csc(x)^4")]
        public void AProductIsNotThisRules(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            // Unanswered or answered, either is a legitimate verdict; what is pinned is that the
            // answer, if there is one, is right.
            if (integral.Stringize().Contains("integral("))
                return;
            DifferentiatesBack(integrand);
        }
    }
}
