using System.Collections.Generic;
using HonkSharp.Fluency;
using HonkSharp.Functional;
using static AngouriMath.Entity.Number;

namespace AngouriMath.Functions
{
    internal static class Fraction
    {
        private enum NextAction
        {
            GiveItAChance,
            MoveOver
        }
        
        /// <summary>
        /// Decomposes an arbitrary rational
        /// number into sum of rationals a_i / p_i^k,
        /// where p_i is a prime number. Evaluates
        /// lazily.
        /// </summary>
        internal static IEnumerable<(Integer numerator, Integer denPrime, Integer denPower)> Decompose(Integer num, Integer den)
        {
            var primeId = 0;
            var power = 1;
            while (num != 0)
            {
                if (Expand(num, den, primeId, power).Alias(out var expansion).Is<(Integer, Integer, Integer, Integer, Integer)>(out var valid))
                {
                    var (newNum, y, denPrime, _, newDen) = valid;
                    if (y != 0)
                        yield return (y, denPrime, power);
                    num = newNum;
                    den = newDen;
                    power++;
                }
                else if (expansion.As<NextAction>().AssumeBest() == NextAction.GiveItAChance)
                    power++;
                else
                {
                    power = 1;
                    primeId++;
                }
            }

            static Either<(Integer newNum, Integer resNum, Integer denPrime, Integer resDen, Integer newDen), NextAction> Expand(Integer num, Integer den, int primeId, int power)
            {
                var prime = Primes.GetPrime(primeId);
                
                var resDen = (Integer)prime.EInteger.Pow(power);
                
                if (den % resDen != 0)
                    return NextAction.MoveOver;

                var newDen = den.IntegerDiv(resDen);
                
                // den = prime ^ power * newDen = resDen * newDen
                // num / den = a / resDen + b / newden
                // num = a * newDen + b * resDen
                if (Diophantine.Solve(newDen, resDen, num) is not var (b, a))
                    return NextAction.GiveItAChance;

                // a is numerator to yield return
                // b is the new numerator
                var resNum = a;
                var newNum = b;

                return (newNum, resNum, prime, resDen, newDen);
            }
        }
    }
}
