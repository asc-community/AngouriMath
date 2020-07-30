using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using AngouriMath.Core;
using AngouriMath.Core.Numerix;
using PeterO.Numbers;

namespace AngouriMath.Functions.DiscreteMath
{
    internal static class Combinatorics
    {
        internal static EInteger C(EInteger n, EInteger k) => n.Factorial() / ((n - k).Factorial() * k.Factorial());
        internal static IEnumerable<List<EInteger>> Combinations(EInteger min, EInteger max, EInteger cellCount)
        {
            for (EInteger i = min; i <= max; i++)
            {
                if (!cellCount.Equals(EInteger.One))
                    foreach (var l in Combinations(i + 1, max, cellCount - 1))
                    {
                        l.Add(i);
                        yield return l;
                    }
                else
                    yield return new List<EInteger> { i };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemCount"></param>
        /// <param name="targetSum"></param>
        /// <returns></returns>
        internal static IEnumerable<List<EInteger>> CombinateSums(EInteger itemCount, EInteger targetSum)
        {
            foreach (var comb in Combinations(1, targetSum + itemCount - 1, itemCount - 1))
            {
                var newComb = new List<EInteger> { 0 };
                comb.Reverse();
                newComb.AddRange(comb);
                newComb.Add(targetSum + itemCount);

                var item = new List<EInteger>();
                for (int i = 0; i < itemCount; i++)
                    item.Add(newComb[i + 1] - newComb[i] - 1);
                yield return item;
            }
        }
    }
}
