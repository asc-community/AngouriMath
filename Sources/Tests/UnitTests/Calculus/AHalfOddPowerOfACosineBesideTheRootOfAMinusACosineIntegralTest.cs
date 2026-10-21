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
    /// A half-odd power of <c>cos(y)</c> beside <c>sqrt(a - a cos(y))</c> or its reciprocal, and the
    /// sine's the same, rational in <c>v = sin(y)/(sqrt(a - a cos(y)) sqrt(cos(y)))</c>. They were
    /// answered with <c>sgn(sqrt(cos(y)) - 1)</c> and owed <c>a &gt; 0</c> and <c>cos(y) &gt;= 0</c>.
    /// Checked on every interval between the zeros of <c>sin(y/2)</c> and of <c>cos(y)</c> for a
    /// positive <c>a</c>, and where the integrand is real for a negative one. Rubi's 4.2.2.1 and
    /// 4.1.2.1.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AHalfOddPowerOfACosineBesideTheRootOfAMinusACosineIntegralTest
    {
        // y = 0.3 + 1.1 x at these is -0.69, 0.74, 2.06, 2.83, 3.49, 4.37 and 5.47: one point
        // below zero, one in (0, pi/2), two on each side of pi between pi/2 and 3pi/2, and one
        // in (3pi/2, 2pi), so on both sides of every zero of sin(y/2), cos(y) and sin(y).
        private static readonly double[] Points = { -0.9, 0.4, 1.6, 2.3, 2.9, 3.7, 4.7 };

        private static Entity Pinned(Entity e, double a)
            => e.Substitute("a", a).Substitute("c", 0.3).Substitute("d", 1.1);

        [Theory]
        [InlineData("cos(c + d*x)^(5/2)/sqrt(a - a*cos(c + d*x))")]
        [InlineData("cos(c + d*x)^(3/2)/sqrt(a - a*cos(c + d*x))")]
        [InlineData("sqrt(cos(c + d*x))/sqrt(a - a*cos(c + d*x))")]
        [InlineData("1/(sqrt(cos(c + d*x))*sqrt(a - a*cos(c + d*x)))")]
        [InlineData("1/(cos(c + d*x)^(3/2)*sqrt(a - a*cos(c + d*x)))")]
        [InlineData("1/(cos(c + d*x)^(7/2)*sqrt(a - a*cos(c + d*x)))")]
        [InlineData("cos(c + d*x)^(3/2)*sqrt(a - a*cos(c + d*x))")]
        [InlineData("sqrt(a - a*cos(c + d*x))/cos(c + d*x)^(5/2)")]
        [InlineData("cos(c + d*x)^(5/2)/sqrt(1 - cos(c + d*x))")]
        [InlineData("1/(sqrt(1 - cos(c + d*x))*sqrt(cos(c + d*x)))")]
        [InlineData("sin(c + d*x)^(5/2)/sqrt(a - a*sin(c + d*x))")]
        [InlineData("1/(sqrt(sin(c + d*x))*sqrt(a - a*sin(c + d*x)))")]
        [InlineData("sqrt(a - a*sin(c + d*x))/sin(c + d*x)^(3/2)")]
        public void DifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.DoesNotContain("sgn", text);
            Assert.DoesNotContain("provided", text);
            Assert.True(text.Length < 600, $"{text.Length} characters of answer for {integrand}");
            foreach (var a in new[] { 1.3, -1.3 })
            {
                var answer = Pinned(integral.Substitute("C", 0), a);
                var derivative = answer.Differentiate("x");
                var original = Pinned(integrand.ToEntity(), a);
                var compared = 0;
                foreach (var at in Points)
                {
                    var want = original.Substitute("x", at).EvalNumerical();
                    var real = Math.Abs((double)want.ImaginaryPart) < 1e-12 * Math.Max(1, Math.Abs((double)want.RealPart));
                    // For a negative a, where the integrand is real only.
                    if (a < 0 && !real)
                        continue;
                    var got = derivative.Substitute("x", at).EvalNumerical();
                    compared++;
                    Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                                < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                        $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, a = {a}, where the integrand is {want}");
                    if (real)
                    {
                        var value = answer.Substitute("x", at).EvalNumerical();
                        Assert.True(Math.Abs((double)value.ImaginaryPart) < 1e-9 * Math.Max(1, Math.Abs((double)value.RealPart)),
                            $"the antiderivative of {integrand} is {value} at x = {at}, a = {a}, where the integrand is real");
                    }
                }
                Assert.True(compared >= (a > 0 ? 7 : 2), $"only {compared} points compared for {integrand} at a = {a}");
            }
        }

        /// <summary>
        /// Across <c>y = pi</c>, where the sine changes sign and the integrand is real and smooth
        /// for a negative <c>a</c>: the antiderivative does not step there.
        /// </summary>
        [Theory]
        [InlineData("sqrt(cos(c + d*x))/sqrt(a - a*cos(c + d*x))")]
        [InlineData("cos(c + d*x)^(5/2)/sqrt(a - a*cos(c + d*x))")]
        [InlineData("1/(cos(c + d*x)^(3/2)*sqrt(a - a*cos(c + d*x)))")]
        [InlineData("cos(c + d*x)^(3/2)*sqrt(a - a*cos(c + d*x))")]
        public void ContinuousWhereTheSineChangesSign(string integrand)
        {
            var answer = Pinned(integrand.ToEntity().Integrate("x").Substitute("C", 0), -1.3);
            // y = pi at x = (pi - 0.3)/1.1 = 2.583...
            var below = answer.Substitute("x", 2.5832).EvalNumerical();
            var above = answer.Substitute("x", 2.5834).EvalNumerical();
            Assert.True(Math.Abs((double)(above - below).RealPart) + Math.Abs((double)(above - below).ImaginaryPart) < 1e-3,
                $"the antiderivative of {integrand} steps from {below} to {above} across y = pi");
        }
    }
}
