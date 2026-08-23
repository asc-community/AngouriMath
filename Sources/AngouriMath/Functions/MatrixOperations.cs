//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Exceptions;
using GenericTensor.Core;
using static AngouriMath.Entity;

namespace AngouriMath.Core;
internal static class MatrixOperations
{
    internal static Matrix Concat(MathS.Matrices.Direction dir, params Matrix[] matrices)
    {
        if (matrices.Length == 0)
            throw new WrongNumberOfArgumentsException("Cannot concat 0 matrix");
        if (matrices.Length == 1)
            return matrices[0];
        var (axis1, axis2) = dir switch
        {
            MathS.Matrices.Direction.Vertical => (1, 0),
            MathS.Matrices.Direction.Horizontal => (0, 1),
            _ => throw new AngouriBugException($"Unhandled concatenation direction {dir}")
        };

        // Axis 0 is rows and axis 1 is columns, which is what the removed Shape indexer
        // meant by those positions.
        static int SizeAlong(Matrix matrix, int axis) => axis is 0 ? matrix.RowCount : matrix.ColumnCount;

        var expectedSize = SizeAlong(matrices[0], axis1);

        var totalSize2 = 0;
        foreach (var matrix in matrices)
        {
            if (SizeAlong(matrix, axis1) != expectedSize)
                throw new BadMatrixShapeException(
                    $"Concatenating {dir}ly needs every matrix to have {expectedSize} along axis {axis1}, "
                    + $"and `{matrix.Stringize()}` is {matrix.RowCount}x{matrix.ColumnCount}");
            totalSize2 += SizeAlong(matrix, axis2);
        }

        var result = GenTensor<Entity, Matrix.EntityTensorWrapperOperations>.CreateMatrix(
            expectedSize * axis2 + totalSize2 * axis1,
            expectedSize * axis1 + totalSize2 * axis2
        );

        return dir switch
        {
            MathS.Matrices.Direction.Horizontal => ConcatHorizontal(result, matrices),
            MathS.Matrices.Direction.Vertical => ConcatVertical(result, matrices),
            _ => throw new AngouriBugException($"Unhandled concatenation direction {dir}")
        };
    }

    internal static Matrix ConcatHorizontal(GenTensor<Entity, Matrix.EntityTensorWrapperOperations> res, Matrix[] matrices)
    {
        var yOffset = 0;
        foreach (var matrix in matrices)
        {
            for (int y = 0; y < matrix.ColumnCount; y++)
            for (int x = 0; x < matrix.RowCount; x++)
#if DEBUG
                res[x, y + yOffset] = matrix[x, y];
#else
                res.SetValueNoCheck(matrix[x, y], x, y + yOffset);
#endif
            yOffset += matrix.ColumnCount;
        }

        return new Matrix(res);
    }
    
    internal static Matrix ConcatVertical(GenTensor<Entity, Matrix.EntityTensorWrapperOperations> res, Matrix[] matrices)
    {
        var xOffset = 0;
        foreach (var matrix in matrices)
        {
            for (int x = 0; x < matrix.RowCount; x++)
            for (int y = 0; y < matrix.ColumnCount; y++)
#if DEBUG
                res[x + xOffset, y] = matrix[x, y];
#else
                res.SetValueNoCheck(matrix[x, y], x + xOffset, y);
#endif
            xOffset += matrix.RowCount;
        }

        return new Matrix(res);
    }
}
