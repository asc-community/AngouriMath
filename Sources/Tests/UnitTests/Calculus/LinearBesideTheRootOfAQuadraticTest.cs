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
    /// <c>K/((x - p) sqrt(Q))</c> by the reciprocal of the linear: <c>t = 1/(x - p)</c> makes
    /// the root one of a quadratic in <c>t</c> alone, <c>Q(p) t^2 + Q'(p) t + a</c>, and the
    /// table answers that with an arcsine or a logarithm by the sign of <c>Q(p)</c>. The sign
    /// of <c>t</c> is the sign of <c>x - p</c>, and the antiderivative is
    /// <c>-K sgn(x - p) G(1/(x - p))</c> on both sides of <c>p</c>.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// The Euler substitution answered this shape at length, as a partial-fraction
    /// decomposition in its <c>t</c>, and declined it where the leading coefficient and the
    /// constant are symbols it cannot take a real root of: Hearn's
    /// <c>1/(r sqrt(-alpha^2 - epsilon^2 + 2h r^2 - 2k r^4))</c>, which is this under
    /// <c>u = r^2</c>, had no antiderivative.
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class LinearBesideTheRootOfAQuadraticTest
    {
        private static Entity DifferentiatesBack(string integrand, double[] points, params (string, double)[] pins)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Assert.DoesNotContain("NaN", integral.Stringize());

            Entity answer = integral.Substitute("C", 0);
            Entity original = integrand.ToEntity();
            foreach (var (name, value) in pins)
            {
                answer = answer.Substitute(name, value);
                original = original.Substitute(name, value);
            }
            // With the symbols pinned the piecewise's conditions are decidable, and the
            // simplification picks the arm before the derivative is taken.
            var derivative = answer.Simplify().Differentiate("x");

            var compared = 0;
            foreach (var at in points)
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                if (got.IsNaN || want.IsNaN)
                    continue;
                compared++;
                var difference = Math.Abs((double)(got - want).RealPart)
                               + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart));
                Assert.True(difference / scale < 1e-8,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, "
                    + $"where the integrand is {want}");
            }
            Assert.True(compared >= 3,
                $"only {compared} of {points.Length} points were comparable for {integrand}");
            return integral;
        }

        /// <summary>
        /// Both signs of <c>Q(p)</c>, and the root at <c>p</c> itself; checked on both sides of
        /// <c>p</c>, where the sign of <c>x - p</c> is what makes one formula serve. The
        /// answers are a line each where Euler's were a paragraph.
        /// </summary>
        [Theory]
        [InlineData("1/((x + 1)*sqrt(x^2 + x + 3))", new[] { -3.0, -1.7, -0.4, 0.9, 2.6 })]
        [InlineData("1/((x + 1)*sqrt(x^2 + x - 3))", new[] { -4.0, -2.9, 1.4, 2.6 })]
        [InlineData("1/(x*sqrt(x^2 + x + 1))", new[] { -2.6, -0.9, 0.3, 0.9, 2.6 })]
        [InlineData("1/((x - 1)*sqrt(x^2 - 2*x + 5))", new[] { -1.3, 0.2, 1.9, 2.7, 3.6 })]
        [InlineData("1/(x*sqrt(-5 + 10*x - 4*x^2))", new[] { 0.8, 1.0, 1.3, 1.6 })]
        [InlineData("1/(x*sqrt(3*x - x^2))", new[] { 0.5, 1.0, 1.7, 2.4 })]
        [InlineData("3/((2*x + 1)*sqrt(2 - x^2))", new[] { -1.3, -0.9, 0.2, 0.7, 1.3 })]
        public void ALinearBesideTheRoot(string integrand, double[] points)
        {
            var integral = DifferentiatesBack(integrand, points);
            var length = integral.Stringize().Length;
            Assert.True(length < 300, $"{length} characters of answer for {integrand}");
        }

        /// <summary>
        /// Hearn's, with the coefficients symbols: a piecewise on the sign of <c>Q(0)</c>,
        /// which is <c>-alpha^2 - epsilon^2</c>, and Rubi's arcsine in the arm that holds.
        /// Reached under <c>u = r^2</c>, for which a symbolic integrand this small has its
        /// powers of <c>r</c> collected like a numeric one.
        /// </summary>
        [Theory]
        [InlineData("1/(x*sqrt(-alpha^2 - epsilon^2 + 2*h*x^2 - 2*k*x^4))", new[] { 0.7, 0.9, 1.1, 1.3 })]
        [InlineData("1/(x*sqrt(-alpha^2 - epsilon^2 + 2*h*x - 2*k*x^2))", new[] { 0.5, 0.8, 1.2, 1.6 })]
        public void TheCoefficientsMayBeSymbols(string integrand, double[] points)
            => DifferentiatesBack(integrand, points, ("alpha", 0.6), ("epsilon", 0.5), ("h", 2.0), ("k", 0.6));

        /// <summary>
        /// A polynomial over the root, with the leading coefficient a symbol: reduced to
        /// <c>R sqrt(Q) + K/sqrt(Q)</c> by one solve, the last term the table's piecewise on
        /// the sign of the leading coefficient. Hearn's <c>r/sqrt(-alpha^2 - 2k r + 2pe r^2)</c>
        /// had no antiderivative, with <c>2pe</c> in front and no sign to go on; checked on
        /// both signs of the leading coefficient, and with it zero, where the arm is the
        /// polynomial over the root of a linear and not the reduction that divided by it.
        /// </summary>
        [Theory]
        [InlineData("x/sqrt(-alpha^2 - 2*k*x + 2*pe*x^2)", new[] { 2.0, 2.5, 3.0, 3.5 }, 1.0)]
        [InlineData("x/sqrt(-alpha^2 - 2*k*x + 2*pe*x^2)", new[] { -1.5, -1.2, -0.8, -0.5 }, -0.4)]
        [InlineData("x/sqrt(-alpha^2 - 2*k*x + 2*pe*x^2)", new[] { -3.0, -2.5, -2.0, -1.5 }, 0.0)]
        [InlineData("x^2/sqrt(c + b*x + a*x^2)", new[] { 0.7, 1.1, 1.6, 2.2 }, 1.0)]
        [InlineData("(x^3 + 2*x)/sqrt(c + b*x + a*x^2)", new[] { 0.7, 1.1, 1.6, 2.2 }, 1.0)]
        public void APolynomialOverTheRootWithASymbolInFront(string integrand, double[] points, double leading)
            => DifferentiatesBack(integrand, points, ("alpha", 0.6), ("k", 0.7), ("pe", leading), ("a", leading), ("b", 0.7), ("c", 0.6));

        /// <summary>
        /// The same reduction for any odd half power: Stewart's <c>x^2/(a^2 - x^2)^(3/2)</c>,
        /// Apostol's <c>(a^2 - x^2)^(5/2)</c> and Timofeev's <c>x^2 sqrt(2 r x - x^2)</c>, each
        /// with a symbol in the radicand and each declined by the trigonometric substitution
        /// for it. Checked with the symbol of either sign, since <c>arcsin(x/sqrt(a^2))</c> is
        /// what the table writes and it is right for both.
        /// </summary>
        [Theory]
        [InlineData("x^2/(a^2 - x^2)^(3/2)", new[] { -0.9, -0.4, 0.3, 0.8 }, 1.3)]
        [InlineData("x^2/(a^2 - x^2)^(3/2)", new[] { -0.9, -0.4, 0.3, 0.8 }, -1.3)]
        [InlineData("(a^2 - x^2)^(5/2)", new[] { -0.9, -0.4, 0.3, 0.8 }, 1.3)]
        [InlineData("x^3/(a^2 + x^2)^(5/2)", new[] { -0.9, -0.4, 0.3, 0.8 }, 1.3)]
        [InlineData("x^2*sqrt(2*a*x - x^2)", new[] { 0.3, 0.8, 1.4, 2.1 }, 1.3)]
        [InlineData("x^3*sqrt(2*a*x - x^2)", new[] { 0.3, 0.8, 1.4, 2.1 }, 1.3)]
        public void AnyOddHalfPowerWithASymbol(string integrand, double[] points, double a)
            => DifferentiatesBack(integrand, points, ("a", a));

        /// <summary>
        /// With a power of x below: two remainders, <c>K_1/sqrt(Q)</c> and <c>K_2/(x sqrt(Q))</c>.
        /// Stewart's <c>sqrt(x^2 - a^2)/x^4</c> and <c>sqrt(a^2 - x^2)/x^2</c>, the second
        /// <c>-sqrt(a^2 - x^2)/x - arcsin(x/a)</c>.
        /// </summary>
        [Theory]
        [InlineData("sqrt(a^2 - x^2)/x^2", new[] { -0.9, -0.4, 0.3, 0.8 }, 1.3)]
        [InlineData("sqrt(x^2 - a^2)/x^4", new[] { -3.0, -2.0, 1.5, 2.5 }, 1.3)]
        [InlineData("sqrt(x^2 + a^2)/x^2", new[] { -3.0, -2.0, 1.5, 2.5 }, 1.3)]
        [InlineData("(x + 1)*sqrt(2*x^2 + 3)/x^3", new[] { -3.0, -2.0, 1.5, 2.5 }, 1.3)]
        [InlineData("1/(x^2*(a^2 - x^2)^(3/2))", new[] { -0.9, -0.4, 0.3, 0.8 }, 1.3)]
        public void AnOddHalfPowerOverAPowerOfX(string integrand, double[] points, double a)
            => DifferentiatesBack(integrand, points, ("a", a));

        /// <summary>
        /// The sign of <c>x - p</c> is not decoration: the antiderivative is exact on both
        /// sides of the pole, and the two one-sided pieces are not the same formula.
        /// </summary>
        [Fact]
        public void TheSignOfTheLinearIsInTheAnswer()
            => Assert.Contains("sgn(", "1/((x + 1)*sqrt(x^2 + x + 3))".ToEntity().Integrate("x").Stringize());
    }
}
