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
    /// <c>(a + b sin(c x + d))^n</c> and the cosine, for <c>a^2 = b^2</c> and <c>n</c> a positive
    /// half-integer, closed by the reduction Rubi uses for the same case.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <c>sqrt(1 + sin(x))</c> had no antiderivative and is <c>-2 cos(x)/sqrt(1 + sin(x))</c>;
    /// the cancellation <c>cos^2 = (1 - sin)(1 + sin)</c> that checks it is what <c>a^2 = b^2</c>
    /// buys. Each answer here is differentiated back. The sample points avoid the zeros of
    /// <c>1 + sin</c>, where the integrand is zero and the comparison says nothing.
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class HalfPowerOfOnePlusASineTest
    {
        private static readonly double[] Points = { 0.3, 0.9, 1.7, 2.6, -0.4 };

        private static void DifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Assert.DoesNotContain("NaN", integral.Stringize());

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
                $"only {compared} of {Points.Length} points were comparable for {integrand}");
        }

        /// <summary>
        /// The base case and the reduction above it, for the sine and the cosine, both signs,
        /// a rate, an offset and a common factor. The second is Rubi's (Timofeev).
        /// </summary>
        [Theory]
        [InlineData("(1 + sin(x))^(1/2)")]
        [InlineData("(1 - sin(2/3*x))^(5/2)")]
        [InlineData("(1 + sin(x))^(3/2)")]
        [InlineData("(1 + cos(x))^(1/2)")]
        [InlineData("(1 - cos(3*x))^(3/2)")]
        [InlineData("(2 + 2*sin(x))^(1/2)")]
        [InlineData("(3 - 3*cos(x + 1))^(3/2)")]
        public void AHalfPowerWithEqualSquares(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// The base case's exact form, so that a later change to the reduction is visible.
        /// </summary>
        [Fact]
        public void TheBaseCaseIsTheClosedForm()
        {
            var integral = "(1 + sin(x))^(1/2)".ToEntity().Integrate("x");
            Assert.Equal("(-2) * cos(x) / sqrt(1 + sin(x)) + C", integral.Stringize());
        }

        /// <summary>
        /// <c>a^2 != b^2</c> is an elliptic integral, and a whole power is another rule's; both
        /// are declined here and the whole power is answered elsewhere.
        /// </summary>
        [Fact]
        public void UnequalSquaresAreDeclined()
            => Assert.Contains("integral(", "(1 + 2*sin(x))^(1/2)".ToEntity().Integrate("x").Stringize());

        [Fact]
        public void AWholePowerIsAnotherRules() => DifferentiatesBack("(1 + sin(x))^2");
    }
}
