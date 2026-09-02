//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using PeterO.Numbers;
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
        /// <c>N/D</c> written as two fractions over the quadratic factors of a <b>biquadratic</b>
        /// <paramref name="denominator"/> — one with no odd power in it — or
        /// <see langword="false"/> where it is not one, or does not split into two distinct
        /// quadratics.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Why this exists next to the step above.</b> That one factors over the rationals,
        /// and stops where the rationals do: <c>x^4 + 1</c> is irreducible over <c>Q</c>, so it
        /// is left whole and <c>x^2/(x^4 + 1)</c> has no antiderivative — the case
        /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a> names as
        /// wanting "partial fractioning". Over the reals it is
        /// <c>(x^2 - sqrt(2)x + 1)(x^2 + sqrt(2)x + 1)</c>, and both halves are read by the rule
        /// for a linear numerator over a quadratic. Nothing was missing but a factorisation the
        /// rational one is right to refuse.
        /// </para>
        /// <para>
        /// <b>Biquadratic only, and that is a real boundary rather than a first cut.</b> A
        /// general quartic factors into real quadratics through its resolvent cubic, whose roots
        /// carry Cardano's nested radicals; a biquadratic <c>x^4 + px^2 + q</c> is the case where
        /// the resolvent is solvable by inspection, and the two factors stay in one square root.
        /// Two shapes come out of it, by the sign of <c>p^2 - 4q</c>:
        /// </para>
        /// <list type="bullet">
        /// <item>
        /// <b>Negative</b> — no real root in <c>x^2</c>. Matching
        /// <c>(x^2 + ax + b)(x^2 - ax + b) = x^4 + (2b - a^2)x^2 + b^2</c> gives
        /// <c>b = sqrt(q)</c> and <c>a = sqrt(2b - p)</c>, both real because <c>q &gt; 0</c> and
        /// <c>p^2 &lt; 4q</c> forces <c>p &lt; 2sqrt(q)</c>. This is <c>x^4 + 1</c>, at
        /// <c>a = sqrt(2)</c>, <c>b = 1</c>.
        /// </item>
        /// <item>
        /// <b>Positive</b> — two distinct real roots in <c>x^2</c>, so
        /// <c>(x^2 + M)(x^2 + N)</c> with <c>M, N = (p +- sqrt(p^2 - 4q))/2</c>. Both factors are
        /// even, and the split is two independent pairs of equations rather than four.
        /// </item>
        /// <item>
        /// <b>Zero</b> — <c>(x^2 + p/2)^2</c>, a repeated quadratic, declined for the same reason
        /// the guard above declines one: there is no rule for a numerator over
        /// <c>(x^2 + c)^k</c>, so decomposing it ends in the integral it started from.
        /// </item>
        /// </list>
        /// <para>
        /// <b>No condition is owed</b>, on the same argument as the step above: the two factors
        /// are distinct and coprime, so their product vanishes exactly where the original
        /// denominator does. <c>q &gt; 0</c> is required rather than assumed, which is what keeps
        /// <c>b</c> real; a negative <c>q</c> puts a real root in <c>x^2</c> of either sign and
        /// is left to the rational step, which reaches it whenever the root is rational.
        /// </para>
        /// <para>
        /// Reached only after the rational split has declined, so a biquadratic that factors over
        /// <c>Q</c> — <c>x^4 + 3x^2 + 2</c> — is decomposed there, in exact arithmetic, and never
        /// arrives here to be given a square root it does not need.
        /// </para>
        /// </remarks>
        internal static bool TrySplitBiquadraticOverTheReals(
            Entity numerator, Entity denominator, Variable x,
            [NotNullWhen(true)] out Entity? left,
            [NotNullWhen(true)] out Entity? right)
        {
            left = right = null;

            // Every guard below is rational arithmetic on coefficients already in hand, so a
            // denominator this does not apply to costs one polynomial read to decline.
            if (!PolynomialFactoring.TryGetRationalCoefficients(
                    denominator, x, leastTerms: 2, leastDegree: 4, maxDegree: 4, out var d)
                || d.Length != 5
                || !d[1].IsZero || !d[3].IsZero)
                return false;

            // A proper fraction only, as above: an improper one is a polynomial plus a proper
            // fraction and has to be divided out first, which is not done here. The degree
            // ceiling of three is what says so, the denominator's being four.
            if (!PolynomialFactoring.TryGetRationalCoefficients(
                    numerator, x, leastTerms: 1, leastDegree: 0, maxDegree: 3, out var c))
                return false;

            var lead = d[4];
            var p = d[2].Divide(lead);
            var q = d[0].Divide(lead);

            // At q = 0 the quartic is x^2(x^2 + p), whose rational root zero the step above has
            // already had. Nothing else about the sign of q is required here: the branch that
            // needs sqrt(q) real is the one below with a negative discriminant, and p^2 < 4q
            // makes q positive on its own.
            if (q.IsZero)
                return false;

            var discriminant = p.Multiply(p).Subtract(q.Multiply(ERational.FromInt32(4)));
            if (discriminant.IsZero)
                return false;

            // The numerator, padded to four coefficients so the two branches can index it
            // without asking how many terms it happened to have.
            var n = new Entity[4];
            for (var i = 0; i < n.Length; i++)
                n[i] = Rational.Create(i < c.Length ? c[i] : ERational.Zero);

            Entity leftNumerator, leftFactor, rightNumerator, rightFactor;
            if (discriminant.Sign < 0)
            {
                // (x^2 + ax + b)(x^2 - ax + b). Writing the split as (alpha x + beta)/A +
                // (gamma x + delta)/B and equating the four coefficients of
                // (alpha x + beta)B + (gamma x + delta)A against the numerator gives, using
                // B's -a where A has +a:
                //
                //   x^3:  alpha + gamma          = n3
                //   x^2:  a(gamma - alpha) + beta + delta = n2
                //   x^1:  b(alpha + gamma) + a(delta - beta) = n1
                //   x^0:  b(beta + delta)        = n0
                //
                // which is two sums and two differences rather than a linear solve.
                var b = MathS.Sqrt(Rational.Create(q)).InnerSimplified;
                var a = MathS.Sqrt(2 * b - Rational.Create(p)).InnerSimplified;

                var sum = (n[0] / b).InnerSimplified;                       // beta + delta
                var difference = ((n[1] - b * n[3]) / a).InnerSimplified;   // delta - beta
                var spread = ((n[2] - sum) / a).InnerSimplified;            // gamma - alpha

                leftFactor = MathS.Sqr(x) + a * x + b;
                rightFactor = MathS.Sqr(x) - a * x + b;
                leftNumerator = ((n[3] - spread) / 2 * x + (sum - difference) / 2).InnerSimplified;
                rightNumerator = ((n[3] + spread) / 2 * x + (sum + difference) / 2).InnerSimplified;
            }
            else
            {
                // (x^2 + u)(x^2 + v), both even, so the odd and even halves of the numerator
                // separate and each gives its own pair rather than one system of four. Writing
                // the split as (alpha x + beta)/(x^2 + u) + (gamma x + delta)/(x^2 + v), the
                // coefficients of (alpha x + beta)(x^2 + v) + (gamma x + delta)(x^2 + u) are
                //
                //   x^3: alpha + gamma = n3     x^1: v*alpha + u*gamma = n1
                //   x^2: beta  + delta = n2     x^0: v*beta  + u*delta = n0
                //
                // so alpha = (n1 - u*n3)/(v - u) and beta = (n0 - u*n2)/(v - u), with v - u the
                // square root of the discriminant. Note which of the two the numerators divide
                // by: pairing a numerator with the wrong factor flips the sign of the answer
                // and still satisfies the x^3 and x^2 rows, so it is not something the identity
                // check further down would catch on every numerator.
                var root = MathS.Sqrt(Rational.Create(discriminant)).InnerSimplified;
                var v = ((Rational.Create(p) + root) / 2).InnerSimplified;
                var u = ((Rational.Create(p) - root) / 2).InnerSimplified;

                var alpha = ((n[1] - u * n[3]) / root).InnerSimplified;
                var beta = ((n[0] - u * n[2]) / root).InnerSimplified;

                leftFactor = MathS.Sqr(x) + u;
                rightFactor = MathS.Sqr(x) + v;
                leftNumerator = (alpha * x + beta).InnerSimplified;
                rightNumerator = ((n[3] - alpha) * x + (n[2] - beta)).InnerSimplified;
            }

            // Two identities, checked rather than assumed -- the numerators against the numerator
            // they decompose, and the factors against the denominator they came from. The step
            // above checks one because its factorisation is exact by construction; here the
            // factors were built by matching coefficients through a square root, so the
            // factorisation is a claim of its own.
            //
            // Neither implies the other, so both are made. A wrong term common to the two factors
            // -- MathS.Sqr(x) misread as C#'s x ^ 2, which on an Entity is exclusive or and not a
            // power -- cancels between the two halves of the first identity and passes it, while
            // the second sees it immediately.
            if ((leftNumerator * rightFactor + rightNumerator * leftFactor
                    - numerator).Simplify() != Integer.Create(0))
                return false;
            if ((Rational.Create(lead) * leftFactor * rightFactor - denominator).Simplify()
                    != Integer.Create(0))
                return false;

            left = (leftNumerator / (Rational.Create(lead) * leftFactor)).InnerSimplified;
            right = (rightNumerator / (Rational.Create(lead) * rightFactor)).InnerSimplified;
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
