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
        /// Whether a numerator over <paramref name="factor"/> to the power
        /// <paramref name="multiplicity"/> is a shape some integration rule answers.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is a claim about the rule set, not about polynomials, so it is kept in one place
        /// and named rather than left as a condition in a loop — it goes stale whenever a rule is
        /// added, and it had. Each line says which rule reads that shape.
        /// </para>
        /// <para>
        /// A factor with no rule leaves the whole integral unevaluated either way, so admitting
        /// one costs no answer and a great deal of time; that is what the guard is for and why
        /// widening it is done by measurement rather than by leaving it open.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        private static bool AnIntegrationRuleReadsIt(IntegerPolynomial factor, int multiplicity)
            => factor.Degree switch
            {
                // A power of a linear factor: the rule for a numerator over (a x + b)^k.
                <= 1 => true,
                // A quadratic, once. There is no rule for a numerator over (x^2 + c)^k.
                2 => multiplicity == 1,
                // A biquadratic quartic, once: TrySplitBiquadraticOverTheReals factors it into
                // two real quadratics. Only the even-powered shape -- that rule reads
                // x^4 + p x^2 + q and nothing else, and a general quartic has no rule.
                4 => multiplicity == 1 && IsBiquadratic(factor),
                _ => false
            };

        /// <summary>
        /// Whether <paramref name="factor"/> has only even powers, so that it is a quadratic in
        /// <c>x^2</c>.
        /// </summary>
        private static bool IsBiquadratic(IntegerPolynomial factor)
        {
            for (var power = 1; power <= factor.Degree; power += 2)
                if (!factor[power].IsZero)
                    return false;
            return true;
        }

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
            // reads, or the decomposition answers nothing and is not worth producing. What that
            // amounts to is written out in AnIntegrationRuleReadsIt, which is a claim about the
            // rules rather than about polynomials and goes stale when they change -- as it had:
            // this said "none for an irreducible factor of degree three or more at all", and an
            // irreducible *biquadratic* quartic has been read by
            // TrySplitBiquadraticOverTheReals since #1102. So x^6 + 1 was declined although the
            // library factors it into (x^2 + 1)(x^4 - x^2 + 1) and integrates both of those on
            // their own.
            //
            // Deleting the guard costs no answer and a great deal of time: a piece with no
            // rule leaves the whole integral unevaluated either way, but every half of every
            // split is a fresh problem the whole integrator searches before that is known.
            // Without it (1 - x^4)/(1 + x^4 + x^8), whose factorisation holds the irreducible
            // quartic x^4 - x^2 + 1, takes 18s to decline where it took 203ms. Read off the
            // factorisation, which is already in hand, the cost of declining is that one
            // factorisation.
            foreach (var part in factorization.Parts)
                if (!AnIntegrationRuleReadsIt(part.Factor, part.Multiplicity))
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

        /// <summary>
        /// <c>N/D</c> written as one fraction per factor of <paramref name="denominator"/>,
        /// where the denominator is <b>written</b> as a product of distinct linear and quadratic
        /// factors whose coefficients may be symbols, or <see langword="false"/> where it is not
        /// or the decomposition cannot be settled.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The two splits above work in exact rational arithmetic and stop where the rationals
        /// do, and a symbol is not a rational: <c>1/((x + a)(x^2 + b))</c> has factors already in
        /// hand and each of them is a shape the integrator reads, and it was declined because
        /// nothing here could read the factors. This is the textbook method for it -- undetermined
        /// coefficients -- and it is the one that survives a symbol, because it solves a linear
        /// system in the unknown numerators rather than dividing polynomials whose coefficients
        /// have to be compared to zero.
        /// </para>
        /// <para>
        /// <b>Which pivots are safe.</b> The system's entries are polynomials in the parameters,
        /// and a row reduction has to decide whether a pivot is zero. A number is decided by
        /// looking; a symbolic pivot is taken only when it does not simplify to zero, which is a
        /// judgement and not a proof, and that is why <b>the decomposition is checked before it
        /// is returned</b>: the identity <c>N = sum P_i * D/F_i</c> is evaluated at several
        /// points with every symbol pinned, and a split that fails it is declined rather than
        /// handed on. A wrong pivot can cost an answer here and cannot produce a wrong one.
        /// </para>
        /// <para>
        /// The generic case, as everywhere in this integrator: two factors that coincide for
        /// one value of a parameter are coprime for every other, and the answer is the one for
        /// those. See the note on <c>PolynomialLongDivision</c>.
        /// </para>
        /// <para>
        /// Distinct factors, where a repeated linear one counts as a single block over its whole
        /// power. A repeated symbolic quadratic has no rule to land on and is declined.
        /// </para>
        /// https://github.com/asc-community/AngouriMath/issues/718
        /// </remarks>
        internal static bool TrySplitOverWrittenFactors(
            Entity numerator, Entity denominator, Variable x, [NotNullWhen(true)] out Entity? decomposition)
        {
            decomposition = null;

            // The factors as written, each a polynomial in x of degree one or two with
            // coefficients free of x; a factor free of x is a constant and comes out in front.
            var factors = new List<(Entity Factor, int Degree)>();
            Entity constant = Integer.One;
            foreach (var part in Mulf.LinearChildren(denominator))
            {
                if (!part.ContainsNode(x))
                {
                    constant *= part;
                    continue;
                }
                // A factor written as a power is a repeated factor, whatever its degree reads
                // as: `(a + b u)^2` is a quadratic to the reader below and a repeated linear
                // factor to the decomposition, and taking it as the former cost five seconds on
                // `1/(a + b e^(p x))^2` for a split that nothing downstream answers. A repeated
                // **linear** factor is taken as one block, `P/(a + b u)^k` with `P` of degree
                // `k - 1`, which is the shape the rule for a numerator over a power of a linear
                // reads; a repeated quadratic has no such rule and is declined.
                var toRead = part;
                var multiplicity = 1;
                if (part is Powf(var repeatedBase, Integer { EInteger.Sign: > 0 } repeated) && repeated != Integer.One)
                {
                    if (!repeated.EInteger.CanFitInInt32())
                        return false;
                    toRead = repeatedBase;
                    multiplicity = repeated.EInteger.ToInt32Unchecked();
                }
                if (!TreeAnalyzer.TryGetPolynomial(toRead, x, out var read) || read.Count == 0)
                    return false;
                var degree = read.Keys.Max()!;
                if (!degree.CanFitInInt32() || degree.ToInt32Unchecked() is not (1 or 2))
                    return false;
                if (multiplicity > 1 && degree.ToInt32Unchecked() != 1)
                    return false;
                foreach (var pair in read)
                    if (pair.Key.Sign < 0 || pair.Value.ContainsNode(x))
                        return false;
                factors.Add((part.InnerSimplified, degree.ToInt32Unchecked() * multiplicity));
            }
            if (factors.Count < 2)
                return false;
            for (var i = 0; i < factors.Count; i++)
                for (var j = i + 1; j < factors.Count; j++)
                    if (factors[i].Factor == factors[j].Factor)
                        return false;

            var total = factors.Sum(f => f.Degree);
            if (!TreeAnalyzer.TryGetPolynomial(numerator, x, out var above))
                return false;
            foreach (var pair in above)
                if (pair.Key.Sign < 0 || pair.Key.CompareTo(EInteger.FromInt32(total)) >= 0 || pair.Value.ContainsNode(x))
                    return false;

            // One unknown per coefficient of each numerator P_i, of degree one less than F_i.
            var unknowns = new List<Variable>();
            var nameSource = numerator + denominator;
            var numerators = new List<Entity>();
            foreach (var (_, degree) in factors)
            {
                // Built without a `x^0`, which `Expand` guards with a `provided x != 0` that
                // nothing downstream can read as a polynomial.
                Entity? p = null;
                for (var k = 0; k < degree; k++)
                {
                    var unknown = Variable.CreateUnique(nameSource, "c");
                    nameSource += unknown;
                    unknowns.Add(unknown);
                    Entity term = k == 0 ? unknown : k == 1 ? unknown * x : unknown * MathS.Pow(x, k);
                    p = p is null ? term : p + term;
                }
                numerators.Add(p!);
            }

            // N = sum P_i * prod_{j != i} F_j, compared coefficient by coefficient in x.
            Entity identity = 0;
            for (var i = 0; i < factors.Count; i++)
            {
                Entity cofactor = numerators[i];
                for (var j = 0; j < factors.Count; j++)
                    if (j != i)
                        cofactor *= factors[j].Factor;
                identity += cofactor;
            }
            if (!TreeAnalyzer.TryGetPolynomial(identity, x, out var rows))
                return false;

            var width = unknowns.Count;
            var matrix = new Entity[total][];
            var rhs = new Entity[total];
            for (var power = 0; power < total; power++)
            {
                var row = rows.TryGetValue(EInteger.FromInt32(power), out var atPower) ? atPower : Integer.Zero;
                matrix[power] = new Entity[width];
                for (var column = 0; column < width; column++)
                    matrix[power][column] = row.Differentiate(unknowns[column]).InnerSimplified;
                rhs[power] = above.TryGetValue(EInteger.FromInt32(power), out var wanted) ? wanted : Integer.Zero;
            }

            if (!TrySolveSquare(matrix, rhs, out var values))
                return false;

            Entity sum = 0;
            for (var i = 0; i < factors.Count; i++)
            {
                var p = numerators[i];
                for (var column = 0; column < width; column++)
                    p = p.Substitute(unknowns[column], values[column]);
                sum += Bare(p) / factors[i].Factor;
            }

            // Checked rather than trusted, at points with every symbol pinned: a pivot that was
            // zero without looking it would have produced a wrong decomposition, and this is
            // what turns that into a declined one.
            if (!HoldsAtSampledPoints(numerator / denominator, sum / constant, x))
                return false;

            decomposition = sum / constant;
            return true;
        }

        /// <summary>
        /// Gaussian elimination on a square system whose entries may be symbols. A pivot is a
        /// number that is not zero where one is available in its column, and otherwise an entry
        /// that does not simplify to zero; no such entry declines the system.
        /// </summary>
        private static bool TrySolveSquare(Entity[][] matrix, Entity[] rhs, [NotNullWhen(true)] out Entity[]? values)
        {
            values = null;
            var size = rhs.Length;
            if (size == 0 || matrix.Any(row => row.Length != size))
                return false;
            return TrySolveLinear(matrix, rhs, out values);
        }

        /// <summary>
        /// Gaussian elimination on a system of any shape whose entries may be symbols: a row
        /// that reduces to <c>0 = c</c> with <c>c</c> not decidably zero declines the system,
        /// and an unknown no row pivots on is set to zero. A pivot is a number that is not zero
        /// where a column has one, and otherwise an entry that does not simplify to zero — a
        /// judgement, which is why every caller checks what it builds from the answer.
        /// </summary>
        internal static bool TrySolveLinear(Entity[][] matrix, Entity[] rhs, [NotNullWhen(true)] out Entity[]? values)
        {
            values = null;
            var rows = rhs.Length;
            if (rows == 0 || matrix.Length != rows)
                return false;
            var width = matrix[0].Length;
            if (width == 0 || matrix.Any(row => row.Length != width))
                return false;

            var pivotColumnOfRow = new int[rows];
            for (var row = 0; row < rows; row++)
                pivotColumnOfRow[row] = -1;
            var rank = 0;
            for (var column = 0; column < width && rank < rows; column++)
            {
                var pivot = -1;
                for (var row = rank; row < rows; row++)
                    if (matrix[row][column].Evaled is Complex { IsExact: true, IsZero: false })
                    {
                        pivot = row;
                        break;
                    }
                if (pivot < 0)
                    for (var row = rank; row < rows; row++)
                        if (matrix[row][column] != Integer.Zero && matrix[row][column].Evaled is not Complex { IsZero: true })
                        {
                            pivot = row;
                            break;
                        }
                if (pivot < 0)
                    continue;
                if (pivot != rank)
                {
                    (matrix[pivot], matrix[rank]) = (matrix[rank], matrix[pivot]);
                    (rhs[pivot], rhs[rank]) = (rhs[rank], rhs[pivot]);
                }
                for (var row = rank + 1; row < rows; row++)
                {
                    if (matrix[row][column] == Integer.Zero)
                        continue;
                    var factor = Bare(matrix[row][column] / matrix[rank][column]);
                    for (var k = column; k < width; k++)
                        matrix[row][k] = Bare(matrix[row][k] - factor * matrix[rank][k]);
                    rhs[row] = Bare(rhs[row] - factor * rhs[rank]);
                }
                pivotColumnOfRow[rank] = column;
                rank++;
            }

            // A row with no pivot says 0 = rhs; the system is consistent only where that is
            // decidably so.
            for (var row = rank; row < rows; row++)
                if (rhs[row] != Integer.Zero && rhs[row].Evaled is not Complex { IsZero: true })
                    return false;

            values = new Entity[width];
            for (var column = 0; column < width; column++)
                values[column] = Integer.Zero;
            for (var row = rank - 1; row >= 0; row--)
            {
                var column = pivotColumnOfRow[row];
                var accumulated = rhs[row];
                for (var k = column + 1; k < width; k++)
                    if (matrix[row][k] != Integer.Zero)
                        accumulated -= matrix[row][k] * values[k];
                values[column] = Bare(accumulated / matrix[row][column]);
            }
            return true;
        }

        /// <summary>
        /// Simplified, and without the <c>provided pivot != 0</c> a division by a symbol
        /// acquires on the way: that condition is the generic case this decomposition is
        /// answering in, and left on it makes a term nothing downstream reads.
        /// </summary>
        internal static Entity Bare(Entity e)
            => e.InnerSimplified is Providedf(var inner, _) ? inner : e.InnerSimplified;

        /// <summary>
        /// Whether <paramref name="left"/> and <paramref name="right"/> agree, numerically, at a
        /// few points in <paramref name="x"/> with every other symbol pinned to a fixed value.
        /// A point where either is undefined is skipped, and at least two must compare.
        /// </summary>
        internal static bool HoldsAtSampledPoints(Entity left, Entity right, Variable x)
        {
            var parameters = left.Vars.Concat(right.Vars).Where(v => v != x).Distinct().ToList();
            var pinned = 0;
            foreach (var parameter in parameters)
            {
                // Distinct values, off the integers, so that no two factors coincide by
                // accident and a sign or a root in a coefficient stays generic.
                var fraction = (pinned % 3) switch { 0 => "1.37", 1 => "2.71", _ => "0.83" };
                var value = Real.Create(EDecimal.FromString(fraction).Add(EDecimal.FromInt32(pinned)));
                left = left.Substitute(parameter, value);
                right = right.Substitute(parameter, value);
                pinned++;
            }
            var compared = 0;
            foreach (var at in new[] { "0.29", "1.43", "3.17", "-0.61" })
            {
                var point = Real.Create(EDecimal.FromString(at));
                var l = left.Substitute(x, point).EvalNumerical();
                var r = right.Substitute(x, point).EvalNumerical();
                if (l.IsNaN || r.IsNaN)
                    continue;
                var difference = (l - r).Abs().EDecimal;
                var scale = EDecimal.Max(EDecimal.One, l.Abs().EDecimal);
                if (difference.CompareTo(scale.Multiply(EDecimal.FromString("1e-9"))) > 0)
                    return false;
                compared++;
            }
            return compared >= 2;
        }
    }
}
