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

        /// <summary>
        /// The <see cref="Number.Rational"/> that a quotient of two integer literals denotes, or
        /// the node unchanged where it denotes none.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A <see cref="Number.Rational"/> prints as <c>7/2</c> and there is no rational literal
        /// in the grammar, so re-parsing gave a <see cref="Divf"/> and the round trip was not an
        /// identity — the value survived and the node did not. That is
        /// https://github.com/asc-community/AngouriMath/issues/946's neighbour,
        /// https://github.com/asc-community/AngouriMath/issues/873, answered there with "if you
        /// have two integers on both sides of the division, it is reasonable to try to parse it
        /// as a rational".
        /// </para>
        /// <para>
        /// <b>A quotient that reduces to an integer is left alone</b>, deliberately. Parsing is
        /// not simplification: turning <c>4/2</c> into <c>2</c> would discard what the caller
        /// wrote, and <c>4/2</c> already round-trips, being a <see cref="Divf"/> before and after.
        /// Only the non-integer case is what a <see cref="Number.Rational"/> can print as, so only
        /// it is what the round trip needs.
        /// </para>
        /// <para>
        /// This agrees with the normalisation rather than anticipating it:
        /// <c>Divf(1, 2).InnerSimplified</c> was already a <see cref="Number.Rational"/>, so the
        /// parser was the one step that disagreed.
        /// </para>
        /// <para>
        /// The codomain is not read here and not carried. Every caller either has no codomain to
        /// carry — the sweep over a finished tree, where an annotation cannot have reached a
        /// quotient — or is <c>domain(...)</c> itself, which applies its own afterwards.
        /// </para>
        /// </remarks>
        internal static Entity RationalLiteral(Entity node)
        {
            if (node is not Divf(Number.Integer numerator, Number.Integer denominator))
                return node;
            if (denominator.EInteger.IsZero)
                return node;
            var value = Number.Rational.Create(numerator.EInteger, denominator.EInteger);
            return value is Number.Integer ? node : value;
        }
    }
}
