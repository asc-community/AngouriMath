
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


using AngouriMath.Core;
using AngouriMath.Core.Exceptions;
using AngouriMath.Functions.Algebra.NumbericalSolving;
using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Functions.Algebra.InequalitySolver
{
    internal static class NumericalInequalitySolver
    {
        /// <summary>
        /// expr must contain only VariableEntity x as a variable
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="x"></param>
        /// <param name="sign">
        /// ">"
        /// "<"
        /// ">="
        /// "<="
        /// </param>
        /// <returns></returns>
        internal static Set _Solve(Entity expr, VariableEntity x, string sign)
        {
            bool Corresponds(Number val)
            => sign switch
            {
                ">" => val.Re > 0,
                "<" => val.Re < 0,
                ">=" => val.Re >= 0,
                "<=" => val.Re <= 0,
                _ => throw new SysException("Uknown sign")
            } && val.IsReal();

            var compiled = expr.Compile(x);
            var roots = expr.SolveNt(x);
            var realRootsSet = roots.FiniteWhere(root => root.Eval().IsReal());
            var realRoots = realRootsSet.FiniteSet().ToList();
            realRoots.Sort((a, b) => a.Eval().Re.CompareTo(b.Eval().Re));
            if (realRoots.Count > 0)
            {
                realRoots.Insert(0, realRoots[0].Eval() - 5);
                realRoots.Insert(realRoots.Count, realRoots[realRoots.Count - 1].Eval() + 5);
            }
            var result = new Set();
            for(int i = 0; i < realRoots.Count - 1; i++)
            {
                var left = realRoots[i].Eval().Re;
                var right = realRoots[i + 1].Eval().Re;
                var point = (left + right) / 2;
                var val = compiled.Call(point);
                if (Corresponds(val))
                    result.Add((left, right, false, false));
            }
            if (sign.Contains("="))
                return (realRootsSet | result) as Set;
            else
                return result;
        }

        internal static Set Solve(Entity expr, VariableEntity x, string sign)
        {
            var uv = MathS.Utils.GetUniqueVariables(expr);
            if (uv.Count != 1 || 
                uv.Pieces[0].Type != Piece.PieceType.ENTITY || 
                uv.Pieces[0].UpperBound().Item1 != x)
                throw new MathSException("expr should only contain VariableEntity x");
            return _Solve(expr, x, sign);
        }
    }
}
