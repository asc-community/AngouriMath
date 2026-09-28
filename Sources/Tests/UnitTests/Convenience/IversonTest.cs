//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Convenience
{
    /// <summary>
    /// The Iverson bracket <c>iverson(P)</c>: <c>1</c> where a statement holds and <c>0</c> where
    /// it does not, which is how a count is written as a sum.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1478">#1478</a>
    /// </summary>
    [Trait("Area", "Discrete")]
    public sealed class IversonTest
    {
        [Theory]
        [InlineData("iverson(3 divides 12)", "1")]
        [InlineData("iverson(5 divides 12)", "0")]
        [InlineData("iverson(2 > 3)", "0")]
        [InlineData("iverson(pi > 3)", "1")]
        [InlineData("iverson(1/2 in ZZ)", "0")]
        [InlineData("iverson(True) + iverson(False)", "1")]
        public void ADecidedStatementIsOneOrZero(string input, string expected)
            => Assert.Equal(expected.ToEntity(), input.ToEntity().Simplify());

        [Theory]
        [InlineData("iverson(x > 0)")]
        [InlineData("iverson(n divides 12)")]
        public void AnUndecidedStatementKeepsTheNode(string input)
            => Assert.IsType<Entity.Iversonf>(input.ToEntity().Simplify());

        /// <summary>
        /// A number is not a statement: <c>1</c> is not true, as a connective reads it.
        /// </summary>
        [Theory]
        [InlineData("iverson(1)")]
        [InlineData("iverson(0)")]
        public void ANumberIsNotAStatement(string input)
            => Assert.IsType<Entity.Iversonf>(input.ToEntity().Simplify());

        /// <summary>
        /// A statement with no truth value is <c>NaN</c>, and so is its bracket: the order
        /// comparison <c>i > 0</c> has no answer on the complex plane.
        /// </summary>
        [Fact]
        public void AStatementWithNoTruthValueHasNoBracket()
            => Assert.True("iverson(i > 0)".ToEntity().Simplify().IsNaN);

        [Fact]
        public void SubstitutingDecidesIt()
        {
            Assert.Equal("1".ToEntity(), "iverson(x > 0)".ToEntity().Substitute("x", 2).Simplify());
            Assert.Equal("0".ToEntity(), "iverson(x > 0)".ToEntity().Substitute("x", -2).Simplify());
        }

        /// <summary>
        /// A count is a sum of brackets. Sullivan and Mackey's Ex 8.3.18 draws four balls from
        /// three red, three blue and three green, and counts the outcomes as triples
        /// <c>(r, b, g)</c>: there are 12, where the book says 15.
        /// </summary>
        [Theory]
        [InlineData("sum(iverson(k divides 12), k, 1, 12)", "6")]
        [InlineData("sum(sum(sum(iverson(r + b + g = 4), g, 0, 3), b, 0, 3), r, 0, 3)", "12")]
        public void ACountIsASumOfBrackets(string input, string expected)
            => Assert.Equal(expected.ToEntity(), input.ToEntity().Simplify());

        [Theory]
        [InlineData("iverson(x > 0)", "iverson(x > 0)")]
        [InlineData("iverson(n divides 12) + 1", "iverson(n divides 12) + 1")]
        public void ThePrintedFormIsTheFunction(string input, string expected)
            => Assert.Equal(expected, input.ToEntity().Stringize());

        [Theory]
        [InlineData("iverson(x > 0)")]
        [InlineData("2 iverson(x > 0 and y > 0)")]
        public void ThePrintedFormParsesBackToTheSameExpression(string input)
            => Assert.Equal(input.ToEntity(), input.ToEntity().Stringize().ToEntity());

        /// <summary>
        /// The double bracket, which no vector, matrix or interval prints as.
        /// </summary>
        [Fact]
        public void TheLatexIsTheDoubleBracket()
            => Assert.Equal(@"[\![x > 0]\!]", "iverson(x > 0)".ToEntity().Latexize());

        [Fact]
        public void TheSympyCodeIsAPiecewise()
            => Assert.Contains("sympy.Piecewise((1, x > 0), (0, True))", MathS.ToSympyCode("iverson(x > 0)".ToEntity()));

        [Fact]
        public void TheEntryPointIsTheNode()
            => Assert.Equal("iverson(x > 0)".ToEntity(), MathS.Iverson("x > 0"));
    }
}
