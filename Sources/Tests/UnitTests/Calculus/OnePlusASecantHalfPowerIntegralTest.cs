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
        /// Two roots written apart are exact where <c>cos(y)</c> is positive, for any sign of the
        /// constants, and the answer says so: beyond it each may turn its sign where the other
        /// does not, while the integrand is real. Checked inside, where both roots of
        /// <c>1 - sec(x)</c> here are imaginary and their product real.
        /// </summary>
        [Fact]
        public void TwoRootsSayWhereTheyHold()
        {
            const string integrand = "(c - c*sec(x))^(7/2)/sqrt(a - a*sec(x))";
            Assert.Contains("provided cos(x) >= 0", integrand.ToEntity().Integrate("x").Stringize());
            DifferentiatesBackPinned(integrand, new[] { 0.3, 0.7, 1.1, 1.4 });
        }

        /// <summary>The cosecant's, through the complement: <c>csc(y) = sec(pi/2 - y)</c>.</summary>
        [Theory]
        [InlineData("sqrt(1 + csc(x))")]
        [InlineData("csc(x)/sqrt(a + a*csc(x))")]
        [InlineData("cot(x)^2/sqrt(a + a*csc(x))")]
        public void OnePlusACosecant(string integrand)
            => DifferentiatesBackPinned(integrand, new[] { 0.3, 0.7, 1.1, 1.4 });
    }
}
