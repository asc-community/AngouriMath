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
    }
}
