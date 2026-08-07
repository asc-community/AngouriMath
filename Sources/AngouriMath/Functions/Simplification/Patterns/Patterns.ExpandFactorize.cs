//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using PeterO.Numbers;
using static AngouriMath.Entity;

namespace AngouriMath.Functions
{
    internal static partial class Patterns
    {
        internal static Entity ExpandRules(Entity x) => x switch
        {
            Sinf(Sumf(var any1, var any2)) => new Sinf(any1) * new Cosf(any2) + new Sinf(any2) * new Cosf(any1),
            Sinf(Minusf(var any1, var any2)) => new Sinf(any1) * new Cosf(any2) - new Sinf(any2) * new Cosf(any1),

            _ => x
        };

        internal static Entity FactorizeRules(Entity x) => x switch
        {
            // {1}^2n - {2}^2m = ({1}^n - {2}^m) * ({1}^n + {2}^m).
            // Both exponents have to be even, or halving them introduces radicals and the
            // rule fires again on the factors it has just produced: x^2 - y^2 came back as
            // (sqrt(x) - sqrt(y)) * (sqrt(x) + sqrt(y)) * (x^1 + y^1). The halves are built
            // as integers rather than as const/2, which left an unevaluated 2/2 behind.
            Minusf(Powf(var any1, Integer const1), Powf(var any2, Integer const2))
                when const1.EInteger.IsEven && const2.EInteger.IsEven =>
                (new Powf(any1, Integer.Create(const1.EInteger / 2)) - new Powf(any2, Integer.Create(const2.EInteger / 2))) *
                (new Powf(any1, Integer.Create(const1.EInteger / 2)) + new Powf(any2, Integer.Create(const2.EInteger / 2))),

            Minusf(Powf(var any1, Integer(2)), Number const1) =>
                (any1 - new Powf(const1, Rational.Create(1, 2))) *
                (any1 + new Powf(const1, Rational.Create(1, 2))),

            // {1} * {2} + {1} * {3} = {1} * ({2} + {3})
            Sumf(Mulf(var any1, var any2), Mulf(var any1a, var any3)) when any1 == any1a => any1 * (any2 + any3),
            Sumf(Mulf(var any1, var any2), Mulf(var any3, var any1a)) when any1 == any1a => any1 * (any2 + any3),
            Sumf(Mulf(var any2, var any1), Mulf(var any1a, var any3)) when any1 == any1a => any1 * (any2 + any3),
            Sumf(Mulf(var any2, var any1), Mulf(var any3, var any1a)) when any1 == any1a => any1 * (any2 + any3),
            Sumf(var any1, Mulf(var any1a, var any2)) when any1 == any1a => any1 * (1 + any2),
            Sumf(var any1, Mulf(var any2, var any1a)) when any1 == any1a => any1 * (1 + any2),
            Sumf(Mulf(var any1, var any2), var any1a) when any1 == any1a => any1 * (1 + any2),
            Sumf(Mulf(var any2, var any1), var any1a) when any1 == any1a => any1 * (1 + any2),
            Sumf(var any1, var any1a) when any1 == any1a => 2 * any1,

            Minusf(Mulf(var any1, var any2), Mulf(var any1a, var any3)) when any1 == any1a => any1 * (any2 - any3),
            Minusf(Mulf(var any2, var any1), Mulf(var any1a, var any3)) when any1 == any1a => any1 * (any2 - any3),
            Minusf(Mulf(var any1, var any2), Mulf(var any3, var any1a)) when any1 == any1a => any1 * (any2 - any3),
            Minusf(Mulf(var any2, var any1), Mulf(var any3, var any1a)) when any1 == any1a => any1 * (any2 - any3),
            Minusf(var any1, Mulf(var any1a, var any2)) when any1 == any1a => any1 * (1 - any2),
            Minusf(var any1, Mulf(var any2, var any1a)) when any1 == any1a => any1 * (1 - any2),
            Minusf(Mulf(var any1, var any2), var any1a) when any1 == any1a => any1 * (any2 - 1),
            Minusf(Mulf(var any2, var any1), var any1a) when any1 == any1a => any1 * (any2 - 1),
            Minusf(var any1, var any1a) when any1 == any1a => 0,

            // a ^ b * c ^ b = (a * c) ^ b
            //
            // Guarded like its twin in PowerRules, which is where this identity is written
            // out and explained: true for a whole b whatever the signs, and for positive
            // real bases whatever the exponent, and false outside those two --
            // sqrt(x) * sqrt(y) became sqrt(x * y), which at x = y = -1 is 1 where the
            // product is -1. https://github.com/asc-community/AngouriMath/issues/801
            Mulf(Powf(var any1, var any2), Powf(var any3, var any2a))
                when any2 == any2a
                     && (any2 is Integer
                         || (any1.Evaled is Real { IsPositive: true }
                             && any3.Evaled is Real { IsPositive: true }))
                => new Powf(any1 * any3, any2),

            Sumf or Minusf when CollectCommonFactors(x) is { } collected => collected,

            _ => x
        };


        /// <summary>
        /// A three-term sum that is a perfect square with a radical in it.
        /// </summary>
        /// <remarks>
        /// Its own pass, run before <see cref="FactorizeRules"/> rather than as an arm of
        /// it. <c>Replace</c> walks bottom-up, so the common-factor rule reaches the inner
        /// <c>4 + 4*sqrt(x)</c> of <c>4 + 4*sqrt(x) + x</c> first and rewrites it to
        /// <c>4 * (1 + sqrt(x))</c> -- by the time the outer sum is looked at, the three
        /// terms this needs are no longer there. Ordering the arms within the switch does
        /// not help, because the two rules are looking at different nodes.
        /// https://github.com/asc-community/AngouriMath/issues/176
        /// </remarks>
        internal static Entity PerfectSquareRules(Entity x)
            => x is Sumf or Minusf && CollapseToPerfectSquare(x) is { } square ? square : x;

        /// <summary>
        /// <c>u + 2*sqrt(u)*sqrt(v) + v</c> collapses to <c>(sqrt(u) + sqrt(v))^2</c>, which
        /// is what <c>1 + sqrt(2x) + x/2</c> is.
        /// https://github.com/asc-community/AngouriMath/issues/176
        /// </summary>
        /// <remarks>
        /// <para>
        /// The identity itself is unconditional: <c>(sqrt(u) + sqrt(v))^2</c> expands to
        /// <c>u + 2*sqrt(u)*sqrt(v) + v</c> because <c>sqrt(u)^2</c> is <c>u</c> for every
        /// complex <c>u</c>. It is the *square of a principal root*, not the root of a
        /// square -- <c>sqrt(u^2) = u</c> is the false one, and holds only for a
        /// non-negative <c>u</c>. https://github.com/asc-community/AngouriMath/issues/752
        /// </para>
        /// <para>
        /// What cannot be trusted is the test for whether the cross term matches. Deciding
        /// it needs <see cref="Entity.Simplify"/>, and the simplifier equates
        /// <c>sqrt(x)*sqrt(y)</c> with <c>sqrt(x*y)</c>, which is false on the branch cuts:
        /// at <c>x = y = -1</c>, <c>x + 2*sqrt(x*y) + y</c> is 0 while
        /// <c>(sqrt(x) + sqrt(y))^2</c> is -4. Asked symbolically, this rule fired on that
        /// sum and produced a wrong answer.
        /// </para>
        /// <para>
        /// So the symbolic match only proposes, and a numeric check at sample points
        /// disposes. The points include negative values, which is where a branch-cut error
        /// shows and nowhere else, and every free variable is given a different one so that
        /// <c>x</c> and <c>y</c> cannot coincide into a case that happens to hold. The rule
        /// withdraws unless every sampled point agrees, so a variable it cannot evaluate at
        /// simply means no collapse.
        /// </para>
        /// <para>
        /// Restricted to sums that contain a radical, both because that is where the gap is
        /// -- a polynomial trinomial is collapsed by the rules above -- and to keep the cost
        /// of that <c>Simplify</c> off every three-term sum in every tree.
        /// </para>
        /// </remarks>
        /// <returns><see langword="null"/> when the sum is not a square, so the rule does not fire.</returns>
        private static Entity? CollapseToPerfectSquare(Entity expr)
        {
            if (!expr.Nodes.Any(node => node is Powf(_, Rational and not Integer)))
                return null;
            var terms = Sumf.LinearChildren(expr).ToList();
            if (terms.Count != 3)
                return null;

            for (var cross = 0; cross < 3; cross++)
            {
                var w = terms[cross];
                var p = new Powf(terms[(cross + 1) % 3], Rational.Create(1, 2));
                var q = new Powf(terms[(cross + 2) % 3], Rational.Create(1, 2));
                var product = (2 * p * q).Simplify();
                var candidate =
                    (product - w).Simplify() == 0 ? new Powf((p + q).Simplify(), 2) :
                    (product + w).Simplify() == 0 ? new Powf((p - q).Simplify(), 2) :
                    null;
                if (candidate is { } square && AgreesNumerically(expr, square))
                    return square;
            }
            return null;
        }

        /// <summary>Sample points, negative first: a branch-cut error shows nowhere else.</summary>
        [ConstantField] private static readonly EDecimal[] PerfectSquareSamplePoints =
            { EDecimal.FromString("-1.7"), EDecimal.FromString("-0.6"),
              EDecimal.FromString("0.8"), EDecimal.FromString("2.3") };

        /// <summary>
        /// Whether a proposed collapse is the same number as what it replaces, checked at
        /// <see cref="PerfectSquareSamplePoints"/> with each free variable offset from the
        /// last so that two of them never take the same value.
        /// </summary>
        private static bool AgreesNumerically(Entity original, Entity candidate)
        {
            var variables = original.Vars.ToList();
            if (variables.Count == 0)
                return true;
            for (var i = 0; i < PerfectSquareSamplePoints.Length; i++)
            {
                Entity before = original, after = candidate;
                for (var v = 0; v < variables.Count; v++)
                {
                    var point = PerfectSquareSamplePoints[(i + v) % PerfectSquareSamplePoints.Length];
                    before = before.Substitute(variables[v], Real.Create(point));
                    after = after.Substitute(variables[v], Real.Create(point));
                }
                if (before.Evaled is not Complex left || after.Evaled is not Complex right)
                    return false;
                var difference = (left - right).Abs();
                if (difference is not Real real || real.EDecimal.Abs()
                        .CompareTo(EDecimal.FromString("1e-12")) > 0)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Pulls a shared factor out of the terms of a sum: <c>a*c + a*d + b*c + b*d</c>
        /// becomes <c>a*(c + d) + b*(c + d)</c>, which the pairwise rules above close to
        /// <c>(a + b) * (c + d)</c> on the next pass.
        /// </summary>
        /// <remarks>
        /// The pairwise rules only ever see two adjacent terms of the sum tree, so a sum of
        /// four was left half-factored at <c>a*(c + d) + b*c + b*d</c> -- that is
        /// https://github.com/asc-community/AngouriMath/issues/531. Sums
        /// of two are left to those rules, which are older and better tested.
        /// </remarks>
        /// <returns><see langword="null"/> when no factor is shared, so the rule does not fire.</returns>
        private static Entity? CollectCommonFactors(Entity expr)
        {
            var terms = Sumf.LinearChildren(expr).ToList();
            return terms.Count > 2 ? CollectOver(terms) : null;
        }

        private static Entity? CollectOver(IReadOnlyList<Entity> terms)
        {
            var factorsOf = terms.Select(term => Mulf.LinearChildren(term).ToList()).ToList();

            // The factor shared by the most terms, so the largest group comes out first.
            // Numbers are left alone: pulling a coefficient out only moves it about, and
            // the rules that gather coefficients are elsewhere.
            Entity? shared = null;
            var sharedBy = 1;
            foreach (var candidate in factorsOf.SelectMany(factors => factors).Distinct())
                if (candidate is not Number
                    && factorsOf.Count(factors => factors.Contains(candidate)) is var count
                    && count > sharedBy)
                    (shared, sharedBy) = (candidate, count);
            if (shared is null)
                return null;

            var grouped = new List<Entity>();
            var rest = new List<Entity>();
            for (var i = 0; i < terms.Count; i++)
            {
                var factors = factorsOf[i];
                var at = factors.IndexOf(shared);
                if (at < 0)
                {
                    rest.Add(terms[i]);
                    continue;
                }
                factors.RemoveAt(at); // one occurrence only, so a^2 keeps an a behind
                grouped.Add(factors.Count == 0 ? 1 : factors.Aggregate((left, right) => left * right));
            }

            var collected = shared * Sumf.Sum(grouped);
            // Every recursion drops at least the two terms just grouped, so this ends.
            return rest.Count == 0 ? collected : collected + (CollectOver(rest) ?? Sumf.Sum(rest));
        }
    }
}
