//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

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
            Mulf(Powf(var any1, var any2), Powf(var any3, var any2a)) when any2 == any2a => new Powf(any1 * any3, any2),

            Sumf or Minusf when CollectCommonFactors(x) is { } collected => collected,

            _ => x
        };

        /// <summary>
        /// Pulls a shared factor out of the terms of a sum: <c>a*c + a*d + b*c + b*d</c>
        /// becomes <c>a*(c + d) + b*(c + d)</c>, which the pairwise rules above close to
        /// <c>(a + b) * (c + d)</c> on the next pass.
        /// </summary>
        /// <remarks>
        /// The pairwise rules only ever see two adjacent terms of the sum tree, so a sum of
        /// four was left half-factored at <c>a*(c + d) + b*c + b*d</c> -- that is #531. Sums
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
