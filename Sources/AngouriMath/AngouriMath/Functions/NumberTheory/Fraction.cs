//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

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
            if (den == 1)
            {
                yield return (num, 1, 1);
                yield break;
            }
            
            // Statements are referenced with [i].
            //
            // The algorithm works as follows.
            // `num` and `den` are current numerator and denominator.
            // By default, they equal those of the rational we want
            // to decompose. The algorithm finishes when the
            // numerator is zeroed [1].
            
            // newNum - the new numerator (replaces the current with this one)
            // newDen - the new denominator
            // resNum - the result numerator (the one to yield)
            // resDenPrime - the exp base of the result denominator
            // resDenPower - the exp power of the result denominator
            
            //
            // For each prime `prime`,
            //     Find the highest power `power` such that
            //     `den` is divided by `power` [2]. If it equals
            //     0, then we move to the next prime [3].
            //
            //     Now we have `resDen` = `prime` ^ `power`
            //     `newDen` = `den` / `prime` ^ `power`
            //     we need to find `resNum` and `newNum` by
            //     solving a diophantine equation:
            //     `num` / `den` = `resNum` / `resDen` + `newNum` / `newDen` [4]

            var primeId = 0;
            while (num != 0) // [1]
            {
                // [4]
                // Assume num and den are current numerator and denominator, then
                // num / den  = newNum / newDen + resNum / resDenPrime ^ resDenPower
                //
                // Hence, here is how we move:
                // num <- newNum
                // den <- newDen
                // yield resNum, resDenPrime, resDenPower
                if (Expand(num, den, primeId) is var (newNum, newDen, resNum, resDenPrime, resDenPower))
                {
                    if (resNum != 0)
                        yield return (resNum, resDenPrime, resDenPower);
                    num = newNum;
                    den = newDen;
                }
                primeId++;
            }

            
            static (Integer newNum, Integer newDen, Integer resNum, Integer resDenPrime, Integer resDenPower)? Expand(Integer num, Integer den, int primeId)
            {
                var prime = Primes.GetPrime(primeId);
                
                // [2]
                var power = 0;
                while (den % prime == 0)
                {
                    power++;
                    den = den.IntegerDiv(prime);
                }
                
                // [3]
                if (power is 0)
                    return null;

                var resDen = (Integer)prime.EInteger.Pow(power);

                var newDen = den;
                
                if (newDen == 1)
                    return (newNum: 0, newDen: 1, resNum: num, resDenPrime: prime, resDenPower: power); 
                // den = prime ^ power * newDen = resDen * newDen
                // num / den = a / resDen + b / newden
                // num = a * newDen + b * resDen
                if (Diophantine.Solve(newDen, resDen, num) is not var (a, b))
                    return null;

                // a is numerator to yield return
                // b is the new numerator
                var resNum = a;
                var newNum = b;

                return (newNum, newDen, resNum, prime, power);
            }
        }
    }
}
