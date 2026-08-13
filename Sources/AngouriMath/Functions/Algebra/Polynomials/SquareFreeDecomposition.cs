//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

namespace AngouriMath.Functions
{
    /// <summary>
    /// Splits a polynomial into square-free parts: <c>f = a_1 * a_2^2 * a_3^3 * ...</c> with
    /// each <c>a_i</c> square-free and any two of them coprime.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The first step of every factorisation, and useful on its own. A repeated factor is
    /// exactly a factor shared with the derivative — <c>(x - r)^k</c> contributes
    /// <c>(x - r)^(k-1)</c> to <c>f'</c> — so <c>gcd(f, f')</c> collects every repetition and
    /// dividing by it leaves each distinct factor standing once. That is why factoring only
    /// ever has to deal with square-free input, and why a square-free routine is worth having
    /// before the factoriser that consumes it.
    /// </para>
    /// <para>
    /// Yun's algorithm rather than the older Tobey-Horowitz one: both start from
    /// <c>gcd(f, f')</c>, but Yun's carries the derivative quotient forward so that each
    /// round's greatest common divisor is taken between polynomials whose degrees have
    /// already fallen, instead of re-dividing the original every time. The cost is one extra
    /// subtraction a round and the saving is an order of magnitude on high multiplicities.
    /// Yun, <i>On square-free decomposition algorithms</i>, SYMSAC '76; Geddes, Czapor and
    /// Labahn, <i>Algorithms for Computer Algebra</i>, §8.1.
    /// </para>
    /// <para>
    /// Characteristic zero only. Over <c>F_p</c> the derivative of <c>x^p</c> vanishes and
    /// the argument above breaks, which is why the finite-field side handles its own
    /// square-free step rather than calling this.
    /// </para>
    /// </remarks>
    internal static class SquareFreeDecomposition
    {
        /// <summary>
        /// The square-free parts of <paramref name="poly"/> paired with the multiplicity each
        /// carries, or <see langword="null"/> where a step declined. Parts of multiplicity
        /// with nothing in them are omitted, so the list is never padded with constants.
        /// </summary>
        /// <remarks>
        /// The product of the parts raised to their multiplicities is checked against the
        /// primitive part of the input before the answer is handed back. A decomposition that
        /// does not multiply back is refused rather than returned: the factoriser above this
        /// treats each part as a complete account of one multiplicity, and a part that is
        /// missing a factor would silently drop it from the final answer.
        /// </remarks>
        internal static IReadOnlyList<SquareFreePart>? Decompose(IntegerPolynomial poly)
        {
            if (poly.IsZero)
                return null;
            var primitive = poly.PrimitivePart();
            if (primitive.IsConstant)
                return System.Array.Empty<SquareFreePart>();

            var derivative = primitive.Derivative();
            // Only a constant has a vanishing derivative in characteristic zero, and that
            // was answered above.
            if (derivative.IsZero)
                return null;

            var repeated = IntegerPolynomial.Gcd(primitive, derivative);
            if (primitive.DivideExact(repeated) is not { } distinct
                || derivative.DivideExact(repeated) is not { } quotient)
                return null;
            var difference = quotient.Subtract(distinct.Derivative());

            var parts = new List<SquareFreePart>();
            for (var multiplicity = 1; multiplicity <= IntegerPolynomial.MaxDegree; multiplicity++)
            {
                if (distinct.IsConstant)
                {
                    // Every part accounted for; the leftover is the unit that PrimitivePart
                    // already normalised away.
                    return Verify(parts, primitive) ? parts : null;
                }

                var part = IntegerPolynomial.Gcd(distinct, difference);
                if (distinct.DivideExact(part) is not { } nextDistinct
                    || difference.DivideExact(part) is not { } nextQuotient)
                    return null;

                if (!part.IsConstant)
                    parts.Add(new SquareFreePart(part.PrimitivePart(), multiplicity));

                distinct = nextDistinct;
                difference = nextQuotient.Subtract(nextDistinct.Derivative());
            }
            return null;
        }

        /// <summary>
        /// Whether the parts multiply back to what they came from, up to the sign that
        /// <see cref="IntegerPolynomial.PrimitivePart"/> fixes on both sides.
        /// </summary>
        private static bool Verify(IReadOnlyList<SquareFreePart> parts, IntegerPolynomial primitive)
        {
            var product = IntegerPolynomial.One;
            foreach (var part in parts)
                for (var i = 0; i < part.Multiplicity; i++)
                {
                    if (product.Multiply(part.Factor) is not { } multiplied)
                        return false;
                    product = multiplied;
                }
            return product.PrimitivePart().SameAs(primitive);
        }

        /// <summary>One square-free part of a decomposition, and the power it is raised to.</summary>
        internal readonly struct SquareFreePart
        {
            internal SquareFreePart(IntegerPolynomial factor, int multiplicity)
            {
                Factor = factor;
                Multiplicity = multiplicity;
            }

            internal IntegerPolynomial Factor { get; }

            internal int Multiplicity { get; }
        }
    }
}
