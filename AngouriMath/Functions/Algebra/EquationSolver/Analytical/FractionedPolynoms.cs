
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
        internal static Set Solve(Entity expr, VariableEntity x)
        {
            var children = TreeAnalyzer.GatherLinearChildrenOverAndExpand(
                expr, entity => entity.FindSubtree(x) != null
            );

            if (children is null)
                return null;

            children = children.Select(c => c.InnerSimplify()).ToList();

            Entity normalPolynom = 0;
            var fractioned = new List<(Entity multiplier, List<(Entity main, IntegerNumber pow)> fracs)>();

            foreach (var child in children)
            {
                (Entity multiplier, List<(Entity main, IntegerNumber pow)> fracs) potentialFraction;
                potentialFraction.multiplier = 1;
                potentialFraction.fracs = new List<(Entity main, IntegerNumber pow)>();
                foreach (var mpChild in TreeAnalyzer.LinearChildrenOverProduct(child))
                {
                    if (mpChild.entType != Entity.EntType.OPERATOR)
                    {
                        potentialFraction.multiplier *= mpChild;
                        continue;
                    }
                    if (mpChild.Name != "powf")
                    {
                        potentialFraction.multiplier *= mpChild;
                        continue;
                    }
                    if (mpChild.Children[1].entType != Entity.EntType.NUMBER)
                    {
                        potentialFraction.multiplier *= mpChild;
                        continue;
                    }
                    if (mpChild.Children[0].FindSubtree(x) == null)
                    {
                        potentialFraction.multiplier *= mpChild;
                        continue;
                    }
                    var num = (mpChild.Children[1] as NumberEntity).Value;
                    if (!num.IsRational())
                        return null; // (x + 1)^0.2348728
                    if (!num.IsFraction()) // (x + 3) ^ 3
                    { 
                        potentialFraction.multiplier *= mpChild;
                        continue;
                    }
                    var fracNum = num as RationalNumber;
                    var newChild = MathS.Pow(mpChild.Children[0], fracNum.Numerator).InnerSimplify();
                    var den = fracNum.Denominator;
                    potentialFraction.fracs.Add((newChild, den));
                }

                if (potentialFraction.fracs.Count > 0)
                    fractioned.Add(potentialFraction);
                else
                    normalPolynom += child;

                if (fractioned.Count > 1)
                    return null; // (x + 1)^(1/2) + (x + 2)^(1/3) unsolvable yet
            }

            if (fractioned.Count == 0)
                return null; // means that one can either be solved polynomially or unsolvable at all

            var fractionedProduct = fractioned[0];

            var lcm = Utils.LCM(fractionedProduct.fracs.Select(
                c => c.pow.Value
                ).ToArray());
            var intLcm = Number.Create(lcm);

            //                        "-" to compensate sum: x + sqrt(x + 1) = 0 => x = -sqrt(x+1)
            Entity mp = MathS.Pow(-fractionedProduct.multiplier, intLcm);
            foreach (var mainPowPair in fractionedProduct.fracs)
                mp *= MathS.Pow(mainPowPair.main, Number.Create(lcm.Divide(mainPowPair.pow.Value)));

            var finalExpr = MathS.Pow(normalPolynom, intLcm) - mp;

            return finalExpr.SolveEquation(x);
        }
    }
}
