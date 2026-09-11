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
    /// A power of the sine times a power of the cosine — which is every product of the six
    /// trigonometric functions, once a tangent and a secant are read as the pair of exponents
    /// they are.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>tan(x)^m sec(x)^n</c> is <c>sin^m cos^(-m-n)</c> and <c>cot(x)^m csc(x)^n</c> is
    /// <c>cos^m sin^(-m-n)</c>, so the whole family is one rule and one test. Each of these was
    /// declined before it, including <c>sec(x)^6 tan(x)^3</c>, which is the integrand
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1265">#1265</a> was written
    /// about.
    /// </para>
    /// <para>
    /// Checked by differentiating back and comparing at points, never against a printed form: the
    /// substitution decides whether an answer comes out in the cosine, the sine or the tangent,
    /// and all three are right.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class TrigonometricPowerProductTest
    {
        /// <summary>
        /// Inside one branch and away from the poles of all six functions, which sit at multiples
        /// of <c>pi/2</c>.
        /// </summary>
        private static readonly double[] Points = { 0.35, 0.61, 0.9, 1.15, 1.35 };

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
        /// <c>tan^m sec^n</c>. An odd <c>m</c> goes by <c>u = cos</c> and an even one with
        /// <c>n</c> even by <c>u = tan</c>, so both branches are covered here rather than only
        /// the textbook case.
        /// </summary>
        [Theory]
        [InlineData("tan(x)^3*sec(x)^4")]
        [InlineData("tan(x)^2*sec(x)^4")]
        [InlineData("tan(x)*sec(x)^3")]
        [InlineData("tan(x)^5*sec(x)^2")]
        [InlineData("tan(x)^3*sec(x)^5")]
        [InlineData("sec(x)^6*tan(x)^3")]
        [InlineData("sec(x)^4*tan(x)^2")]
        public void TheTangentTimesTheSecant(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// <c>cot^m csc^n</c>, which is a separate set of nodes here and so separate coverage
        /// rather than a symmetry anyone may assume.
        /// </summary>
        [Theory]
        [InlineData("cot(x)^3*csc(x)^4")]
        [InlineData("cot(x)^2*csc(x)^4")]
        [InlineData("csc(x)^3*cot(x)")]
        [InlineData("cot(x)^5*csc(x)^2")]
        public void TheCotangentTimesTheCosecant(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// The same integrands spelled in the sine and the cosine, including as quotients. One
        /// expression written two ways has to get one verdict, which is the defect this file's
        /// neighbours have carried repeatedly.
        /// </summary>
        [Theory]
        [InlineData("sin(x)^2/cos(x)^4")]
        [InlineData("sin(x)^3/cos(x)^5")]
        [InlineData("sin(x)^3/cos(x)^7")]
        [InlineData("cos(x)^3/sin(x)^7")]
        [InlineData("sin(x)/cos(x)^3")]
        [InlineData("1/(sin(x)^2*cos(x)^2)")]
        [InlineData("1/(sin(x)*cos(x))")]
        public void TheSameThingInSineAndCosine(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// Positive powers of both, where the answer is a polynomial in whichever of the two the
        /// odd exponent picks out.
        /// </summary>
        [Theory]
        [InlineData("sin(x)^3*cos(x)^2")]
        [InlineData("sin(x)^5*cos(x)^4")]
        [InlineData("sin(x)^3*cos(x)^3")]
        [InlineData("sin(x)^3")]
        [InlineData("cos(x)^5")]
        public void PositivePowersOfBoth(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// A linear argument, which divides the whole answer by the rate. The constant term is
        /// there because a rule reading only the coefficient gets <c>3x + 1</c> right by accident
        /// and <c>3x</c> wrong in the same way.
        /// </summary>
        [Theory]
        [InlineData("tan(2*x)^3*sec(2*x)^4")]
        [InlineData("cot(3*x + 1)^3*csc(3*x + 1)^4")]
        [InlineData("sin(2*x)^3*cos(2*x)^2")]
        public void ALinearArgument(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// The neighbours, each answered without this rule and each a chance for it to take an
        /// integrand it should leave alone. Both exponents even and non-negative is deliberately
        /// outside it — the tangent substitution leaves a negative power of <c>1 + u^2</c> there
        /// rather than a polynomial — and the power reductions answer those.
        /// </summary>
        [Theory]
        [InlineData("sin(x)^2*cos(x)^4")]
        [InlineData("sin(x)^4*cos(x)^2")]
        [InlineData("sin(x)^2")]
        [InlineData("cos(x)^6")]
        [InlineData("sec(x)^4")]
        [InlineData("csc(x)^6")]
        [InlineData("sec(x)^3")]
        [InlineData("tan(x)^2")]
        [InlineData("cot(x)^4")]
        [InlineData("sin(x)*cos(2*x)")]
        [InlineData("x*sin(x)")]
        public void TheNeighboursAreUntouched(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// Integrands the read must refuse: two different arguments, a non-integer power, and a
        /// factor that is not trigonometric at all. A rule that guessed at the part it did not
        /// recognise would answer a question it was not asked, so these must come back unanswered
        /// or answered by something else — never wrong.
        /// </summary>
        [Theory]
        [InlineData("sin(x)^3*cos(2*x)^2")]
        [InlineData("sin(x)^(1/2)*cos(x)^3")]
        [InlineData("x*tan(x)^3*sec(x)^4")]
        [InlineData("a*sin(x)^3*cos(x)^2")]
        public void WhatTheReadRefuses(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("NaN", integral.Stringize());
            if (integral.Stringize().Contains("integral("))
                return;   // unanswered is a legitimate verdict; a wrong answer is not
            // Answered by some other rule, which is fine — but it has to be right.
            var derivative = integral.Substitute("C", 0).Differentiate("x").Substitute("a", 3);
            var original = integrand.ToEntity().Substitute("a", 3);
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
    }
}
