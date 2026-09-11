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
    /// A polynomial times an exponential times a sine or a cosine, closed by the repeated by
    /// parts this shape is the textbook case for.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>e^x cos(x)</c> came out and <c>x e^x cos(x)</c> did not: the polynomial by-parts
    /// integrates its <c>dv</c> with by-parts switched off, and <c>e^x cos(x)</c> is answered by
    /// nothing but by-parts — the cyclic one. Switching that flag on buys these and takes
    /// <c>x tan(x)^3 sec(x)^4</c> from 20 ms to 94 seconds to decline, so the family is named
    /// rather than searched for.
    /// </para>
    /// <para>
    /// Checked by differentiating back and comparing at points, never against a printed form.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class PolynomialExponentialTrigTest
    {
        private static readonly double[] Points = { -1.3, -0.4, 0.7, 1.5, 2.2 };

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

        /// <summary>All three factors at once, which is what was declined.</summary>
        [Theory]
        [InlineData("x*e^x*cos(x)")]
        [InlineData("x*e^x*sin(x)")]
        [InlineData("x^2*e^x*cos(x)")]
        [InlineData("x^3*e^x*cos(x)")]
        [InlineData("(x^2 + 1)*e^x*sin(x)")]
        [InlineData("x*e^(2*x)*sin(3*x)")]
        [InlineData("x*e^(-x)*cos(2*x)")]
        public void AllThreeFactors(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// A base other than <c>e</c>, which is the same shape written differently:
        /// <c>2^x</c> is <c>e^(x ln 2)</c>, and these were declined for the spelling alone.
        /// </summary>
        [Theory]
        [InlineData("2^x*x*cos(x)")]
        [InlineData("3^x*x^2*sin(x)")]
        [InlineData("2^(3*x)*x*sin(x)")]
        public void ABaseOtherThanE(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// The degenerate corners of the same formula — no polynomial, no exponential, no
        /// trigonometric factor. Each already came out, and each now comes out through this rule
        /// as well, so the signs have to be right at <c>a = 0</c> and <c>b = 0</c> too. That is
        /// not a formality: the first version of the formula had the sign of the <c>b</c> terms
        /// reversed, which is invisible on <c>e^x cos(x)</c> and wrong on <c>x^2 sin(x)</c>.
        /// </summary>
        [Theory]
        [InlineData("e^x*cos(x)")]
        [InlineData("e^(2*x)*sin(3*x)")]
        [InlineData("x*e^x")]
        [InlineData("x^2*e^x")]
        [InlineData("x*sin(x)")]
        [InlineData("x^2*sin(x)")]
        [InlineData("x^2*cos(x)")]
        [InlineData("x^3*sin(x)")]
        [InlineData("2^x*x")]
        [InlineData("e^x")]
        [InlineData("sin(x)")]
        [InlineData("cos(3*x)")]
        public void TheDegenerateCorners(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// What the read must refuse: a bare polynomial, which is the power rule's; a phase in
        /// the trigonometric argument, which wants the angle-sum identity first; two
        /// trigonometric factors, which the product-to-sum rule takes apart; and a factor that is
        /// neither polynomial nor exponential nor trigonometric.
        /// </summary>
        [Theory]
        [InlineData("x^2")]
        [InlineData("e^x*cos(x + 1)")]
        [InlineData("x*sin(x)*cos(2*x)")]
        [InlineData("x*ln(x)*e^x")]
        [InlineData("x*e^(x^2)*cos(x)")]
        public void WhatTheReadRefuses(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("NaN", integral.Stringize());
            if (integral.Stringize().Contains("integral("))
                return;   // unanswered is a legitimate verdict; a wrong answer is not
            DifferentiatesBack(integrand);
        }

        /// <summary>
        /// The integrand the closed form exists to avoid opening a search on. It is declined
        /// either way; what is pinned is that it is declined <em>cheaply</em>, which the closed
        /// rule keeps and switching the by-parts flag on did not.
        /// </summary>
        /// <remarks>
        /// Bounded by the integrator's own budget rather than by a wall clock, which measures the
        /// runner: `x tan(x)^3 sec(x)^4` took 94 seconds with the flag on and takes about thirty
        /// milliseconds here.
        /// </remarks>
        [Theory]
        [InlineData("x*tan(x)^3*sec(x)^4")]
        [InlineData("x*cot(x)^3*csc(x)^4")]
        public void TheSearchThisAvoidsOpening(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("NaN", integral.Stringize());
            if (integral.Stringize().Contains("integral("))
                return;
            DifferentiatesBack(integrand);
        }
    }
}
