
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


using AngouriMath.Core.Numerix;
using System.Diagnostics;

namespace AngouriMath.Core.TreeAnalysis
{
    internal static partial class TreeAnalyzer
    {
        /// <summary>
        /// If an evaluable expression is equal to zero, <see langword="true"/>, otherwise, <see langword="false"/>
        /// For example, 1 - 1 is zero, but 1 + a is not
        /// </summary>
        internal static bool IsZero(Entity e) => MathS.CanBeEvaluated(e) && e.Eval() == 0;

        /// <summary>a ^ (-1) => 1 / a</summary>
        internal static void InvertNegativePowers(ref Entity expr)
        {
            if (expr is OperatorEntity { Name: "powf" } &&
                expr.Children[1] is NumberEntity { Value:IntegerNumber pow } && pow < 0)
                expr = 1 / MathS.Pow(expr.Children[0], -1 * pow);
            else
            {
                for (int i = 0; i < expr.Children.Count; i++)
                {
                    var tmp = expr.Children[i];
                    InvertNegativePowers(ref tmp);
                    expr.Children[i] = tmp;
                }
            }
        }

        internal static readonly Pattern NegativeMultiplyerPattern = Patterns.any1 + Patterns.const1 * Patterns.any2;
        /// <summary>1 + (-x) => 1 - x</summary>
        internal static void InvertNegativeMultipliers(ref Entity expr)
        {
            if (NegativeMultiplyerPattern.Match(expr)
                && expr.Children[1].Children[0] is NumberEntity { Value:RealNumber { Value:var real } }
                && real > IntegerNumber.Zero)
                expr = expr.Children[0] - (-1 * real) * expr.Children[1].Children[1];
            else
            {
                for (int i = 0; i < expr.Children.Count; i++)
                {
                    var tmp = expr.Children[i];
                    InvertNegativePowers(ref tmp);
                    expr.Children[i] = tmp;
                }
            }
        }

        internal static readonly Pattern FactorialDivisionPatternCC =
            Factorialf.PHang(Patterns.any1 + Patterns.const1) / Factorialf.PHang(Patterns.any1 + Patterns.const2);
        internal static readonly Pattern FactorialDivisionPatternC0 =
            Factorialf.PHang(Patterns.any1 + Patterns.const1) / Factorialf.PHang(Patterns.any1);
        internal static readonly Pattern FactorialDivisionPattern0C =
            Factorialf.PHang(Patterns.any1) / Factorialf.PHang(Patterns.any1 + Patterns.const2);

        /// <summary>(x + a)! / (x + b)! -> (x+b+1)*(x+b+2)*...*(x+a)</summary>
        internal static void ExpandFactorialDivisions(ref Entity expr)
        {
            (Entity any1, Entity const1, Entity const2)? consts = null;
            if (FactorialDivisionPatternCC.Match(expr))
                consts = (expr.Children[0].Children[0].Children[0],
                          expr.Children[0].Children[0].Children[1],
                          expr.Children[1].Children[0].Children[1]);
            else if (FactorialDivisionPatternC0.Match(expr))
                consts = (expr.Children[0].Children[0].Children[0],
                          expr.Children[0].Children[0].Children[1],
                          new NumberEntity(0));
            else if (FactorialDivisionPattern0C.Match(expr))
                consts = (expr.Children[1].Children[0].Children[0],
                          new NumberEntity(0),
                          expr.Children[1].Children[0].Children[1]);
            if (consts is (var x,
                           NumberEntity { Value: IntegerNumber { Value:var num } },
                           NumberEntity { Value: IntegerNumber { Value:var den } })
                && (num - den).Abs() < 20) // We don't want to expand (x+100)!/x!
                if (num > den) // e.g. (x+3)!/x! = (x+1)(x+2)(x+3)
                {
                    expr = Add(den + 1, x);
                    for (var i = den + 2; i <= num; i++)
                        expr *= Add(i, x);
                } else // e.g. x!/(x+3)! = 1/(x+1)/(x+2)/(x+3)
                {
                    expr = 1 / Add(num + 1, x);
                    for (var i = num + 2; i <= den; i++)
                        expr /= Add(i, x);
                }
            static Entity Add(PeterO.Numbers.EInteger a, Entity b) =>
                a.IsZero ? b : new NumberEntity(a) + b;
        }
    }
}
