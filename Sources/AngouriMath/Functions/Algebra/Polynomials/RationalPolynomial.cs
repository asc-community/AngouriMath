//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using PeterO.Numbers;
using System;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;

namespace AngouriMath.Functions
{
    /// <summary>
    /// A polynomial in one variable over the rationals, stored densely.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A third polynomial representation, next to <see cref="IntegerPolynomial"/> and
    /// <see cref="MultivariatePolynomial"/>, because division is the operation this one exists
    /// for. Factoring works over <c>Z</c>, where content, primitive parts and Mignotte's bound
    /// all mean something; a partial fraction decomposition works over <c>Q</c>, where every
    /// polynomial can be divided by any other and the extended Euclidean algorithm terminates
    /// with a genuine unit. Doing that over <c>Z</c> means carrying a scaling factor beside
    /// every intermediate, which is where the sign and content errors live.
    /// </para>
    /// <para>
    /// Trailing zero coefficients are never stored, so <see cref="Degree"/> is
    /// <c>coefficients.Length - 1</c> and the zero polynomial is the empty array with degree
    /// <c>-1</c>. Coefficients are lowest power first, the order the rest of
    /// <c>Functions/Algebra/Polynomials</c> uses.
    /// </para>
    /// <para>
    /// Every coefficient is reduced to lowest terms as it is written. <see cref="ERational"/>
    /// does not do that on its own, and a remainder sequence that leaves it undone multiplies
    /// numerator and denominator up at every step: the ratios stay correct and become
    /// arbitrarily expensive to compare, which is the failure that looks like a hang.
    /// </para>
    /// </remarks>
    internal sealed class RationalPolynomial
    {
        [ConstantField] private static readonly ERational[] NoCoefficients = Array.Empty<ERational>();

        /// <summary>Lowest power first, in lowest terms, with no trailing zero.</summary>
        private readonly ERational[] coefficients;

        private RationalPolynomial(ERational[] coefficients) => this.coefficients = coefficients;

        internal static RationalPolynomial Create(IReadOnlyList<ERational> coefficientsLowestFirst)
        {
            var length = coefficientsLowestFirst.Count;
            while (length > 0 && coefficientsLowestFirst[length - 1].IsZero)
                length--;
            if (length == 0)
                return Zero;
            var trimmed = new ERational[length];
            for (var i = 0; i < length; i++)
                trimmed[i] = coefficientsLowestFirst[i].ToLowestTerms();
            return new(trimmed);
        }

        [ConstantField] internal static readonly RationalPolynomial Zero = new(NoCoefficients);

        internal static RationalPolynomial Constant(ERational value)
            => value.IsZero ? Zero : new(new[] { value.ToLowestTerms() });

        internal static RationalPolynomial One => Constant(ERational.One);

        /// <summary>The same polynomial read over <c>Q</c>, every denominator being one.</summary>
        internal static RationalPolynomial FromInteger(IntegerPolynomial poly)
        {
            var converted = new ERational[poly.Degree + 1];
            for (var i = 0; i < converted.Length; i++)
                converted[i] = ERational.Create(poly[i], EInteger.One);
            return Create(converted);
        }

        /// <summary>-1 for the zero polynomial.</summary>
        internal int Degree => coefficients.Length - 1;

        internal bool IsZero => coefficients.Length == 0;

        internal bool IsConstant => coefficients.Length <= 1;

        internal ERational this[int power]
            => power >= 0 && power < coefficients.Length ? coefficients[power] : ERational.Zero;

        internal ERational Leading => IsZero ? ERational.Zero : coefficients[coefficients.Length - 1];

        internal RationalPolynomial ScaleBy(ERational factor)
        {
            if (factor.IsZero)
                return Zero;
            var scaled = new ERational[coefficients.Length];
            for (var i = 0; i < scaled.Length; i++)
                scaled[i] = coefficients[i].Multiply(factor);
            return Create(scaled);
        }

        internal RationalPolynomial Add(RationalPolynomial other) => Combine(other, subtract: false);

        internal RationalPolynomial Subtract(RationalPolynomial other) => Combine(other, subtract: true);

        private RationalPolynomial Combine(RationalPolynomial other, bool subtract)
        {
            var length = Math.Max(coefficients.Length, other.coefficients.Length);
            var combined = new ERational[length];
            for (var i = 0; i < length; i++)
            {
                var right = other[i];
                combined[i] = subtract ? this[i].Subtract(right) : this[i].Add(right);
            }
            return Create(combined);
        }

        internal RationalPolynomial Multiply(RationalPolynomial other)
        {
            if (IsZero || other.IsZero)
                return Zero;
            var product = new ERational[coefficients.Length + other.coefficients.Length - 1];
            for (var i = 0; i < product.Length; i++)
                product[i] = ERational.Zero;
            for (var i = 0; i < coefficients.Length; i++)
            {
                if (coefficients[i].IsZero)
                    continue;
                for (var j = 0; j < other.coefficients.Length; j++)
                {
                    if (other.coefficients[j].IsZero)
                        continue;
                    product[i + j] = product[i + j].Add(coefficients[i].Multiply(other.coefficients[j])).ToLowestTerms();
                }
            }
            return Create(product);
        }

        internal RationalPolynomial Pow(int power)
        {
            var result = One;
            for (var i = 0; i < power; i++)
                result = result.Multiply(this);
            return result;
        }

        /// <summary>
        /// <c>this = quotient * divisor + remainder</c>, the remainder being of lower degree
        /// than the divisor. <see langword="false"/> only where the divisor is zero.
        /// </summary>
        internal bool TryDivide(
            RationalPolynomial divisor, out RationalPolynomial quotient, out RationalPolynomial remainder)
        {
            quotient = remainder = Zero;
            if (divisor.IsZero)
                return false;
            if (Degree < divisor.Degree)
            {
                remainder = this;
                return true;
            }

            var working = new ERational[coefficients.Length];
            for (var i = 0; i < working.Length; i++)
                working[i] = coefficients[i];
            var quotientCoefficients = new ERational[Degree - divisor.Degree + 1];
            for (var i = 0; i < quotientCoefficients.Length; i++)
                quotientCoefficients[i] = ERational.Zero;

            for (var power = Degree; power >= divisor.Degree; power--)
            {
                if (working[power].IsZero)
                    continue;
                var factor = working[power].Divide(divisor.Leading).ToLowestTerms();
                quotientCoefficients[power - divisor.Degree] = factor;
                for (var i = 0; i <= divisor.Degree; i++)
                    working[power - divisor.Degree + i] =
                        working[power - divisor.Degree + i].Subtract(factor.Multiply(divisor[i])).ToLowestTerms();
            }

            quotient = Create(quotientCoefficients);
            remainder = Create(working);
            return true;
        }

        /// <summary>
        /// <paramref name="left"/> and <paramref name="right"/> being coprime, the pair with
        /// <c>u*left + v*right = 1</c>. <see langword="false"/> where they share a factor of
        /// positive degree, or where either is zero.
        /// </summary>
        /// <remarks>
        /// The extended Euclidean algorithm, which over a field ends at a constant remainder
        /// exactly when the two are coprime — so the test for coprimality and the cofactors
        /// come out of the same run and neither is asserted separately.
        /// </remarks>
        internal static bool TryBezout(
            RationalPolynomial left, RationalPolynomial right,
            out RationalPolynomial u, out RationalPolynomial v)
        {
            u = v = Zero;
            if (left.IsZero || right.IsZero)
                return false;

            var (remainderPrevious, remainderCurrent) = (left, right);
            var (leftPrevious, leftCurrent) = (One, Zero);
            var (rightPrevious, rightCurrent) = (Zero, One);

            while (!remainderCurrent.IsZero)
            {
                if (!remainderPrevious.TryDivide(remainderCurrent, out var quotient, out var next))
                    return false;
                (remainderPrevious, remainderCurrent) = (remainderCurrent, next);
                (leftPrevious, leftCurrent) =
                    (leftCurrent, leftPrevious.Subtract(quotient.Multiply(leftCurrent)));
                (rightPrevious, rightCurrent) =
                    (rightCurrent, rightPrevious.Subtract(quotient.Multiply(rightCurrent)));
            }

            // The last non-zero remainder is the greatest common divisor. A constant one is a
            // unit over Q and divides out; anything of positive degree means the two are not
            // coprime, and there is no such pair to return.
            if (!remainderPrevious.IsConstant || remainderPrevious.IsZero)
                return false;
            var inverse = ERational.One.Divide(remainderPrevious[0]);
            u = leftPrevious.ScaleBy(inverse);
            v = rightPrevious.ScaleBy(inverse);
            return true;
        }

        internal bool SameAs(RationalPolynomial other)
        {
            if (coefficients.Length != other.coefficients.Length)
                return false;
            for (var i = 0; i < coefficients.Length; i++)
                if (coefficients[i].CompareTo(other.coefficients[i]) != 0)
                    return false;
            return true;
        }

        internal Entity ToEntity(Variable x)
        {
            Entity result = Integer.Create(0);
            for (var i = Degree; i >= 0; i--)
            {
                if (coefficients[i].IsZero)
                    continue;
                Entity term = Rational.Create(coefficients[i]);
                if (i == 1)
                    term = term == Integer.One ? x : term * x;
                else if (i > 1)
                    term = term == Integer.One ? x.Pow(i) : term * x.Pow(i);
                result = result == Integer.Create(0) ? term : result + term;
            }
            return result.InnerSimplified;
        }

        public override string ToString()
        {
            if (IsZero)
                return "0";
            var parts = new List<string>();
            for (var i = Degree; i >= 0; i--)
            {
                if (coefficients[i].IsZero)
                    continue;
                parts.Add(i switch
                {
                    0 => coefficients[i].ToString(),
                    1 => coefficients[i] + "x",
                    _ => coefficients[i] + "x^" + i
                });
            }
            return string.Join(" + ", parts);
        }
    }
}
