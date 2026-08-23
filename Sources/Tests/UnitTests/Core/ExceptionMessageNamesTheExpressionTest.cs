//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Exceptions;
using AngouriMath.Extensions;
using Xunit;
using static AngouriMath.Entity;

namespace AngouriMath.Tests.Core
{
    /// <summary>
    /// A refusal that does not say what it choked on is barely a refusal: "I could not settle
    /// this" leaves a caller with nothing to try instead and a contributor with nothing to
    /// build. These pin the sub-expression, shape or value in the messages a caller can reach,
    /// so that a later edit cannot quietly take it back out again.
    /// https://github.com/asc-community/AngouriMath/issues/746
    /// </summary>
    [Trait("Area", "Core")]
    public sealed class ExceptionMessageNamesTheExpressionTest
    {
        /// <summary>
        /// ANTLR's diagnostic is a line and a column, which names no input once the string has
        /// travelled through a caller's own layers. The location stays at the front, since
        /// that is what the message is read for first.
        /// </summary>
        [Theory]
        [InlineData("()")]
        [InlineData("+!")]
        [InlineData("a*a_")]
        public void AParseFailureNamesTheInput(string input)
        {
            var thrown = Assert.Throws<UnhandledParseException>(() => (Entity)input);
            Assert.StartsWith("line ", thrown.Message);
            Assert.Contains(input, thrown.Message);
        }

        [Theory]
        [InlineData("x + 1")]
        [InlineData("sin(a) * b")]
        public void ANumericEvaluationFailureNamesTheExpression(string expression)
        {
            var thrown = Assert.Throws<CannotEvalException>(() => expression.ToEntity().EvalNumerical());
            Assert.Contains(expression.ToEntity().Stringize(), thrown.Message);
        }

        [Fact]
        public void ABooleanEvaluationFailureNamesTheExpression()
        {
            var expression = "a and b".ToEntity();
            var thrown = Assert.Throws<CannotEvalException>(() => expression.EvalBoolean());
            Assert.Contains(expression.Stringize(), thrown.Message);
        }

        /// <summary>
        /// The shapes are the whole content of the complaint, and neither of them was in the
        /// message before.
        /// </summary>
        [Fact]
        public void AddingMatricesOfDifferentShapesNamesBothShapes()
        {
            var thrown = Assert.Throws<InvalidMatrixOperationException>(
                () => (Matrix)"[1, 2]".ToEntity() + (Matrix)"[1, 2, 3]".ToEntity());
            Assert.Contains("2", thrown.Message);
            Assert.Contains("3", thrown.Message);
        }

        [Fact]
        public void AsScalarOnSomethingLargerNamesItsShape()
        {
            var matrix = (Matrix)"[[1, 2], [3, 4]]".ToEntity();
            var thrown = Assert.Throws<InvalidMatrixOperationException>(() => matrix.AsScalar());
            Assert.Contains("2x2", thrown.Message);
        }

        [Fact]
        public void RaisingANonSquareMatrixToAPowerNamesItsShape()
        {
            var matrix = (Matrix)"[[1, 2, 3], [4, 5, 6]]".ToEntity();
            var thrown = Assert.Throws<InvalidMatrixOperationException>(() => matrix.Pow(2));
            Assert.Contains("2x3", thrown.Message);
        }

        /// <summary>
        /// Membership of a symbolic element in a symbolic set is not always decidable, and
        /// which element and which set were being asked about is what the caller needs.
        /// </summary>
        [Fact]
        public void AnUndecidableMembershipNamesTheElementAndTheSet()
        {
            var set = (Set)"{ x : x > a }".ToEntity();
            var thrown = Assert.Throws<ElementInSetAmbiguousException>(() => set.Contains(1));
            Assert.Contains("1", thrown.Message);
            Assert.Contains("a", thrown.Message);
        }

        [Theory]
        [InlineData("derivative(x, x)")]
        [InlineData("integral(x, x)")]
        [InlineData("limit(x, x, 0)")]
        public void ANodeWithNoCompiledFormNamesTheNode(string expression)
        {
            var thrown = Assert.Throws<UncompilableNodeException>(
                () => expression.ToEntity().Compile("x"));
            Assert.Contains(expression.ToEntity().Stringize(), thrown.Message);
        }

        [Fact]
        public void ABaseWithNoDigitsForItNamesTheBase()
        {
            var thrown = Assert.Throws<InvalidNumericSystemException>(() => MathS.ToBaseN(1.5m, 500));
            Assert.Contains("500", thrown.Message);
        }

        /// <summary>
        /// The number of variables the expression actually has is the information that turns
        /// this from a puzzle into a correction.
        /// </summary>
        [Fact]
        public void ATruthTableOverTheWrongVariablesNamesTheExpressionAndItsVariables()
        {
            var expression = "a and b".ToEntity();
            var thrown = Assert.Throws<WrongNumberOfArgumentsException>(
                () => MathS.Boolean.BuildTruthTable(expression, "a"));
            Assert.Contains(expression.Stringize(), thrown.Message);
            Assert.Contains("b", thrown.Message);
        }

        /// <summary>
        /// A row of the wrong width used to come back as "Incorrect usage of MatrixBuilder",
        /// which says neither width.
        /// </summary>
        [Fact]
        public void ARowOfTheWrongWidthNamesBothWidths()
        {
            var thrown = Assert.Throws<InvalidMatrixOperationException>(
                () => MathS.MatrixFromIEnum2x2(new Entity[][]
                {
                    new Entity[] { 1, 2, 3 },
                    new Entity[] { 4, 5 }
                }));
            Assert.Contains("2", thrown.Message);
            Assert.Contains("3", thrown.Message);
        }

        /// <summary>
        /// Every message here is reached only when the exception is actually constructed:
        /// an interpolation written inside the <c>throw new</c> expression costs the paths
        /// that do not throw nothing at all.
        /// </summary>
        [Fact]
        public void TheImprovedPathsStillSucceedWhenNothingIsWrong()
        {
            Assert.Equal(2, "1 + 1".ToEntity().EvalNumerical());
            Assert.Equal(Entity.Boolean.True, "true and true".ToEntity().EvalBoolean());
            Assert.Equal("[4, 6]".ToEntity(), (Matrix)"[1, 2]".ToEntity() + (Matrix)"[3, 4]".ToEntity());
            Assert.Equal((Entity)1, ((Matrix)"[1]".ToEntity()).AsScalar());
        }
    }
}
