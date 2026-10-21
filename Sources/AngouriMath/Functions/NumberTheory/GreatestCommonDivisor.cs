//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Numerics;
using PeterO.Numbers;

namespace AngouriMath
{
    /// <summary>
    /// The greatest common divisor of two integers, and a fraction in lowest terms by it: the
    /// numerator and the denominator divided by their greatest common divisor. Both answer
    /// exactly what PeterO's <see cref="EInteger.Gcd(EInteger)"/> and
    /// <see cref="ERational.ToLowestTerms"/> answer, and every greatest common divisor and
    /// reduction in the library goes through them instead.
    /// </summary>
    /// <remarks>
    /// <para>
    /// PeterO's greatest common divisor is fast on coprime operands and slow, without bound,
    /// when the divisor itself is large: on a pair of 2658 and 2249 bits sharing a factor of
    /// 1117 bits it takes 1.5 s, where Euclid's algorithm takes 1.6 ms and the BCL's
    /// <see cref="BigInteger.GreatestCommonDivisor"/> 14 µs; a shared factor of 5000 bits does
    /// not finish in 30 s. A sum of two fractions with a large common denominator is exactly
    /// such a pair, and every <see cref="Entity.Number.Rational"/> is reduced on creation, so an
    /// integrand evaluated at a sample point whose fractions grew to a few thousand bits spent
    /// nearly all its time reducing them.
    /// </para>
    /// <para>
    /// Operands within 64 bits run Euclid's algorithm in machine words, which reduces the small
    /// fractions the simplifier makes in about half PeterO's time. Larger ones are converted to
    /// <see cref="BigInteger"/> by their two's-complement bytes, which both types read and write,
    /// and the BCL's Lehmer algorithm answers: faster than PeterO's at every size measured, from
    /// 64 bits to 100,000, coprime or not, conversion included. Euclid's algorithm on
    /// <see cref="EInteger"/> is not an alternative: its divisions make it 500 ms on a coprime
    /// pair of 10,000 bits, where PeterO takes 3 ms and the BCL 0.1 ms.
    /// </para>
    /// </remarks>
    internal static class GreatestCommonDivisors
    {
        /// <summary>
        /// The greatest common divisor of <paramref name="a"/> and <paramref name="b"/>, never
        /// negative, and 0 only when both are: what <see cref="EInteger.Gcd(EInteger)"/> answers.
        /// </summary>
        internal static EInteger GreatestCommonDivisor(this EInteger a, EInteger b)
        {
            if (a.CanFitInInt64() && b.CanFitInInt64()
                && Euclid(Magnitude(a.ToInt64Unchecked()), Magnitude(b.ToInt64Unchecked())) is var divisor
                && divisor <= long.MaxValue)
                return EInteger.FromInt64((long)divisor);
            return InTheBcl(a, b);
        }

        /// <summary>
        /// <paramref name="value"/> in lowest terms, the same <see cref="ERational"/> as
        /// <see cref="ERational.ToLowestTerms"/> gives: a zero is 0/1 with its sign kept, an
        /// infinity or a NaN is itself, and anything else is its numerator and denominator
        /// divided by their greatest common divisor.
        /// </summary>
        internal static ERational InLowestTerms(this ERational value)
        {
            if (!value.IsFinite || value.IsZero)
                return value.ToLowestTerms();
            var magnitude = value.UnsignedNumerator;
            var denominator = value.Denominator;
            if (magnitude.CanFitInInt64() && denominator.CanFitInInt64())
            {
                var top = magnitude.ToInt64Unchecked();
                var bottom = denominator.ToInt64Unchecked();
                var divisor = (long)Euclid((ulong)top, (ulong)bottom);
                if (divisor == 1)
                    return value;
                top /= divisor;
                return ERational.Create(EInteger.FromInt64(value.IsNegative ? -top : top), EInteger.FromInt64(bottom / divisor));
            }
            var common = InTheBcl(magnitude, denominator);
            if (common.Equals(EInteger.One))
                return value;
            return ERational.Create(value.Numerator.Divide(common), denominator.Divide(common));
        }

        /// <summary>The magnitude of a machine integer, <see cref="long.MinValue"/> included.</summary>
        private static ulong Magnitude(long value) => value < 0 ? unchecked((ulong)-value) : (ulong)value;

        private static ulong Euclid(ulong a, ulong b)
        {
            while (b != 0)
                (a, b) = (b, a % b);
            return a;
        }

        private static EInteger InTheBcl(EInteger a, EInteger b)
            => EInteger.FromBytes(BigInteger.GreatestCommonDivisor(
                new BigInteger(a.ToBytes(littleEndian: true)), new BigInteger(b.ToBytes(littleEndian: true))).ToByteArray(), littleEndian: true);
    }
}
