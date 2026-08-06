//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Collections.Generic;
using System.Linq;

namespace AngouriMath.Functions.Algebra.MonoidAlgebra
{
    using static Entity;

    /// <summary>
    /// A finitely-supported map from a basis into a coefficient semiring -- the shape shared by
    /// a polynomial, an asymptotic series, a boolean expression's minterms and a quantum state.
    /// </summary>
    /// <typeparam name="TBasis">
    /// The basis element. An exponent vector, a rational exponent, a basis ket.
    /// <para/>
    /// **It must compare by value.** The terms are keyed on it, so a basis with reference
    /// equality would leave every element distinct and quietly stop collecting like terms --
    /// which is not an error anywhere, just a wrong answer. Use a struct or a record.
    /// </typeparam>
    /// <remarks>
    /// Immutable by construction: every operation returns a new instance, and the dictionary
    /// handed in is copied rather than adopted. It is not
    /// <c>System.Collections.Immutable</c> because that would be a new package reference on a
    /// library that has none, which is a packaging decision rather than a design one.
    /// <para/>
    /// The invariant is that **no term carries a zero coefficient**. That is what makes the
    /// count of terms meaningful, and it is where an idempotent semiring quietly pays for
    /// itself: adding a boolean term to itself collapses to one term by
    /// <see cref="Semiring.Add"/> alone, so absorption needs no special case here.
    /// <para/>
    /// See *One structure under several features* in <c>AGENTS.md</c> for what this is for and,
    /// as importantly, what must not be built on it -- cover selection, factorisation into
    /// irreducibles and series truncation are each specific to one feature and belong in it.
    /// </remarks>
    internal sealed class SparseTerms<TBasis> where TBasis : notnull
    {
        private readonly Dictionary<TBasis, Entity> terms;

        internal Semiring Coefficients { get; }

        /// <summary>The basis elements present, each with a non-zero coefficient.</summary>
        internal IReadOnlyDictionary<TBasis, Entity> Terms => terms;

        internal int Count => terms.Count;

        /// <summary>Whether nothing is left: the zero of the algebra.</summary>
        internal bool IsEmpty => terms.Count == 0;

        private SparseTerms(Dictionary<TBasis, Entity> terms, Semiring coefficients)
        {
            this.terms = terms;
            Coefficients = coefficients;
        }

        internal static SparseTerms<TBasis> Empty(Semiring coefficients)
            => new(new Dictionary<TBasis, Entity>(), coefficients);

        /// <summary>
        /// The terms with like bases collected and zero coefficients dropped, so that the
        /// invariant holds however the caller assembled them.
        /// </summary>
        internal static SparseTerms<TBasis> From(
            IEnumerable<KeyValuePair<TBasis, Entity>> terms, Semiring coefficients)
        {
            var gathered = new Dictionary<TBasis, Entity>();
            // KeyValuePair has no Deconstruct on netstandard2.0, which this library targets.
            foreach (var term in terms)
                gathered[term.Key] = gathered.TryGetValue(term.Key, out var running)
                    ? coefficients.Add(running, term.Value)
                    : term.Value;
            foreach (var basis in gathered.Keys.Where(b => coefficients.IsZero(gathered[b])).ToList())
                gathered.Remove(basis);
            return new SparseTerms<TBasis>(gathered, coefficients);
        }

        internal static SparseTerms<TBasis> Single(TBasis basis, Entity coefficient, Semiring coefficients)
            => From(new[] { new KeyValuePair<TBasis, Entity>(basis, coefficient) }, coefficients);

        /// <summary>
        /// The sum, with like bases collected. In an idempotent semiring this is absorption.
        /// </summary>
        internal SparseTerms<TBasis> Add(SparseTerms<TBasis> other)
            => From(terms.Concat(other.terms), Coefficients);

        /// <summary>Every coefficient multiplied by <paramref name="scalar"/>.</summary>
        internal SparseTerms<TBasis> Scale(Entity scalar)
            => From(terms.Select(term => new KeyValuePair<TBasis, Entity>(
                        term.Key, Coefficients.Multiply(scalar, term.Value))),
                    Coefficients);

        /// <summary>
        /// The product: every pair of terms, bases combined by the monoid and coefficients by
        /// the semiring. Polynomial multiplication and the tensor product are both this.
        /// </summary>
        internal SparseTerms<TBasis> Multiply(SparseTerms<TBasis> other, IBasisOps<TBasis> basis)
            => From(terms.SelectMany(left => other.terms.Select(right =>
                        new KeyValuePair<TBasis, Entity>(
                            basis.Combine(left.Key, right.Key),
                            Coefficients.Multiply(left.Value, right.Value)))),
                    Coefficients);

        /// <summary>
        /// The greatest basis element dividing every term, and what is left once it is divided
        /// out -- so that the original is the one multiplied by the other.
        /// </summary>
        /// <remarks>
        /// This is the shared half of factoring. <c>x^2*y + x^2</c> comes back as
        /// <c>(2,0)</c> with <c>y + 1</c>; <c>|001&gt; + |011&gt;</c> as the cube <c>0-1</c>
        /// with <c>|0&gt; + |1&gt;</c>. What each feature does with that differs, and does not
        /// belong here.
        /// <para/>
        /// Empty terms have no common factor to speak of, so the identity is returned and
        /// nothing is claimed.
        /// </remarks>
        internal (TBasis Common, SparseTerms<TBasis> Remainder) FactorOutCommon(IBasisOps<TBasis> basis)
        {
            if (IsEmpty)
                return (basis.Identity, this);
            var common = terms.Keys.Aggregate(basis.Meet);
            var remainder = new Dictionary<TBasis, Entity>();
            foreach (var term in terms)
            {
                if (!basis.TryDivide(term.Key, common, out var quotient))
                    // The meet of the support divides every element of it by construction, so
                    // this is a broken IBasisOps rather than an expression this cannot handle,
                    // and silently returning something wrong would hide it.
                    return (basis.Identity, this);
                remainder[quotient] = term.Value;
            }
            return (common, new SparseTerms<TBasis>(remainder, Coefficients));
        }
    }
}
