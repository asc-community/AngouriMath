using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using AngouriMath.Core;
using AngouriMath.Core.Numerix;

namespace AngouriMath.Functions.DiscreteMath
{
    internal static class Combinatorics
    {
        internal static BigInteger Factorial(int t)
            => t == 0 ? 1 : t * Factorial(t - 1);

        internal static BigInteger C(int n, int k)
            => Factorial(n) / (Factorial(n - k) * Factorial(k));

        internal static IEnumerable<List<int>> Combinations(int min, int max, int cellCount)
        {
            var res = new List<List<int>>();
            for (int i = min; i <= max; i++)
            {
                if (cellCount != 1)
                    foreach (var l in Combinations(i + 1, max, cellCount - 1))
                    {
                        l.Add(i);
                        yield return l;
                    }
                else
                    yield return new List<int> {i};
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemCount"></param>
        /// <param name="targetSum"></param>
        /// <returns></returns>
        internal static IEnumerable<List<int>> CombinateSums(int itemCount, int targetSum)
        {
            var res = new List<List<int>>();
            foreach (var comb in Combinations(1, targetSum + itemCount - 1, itemCount - 1))
            {
                var newComb = new List<int>();
                newComb.Add(0);
                comb.Reverse();
                newComb.AddRange(comb);
                newComb.Add(targetSum + itemCount);

                var item = new List<int>();
                for (int i = 0; i < itemCount; i++)
                    item.Add(newComb[i + 1] - newComb[i] - 1);
                yield return item;
            }
        }
    }
}
