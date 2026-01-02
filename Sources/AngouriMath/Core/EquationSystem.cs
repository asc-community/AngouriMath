//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Text;
using AngouriMath.Functions.Algebra;
using static AngouriMath.Entity;

namespace AngouriMath.Core
{
    /// <summary>
    /// A class for systems of equations. Is not part of AM's ecosystem,
    /// that is, it is not an Entity, just an intermediate class.
    /// It is a system of arbitrary equations, not only those linear. However,
    /// it is not a system of <see cref="Statement"/>s.
    /// </summary>
    public sealed class EquationSystem : ILatexiseable
    {
        private readonly IEnumerable<Entity> equations;

        /// <summary>
        /// The equations you pass should not have an <see cref="MathS.Equality"/> node.
        /// After having created a system of equations, you may solve or latexise it.
        /// </summary>
        /// <param name="equations">
        /// Any <see cref="IEnumerable{T}"/> parameter, such as <see cref="List{T}"/> or <see cref="System.Array"/>.
        /// </param>
        /// <example>
        /// <code>
        /// var eq = new EquationSystem(new Entity[] {
        ///     "x + sin(y)2 + 3",
        ///     "y - a"
        /// });
        /// </code>
        /// </example>
        public EquationSystem(IEnumerable<Entity> equations) => this.equations = equations;

        /// <summary>
        /// The equations you pass should not have an <see cref="MathS.Equality"/> node.
        /// After having created a system of equations, you may solve or latexise it.
        /// </summary>
        /// <param name="equations">
        /// An <see cref="System.Array"/>
        /// </param>
        /// <example>
        /// <code>
        /// var eq = new EquationSystem(
        ///     "x + sin(y)2 + 3",
        ///     "y - a"
        /// );
        /// </code>
        /// </example>
        public EquationSystem(params Entity[] equations) => this.equations = equations;

        /// <summary>
        /// Solves a system of equations, if possible.
        /// </summary>
        /// <param name="vars">
        /// The umber of variables must match number of equations
        /// </param>
        /// <return>
        /// If there exists at least one solution, returns a solution matrix
        /// The first axis of the matrix corresponds to the number of solutions,
        /// the second one corresponds to the number of variables.
        /// 
        /// The i-th column of the matrix represents the possible values of the i-th variable.
        /// 
        /// If no solution was found, returns a null.
        /// </return>
        public Matrix? Solve(params Variable[] vars) => EquationSolver.SolveSystem(equations, vars);

        /// <returns>
        /// Latexised version of the system. It adds
        /// zero equality to all the provided expressions
        /// </returns>
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