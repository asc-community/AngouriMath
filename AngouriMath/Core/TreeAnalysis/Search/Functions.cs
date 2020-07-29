
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

namespace AngouriMath.Core.TreeAnalysis
{
    internal static partial class TreeAnalyzer
    {
        /// <summary>
        /// If an evaluable expression is equal to zero, true, otherwise, false
        /// For example, 1 - 1 is zero, but 1 + a is not
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        internal static bool IsZero(Entity e) => MathS.CanBeEvaluated(e) && e.Eval() == 0;

        /// <summary>
        /// a ^ (-1) => 1 / a
        /// </summary>
        /// <param name="expr"></param>
        internal static void InvertNegativePowers(ref Entity expr)
        {
            if (expr is OperatorEntity { Name: "powf" } &&
                expr.GetChild(1) is NumberEntity { Value:IntegerNumber pow } && pow < 0)
                expr = 1 / MathS.Pow(expr.GetChild(0), -1 * pow);
            else
            {
                for (int i = 0; i < expr.ChildrenCount; i++)
                {
                    var tmp = expr.GetChild(i);
                    InvertNegativePowers(ref tmp);
                    expr.SetChild(i, tmp);
                }
            }
        }

        internal static readonly Pattern NegativeMultiplyerPattern = Patterns.any1 + Patterns.const1 * Patterns.any2;

        /// <summary>
        /// 1 + (-x) => 1 - x
        /// </summary>
        /// <param name="expr"></param>
        internal static void InvertNegativeMultipliers(ref Entity expr)
        {
            if (NegativeMultiplyerPattern.Match(expr)
                && expr.GetChild(1).GetChild(0) is NumberEntity { Value:RealNumber { Value:var real } }
                && real > IntegerNumber.Zero)
                expr = expr.GetChild(0) - (-1 * real) * expr.GetChild(1).GetChild(1);
            else
            {
                for (int i = 0; i < expr.ChildrenCount; i++)
                {
                    var tmp = expr.GetChild(i);
                    InvertNegativePowers(ref tmp);
                    expr.SetChild(i, tmp);
                }
            }
        }
    }
}
