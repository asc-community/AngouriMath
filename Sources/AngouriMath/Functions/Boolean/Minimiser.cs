//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Collections.Generic;
using System.Linq;
using AngouriMath.Core.Multithreading;

namespace AngouriMath.Functions.Boolean
{
    using static Entity;

    /// <summary>
    /// Two-level minimisation of a boolean expression by Quine-McCluskey: the minterms where
    /// it holds are combined into prime implicants, and a cover is chosen from those.
    /// </summary>
    /// <remarks>
    /// The rewrite rules reach absorption and nothing past it, so
    /// <c>a and b or a and not b</c> stopped at <c>a and (b or not b)</c> -- the factoring is
    /// right and there is no rule to finish it, because <c>b or not b</c> has none reducing it
    /// to <c>true</c>. One classical algorithm covers that, excluded middle, non-contradiction
    /// and every larger cover at once, where each would otherwise be its own rule.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/768">#768</a>
    /// </remarks>
    internal static class Minimiser
    {
        /// <summary>
        /// Above this many variables the table is not built at all. The cost is
        /// <c>2^n</c> evaluations of the expression before any minimising starts, and the
        /// expression is substituted into once per row -- so this is a bound on work that is
        /// already lost by the time it is found to be wasted.
        /// </summary>
        /// <remarks>
        /// Ten variables is 1024 rows, and the worst case is parity -- <c>a xor b xor ...</c>,
        /// where no two minterms combine. Measured on that: 13 ms at eight variables, 22 at
        /// nine, 34 at ten, roughly doubling per variable. Without the early exit below the
        /// same three were 196 ms, 803 ms and 1517 ms, so most of what the bound is protecting
        /// against is handled by not searching rather than by refusing.
        /// <para/>
        /// What remains is the table itself, which has to be built before anything can be said
        /// about it. The bound is where that stops being worth paying blind, and it exists so
        /// a wide expression declines rather than hangs -- the behaviour every other guard
        /// here has.
        /// </remarks>
        internal const int MaxVariables = 10;

        /// <summary>
        /// The expression as a minimal sum of products, or <see langword="null"/> where it is
        /// not a purely boolean expression, has too many variables, or is already what this
        /// would produce.
        /// </summary>
        /// <remarks>
        /// **Offered as one more candidate rather than taken.** `Simplify` ranks what it is
        /// given by node count and returns the shortest, so handing it this can only change an
        /// answer where it is shorter than everything else on offer -- which is what makes it
        /// safe to add to a pipeline that already answers most expressions well. It is also
        /// what fixes
        /// <a href="https://github.com/asc-community/AngouriMath/issues/769">#769</a>: there
        /// the winning candidate was an `implies` form at 12 nodes, beating the 16-node input,
        /// and the minimal `not (a or b)` at 4 was never generated for it to lose to.
        /// </remarks>
        internal static Entity? Minimise(Entity expr)
        {
            // Declaring the variable boolean is how a caller says what this is about, so
            // `a and b or a and not b provided a in BB` had better reach the same answer the
            // bare expression does. The condition says nothing about which minterms hold, so
            // it travels with the result rather than blocking it.
            if (expr is Providedf(var body, var condition))
                return Minimise(body) is { } minimisedBody ? minimisedBody.Provided(condition) : null;
            if (!IsPurelyBoolean(expr))
                return null;
            var variables = expr.Vars.ToArray();
            if (variables.Length is 0 || variables.Length > MaxVariables)
                return null;
            // `e` and `pi` are Variable nodes that evaluate to numbers, so they pass the check
            // above and are then absent from Vars -- which is how `a and (e or not e)` reached
            // the table with a variable it could not assign. A boolean expression has no
            // business holding one, and VarsAndConsts is what tells them apart.
            if (expr.VarsAndConsts.Count() != variables.Length)
                return null;

            var rows = 1 << variables.Length;
            var minterms = new List<int>();
            var assignment = new Dictionary<Variable, Entity>(variables.Length);
            for (var row = 0; row < rows; row++)
            {
                for (var bit = 0; bit < variables.Length; bit++)
                    assignment[variables[bit]] = Bit(row, bit, variables.Length);
                bool holds;
                try { holds = expr.Substitute(assignment).EvalBoolean(); }
                catch (Core.Exceptions.AngouriBugException) { throw; }
                catch (Exception) { return null; }
                if (holds)
                    minterms.Add(row);
            }
            MultithreadingFunctional.ExitIfCancelled();

            if (minterms.Count == 0)
                return Entity.Boolean.False;
            if (minterms.Count == rows)
                return Entity.Boolean.True;
            // A sum of k products is at least 2k-1 nodes -- k terms of at least one node,
            // joined by k-1 disjunctions -- so where that already exceeds what came in, no
            // cover this could find will be chosen, and the search is work thrown away.
            // Parity is the case that matters: `a xor b xor ... ` over ten variables has 512
            // minterms and no two of them combine, so every one is a prime implicant and the
            // minimal form is 512 terms long. Finding that took 1.5 s to produce a candidate
            // that loses to the input at 19 nodes.
            if (2 * minterms.Count - 1 >= expr.Complexity)
                return null;

            var cover = Cover(PrimeImplicants(minterms, variables.Length), minterms);
            return SumOfProducts(cover, variables);
        }

        /// <summary>
        /// Whether every node is a boolean connective, a variable or a boolean constant.
        /// </summary>
        /// <remarks>
        /// Substituting <see langword="true"/> for the variable of <c>x &gt; 1</c> is not a
        /// question this can ask, so anything carrying arithmetic is declined before the table
        /// is built rather than by catching what the evaluation throws.
        /// </remarks>
        private static bool IsPurelyBoolean(Entity expr)
            => expr.Nodes.All(node =>
                node is Andf or Orf or Notf or Xorf or Impliesf or Variable or Entity.Boolean);

        /// <summary>
        /// The value of the variable at <paramref name="bit"/> in the row numbered
        /// <paramref name="row"/>, counting from the most significant so that the rows read in
        /// the order a truth table is written.
        /// </summary>
        private static Entity Bit(int row, int bit, int count)
            => (row >> (count - 1 - bit) & 1) == 1;

        /// <summary>
        /// An implicant: the bits that are fixed, and which positions those are.
        /// </summary>
        /// <param name="Bits">The fixed values, with the free positions zeroed.</param>
        /// <param name="Fixed">One bit set per position that is fixed.</param>
        private readonly record struct Implicant(int Bits, int Fixed);

        /// <summary>
        /// The prime implicants: every minterm, combined with its neighbours as far as it will
        /// go. Two implicants combine when they fix the same positions and differ in exactly
        /// one of them, and what comes out is the pair with that position freed. Whatever is
        /// never combined is prime.
        /// </summary>
        private static List<Implicant> PrimeImplicants(List<int> minterms, int count)
        {
            var all = (1 << count) - 1;
            var current = new HashSet<Implicant>(minterms.Select(m => new Implicant(m, all)));
            var primes = new List<Implicant>();
            while (current.Count > 0)
            {
                var combined = new HashSet<Implicant>();
                var used = new HashSet<Implicant>();
                foreach (var left in current)
                    foreach (var right in current)
                    {
                        if (left.Fixed != right.Fixed)
                            continue;
                        var difference = left.Bits ^ right.Bits;
                        // Exactly one fixed position differs -- and a power of two with its
                        // low bit cleared is zero, which is the cheapest way to say so.
                        if (difference == 0 || (difference & (difference - 1)) != 0
                                            || (difference & left.Fixed) == 0)
                            continue;
                        combined.Add(new Implicant(left.Bits & ~difference, left.Fixed & ~difference));
                        used.Add(left);
                        used.Add(right);
                    }
                primes.AddRange(current.Where(implicant => !used.Contains(implicant)));
                current = combined;
                MultithreadingFunctional.ExitIfCancelled();
            }
            return primes;
        }

        /// <summary>
        /// A cover of the minterms by the prime implicants: every implicant that alone covers
        /// some minterm has to be in it, and what those leave uncovered is taken greedily,
        /// widest first.
        /// </summary>
        /// <remarks>
        /// Greedy rather than Petrick's method, which is exact and exponential in the number
        /// of implicants. The essential pass alone settles every case in <c>work/boolmin</c>,
        /// and where it does not, a cover that is one term wider than the optimum still has to
        /// beat every other candidate on node count before `Simplify` will return it.
        /// </remarks>
        private static List<Implicant> Cover(List<Implicant> primes, List<int> minterms)
        {
            var covers = primes.ToDictionary(
                implicant => implicant,
                implicant => new HashSet<int>(minterms.Where(m => Covers(implicant, m))));
            var chosen = new List<Implicant>();
            var remaining = new HashSet<int>(minterms);

            foreach (var minterm in minterms)
            {
                var only = primes.Where(implicant => covers[implicant].Contains(minterm)).ToList();
                if (only.Count == 1 && !chosen.Contains(only[0]))
                {
                    chosen.Add(only[0]);
                    remaining.ExceptWith(covers[only[0]]);
                }
            }
            while (remaining.Count > 0)
            {
                var best = primes.Where(implicant => !chosen.Contains(implicant))
                    .OrderByDescending(implicant => covers[implicant].Count(remaining.Contains))
                    .ThenBy(implicant => FixedCount(implicant))
                    .First();
                chosen.Add(best);
                remaining.ExceptWith(covers[best]);
            }
            return chosen;
        }

        /// <summary>
        /// How many positions an implicant fixes -- its width, counted by hand because
        /// <c>BitOperations</c> is not in netstandard2.0, which this library still targets.
        /// </summary>
        private static int FixedCount(Implicant implicant)
        {
            var count = 0;
            for (var bits = implicant.Fixed; bits != 0; bits &= bits - 1)
                count++;
            return count;
        }

        private static bool Covers(Implicant implicant, int minterm)
            => (minterm & implicant.Fixed) == implicant.Bits;

        /// <summary>
        /// The implicants written back out as a disjunction of conjunctions.
        /// </summary>
        private static Entity SumOfProducts(List<Implicant> cover, Variable[] variables)
            => cover
                .Select(implicant => Product(implicant, variables))
                .Aggregate((left, right) => left | right);

        private static Entity Product(Implicant implicant, Variable[] variables)
            => Enumerable.Range(0, variables.Length)
                .Where(bit => (implicant.Fixed >> (variables.Length - 1 - bit) & 1) == 1)
                .Select(bit => (implicant.Bits >> (variables.Length - 1 - bit) & 1) == 1
                    ? (Entity)variables[bit]
                    : !variables[bit])
                .Aggregate((left, right) => left & right);
    }
}
