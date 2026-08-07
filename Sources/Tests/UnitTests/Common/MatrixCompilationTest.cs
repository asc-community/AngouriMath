//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Core.Exceptions;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Common
{
    /// <summary>
    /// Compiling expressions that mention matrices. A matrix has no compiled form, but an
    /// expression built out of matrices often has a value that is an ordinary number.
    /// </summary>
    [Trait("Area", "Common")]
    public sealed class MatrixCompilationTest
    {
        // https://github.com/asc-community/AngouriMath/issues/425
        // This is the issue's own example of what ought to work, and did not: it is the
        // scalar c, but nothing simplified before compiling, so it reached the matrix node
        // and threw.
        [Fact]
        public void ScalarBuiltFromMatricesCompiles()
        {
            var compiled = "[0, 1]T * [[a, b], [c, d]] * [1, 0]".ToEntity()
                .Compile<double, double, double, double, double>("a", "b", "c", "d");
            Assert.Equal(3, compiled(1, 2, 3, 4), 9);
        }

        [Fact]
        public void ScalarProductOfVectorsCompiles()
        {
            var compiled = "[1, 2]T * [a, b]".ToEntity().Compile<double, double, double>("a", "b");
            Assert.Equal(1 * 3 + 2 * 4, compiled(3, 4), 9);
        }

        [Theory]
        [InlineData("[[a, b], [c, d]]")]
        [InlineData("[[a, b], [c, d]] * [1, 0]")]
        [InlineData("[a, b]")]
        public void MatrixValuedExpressionsSayTheyCannotBeCompiled(string expression)
        {
            var thrown = Assert.Throws<UncompilableNodeException>(
                () => expression.ToEntity().Compile<double, double>("a"));
            Assert.Contains("Matrix", thrown.Message);
        }

        // Expressions with no matrix in them must be unaffected -- they do not go anywhere
        // near the extra simplification.
        [Theory]
        [InlineData("x + 1", 2.0, 3.0)]
        [InlineData("x ^ 2", 3.0, 9.0)]
        [InlineData("sin(x)", 0.0, 0.0)]
        public void OrdinaryExpressionsAreUnaffected(string expression, double argument, double expected) =>
            Assert.Equal(expected, expression.ToEntity().Compile<double, double>("x")(argument), 9);
    }
}
