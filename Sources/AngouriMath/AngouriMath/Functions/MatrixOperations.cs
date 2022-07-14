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
#pragma warning disable CS0618
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
            _ => throw new AngouriBugException("Unhandled case")
        };

        var expectedSize = matrices[0].Shape[axis1];

        var totalSize2 = 0;
        foreach (var matrix in matrices)
        {
            if (matrix.Shape[axis1] != expectedSize)
                throw new BadMatrixShapeException($"Expected size of {expectedSize} but got {matrix.Shape[axis1]} instead");
            totalSize2 += matrix.Shape[axis2];
        }

        var result = GenTensor<Entity, Matrix.EntityTensorWrapperOperations>.CreateMatrix(
            expectedSize * axis2 + totalSize2 * axis1,
            expectedSize * axis1 + totalSize2 * axis2
        );

        return dir switch
        {
            MathS.Matrices.Direction.Horizontal => ConcatHorizontal(result, matrices),
            MathS.Matrices.Direction.Vertical => ConcatVertical(result, matrices),
            _ => throw new AngouriBugException("Unhandled case")
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
#pragma warning restore CS0618