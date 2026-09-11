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
    /// Integrands that came out with a numeric coefficient and were declined with a symbolic
    /// one — three separate causes, found by probing each shape in both spellings.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Every test here pins the parameters after integrating and differentiates back, so the
    /// answer is checked as a function of <c>x</c> for one value of each parameter, which is
    /// what an answer with a parameter in it claims for every value of it.
    /// </para>
    /// <para>
    /// <b>The long division's leading coefficient.</b> The division declined a divisor whose
    /// leading coefficient was a symbol, because the quotient loses the value that makes it
    /// zero. That is the right decision for the simplifier, whose rewrite has to be an
    /// equivalence, and the wrong one for the integrator, where every neighbouring rule already
    /// gives the generic answer: <c>int 1/(a x + b) dx</c> is <c>ln(a x + b)/a</c>, undefined at
    /// <c>a = 0</c> and given anyway. The integrator now asks for the generic case, and the
    /// simplifier's behaviour is unchanged.
    /// </para>
    /// <para>
    /// <b>The variable the division is in.</b> It was whichever came first, and for
    /// <c>a x^2 / (b + a x)</c> that could be <c>a</c> — dividing in the parameter gives
    /// <c>x - b x/(b + a x)</c>, true and useless, and consumes the one attempt. The integrator
    /// now says which variable it means.
    /// </para>
    /// <para>
    /// <b>A dead <c>d^0</c>.</b> The exponential-times-trigonometric reader multiplies the
    /// polynomial part by the base to the constant part of the exponent, and with no constant
    /// part that was <c>d^0</c> written out — which a numeric base folded away and a symbolic
    /// one did not, and which stopped the polynomial being read.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class SymbolicParameterIntegralTest
    {
        private static readonly double[] Points = { 0.3, 0.9, 1.7, 2.6 };

        private static void DifferentiatesBack(string integrand, params (string, double)[] pins)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Assert.DoesNotContain("NaN", integral.Stringize());

            var derivative = integral.Substitute("C", 0).Differentiate("x");
            Entity original = integrand.ToEntity();
            foreach (var (name, value) in pins)
            {
                derivative = derivative.Substitute(name, value);
                original = original.Substitute(name, value);
            }

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
            Assert.True(compared >= 3,
                $"only {compared} of {Points.Length} points were comparable for {integrand}");
        }

        /// <summary>
        /// An improper fraction whose divisor has a symbolic leading coefficient, which is the
        /// gap <see cref="ImproperFractionIntegralTest"/> used to record as still open.
        /// </summary>
        [Theory]
        [InlineData("x/(a + b*x)")]
        [InlineData("x^2/(a + b*x)")]
        [InlineData("x^2/(2 + b*x)")]
        [InlineData("x^3/(a + b*x)")]
        [InlineData("(x^2 + 1)/(a + b*x)")]
        public void ASymbolicLeadingCoefficientIsDividedOut(string integrand)
            => DifferentiatesBack(integrand, ("a", 1.3), ("b", 0.7));

        /// <summary>
        /// The division in the variable the integrator means, not the one that came first: the
        /// parameter appears in the numerator too, so the division could be taken in it.
        /// </summary>
        [Theory]
        [InlineData("a*x^2/(b + a*x)")]
        [InlineData("x^2*a/(b + a*x)")]
        [InlineData("b*x^3/(b + a*x)")]
        public void TheDivisionIsInTheIntegrationVariable(string integrand)
            => DifferentiatesBack(integrand, ("a", 1.3), ("b", 0.7));

        /// <summary>
        /// Integration by parts leaves exactly that improper fraction behind, so these are what
        /// the two changes above are for.
        /// </summary>
        [Theory]
        [InlineData("x*ln(b + a*x)")]
        [InlineData("x*ln(3 + a*x)")]
        [InlineData("x^2*ln(b + a*x)")]
        public void ByPartsThatLeavesASymbolicImproperFraction(string integrand)
            => DifferentiatesBack(integrand, ("a", 1.3), ("b", 0.7));

        /// <summary>
        /// A symbolic base on the exponential, with a polynomial and a trigonometric factor beside
        /// it. <c>d^x cos(x)</c> came out and <c>d^x x cos(x)</c> did not, which is what named
        /// the polynomial read as the place.
        /// </summary>
        [Theory]
        [InlineData("d^x*x*cos(x)")]
        [InlineData("d^x*x*sin(x)")]
        [InlineData("d^x*x^2*cos(x)")]
        [InlineData("d^x*x^3*cos(x)")]
        [InlineData("d^(2*x)*x*cos(x)")]
        [InlineData("d^x*x*cos(2*x)")]
        public void ASymbolicExponentialBaseTimesAPolynomialAndATrigonometric(string integrand)
            => DifferentiatesBack(integrand, ("d", 1.7));

        /// <summary>
        /// Two symbolic bases and no polynomial at all, where the dead factor was <c>a^0 b^0</c>
        /// standing in for the polynomial. Found by the corpus rather than the probe.
        /// </summary>
        [Theory]
        [InlineData("a^x/b^x")]
        [InlineData("a^x*b^x")]
        public void TwoSymbolicExponentialBases(string integrand)
            => DifferentiatesBack(integrand, ("a", 1.7), ("b", 2.3));

        /// <summary>
        /// The numeric spellings, which came out before and must keep coming out the same way.
        /// </summary>
        [Theory]
        [InlineData("x/(3 + 5*x)")]
        [InlineData("x*ln(3 + 5*x)")]
        [InlineData("3^x*x*cos(x)")]
        [InlineData("2^x*x*cos(x)")]
        [InlineData("x^2/(x + a)")]
        public void TheNumericSpellingsStillAnswer(string integrand)
            => DifferentiatesBack(integrand, ("a", 1.3));

        /// <summary>
        /// The simplifier's own long division is not what changed: a symbolic leading coefficient
        /// is still not divided out by <c>Simplify</c>, because there the rewrite has to hold for
        /// every value of the parameter and at <c>b = 0</c> it does not.
        /// </summary>
        [Theory]
        [InlineData("x^2/(a + b*x)")]
        [InlineData("x/(a + b*x)")]
        public void TheSimplifierStillDoesNotDivideBySymbol(string expression)
        {
            var simplified = expression.ToEntity().Simplify();
            // A quotient of the form `x/b + ...` would have to have divided by the symbol; the
            // shape that keeps the parameter under the fraction has not.
            Assert.DoesNotContain("/ b", simplified.Stringize());
            Assert.DoesNotContain("/b", simplified.Stringize());
        }
    }
}
