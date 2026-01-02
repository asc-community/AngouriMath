//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Exceptions;
using static AngouriMath.Entity;

namespace AngouriMath.Core.Antlr
{
    internal static class ParsingHelpers
    {
        internal static Matrix TryBuildingMatrix(List<Entity> elements)
        {
            if (!elements.Any())
                return MathS.Vector(elements.ToArray());
            var first = elements.First();
            if (first is not Matrix { IsVector: true } firstVec)
                return MathS.Vector(elements.ToArray());
            var tb = new MatrixBuilder(firstVec.RowCount);
            foreach (var row in elements)
            {
                if (row is not Matrix { IsVector: true } rowVec)
                    return MathS.Vector(elements.ToArray());
                if (rowVec.RowCount != firstVec.RowCount)
                    return MathS.Vector(elements.ToArray());
                tb.Add(rowVec);
            }
            return tb.ToMatrix() ?? throw new AngouriBugException("Should've been checked already");
        }
    }
}
