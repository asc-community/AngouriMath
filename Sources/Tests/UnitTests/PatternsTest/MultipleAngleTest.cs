//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.PatternsTest
{
    /// <summary>
    /// sin(n x) and cos(n x) written out in sin(x) and cos(x). The expanded form is
    /// offered to the simplifier as a candidate, so what these pin is both that it is
    /// produced and that it is only preferred where the pieces cancel.
    /// </summary>
    [Trait("Area", "PatternsTest")]
    public sealed class MultipleAngleTest
    {
        // Identities that only close once the multiple angle is opened up.
        [Theory]
        [InlineData("sin(2 * x) - 2 * sin(x) * cos(x)")]
        [InlineData("cos(2 * x) - (1 - 2 * sin(x) ^ 2)")]
        [InlineData("cos(2 * x) - (cos(x) ^ 2 - sin(x) ^ 2)")]
        [InlineData("sin(4 * x) - 2 * sin(2 * x) * cos(2 * x)")]
        // Was pinned separately as PythagoreanPairAcrossASubtractionIsStillMissed, which
        // recorded that this form stops at 1 - cos(x)^2 - sin(x)^2 because sin^2 + cos^2 = 1
        // was matched only as an adjacent *sum* and here the pair sits either side of a
        // subtraction. The identity solved for one square rather than for 1 --
        // 1 - cos(:)^2 = sin(:)^2 -- closes that arrangement, so it belongs here now:
        // https://github.com/asc-community/AngouriMath/issues/725. Nothing about opening the
        // angle changed, and the note that this was not something the angle expansion could
        // reach was correct.
        [InlineData("cos(2 * x) - (2 * cos(x) ^ 2 - 1)")]
        public void IdentitiesReduceToZero(string input) =>
            Assert.Equal(Entity.Number.Integer.Create(0), input.ToEntity().Simplify());

        [Fact]
        public void DoubleAngleAndSquareCombine() =>
            Assert.Equal("cos(x) ^ 2".ToEntity(), "cos(2 * x) + sin(x) ^ 2".ToEntity().Simplify());

        // Left alone where opening it up only makes the expression longer.
        [Theory]
        [InlineData("sin(2 * x)")]
        [InlineData("cos(2 * x)")]
        [InlineData("sin(3 * x)")]
        public void CompactFormsAreKept(string input) =>
            Assert.Equal(input.ToEntity(), input.ToEntity().Simplify());

        // Whatever form comes out, it has to be the same function.
        [Theory]
        [InlineData("sin(2 * x)")]
        [InlineData("cos(2 * x)")]
        [InlineData("sin(3 * x)")]
        [InlineData("cos(4 * x)")]
        [InlineData("cos(2 * x) + sin(x) ^ 2")]
        public void ExpansionPreservesValue(string input)
        {
            var expr = input.ToEntity();
            var simplified = expr.Simplify();
            foreach (var point in new[] { 0.37, 1.41, 2.71 })
            {
                var before = expr.Substitute("x", point).EvalNumerical().RealPart.EDecimal.ToDouble();
                var after = simplified.Substitute("x", point).EvalNumerical().RealPart.EDecimal.ToDouble();
                Assert.Equal(before, after, 9);
            }
        }

        // Power reduction, sin(u)^2 = (1 - cos(2u)) / 2, in the integral table. Before it
        // was there, integrating sin(x)^2 fell through to integration by parts and cycled.
        [Theory]
        [InlineData("sin(x) ^ 2")]
        [InlineData("cos(x) ^ 2")]
        [InlineData("sin(2 * x + 1) ^ 2")]
        [InlineData("cos(3 * x) ^ 2")]
        public void SquaresOfSineAndCosineIntegrate(string integrand)
        {
            var f = integrand.ToEntity();
            var antiderivative = f.Integrate("x");
            Assert.DoesNotContain("integral(", antiderivative.Stringize());
            var derivative = antiderivative.Substitute("C", 0).Differentiate("x");
            foreach (var point in new[] { 0.37, 1.41, 2.71 })
            {
                var expected = f.Substitute("x", point).EvalNumerical().RealPart.EDecimal.ToDouble();
                var actual = derivative.Substitute("x", point).EvalNumerical().RealPart.EDecimal.ToDouble();
                Assert.Equal(expected, actual, 8);
            }
        }
    }
}
