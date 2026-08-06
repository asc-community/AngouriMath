//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

namespace AngouriMath.Functions.Algebra.MonoidAlgebra
{
    /// <summary>
    /// What a basis has to be able to do for its terms to be factorable: it is a monoid, and
    /// its elements have a greatest common part that can be divided out.
    /// </summary>
    /// <typeparam name="TBasis">
    /// An exponent vector for a polynomial, a rational exponent for a series, a basis ket for a
    /// quantum state.
    /// </typeparam>
    /// <remarks>
    /// Passed as an instance rather than fixed by a type parameter with <c>new()</c>. The
    /// codebase does it the other way in
    /// <c>PolynomialSolver.GatherMonomialInformation&lt;T, TPrimitive&gt;</c>, and that shape
    /// is harder to test in isolation for no gain here, since these operations are stateless
    /// and one instance serves every call.
    /// <para/>
    /// **<see cref="Meet"/> is what makes factoring one operation across features.** Dividing
    /// out the meet of the support is simultaneously "take the common monomial out of a
    /// polynomial" and "detect that a quantum state is separable":
    /// <code>
    /// x^2*y + x^2      meet of (2,1) and (2,0) is (2,0), leaving y + 1
    /// |001&gt; + |011&gt;    meet of 001 and 011 is 0-1,   leaving |0&gt; + |1&gt;
    /// </code>
    /// The two look different only because a polynomial's basis is ordered -- so the meet is a
    /// componentwise minimum -- while a ket's is flat, and the meet keeps a position only where
    /// every element agrees. Both are the meet in a product of semilattices.
    /// </remarks>
    internal interface IBasisOps<TBasis>
    {
        /// <summary>The empty product: the monoid's identity.</summary>
        TBasis Identity { get; }

        /// <summary>
        /// The monoid operation -- adding exponents, concatenating kets. Multiplying two terms
        /// combines their bases with this and their coefficients with the semiring.
        /// </summary>
        TBasis Combine(TBasis left, TBasis right);

        /// <summary>
        /// The greatest part the two have in common, which is what may be factored out of
        /// both.
        /// </summary>
        TBasis Meet(TBasis left, TBasis right);

        /// <summary>
        /// <paramref name="whole"/> with <paramref name="part"/> removed, or
        /// <see langword="false"/> where <paramref name="part"/> does not divide it.
        /// </summary>
        bool TryDivide(TBasis whole, TBasis part, out TBasis quotient);
    }
}
