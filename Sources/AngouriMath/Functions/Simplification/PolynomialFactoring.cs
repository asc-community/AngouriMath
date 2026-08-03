//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Functions.Algebra.AnalyticalSolving;
using PeterO.Numbers;
using System.Diagnostics.CodeAnalysis;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;

namespace AngouriMath.Functions
{
    /// <summary>
    /// Factors a polynomial in one variable into linear factors with whole roots:
    /// <c>x^2 + 2x + 1</c> becomes <c>(x + 1)^2</c>, and
    /// <c>x^3 - 6x^2 + 11x - 6</c> becomes <c>(x - 1)(x - 2)(x - 3)</c>.
    /// </summary>
    /// <remarks>
    /// Deliberately narrow, on two counts.
    /// <para>
    /// Rational roots only. Factoring through every root would answer
    /// <c>(x - i)(x + i)</c> for <c>x^2 + 1</c> and <c>(x - sqrt(2))(x + sqrt(2))</c> for
    /// <c>x^2 - 2</c>, which is not what anyone means by factoring those.
    /// </para>
    /// <para>
    /// And only when the polynomial splits completely into whole roots. A partly factored
    /// answer is not obviously better than the sum it came from, and fractional roots turn
    /// up mostly in the output of calculus, where the expanded form is the conventional
    /// one: the antiderivative of <c>x^2 + x</c> reads better as <c>x^3/3 + x^2/2</c> than
    /// as <c>x^2 * (x + 3/2) / 3</c>.
    /// </para>
    /// <para>
    /// What comes out of here is a candidate, not a decision. The simplifier keeps it
    /// alongside the other forms it has found and picks between them by its complexity
    /// metric, which is why <c>x^2 - 1</c> stays as it is while <c>(x + 1)^2</c> wins.
    /// </para>
    /// </remarks>
    internal static class PolynomialFactoring
    {
        /// <summary>Above this the search costs more than the answer is worth.</summary>
        private const int MaxDegree = 16;

        /// <summary>
        /// Rational roots are found by trying divisors of the first and last coefficients,
        /// so those have to be small enough to factor cheaply. A polynomial with a
        /// coefficient past this is left alone rather than paid for.
        /// </summary>
        [ConstantField] private static readonly EInteger MaxCoefficientToFactor = EInteger.FromInt32(1000000);

        internal static bool TryFactor(Entity expr, Variable x, [NotNullWhen(true)] out Entity? factored)
        {
            factored = null;
            if (!TryGetRationalCoefficients(expr, x, out var coefficients))
                return false;

            var degree = coefficients.Length - 1;
            var roots = new List<ERational>();
            // A zero constant term means x itself divides the polynomial. Dividing by
            // (x - 0) is just dropping that coefficient, so it is done first and cheaply.
            while (coefficients.Length > 1 && coefficients[0].IsZero)
            {
                roots.Add(ERational.Zero);
                coefficients = coefficients[1..];
            }

            foreach (var candidate in RationalRootCandidates(coefficients))
                while (coefficients.Length > 1 && Evaluate(coefficients, candidate).IsZero)
                {
                    roots.Add(candidate);
                    coefficients = DivideByLinear(coefficients, candidate);
                }

            // Offered only when the polynomial splits completely into linear factors with
            // whole roots. Partly factored answers are not obviously better than the sum
            // they came from, and fractional roots come up mostly in the output of
            // calculus, where the expanded form is the conventional one: the
            // antiderivative of x^2 + x reads better as x^3/3 + x^2/2 than as
            // x^2 * (x + 3/2) / 3, which is what factoring through its root at -3/2 gives.
            if (roots.Count != degree || roots.Any(root => !root.IsFinite || !root.Denominator.Equals(EInteger.One)))
                return false;

            var factors = new List<Entity>();
            var remainder = BuildPolynomial(coefficients, x);
            if (remainder != Integer.One)
                factors.Add(remainder);
            foreach (var group in roots.GroupBy(root => root))
            {
                // x - r, written as x + |r| when r is negative so that the printed form is
                // the usual one rather than `x - (-1)`.
                var root = group.Key;
                Entity linear = root.IsZero ? x
                    : root.Sign < 0 ? x + Rational.Create(root.Negate())
                    : x - Rational.Create(root);
                factors.Add(group.Count() == 1 ? linear : linear.Pow(group.Count()));
            }

            factored = factors.Aggregate((left, right) => left * right);
            return degree > 1;
        }

        /// <summary>
        /// The coefficients of <paramref name="expr"/> in <paramref name="x"/>, lowest
        /// power first, or <see langword="false"/> if it is not a polynomial in
        /// <paramref name="x"/> with rational coefficients alone.
        /// </summary>
        private static bool TryGetRationalCoefficients(Entity expr, Variable x, out ERational[] coefficients)
        {
            coefficients = System.Array.Empty<ERational>();
            var monomials = PolynomialSolver.GatherMonomialInformation<EInteger, TreeAnalyzer.PrimitiveInteger>(
                Sumf.LinearChildren(expr.Expand()), x);
            if (monomials is null || monomials.Count < 2)
                return false;

            var degree = monomials.Keys.Max();
            if (degree is null || degree > MaxDegree || degree < 2)
                return false;

            var found = new ERational[degree.ToInt32Checked() + 1];
            for (var i = 0; i < found.Length; i++)
                found[i] = ERational.Zero;
            foreach (var (power, coefficient) in monomials)
            {
                if (power is null || power.Sign < 0 || coefficient.Evaled is not Rational ratio)
                    return false;
                found[power.ToInt32Checked()] = ratio.ERational;
            }

            coefficients = found;
            return true;
        }

        /// <summary>
        /// Every p/q that could be a root, by the rational root theorem: p divides the
        /// constant term and q the leading one.
        /// </summary>
        private static IEnumerable<ERational> RationalRootCandidates(ERational[] coefficients)
        {
            // Cleared of denominators first, so that the theorem applies.
            var scale = coefficients.Aggregate(EInteger.One, (acc, c) => Lcm(acc, c.Denominator));
            var whole = coefficients.Select(c => c.Numerator * scale.Divide(c.Denominator)).ToArray();

            var constant = whole[0].Abs();
            var leading = whole[^1].Abs();
            if (constant.IsZero || constant > MaxCoefficientToFactor || leading > MaxCoefficientToFactor)
                return Enumerable.Empty<ERational>();

            var candidates = new List<ERational>();
            foreach (var p in Divisors(constant))
                foreach (var q in Divisors(leading))
                {
                    candidates.Add(ERational.Create(p, q));
                    candidates.Add(ERational.Create(p.Negate(), q));
                }
            return candidates.Distinct();
        }

        private static EInteger Lcm(EInteger a, EInteger b) => a.Divide(a.Gcd(b)).Multiply(b);

        private static IEnumerable<EInteger> Divisors(EInteger n)
        {
            var divisors = new List<EInteger> { EInteger.One };
            foreach (var (prime, power) in n.Factorize())
            {
                var extended = new List<EInteger>(divisors);
                var multiplier = EInteger.One;
                for (long i = 0; i < power; i++)
                {
                    multiplier = multiplier.Multiply(EInteger.FromInt64(prime));
                    foreach (var divisor in divisors)
                        extended.Add(divisor.Multiply(multiplier));
                }
                divisors = extended;
            }
            return divisors;
        }

        /// <summary>Horner, on the exact ratios.</summary>
        private static ERational Evaluate(ERational[] coefficients, ERational at)
        {
            var acc = ERational.Zero;
            for (var i = coefficients.Length - 1; i >= 0; i--)
                acc = acc.Multiply(at).Add(coefficients[i]);
            return acc;
        }

        /// <summary>
        /// Synthetic division by <c>x - root</c>, which is exact because the root is one.
        /// </summary>
        private static ERational[] DivideByLinear(ERational[] coefficients, ERational root)
        {
            var quotient = new ERational[coefficients.Length - 1];
            var carry = ERational.Zero;
            for (var i = coefficients.Length - 1; i >= 1; i--)
            {
                carry = coefficients[i].Add(carry.Multiply(root));
                quotient[i - 1] = carry;
            }
            return quotient;
        }

        private static Entity BuildPolynomial(ERational[] coefficients, Variable x)
        {
            Entity result = Integer.Create(0);
            for (var i = coefficients.Length - 1; i >= 0; i--)
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
    }
}
