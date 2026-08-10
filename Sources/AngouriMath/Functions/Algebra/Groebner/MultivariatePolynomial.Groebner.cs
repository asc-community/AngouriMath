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
    /// <summary>Which monomial is the leading one.</summary>
    internal enum MonomialOrder
    {
        /// <summary>
        /// Compare the packed exponents as integers. Free here, because the packing puts the
        /// first variable in the most significant byte, and a lexicographic basis is
        /// triangular — but it is also what makes coefficients explode, so it is the order
        /// to answer in rather than the order to compute in.
        /// </summary>
        Lexicographic,

        /// <summary>
        /// Total degree first, then the last variable the two differ in, where the smaller
        /// exponent wins. Has to be computed rather than compared, and is worth it: on dense
        /// input it finishes systems lexicographic cannot, with coefficients smaller by two
        /// orders of magnitude.
        /// </summary>
        DegreeReverseLexicographic,
    }

    internal sealed partial class MultivariatePolynomial
    {
        internal ERational CoefficientOf(ulong monomial)
            => terms.TryGetValue(monomial, out var value) ? value : ERational.Zero;

        internal IEnumerable<ulong> Monomials => terms.Keys;

        internal static MultivariatePolynomial Term(int variableCount, ulong monomial, ERational coefficient)
        {
            var built = new Dictionary<ulong, ERational>();
            if (!coefficient.IsZero)
                built[monomial] = coefficient.ToLowestTerms();
            return new(variableCount, built);
        }

        /// <summary>The greatest monomial under <paramref name="order"/>; zero if there is none.</summary>
        internal ulong LeadingMonomial(MonomialOrder order)
        {
            var found = false;
            ulong best = 0;
            foreach (var monomial in terms.Keys)
                if (!found || Greater(order, monomial, best))
                {
                    best = monomial;
                    found = true;
                }
            return best;
        }

        internal ERational LeadingCoefficient(MonomialOrder order) => terms[LeadingMonomial(order)];

        internal static bool Greater(MonomialOrder order, ulong left, ulong right)
        {
            if (order is MonomialOrder.Lexicographic)
                return left > right;
            int leftDegree = TotalDegree(left), rightDegree = TotalDegree(right);
            if (leftDegree != rightDegree)
                return leftDegree > rightDegree;
            for (var variable = MaxVariables - 1; variable >= 0; variable--)
            {
                int here = PowerOf(left, variable), there = PowerOf(right, variable);
                if (here != there)
                    return here < there;
            }
            return false;
        }

        internal static int PowerOfMonomial(ulong monomial, int variable) => PowerOf(monomial, variable);

        internal static ulong PackMonomial(int variable, int power) => Pack(variable, power);

        internal static int TotalDegree(ulong monomial)
        {
            var degree = 0;
            for (var variable = 0; variable < MaxVariables; variable++)
                degree += PowerOf(monomial, variable);
            return degree;
        }

        internal static bool MonomialDivides(ulong divisor, ulong dividend)
        {
            for (var variable = 0; variable < MaxVariables; variable++)
                if (PowerOf(divisor, variable) > PowerOf(dividend, variable))
                    return false;
            return true;
        }

        /// <summary>Only valid where <see cref="MonomialDivides"/> holds.</summary>
        internal static ulong MonomialQuotient(ulong dividend, ulong divisor)
        {
            ulong quotient = 0;
            for (var variable = 0; variable < MaxVariables; variable++)
                quotient |= Pack(variable, PowerOf(dividend, variable) - PowerOf(divisor, variable));
            return quotient;
        }

        internal static ulong MonomialLcm(ulong left, ulong right)
        {
            ulong lcm = 0;
            for (var variable = 0; variable < MaxVariables; variable++)
                lcm |= Pack(variable, Math.Max(PowerOf(left, variable), PowerOf(right, variable)));
            return lcm;
        }

        /// <summary>
        /// Reaches the private multiplication by a single term, which already refuses rather
        /// than wraps when an exponent would outgrow its byte.
        /// </summary>
        internal MultivariatePolynomial? TimesTerm(ulong monomial, ERational coefficient)
            => MultiplyByTerm(monomial, coefficient);

        internal static bool TryTimesMonomials(ulong left, ulong right, int variableCount, out ulong product)
            => TryMultiplyMonomials(left, right, variableCount, out product);

        internal MultivariatePolynomial MakeMonic(MonomialOrder order)
            => IsZero ? this : ScaleBy(ERational.One.Divide(LeadingCoefficient(order)));

        /// <summary>Decimal digits in the widest numerator or denominator carried here.</summary>
        internal int MaxCoefficientDigits()
        {
            var widest = 0;
            foreach (var coefficient in terms.Values)
            {
                var numerator = coefficient.Numerator.Abs().ToString().Length;
                if (numerator > widest) widest = numerator;
                var denominator = coefficient.Denominator.Abs().ToString().Length;
                if (denominator > widest) widest = denominator;
            }
            return widest;
        }
    }
}
