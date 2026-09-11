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
    /// Partial fractions over a denominator <b>written</b> as a product of distinct linear and
    /// quadratic factors whose coefficients are symbols, by undetermined coefficients.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>1/((x + 3)(x^2 + 5))</c> came out and <c>1/((x + a)(x^2 + b))</c> did not: the split
    /// over the rationals cannot read a symbol, and no other split existed. The factors were
    /// already in hand and each is a shape the integrator answers on its own.
    /// </para>
    /// <para>
    /// The decomposition solves a linear system whose entries are the parameters, and a
    /// symbolic pivot is a judgement rather than a proof — so the identity is checked at
    /// sampled points with the symbols pinned before anything is returned, and these tests
    /// check the antiderivative the same way. Where a piece lands on the quadratic rule with an
    /// undecidable discriminant the answer is a piecewise, which is correct and is what the
    /// differentiate-back check sees through numerically.
    /// </para>
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class SymbolicFactorsPartialFractionsTest
    {
        private static readonly double[] Points = { 0.31, 0.77, 1.43, 2.19 };

        private static void DifferentiatesBack(string integrand, string variable, params (string, double)[] pins)
        {
            var integral = integrand.ToEntity().Integrate(variable);
            Assert.DoesNotContain("integral(", integral.Stringize());
            Assert.DoesNotContain("NaN", integral.Stringize());

            var derivative = integral.Substitute("C", 0).Differentiate(variable);
            Entity original = integrand.ToEntity();
            foreach (var (name, value) in pins)
            {
                derivative = derivative.Substitute(name, value);
                original = original.Substitute(name, value);
            }

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
        /// A linear factor beside a quadratic one, and two quadratics, with a symbol in each.
        /// </summary>
        [Theory]
        [InlineData("1/((x + a)*(x^2 + b))")]
        [InlineData("1/((x^2 + a)*(x + 1))")]
        [InlineData("1/((a + x^2)*(b + x^2))")]
        [InlineData("1/((x^2 + 4)*(x^2 + b))")]
        [InlineData("x/((a^2 + x^2)*(b^2 + x^2))")]
        [InlineData("(x + 1)/((x + a)*(x^2 + b))")]
        [InlineData("x^2/((x + a)*(x^2 + b))")]
        public void DistinctSymbolicFactors(string integrand)
            => DifferentiatesBack(integrand, "x", ("a", 1.7), ("b", 2.3));

        /// <summary>
        /// Three factors, so the system is larger than one pair and the elimination has to
        /// pivot past a symbol.
        /// </summary>
        [Theory]
        [InlineData("1/((x + a)*(x + b)*(x^2 + 1))")]
        [InlineData("x/((x + a)*(x^2 + b)*(x^2 + 2))")]
        public void ThreeFactors(string integrand)
            => DifferentiatesBack(integrand, "x", ("a", 1.7), ("b", 2.3));

        /// <summary>
        /// A parameter that multiplies the variable, and a factor with a constant in front of
        /// the product — Moses's <c>-B (A^2 + B^2) / ((1 + w^2)(B^2 - A^2 w^2))</c> from the
        /// Rubi suite, integrated in <c>w</c>.
        /// </summary>
        [Theory]
        [InlineData("1/((1 + w^2)*(B^2 - A^2*w^2))")]
        [InlineData("-B*(A^2 + B^2)/((1 + w^2)*(B^2 - A^2*w^2))")]
        public void AParameterOnTheVariable(string integrand)
            => DifferentiatesBack(integrand, "w", ("A", 0.6), ("B", 1.9));

        /// <summary>
        /// The numeric spellings, which the split over the rationals still takes first and
        /// which keep their exact answers.
        /// </summary>
        [Theory]
        [InlineData("1/((x + 3)*(x^2 + 5))")]
        [InlineData("1/((3 + x^2)*(5 + x^2))")]
        public void TheNumericSpellingsStillAnswer(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x").Stringize();
            Assert.DoesNotContain("piecewise", integral);
            DifferentiatesBack(integrand, "x");
        }

        /// <summary>
        /// A repeated factor is not this rule's — there is nothing for a symbolic repeated
        /// quadratic to land on — and a denominator not written as a product is left to the
        /// other splits.
        /// </summary>
        [Theory]
        [InlineData("1/((x^2 + a)^2*(x + 1))")]
        public void ARepeatedSymbolicQuadraticIsDeclined(string integrand)
            => Assert.Contains("integral(", integrand.ToEntity().Integrate("x").Stringize());
    }
}
