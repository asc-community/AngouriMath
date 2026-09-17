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
    /// A fractional power of a constant times an even power of a function, the function
    /// taken out of the power with its sign: <c>(a sin(x)^2)^(5/2)</c> is
    /// <c>a^(5/2) sgn(sin(x)) sin(x)^5</c>, and the sign is a constant between the zeros of
    /// the sine that comes out in front of the integral.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// Rubi's <c>x sqrt(sin(x)^2)</c>, <c>(csc(x)^2)^(3/2)</c> and <c>1/(csc(x)^2)^(7/2)</c>
    /// had no antiderivative, and <c>(a sin(x)^2)^(5/2)</c> one in powers of the modulus of the
    /// sine. Every answer is differentiated back on both sides of a zero of the function, and
    /// the constant is given a negative value as well as a positive one: an even power is not
    /// negative, so <c>(c q)^p = c^p q^p</c> for the principal powers whatever <c>c</c> is.
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class EvenPowerSignTest
    {
        /// <summary>On both sides of the sine's zero at pi, and of the cosine's at pi/2 and 3pi/2.</summary>
        private static readonly double[] AroundTheZeros = { 0.5, 1.2, 2.0, 3.5, 5.0, 6.5 };

        private static void DifferentiatesBack(string integrand, double[] points, double constant)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Assert.DoesNotContain("NaN", integral.Stringize());
            Assert.DoesNotContain("abs(", integral.Stringize());

            var derivative = integral.Substitute("C", 0).Substitute("a", constant).Differentiate("x");
            var original = integrand.ToEntity().Substitute("a", constant);
            foreach (var at in points)
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                var difference = Math.Abs((double)(got - want).RealPart)
                               + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart));
                Assert.True(difference / scale < 1e-8,
                    $"d/dx of the antiderivative of {integrand} at a = {constant} is {got} at x = {at}, "
                    + $"where the integrand is {want}");
            }
        }

        [Theory]
        [InlineData("(a*sin(x)^2)^(5/2)")]
        [InlineData("(a*sin(x)^2)^(3/2)/a")]
        [InlineData("x*sqrt(a*sin(x)^2)")]
        [InlineData("(a*csc(x)^2)^(3/2)")]
        [InlineData("1/(a*csc(x)^2)^(7/2)")]
        [InlineData("1/sqrt(a*cot(x)^2)")]
        [InlineData("(a*cos(x)^4)^(3/4)")]
        public void TheFunctionComesOutOfThePowerWithItsSign(string integrand)
        {
            DifferentiatesBack(integrand, AroundTheZeros, 2);
            DifferentiatesBack(integrand, AroundTheZeros, -3);
            Assert.Contains("sgn(", integrand.ToEntity().Integrate("x").Stringize());
        }

        /// <summary>An even product of the exponents has no sign to keep.</summary>
        [Theory]
        [InlineData("(a*sin(x)^4)^(1/2)")]
        [InlineData("1/(a*cos(x)^4)^(1/2)")]
        [InlineData("(a*cos(x)^4)^(3/2)")]
        public void AnEvenWholePowerNeedsNoSign(string integrand)
        {
            DifferentiatesBack(integrand, AroundTheZeros, 2);
            DifferentiatesBack(integrand, AroundTheZeros, -3);
            Assert.DoesNotContain("sgn(", integrand.ToEntity().Integrate("x").Stringize());
        }
    }
}
