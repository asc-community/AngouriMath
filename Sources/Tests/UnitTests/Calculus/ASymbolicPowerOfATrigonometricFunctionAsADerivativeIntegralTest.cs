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
    /// A symbolic power of a sine, cosine, secant or cosecant times <c>alpha + beta f^2</c>, where it is
    /// the derivative of <c>cos sin^(m + 1)</c>, <c>sin cos^(m + 1)</c>, <c>tan sec^m</c> or <c>cot csc^m</c>:
    /// Rubi's 4.1.4.1 and 4.5.4.1.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ASymbolicPowerOfATrigonometricFunctionAsADerivativeIntegralTest
    {
        private static void DifferentiatesBack(string integrand, params (string, double)[] pins)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            DifferentiatesBack(integrand, integral, pins);
        }

        private static void DifferentiatesBack(string integrand, Entity integral, params (string, double)[] pins)
        {
            var pinned = integral.Substitute("C", 0);
            Entity original = integrand.ToEntity();
            foreach (var (name, value) in pins)
            {
                pinned = pinned.Substitute(name, value);
                original = original.Substitute(name, value);
            }
            var derivative = pinned.Differentiate("x");
            foreach (var at in new[] { -2.3, -0.7, 0.4, 1.9 })
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                var difference = Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart));
                Assert.True(difference / scale < 1e-8,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
        }

        // d with the sign that keeps the argument real at every sampled x, and |d| below 2/2.3^2
        // so that the inverse sine and cosine stay inside [-1, 1]; off it the exact checks hold too.
        [Theory]
        [InlineData("sec(c+d*x)^m*(-q*m/(1+m)+q*sec(c+d*x)^2)")]
        [InlineData("sec(c+d*x)^m*(A-A*(1+m)*sec(c+d*x)^2/m)")]
        [InlineData("sin(c+d*x)^m*(1+m-(2+m)*sin(c+d*x)^2)")]
        [InlineData("cos(x)^m*(-(1+m)+(2+m)*cos(x)^2)")]
        [InlineData("csc(x)^m*(m-(1+m)*csc(x)^2)")]
        // Under the bar the power is f^(-m): (tan sec^(-m))' and (sin cos^(1 - m))'.
        [InlineData("(m+(1-m)*sec(c+d*x)^2)/sec(c+d*x)^m")]
        [InlineData("(-(1-m)+(2-m)*cos(x)^2)/cos(x)^m")]
        public void IsAnsweredAndDifferentiatesBack(string integrand)
        {
            DifferentiatesBack(integrand, ("A", 0.6), ("q", 1.25), ("c", 0.3), ("d", 0.75), ("m", 2.5));
            DifferentiatesBack(integrand, ("A", -0.75), ("q", 0.4), ("c", -1), ("d", -0.5), ("m", 1.0 / 3));
        }

        // A power that is not whole: Rubi's 4.5.4.1 (3 + 3 sec(c + d x)^2)/sqrt(sec(c + d x)), and the
        // same derivatives at m = 1/2, 1/3 and 3/2.
        [Theory]
        [InlineData("(3+3*sec(c+d*x)^2)/sec(c+d*x)^(1/2)")]
        [InlineData("sqrt(sin(x))*(3/2-5/2*sin(x)^2)")]
        [InlineData("sin(c+d*x)^(1/3)*(4/3-7/3*sin(c+d*x)^2)")]
        [InlineData("cos(x)^(3/2)*(-5/2+7/2*cos(x)^2)")]
        [InlineData("csc(x)^(1/2)*(1/2-3/2*csc(x)^2)")]
        public void IsAnsweredForAPowerThatIsNotWhole(string integrand)
        {
            DifferentiatesBack(integrand, ("c", 0.3), ("d", 0.75));
            DifferentiatesBack(integrand, ("c", -1), ("d", -0.5));
        }

        // The power is not a factor of the whole integrand, or is one under the bar, where the bracket is
        // not the derivative's: none of these is the derivative of a single product.
        [Theory]
        [InlineData("(-m+(1+m)*sec(x)^2)/sec(x)^m")]
        [InlineData("-m*sec(x)^m+(1+m)*sec(x)^2")]
        [InlineData("sin(x)^m*(1+m)-(2+m)*sin(x)^2")]
        [InlineData("(1+m-(2+m)*sin(x)^2)/sin(x)^m")]
        [InlineData("(-m+(1+m)*csc(x)^2)/csc(x)^m")]
        public void IsNotAnsweredWronglyWhereThePowerIsNotThatFactor(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            if (integral.Stringize().Contains("integral("))
                return;
            DifferentiatesBack(integrand, integral, ("m", 2.5));
            DifferentiatesBack(integrand, integral, ("m", 1.0 / 3));
        }

        [Fact]
        public void IsNotClaimedWhereTheBracketIsNotTheDerivative()
        {
            // beta p0 != alpha p2: (1 + m) sec^2 - (m + 1) is not the derivative of tan sec^m.
            var integral = "sec(x)^m*(1+m-(1+m)*sec(x)^2)".ToEntity().Integrate("x").Stringize();
            Assert.DoesNotContain("tan(x) * sec(x) ^ m", integral);
        }
    }
}
