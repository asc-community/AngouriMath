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
        [ConcurrentField] private static readonly List<(Integer actual, int? cache)> primes = new(capacity: 20) { (2, 2), (3, 3), (5, 5), (7, 7), (11, 11) };


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
            EnsurePrimesExist(index);
            return primes[index].actual;
        }
    }
}
