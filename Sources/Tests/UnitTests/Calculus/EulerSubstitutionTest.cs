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
    /// A rational function of <c>x</c> and one square root of a quadratic, rationalised by
    /// an Euler substitution and finished by the rational integrator.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// The trigonometric substitution answers <c>x^m sqrt(a + b x^2)^k</c> and nothing wider.
    /// <c>sqrt(2 - x - x^2)/x^2</c>, <c>1/((4 + x^2) sqrt(1 + 4x^2))</c> and
    /// <c>x sqrt(2 r x - x^2)</c> had no antiderivative, and neither did anything another rule
    /// reduces to a rational function of <c>x</c> and one such root. Each of Euler's three
    /// substitutions makes <c>x</c>, the root and <c>dx</c> rational in <c>t</c>; the rational
    /// function goes to the rational integrator directly, and the answer comes back through
    /// <c>t = sqrt(Q) + sqrt(a) x</c> or its kin.
    /// </para>
    /// <para>
    /// Every answer is differentiated back at points where the root is real. The one input that
    /// used to throw — a complex constant under the root — and the one that used to answer
    /// <c>NaN</c> are pinned as declined.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class EulerSubstitutionTest
    {
        private static void DifferentiatesBack(string integrand, double[] points, params (string, double)[] pins)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Assert.DoesNotContain("NaN", integral.Stringize());

            var derivative = integral.Substitute("C", 0).Differentiate("x");
            Entity original = integrand.ToEntity();
            foreach (var (name, value) in pins)
            {
                derivative = derivative.Substitute(name, value);
                original = original.Substitute(name, value);
            }

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

        /// <summary>
        /// The first substitution, <c>a &gt; 0</c>: Timofeev's <c>1/((4 + x^2) sqrt(1 + 4x^2))</c>
        /// and its neighbours.
        /// </summary>
        [Theory]
        [InlineData("1/((4 + x^2)*sqrt(1 + 4*x^2))", new[] { 0.3, 0.9, 1.7, 2.6 })]
        [InlineData("sqrt(1 + x^2)/(1 + x)", new[] { 0.3, 0.9, 1.7, 2.6 })]
        [InlineData("1/(x*sqrt(x^2 + x + 1))", new[] { 0.3, 0.9, 1.7, 2.6 })]
        [InlineData("x/(1 + sqrt(1 + x^2))", new[] { 0.3, 0.9, 1.7, 2.6 })]
        [InlineData("1/((x - 1)*sqrt(x^2 - 2*x + 5))", new[] { 1.3, 1.9, 2.7, 3.6 })]
        public void ThePositiveLeadingCoefficient(string integrand, double[] points) => DifferentiatesBack(integrand, points);

        /// <summary>
        /// The second, <c>c &gt; 0</c>: Apostol's <c>sqrt(2 - x - x^2)/x^2</c>, and the input that
        /// once made a rational reader divide by zero.
        /// </summary>
        [Theory]
        [InlineData("sqrt(2 - x - x^2)/x^2", new[] { 0.2, 0.4, 0.6, 0.8 })]
        [InlineData("1/(1 + sqrt(1 - x^2))", new[] { 0.2, 0.4, 0.6, 0.8 })]
        public void ThePositiveConstant(string integrand, double[] points) => DifferentiatesBack(integrand, points);

        /// <summary>
        /// The third, at the root the quadratic has at zero: Timofeev's <c>x sqrt(2 r x - x^2)</c>,
        /// with <c>r</c> a symbol.
        /// </summary>
        [Theory]
        [InlineData("x*sqrt(2*r*x - x^2)", new[] { 0.3, 0.9, 1.7, 2.6 })]
        [InlineData("sqrt(x^2 + 2*x)/x", new[] { 0.3, 0.9, 1.7, 2.6 })]
        public void TheRootAtZero(string integrand, double[] points) => DifferentiatesBack(integrand, points, ("r", 1.7));

        /// <summary>
        /// Reached one level down, which is why the rule is not scoped: a nested radical under
        /// <c>u = sqrt(1 + x)</c>, and the remainders of by parts against an inverse function.
        /// </summary>
        [Theory]
        [InlineData("sqrt(x + sqrt(1 + x))/x^2", new[] { 0.4, 0.9, 1.7, 2.6 })]
        [InlineData("x*atan(x)/sqrt(1 - x^2)", new[] { 0.2, 0.4, 0.6, 0.8 })]
        [InlineData("atan(x)/(x^2*sqrt(1 - x^2))", new[] { 0.2, 0.4, 0.6, 0.8 })]
        [InlineData("tanh(x)/sqrt(exp(x) + exp(2*x))", new[] { 0.3, 0.9, 1.7, 2.6 })]
        public void ReachedOneLevelDown(string integrand, double[] points) => DifferentiatesBack(integrand, points);

        /// <summary>
        /// A complex constant under the root is none of the three substitutions', and taking
        /// <c>-i</c> for a symbol once answered <c>NaN</c>; a rational function of the root
        /// whose Euler form is past the degree the splits can factor is declined too, and not
        /// slowly.
        /// </summary>
        [Theory]
        [InlineData("1/((1 + x)^2*sqrt(2)*sqrt(-i + x^2))")]
        [InlineData("ln(x^2 + sqrt(1 - x^2))")]
        public void DeclinedRatherThanWrongOrSlow(string integrand)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var integral = integrand.ToEntity().Integrate("x");
            Assert.True(watch.Elapsed < TimeSpan.FromSeconds(20), $"took {watch.Elapsed}");
            Assert.DoesNotContain("NaN", integral.Stringize());
        }

        /// <summary>
        /// The shapes the rule leaves to the rules that answer them more shortly keep their
        /// answers: a power of the variable times a root with no linear term, and a root of
        /// something linear.
        /// </summary>
        [Theory]
        [InlineData("x^3/sqrt(1 + x^2)", "-((1 / sqrt(1 + x ^ 2)) ^ (-3) / (-3) + -1 / (1 / sqrt(1 + x ^ 2)) / (-1)) + C")]
        [InlineData("sqrt(1 + 2*x)", "(1 + 2 * x) ^ (3/2) / (3/2) / 2 + C")]
        public void TheNeighboursKeepTheirForms(string integrand, string expected)
            => Assert.Equal(expected, integrand.ToEntity().Integrate("x").Stringize());
    }
}
