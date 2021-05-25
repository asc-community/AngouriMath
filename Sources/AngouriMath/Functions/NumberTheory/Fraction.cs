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
                var (newNum, y, denPrime, denPower, newDen) = Expand(num, den, primeId);
                yield return (y, denPrime, denPower);
                num = newNum;
                den = newDen;
                primeId++;
            }

            static (Integer x, Integer y, Integer denPrime, Integer denPower, Integer newDen) Expand(Integer num, Integer den, int primeId)
            {
                var prime = Primes.GetPrime(primeId);
                int denPower = 0;
                while (den % prime == 0)
                {
                    den = den.IntegerDiv(prime);
                    denPower++;
                }

                /* 
                 * c / (a b) = x / b + y / a    <=>    a x + b y = c
                 * 
                 * a = p ^ k
                 * b = den / p^k
                 * 
                 * gcd(a, b) = 1 so there always should be a solution over x and y
                 * 
                */
                
                var a = (Integer)prime.EInteger.Pow(denPower);
                var b = den;
                var c = num;

                if (Diophantine.Solve(a, b, c) is not var (x, y))
                    throw new AngouriBugException("There should always be a solution");

                return (x, y, prime, denPower, den);
            }
        }
    }
}
