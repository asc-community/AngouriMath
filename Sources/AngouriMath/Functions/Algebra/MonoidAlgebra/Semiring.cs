//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

namespace AngouriMath.Functions.Algebra.MonoidAlgebra
{
    using static Entity;

    /// <summary>
    /// The arithmetic the coefficients of a <see cref="SparseTerms{TBasis}"/> obey.
    /// </summary>
    /// <remarks>
    /// Coefficients are always <see cref="Entity"/>; what changes between features is what
    /// adding and multiplying two of them means. A polynomial adds them in a field, a boolean
    /// expression joins them in a lattice, and everything else about the representation is the
    /// same.
    /// <para/>
    /// **<see cref="IsIdempotent"/> is the whole reason this is a type rather than a pair of
    /// delegates.** <c>a or a = a</c> holds in the boolean semiring and fails in the complex
    /// one, where <c>|x&gt; + |x&gt;</c> is <c>2|x&gt;</c> -- and that single difference is
    /// what separates *covering* from *superposition*. Quine-McCluskey's merge step is
    /// absorption, and it is sound only because a minterm may be covered twice for free; a
    /// procedure that assumed the same of amplitudes would double them. Anything reading this
    /// flag is asking whether it may cover the same basis element more than once.
    /// </remarks>
    internal abstract class Semiring
    {
        /// <summary>The additive identity: a term carrying it is not a term at all.</summary>
        internal abstract Entity Zero { get; }

        /// <summary>The multiplicative identity.</summary>
        internal abstract Entity One { get; }

        internal abstract Entity Add(Entity left, Entity right);

        internal abstract Entity Multiply(Entity left, Entity right);

        /// <summary>
        /// Whether <c>Add(a, a)</c> is <c>a</c> for every <c>a</c>.
        /// </summary>
        internal abstract bool IsIdempotent { get; }

        /// <summary>
        /// Whether a coefficient is the additive identity, and so whether its term should be
        /// dropped.
        /// </summary>
        /// <remarks>
        /// Decided on <see cref="Entity.InnerSimplified"/> rather than on
        /// <c>Simplify</c>, which is the cheap end of a real trade: a
        /// coefficient that vanishes only under full simplification survives as a term with a
        /// zero coefficient. That is a tidiness cost and never a correctness one -- the term
        /// contributes nothing either way -- and the caller may simplify first where it
        /// matters.
        /// </remarks>
        internal virtual bool IsZero(Entity coefficient)
            => coefficient.InnerSimplified == Zero;

        /// <summary>
        /// Ordinary arithmetic, for polynomials, series and quantum amplitudes.
        /// </summary>
        internal static Semiring Field { get; } = new FieldSemiring();

        /// <summary>
        /// Disjunction and conjunction, for boolean expressions. Idempotent, which is what
        /// makes covering sound.
        /// </summary>
        internal static Semiring Boolean { get; } = new BooleanSemiring();

        private sealed class FieldSemiring : Semiring
        {
            internal override Entity Zero => Integer.Zero;
            internal override Entity One => Integer.One;
            internal override Entity Add(Entity left, Entity right) => left + right;
            internal override Entity Multiply(Entity left, Entity right) => left * right;
            internal override bool IsIdempotent => false;
        }

        private sealed class BooleanSemiring : Semiring
        {
            internal override Entity Zero => Entity.Boolean.False;
            internal override Entity One => Entity.Boolean.True;
            internal override Entity Add(Entity left, Entity right) => left | right;
            internal override Entity Multiply(Entity left, Entity right) => left & right;
            internal override bool IsIdempotent => true;
        }
    }
}
