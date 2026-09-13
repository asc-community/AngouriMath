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
    /// A bare logarithm or inverse function of something that is not linear, by parts against
    /// <c>1</c>: <c>int f(g) dx = x f(g) - int x g' f'(g) dx</c>.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// The by-parts rule runs on a product, and a single node is not one; a linear argument is
    /// the table's, and anything else reached no rule at all. <c>arctan(x sqrt(1 - x^2))</c>
    /// had no antiderivative and is one step of this. The remainder is algebraic and is
    /// answered where the radical rules reach it — three of Charlwood's and Bondarenko's do,
    /// and the rest of that family, whose remainders want an Euler substitution, decline in
    /// about a second and are pinned below as declined or correct.
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class ABareInverseFunctionByPartsTest
    {
        private static readonly double[] Points = { 0.2, 0.45, 0.7, 0.9 };

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
            Assert.True(compared >= 3,
                $"only {compared} of {Points.Length} points were comparable for {integrand}");
        }

        [Theory]
        [InlineData("atan(x*sqrt(1 - x^2))")]
        [InlineData("atan(x*sqrt(1 + x^2))")]
        [InlineData("ln(1/x^4 + x^4)")]
        [InlineData("ln(1 + x^2)")]
        public void AnInverseFunctionOfANonLinearArgument(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// The rest of Charlwood's family, whose remainders the radical rules do not reach, and
        /// <c>arcsin(x^2)</c>, whose remainder <c>2x^2/sqrt(1 - x^4)</c> is elliptic: declined or
        /// answered, never wrong, and not slowly.
        /// </summary>
        /// <summary>
        /// Charlwood's <c>arctan(x + sqrt(1 - x^2))</c>, whose remainder is a sum whose expanded
        /// terms carry a root over the same root -- cancelled as written now, before the
        /// chain's normalisation collects it into a zero power nothing reads -- and each of
        /// which is Euler's, one of them through the Rothstein-Trager resultant behind the
        /// splits. And its companion with the root below the bar.
        /// </summary>
        [Theory]
        [InlineData("atan(x + sqrt(1 - x^2))")]
        [InlineData("x*atan(x + sqrt(1 - x^2))/sqrt(1 - x^2)")]
        public void TheRemainderIsEulersThroughTheResultant(string integrand) => DifferentiatesBack(integrand);

        [Theory]
        [InlineData("asin(x^2)")]
        [InlineData("ln(1 + x*sqrt(1 + x^2))")]
        [InlineData("asin(x/sqrt(1 - x^2))")]
        [InlineData("asin(sqrt(1 + x) - sqrt(x))")]
        public void TheRemainderMayBeOutOfReach(string integrand)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var integral = integrand.ToEntity().Integrate("x");
            Assert.True(watch.Elapsed < TimeSpan.FromSeconds(20), $"took {watch.Elapsed}");
            if (!integral.Stringize().Contains("integral("))
                DifferentiatesBack(integrand);
        }
    }
}
