//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using AngouriMath.Extensions;
using PeterO.Numbers;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// <see cref="Entity.DefiniteIntegral(Entity.Variable, EDecimal, EDecimal, int)"/> is composite five-point
    /// Gauss–Legendre, where it was a first-order rule printed to a hundred digits: <c>x^2</c> over <c>[0, 1]</c>
    /// was off by 1.67e-4 at 1000 steps. It never evaluates a bound, so a convergent integral with a singular
    /// endpoint has a value, where <c>sin(x)/x</c> and <c>ln(x)</c> over <c>[0, 1]</c> were NaN; one that
    /// diverges there is NaN; and the value has the digits the rule agrees on, fifteen at most.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1675">#1675</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class DefiniteIntegralByGaussLegendreTest
    {
        private static double Integral(string integrand, double from, double to, int steps = 100)
            => integrand.ToEntity().DefiniteIntegral("x", EDecimal.FromDouble(from), EDecimal.FromDouble(to), steps).RealPart.EDecimal.ToDouble();

        [Theory]
        [InlineData("x^2", 0, 1, 1.0 / 3, 1e-14)]
        [InlineData("e^x", 0, 2, 6.38905609893065, 1e-12)]
        [InlineData("sin(x)", 0, 3.141592653589793, 2, 1e-12)]
        [InlineData("x^3", -1, 1, 0, 1e-14)]
        // A removable singularity at a bound: Si(1).
        [InlineData("sin(x)/x", 0, 1, 0.946083070367183, 1e-12)]
        // Integrable singularities at a bound, where the rule converges slowly and the rest of the
        // series of differences is added.
        [InlineData("ln(x)", 0, 1, -1, 1e-4)]
        [InlineData("1/sqrt(x)", 0, 1, 2, 1e-2)]
        public void IsTheIntegral(string integrand, double from, double to, double expected, double tolerance)
        {
            var value = Integral(integrand, from, to);
            Assert.False(double.IsNaN(value), $"{integrand} over [{from}, {to}] is NaN");
            Assert.True(Math.Abs(value - expected) <= tolerance, $"{integrand} over [{from}, {to}] is {value}, where it is {expected}");
        }

        [Theory]
        [InlineData("1/x")]
        [InlineData("1/x^2")]
        public void DivergesAtABoundToNaN(string integrand)
            => Assert.True(double.IsNaN(Integral(integrand, 0, 1)), $"{integrand} over [0, 1] diverges, and came to {Integral(integrand, 0, 1)}");

        [Fact]
        public void IntegratesAlongAComplexSegment()
        {
            var value = "x".ToEntity().DefiniteIntegral("x", 0, Entity.Number.Complex.ImaginaryOne, 10);
            Assert.True((value + Entity.Number.Rational.Create(1, 2)).Abs().EDecimal.ToDouble() < 1e-14, $"x over [0, i] is {value}");
        }

        [Fact]
        public void HasNoMoreDigitsThanADouble()
        {
            var text = "x^2".ToEntity().DefiniteIntegral("x", 0, 1, 1000).Stringize();
            Assert.True(text.TrimStart('-', '0', '.').Length <= 15, $"x^2 over [0, 1] printed as {text}");
        }
    }
}
