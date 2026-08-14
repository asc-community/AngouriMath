//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Diagnostics.CodeAnalysis;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;

namespace AngouriMath.Functions
{
    /// <summary>
    /// One step of a partial fraction decomposition, at a coprime pair of factors of the
    /// denominator rather than at a root of it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The sibling step, <see cref="PolynomialFactoring.TrySplitOffRationalRoot"/>, splits at a
    /// rational root, which is all the decomposition there was: a denominator with no rational
    /// root was left whole, so <c>1/(x^4 + 3x^2 + 2)</c> had no antiderivative even though it
    /// is <c>(x^2 + 1)(x^2 + 2)</c> and both of those are integrated by the rule for a linear
    /// numerator over a quadratic. Nothing was missing but the split.
    /// https://github.com/asc-community/AngouriMath/issues/919
    /// </para>
    /// <para>
    /// One step and not the whole decomposition, for the same reason as the sibling: what comes
    /// out is two strictly smaller problems of the same kind, and the integrator recurses into
    /// them. Splitting <c>D</c> into coprime <c>A</c> and <c>B</c>, the extended Euclidean
    /// algorithm gives <c>U*A + V*B = 1</c>, so <c>N = N*V*B + N*U*A</c> and
    /// <c>N/(A*B) = N*V/A + N*U/B</c>. Each numerator is then reduced modulo its own
    /// denominator; the polynomial parts that come off cannot survive, since a proper fraction
    /// minus two proper fractions is a polynomial that vanishes at infinity.
    /// </para>
    /// <para>
    /// <b>No condition is owed.</b> <c>A</c> and <c>B</c> being coprime, <c>A*B</c> is zero
    /// exactly where one of them is, so the two sides are undefined at the same points and the
    /// domain neither widens nor narrows. That is what makes this different from cancelling a
    /// shared factor, which is where a decomposition usually loses a singularity.
    /// </para>
    /// <para>
    /// The decomposition is produced only where every piece of it is a shape an integration
    /// rule reads — see the guard below, which is what keeps declining cheap. A denominator
    /// that is a power of one irreducible is declined for the further reason that it has no
    /// coprime pair to split into at all. The ladder that decomposes that one — <c>N/f^k</c>
    /// as terms over <c>f^k</c>, <c>f^(k-1)</c>, ... — is deliberately not built here: for
    /// <c>f</c> linear the sibling step already does it, and for <c>f</c> quadratic every term
    /// it produces is over <c>(x^2 + c)^k</c>, which nothing reads, so the decomposition would
    /// end in the same unevaluated integral it started from.
    /// </para>
    /// </remarks>
    internal static class PartialFractions
    {
        /// <summary>
        /// <c>N/D</c> written as two fractions over coprime factors of <paramref name="denominator"/>,
        /// each a strictly smaller problem of the same kind, or <see langword="false"/> where
        /// the denominator does not factor into a coprime pair.
        /// </summary>
        internal static bool TrySplitIntoCoprimeParts(
            Entity numerator, Entity denominator, Variable x,
            [NotNullWhen(true)] out Entity? left,
            [NotNullWhen(true)] out Entity? right)
        {
            left = right = null;

            // Degree four is the first at which a polynomial can factor with no rational root
            // anywhere in it, x^4 + 3x^2 + 2 being the smallest. Below that a factorisation
            // implies a root, and the step at a root has already been tried and has failed.
            if (!PolynomialFactoring.TryGetRationalCoefficients(
                    denominator, x, leastTerms: 2, leastDegree: 4, IntegerPolynomial.MaxDegree, out var d)
                || !PolynomialFactoring.TryGetRationalCoefficients(
                    numerator, x, leastTerms: 1, leastDegree: 0, IntegerPolynomial.MaxDegree, out var n))
                return false;

            // Only a proper fraction decomposes; an improper one is a polynomial plus a proper
            // fraction and has to be divided out first, which is not done here.
            if (n.Length >= d.Length)
                return false;

            if (PolynomialFactorization.Factor(denominator, x) is not { } factorization)
                return false;

            // Every piece the decomposition would produce has to be one an integration rule
            // reads, or the decomposition answers nothing and is not worth producing. A linear
            // factor is read at any multiplicity, and a quadratic one only at the first: there
            // is no rule for a numerator over (x^2 + c)^k, and none for an irreducible factor
            // of degree three or more at all.
            //
            // Deleting the guard costs no answer and a great deal of time: a piece with no
            // rule leaves the whole integral unevaluated either way, but every half of every
            // split is a fresh problem the whole integrator searches before that is known.
            // Without it (1 - x^4)/(1 + x^4 + x^8), whose factorisation holds the irreducible
            // quartic x^4 - x^2 + 1, takes 18s to decline where it took 203ms. Read off the
            // factorisation, which is already in hand, the cost of declining is that one
            // factorisation.
            foreach (var part in factorization.Parts)
                if (part.Factor.Degree > 2 || (part.Factor.Degree == 2 && part.Multiplicity > 1))
                    return false;

            // Each part of the factorisation is a distinct irreducible with its multiplicity,
            // so any one of them raised to that multiplicity is coprime to the product of the
            // rest. The smallest is taken, which keeps the intermediate coefficients of the
            // extended Euclidean run down; which one is chosen cannot change what is reachable,
            // since both sides recurse and the full decomposition is arrived at either way.
            var chosen = factorization.Parts[0];
            foreach (var part in factorization.Parts)
                if (part.Factor.Degree * part.Multiplicity < chosen.Factor.Degree * chosen.Multiplicity)
                    chosen = part;

            var whole = RationalPolynomial.Create(d);
            var a = RationalPolynomial.FromInteger(chosen.Factor).Pow(chosen.Multiplicity);
            if (!whole.TryDivide(a, out var b, out var remainder) || !remainder.IsZero || b.IsConstant)
                return false;

            if (!RationalPolynomial.TryBezout(a, b, out var u, out var v))
                return false;

            var wanted = RationalPolynomial.Create(n);
            if (!wanted.Multiply(v).TryDivide(a, out _, out var overA)
                || !wanted.Multiply(u).TryDivide(b, out _, out var overB))
                return false;

            // The identity the two fractions stand for, checked rather than assumed: nothing
            // above is trusted to have produced a decomposition of this numerator in
            // particular, and a wrong split would otherwise leave a wrong antiderivative that
            // only differentiating it back would catch.
            if (!overA.Multiply(b).Add(overB.Multiply(a)).SameAs(wanted))
                return false;

            left = AsFraction(overA, a, x);
            right = AsFraction(overB, b, x);
            return true;
        }

        /// <summary>
        /// A vanishing numerator is answered as zero rather than as a quotient, so that a
        /// numerator sharing a factor with the denominator does not leave the integrator
        /// working out the antiderivative of the other side only to multiply it by nothing.
        /// </summary>
        private static Entity AsFraction(RationalPolynomial numerator, RationalPolynomial denominator, Variable x)
            => numerator.IsZero ? Integer.Create(0) : numerator.ToEntity(x) / denominator.ToEntity(x);
    }
}
