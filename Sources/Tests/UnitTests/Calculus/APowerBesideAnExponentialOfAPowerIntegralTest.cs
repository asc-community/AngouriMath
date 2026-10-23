//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Linq;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// <c>x^m F^(a + b x^n)</c> and <c>(c + d x)^m F^(a + b x)</c>, whose integral is a gamma function of
    /// <c>s = (m + 1)/n</c> where <c>s</c> is neither whole nor half a whole number. Rubi's 2.3.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/2075">#2075</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class APowerBesideAnExponentialOfAPowerIntegralTest
    {
        private static readonly (string Name, string Value)[] Pins =
            { ("a", "1/3"), ("b", "-2/3"), ("c", "5/4"), ("d", "1/2"), ("f", "3"), ("m", "3/7"), ("n", "5/2") };

        /// <summary>Where every integrand here is real.</summary>
        private static readonly double[] Points = { 0.35, 0.9, 1.45, 2.3 };

        private static Entity DifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Entity Pinned(Entity e) => Pins.Aggregate(e, (current, pin) => current.Substitute(pin.Name, pin.Value.ToEntity()));
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            foreach (var at in Points)
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                var difference = Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart));
                Assert.True(difference / scale < 1e-9, $"d/dx of {integral} is {got} at x = {at}, where {integrand} is {want}");
            }
            return integral;
        }

        [Theory]
        [InlineData("x^m*e^(b*x)")]
        [InlineData("(d*x)^m*e^(b*x)")]
        [InlineData("x^(1/3)*e^(-x)")]
        [InlineData("f^(a + b*x^3)")]
        [InlineData("f^(a + b*x^3)*x")]
        [InlineData("e^(b*x^3)/x^2")]
        [InlineData("x^m*f^(a + b*x^n)")]
        [InlineData("f^(a + b/x)*x^m")]
        [InlineData("f^(a + b/x^2)*x^m")]
        [InlineData("(d*x)^m*x^2*e^(b*x)")]
        [InlineData("x^(1/3)*sqrt(x)*e^(-x)")]
        // A whole order is elementary before it is integrated, which leaves exactly these: Rubi's 8.6.
        [InlineData("(d*x)^m*gamma(3, b*x)")]
        public void APowerOfXBesideAnExponentialOfAPowerOfX(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// Past 30, a whole <c>s</c> as the gamma function, where parts would write <c>s</c> terms.
        /// </summary>
        [Fact]
        public void AHighWholePowerIsOneGammaFunction()
            => Assert.Contains("gamma(101, a * x)", DifferentiatesBack("x^100*e^(-a*x)").Stringize());

        [Theory]
        [InlineData("(c + d*x)^m*e^(a + b*x)")]
        // A linear exponent written as a negation, which is how gamma(2, a + b x) is elementary.
        [InlineData("x^(1/3)*e^(-(a + b*x))")]
        [InlineData("x^(1/3)*gamma(2, a + b*x)")]
        [InlineData("gamma(2, a + b*x)/x^(4/3)")]
        [InlineData("(c + d*x)^(1/3)*f^(a + b*x)")]
        [InlineData("e^(a + b*x)/(c + d*x)^(4/3)")]
        public void APowerOfALinearBesideAnExponentialOfAnother(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// Where <c>x^(m + 1) z^(-s)</c> is one number on both half-lines, it is written as that number, and the answer has a
        /// value at 0: from 0 to 1, by quadrature, 1.34190441797742 and 0.807511182139671.
        /// </summary>
        [Theory]
        [InlineData("integral(e^(x^3), x, 0, 1)", 1.34190441797742)]
        [InlineData("integral(e^(-x^3), x, 0, 1)", 0.807511182139671)]
        // 3^(-2/3) on both half-lines, which is not a number but one constant.
        [InlineData("integral(x^(1/3)*e^(-(1 + 3*x^2)), x, 0, 1)", 0.1169389395696306439)]
        [InlineData("integral(x*(1/3 + 2*ln(3/2*(1 + 2*x)))^(5/2), x, 1, 2)", 47.01873105138465896167)]
        public void ADefiniteIntegralFromZero(string integral, double value)
        {
            var result = integral.ToEntity().Simplify().EvalNumerical();
            Assert.True(Math.Abs((double)result.ImaginaryPart) < 1e-20 && Math.Abs((double)result.RealPart - value) < 1e-12, $"{result}");
        }

        /// <summary>
        /// With a positive rate, <c>x^(m + 1) (k x)^(-s)</c> is <c>k^(-s)</c>, and is written so.
        /// </summary>
        [Fact]
        public void APositiveRateIsTakenOutOfThePower()
            => Assert.Equal("-gamma(4/3, x)", DifferentiatesBack("x^(1/3)*e^(-x)").Substitute("C", 0).InnerSimplified.Stringize());

        /// <summary>
        /// Beside a power of a sine and a cosine, by Euler's formula: each <c>e^(i q u)</c> beside the power is a gamma function
        /// of an imaginary argument. Rubi's 4.1.10 and 4.1.12.
        /// </summary>
        [Theory]
        [InlineData("(c + d*x)^m*sin(a + b*x)")]
        [InlineData("(c + d*x)^m*cos(a + b*x)^2")]
        [InlineData("x^m*sin(b*x)")]
        [InlineData("x^(1/3)*sin(x)*cos(x)")]
        [InlineData("x*(a + b*sin(c + d*x^3))")]
        [InlineData("sin(c + d*x^3)^2")]
        public void APowerBesideASineOrACosine(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// And a hyperbolic function of a power of <c>x</c>, which arrives as <c>e^u</c> and <c>e^(-u)</c>: the second's exponent
        /// is read expanded. Rubi's 6.1.3.
        /// </summary>
        [Theory]
        [InlineData("x*sinh(c + d*x^3)")]
        [InlineData("(f*x)^m*sinh(c + d*x^2)")]
        [InlineData("x^m*sinh(a + b/x)")]
        [InlineData("x^m*e^(-(a + b*x^2))")]
        public void AHyperbolicFunctionOfAPowerOfX(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// A polynomial beside a symbolic power of a logarithm of one shifted linear, or an exponential of a power of one,
        /// written in <c>u = a + b x</c>, where each term is a gamma function. The constant in front comes along. Rubi's 3.3,
        /// 3.4 and 2.3.
        /// </summary>
        [Theory]
        [InlineData("2*x*(a + b*ln(c*(d + f*x)))^n")]
        [InlineData("x*(a + b*ln(c*(d + f*sqrt(x))))^n")]
        [InlineData("x^2*f^(c*(a + b*x)^3)")]
        [InlineData("(a + b*x)^2*e^(-c*(d + f*x)^3)")]
        public void APolynomialBesideAFunctionOfAShiftedLinear(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// A symbolic power of an inverse hyperbolic function, which its substitution writes as powers of <c>u</c> beside
        /// exponentials of it, each a gamma function. The route had these answers and turned them away: its check at
        /// sampled points could not evaluate a gamma function, and so had no point to compare at. Rubi's 7.1.4 and 7.2.4.
        /// </summary>
        [Theory]
        [InlineData("x*asinh(c*x)^n/sqrt(1 + c^2*x^2)")]
        [InlineData("(d + f*asinh(c*x))^n*sqrt(1 + c^2*x^2)")]
        [InlineData("x*(d + f*acosh(f*x))^n/(sqrt(f*x - 1)*sqrt(f*x + 1))")]
        public void ASymbolicPowerOfAnInverseHyperbolicFunction(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// Not where the integral is elementary, an exponential integral or an error function: those keep the form
        /// they had.
        /// </summary>
        [Theory]
        [InlineData("x^2*e^(b*x^3)")]
        [InlineData("sqrt(x)*e^(-x)")]
        [InlineData("e^(b*x^2)")]
        [InlineData("e^(b*x)/x")]
        [InlineData("x^2*sin(x)")]
        [InlineData("sin(x)/x")]
        public void NotWhereTheIntegralIsElementaryOrAnErrorFunction(string integrand)
            => Assert.DoesNotContain("gamma(", DifferentiatesBack(integrand).Stringize());
    }
}
