//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Extensions;
using Xunit;
using static AngouriMath.Entity.Set;

namespace AngouriMath.Tests.Algebra
{
    /// <summary>
    /// An equation written as an equality <c>a = b</c> is solved as <c>a - b = 0</c> where an expression
    /// equal to zero is expected, as a statement's own solve reads it. Passed on as it stood, a system
    /// answered that it had no solution, and <see cref="MathS.SolveEquation"/> answered a set builder
    /// with a false condition in it, which is empty.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1673">#1673</a>
    /// </summary>
    [Trait("Area", "Algebra")]
    public sealed class AnEqualityReadAsADifferenceTest
    {
        [Theory]
        [InlineData("x + y = 3", "x - y = 1")]
        [InlineData("x + y = 3", "x - y - 1")]
        [InlineData("x + y - 3", "x - y = 1")]
        [InlineData("x = 3 - y", "y = x - 1")]
        public void ASystemOfEqualitiesIsSolved(string first, string second)
        {
            var solution = MathS.Equations(first.ToEntity(), second.ToEntity()).Solve("x", "y");
            Assert.NotNull(solution);
            Assert.Equal(1, solution!.RowCount);
            Assert.Equal(2, (int)solution[0, 0].EvalNumerical().RealPart);
            Assert.Equal(1, (int)solution[0, 1].EvalNumerical().RealPart);
        }

        [Fact]
        public void ASystemPrintsEachEquationAsADifferenceEqualToZero()
            => Assert.Equal("x + y - 3 = 0\nx - y - 1 = 0", MathS.Equations("x + y = 3".ToEntity(), "x - y = 1".ToEntity()).ToString());

        [Fact]
        public void OneEqualityIsSolvedInItsVariable()
            => Assert.Equal(new FiniteSet(2), MathS.SolveEquation("x + 1 = 3", "x"));

        [Fact]
        public void TheExtensionReadsAnEqualityTheSameWay()
            => Assert.Equal(new FiniteSet(-2, 2), "x^2 = 4".SolveEquation("x"));

        [Fact]
        public void AnEqualityWithAParameterIsSolvedForTheVariable()
            => Assert.Equal(new FiniteSet("b / a".ToEntity().InnerSimplified), "a * x = b".ToEntity().SolveEquation("x").InnerSimplified);
    }
}
