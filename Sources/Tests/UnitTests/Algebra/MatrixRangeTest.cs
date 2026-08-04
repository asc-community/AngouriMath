//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using AngouriMath;
using AngouriMath.Core.Exceptions;
using Xunit;

namespace AngouriMath.Tests.Algebra
{
    /// <summary>
    /// Taking part of a matrix by range, as asked for in
    /// https://github.com/asc-community/AngouriMath/issues/443.
    /// </summary>
    public sealed class MatrixRangeTest
    {
        private static readonly Entity.Matrix ThreeByThree = MathS.Matrices.Matrix(3, 3,
            1, 2, 3,
            4, 5, 6,
            7, 8, 9
        );

        private static readonly Entity.Matrix Column = MathS.Matrices.Vector(1, 2, 3);

        [Fact]
        public void EveryRowButTheFirst() =>
            Assert.Equal(MathS.Matrices.Matrix(2, 3,
                4, 5, 6,
                7, 8, 9), ThreeByThree[1.., ..]);

        [Fact]
        public void EveryColumnButTheLast() =>
            Assert.Equal(MathS.Matrices.Matrix(3, 2,
                1, 2,
                4, 5,
                7, 8), ThreeByThree[.., ..2]);

        [Fact]
        public void AnInnerBlock() =>
            Assert.Equal(MathS.Matrices.Matrix(2, 2,
                5, 6,
                8, 9), ThreeByThree[1..3, 1..3]);

        [Fact]
        public void CountedFromTheEnd() =>
            Assert.Equal(MathS.Matrices.Matrix(1, 1, 9), ThreeByThree[^1.., ^1..]);

        [Fact]
        public void TheWholeMatrix() =>
            Assert.Equal(ThreeByThree, ThreeByThree[.., ..]);

        // A vector is a matrix of one column, so the second extent is what a vector is cut by
        // rows with, and it has only the one column to ask for.
        [Fact]
        public void PartOfAVector() =>
            Assert.Equal(MathS.Matrices.Vector(2, 3), Column[1.., ..]);

        // The result is a matrix in its own right and nothing of the original is shared with
        // it, so what is read out of it does not depend on the order the two were built in.
        [Fact]
        public void TheResultIsAMatrixLikeAnyOther()
        {
            var block = ThreeByThree[..2, 1..];
            Assert.Equal(2, block.RowCount);
            Assert.Equal(2, block.ColumnCount);
            Assert.Equal((Entity)6, block[1, 1]);
            Assert.Equal(MathS.Matrices.Matrix(2, 2,
                2, 5,
                3, 6), block.T);
        }

        [Fact]
        public void SymbolicEntriesAreCarriedOverUntouched() =>
            Assert.Equal(
                MathS.Matrices.Matrix(1, 2, "y", "x + 1"),
                MathS.Matrices.Matrix(2, 2,
                    "x", "y",
                    "y", "x + 1")[1.., ..][.., ..]);

        [Fact]
        public void AnEmptyRangeIsRefused()
        {
            Assert.Throws<InvalidMatrixOperationException>(() => ThreeByThree[1..1, ..]);
            Assert.Throws<InvalidMatrixOperationException>(() => ThreeByThree[.., 2..2]);
        }

        [Fact]
        public void ARangeReachingPastTheMatrixIsRefused()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ThreeByThree[..4, ..]);
            Assert.Throws<ArgumentOutOfRangeException>(() => ThreeByThree[.., 1..5]);
        }
    }
}
