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
    /// Products of sines and cosines of <b>different</b> arguments, which the product-to-sum
    /// identities turn into sums the integrator already reads.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// These are the shapes every substitution in the chain declines, and rightly: a product of
    /// trigonometric functions of unequal arguments is not a function of any one of them, so there
    /// is nothing to substitute for. The identity is the tool rather than a change of variable.
    /// </para>
    /// <para>
    /// Unlike every substitution here, the identities hold on the whole complex plane — both sides
    /// are entire — so these answers are antiderivatives wherever the integrand is defined, with
    /// no branch to pick and no interval to stay inside. The points below can therefore straddle
    /// zero and go negative, which the half-angle and radical tests cannot.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class ProductToSumIntegralTest
    {
        private static readonly double[] Points = { 0.31, 0.77, 1.42, 2.15, -0.6 };

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
                var difference = Math.Abs((double)(got - want).RealPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart));
                Assert.True(difference / scale < 1e-9,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, "
                    + $"where the integrand is {want}");
            }
            Assert.True(compared >= 4,
                $"only {compared} of {Points.Length} points were comparable for {integrand}, "
                + "so this asserts almost nothing");
        }

        /// <summary>Two factors, one of each of the three identities.</summary>
        [Theory]
        [InlineData("sin(x) * sin(2*x)")]
        [InlineData("cos(x) * cos(2*x)")]
        [InlineData("cos(3*x) * sin(2*x)")]
        [InlineData("sin(2*x) * cos(5*x)")]
        public void TwoFactorsOfDifferentArguments(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// Three factors, which needs the rewrite to apply to what the first application produced.
        /// </summary>
        [Theory]
        [InlineData("sin(x) * sin(2*x) * sin(3*x)")]
        [InlineData("cos(x) * cos(2*x) * cos(3*x)")]
        public void ThreeFactors(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// A polynomial factor alongside, so the sum the identity produces is then integrated by
        /// parts term by term.
        /// </summary>
        [Theory]
        [InlineData("x * sin(x) * cos(2*x)")]
        public void APolynomialFactorAlongside(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// Answered, but not by this rule, and the distinction is the point of the guard on equal
        /// arguments.
        /// </summary>
        /// <remarks>
        /// <c>sin(x)cos(x)</c> and <c>sin(x)^2</c> have one argument between them, so they are
        /// powers rather than products of two arguments and want a different tool — they come out
        /// as <c>sin(x)^2/2</c> and through the half-angle form respectively, both tidier than
        /// anything this rule would build. <c>sin(2)sin(x)</c> has a constant factor, and pairing
        /// a number with a real factor would turn one term into two for nothing.
        /// </remarks>
        [Theory]
        [InlineData("sin(x) * cos(x)")]
        [InlineData("sin(x)^2")]
        [InlineData("sin(2) * sin(x)")]
        public void EqualOrConstantArgumentsGoElsewhere(string integrand)
        {
            DifferentiatesBack(integrand);
            var integral = integrand.ToEntity().Integrate("x").Stringize();
            // What this rule builds always holds a sine or cosine of a sum or difference of the
            // two arguments; none of these should have gone that way.
            Assert.DoesNotContain("x - x", integral);
        }
    }
}
