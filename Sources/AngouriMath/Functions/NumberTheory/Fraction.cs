using AngouriMath.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using static AngouriMath.Entity.Number;

namespace AngouriMath.Functions
{
    internal static class Fraction
    {
        /// <summary>
        /// Decomposes an arbitrary rational
        /// number into sum of rationals a_i / p_i^k,
        /// where p_i is a prime number. Evaluates
        /// lazily.
        /// </summary>
        internal static IEnumerable<(Integer numerator, Integer denPrime, Integer denPower)> Decompose(Integer num, Integer den)
        {
            var primeId = 0;
            while (num != 0)
            {
                if (Expand(num, den, primeId) is var (newNum, y, denPrime, denPower, _, _))
                {
                    yield return (y, denPrime, denPower);
                    num = newNum;
                }
                primeId++;
            }

            static (Integer newNum, Integer resNum, Integer denPrime, Integer denPower, Integer resDen, Integer newDen)? Expand(Integer num, Integer den, int primeId)
            {
                var prime = Primes.GetPrime(primeId);
                int denPower = 0;
                while (den % prime == 0)
                {
                    den = den.IntegerDiv(prime);
                    denPower++;
                }

                if (denPower == 0)
                    return null;

                var resDen = (Integer)prime.EInteger.Pow(denPower);
                var newNum = num % den;
                var resNum = num.IntegerDiv(den);

                return (newNum, resNum, prime, denPower, resDen, den);
            }
        }
    }
}
