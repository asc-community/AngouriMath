
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



﻿using System;
using System.Collections.Generic;
using System.Text;

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
            if (expr.entType == Entity.EntType.OPERATOR &&
                expr.Name == "powf" &&
                expr.Children[1].entType == Entity.EntType.NUMBER &&
                expr.Children[1].GetValue().IsInteger() &&
                !expr.Children[1].GetValue().IsNatural())
                expr = 1 / MathS.Pow(expr.Children[0], (-1 * expr.Children[1].GetValue()));
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

        /// <summary>
        /// 1 + (-x) => 1 - x
        /// </summary>
        /// <param name="expr"></param>
        internal static void InvertNegativeMultipliers(ref Entity expr)
        {
            bool ifMatches = NegativeMultiplyerPattern.Match(expr);
            bool isRealAndMatches = ifMatches && expr.Children[1].Children[0].GetValue().IsReal();
            bool isPositiveAndIsRealAndMatches = isRealAndMatches && expr.Children[1].Children[0].GetValue().Real > 0;
            if (isPositiveAndIsRealAndMatches)
                expr = expr.Children[0] - (-1 * expr.Children[1].Children[0].GetValue()) * expr.Children[1].Children[1];
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
    }
}
