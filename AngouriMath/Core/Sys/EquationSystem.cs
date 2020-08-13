
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
using System.Text;
using AngouriMath.Functions.Algebra;

namespace AngouriMath.Core.Sys
{
    using static Entity;
    public class EquationSystem : ILatexiseable
    {
        private readonly List<Entity> equations;

        /// <summary>
        /// After having created a system of equations,
        /// you may solve or latexise it
        /// </summary>
        /// <param name="equations"></param>
        public EquationSystem(params Entity[] equations) => this.equations = equations.ToList();

        /// <summary>
        /// Returns a solution matrix
        /// The first axis of the matrix corresponds to the number of solutions,
        /// the second one corresponds to the number of variables.
        /// </summary>
        /// <param name="vars">
        /// Number of variables must match number of equations
        /// </param>
        /// <returns></returns>
        public Tensor? Solve(params Var[] vars) => EquationSolver.SolveSystem(equations, vars);

        /// <summary>
        /// Returns latexised version of the system
        /// </summary>
        /// <returns></returns>
        public string Latexise()
        {
            if (equations.Count == 1)
                return equations[0].Latexise() + " = 0";
            if (equations.Count == 0)
                return string.Empty;
            var sb = new StringBuilder();
            sb.Append(@"\begin{cases}");
            foreach (var eq in equations)
                sb.Append(eq.Latexise()).Append(" = 0").Append(@"\\");
            sb.Append(@"\end{cases}");
            return sb.ToString();
        }
    }
}
