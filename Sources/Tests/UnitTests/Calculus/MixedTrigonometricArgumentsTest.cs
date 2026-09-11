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
    /// An integrand whose trigonometric functions have different multiples of one argument —
    /// <c>sin(x)/cos(2x)</c>, <c>cos(x)/(sin(x) tan(x/2))</c> — rewritten to one argument and
    /// handed on.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Every trigonometric rule reads one argument, so an integrand with <c>x</c> and <c>2x</c>
    /// in it was declined by all of them, while the same integrand with <c>cos(2x)</c> written
    /// as <c>1 - 2 sin(x)^2</c> came out. The rewriting goes down to the greatest common
    /// divisor of the slopes through the Chebyshev recurrences, or — when only an argument and
    /// its double are present and the integrand is even in the smaller — up to the double,
    /// which gives polynomials of half the degree.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class MixedTrigonometricArgumentsTest
    {
        private static readonly double[] Points = { 0.3, 0.7, 1.1, 2.3 };

        private static void DifferentiatesBack(string integrand, string variable = "x")
        {
            var integral = integrand.ToEntity().Integrate(variable);
            Assert.DoesNotContain("integral(", integral.Stringize());
            Assert.DoesNotContain("NaN", integral.Stringize());

            var derivative = integral.Substitute("C", 0).Differentiate(variable);
            var original = integrand.ToEntity();
            var compared = 0;
            foreach (var at in Points)
            {
                var got = derivative.Substitute(variable, at).EvalNumerical();
                var want = original.Substitute(variable, at).EvalNumerical();
                if (got.IsNaN || want.IsNaN)
                    continue;
                compared++;
                var difference = Math.Abs((double)(got - want).RealPart)
                               + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart));
                Assert.True(difference / scale < 1e-9,
                    $"d/d{variable} of the antiderivative of {integrand} is {got} at {variable} = {at}, "
                    + $"where the integrand is {want}");
            }
            Assert.True(compared >= 3,
                $"only {compared} of {Points.Length} points were comparable for {integrand}");
        }

        /// <summary>
        /// The argument and its double, written down to the argument. The first three are
        /// Rubi's (Timofeev, Stewart, and — with the half angle — Hearn).
        /// </summary>
        [Theory]
        [InlineData("sin(x)/cos(2*x)")]
        [InlineData("(cos(x) + sin(x))/sin(2*x)")]
        [InlineData("cos(x)/(sin(x)*tan(x/2))")]
        [InlineData("sin(x/2)/(1 + cos(x))")]
        [InlineData("1/(sin(x) + sin(2*x))")]
        [InlineData("sin(3*x)/sin(x)")]
        [InlineData("sin(2*x)/(1 + cos(x)^2)")]
        public void DifferentMultiplesOfOneArgument(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// Even in the smaller argument, so written up to the double: <c>tan(x)/tan(2x)</c> is
        /// <c>1 - 1/(2 cos(x)^2)</c> that way, and Moses's <c>sec(2t)/(1 + sec(t)^2 + 3 tan(t))</c>
        /// is a rational function of <c>sin(2t)</c> and <c>cos(2t)</c> that the half-angle
        /// substitution answers, where the other direction declined after five seconds.
        /// </summary>
        [Theory]
        [InlineData("tan(x)/tan(2*x)", "x")]
        [InlineData("cos(2*x)/cos(x)^2", "x")]
        [InlineData("sec(2*t)/(1 + sec(t)^2 + 3*tan(t))", "t")]
        public void EvenInTheSmallerArgument(string integrand, string variable) => DifferentiatesBack(integrand, variable);

        /// <summary>
        /// A product of two arguments is product-to-sum's and keeps the answer it had, in sines
        /// and cosines of sums rather than in powers.
        /// </summary>
        [Fact]
        public void AProductIsStillProductToSums()
        {
            var integral = "sin(x)*sin(2*x)".ToEntity().Integrate("x").Stringize();
            Assert.Contains("sin(x + 2 * x)", integral);
            DifferentiatesBack("sin(x)*sin(2*x)");
        }

        /// <summary>
        /// One argument throughout is not this rule's, and an offset in one of the arguments is
        /// not read; both are declined by it and answered or not by the rest as before.
        /// </summary>
        [Theory]
        [InlineData("sin(x)/cos(x)^2")]
        [InlineData("sin(x)*cos(x)")]
        public void OneArgumentIsLeftToTheOthers(string integrand) => DifferentiatesBack(integrand);
    }
}
