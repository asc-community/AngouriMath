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
    /// Integrands that are real where a radicand is negative, as a product of two imaginary
    /// factors, integrated through the root of that radicand taken as a non-negative real. The
    /// answers said <c>provided</c> the radicand is not negative, and so said nothing on part of
    /// the line where the integrand is real; they hold there as they stand, and the condition is
    /// now the integrand's real domain or none.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    /// <remarks>
    /// A point where the integrand is real and the answer has no value is a failure here, not a
    /// point skipped: skipping it is how a condition false on half the line passed for an answer.
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class AnAnswerBeyondANegativeRadicandIntegralTest
    {
        private static void HoldsWhereTheIntegrandIsReal(string integrand, Func<Entity, Entity> pinned, double[] points)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.DoesNotContain("NaN", text);
            var derivative = pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = pinned(integrand.ToEntity());
            foreach (var at in points)
            {
                var want = original.Substitute("x", at).EvalNumerical();
                Assert.False(want.IsNaN, $"{integrand} has no value at x = {at}");
                Assert.True(Math.Abs((double)want.ImaginaryPart) < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart)),
                    $"{integrand} is {want} at x = {at}, which is not a point where it is real");
                var got = derivative.Substitute("x", at).EvalNumerical();
                Assert.False(got.IsNaN, $"the antiderivative of {integrand} has no value at x = {at}, where the integrand is {want}");
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
        }

        /// <summary>
        /// Timofeev's arcsecants by parts: the remainder is answered in <c>w = x^2</c> through
        /// <c>sqrt(w - 1)</c>, and the integrands are real for <c>x &lt; -1</c>, for <c>x &gt; 1</c> and,
        /// as the arcsecant and the root are both imaginary, for <c>0 &lt; x &lt; 1</c>. Two points on
        /// each of the three intervals.
        /// </summary>
        [Theory]
        [InlineData("asec(x)*sqrt(-1 + x^2)/x^4")]
        [InlineData("asec(x)/(-1 + x^2)^(5/2)")]
        public void AnArcsecantOnAllThreeIntervals(string integrand)
            => HoldsWhereTheIntegrandIsReal(integrand, e => e, new[] { -2.7, -1.3, 0.23, 0.59, 1.3, 2.6 });

        /// <summary>
        /// Rubi's 4.3.2.1 and 4.3.3.1, a root of the tangent beside a root of <c>a + b tan(c + d x)</c>, through
        /// <c>u = sqrt(tan(c + d x))</c>: real where the tangent is positive and where it is below
        /// <c>-a/b</c>, two imaginary roots there. With <c>a = 1.3</c>, <c>b = 0.7</c>,
        /// <c>c = 0.4</c>, <c>d = 0.9</c>, the tangent is positive for <c>-0.44 &lt; x &lt; 1.30</c> and
        /// below <c>-a/b</c> for <c>1.30 &lt; x &lt; 1.85</c> and again a period on.
        /// </summary>
        [Theory]
        [InlineData("sqrt(cot(c + d*x))*sqrt(a + b*tan(c + d*x))")]
        [InlineData("1/(sqrt(cot(c + d*x))*(a + b*tan(c + d*x))^(3/2))")]
        [InlineData("sqrt(a + b*tan(c + d*x))*(A + B*tan(c + d*x))/tan(c + d*x)^(3/2)")]
        [InlineData("sqrt(cot(c + d*x))*(a + b*tan(c + d*x))^(5/2)*(A + B*tan(c + d*x))")]
        [InlineData("sqrt(cot(c + d*x))*(A + B*tan(c + d*x))/sqrt(a + b*tan(c + d*x))")]
        [InlineData("(A + B*tan(c + d*x))/(sqrt(cot(c + d*x))*(a + b*tan(c + d*x))^(3/2))")]
        public void ARootOfTheTangentBesideARootOfALinearInIt(string integrand)
            => HoldsWhereTheIntegrandIsReal(integrand,
                e => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 0.4).Substitute("d", 0.9)
                      .Substitute("A", 1.1).Substitute("B", 0.6),
                new[] { 0.3, 0.9, 1.45, 1.7, 5.0 });

        /// <summary>
        /// Where the integrand is imaginary the answer still claims nothing: between <c>-a/b</c>
        /// and 0 the tangent's root and the linear's are not both imaginary, and there the answer
        /// through a real-or-imaginary <c>u</c> is not the integrand's.
        /// </summary>
        [Fact]
        public void NothingIsClaimedWhereTheIntegrandIsImaginary()
        {
            var integral = "sqrt(a + b*tan(c + d*x))*(A + B*tan(c + d*x))/tan(c + d*x)^(3/2)".ToEntity().Integrate("x");
            var pinned = integral.Substitute("C", 0).Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 0.4)
                .Substitute("d", 0.9).Substitute("A", 1.1).Substitute("B", 0.6);
            Assert.True(pinned.Substitute("x", 2.3).EvalNumerical().IsNaN);
        }
    }
}
