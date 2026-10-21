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
    /// <c>a + b/x^3</c> is real on both sides of 0, and taking <c>x^(-3/2)</c> out of its root, as
    /// <c>sqrt(a + b/x^3) = K x^(-3/2) sqrt(a x^3 + b)</c>, left an answer with no value at a negative
    /// x where the integrand is real: Rubi's 1.1.3.2 <c>x^m (a + b/x^3)^(k/2)</c>, fourteen of them. The
    /// substitution under the root answers on both sides.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class APowerOfXOutOfABinomialInAnOddNegativePowerIntegralTest
    {
        [Theory]
        [InlineData("sqrt(a+b/x^3)/x")]
        [InlineData("sqrt(a+b/x^3)/x^7")]
        [InlineData("(a+b/x^3)^(3/2)*x^2")]
        [InlineData("(a+b/x^3)^(3/2)/x^10")]
        [InlineData("x^5/sqrt(a+b/x^3)")]
        [InlineData("x^2/(a+b/x^3)^(3/2)")]
        public void DifferentiatesBackOnBothSidesOfZero(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            // a + b/x^3 is positive below the root of -b/a, about -0.81, and above 0.
            foreach (var at in new[] { -2.3, -1.7, -1.1, 0.4, 1.3, 2.2 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                Assert.False(got.IsNaN, $"d/dx of the antiderivative of {integrand} has no value at x = {at}, where the integrand is {want}");
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
        }

        /// <summary>
        /// And an odd power taken out where <c>j p</c> is whole, which is real on both sides: Rubi's
        /// 1.2.3.2 <c>(a x^3 + b x^6)^(5/3)</c> is <c>x^5</c> times a power of <c>a + b x^3</c>, and a
        /// guard on the odd power alone declined it.
        /// </summary>
        [Fact]
        public void TakesAnOddPowerOutWhereItsPowerIsWhole()
        {
            const string integrand = "(a*x^3+b*x^6)^(5/3)";
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            // a x^3 + b x^6 is positive below the root of -a/b, about -1.23, and above 0.
            foreach (var at in new[] { -2.3, -1.7, -1.4, 0.4, 1.3, 2.2 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                Assert.False(got.IsNaN, $"d/dx of the antiderivative of {integrand} has no value at x = {at}, where the integrand is {want}");
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
        }
    }
}
