/* 
 * Copyright (c) 2019-2021 Angouri.
 * AngouriMath is licensed under MIT. 
 * Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
 * Website: https://am.angouri.org.
 */
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AngouriMath.Functions.Algebra;
using static AngouriMath.Entity;

namespace AngouriMath.Core
{
    /// <summary>
    /// A class for systems of equations
    /// Is not part of AM's ecosystem,
    /// that is, it is not an Entity, just an intermediate
    /// class
    /// </summary>
    public sealed class EquationSystem : ILatexiseable
    {
        private readonly IEnumerable<Entity> equations;

        /// <summary>After having created a system of equations, you may solve or latexise it</summary>
        /// <param name="equations">
        /// Any <see cref="IEnumerable{T}"/> parameter, such as <see cref="List{T}"/> or <see cref="System.Array"/>
        /// </param>
        public EquationSystem(IEnumerable<Entity> equations) => this.equations = equations;

        /// <summary>After having created a system of equations, you may solve or latexise it</summary>
        /// <param name="equations">An arbitrary number of equations</param>
        public EquationSystem(params Entity[] equations) => this.equations = equations;

        /// <summary>
        /// Returns a solution matrix
        /// The first axis of the matrix corresponds to the number of solutions,
        /// the second one corresponds to the number of variables.
        /// </summary>
        /// <param name="vars">Number of variables must match number of equations</param>
        public Tensor? Solve(params Variable[] vars) => EquationSolver.SolveSystem(equations, vars);
        /// <summary>
        /// Returns a solution matrix
        /// The first axis of the matrix corresponds to the number of solutions,
        /// the second one corresponds to the number of variables.
        /// </summary>
        /// <param name="vars">Number of variables must match number of equations</param>
        public Tensor? Solve(System.ReadOnlySpan<Variable> vars) => EquationSolver.SolveSystem(equations, vars);

        /// <returns>Latexised version of the system</returns>
        public string Latexise()
        {
            using var enumerator = equations.GetEnumerator();
            if (!enumerator.MoveNext())
                return string.Empty;
            var firstEquation = enumerator.Current.Latexise() + " = 0";
            if (!enumerator.MoveNext())
                return firstEquation;
            var sb = new StringBuilder(@"\begin{cases}").Append(firstEquation);
            do sb.Append(@"\\").Append(enumerator.Current.Latexise()).Append(" = 0");
            while (enumerator.MoveNext());
            sb.Append(@"\end{cases}");
            return sb.ToString();
        }

        /// <inheritdoc/>
        public override string ToString() => string.Join("\n", equations.Select(c => c.Stringize() + " = 0"));
    }
}