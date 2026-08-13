//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Multithreading;
using PeterO.Numbers;
using System.Diagnostics.CodeAnalysis;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;

namespace AngouriMath.Functions
{
    /// <summary>
    /// Factors a polynomial in one variable over the rationals into irreducibles:
    /// <c>x^4 + 3x^2 + 2</c> becomes <c>(x^2 + 1)(x^2 + 2)</c>, which no search for roots can
    /// find because it has none.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is what <see cref="PolynomialFactoring"/> is not. That one tries the candidates of
    /// the rational root theorem and divides out the linear factors it finds, which answers
    /// the question only when the polynomial splits into linear pieces. A factor of degree two
    /// or more with no rational root is invisible to it, and those are the common case above
    /// degree three.
    /// </para>
    /// <para>
    /// The route is Zassenhaus's, in four steps. Clear denominators and take the primitive
    /// part, so the problem is over <c>Z</c>. Split off the repeated factors with
    /// <see cref="SquareFreeDecomposition"/>, so every remaining problem is square-free.
    /// Factor what is left modulo a small prime, where
    /// <see cref="PrimeFieldFactorization">Berlekamp</see> answers completely and cheaply.
    /// Then lift that factorisation from <c>p</c> to <c>p^k</c> by Hensel's construction and
    /// try products of the lifted pieces against the original, which is where the true
    /// factors are recovered — an irreducible factor over <c>Z</c> may well split further
    /// modulo <c>p</c>, so the modular factors have to be recombined rather than read off.
    /// Zassenhaus, <i>On Hensel factorization I</i>, J. Number Theory 1 (1969); von zur Gathen
    /// and Gerhard, <i>Modern Computer Algebra</i>, ch. 15.
    /// </para>
    /// <para>
    /// Two things bound the cost, and both are refusals rather than approximations. The
    /// recombination is exponential in the number of modular factors, so it is given a budget
    /// and declines past it. And <c>k</c> is chosen from Mignotte's bound so that a
    /// coefficient in the symmetric range modulo <c>p^k</c> is the coefficient itself; a
    /// smaller <c>k</c> would not make an answer wrong, since every candidate is confirmed by
    /// exact division over <c>Z</c> before it is accepted, but it would make the search miss
    /// factors and call a reducible polynomial irreducible.
    /// </para>
    /// <para>
    /// Nothing here is trusted. Every candidate factor is divided out exactly over <c>Z</c>,
    /// and the factors are multiplied back and compared with what they came from before the
    /// answer is returned. The failure mode that survives all of that is an incomplete
    /// factorisation, never a wrong one.
    /// </para>
    /// </remarks>
    internal static class PolynomialFactorization
    {
        /// <summary>
        /// Small primes to reduce modulo. Berlekamp's splitting loop runs over the whole
        /// field, so the cost grows with the prime and there is no reason to reach for a
        /// large one: a prime is only unusable when it divides the leading coefficient or
        /// makes the polynomial repeat a factor, and both are rare.
        /// </summary>
        [ConstantField]
        private static readonly long[] Primes =
        {
            5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97
        };

        /// <summary>
        /// How many usable primes to weigh against each other. The number of modular factors
        /// is an upper bound on the number of true ones and the recombination is exponential
        /// in it, so the cheapest thing that pays for itself is trying a few primes and
        /// keeping the one that splits the polynomial least.
        /// </summary>
        private const int MaxPrimeAttempts = 5;

        /// <summary>
        /// Subsets of modular factors to try before giving up. Reached only by polynomials
        /// that split into many pieces modulo every prime tried, which is the case
        /// recombination is exponential on.
        /// </summary>
        private const int MaxRecombinationCandidates = 20000;

        /// <summary>
        /// A leading coefficient is cleared by rescaling the variable, which raises it to the
        /// degree; past this the intermediate polynomial costs more than the answer is worth.
        /// </summary>
        [ConstantField]
        private static readonly EInteger MaxMonicScale = EInteger.FromInt32(2).Pow(512);

        /// <summary>
        /// <paramref name="expr"/> written as a product of powers of irreducible polynomials
        /// over <c>Q</c>, or <see langword="false"/> where it is not a polynomial in
        /// <paramref name="x"/>, where it is already irreducible, or where the machinery
        /// declines.
        /// </summary>
        /// <remarks>
        /// An irreducible polynomial is reported as a refusal rather than as a one-element
        /// product: the caller asked for a factorisation and there is not one to give, and
        /// answering with the input unchanged would have every caller test for that case.
        /// </remarks>
        internal static bool TryFactorIntoIrreducibles(
            Entity expr, Variable x, [NotNullWhen(true)] out Entity? factored)
        {
            factored = null;
            if (Factor(expr, x) is not { } factorization)
                return false;

            Entity? product = null;
            foreach (var part in factorization.Parts)
            {
                var piece = part.Factor.ToEntity(x);
                if (part.Multiplicity > 1)
                    piece = piece.Pow(part.Multiplicity);
                product = product is null ? piece : product * piece;
            }
            if (product is null)
                return false;
            if (factorization.Constant.CompareTo(ERational.One) != 0)
                product = Rational.Create(factorization.Constant) * product;
            factored = product;
            return true;
        }

        /// <summary>
        /// Whether <paramref name="expr"/> is <c>a * x^n + b</c> — a polynomial in
        /// <paramref name="x"/> with only two terms in it.
        /// </summary>
        /// <remarks>
        /// Asked by callers for which such a polynomial is better left whole than factored.
        /// Solving is the case: <c>x^n = -b/a</c> inverts in one step and gives the roots in
        /// polar form, where factoring first divides out the rational ones and leaves the
        /// rest to be recovered from a quotient — <c>x^3 - 8</c> reads as <c>2</c> and
        /// <c>(-1/2 +- i sqrt(3)/2) * 2</c> one way and as <c>2</c> and
        /// <c>(-2 -+ sqrt(-12))/2</c> the other. This is the same judgement
        /// <see cref="PolynomialFactoring.TrySplitOffRationalRoots"/> makes, and it is here
        /// rather than inside the factoriser because it is a statement about what a caller
        /// wants, not about what factors.
        /// </remarks>
        internal static bool IsTwoTermed(Entity expr, Variable x)
            => PolynomialFactoring.TryGetRationalCoefficients(
                   expr, x, leastTerms: 1, leastDegree: 0, IntegerPolynomial.MaxDegree, out var coefficients)
               && coefficients.Count(coefficient => !coefficient.IsZero) <= 2;

        /// <summary>
        /// The irreducible factors of <paramref name="expr"/> over <c>Q</c> with their
        /// multiplicities, and the rational constant left in front. <see langword="null"/>
        /// where there is nothing to say — including where the polynomial is irreducible.
        /// </summary>
        internal static Factorization? Factor(Entity expr, Variable x)
        {
            if (!PolynomialFactoring.TryGetRationalCoefficients(
                    expr, x, leastTerms: 2, leastDegree: 2, IntegerPolynomial.MaxDegree, out var rational))
                return null;

            // Cleared of denominators, so the whole of the rest of this works over Z; the
            // divisor comes back out in the constant at the end.
            var denominator = EInteger.One;
            foreach (var coefficient in rational)
                denominator = Lcm(denominator, coefficient.Denominator);
            var whole = new EInteger[rational.Length];
            for (var i = 0; i < whole.Length; i++)
                whole[i] = rational[i].Numerator.Multiply(denominator.Divide(rational[i].Denominator));

            var poly = IntegerPolynomial.Create(whole);
            if (poly.Degree < 2)
                return null;

            var primitive = poly.PrimitivePart();
            var content = poly.Content();
            if (poly.Leading.Sign < 0)
                content = content.Negate();

            if (FactorPrimitive(primitive) is not { } parts)
                return null;
            // One factor to the first power is the input back again, which is not a
            // factorisation of it.
            if (parts.Count == 1 && parts[0].Multiplicity == 1)
                return null;

            return new Factorization(ERational.Create(content, denominator).ToLowestTerms(), parts);
        }

        /// <summary>
        /// The irreducible factors of a primitive integer polynomial with their
        /// multiplicities, verified to multiply back to it.
        /// </summary>
        internal static IReadOnlyList<SquareFreeDecomposition.SquareFreePart>? FactorPrimitive(
            IntegerPolynomial primitive)
        {
            if (SquareFreeDecomposition.Decompose(primitive) is not { } squareFree)
                return null;

            var parts = new List<SquareFreeDecomposition.SquareFreePart>();
            foreach (var part in squareFree)
            {
                if (FactorSquareFree(part.Factor) is not { } irreducibles)
                    return null;
                foreach (var irreducible in irreducibles)
                    parts.Add(new SquareFreeDecomposition.SquareFreePart(irreducible, part.Multiplicity));
            }

            // Multiplied back independently of the machinery that produced them. An
            // incomplete factorisation is a tolerable answer and a wrong one is not, so the
            // product is what decides whether this is returned at all.
            var product = IntegerPolynomial.One;
            foreach (var part in parts)
                for (var i = 0; i < part.Multiplicity; i++)
                {
                    if (product.Multiply(part.Factor) is not { } multiplied)
                        return null;
                    product = multiplied;
                }
            return product.SameAs(primitive) ? parts : null;
        }

        /// <summary>
        /// The irreducible factors of a primitive square-free integer polynomial.
        /// </summary>
        /// <remarks>
        /// A leading coefficient other than one is cleared first, by the substitution that
        /// turns <c>f</c> of degree <c>n</c> into the monic
        /// <c>lc^(n-1) * f(x / lc)</c>. Everything below it can then assume monic input,
        /// which is what makes the Hensel step below keep the degrees it starts with. The
        /// factors come back through the inverse substitution, and their primitive parts are
        /// the factors of the original — Gauss's lemma is what makes that last step legal.
        /// </remarks>
        private static IReadOnlyList<IntegerPolynomial>? FactorSquareFree(IntegerPolynomial primitive)
        {
            if (primitive.Degree <= 1)
                return new[] { primitive };

            var lead = primitive.Leading;
            if (lead.CompareTo(EInteger.One) == 0)
                return FactorMonic(primitive);

            var degree = primitive.Degree;
            if (lead.Abs().Pow(degree - 1).CompareTo(MaxMonicScale) > 0)
                return null;

            var scaled = new EInteger[degree + 1];
            for (var i = 0; i < degree; i++)
                scaled[i] = primitive[i].Multiply(lead.Pow(degree - 1 - i));
            scaled[degree] = EInteger.One;

            if (FactorMonic(IntegerPolynomial.Create(scaled)) is not { } monicFactors)
                return null;

            var recovered = new List<IntegerPolynomial>(monicFactors.Count);
            foreach (var factor in monicFactors)
            {
                var back = new EInteger[factor.Degree + 1];
                for (var i = 0; i < back.Length; i++)
                    back[i] = factor[i].Multiply(lead.Pow(i));
                recovered.Add(IntegerPolynomial.Create(back).PrimitivePart());
            }
            return recovered;
        }

        /// <summary>The irreducible factors of a monic square-free integer polynomial.</summary>
        private static IReadOnlyList<IntegerPolynomial>? FactorMonic(IntegerPolynomial poly)
        {
            if (poly.Degree <= 1)
                return new[] { poly };

            long chosenPrime = 0;
            IReadOnlyList<PrimeFieldPolynomial>? chosen = null;
            var attempts = 0;
            foreach (var prime in Primes)
            {
                if (attempts >= MaxPrimeAttempts)
                    break;
                MultithreadingFunctional.ExitIfCancelled();
                // A prime dividing the leading coefficient drops the degree, and the
                // reduction then says nothing about the factors of what it came from.
                if (poly.ToPrimeField(prime) is not { } reduced || !reduced.IsSquareFree)
                    continue;
                if (PrimeFieldFactorization.Factor(reduced.MakeMonic()) is not { } modular)
                    continue;
                attempts++;
                // Irreducible modulo a prime that keeps the degree is irreducible over Z: a
                // proper factorisation over Z would reduce to one modulo p.
                if (modular.Count == 1)
                    return new[] { poly };
                if (chosen is null || modular.Count < chosen.Count)
                {
                    chosen = modular;
                    chosenPrime = prime;
                }
                if (chosen.Count == 2)
                    break;
            }
            if (chosen is null)
                return null;

            // Far enough that a coefficient in the symmetric range modulo the result is the
            // coefficient itself rather than a residue standing in for it.
            var target = poly.FactorCoefficientBound().Multiply(EInteger.FromInt32(2)).Add(EInteger.One);
            var prime2 = EInteger.FromInt64(chosenPrime);
            var modulus = prime2;
            while (modulus.CompareTo(target) < 0)
                modulus = modulus.Multiply(prime2);

            if (HenselLift(poly, chosen, chosenPrime, modulus) is not { } lifted)
                return null;
            return Recombine(poly, lifted, modulus);
        }

        /// <summary>
        /// The modular factors lifted from <paramref name="prime"/> to
        /// <paramref name="modulus"/>, a power of it.
        /// </summary>
        /// <remarks>
        /// One factor is peeled off at a time and the rest are carried as a single cofactor,
        /// which turns the many-factor lift into a sequence of two-factor ones. The cofactor
        /// is kept as residues rather than as an integer polynomial: it is a product of some
        /// of the true factors only when the modular factors happen to group that way, so
        /// reading it as an integer polynomial part-way through would be reading something
        /// that need not exist.
        /// </remarks>
        private static IReadOnlyList<IntegerPolynomial>? HenselLift(
            IntegerPolynomial poly, IReadOnlyList<PrimeFieldPolynomial> modular, long prime, EInteger modulus)
        {
            var lifted = new List<IntegerPolynomial>(modular.Count);
            var remaining = poly;
            for (var i = 0; i < modular.Count - 1; i++)
            {
                MultithreadingFunctional.ExitIfCancelled();
                var cofactor = PrimeFieldPolynomial.One(prime);
                for (var j = i + 1; j < modular.Count; j++)
                    cofactor = cofactor.Multiply(modular[j]);
                if (!LiftPair(remaining, modular[i], cofactor, prime, modulus, out var factor, out var rest))
                    return null;
                lifted.Add(factor);
                remaining = rest;
            }
            lifted.Add(remaining);
            return lifted;
        }

        /// <summary>
        /// Lifts <c>poly = left * right</c> from modulo <paramref name="prime"/> to modulo
        /// <paramref name="modulus"/>, one power at a time.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Writing the next pair as <c>A + m*alpha</c> and <c>B + m*beta</c> and asking that
        /// the product match one power further leaves <c>alpha*B + beta*A = e</c> modulo the
        /// prime, where <c>e</c> is the error so far divided by <c>m</c>. The Bezout pair for
        /// <c>A</c> and <c>B</c> modulo the prime solves that immediately, and reducing
        /// <c>alpha</c> below the degree of <c>A</c> — pushing the quotient into <c>beta</c> —
        /// is what keeps both sides at the degree they started with, so that the product stays
        /// the degree of the polynomial being factored. Knuth, <i>TAOCP</i> vol. 2, §4.6.2,
        /// algorithm H.
        /// </para>
        /// <para>
        /// Each round checks that the product really does agree with the input to the power
        /// just reached, and declines if it does not. The Bezout pair is computed once, from
        /// the factors modulo the prime, and stays correct because neither factor changes
        /// modulo the prime as it is lifted.
        /// </para>
        /// </remarks>
        private static bool LiftPair(
            IntegerPolynomial poly, PrimeFieldPolynomial left, PrimeFieldPolynomial right,
            long prime, EInteger modulus,
            [NotNullWhen(true)] out IntegerPolynomial? liftedLeft,
            [NotNullWhen(true)] out IntegerPolynomial? liftedRight)
        {
            liftedLeft = liftedRight = null;
            if (!TryBezout(left, right, out var leftInverse, out var rightInverse))
                return false;

            var a = IntegerPolynomial.FromPrimeField(left);
            var b = IntegerPolynomial.FromPrimeField(right);
            var step = EInteger.FromInt64(prime);
            var reached = step;

            while (reached.CompareTo(modulus) < 0)
            {
                MultithreadingFunctional.ExitIfCancelled();
                if (a.Multiply(b) is not { } product)
                    return false;
                if (poly.Subtract(product).DivideByInteger(reached) is not { } error)
                    return false;

                var e = error.ReduceModuloPrime(prime);
                var scaled = e.Multiply(rightInverse);
                var quotient = scaled.Quotient(left);
                var alpha = scaled.Subtract(quotient.Multiply(left));
                var beta = e.Multiply(leftInverse).Add(quotient.Multiply(right));
                // Both must stay under the degree of the factor they correct, or the lifted
                // pair stops being monic of the degree it started at and the product drifts
                // away from the polynomial being factored.
                if (alpha.Degree >= left.Degree || beta.Degree >= right.Degree)
                    return false;

                a = a.Add(IntegerPolynomial.FromPrimeField(alpha).ScaleBy(reached));
                b = b.Add(IntegerPolynomial.FromPrimeField(beta).ScaleBy(reached));
                reached = reached.Multiply(step);

                if (a.Multiply(b) is not { } check || !poly.Subtract(check).Modulo(reached).IsZero)
                    return false;
            }

            liftedLeft = a.Modulo(modulus);
            liftedRight = b.Modulo(modulus);
            return true;
        }

        /// <summary>
        /// The pair with <c>leftInverse * left + rightInverse * right = 1</c> over
        /// <c>F_p</c>, or <see langword="false"/> where the two are not coprime there.
        /// </summary>
        private static bool TryBezout(
            PrimeFieldPolynomial left, PrimeFieldPolynomial right,
            [NotNullWhen(true)] out PrimeFieldPolynomial? leftInverse,
            [NotNullWhen(true)] out PrimeFieldPolynomial? rightInverse)
        {
            leftInverse = rightInverse = null;
            var prime = left.Prime;
            var remainderOld = left;
            var remainder = right;
            var leftOld = PrimeFieldPolynomial.One(prime);
            var leftNew = PrimeFieldPolynomial.Zero(prime);
            var rightOld = PrimeFieldPolynomial.Zero(prime);
            var rightNew = PrimeFieldPolynomial.One(prime);

            while (!remainder.IsZero)
            {
                var quotient = remainderOld.Quotient(remainder);
                var nextRemainder = remainderOld.Subtract(quotient.Multiply(remainder));
                var nextLeft = leftOld.Subtract(quotient.Multiply(leftNew));
                var nextRight = rightOld.Subtract(quotient.Multiply(rightNew));
                remainderOld = remainder;
                remainder = nextRemainder;
                leftOld = leftNew;
                leftNew = nextLeft;
                rightOld = rightNew;
                rightNew = nextRight;
            }
            // A common factor of positive degree means the two modular factors were not
            // distinct, which the square-free step upstream should already have ruled out.
            if (remainderOld.Degree != 0)
                return false;

            var inverse = PrimeFieldPolynomial.Inverse(remainderOld.Coefficients[0], prime);
            if (inverse == 0)
                return false;
            var unit = PrimeFieldPolynomial.Create(new[] { inverse }, prime);
            leftInverse = leftOld.Multiply(unit);
            rightInverse = rightOld.Multiply(unit);
            return true;
        }

        /// <summary>
        /// The true factors, recovered by trying products of the lifted modular ones against
        /// the polynomial itself.
        /// </summary>
        /// <remarks>
        /// Subsets are tried smallest first, and once one divides it is taken out and the
        /// search starts again at the same size — a factor found early shrinks every later
        /// subset. Only sizes up to half the pool are tried, because the complement of a
        /// larger subset is a smaller one that has already been offered; whatever is left at
        /// the end is irreducible for exactly that reason.
        /// </remarks>
        private static IReadOnlyList<IntegerPolynomial>? Recombine(
            IntegerPolynomial poly, IReadOnlyList<IntegerPolynomial> lifted, EInteger modulus)
        {
            var pool = new List<IntegerPolynomial>(lifted);
            var factors = new List<IntegerPolynomial>();
            var remaining = poly;
            var budget = MaxRecombinationCandidates;

            for (var size = 1; size * 2 <= pool.Count;)
            {
                MultithreadingFunctional.ExitIfCancelled();
                var indices = new int[size];
                for (var i = 0; i < size; i++)
                    indices[i] = i;

                var found = false;
                do
                {
                    if (--budget < 0)
                        return null;
                    var product = IntegerPolynomial.One;
                    foreach (var index in indices)
                    {
                        if (product.Multiply(pool[index]) is not { } multiplied)
                            return null;
                        product = multiplied.Modulo(modulus);
                    }

                    var candidate = product.SymmetricModulo(modulus);
                    if (candidate.IsConstant || remaining.DivideExact(candidate) is not { } quotient)
                        continue;

                    factors.Add(candidate);
                    remaining = quotient;
                    for (var i = indices.Length - 1; i >= 0; i--)
                        pool.RemoveAt(indices[i]);
                    found = true;
                }
                while (!found && NextCombination(indices, pool.Count));

                if (!found)
                    size++;
            }

            if (!remaining.IsConstant)
                factors.Add(remaining);
            return factors;
        }

        /// <summary>
        /// Advances <paramref name="indices"/> to the next combination in lexicographic
        /// order, or answers <see langword="false"/> when it was the last. The order is fixed
        /// so that the same polynomial factors the same way every time.
        /// </summary>
        private static bool NextCombination(int[] indices, int poolCount)
        {
            var size = indices.Length;
            if (size > poolCount)
                return false;
            var position = size - 1;
            while (position >= 0 && indices[position] == poolCount - size + position)
                position--;
            if (position < 0)
                return false;
            indices[position]++;
            for (var i = position + 1; i < size; i++)
                indices[i] = indices[i - 1] + 1;
            return true;
        }

        private static EInteger Lcm(EInteger a, EInteger b) => a.Divide(a.Gcd(b)).Multiply(b);

        /// <summary>A polynomial written as a rational constant times powers of irreducibles.</summary>
        internal readonly struct Factorization
        {
            internal Factorization(ERational constant, IReadOnlyList<SquareFreeDecomposition.SquareFreePart> parts)
            {
                Constant = constant;
                Parts = parts;
            }

            internal ERational Constant { get; }

            internal IReadOnlyList<SquareFreeDecomposition.SquareFreePart> Parts { get; }
        }
    }
}
