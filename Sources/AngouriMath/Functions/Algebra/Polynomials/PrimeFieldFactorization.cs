//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using PeterO.Numbers;
using System;

namespace AngouriMath.Functions
{
    /// <summary>
    /// Factorisation of a monic square-free polynomial over the prime field <c>F_p</c> into
    /// monic irreducibles, by Berlekamp's algorithm.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The algorithm rests on one observation. Raising to the <c>p</c>-th power is a field
    /// automorphism fixing <c>F_p</c> pointwise, so on the quotient ring <c>F_p[x]/(f)</c>
    /// it is a linear map, and the polynomials it fixes — the <i>Berlekamp subalgebra</i>
    /// <c>{ v : v^p = v mod f }</c> — form a vector space. By the Chinese remainder theorem
    /// <c>F_p[x]/(f)</c> is the product of the fields <c>F_p[x]/(f_i)</c> over the
    /// irreducible factors of <c>f</c>, and in each of those the fixed elements are exactly
    /// the constants. So the subalgebra is a product of <c>r</c> copies of <c>F_p</c>, its
    /// dimension is <c>r</c>, and that dimension <i>is</i> the number of irreducible
    /// factors — which is what gives the loop below an exact termination test rather than a
    /// heuristic one. Knuth, <i>TAOCP</i> vol. 2, §4.6.2, algorithm B; von zur Gathen and
    /// Gerhard, <i>Modern Computer Algebra</i>, §14.8.
    /// </para>
    /// <para>
    /// Given a <c>v</c> in that subalgebra, <c>v^p - v = prod_{s in F_p} (v - s)</c>
    /// vanishes modulo <c>f</c>, and the factors on the right are pairwise coprime, so
    /// <c>f = prod_s gcd(f, v - s)</c>. A <c>v</c> that is not constant takes different
    /// values in different components and therefore separates them; running over a basis of
    /// the subalgebra separates all of them.
    /// </para>
    /// <para>
    /// Berlekamp rather than Cantor–Zassenhaus, and the reason is not speed — for a large
    /// modulus Cantor–Zassenhaus wins, because it replaces the <c>O(p)</c> sweep over
    /// <c>s</c> with a random probe. It is that Cantor–Zassenhaus is randomised, and design
    /// principle 3 of
    /// <a href="https://github.com/asc-community/AngouriMath/issues/746">#746</a> requires
    /// the same input to give the same answer on every platform and in every thread count.
    /// Berlekamp is deterministic by construction: the matrix, its null space and the sweep
    /// over <c>s</c> are all fixed by the input.
    /// </para>
    /// <para>
    /// The price is that the sweep costs <c>O(p n^3)</c> field operations in the worst case,
    /// linear in the modulus, so this is only sensible for a small one — hence
    /// <see cref="MaxPrime"/>. That is not a restriction in practice, because the caller
    /// chooses the modulus: factoring over the integers means picking a prime at which the
    /// polynomial stays square-free and lifting the result, and there is no reason for that
    /// prime to be large.
    /// </para>
    /// </remarks>
    internal static class PrimeFieldFactorization
    {
        /// <summary>
        /// Moduli past this are refused. The splitting sweep runs over every element of the
        /// field, so the cost is linear in the modulus where nothing else here is; at this
        /// value together with <see cref="MaxDegree"/> the worst case is under 3*10^8 field
        /// operations, which is a ceiling on a pathological input rather than a typical
        /// cost. This is far below <see cref="PrimeFieldPolynomial.MaxPrime"/>, which bounds
        /// what the arithmetic can represent rather than what this can afford.
        /// </summary>
        internal const long MaxPrime = 1024L;

        /// <summary>
        /// Degrees past this are refused. Building the Berlekamp matrix is <c>O(n^3)</c> and
        /// splitting is <c>O(p n^3)</c>; the bound is a ceiling so that a caller handing
        /// over something outsized gets a refusal rather than a hang.
        /// </summary>
        internal const int MaxDegree = 64;

        /// <summary>
        /// The monic irreducible factors of a monic square-free polynomial over
        /// <c>F_p</c>, in a deterministic order — by degree, then by coefficient from the
        /// leading one down. Their product is the input.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Answers <see langword="null"/> where it declines, which is every case it is not
        /// contracted for rather than only the expensive ones, since a caller that hands
        /// over the wrong shape wants to hear so:
        /// </para>
        /// <list type="bullet">
        /// <item><description>the modulus is composite, or above <see cref="MaxPrime"/>;</description></item>
        /// <item><description>the degree is above <see cref="MaxDegree"/>;</description></item>
        /// <item><description>
        /// the polynomial is not monic — which includes the zero polynomial. A non-monic
        /// one is the caller's to normalise, because the unit it drops belongs in whatever
        /// the caller is reassembling;
        /// </description></item>
        /// <item><description>
        /// the polynomial is not square-free. Berlekamp counts the <i>distinct</i>
        /// irreducible factors, so on a repeated one it would return a product short of the
        /// input, and that is a wrong answer rather than a partial one. Square-free
        /// decomposition comes first, and is the caller's.
        /// </description></item>
        /// </list>
        /// <para>
        /// The constant polynomial 1 is monic and square-free, and its factorisation is the
        /// empty product — so an empty list, not <see langword="null"/>.
        /// </para>
        /// </remarks>
        internal static IReadOnlyList<PrimeFieldPolynomial>? Factor(PrimeFieldPolynomial poly)
        {
            var prime = poly.Prime;
            if (prime > MaxPrime || !IsPrime(prime))
                return null;
            if (!poly.IsMonic || poly.Degree > MaxDegree)
                return null;
            if (poly.Degree == 0)
                return Array.Empty<PrimeFieldPolynomial>();
            if (!poly.IsSquareFree)
                return null;

            var subalgebra = SubalgebraBasis(poly);
            // The dimension of the Berlekamp subalgebra is the number of irreducible
            // factors exactly, so this is a count and not an estimate.
            var expected = subalgebra.Count;
            if (expected == 1)
                return new[] { poly };

            var factors = new List<PrimeFieldPolynomial> { poly };
            foreach (var separator in subalgebra)
            {
                if (factors.Count == expected)
                    break;
                // The constants lie in the subalgebra and take one value everywhere, so
                // they separate nothing; one of them is always in the basis.
                if (separator.Degree <= 0)
                    continue;
                factors = SplitBy(factors, separator, prime);
            }

            // Unreachable on the mathematics — a basis of the subalgebra separates every
            // pair of factors — so this is the guard that says so rather than returning a
            // product that is not the input.
            if (factors.Count != expected)
                return null;

            factors.Sort(Compare);
            return factors;
        }

        /// <summary>
        /// Each element of <paramref name="factors"/> replaced by the pieces
        /// <paramref name="separator"/> cuts it into. Since <c>g</c> divides the input and
        /// the input divides <c>prod_s (v - s)</c>, the greatest common divisors taken here
        /// multiply back to <c>g</c> whether or not any of them is proper.
        /// </summary>
        private static List<PrimeFieldPolynomial> SplitBy(
            List<PrimeFieldPolynomial> factors, PrimeFieldPolynomial separator, long prime)
        {
            var split = new List<PrimeFieldPolynomial>(factors.Count);
            var constant = new long[1];
            foreach (var factor in factors)
            {
                if (factor.Degree <= 1)
                {
                    split.Add(factor);
                    continue;
                }
                for (var value = 0L; value < prime; value++)
                {
                    constant[0] = value;
                    var piece = factor.Gcd(separator.Subtract(PrimeFieldPolynomial.Create(constant, prime)));
                    if (piece.Degree >= 1)
                        split.Add(piece);
                }
            }
            return split;
        }

        /// <summary>
        /// A basis of <c>{ v : v^p = v mod f }</c>, as polynomials of degree below that of
        /// <paramref name="poly"/>.
        /// </summary>
        private static List<PrimeFieldPolynomial> SubalgebraBasis(PrimeFieldPolynomial poly)
        {
            var size = poly.Degree;
            var prime = poly.Prime;

            // Row i of the Berlekamp matrix is x^(p*i) mod f, which is where the Frobenius
            // map sends x^i. One modular exponentiation gives x^p; the rest are one
            // multiplication each, which is what keeps this O(n^3) rather than O(n^3 log p).
            var frobenius = PrimeFieldPolynomial.Monomial(1, prime).PowMod(EInteger.FromInt64(prime), poly);
            var image = new long[size][];
            var power = PrimeFieldPolynomial.One(prime);
            for (var i = 0; i < size; i++)
            {
                image[i] = new long[size];
                var coefficients = power.Coefficients;
                for (var j = 0; j < coefficients.Count; j++)
                    image[i][j] = coefficients[j];
                if (i + 1 < size)
                    power = power.Multiply(frobenius).Remainder(poly);
            }

            // v is fixed iff the row vector of its coefficients is annihilated by Q - I on
            // the right, so the system to solve is the transpose of that.
            var system = new long[size][];
            for (var row = 0; row < size; row++)
            {
                system[row] = new long[size];
                for (var column = 0; column < size; column++)
                {
                    var value = image[column][row] - (column == row ? 1 : 0);
                    system[row][column] = value < 0 ? value + prime : value;
                }
            }

            var basis = new List<PrimeFieldPolynomial>();
            foreach (var vector in NullSpace(system, size, prime))
                basis.Add(PrimeFieldPolynomial.Create(vector, prime));
            return basis;
        }

        /// <summary>
        /// A basis of the null space of a square matrix over <c>F_p</c>, one vector per
        /// column left without a pivot by reduction to row echelon form. The order is by
        /// that column, so it depends on the matrix alone.
        /// </summary>
        private static List<long[]> NullSpace(long[][] matrix, int size, long prime)
        {
            var pivotRowOfColumn = new int[size];
            for (var column = 0; column < size; column++)
                pivotRowOfColumn[column] = -1;

            var pivotRow = 0;
            for (var column = 0; column < size && pivotRow < size; column++)
            {
                var found = -1;
                for (var row = pivotRow; row < size; row++)
                    if (matrix[row][column] != 0)
                    {
                        found = row;
                        break;
                    }
                if (found < 0)
                    continue;
                (matrix[pivotRow], matrix[found]) = (matrix[found], matrix[pivotRow]);

                var inverse = PrimeFieldPolynomial.Inverse(matrix[pivotRow][column], prime);
                for (var c = column; c < size; c++)
                    matrix[pivotRow][c] = matrix[pivotRow][c] * inverse % prime;

                // Eliminating above as well as below leaves the matrix in reduced echelon
                // form, which is what lets a free column be read off in one pass below.
                for (var row = 0; row < size; row++)
                {
                    if (row == pivotRow || matrix[row][column] == 0)
                        continue;
                    var factor = matrix[row][column];
                    for (var c = column; c < size; c++)
                    {
                        var value = matrix[row][c] - factor * matrix[pivotRow][c] % prime;
                        matrix[row][c] = value < 0 ? value + prime : value;
                    }
                }

                pivotRowOfColumn[column] = pivotRow;
                pivotRow++;
            }

            var basis = new List<long[]>(size - pivotRow);
            for (var free = 0; free < size; free++)
            {
                if (pivotRowOfColumn[free] >= 0)
                    continue;
                var vector = new long[size];
                vector[free] = 1;
                // Setting the free coordinate to one forces every pivot coordinate, since
                // its row reads "pivot + (the free ones) = 0".
                for (var column = 0; column < size; column++)
                {
                    var row = pivotRowOfColumn[column];
                    if (row >= 0 && matrix[row][free] != 0)
                        vector[column] = prime - matrix[row][free];
                }
                basis.Add(vector);
            }
            return basis;
        }

        /// <summary>
        /// A total order on the factors: shorter first, then by coefficient from the
        /// leading one down. Any total order would do; what matters is that the output does
        /// not depend on the order in which the splitting happened to find them.
        /// </summary>
        private static int Compare(PrimeFieldPolynomial left, PrimeFieldPolynomial right)
        {
            if (left.Degree != right.Degree)
                return left.Degree.CompareTo(right.Degree);
            for (var i = left.Degree; i >= 0; i--)
            {
                var order = left.Coefficients[i].CompareTo(right.Coefficients[i]);
                if (order != 0)
                    return order;
            }
            return 0;
        }

        /// <summary>
        /// Trial division, which is ample below <see cref="MaxPrime"/> and is the only
        /// place the modulus is checked at all — the arithmetic itself takes primality on
        /// trust, see <see cref="PrimeFieldPolynomial"/>.
        /// </summary>
        private static bool IsPrime(long value)
        {
            if (value < 2)
                return false;
            if (value % 2 == 0)
                return value == 2;
            for (var divisor = 3L; divisor * divisor <= value; divisor += 2)
                if (value % divisor == 0)
                    return false;
            return true;
        }
    }
}
