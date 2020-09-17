
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
using System;
using System.Linq;
using static AngouriMath.Entity;

namespace AngouriMath.Functions.Boolean
{
    internal static class BooleanSolver
    {
        private static bool Next(in Span<bool> states)
        {
            var id = states.Length - 1;
            if (!states[id])
            {
                states[id] = true;
                return true;
            }
            while (id > -1 && states[id])
            {
                states[id] = false;
                id--;
            }
            if (id == -1)
                return false;
            states[id] = true;
            return true;
        }
        
        /// <summary>
        /// Returns a tensor of solutions over <paramref name="variables"/> so that
        /// the expression turns into a True when evaled. Computes the roots by
        /// compiling the truth table
        /// </summary>
        internal static Tensor? SolveTable(Entity expr, Variable[] variables)
        {
            var count = expr.Vars.Count();
            // TODO: we probably also should verify the uniqueness of the given variables
            if (count != variables.Length)
                throw new ArgumentException("Number of variables must equal number of variables in the expression");
            Span<bool> states = stackalloc bool[variables.Length];
            var tb = new TensorBuilder(count);
            do
            {
                var exprSubstituted = expr;
                for (var i = 0; i < count; i++)
                    exprSubstituted = exprSubstituted.Substitute(variables[i], states[i]);
                if (exprSubstituted.EvalBoolean())
                    tb.Add(states.ToArray().Select(s => (Entity)s));
            }
            while (Next(states));

            return tb.ToTensor();
        }
    }
}
