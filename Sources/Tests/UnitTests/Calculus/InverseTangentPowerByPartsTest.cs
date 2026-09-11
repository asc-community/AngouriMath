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
    /// A power of an inverse tangent times a rational function, by two rounds of parts: the
    /// descent measure of integration by parts now counts the power of the factor it
    /// differentiates as well as the size of what is left.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>x arctan(x)^2</c> leaves <c>x^2 arctan(x)/(1 + x^2)</c> after one step, which has more
    /// nodes and needs one more step to finish; on nodes alone the second step was refused. The
    /// power fell from two to one, and that is the descent the step made. The measure is the
    /// pair, power first, and a remainder with no such factor left is still measured by nodes
    /// alone — which is what keeps <c>asec(x)^2</c> times a radical from opening a search on a
    /// hundred-node remainder, measured at thirty seconds when it did.
    /// </para>
    /// <para>
    /// The remainder is also re-spelled as a product cut at the factor, where what is beside it
    /// is rational, since <c>Simplify</c> writes it as a quotient and by parts runs on a product.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class InverseTangentPowerByPartsTest
    {
        private static readonly double[] Points = { 0.3, 0.9, 1.7, 2.6 };

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

        /// <summary>
        /// Apostol's <c>x arctan(x)^2</c> and Timofeev's <c>arctan(x)^2/x^3</c>, with the
        /// logarithm's square beside them, which came out before and must keep coming out.
        /// </summary>
        [Theory]
        [InlineData("x*atan(x)^2")]
        [InlineData("atan(x)^2/x^3")]
        [InlineData("x^3*atan(x)^2")]
        [InlineData("x*ln(x)^2")]
        [InlineData("x*ln(x)^3")]
        [InlineData("x^2*ln(x)^2")]
        public void APowerOfTheFactorTimesARationalFunction(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// <c>arctan(x)^2/x^2</c> leaves <c>arctan(x)/(x (1 + x^2))</c>, which has no elementary
        /// antiderivative, and the descent has to stop there rather than search: bounded by the
        /// clock, since the measure is what keeps a decline quick.
        /// </summary>
        [Fact]
        public void ANonElementaryRemainderIsDeclinedQuickly()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var integral = "atan(x)^2/x^2".ToEntity().Integrate("x");
            Assert.Contains("integral(", integral.Stringize());
            Assert.True(watch.Elapsed < TimeSpan.FromSeconds(20), $"declined in {watch.Elapsed}");
        }

        /// <summary>
        /// A radical beside the power is not re-spelled: the remainder is a hundred-node radical
        /// expression that a by-parts search took thirty seconds to decline, and is declined in
        /// about one without the re-spelling.
        /// </summary>
        [Fact]
        public void ARadicalRemainderIsNotSearched()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            _ = "(x^2 - 1)^(3/2)*asec(x)^2/x^5".ToEntity().Integrate("x");
            Assert.True(watch.Elapsed < TimeSpan.FromSeconds(20), $"took {watch.Elapsed}");
        }
    }
}
