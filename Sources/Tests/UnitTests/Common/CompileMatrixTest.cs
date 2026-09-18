//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Numerics;
using AngouriMath;
using AngouriMath.Core.Compilation.IntoLinq;
using AngouriMath.Core.Exceptions;
using AngouriMath.Extensions;
using GenericTensor.Core;
using Xunit;

namespace AngouriMath.Tests.Common
{
    /// <summary>
    /// A matrix compiles to a <see cref="GenTensor{T, TWrapper}"/> over its elements' type, and
    /// the arithmetic on it to GenericTensor's operations: matrices are values a compiled
    /// delegate takes and returns. <a href="https://github.com/asc-community/AngouriMath/issues/526">#526</a>
    /// </summary>
    [Trait("Area", "Common")]
    public sealed class CompileMatrixTest
    {
        private static GenTensor<double, DoubleOperations> Doubles(params double[][] rows)
            => GenTensor<double, DoubleOperations>.CreateMatrix(rows.Length, rows[0].Length, (r, c) => rows[r][c]);

        private static void AssertSame(GenTensor<double, DoubleOperations> expected, GenTensor<double, DoubleOperations> actual)
        {
            Assert.Equal(expected.Shape, actual.Shape);
            for (var r = 0; r < expected.Shape[0]; r++)
                for (var c = 0; c < expected.Shape[1]; c++)
                    Assert.Equal(expected[r, c], actual[r, c], 12);
        }

        [Fact]
        public void AMatrixOfExpressionsCompilesToATensor()
        {
            var f = "[[x, 2 x], [x + 1, x^2]]".ToEntity().Compile<double, GenTensor<double, DoubleOperations>>("x");
            AssertSame(Doubles(new[] { 3.0, 6.0 }, new[] { 4.0, 9.0 }), f(3));
        }

        [Fact]
        public void TheProductOfTwoMatrixArgumentsIsTheMatrixProduct()
        {
            var f = "A * B".ToEntity().Compile<GenTensor<double, DoubleOperations>, GenTensor<double, DoubleOperations>, GenTensor<double, DoubleOperations>>("A", "B");
            var a = Doubles(new[] { 1.0, 2.0 }, new[] { 3.0, 4.0 });
            var b = Doubles(new[] { 0.0, 1.0 }, new[] { 1.0, 0.0 });
            AssertSame(Doubles(new[] { 2.0, 1.0 }, new[] { 4.0, 3.0 }), f(a, b));
        }

        [Fact]
        public void ScalarsAndMatricesMix()
        {
            var f = "2 A + B - 1".ToEntity().Compile<GenTensor<double, DoubleOperations>, GenTensor<double, DoubleOperations>, GenTensor<double, DoubleOperations>>("A", "B");
            var a = Doubles(new[] { 1.0, 2.0 }, new[] { 3.0, 4.0 });
            var b = Doubles(new[] { 10.0, 10.0 }, new[] { 10.0, 10.0 });
            AssertSame(Doubles(new[] { 11.0, 13.0 }, new[] { 15.0, 17.0 }), f(a, b));
        }

        [Fact]
        public void AScalarVariableScalesALiteralMatrix()
        {
            var f = "x * [[1, 2], [3, 4]] / 2".ToEntity().Compile<double, GenTensor<double, DoubleOperations>>("x");
            AssertSame(Doubles(new[] { 2.0, 4.0 }, new[] { 6.0, 8.0 }), f(4));
        }

        [Fact]
        public void AMatrixPowerIsTheMatrixPower()
        {
            var f = "A ^ 3".ToEntity().Compile<GenTensor<double, DoubleOperations>, GenTensor<double, DoubleOperations>>("A");
            var a = Doubles(new[] { 1.0, 1.0 }, new[] { 0.0, 1.0 });
            AssertSame(Doubles(new[] { 1.0, 3.0 }, new[] { 0.0, 1.0 }), f(a));
        }

        [Fact]
        public void AnExpressionOfMatricesWithAScalarValueStillCompilesToTheScalar()
        {
            // #425's case: the matrix is simplified away before compiling, as before.
            var f = "[[1, 0]] * [[a, b], [c, d]] * [[0], [1]]".ToEntity().Compile<double, double, double, double, double>("a", "b", "c", "d");
            Assert.Equal(2.0, f(1, 2, 3, 4));
        }

        [Fact]
        public void ComplexAndIntegerElementsHaveTheirOwnTensors()
        {
            var g = "[[x, i x]]".ToEntity().Compile<Complex, GenTensor<Complex, ComplexOperations>>("x");
            var z = g(new Complex(1, 1));
            Assert.Equal(new Complex(1, 1), z[0, 0]);
            Assert.Equal(new Complex(-1, 1), z[0, 1]);
            var h = "[[n, 2 n]]".ToEntity().Compile<long, GenTensor<long, Int64Operations>>("n");
            Assert.Equal(6L, h(3)[0, 1]);
            Assert.Equal(typeof(GenTensor<double, DoubleOperations>), TensorCompilation.TensorTypeOf(typeof(double)));
            Assert.Null(TensorCompilation.TensorTypeOf(typeof(string)));
        }

        [Fact]
        public void AMatrixIsNotANumberAndSaysSo()
        {
            Assert.Throws<UncompilableNodeException>(() => "[[x, 2 x]]".ToEntity().Compile<double, double>("x"));
            Assert.Throws<UncompilableNodeException>(() => "sin(A)".ToEntity().Compile<GenTensor<double, DoubleOperations>, GenTensor<double, DoubleOperations>>("A"));
        }
    }
}
