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
    /// Half-odd powers of <c>a ± a sec(y)</c> and <c>a ± a csc(y)</c>, beside powers of the
    /// secant or cosecant and anything rational in the sine and cosine, by the half-angle
    /// tangent, in which the whole is rational beside one root: Rubi's
    /// <c>(a + b sec)^m (d sec)^n</c> files with <c>a^2 = b^2</c>, of which none was answered.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// Checked by differentiating back with the symbols pinned, at points where the integrand is
    /// real: inside <c>(0, pi/2)</c> for <c>1 + sec(y)</c> and <c>1 + csc(y)</c>, and in
    /// <c>(pi/2, pi)</c> for <c>1 - sec(y)</c>, whose root is real where the secant is negative.
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class OnePlusASecantHalfPowerIntegralTest
    {
        private static readonly (string, double)[] Pins = { ("a", 1.7), ("c", 0.9), ("A", 0.6), ("B", 1.3) };

        private static void DifferentiatesBackPinned(string integrand, double[] points)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());

            var derivative = integral.Substitute("C", 0).Differentiate("x");
            Entity original = integrand.ToEntity();
            foreach (var (name, value) in Pins)
            {
                derivative = derivative.Substitute(name, value);
                original = original.Substitute(name, value);
            }
            var compared = 0;
            foreach (var at in points)
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                if (got.IsNaN || want.IsNaN)
                    continue;
                compared++;
                var difference = Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart));
                Assert.True(difference / scale < 1e-9,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, "
                    + $"where the integrand is {want}");
            }
            Assert.True(compared >= 3, $"only {compared} points could be compared for {integrand}");
        }

        /// <summary>
        /// <c>1 + sec(y)</c> is <c>2/(1 - t^2)</c> for <c>t = tan(y/2)</c>: its half-odd powers
        /// beside a secant, a cotangent, or a half-odd power of the secant, whose root is then of
        /// <c>1 + t^2</c> alone.
        /// </summary>
        [Theory]
        [InlineData("sqrt(1 + sec(x))")]
        [InlineData("sqrt(a + a*sec(x))")]
        [InlineData("sec(x)/sqrt(a + a*sec(x))")]
        [InlineData("sec(x)/(a + a*sec(x))^(3/2)")]
        [InlineData("(a + a*sec(x))^(3/2)")]
        [InlineData("cot(x)^2/sqrt(a + a*sec(x))")]
        [InlineData("tan(x)^2*sqrt(1 + sec(x))")]
        [InlineData("1/(sec(x)^(3/2)*sqrt(1 + sec(x)))")]
        [InlineData("sqrt(sec(x))/sqrt(1 + sec(x))")]
        [InlineData("sec(x)^(3/2)*(A + B*sec(x))/(a + a*sec(x))^(5/2)")]
        public void OnePlusASecant(string integrand)
            => DifferentiatesBackPinned(integrand, new[] { 0.3, 0.7, 1.1, 1.4 });

        /// <summary>
        /// <c>1 - sec(y)</c> is <c>-2 t^2/(1 - t^2)</c>, and the square comes out of the root as
        /// a sign of <c>tan(y/2)</c>, constant between its zeros.
        /// </summary>
        [Theory]
        [InlineData("sqrt(a - a*sec(x))")]
        public void OneMinusASecant(string integrand)
            => DifferentiatesBackPinned(integrand, new[] { 1.8, 2.2, 2.6, 3.0 });

        /// <summary>
        /// d/dx of the antiderivative, the symbols pinned first, against the integrand at every
        /// point where the integrand is real: each of them must have a value there.
        /// </summary>
        private static void HoldsWhereverTheIntegrandIsReal(string integrand, double[] points, int compare, (string, double)[] pins)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());

            var answer = integral;
            Entity original = integrand.ToEntity();
            foreach (var (name, value) in pins)
            {
                answer = answer.Substitute(name, value);
                original = original.Substitute(name, value);
            }
            var derivative = answer.Differentiate("x");
            var compared = 0;
            foreach (var at in points)
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart));
                if (want.IsNaN || Math.Abs((double)want.ImaginaryPart) > 1e-12 * scale)
                    continue;
                compared++;
                var got = derivative.Substitute("x", at).EvalNumerical();
                Assert.False(got.IsNaN, $"the antiderivative of {integrand} has no value at x = {at}, where the integrand is {want}");
                var difference = Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart);
                Assert.True(difference / scale < 1e-9,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, "
                    + $"where the integrand is {want}");
            }
            Assert.True(compared >= compare, $"only {compared} points could be compared for {integrand}");
        }

        /// <summary>
        /// On either side of the cosine's sign change, two quarter turns each: (-pi, -pi/2),
        /// (-pi/2, pi/2), (pi/2, pi) and (pi, 3pi/2), and the sine's the same way for the cosecant.
        /// </summary>
        private static readonly double[] BothSides = { -2.6, -2.0, -1.2, 0.3, 1.1, 1.8, 2.6, 3.6, 4.2 };

        /// <summary>
        /// Two roots written apart are exact where <c>cos(y)</c> is positive, for any sign of the
        /// constants. Where it is negative each turns its sign by its constant's, and where the
        /// integrand is real -- on both sides at once, for a positive product of the constants --
        /// the turns are as many as the roots of the cosine: the answer holds there as written, or
        /// with the cosine's sign in front. Rubi's
        /// <c>cos(c + d x)^(3/2) (A + C sec(c + d x)^2)/sqrt(a + a sec(c + d x))</c> said
        /// <c>provided cos(c + d x) >= 0</c>, where without it the answer was the integrand's
        /// negative.
        /// </summary>
        [Theory]
        [InlineData("(c - c*sec(x))^(7/2)/sqrt(a - a*sec(x))")]
        [InlineData("sqrt(sec(x))*sqrt(1 + sec(x))")]
        [InlineData("cos(x)^(3/2)/sqrt(1 + sec(x))")]
        [InlineData("sqrt(cos(x))*sqrt(a + a*sec(x))")]
        [InlineData("sec(x)^(3/2)*(A + B*sec(x))*sqrt(a + a*sec(x))")]
        [InlineData("cos(x)^(5/2)*(A + B*sec(x))*sqrt(a + a*sec(x))")]
        [InlineData("sqrt(-csc(x))*sqrt(a - a*csc(x))")]
        [InlineData("sin(x)^(3/2)/sqrt(1 + csc(x))")]
        [InlineData("sqrt(cos(x))/sqrt(1 + sec(x))")]
        [InlineData("(1 + sec(x))^(5/2)*sqrt(cos(x))")]
        public void TwoRootsHoldOnBothSides(string integrand)
            => HoldsWhereverTheIntegrandIsReal(integrand, BothSides, BothSides.Length, Pins);

        /// <summary>
        /// Rubi's 4.5.4.2, <c>(a + a sec)^m (d sec)^n (A + B sec + C sec^2)</c>, at
        /// <c>y = 0.9 + 1.3 x</c>: from <c>-2.48</c> to <c>6.36</c>, both signs of the cosine.
        /// </summary>
        [Theory]
        [InlineData("cos(c+d*x)^(3/2)*(A+C*sec(c+d*x)^2)/sqrt(a+a*sec(c+d*x))")]
        [InlineData("sec(c+d*x)^(3/2)*(A+B*sec(c+d*x)+C*sec(c+d*x)^2)*sqrt(a+a*sec(c+d*x))")]
        [InlineData("(a+a*sec(c+d*x))^(3/2)*(A+C*sec(c+d*x)^2)/sec(c+d*x)^(5/2)")]
        [InlineData("cos(c+d*x)^(5/2)*(A+B*sec(c+d*x)+C*sec(c+d*x)^2)/(a+a*sec(c+d*x))^(3/2)")]
        public void RubisSecantRowsHoldOnBothSides(string integrand)
            => HoldsWhereverTheIntegrandIsReal(integrand, BothSides, BothSides.Length,
                new[] { ("a", 1.7), ("c", 0.9), ("d", 1.3), ("A", 0.6), ("B", 1.3), ("C", 0.8) });

        /// <summary>
        /// The product of the constants of <c>sqrt(sec(x)) sqrt(a - a sec(x))</c> is <c>-a</c>,
        /// whose sign is not known: the integrand is real for a negative <c>a</c>, and then the
        /// answer holds on both sides, and complex everywhere for a positive one, where it says it
        /// holds on the cosine's positive side.
        /// </summary>
        [Fact]
        public void AProductOfConstantsOfUnknownSignSaysWhereItHolds()
        {
            const string integrand = "sqrt(sec(x))*sqrt(a - a*sec(x))";
            Assert.Contains("provided", integrand.ToEntity().Integrate("x").Stringize());
            HoldsWhereverTheIntegrandIsReal(integrand, BothSides, BothSides.Length, new[] { ("a", -1.7) });
        }

        /// <summary>
        /// Three roots leave one of <c>1 - t^2</c> in the answer in t, which is kept beyond
        /// <c>t^2 < 1</c> where it differentiates back there. An odd number of roots makes the
        /// integrand real on one side of the cosine's sign change at most, the negative one for a
        /// negative product of the constants, and there the answer holds as written or with the
        /// cosine's sign in front. It said <c>provided cos(x) >= 0</c>, and
        /// <c>sqrt(-1 - sec(x)) sqrt(sec(x)) sqrt(cos(x))</c>, real only where the cosine is
        /// negative, had no value anywhere it is real.
        /// </summary>
        [Theory]
        [InlineData("sqrt(cos(x))*sqrt(sec(x))*sqrt(1 + sec(x))", 3)]
        [InlineData("(1 + sec(x))^(3/2)*sqrt(2*sec(x))*sqrt(3*cos(x))", 3)]
        [InlineData("sqrt(-1 - sec(x))*sqrt(sec(x))*sqrt(cos(x))", 6)]
        [InlineData("sqrt(1 - sec(x))*sqrt(sec(x))*sqrt(cos(x))", 6)]
        [InlineData("sqrt(sin(x))*sqrt(csc(x))*sqrt(1 + csc(x))", 4)]
        [InlineData("sqrt(-1 - csc(x))*sqrt(csc(x))*sqrt(sin(x))", 5)]
        public void ThreeRootsHoldWhereTheIntegrandIsReal(string integrand, int compare)
        {
            Assert.DoesNotContain("provided", integrand.ToEntity().Integrate("x").Stringize());
            HoldsWhereverTheIntegrandIsReal(integrand, BothSides, compare, Pins);
        }

        /// <summary>
        /// Three roots whose product of constants is of unknown sign: the integrand is real where
        /// the cosine is negative for a negative product, and the answer holds there, and where it
        /// is positive for a positive one; the answer says <c>provided a < 0 or cos(x) >= 0</c>, or
        /// <c>-a < 0</c> for <c>a - a sec(x)</c>, the integrand complex beyond it. Each at both
        /// signs of the constant, and the cosecant's through the complement.
        /// </summary>
        [Theory]
        [InlineData("sqrt(a + a*sec(x))*sqrt(sec(x))*sqrt(cos(x))", 1.7, 3)]
        [InlineData("sqrt(a + a*sec(x))*sqrt(sec(x))*sqrt(cos(x))", -1.7, 6)]
        [InlineData("sqrt(a + a*sec(x))*sec(x)^(3/2)*sqrt(cos(x))*(A + B*sec(x))", 1.7, 3)]
        [InlineData("sqrt(a + a*sec(x))*sec(x)^(3/2)*sqrt(cos(x))*(A + B*sec(x))", -1.7, 6)]
        [InlineData("sqrt(a - a*sec(x))*sqrt(sec(x))*sqrt(cos(x))*cos(x)", 1.7, 6)]
        [InlineData("sqrt(a - a*sec(x))*sqrt(sec(x))*sqrt(cos(x))*cos(x)", -1.7, 3)]
        [InlineData("sqrt(a + a*csc(x))*sqrt(csc(x))*sqrt(sin(x))", 1.7, 4)]
        [InlineData("sqrt(a + a*csc(x))*sqrt(csc(x))*sqrt(sin(x))", -1.7, 5)]
        public void ThreeRootsOfAConstantOfUnknownSignHoldWhereTheIntegrandIsReal(string integrand, double a, int compare)
        {
            Assert.Contains(" < 0 or ", integrand.ToEntity().Integrate("x").Stringize());
            HoldsWhereverTheIntegrandIsReal(integrand, BothSides, compare, new[] { ("a", a), ("A", 0.6), ("B", 1.3) });
        }

        /// <summary>
        /// The same at <c>y = 0.9 + 1.3 x</c>, where the points fall four where the cosine is
        /// positive and five where it is negative, and with three binomials, of which the
        /// integrand is real where the cosine is negative for <c>a c k > 0</c> and where it is
        /// positive for <c>a c k < 0</c>.
        /// </summary>
        [Theory]
        [InlineData("sqrt(a + a*sec(c + d*x))*sqrt(sec(c + d*x))*sqrt(cos(c + d*x))", 1.7, 0.9, 4)]
        [InlineData("sqrt(a + a*sec(c + d*x))*sqrt(sec(c + d*x))*sqrt(cos(c + d*x))", -1.7, 0.9, 5)]
        [InlineData("sqrt(a + a*sec(x))*sqrt(c + c*sec(x))*sqrt(k - k*sec(x))", 1.7, 0.9, 6)]
        [InlineData("sqrt(a + a*sec(x))*sqrt(c + c*sec(x))*sqrt(k - k*sec(x))", -1.7, 0.9, 3)]
        public void ThreeRootsOfSymbolsHoldWhereTheIntegrandIsReal(string integrand, double a, double c, int compare)
            => HoldsWhereverTheIntegrandIsReal(integrand, BothSides, compare,
                new[] { ("a", a), ("c", c), ("d", 1.3), ("k", 1.3) });

        /// <summary>The cosecant's, through the complement: <c>csc(y) = sec(pi/2 - y)</c>.</summary>
        [Theory]
        [InlineData("sqrt(1 + csc(x))")]
        [InlineData("csc(x)/sqrt(a + a*csc(x))")]
        [InlineData("cot(x)^2/sqrt(a + a*csc(x))")]
        public void OnePlusACosecant(string integrand)
            => DifferentiatesBackPinned(integrand, new[] { 0.3, 0.7, 1.1, 1.4 });
    }
}
