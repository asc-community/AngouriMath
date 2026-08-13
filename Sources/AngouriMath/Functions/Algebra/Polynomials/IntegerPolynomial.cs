//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using PeterO.Numbers;
using System;
using static AngouriMath.Entity;

namespace AngouriMath.Functions
{
    /// <summary>
    /// A polynomial in one variable over the integers, stored densely.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A second polynomial representation next to <see cref="MultivariatePolynomial"/>, and
    /// deliberately so. That one is sparse, packs eight exponents into a <c>ulong</c> and
    /// carries coefficients over <c>Q</c>, which is what a multivariate greatest common
    /// divisor wants. Factoring wants the opposite of all three: one variable, dense
    /// coefficient access by degree, and coefficients over <c>Z</c>, because every bound the
    /// algorithm relies on — Mignotte's on the size of a factor, the modulus a Hensel lift
    /// has to reach — is a statement about integers. Sharing one type between the two would
    /// mean paying a dictionary lookup per coefficient in the inner loop of the lift and
    /// re-deriving a denominator that is known to be one.
    /// </para>
    /// <para>
    /// Trailing zero coefficients are never stored, so <see cref="Degree"/> is
    /// <c>coefficients.Length - 1</c> and the zero polynomial is the empty array with degree
    /// <c>-1</c>. Coefficients are ordered lowest power first, which is the order the rest of
    /// <c>Functions/Algebra/Polynomials</c> already uses.
    /// </para>
    /// </remarks>
    internal sealed class IntegerPolynomial
    {
        /// <summary>
        /// Past this the factoriser refuses. Recombination is exponential in the number of
        /// modular factors and that number is bounded by the degree, so the ceiling is on
        /// the one quantity that bounds every stage.
        /// </summary>
        internal const int MaxDegree = 32;

        [ConstantField] private static readonly EInteger[] NoCoefficients = Array.Empty<EInteger>();

        /// <summary>Lowest power first, with no trailing zero.</summary>
        private readonly EInteger[] coefficients;

        private IntegerPolynomial(EInteger[] coefficients) => this.coefficients = coefficients;

        internal static IntegerPolynomial Create(IReadOnlyList<EInteger> coefficientsLowestFirst)
        {
            var length = coefficientsLowestFirst.Count;
            while (length > 0 && coefficientsLowestFirst[length - 1].IsZero)
                length--;
            if (length == 0)
                return Zero;
            var trimmed = new EInteger[length];
            for (var i = 0; i < length; i++)
                trimmed[i] = coefficientsLowestFirst[i];
            return new(trimmed);
        }

        [ConstantField] internal static readonly IntegerPolynomial Zero = new(NoCoefficients);

        internal static IntegerPolynomial Constant(EInteger value)
            => value.IsZero ? Zero : new(new[] { value });

        internal static IntegerPolynomial One => Constant(EInteger.One);

        /// <summary><c>-1</c> for the zero polynomial, so that a degree comparison is total.</summary>
        internal int Degree => coefficients.Length - 1;

        internal bool IsZero => coefficients.Length == 0;

        internal bool IsConstant => coefficients.Length <= 1;

        internal EInteger this[int power]
            => power >= 0 && power < coefficients.Length ? coefficients[power] : EInteger.Zero;

        internal EInteger Leading => IsZero ? EInteger.Zero : coefficients[coefficients.Length - 1];

        internal IntegerPolynomial Negate() => ScaleBy(EInteger.FromInt32(-1));

        internal IntegerPolynomial ScaleBy(EInteger factor)
        {
            if (factor.IsZero || IsZero)
                return Zero;
            var scaled = new EInteger[coefficients.Length];
            for (var i = 0; i < scaled.Length; i++)
                scaled[i] = coefficients[i].Multiply(factor);
            return new(scaled);
        }

        internal IntegerPolynomial Add(IntegerPolynomial other) => Combine(other, subtract: false);

        internal IntegerPolynomial Subtract(IntegerPolynomial other) => Combine(other, subtract: true);

        private IntegerPolynomial Combine(IntegerPolynomial other, bool subtract)
        {
            var length = Math.Max(coefficients.Length, other.coefficients.Length);
            var result = new EInteger[length];
            for (var i = 0; i < length; i++)
            {
                var right = other[i];
                result[i] = subtract ? this[i].Subtract(right) : this[i].Add(right);
            }
            return Create(result);
        }

        internal IntegerPolynomial? Multiply(IntegerPolynomial other)
        {
            if (IsZero || other.IsZero)
                return Zero;
            if (Degree + other.Degree > MaxDegree)
                return null;
            var result = new EInteger[Degree + other.Degree + 1];
            for (var i = 0; i < result.Length; i++)
                result[i] = EInteger.Zero;
            for (var i = 0; i < coefficients.Length; i++)
                for (var j = 0; j < other.coefficients.Length; j++)
                    result[i + j] = result[i + j].Add(coefficients[i].Multiply(other.coefficients[j]));
            return Create(result);
        }

        /// <summary>Multiplied by <c>x ^ power</c>.</summary>
        internal IntegerPolynomial ShiftedBy(int power)
        {
            if (IsZero || power == 0)
                return this;
            var result = new EInteger[coefficients.Length + power];
            for (var i = 0; i < power; i++)
                result[i] = EInteger.Zero;
            Array.Copy(coefficients, 0, result, power, coefficients.Length);
            return new(result);
        }

        internal IntegerPolynomial Derivative()
        {
            if (coefficients.Length <= 1)
                return Zero;
            var result = new EInteger[coefficients.Length - 1];
            for (var i = 1; i < coefficients.Length; i++)
                result[i - 1] = coefficients[i].Multiply(EInteger.FromInt32(i));
            return Create(result);
        }

        /// <summary>
        /// <paramref name="divisor"/> divided out over the integers, or <see langword="null"/>
        /// where the division leaves a remainder or a coefficient that is not whole.
        /// </summary>
        /// <remarks>
        /// Exactness over <c>Z</c> rather than over <c>Q</c> is the point: this is what every
        /// candidate factor is tested with, and a candidate that only divides over <c>Q</c> is
        /// not a factor of an integer polynomial.
        /// </remarks>
        internal IntegerPolynomial? DivideExact(IntegerPolynomial divisor)
        {
            if (divisor.IsZero)
                return null;
            if (IsZero)
                return Zero;
            if (Degree < divisor.Degree)
                return null;

            var remainder = new EInteger[coefficients.Length];
            Array.Copy(coefficients, remainder, coefficients.Length);
            var quotient = new EInteger[Degree - divisor.Degree + 1];
            for (var i = 0; i < quotient.Length; i++)
                quotient[i] = EInteger.Zero;
            var divisorLead = divisor.Leading;

            for (var power = Degree; power >= divisor.Degree; power--)
            {
                if (remainder[power].IsZero)
                    continue;
                if (!remainder[power].Remainder(divisorLead).IsZero)
                    return null;
                var divided = remainder[power].Divide(divisorLead);
                var shift = power - divisor.Degree;
                quotient[shift] = divided;
                for (var i = 0; i <= divisor.Degree; i++)
                    remainder[i + shift] = remainder[i + shift].Subtract(divided.Multiply(divisor[i]));
            }
            for (var i = 0; i < remainder.Length; i++)
                if (!remainder[i].IsZero)
                    return null;
            return Create(quotient);
        }

        /// <summary>The greatest common divisor of the coefficients, positive; zero for the zero polynomial.</summary>
        internal EInteger Content()
        {
            var content = EInteger.Zero;
            foreach (var coefficient in coefficients)
                content = content.Gcd(coefficient);
            return content;
        }

        /// <summary>
        /// The same polynomial divided by its content and signed so that the leading
        /// coefficient is positive — the representative of its class up to a unit of
        /// <c>Z</c>, which is what makes two greatest common divisors comparable.
        /// </summary>
        internal IntegerPolynomial PrimitivePart()
        {
            if (IsZero)
                return Zero;
            var content = Content();
            if (Leading.Sign < 0)
                content = content.Negate();
            var result = new EInteger[coefficients.Length];
            for (var i = 0; i < result.Length; i++)
                result[i] = coefficients[i].Divide(content);
            return new(result);
        }

        /// <summary>
        /// <c>lc(divisor) ^ (deg(dividend) - deg(divisor) + 1) * dividend</c> reduced modulo
        /// <paramref name="divisor"/>. The power in front is what keeps every division by the
        /// leading coefficient whole, so no denominator enters the coefficient ring.
        /// </summary>
        private static IntegerPolynomial? PseudoRemainder(IntegerPolynomial dividend, IntegerPolynomial divisor)
        {
            if (divisor.IsZero)
                return null;
            if (dividend.Degree < divisor.Degree)
                return dividend;
            var divisorLead = divisor.Leading;
            var remainder = dividend;
            var outstanding = dividend.Degree - divisor.Degree + 1;
            while (!remainder.IsZero && remainder.Degree >= divisor.Degree)
            {
                var shift = remainder.Degree - divisor.Degree;
                var scaled = remainder.ScaleBy(divisorLead);
                var cancelling = divisor.ScaleBy(remainder.Leading).ShiftedBy(shift);
                remainder = scaled.Subtract(cancelling);
                outstanding--;
            }
            for (var i = 0; i < outstanding; i++)
                remainder = remainder.ScaleBy(divisorLead);
            return remainder;
        }

        /// <summary>
        /// The greatest common divisor in <c>Z[x]</c>, with a positive leading coefficient.
        /// </summary>
        /// <remarks>
        /// <para>
        /// By Gauss's lemma the divisor splits as the greatest common divisor of the two
        /// integer contents times that of the two primitive parts, so the integer part is
        /// taken out first and the polynomial part is left to a remainder sequence.
        /// </para>
        /// <para>
        /// The sequence is the <i>primitive</i> one: the primitive part is taken at every
        /// step, which is the cheapest way to stop pseudo-division's exponential coefficient
        /// growth — the alternative, the subresultant sequence used by
        /// <see cref="PolynomialGcd"/>, is faster but needs bookkeeping that buys nothing at
        /// the degrees this type is capped at. Knuth, <i>TAOCP</i> vol. 2, §4.6.1, algorithm E.
        /// </para>
        /// <para>
        /// The result really is a divisor of both: it is the last nonzero member of a
        /// sequence in which each member divides the previous two, and callers that cannot
        /// afford to be wrong about it — <see cref="SquareFreeDecomposition"/> divides by it —
        /// use <see cref="DivideExact"/>, which answers <see langword="null"/> rather than
        /// rounding.
        /// </para>
        /// </remarks>
        internal static IntegerPolynomial Gcd(IntegerPolynomial left, IntegerPolynomial right)
        {
            if (left.IsZero && right.IsZero)
                return Zero;
            if (left.IsZero)
                return right.PrimitivePart().ScaleBy(right.Content());
            if (right.IsZero)
                return left.PrimitivePart().ScaleBy(left.Content());

            var contentGcd = left.Content().Gcd(right.Content());
            var a = left.PrimitivePart();
            var b = right.PrimitivePart();
            if (a.Degree < b.Degree)
                (a, b) = (b, a);

            // As long as the degree strictly falls each round the loop ends; the ceiling is
            // there so that a divisor that unexpectedly declines shows up as a refusal
            // rather than a hang.
            for (var step = 0; step <= MaxDegree + 1; step++)
            {
                if (b.IsZero)
                    return a.ScaleBy(contentGcd);
                if (b.IsConstant)
                    return Constant(contentGcd);
                if (PseudoRemainder(a, b) is not { } remainder)
                    return Constant(contentGcd);
                if (remainder.IsZero)
                    return b.ScaleBy(contentGcd);
                a = b;
                b = remainder.PrimitivePart();
            }
            return Constant(contentGcd);
        }

        /// <summary>
        /// An upper bound on the absolute value of any coefficient of any divisor of this
        /// polynomial in <c>Z[x]</c>.
        /// </summary>
        /// <remarks>
        /// Mignotte's bound: a factor of degree <c>m</c> of a polynomial <c>f</c> of degree
        /// <c>n</c> has every coefficient at most <c>2^m * |f|_2</c> in absolute value, and
        /// <c>m &lt;= n</c> gives the uniform bound used here. It is what tells the Hensel
        /// lift when to stop: once the modulus exceeds twice this, a coefficient in the
        /// symmetric range modulo it is the coefficient itself rather than a residue.
        /// Mignotte, <i>An inequality about factors of polynomials</i>, Math. Comp. 28 (1974);
        /// von zur Gathen and Gerhard, <i>Modern Computer Algebra</i>, §6.6 and §15.2.
        /// </remarks>
        internal EInteger FactorCoefficientBound()
        {
            var squared = EInteger.Zero;
            foreach (var coefficient in coefficients)
                squared = squared.Add(coefficient.Multiply(coefficient));
            // Rounded up, since the integer square root rounds down and the bound has to hold.
            var norm = squared.Sqrt().Add(EInteger.One);
            return norm.Multiply(EInteger.FromInt32(2).Pow(Degree < 0 ? 0 : Degree));
        }

        /// <summary>Every coefficient divided by <paramref name="divisor"/>, or
        /// <see langword="null"/> where one of them does not divide.</summary>
        internal IntegerPolynomial? DivideByInteger(EInteger divisor)
        {
            if (divisor.IsZero)
                return null;
            var result = new EInteger[coefficients.Length];
            for (var i = 0; i < result.Length; i++)
            {
                if (!coefficients[i].Remainder(divisor).IsZero)
                    return null;
                result[i] = coefficients[i].Divide(divisor);
            }
            return Create(result);
        }

        /// <summary>The coefficients reduced into <c>[0, modulus)</c>.</summary>
        internal IntegerPolynomial Modulo(EInteger modulus)
        {
            var result = new EInteger[coefficients.Length];
            for (var i = 0; i < result.Length; i++)
            {
                var residue = coefficients[i].Remainder(modulus);
                result[i] = residue.Sign < 0 ? residue.Add(modulus) : residue;
            }
            return Create(result);
        }

        /// <summary>
        /// The coefficients taken in the symmetric range <c>(-modulus/2, modulus/2]</c> — the
        /// representative that is the integer itself, rather than a residue standing for it,
        /// once the modulus has passed twice <see cref="FactorCoefficientBound"/>.
        /// </summary>
        internal IntegerPolynomial SymmetricModulo(EInteger modulus)
        {
            var half = modulus.Divide(EInteger.FromInt32(2));
            var result = new EInteger[coefficients.Length];
            for (var i = 0; i < result.Length; i++)
            {
                var residue = coefficients[i].Remainder(modulus);
                if (residue.Sign < 0)
                    residue = residue.Add(modulus);
                result[i] = residue.CompareTo(half) > 0 ? residue.Subtract(modulus) : residue;
            }
            return Create(result);
        }

        /// <summary>
        /// The reduction modulo <paramref name="prime"/>, whatever it does to the degree.
        /// <see cref="ToPrimeField"/> is the one to use where a dropped degree matters.
        /// </summary>
        internal PrimeFieldPolynomial ReduceModuloPrime(long prime)
        {
            var modulus = EInteger.FromInt64(prime);
            var residues = new long[coefficients.Length];
            for (var i = 0; i < residues.Length; i++)
            {
                var residue = coefficients[i].Remainder(modulus);
                if (residue.Sign < 0)
                    residue = residue.Add(modulus);
                residues[i] = residue.ToInt64Checked();
            }
            return PrimeFieldPolynomial.Create(residues, prime);
        }

        internal bool SameAs(IntegerPolynomial other)
        {
            if (coefficients.Length != other.coefficients.Length)
                return false;
            for (var i = 0; i < coefficients.Length; i++)
                if (coefficients[i].CompareTo(other.coefficients[i]) != 0)
                    return false;
            return true;
        }

        /// <summary>
        /// The polynomial over <c>F_p</c> this one reduces to, or <see langword="null"/> if
        /// the reduction drops the degree — which happens exactly when <c>p</c> divides the
        /// leading coefficient, and which makes the reduction useless for factoring.
        /// </summary>
        internal PrimeFieldPolynomial? ToPrimeField(long prime)
        {
            if (IsZero)
                return null;
            var modulus = EInteger.FromInt64(prime);
            var residues = new long[coefficients.Length];
            for (var i = 0; i < residues.Length; i++)
            {
                var residue = coefficients[i].Remainder(modulus);
                if (residue.Sign < 0)
                    residue = residue.Add(modulus);
                residues[i] = residue.ToInt64Checked();
            }
            var reduced = PrimeFieldPolynomial.Create(residues, prime);
            return reduced.Degree == Degree ? reduced : null;
        }

        internal static IntegerPolynomial FromPrimeField(PrimeFieldPolynomial poly)
        {
            var lifted = new EInteger[poly.Degree + 1];
            for (var i = 0; i < lifted.Length; i++)
                lifted[i] = EInteger.FromInt64(poly.Coefficients[i]);
            return Create(lifted);
        }

        /// <summary>
        /// The same polynomial as an expression in <paramref name="x"/>, highest power first
        /// and with a negative coefficient written as a subtraction rather than as the
        /// addition of a negative.
        /// </summary>
        internal Entity ToEntity(Variable x)
        {
            Entity? result = null;
            for (var power = coefficients.Length - 1; power >= 0; power--)
            {
                var coefficient = coefficients[power];
                if (coefficient.IsZero)
                    continue;
                var negative = coefficient.Sign < 0;
                var magnitude = negative ? coefficient.Negate() : coefficient;
                Entity term;
                if (power == 0)
                    term = Integer.Create(magnitude);
                else
                {
                    Entity raised = power == 1 ? x : MathS.Pow(x, power);
                    term = magnitude.CompareTo(EInteger.One) == 0
                        ? raised
                        : Integer.Create(magnitude) * raised;
                }
                result = result is null
                    ? negative ? -term : term
                    : negative ? result - term : result + term;
            }
            return result ?? Integer.Create(0);
        }

        public override string ToString()
        {
            if (IsZero)
                return "0";
            var parts = new List<string>();
            for (var i = coefficients.Length - 1; i >= 0; i--)
            {
                if (coefficients[i].IsZero)
                    continue;
                parts.Add(i == 0 ? coefficients[i].ToString()
                    : i == 1 ? coefficients[i] + "x"
                    : coefficients[i] + "x^" + i);
            }
            return string.Join(" + ", parts);
        }
    }
}
