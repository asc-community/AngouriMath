//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Collections.Generic;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// Fractional powers of <c>a ± a sin(y)</c>, or of <c>a ± a cos(y)</c>, beside anything
    /// rational in the sine and cosine of <c>y</c>, by the half angle at which they are squares:
    /// <c>1 + sin(y)</c> is <c>2 sin(y/2 + pi/4)^2</c>, so the power is a whole power of a sine
    /// times its sign, and the sign is a constant between the zeros that comes out in front.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// Rubi's <c>(A + C sin^2)/((c - c sin)^(3/2) sqrt(a + a sin))</c> and its kin were timeouts.
    /// Every answer is differentiated back with every symbol pinned, on both sides of a zero of
    /// the half-angle sine, and with <c>a</c> negative as well as positive: <c>1 + sin(y)</c> is
    /// not negative, so <c>(a q)^p = a^p q^p</c> for the principal powers whatever <c>a</c> is.
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class HalfAngleSquareTest
    {
        private static readonly double[] Points = { 0.3, 1.1, 2.6, 3.9, 5.4 };

        private static void DifferentiatesBack(string integrand, double a)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Assert.DoesNotContain("NaN", integral.Stringize());

            var pinned = new Dictionary<string, double> { ["a"] = a, ["c"] = 3, ["A"] = 1, ["B"] = 2, ["C"] = 1.5, ["pe"] = 0.3, ["f"] = 1.7, ["d"] = 1.3 };
            Entity Pin(Entity e)
            {
                foreach (var pair in pinned)
                    e = e.Substitute(pair.Key, pair.Value);
                return e;
            }
            var derivative = Pin(integral).Differentiate("x");
            var original = Pin(integrand.ToEntity());
            foreach (var at in Points)
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                var difference = Math.Abs((double)(got - want).RealPart)
                               + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart));
                Assert.True(difference / scale < 1e-7,
                    $"d/dx of the antiderivative of {integrand} at a = {a} is {got} at x = {at}, "
                    + $"where the integrand is {want}");
            }
        }

        [Theory]
        [InlineData("(a + a*sin(pe + f*x))^(1/2)/(c - c*sin(pe + f*x))^(5/2)")]
        [InlineData("(A + B*sin(pe + f*x))/((a + a*sin(pe + f*x))*(c - c*sin(pe + f*x))^(5/2))")]
        [InlineData("(a + a*sin(pe + f*x))^(3/2)*(c - c*sin(pe + f*x))^(1/2)*(A + B*sin(pe + f*x))")]
        [InlineData("(a + a*cos(x))^(3/2)*sin(x)^2")]
        [InlineData("(A + B*cos(c + d*x))*sec(c + d*x)/(a + a*cos(c + d*x))^(1/2)")]
        [InlineData("sqrt(a + a*sin(x))")]
        public void TheHalfAngleMakesThePowerWhole(string integrand)
        {
            DifferentiatesBack(integrand, 2);
            DifferentiatesBack(integrand, -3);
        }

        /// <summary>
        /// The hyperbolic cosine's: <c>1 + cosh(y)</c> is <c>2 cosh(y/2)^2</c>, with no sign to
        /// write, and <c>1 - cosh(y)</c> is <c>-2 sinh(y/2)^2</c>, with the sign of
        /// <c>sinh(y/2)</c>; the constants <c>2a</c> and <c>-2a</c> stay whole under the power.
        /// Points on both sides of zero, where <c>sinh(y/2)</c> changes sign. Rubi's 6.2.1 and 6.2.5.
        /// </summary>
        [Theory]
        [InlineData("x^2*sqrt(a + a*cosh(c + d*x))")]
        [InlineData("x^3*sqrt(a + a*cosh(x))")]
        [InlineData("x*(a + a*cosh(x))^(3/2)")]
        [InlineData("cosh(x)/sqrt(a - a*cosh(x))")]
        [InlineData("1/(a - a*cosh(c + d*x))^(3/2)")]
        [InlineData("(A + B*cosh(x))/(a - a*cosh(x))^(5/2)")]
        public void TheHalfAngleMakesTheHyperbolicPowerWhole(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Assert.DoesNotContain("NaN", integral.Stringize());
            foreach (var a in new[] { 2.0, -3.0 })
            {
                var pinned = new Dictionary<string, double> { ["a"] = a, ["c"] = 0.4, ["d"] = 1.3, ["A"] = 1, ["B"] = 2 };
                Entity Pin(Entity e)
                {
                    foreach (var pair in pinned)
                        e = e.Substitute(pair.Key, pair.Value);
                    return e;
                }
                var derivative = Pin(integral).Differentiate("x");
                var original = Pin(integrand.ToEntity());
                foreach (var at in new[] { -2.1, -0.7, 0.3, 1.1, 2.6 })
                {
                    var got = derivative.Substitute("x", at).EvalNumerical();
                    var want = original.Substitute("x", at).EvalNumerical();
                    var difference = Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart);
                    var scale = Math.Max(1.0, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart));
                    Assert.True(difference / scale < 1e-7,
                        $"d/dx of the antiderivative of {integrand} at a = {a} is {got} at x = {at}, where the integrand is {want}");
                }
            }
        }
        /// <summary>
        /// Hyperbolic powers two apart whose coefficients kill the reduction's residual:
        /// <c>int cosh^p = sinh cosh^(p - 1)/p + (p - 1)/p int cosh^(p - 2)</c>, so
        /// <c>cosh^p - (p - 1)/p cosh^(p - 2)</c> is <c>sinh cosh^(p - 1)/p</c> although neither
        /// power alone is elementary; the chain may be several steps long, as Rubi's
        /// <c>x/sech(x)^(7/2) - 5 x sqrt(sech(x))/21</c> is, whose coefficient is
        /// <c>(5/7)(1/3)</c>. One linear factor in front is taken by parts against the same
        /// antiderivative. Rubi's 6.1.1, 6.2.1, 6.5.1 and 6.6.1.
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </summary>
        [Theory]
        [InlineData("x/sech(x)^(3/2) - x*sqrt(sech(x))/3")]
        [InlineData("x/sech(x)^(5/2) - 3*x/sqrt(sech(x))/5")]
        [InlineData("x/sech(x)^(7/2) - 5*x*sqrt(sech(x))/21")]
        [InlineData("x/cosh(x)^(7/2) + 3*x*sqrt(cosh(x))/5")]
        [InlineData("x/csch(x)^(3/2) + x*sqrt(csch(x))/3")]
        [InlineData("cosh(x)^(5/2) - 3*cosh(x)^(1/2)/5")]
        [InlineData("sinh(2*x + 1)^(3/2) + sinh(2*x + 1)^(-1/2)/3")]
        public void ThePairOfPowersTwoApartIsElementary(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x").Substitute("C", 0);
            Assert.DoesNotContain("integral(", integral.Stringize());
            Assert.DoesNotContain("NaN", integral.Stringize());
            var derivative = integral.Differentiate("x");
            var original = integrand.ToEntity();
            var compared = 0;
            foreach (var at in new[] { 0.3, 0.7, 1.1, 1.9, 2.6 })
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                if (got.IsNaN || want.IsNaN)
                    continue;
                compared++;
                var difference = Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart));
                Assert.True(difference / scale < 1e-7, $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
            Assert.True(compared >= 4, $"only {compared} of five points were comparable for {integrand}");
        }

        /// <summary>A combination that does not kill the residual is not this rule's, and is left alone.</summary>
        [Fact]
        public void AnotherCoefficientIsLeftAsWritten()
            => Assert.Contains("integral(", "cosh(x)^(5/2) - cosh(x)^(1/2)/2".ToEntity().Integrate("x").Stringize());
    }
}
