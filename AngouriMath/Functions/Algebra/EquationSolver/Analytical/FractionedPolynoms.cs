
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
using AngouriMath.Core.Numerix;
using AngouriMath.Core.TreeAnalysis;

namespace AngouriMath.Functions.Algebra.AnalyticalSolving
{
    internal static class FractionedPolynoms
    {
        internal static Set? Solve(Entity expr, VariableEntity x)
        {
            var childrenRaw = TreeAnalyzer.GatherLinearChildrenOverAndExpand(
                expr, entity => entity.FindSubtree(x) is { }
            );

            if (childrenRaw is null)
                return null;

            childrenRaw = childrenRaw.Select(c => c.InnerSimplify()).ToList();
            var children = new List<Entity>();
            foreach (var child in childrenRaw)
            {
                var ch = child;
                // sqrt(f(x))^2 => f(x)
                TreeAnalyzer.ReplaceInPlace(Patterns.PowerRules, ref ch);
                children.Add(ch);
            }

            Entity normalPolynom = 0;
            var fractioned = new List<(Entity multiplier, List<(Entity main, IntegerNumber pow)> fracs)>();

            foreach (var child in children)
            {
                (Entity multiplier, List<(Entity main, IntegerNumber pow)> fracs) potentialFraction;
                potentialFraction.multiplier = 1;
                potentialFraction.fracs = new List<(Entity main, IntegerNumber pow)>();
                foreach (var mpChild in TreeAnalyzer.LinearChildrenOverProduct(child))
                {
                    if (!(mpChild is OperatorEntity))
                    {
                        potentialFraction.multiplier *= mpChild;
                        continue;
                    }
                    if (mpChild.Name != "powf")
                    {
                        potentialFraction.multiplier *= mpChild;
                        continue;
                    }
                    if (!(mpChild.Children[1] is NumberEntity { Value:var num }))
                    {
                        potentialFraction.multiplier *= mpChild;
                        continue;
                    }
                    if (mpChild.Children[0].FindSubtree(x) is null)
                    {
                        potentialFraction.multiplier *= mpChild;
                        continue;
                    }
                    if (!(num is RationalNumber fracNum))
                        return null; // (x + 1)^0.2348728
                    if (num is IntegerNumber) // (x + 3) ^ 3
                    { 
                        potentialFraction.multiplier *= mpChild;
                        continue;
                    }
                    var newChild = MathS.Pow(mpChild.Children[0], (IntegerNumber)fracNum.Value.Numerator).InnerSimplify();
                    var den = fracNum.Value.Denominator;
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
            static bool BasesAreEqual(List<(Entity main, IntegerNumber pow)> f1,
                List<(Entity main, IntegerNumber pow)> f2)
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

            var lcm = Utils.LCM(fracs.Select(
                c => c.pow.Value
                ).ToArray());
            var intLcm = IntegerNumber.Create(lcm);

            //                        "-" to compensate sum: x + sqrt(x + 1) = 0 => x = -sqrt(x+1)
            Entity mp = MathS.Pow(-multiplier, intLcm).InnerSimplify();
            foreach (var (main, pow) in fracs)
                mp *= MathS.Pow(main, IntegerNumber.Create(lcm.Divide(pow.Value)));

            var finalExpr = MathS.Pow(normalPolynom, intLcm) - mp;

            return finalExpr.SolveEquation(x);
        }
    }
}
