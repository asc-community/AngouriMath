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
    /// Integration by parts cuts a product into the polynomial factors and the rest, rather than
    /// at whichever node the product happens to have been built around.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>x*cos(x)*sin(x)</c> had no antiderivative and <c>x*(cos(x)*sin(x))</c> did. They are the
    /// same function: multiplication is associative, and the only difference is that the first
    /// parses left-associated, so the top node's two children are <c>x*cos(x)</c> and
    /// <c>sin(x)</c> — neither a polynomial, which is what every one of the by-parts cases is
    /// looking for.
    /// </para>
    /// <para>
    /// <b>Both spellings are asserted in every case below</b>, and that is the point of the test:
    /// the defect is invisible unless the same function is asked for twice.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class ByPartsRegroupingTest
    {
        private static readonly double[] Points = { 0.35, 0.61, 0.9, 1.15, 1.35 };

        private static void DifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("NaN", integral.Stringize());
            Assert.DoesNotContain("integral(", integral.Stringize());

            var derivative = integral.Substitute("C", 0).Differentiate("x");
            var original = integrand.ToEntity();
            var compared = 0;
            foreach (var at in Points)
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                if (got.IsNaN || want.IsNaN)
                    continue;
                compared++;
                var difference = Math.Abs((double)(got - want).RealPart)
                               + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart));
                Assert.True(difference / scale < 1e-9,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, "
                    + $"where the integrand is {want}");
            }
            Assert.True(compared >= 4,
                $"only {compared} of {Points.Length} points were comparable for {integrand}, "
                + "so this asserts almost nothing");
        }

        /// <summary>
        /// A polynomial times two trigonometric factors, written flat and written grouped. The
        /// second of each pair came out before this and the first did not.
        /// </summary>
        [Theory]
        [InlineData("x*cos(x)*sin(x)", "x*(cos(x)*sin(x))")]
        [InlineData("x*sin(x)*cos(x)^2", "x*(sin(x)*cos(x)^2)")]
        [InlineData("x^2*sin(x)*cos(x)", "x^2*(sin(x)*cos(x))")]
        [InlineData("x*sin(x)^3*cos(x)^2", "x*(sin(x)^3*cos(x)^2)")]
        public void TheSameProductWrittenTwoWays(string flat, string grouped)
        {
            DifferentiatesBack(flat);
            DifferentiatesBack(grouped);
        }

        /// <summary>
        /// Associativity is not the only spelling that moved the split — the order of the factors
        /// decides it too, since <c>cos(x)*sin(x)*x</c> puts the polynomial where the top node can
        /// see it and <c>x*cos(x)*sin(x)</c> does not. All four orders of the same three factors.
        /// </summary>
        [Theory]
        [InlineData("x*cos(x)*sin(x)")]
        [InlineData("cos(x)*x*sin(x)")]
        [InlineData("cos(x)*sin(x)*x")]
        [InlineData("sin(x)*x*cos(x)")]
        public void EveryOrderOfTheSameThreeFactors(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// More than one polynomial factor, which the regrouping gathers onto one side rather than
        /// leaving scattered, and a constant among them — a factor free of the variable belongs
        /// with the part that is differentiated, where it survives one step and then vanishes.
        /// </summary>
        [Theory]
        [InlineData("x*x*sin(x)*cos(x)")]
        [InlineData("2*x*sin(x)*cos(x)")]
        [InlineData("x*(x + 1)*sin(x)*cos(x)")]
        public void SeveralPolynomialFactors(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// What the regrouping must not change: a two-factor product, where the top node's split
        /// is the only one there is, and a product with no polynomial factor at all. These were
        /// answered before and are answered the same way.
        /// </summary>
        [Theory]
        [InlineData("x*sin(x)")]
        [InlineData("x*ln(x)")]
        [InlineData("x*e^x")]
        [InlineData("x^2*sin(x)")]
        [InlineData("sin(x)*cos(x)")]
        [InlineData("e^x*sin(x)")]
        [InlineData("x*arctan(x)")]
        public void TheTwoFactorCasesAreUnchanged(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// A product of three factors with nothing polynomial in it, which the regrouping declines
        /// to cut because there is no polynomial side to cut towards. Unanswered is a legitimate
        /// verdict here; a wrong answer is not.
        /// </summary>
        [Theory]
        [InlineData("sin(x)*cos(x)*e^x")]
        [InlineData("ln(x)*sin(x)*cos(x)")]
        public void NoPolynomialFactorToCutTowards(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("NaN", integral.Stringize());
            if (integral.Stringize().Contains("integral("))
                return;
            DifferentiatesBack(integrand);
        }
    }
}
