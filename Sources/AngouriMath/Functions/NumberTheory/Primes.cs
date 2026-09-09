//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Exceptions;
using PeterO.Numbers;
using System;

namespace AngouriMath.Functions
{
    internal static class Primes
    {
        [ConstantField] private static readonly object addingPrimes = new();
        /// <summary>
        /// The primes found so far. Grown only under <see cref="addingPrimes"/>, and read only
        /// from inside that lock — <see cref="GetPrime"/> reads <see cref="published"/> instead.
        /// </summary>
        [ConcurrentField] private static readonly List<(Integer actual, int? cache)> primes = new(capacity: 20) { (2, 2), (3, 3), (5, 5), (7, 7), (11, 11) };

        /// <summary>
        /// The same primes as a snapshot nobody mutates, for readers outside the lock.
        /// </summary>
        /// <remarks>
        /// <c>GetPrime</c> used to call <see cref="EnsurePrimesExist"/> and then index
        /// <see cref="primes"/> after the lock had been released, which is not safe for a
        /// <see cref="List{T}"/> that another thread may be growing: <c>Add</c> reallocates the
        /// backing array, so the reader can see the new <c>Count</c> against the old array and
        /// come back with a different element. A wrong prime is a well-formed integer and
        /// propagates into factorisation rather than throwing. Publishing an array by a single
        /// reference write keeps the read lock-free and correct: a reader takes the reference
        /// once, which is atomic, and then indexes something that is never written again.
        /// </remarks>
        [ConcurrentField] private static volatile Integer[] published = ToSnapshot();

        /// <summary>The current <see cref="primes"/> as an array. Call under the lock.</summary>
        private static Integer[] ToSnapshot()
        {
            var snapshot = new Integer[primes.Count];
            for (var i = 0; i < snapshot.Length; i++)
                snapshot[i] = primes[i].actual;
            return snapshot;
        }


        private static void EnsurePrimesExist(int index)
        {
            lock (addingPrimes)
            {
                if (index < primes.Count)
                    return;

                if (index < 100)
                // The fast way (operating on ints)
                {
                    while (primes.Count <= index)
                        AddPrimeC();
                }
                else
                // The slow way (operating on Integers)
                {
                    while (primes.Count <= index)
                        AddPrimeI();
                }

                // The publishing write, still under the lock. Everything above happened to a
                // list no reader outside it has.
                published = ToSnapshot();
            }

            static bool IsPrimeC(int num)
            {
                var max = (int)Math.Sqrt(num) + 1;
                for (int i = 0; primes[i].cache < max; i++)
                    if (num % primes[i].cache == 0)
                        return false;
                return true;
            }

            static bool IsPrimeI(Integer num)
            {
                var max = (EInteger)EFloat.FromEInteger(num.EInteger).Sqrt(MathS.Settings.DecimalPrecisionContext).RoundToIntegerExact(MathS.Settings.DecimalPrecisionContext) + 1;
                for (var i = 0; primes[i].actual < max; i++)
                    if (num % primes[i].actual == Integer.Zero)
                        return false;
                return true;
            }

            static void AddPrimeC()
            {
                var n = (primes[^1].cache + 2) ?? throw new AngouriBugException("It was supposed to be not null");
                while (!IsPrimeC(n))
                    n += 2;
                primes.Add((n, n));
            }

            static void AddPrimeI()
            {
                var n = (primes[^1].actual + 2) ?? throw new AngouriBugException("It was supposed to be not null");
                while (!IsPrimeI(n))
                    n += 2;
                primes.Add((n, null));
            }
        }

        internal static Integer GetPrime(int index)
        {
            // One volatile read, then an array that is never written again.
            var snapshot = published;
            if (index < snapshot.Length)
                return snapshot[index];
            EnsurePrimesExist(index);
            return published[index];
        }
    }
}
