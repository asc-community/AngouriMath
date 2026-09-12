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
    /// A radical over a sum that holds another radical over something linear —
    /// <c>sqrt(x + sqrt(1 + x))</c> and its family.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// The linear-radical substitution was <b>refusing</b> an integrand that held a radical
    /// over something not linear, where it need only have skipped it: with <c>u^2 = 1 + x</c>
    /// the outer radical becomes <c>sqrt(u^2 + u - 1)</c>, free of <c>x</c>, and what is left is
    /// a shape the quadratic-radical rule answers. The check that no <c>x</c> survives the
    /// substitution is what made skipping safe.
    /// </para>
    /// <para>
    /// Two of these are checked by <b>differences of the antiderivative against a quadrature</b>
    /// rather than by differentiating back. Their answers come through the secant branch of the
    /// quadratic-radical rule and carry a <c>sign</c> for the second interval, and the library
    /// does not differentiate <c>sign</c>; a numeric check of <c>F(b) - F(a)</c> is the one this
    /// answer admits. Simpson's rule on a smooth integrand over a short interval is accurate
    /// well past the tolerance used.
    /// </para>
    /// <para>
    /// The two-pass back-substitution this also fixes was a latent defect in the
    /// quadratic-radical rule: <c>Replace</c> rewrites children before parents, so a bare
    /// <c>t</c> was substituted before its enclosing <c>tan(t)</c> could match, and the answer
    /// held <c>tan(arccos(...))</c> — correct and not evaluable.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class NestedRadicalIntegralTest
    {
        private static void AgreesWithQuadrature(string integrand, double from, double to)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("NaN", integral.Stringize());
            Assert.DoesNotContain("integral(", integral.Stringize());

            var antiderivative = integral.Substitute("C", 0);
            var closed = (double)(antiderivative.Substitute("x", to).EvalNumerical()
                                  - antiderivative.Substitute("x", from).EvalNumerical()).RealPart;

            var f = integrand.ToEntity();
            const int intervals = 400;
            var h = (to - from) / intervals;
            var sum = 0.0;
            for (var i = 0; i <= intervals; i++)
            {
                var weight = i == 0 || i == intervals ? 1 : i % 2 == 1 ? 4 : 2;
                sum += weight * (double)f.Substitute("x", from + i * h).EvalNumerical().RealPart;
            }
            var numeric = sum * h / 3;

            Assert.True(Math.Abs(closed - numeric) < 1e-7 * Math.Max(1, Math.Abs(numeric)),
                $"F({to}) - F({from}) is {closed} for {integrand}, where the quadrature says {numeric}");
        }

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
                $"only {compared} of {points.Length} points were comparable for {integrand}");
        }

        /// <summary>
        /// The case this was found on, and two neighbours with a different inner slope and a
        /// different sign on the inner radical.
        /// </summary>
        [Theory]
        [InlineData("sqrt(x + sqrt(1 + x))", 0.4, 2.7)]
        [InlineData("sqrt(x + sqrt(1 + x))", 1.1, 5.0)]
        [InlineData("sqrt(x + sqrt(2*x + 3))", 0.5, 3.0)]
        [InlineData("sqrt(2*x - sqrt(x + 1))", 1.0, 4.0)]
        public void ARadicalOverASumHoldingALinearRadical(string integrand, double from, double to)
            => AgreesWithQuadrature(integrand, from, to);

        /// <summary>
        /// The same skip lets the substitution through where the outer shape is rational after
        /// it: the nested radical in a denominator.
        /// </summary>
        [Theory]
        [InlineData("1/(x - sqrt(1 + sqrt(1 + x)))", new[] { 0.3, 0.9, 1.7 })]
        [InlineData("sqrt(1 + x)/(x + sqrt(1 + sqrt(1 + x)))", new[] { 0.3, 1.3, 2.7 })]
        public void ANestedRadicalInADenominator(string integrand, double[] points)
            => DifferentiatesBack(integrand, points);

        /// <summary>
        /// The same shape in <c>1/x</c>: Bondarenko's <c>sqrt(1/x + sqrt(1 + 1/x))</c> is
        /// <c>sqrt(u + sqrt(1 + u))</c> over <c>-u^2</c> under <c>u = 1/x</c>, and <c>1/x</c> is
        /// written as a quotient that no power candidate of the substitution rule read. The
        /// reciprocal is offered only where it sits under a root: offered for every <c>/x</c>
        /// it opened searches on <c>(x^2 - 10)^(5/2)/x</c> and <c>x ln(x)/sqrt(1 + x^2)</c> that
        /// did not return, and those two are pinned here as still quick.
        /// </summary>
        [Theory]
        [InlineData("sqrt(1/x + sqrt(1 + 1/x))", new[] { 0.3, 0.9, 1.7 })]
        [InlineData("sqrt(1 + 1/x)/x^2", new[] { 0.3, 0.9, 1.7 })]
        [InlineData("(x^2 - 10)^(5/2)/x", new[] { 3.3, 3.9, 4.7 })]
        [InlineData("x*ln(x)/sqrt(1 + x^2)", new[] { 0.3, 0.9, 1.7 })]
        public void TheReciprocalUnderARoot(string integrand, double[] points)
            => DifferentiatesBack(integrand, points);

        /// <summary>
        /// What the substitution answered before, and must keep answering: one radical over a
        /// linear base, and a radical over a radical of the variable itself.
        /// </summary>
        [Theory]
        [InlineData("sqrt(1 + sqrt(x))", new[] { 0.3, 1.3, 2.7 })]
        [InlineData("x*sqrt(1 + sqrt(x))", new[] { 0.3, 1.3, 2.7 })]
        [InlineData("x*sqrt(1 + x)", new[] { 0.3, 1.3, 2.7 })]
        [InlineData("1/(1 + sqrt(1 + x))", new[] { 0.3, 1.3, 2.7 })]
        public void TheNeighboursStillAnswer(string integrand, double[] points)
            => DifferentiatesBack(integrand, points);

        /// <summary>
        /// The answer must be one the caller can evaluate — no <c>tan(arccos(...))</c> and no
        /// <c>t</c> left from the substitution — which is what the two-pass back-substitution is
        /// for. Evaluating at a point is the test of that.
        /// </summary>
        [Theory]
        [InlineData("sqrt(x + sqrt(1 + x))")]
        [InlineData("x^5/sqrt(5 + x^2)")]
        [InlineData("sqrt(x^2 - 1)")]
        public void TheAnswerEvaluates(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x").Substitute("C", 0);
            Assert.DoesNotContain("integral(", integral.Stringize());
            var value = integral.Substitute("x", 2.5).EvalNumerical();
            Assert.False(value.IsNaN, $"the antiderivative of {integrand} is NaN at x = 2.5: {integral}");
        }
    }
}
