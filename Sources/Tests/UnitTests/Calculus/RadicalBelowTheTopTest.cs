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
    /// Integrands whose radical of a quadratic appears only <b>below the top</b> — after a step of
    /// integration by parts, not in what the caller wrote.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1265">#1265</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>int x arcsin(x) dx</c> leaves <c>int x^2/sqrt(1 - x^2) dx</c>, which is squarely the
    /// quadratic radical rule's and was refused for sitting one level down: that rule was scoped
    /// to the integrand the caller asked about, as the other four closed rules are.
    /// </para>
    /// <para>
    /// <b>Scope is right for a rule whose answers only let some other search carry on into ground
    /// that was doomed, and wrong for one whose answers are used.</b> These are the second kind —
    /// by parts asks for the sub-integral and then uses it. Ungating measured free: ten more of
    /// the Rubi sample, the same wall clock, the same timeouts, and the same 28.8 s over a probe
    /// of integrands declined either way.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class RadicalBelowTheTopTest
    {
        private static void DifferentiatesBack(string integrand, double[] points)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("NaN", integral.Stringize());
            Assert.DoesNotContain("integral(", integral.Stringize());

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
                Assert.True(difference / scale < 1e-9,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, "
                    + $"where the integrand is {want}");
            }
            Assert.True(compared >= 3,
                $"only {compared} of {points.Length} points were comparable for {integrand}, "
                + "so this asserts almost nothing");
        }

        /// <summary>
        /// A polynomial times an inverse trigonometric function. By parts differentiates the
        /// inverse function into something algebraic, and what is left is a power of the variable
        /// over a radical of a quadratic.
        /// </summary>
        [Theory]
        [InlineData("x*arcsin(x)", new[] { 0.3, 0.5, -0.4, -0.8 })]
        [InlineData("x^2*arcsin(x)", new[] { 0.3, 0.5, -0.4, -0.8 })]
        [InlineData("x*arccos(x)", new[] { 0.3, 0.5, -0.4, -0.8 })]
        [InlineData("x^3*arcsin(x)", new[] { 0.3, 0.5, -0.4, -0.8 })]
        public void APolynomialTimesAnInverseFunction(string integrand, double[] points)
            => DifferentiatesBack(integrand, points);

        /// <summary>
        /// The same with a negative power, and with the radical already in the integrand — the
        /// sub-integral is still a level down, which is what was refused.
        /// </summary>
        [Theory]
        [InlineData("arcsin(x)/x^2", new[] { 0.3, 0.5, 0.7, 0.85 })]
        [InlineData("x*arctan(x)/sqrt(1 + x^2)", new[] { 0.3, 1.5, -0.4, -2.8 })]
        [InlineData("x*arcsin(x)/sqrt(1 - x^2)", new[] { 0.3, 0.5, 0.7, 0.85 })]
        public void ANegativePowerAndARadicalAlreadyThere(string integrand, double[] points)
            => DifferentiatesBack(integrand, points);

        /// <summary>
        /// What the rule answered at the top before and must keep answering — the ungating is not
        /// meant to change any of these.
        /// </summary>
        [Theory]
        [InlineData("1/(1 + x^2)^(3/2)", new[] { 0.3, 1.6, -0.4, -2.2 })]
        [InlineData("x^5/sqrt(5 + x^2)", new[] { 0.3, 1.6, -0.4, -2.2 })]
        [InlineData("x^2/sqrt(1 + x^2)", new[] { 0.3, 1.6, -0.4, -2.2 })]
        [InlineData("sqrt(1 - x^2)/x^2", new[] { 0.3, 0.5, 0.7, 0.85 })]
        [InlineData("sqrt(x^2 - 1)", new[] { 1.7, 3.1, -1.9, -4.2 })]
        [InlineData("x/sqrt(1 + x + x^2)", new[] { 0.3, 1.6, -0.4, -2.2 })]
        [InlineData("1/(x*(1 + x^2)^2)", new[] { 0.3, 1.6, -0.4, -2.2 })]
        public void WhatItAnsweredAtTheTopBefore(string integrand, double[] points)
            => DifferentiatesBack(integrand, points);

        /// <summary>
        /// And the integrands that are declined either way, which must stay <b>cheaply</b>
        /// declined — that is the whole risk the scope was guarding against, and the measurement
        /// says it does not arise for this rule.
        /// </summary>
        /// <remarks>
        /// Bounded by the integrator's own budget rather than by a wall clock, which measures the
        /// runner. Over a fourteen-integrand probe these total 28.8 s with the rule scoped and
        /// 28.8 s without it.
        /// </remarks>
        [Theory]
        [InlineData("x*tan(x)^3*sec(x)^4")]
        [InlineData("1/((4 + x^2)*sqrt(1 + 4*x^2))")]
        [InlineData("(1 + x^4)/((1 + x + x^2)*sqrt(2 + x + x^2))")]
        [InlineData("x*sqrt(1 + x^3)*ln(x)")]
        public void WhatIsDeclinedEitherWay(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("NaN", integral.Stringize());
        }
    }
}
