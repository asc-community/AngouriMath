//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Linq;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// A rational function of <c>x</c> and one square root of a cubic or a quartic,
    /// integrated by the ansatz over logarithms of <c>A - B y</c> and arctangents of
    /// <c>A/(B y)</c> whose <c>A/B</c> is a Padé approximant of a branch of <c>y</c> at a pole.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// The curve is elliptic, so no substitution rationalises it, and the ones with an
    /// elementary antiderivative are the pseudo-elliptic integrals: Welz's
    /// <c>(1 + x)/((x - 2) sqrt(1 + x^3))</c> is a logarithm of
    /// <c>(1 + x)^2/3 - sqrt(1 + x^3)</c>, the Taylor polynomial of the root at the pole,
    /// and Bronstein's <c>x/sqrt(x^4 + 10x^2 - 96x - 71)</c> a logarithm of
    /// <c>A - B y</c> with <c>A</c> of degree eight and <c>B</c> of degree six, matched at
    /// infinity, whose norm <c>A^2 - B^2 P</c> is a constant.
    /// </para>
    /// <para>
    /// Every answer is differentiated back at points where the radicand is positive.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class SquareRootLogarithmAnsatzTest
    {
        private static void DifferentiatesBack(string integrand, double[] points)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Assert.DoesNotContain("NaN", integral.Stringize());

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
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart));
                Assert.True(difference / scale < 1e-8,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, "
                    + $"where the integrand is {want}");
            }
            Assert.True(compared >= 5,
                $"only {compared} of {points.Length} points were comparable for {integrand}");
        }

        private static readonly double[] AboveMinusOne = { -0.7, -0.3, 0.2, 0.5, 0.8, 1.6, 2.4, 3.5 };

        /// <summary>
        /// Welz's, a logarithm at the pole <c>2</c> where <c>1 + x^3</c> is <c>9</c>:
        /// <c>(2/3) ln((1 + x)^2/3 - sqrt(1 + x^3)) - ln(x - 2) - ln(1 + x)/3</c>; the same with
        /// a rational part, <c>(2/3) sqrt(1 + x^3)</c>, brought over the denominator, and as a
        /// sum.
        /// </summary>
        [Theory]
        [InlineData("(1+x)/((x-2)*sqrt(1+x^3))")]
        [InlineData("(x^3-2*x^2+x+1)/((x-2)*sqrt(1+x^3))")]
        [InlineData("(1+x)/((x-2)*sqrt(1+x^3)) + x^2/sqrt(1+x^3)")]
        public void ALogarithmAtAPole(string integrand)
        {
            DifferentiatesBack(integrand, AboveMinusOne);
            var integral = integrand.ToEntity().Integrate("x").Stringize();
            Assert.Contains("ln(", integral);
            Assert.Contains("ln(x - 2)", integral);
        }

        /// <summary>
        /// Bronstein's, matched at infinity: the answer is
        /// <c>-ln(A - B sqrt(P))/8</c> with <c>A = x^8 + 20x^6 - 128x^5 + 54x^4 - 1408x^3 + 3124x^2 + 10001</c>
        /// and <c>B = x^6 + 15x^4 - 80x^3 + 27x^2 - 528x + 781</c>, and nothing else -- the
        /// norm is a constant, so no pole is left for a logarithm of a linear factor.
        /// </summary>
        [Fact]
        public void ALogarithmMatchedAtInfinity()
        {
            DifferentiatesBack("x/sqrt(x^4+10*x^2-96*x-71)", new[] { -4.0, -3.0, -2.0, -1.0, 5.0, 6.0, 7.0 });
            var integral = "x/sqrt(x^4+10*x^2-96*x-71)".ToEntity().Integrate("x").Stringize();
            Assert.Contains("10001", integral);
            Assert.Contains("781", integral);
            Assert.Equal(1, integral.Split("ln(").Length - 1);
        }

        /// <summary>
        /// Where the radicand is the negative of a square at the pole the branch is imaginary
        /// and the pair of logarithms is one arctangent: <c>1/((x + 1) sqrt(x^3 + 3x^2 + 3x))</c>
        /// is <c>-(2/3) arctan(1/sqrt(x^3 + 3x^2 + 3x))</c>.
        /// </summary>
        [Fact]
        public void AnArctangentAtAPole()
        {
            DifferentiatesBack("1/((x+1)*sqrt(x^3+3*x^2+3*x))", new[] { 0.2, 0.5, 0.8, 1.6, 2.4, 3.5 });
            var integral = "1/((x+1)*sqrt(x^3+3*x^2+3*x))".ToEntity().Integrate("x").Stringize();
            Assert.Contains("arctan(", integral);
            Assert.DoesNotContain("ln(", integral);
        }

        /// <summary>
        /// And what is elliptic is declined: <c>1/((x - 1) sqrt(x^3 + 1))</c>, where the
        /// radicand is <c>2</c> at the pole and no branch is rational, and
        /// <c>x/((x - 2) sqrt(1 + x^3))</c>, which is <c>1/sqrt(1 + x^3)</c> beside Welz's.
        /// </summary>
        [Theory]
        [InlineData("1/((x-1)*sqrt(x^3+1))")]
        [InlineData("x/((x-2)*sqrt(1+x^3))")]
        public void TheEllipticIsDeclined(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.Contains("integral(", integral.Stringize());
        }
    }
}
