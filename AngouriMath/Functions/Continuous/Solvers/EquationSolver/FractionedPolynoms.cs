
/* Copyright (c) 2019-2020 Angourisoft
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,
 * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software
 * is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System.Collections.Generic;
using System.Linq;
using AngouriMath.Core;

namespace AngouriMath.Functions.Algebra.AnalyticalSolving
{
    using static Entity;
    using static Entity.Number;
    internal static class FractionedPolynoms
    {
        internal static SetNode? Solve(Entity expr, Variable x)
        {
            var children = TreeAnalyzer.GatherLinearChildrenOverSumAndExpand(
                expr, entity => entity.ContainsNode(x)
            );

            if (children is null)
                return null;

            Entity normalPolynom = 0;
            var fractioned = new List<(Entity multiplier, List<(Entity main, Integer pow)> fracs)>();

            // Use PowerRules to replace sqrt(f(x))^2 => f(x)
            foreach (var child in children.Select(c => c.InnerSimplify().Replace(Patterns.PowerRules)))
            {
                (Entity multiplier, List<(Entity main, Integer pow)> fracs) potentialFraction;
                potentialFraction.multiplier = 1;
                potentialFraction.fracs = new List<(Entity main, Integer pow)>();
                foreach (var mpChild in Mulf.LinearChildren(child))
                {
                    if (!(mpChild is Powf(var @base, Number num and not Integer))) // (x + 3) ^ 3
                    {
                        potentialFraction.multiplier *= mpChild;
                        continue;
                    }
                    if (!(num is Rational fracNum))
                        return null; // (x + 1)^0.2348728
                    var newChild = MathS.Pow(@base, fracNum.ERational.Numerator).InnerSimplify();
                    var den = fracNum.ERational.Denominator;
                    potentialFraction.fracs.Add((newChild, den));
                }

                if (potentialFraction.fracs.Count > 0)
                    fractioned.Add(potentialFraction);
                else
                    normalPolynom += child;
            }

            if (fractioned.Count == 0)
                return null; // means that one can either be solved polynomially or unsolvable at all

            // starting from i = 1 check if all are equal to [0]
            static bool BasesAreEqual(List<(Entity main, Integer pow)> f1,
                List<(Entity main, Integer pow)> f2)
            {
                if (f1.Count != f2.Count)
                    return false;
                for (int i = 0; i < f1.Count; i++)
                    if (f1[i].main != f2[i].main || f1[i].pow != f2[i].pow)
                        return false;
                return true;
            }
            for (int i = 1; i < fractioned.Count; i++)
            {
                if (BasesAreEqual(fractioned[i].fracs, fractioned[0].fracs))
                {
                    var were = fractioned[0];
                    fractioned[0] = (were.multiplier + fractioned[i].multiplier, were.fracs);
                }
                else
                    return null;
            }

            var (multiplier, fracs) = fractioned[0];

            var lcm = fracs.Select(c => c.pow.EInteger).Aggregate((aggregate, current) => aggregate.Lcm(current));
            var intLcm = Integer.Create(lcm);

            //                        "-" to compensate sum: x + sqrt(x + 1) = 0 => x = -sqrt(x+1)
            var mp = MathS.Pow(-multiplier, intLcm).InnerSimplify();
            foreach (var (main, pow) in fracs)
                mp *= MathS.Pow(main, Integer.Create(lcm.Divide(pow.EInteger)));

            var finalExpr = MathS.Pow(normalPolynom, intLcm) - mp;

            return AnalyticalEquationSolver.Solve(finalExpr, x);
        }
    }
}
