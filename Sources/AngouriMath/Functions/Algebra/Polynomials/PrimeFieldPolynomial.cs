//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using PeterO.Numbers;

namespace AngouriMath.Functions
{
    /// <summary>
    /// A univariate polynomial over the prime field <c>F_p</c>, stored densely as its
    /// coefficients with the constant term first.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The point of a prime field is that it is a field with no growth in it. Over the
    /// rationals the Euclidean algorithm is unusable directly because the coefficients of
    /// the remainder sequence explode — which is why <see cref="PolynomialGcd"/> has to
    /// carry the whole subresultant apparatus. Here every coefficient is one machine word
    /// no matter how long the computation runs, so the textbook algorithms are the ones
    /// that are actually used: plain Euclid for the greatest common divisor, plain long
    /// division, square-and-multiply for powers.
    /// </para>
    /// <para>
    /// That is what makes the field the working surface for factorisation. A polynomial
    /// over the integers is factored by factoring it modulo a prime, where the problem is
    /// finite, and lifting the result back — von zur Gathen and Gerhard,
    /// <i>Modern Computer Algebra</i>, ch. 14, and Knuth, <i>TAOCP</i> vol. 2, §4.6.2.
    /// <see cref="PrimeFieldFactorization"/> is the first half of that.
    /// </para>
    /// <para>
    /// Arithmetic is <see cref="long"/> throughout, and every product of two reduced
    /// coefficients has to fit without overflow. A reduced coefficient is at most
    /// <c>p - 1</c>, an accumulator holds at most another <c>p - 1</c> before the next
    /// reduction, so the largest intermediate is <c>p(p - 1)</c> and the field is bounded
    /// by <see cref="MaxPrime"/> accordingly. Nothing here uses a wider type, because
    /// factorisation moduli are three digits and paying <see cref="EInteger"/> for them on
    /// the inner loop would be the wrong trade.
    /// </para>
    /// <para>
    /// Instances are immutable, and the coefficient array of a live instance is never
    /// written to after construction; the operations all build a fresh array. That is what
    /// lets <see cref="Coefficients"/> hand the array out rather than copying it.
    /// </para>
    /// <para>
    /// The modulus is required to be prime and this does not check it, because a check
    /// costs <c>O(sqrt p)</c> on every construction and every caller inside the library
    /// knows its own modulus. Where a composite one does reach the arithmetic it surfaces
    /// as a failure to invert rather than as a wrong answer, and
    /// <see cref="PrimeFieldFactorization.Factor"/> — the one entry point a modulus could
    /// arrive at from outside — checks primality itself.
    /// </para>
    /// </remarks>
    internal sealed class PrimeFieldPolynomial
    {
        /// <summary>
        /// The largest modulus the <see cref="long"/> arithmetic below is valid for. The
        /// binding intermediate is <c>p(p - 1)</c>, an accumulator plus one product of two
        /// reduced coefficients; at this value that is 9223372033963249500 against a
        /// <see cref="long"/> ceiling of 9223372036854775807, and one more would overflow.
        /// </summary>
        internal const long MaxPrime = 3_037_000_500L;

        /// <summary>
        /// Constant term first, no trailing zeros, each entry in <c>[0, Prime)</c>. The
        /// zero polynomial is the empty array, so the degree is the length minus one and
        /// comes out as -1 for it.
        /// </summary>
        private readonly long[] coefficients;

        private PrimeFieldPolynomial(long[] coefficients, long prime)
        {
            this.coefficients = coefficients;
            Prime = prime;
        }

        /// <summary>The characteristic of the field, in <c>[2, <see cref="MaxPrime"/>]</c>.</summary>
        internal long Prime { get; }

        /// <summary>The degree, or -1 for the zero polynomial.</summary>
        internal int Degree => coefficients.Length - 1;

        internal bool IsZero => coefficients.Length == 0;

        /// <summary>
        /// Whether the leading coefficient is one. The zero polynomial has no leading
        /// coefficient and so is not monic.
        /// </summary>
        internal bool IsMonic => coefficients.Length > 0 && coefficients[coefficients.Length - 1] == 1;

        /// <summary>
        /// Constant term first, each entry in <c>[0, <see cref="Prime"/>)</c>, with no
        /// trailing zeros — so the last entry is the leading coefficient, and the list is
        /// empty exactly for the zero polynomial.
        /// </summary>
        internal IReadOnlyList<long> Coefficients => coefficients;

        /// <summary>
        /// The polynomial with the given coefficients, constant term first, over
        /// <c>F_prime</c>. Coefficients outside <c>[0, prime)</c>, negative ones included,
        /// are reduced into it, and trailing zeros are dropped.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="prime"/> is below 2 or above <see cref="MaxPrime"/>.
        /// </exception>
        internal static PrimeFieldPolynomial Create(IReadOnlyList<long> coefficientsLowestFirst, long prime)
        {
            CheckPrime(prime);
            var reduced = new long[coefficientsLowestFirst.Count];
            for (var i = 0; i < reduced.Length; i++)
                reduced[i] = Reduce(coefficientsLowestFirst[i], prime);
            return new PrimeFieldPolynomial(Trim(reduced), prime);
        }

        internal static PrimeFieldPolynomial Zero(long prime)
        {
            CheckPrime(prime);
            return new PrimeFieldPolynomial(Array.Empty<long>(), prime);
        }

        internal static PrimeFieldPolynomial One(long prime)
        {
            CheckPrime(prime);
            return new PrimeFieldPolynomial(new long[] { 1 }, prime);
        }

        /// <summary><c>x^degree</c>.</summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="degree"/> is negative.</exception>
        internal static PrimeFieldPolynomial Monomial(int degree, long prime)
        {
            CheckPrime(prime);
            if (degree < 0)
                throw new ArgumentOutOfRangeException(nameof(degree), degree, "A monomial has a non-negative degree.");
            var monomial = new long[degree + 1];
            monomial[degree] = 1;
            return new PrimeFieldPolynomial(monomial, prime);
        }

        internal PrimeFieldPolynomial Add(PrimeFieldPolynomial other)
        {
            CheckSameField(other, nameof(other));
            var length = Math.Max(coefficients.Length, other.coefficients.Length);
            var sum = new long[length];
            for (var i = 0; i < length; i++)
            {
                // Both addends are already in [0, p), so one conditional subtraction is the
                // whole reduction and no division is needed on this path.
                var value = At(i) + other.At(i);
                sum[i] = value >= Prime ? value - Prime : value;
            }
            return new PrimeFieldPolynomial(Trim(sum), Prime);
        }

        internal PrimeFieldPolynomial Subtract(PrimeFieldPolynomial other)
        {
            CheckSameField(other, nameof(other));
            var length = Math.Max(coefficients.Length, other.coefficients.Length);
            var difference = new long[length];
            for (var i = 0; i < length; i++)
            {
                var value = At(i) - other.At(i);
                difference[i] = value < 0 ? value + Prime : value;
            }
            return new PrimeFieldPolynomial(Trim(difference), Prime);
        }

        internal PrimeFieldPolynomial Multiply(PrimeFieldPolynomial other)
        {
            CheckSameField(other, nameof(other));
            if (IsZero || other.IsZero)
                return Zero(Prime);
            var product = new long[coefficients.Length + other.coefficients.Length - 1];
            for (var i = 0; i < coefficients.Length; i++)
            {
                var left = coefficients[i];
                if (left == 0)
                    continue;
                for (var j = 0; j < other.coefficients.Length; j++)
                    product[i + j] = (product[i + j] + left * other.coefficients[j]) % Prime;
            }
            // A product of two monic polynomials is monic, so no leading term can have
            // cancelled; Trim is here for the general case, where one can.
            return new PrimeFieldPolynomial(Trim(product), Prime);
        }

        /// <summary>
        /// The quotient of the division of this by <paramref name="divisor"/>, which over a
        /// field is exact in the sense that <c>this == quotient * divisor + remainder</c>
        /// with the remainder of lower degree than the divisor.
        /// </summary>
        /// <exception cref="DivideByZeroException"><paramref name="divisor"/> is zero.</exception>
        internal PrimeFieldPolynomial Quotient(PrimeFieldPolynomial divisor)
        {
            CheckSameField(divisor, nameof(divisor));
            Divide(divisor, out var quotient, out _);
            return new PrimeFieldPolynomial(Trim(quotient), Prime);
        }

        /// <summary>
        /// The remainder of the division of this by <paramref name="divisor"/>: of lower
        /// degree than the divisor, and zero exactly when the divisor divides this.
        /// </summary>
        /// <exception cref="DivideByZeroException"><paramref name="divisor"/> is zero.</exception>
        internal PrimeFieldPolynomial Remainder(PrimeFieldPolynomial divisor)
        {
            CheckSameField(divisor, nameof(divisor));
            Divide(divisor, out _, out var remainder);
            return new PrimeFieldPolynomial(Trim(remainder), Prime);
        }

        /// <summary>
        /// The monic greatest common divisor. Zero exactly when both are zero; otherwise
        /// <c>gcd(f, 0)</c> is <c>f</c> made monic, and <c>gcd(f, f)</c> likewise.
        /// </summary>
        internal PrimeFieldPolynomial Gcd(PrimeFieldPolynomial other)
        {
            CheckSameField(other, nameof(other));
            // Plain Euclid, with no content or subresultant machinery, because over a field
            // there is no coefficient growth for either of them to control. Each remainder
            // drops the degree by at least one, so the loop is bounded by the degree.
            var left = this;
            var right = other;
            while (!right.IsZero)
            {
                var next = left.Remainder(right);
                left = right;
                right = next;
            }
            return left.MakeMonic();
        }

        /// <summary>
        /// This divided by its leading coefficient, so that the result is monic. The zero
        /// polynomial has no leading coefficient and is returned unchanged.
        /// </summary>
        internal PrimeFieldPolynomial MakeMonic()
        {
            if (IsZero || IsMonic)
                return this;
            var inverse = Inverse(coefficients[coefficients.Length - 1], Prime);
            var scaled = new long[coefficients.Length];
            for (var i = 0; i < scaled.Length; i++)
                scaled[i] = coefficients[i] * inverse % Prime;
            return new PrimeFieldPolynomial(scaled, Prime);
        }

        /// <summary>
        /// The formal derivative. In characteristic <c>p</c> this is not degree-lowering by
        /// one: the derivative of <c>x^p</c> is <c>p x^(p-1)</c>, which is zero, and more
        /// generally the derivative vanishes identically exactly on the polynomials in
        /// <c>x^p</c> — which is why a zero derivative is evidence of a <c>p</c>-th power
        /// rather than of a constant.
        /// </summary>
        internal PrimeFieldPolynomial Derivative()
        {
            if (coefficients.Length <= 1)
                return Zero(Prime);
            var derivative = new long[coefficients.Length - 1];
            for (var i = 1; i < coefficients.Length; i++)
                derivative[i - 1] = coefficients[i] * (i % Prime) % Prime;
            return new PrimeFieldPolynomial(Trim(derivative), Prime);
        }

        /// <summary>
        /// <c>this^exponent mod modulus</c>, by square-and-multiply. The exponent is an
        /// <see cref="EInteger"/> because the caller that needs this is raising to the
        /// power <c>p^k</c>, which leaves the range of <see cref="long"/> long before the
        /// computation becomes expensive.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="exponent"/> is negative.</exception>
        /// <exception cref="DivideByZeroException"><paramref name="modulus"/> is zero.</exception>
        internal PrimeFieldPolynomial PowMod(EInteger exponent, PrimeFieldPolynomial modulus)
        {
            CheckSameField(modulus, nameof(modulus));
            if (exponent.Sign < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(exponent), exponent, "A polynomial cannot be raised to a negative power.");
            // Reducing first also settles the degenerate case: modulo a nonzero constant
            // the quotient ring is trivial and every power of everything is zero there.
            var result = One(Prime).Remainder(modulus);
            var square = Remainder(modulus);
            var remaining = exponent;
            while (remaining.Sign > 0)
            {
                if (!remaining.IsEven)
                    result = result.Multiply(square).Remainder(modulus);
                remaining = remaining.ShiftRight(1);
                if (remaining.Sign > 0)
                    square = square.Multiply(square).Remainder(modulus);
            }
            return result;
        }

        /// <summary>
        /// Whether no irreducible factor occurs twice. False for the zero polynomial, which
        /// is divisible by every square.
        /// </summary>
        /// <remarks>
        /// A repeated factor survives differentiation once, so it divides both the
        /// polynomial and its derivative, and over a field the converse holds too — hence
        /// the test on <c>gcd(f, f')</c>. The characteristic-<c>p</c> case has to be taken
        /// separately: <c>f' = 0</c> makes that gcd <c>f</c> itself, and it happens exactly
        /// when <c>f</c> is a polynomial in <c>x^p</c>, which by the Frobenius map is the
        /// <c>p</c>-th power of another polynomial and so is not square-free.
        /// </remarks>
        internal bool IsSquareFree
        {
            get
            {
                if (IsZero)
                    return false;
                if (Degree == 0)
                    return true;
                var derivative = Derivative();
                if (derivative.IsZero)
                    return false;
                return Gcd(derivative).Degree == 0;
            }
        }

        /// <summary>
        /// Whether the two are the same polynomial over the same field. Polynomials over
        /// different fields are never the same, rather than an error.
        /// </summary>
        internal bool SameAs(PrimeFieldPolynomial other)
        {
            if (Prime != other.Prime || coefficients.Length != other.coefficients.Length)
                return false;
            for (var i = 0; i < coefficients.Length; i++)
                if (coefficients[i] != other.coefficients[i])
                    return false;
            return true;
        }

        /// <summary>
        /// The inverse of <paramref name="value"/> modulo <paramref name="prime"/>, by the
        /// extended Euclidean algorithm. Exposed because the Gaussian elimination in
        /// <see cref="PrimeFieldFactorization"/> works on the field rather than on
        /// polynomials over it and needs the same inversion.
        /// </summary>
        /// <exception cref="DivideByZeroException">
        /// <paramref name="value"/> has no inverse — it is zero modulo
        /// <paramref name="prime"/>, or the modulus is not prime.
        /// </exception>
        internal static long Inverse(long value, long prime)
        {
            var reduced = Reduce(value, prime);
            if (reduced == 0)
                throw new DivideByZeroException("Zero is not invertible in a field.");
            // Bezout coefficients of (reduced, prime); the one on reduced is the inverse
            // once the gcd has come out as one. Both stay bounded by the modulus, so this
            // cannot overflow where the modulus itself is within range.
            long remainder = reduced, previousRemainder = prime;
            long coefficient = 1, previousCoefficient = 0;
            while (remainder != 0)
            {
                var quotient = previousRemainder / remainder;
                (previousRemainder, remainder) = (remainder, previousRemainder - quotient * remainder);
                (previousCoefficient, coefficient) = (coefficient, previousCoefficient - quotient * coefficient);
            }
            if (previousRemainder != 1)
                throw new DivideByZeroException(
                    $"{reduced} is not invertible modulo {prime}, so {prime} is not prime.");
            return Reduce(previousCoefficient, prime);
        }

        public override string ToString()
        {
            if (IsZero)
                return $"0 (mod {Prime})";
            var written = new System.Text.StringBuilder();
            for (var i = coefficients.Length - 1; i >= 0; i--)
            {
                if (coefficients[i] == 0)
                    continue;
                if (written.Length > 0)
                    written.Append(" + ");
                written.Append(coefficients[i]);
                if (i > 0)
                    written.Append("x^").Append(i);
            }
            return $"{written} (mod {Prime})";
        }

        private long At(int power) => power < coefficients.Length ? coefficients[power] : 0;

        private static long Reduce(long value, long prime)
        {
            // C#'s % takes the sign of the dividend, and every invariant here is that a
            // coefficient lies in [0, p), so a negative input needs the one correction.
            var reduced = value % prime;
            return reduced < 0 ? reduced + prime : reduced;
        }

        private static void CheckPrime(long prime)
        {
            if (prime < 2 || prime > MaxPrime)
                throw new ArgumentOutOfRangeException(
                    nameof(prime), prime, $"A modulus has to lie in [2, {MaxPrime}] for the arithmetic to be exact.");
        }

        private void CheckSameField(PrimeFieldPolynomial other, string parameterName)
        {
            if (other.Prime != Prime)
                throw new ArgumentException(
                    $"Cannot combine a polynomial over F_{Prime} with one over F_{other.Prime}.", parameterName);
        }

        /// <summary>
        /// Drops trailing zeros, so that the length of the array is one more than the
        /// degree and every representation of a given polynomial is the same array.
        /// </summary>
        private static long[] Trim(long[] coefficients)
        {
            var degree = coefficients.Length - 1;
            while (degree >= 0 && coefficients[degree] == 0)
                degree--;
            if (degree == coefficients.Length - 1)
                return coefficients;
            var trimmed = new long[degree + 1];
            Array.Copy(coefficients, trimmed, degree + 1);
            return trimmed;
        }

        /// <summary>
        /// Schoolbook long division. Both outputs are untrimmed and belong to the caller.
        /// </summary>
        private void Divide(PrimeFieldPolynomial divisor, out long[] quotient, out long[] remainder)
        {
            if (divisor.IsZero)
                throw new DivideByZeroException("A polynomial cannot be divided by the zero polynomial.");
            remainder = new long[coefficients.Length];
            Array.Copy(coefficients, remainder, coefficients.Length);
            var divisorDegree = divisor.Degree;
            quotient = new long[Math.Max(0, coefficients.Length - divisorDegree)];
            var leadInverse = Inverse(divisor.coefficients[divisorDegree], Prime);
            for (var shift = Degree - divisorDegree; shift >= 0; shift--)
            {
                if (remainder[shift + divisorDegree] == 0)
                    continue;
                var factor = remainder[shift + divisorDegree] * leadInverse % Prime;
                quotient[shift] = factor;
                for (var i = 0; i <= divisorDegree; i++)
                {
                    var value = remainder[shift + i] - factor * divisor.coefficients[i] % Prime;
                    remainder[shift + i] = value < 0 ? value + Prime : value;
                }
            }
        }
    }
}
