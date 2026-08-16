//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using Xunit;
using static AngouriMath.Entity.Number;
using static AngouriMath.MathS;

namespace AngouriMath.Tests.Convenience
{
    /// <summary>
    /// The functions of <a href="https://github.com/asc-community/AngouriMath/issues/321">#321</a>
    /// that need no node of their own, because the kernel can already say what they mean.
    /// </summary>
    /// <remarks>
    /// The point of these tests is less that the formulas are right — they are one line each —
    /// than that being an ordinary expression is what buys everything else. Nothing below teaches
    /// the library to differentiate a sigmoid or take its limit; those work because a sigmoid is
    /// a quotient of things it already knew.
    /// </remarks>
    [Trait("Area", "Convenience")]
    public sealed class NonKernelFunctionsTest
    {
        [Fact]
        public void ASigmoidIsTheLogisticExpression() =>
            Assert.Equal(MathS.Boolean.True,
                Sigmoid("x").EqualTo(1 / (1 + Pow(e, -Var("x")))).Simplify());

        [Fact]
        public void ASigmoidIsAHalfAtZero() =>
            Assert.Equal((Entity)Rational.Create(1, 2), Sigmoid(0).EvalNumerical());

        /// <summary>
        /// The whole argument for defining it this way: nothing here was implemented for the
        /// sigmoid, and all of it works.
        /// </summary>
        [Theory]
        [InlineData("+oo", "1")]
        [InlineData("-oo", "0")]
        public void ASigmoidSaturatesBecauseItIsAnOrdinaryExpression(string destination, string expected) =>
            Assert.Equal(expected.ToEntity().Evaled,
                Sigmoid(Var("x")).Limit("x", destination.ToEntity()).Simplify().Evaled);

        [Fact]
        public void ASigmoidDifferentiatesWithoutBeingTaughtTo()
        {
            var derivative = Sigmoid(Var("x")).Differentiate("x").Simplify();
            Assert.DoesNotContain(derivative.Nodes, node => node is Entity.Derivativef);
            Assert.NotEqual(MathS.NaN, derivative);
        }

        [Fact]
        public void AnAverageOfNumbersIsTheirMean() =>
            Assert.Equal((Entity)2, Average(Vector(1, 2, 3)).Simplify());

        /// <summary>Symbolic, so it averages what it is given rather than what it can evaluate.</summary>
        [Fact]
        public void AnAverageOfSymbolsStaysSymbolic() =>
            Assert.Equal(MathS.Boolean.True,
                Average(Vector("a", "b")).EqualTo((Var("a") + Var("b")) / 2).Simplify());

        [Fact]
        public void AnAverageReadsEveryCellOfAMatrix() =>
            Assert.Equal((Entity)Rational.Create(5, 2),
                Average(Matrix(new Entity[,] { { 1, 2 }, { 3, 4 } })).Simplify());

        [Fact]
        public void AVectorsLengthIsEuclidean() =>
            Assert.Equal((Entity)5, VectorLength(Vector(3, 4)).Simplify());

        /// <summary>
        /// It is a name for <see cref="MathS.Abs(Entity)"/> rather than a second implementation,
        /// so the two must not be able to disagree.
        /// </summary>
        [Fact]
        public void AVectorsLengthIsExactlyItsAbs()
        {
            var v = Vector(1, 2, 3);
            Assert.Equal(Abs(v).Simplify(), VectorLength(v).Simplify());
        }

        [Fact]
        public void NullIsRefusedRatherThanDereferenced()
        {
            Assert.Throws<System.ArgumentNullException>(() => Average(null!));
            Assert.Throws<System.ArgumentNullException>(() => VectorLength(null!));
        }
    }
}
