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
    /// A polynomial times an odd half power of a linear over a second power or higher of
    /// <c>b x + c x^2</c>, under <c>t = sqrt(d + k x)</c> and split over the quadratic's two
    /// linears <c>x</c> and <c>b + c x</c>: the substitution reached the other way multiplies it
    /// out, and these ran past the budget or came back in up to 940,000 characters. Rubi's 1.2.1.2
    /// and 1.2.1.3. Points on both sides of the zeros where the root is imaginary, and in each
    /// interval between them where it is real.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AnOddHalfPowerOfALinearOverAPowerOfAQuadraticWithARootAtZeroIntegralTest
    {
        [Theory]
        [InlineData("(d + k*x)^(1/2)/(b*x + c*x^2)^3")]
        [InlineData("(d + k*x)^(5/2)/(b*x + c*x^2)^3")]
        [InlineData("(A + B*x)*(d + k*x)^(1/2)/(b*x + c*x^2)^3")]
        [InlineData("(A + B*x)*(d + k*x)^(7/2)/(b*x + c*x^2)^3")]
        [InlineData("(2 + 3*x)^(3/2)/(b*x + c*x^2)^3")]
        public void UnderTheRootWithTheQuadraticAsItsLinears(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 30000, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e.Substitute("A", 1.1).Substitute("B", 0.6).Substitute("b", 1.3)
                .Substitute("c", 0.7).Substitute("d", 0.4).Substitute("k", 0.9);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -3.1, -1.7, -0.3, 0.4, 1.3, 2.9 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                if (want.IsNaN)
                    continue;
                compared++;
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
            Assert.True(compared >= 5, $"only {compared} points could be compared for {integrand}");
        }

        [Theory]
        [InlineData("(d + k*x)^(7/2)/(b*x + c*x^2)^2", 3500)]
        [InlineData("(d + k*x)^(9/2)/(b*x + c*x^2)^2", 3500)]
        [InlineData("(d + k*x)^(1/2)/(b*x + c*x^2)^2", 3500)]
        [InlineData("(A + B*x)*(d + k*x)^(7/2)/(b*x + c*x^2)^2", 4500)]
        [InlineData("(A + B*x)*(d + k*x)^(5/2)/(b*x + c*x^2)^2", 4500)]
        [InlineData("(A + B*x)*(d + k*x)^(9/2)/(b*x + c*x^2)^3", 8000)]
        [InlineData("x^2*(d + k*x)^(5/2)/(b*x + c*x^2)^2", 2000)]
        public void SplitOverTheTwoLinearsInEachIntervalWhereTheRootIsReal(string integrand, int longest)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < longest, $"{text.Length} characters of answer for {integrand}");
            // The linear's root left of both of the quadratic's, -4.44 against -1.86 and 0, and
            // then -0.44 against 0 and 1.86: two points in each interval.
            foreach (var (c, d, points) in new[]
            {
                (0.7, 4.0, new[] { -4.1, -3.3, -1.5, -0.6, 0.4, 2.9 }),
                (-0.7, 0.4, new[] { -0.3, -0.1, 0.5, 1.3, 2.4, 3.9 }),
            })
            {
                Entity Pinned(Entity e) => e.Substitute("A", 1.1).Substitute("B", 0.6).Substitute("b", 1.3)
                    .Substitute("c", c).Substitute("d", d).Substitute("k", 0.9);
                var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
                var original = Pinned(integrand.ToEntity());
                foreach (var at in points)
                {
                    var want = original.Substitute("x", at).EvalNumerical();
                    var got = derivative.Substitute("x", at).EvalNumerical();
                    Assert.False(want.IsNaN, $"{integrand} has no value at x = {at}");
                    Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                                < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                        $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, c = {c}, d = {d}, where the integrand is {want}");
                }
            }
        }
    }
}
