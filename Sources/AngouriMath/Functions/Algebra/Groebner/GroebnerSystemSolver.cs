//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Collections.Generic;
using System.Linq;
using AngouriMath.Core;
using static AngouriMath.Entity;

namespace AngouriMath.Functions.Algebra.Groebner
{
    /// <summary>
    /// Solves a system of polynomial equations over <c>Q</c> by triangularising it, for the
    /// systems where that can be done and answered exactly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The solver this sits in front of eliminates one variable at a time by calling
    /// <see cref="Entity.SolveEquation(Variable)"/>, which applies the closed-form radical
    /// formulas. With numeric coefficients those are cheap; with symbolic ones they are not,
    /// and since each elimination turns the next one's coefficients into nested radicals the
    /// size compounds. Four coupled variables did not finish in three hundred seconds, while
    /// four uncoupled ones with 256 solutions took seventeen milliseconds — the cost was
    /// never the size of the system, it was eliminating in radicals.
    /// </para>
    /// <para>
    /// A Gröbner basis eliminates without them: the lexicographic basis is triangular and
    /// leaves the last variable a univariate polynomial with rational coefficients, which
    /// the existing polynomial solver already handles. The basis is computed under
    /// degree-reverse-lexicographic, which is the order that can actually be computed, and
    /// converted by <see cref="Fglm"/>.
    /// </para>
    /// <para>
    /// <b>Deliberately narrow.</b> This answers only where it can check its own answer
    /// exactly: every candidate is substituted back into the original equations and kept
    /// only if they simplify to zero. Where a root is numeric that check cannot be made, and
    /// rather than accept a tuple on a tolerance — which is how a wrong root becomes a
    /// reported solution — the whole system is handed back to the existing solver. So this
    /// takes the systems with exact solutions, which is the case that was hanging, and
    /// declines the rest without changing what they did before.
    /// </para>
    /// </remarks>
    internal static class GroebnerSystemSolver
    {
        /// <summary>
        /// Answers <see langword="true"/> where the system was solved, in which case
        /// <paramref name="solutions"/> holds them, or is <see langword="null"/> where there
        /// are none. Answers <see langword="false"/> where the caller should carry on with
        /// whatever it would have done.
        /// </summary>
        internal static bool TrySolve(
            IReadOnlyList<Entity> equations, IReadOnlyList<Variable> variables, out Matrix? solutions)
        {
            solutions = null;
            if (equations.Count == 0 || variables.Count == 0)
                return false;
            if (variables.Count > MultivariatePolynomial.MaxVariables)
                return false;

            var index = new Dictionary<Variable, int>(variables.Count);
            for (var i = 0; i < variables.Count; i++)
            {
                // A repeated variable would make the column layout of the answer a lie.
                if (index.ContainsKey(variables[i]))
                    return false;
                index[variables[i]] = i;
            }

            var polynomials = new List<MultivariatePolynomial>(equations.Count);
            foreach (var equation in equations)
            {
                // Refuses anything that is not a polynomial over Q in these variables, which
                // is the guard everything below relies on.
                if (MultivariatePolynomial.TryParse(equation, index) is not { } polynomial)
                    return false;
                if (!polynomial.IsZero)
                    polynomials.Add(polynomial);
            }
            if (polynomials.Count == 0)
                return false;

            var budget = new GroebnerBudget();
            var basis = Buchberger.Compute(polynomials, MonomialOrder.DegreeReverseLexicographic, budget);
            if (basis is null)
                return false;

            // The textbook signal for an inconsistent system: the ideal is everything, so a
            // nonzero constant is in it. Nothing satisfies the equations.
            foreach (var element in basis)
                if (element.IsConstant && !element.IsZero)
                {
                    solutions = null;
                    return true;
                }

            var lexicographic = Fglm.ToLexicographic(basis, variables.Count, budget);
            if (lexicographic is null)
                return false;

            var triangular = new List<Entity>(lexicographic.Count);
            foreach (var element in lexicographic)
                triangular.Add(element.ToEntity(variables));

            var found = new List<Entity[]>();
            var assignment = new Entity[variables.Count];
            if (!BackSubstitute(triangular, variables, variables.Count - 1, assignment, found))
                return false;

            foreach (var candidate in found)
                if (!Satisfies(equations, variables, candidate))
                    return false;

            if (found.Count == 0)
            {
                solutions = null;
                return true;
            }

            var builder = new MatrixBuilder(variables.Count);
            foreach (var candidate in found)
                builder.Add(candidate);
            solutions = builder.ToMatrix();
            return true;
        }

        /// <summary>
        /// Walks the triangular system from the last variable back. Answers
        /// <see langword="false"/> where the shape it needs is not there — a variable with no
        /// equation of its own means the system does not have finitely many solutions in the
        /// way this can enumerate.
        /// </summary>
        static bool BackSubstitute(
            IReadOnlyList<Entity> equations, IReadOnlyList<Variable> variables,
            int at, Entity[] assignment, List<Entity[]> found)
        {
            if (at < 0)
            {
                found.Add((Entity[])assignment.Clone());
                return true;
            }

            var variable = variables[at];
            Entity? univariate = null;
            foreach (var equation in equations)
            {
                var free = equation.Vars.ToList();
                if (free.Count == 1 && free[0] == variable)
                {
                    univariate = equation;
                    break;
                }
            }
            if (univariate is null)
                return false;

            if (univariate.SolveEquation(variable).InnerSimplified is not Set.FiniteSet roots)
                return false;

            foreach (var root in roots)
            {
                // Only rational roots are taken, and the reason is the check at the end.
                // Substituting a radical or a decimal back and asking whether the equations
                // come out zero means simplifying nested radicals, which is unbounded work
                // that then usually fails to prove anything -- a degree-nine univariate had
                // this spending longer on the verification than the old solver takes on the
                // whole system. With rationals the check is exact arithmetic and immediate.
                // So: this takes the systems whose solutions are rational, which is the case
                // that was hanging, and leaves the rest to the solver that already has them.
                if (root is not Number.Rational)
                    return false;

                assignment[at] = root;
                var narrowed = new List<Entity>(equations.Count);
                foreach (var equation in equations)
                {
                    var substituted = equation.Substitute(variable, root).InnerSimplified;
                    if (substituted.Vars.Any())
                        narrowed.Add(substituted);
                }
                if (!BackSubstitute(narrowed, variables, at - 1, assignment, found))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Substitutes a candidate into the original equations and insists they come out
        /// exactly zero.
        /// </summary>
        /// <remarks>
        /// Needed because a triangular basis can hand back a tuple that satisfies the
        /// triangle without satisfying the system it came from, where the ideal is not in
        /// shape position. Every coordinate is rational by the time this runs, so the whole
        /// thing is exact rational arithmetic — no tolerance is involved, and none would be
        /// accepted: a tolerance is what turns a root that is merely close into one that is
        /// reported.
        /// </remarks>
        static bool Satisfies(
            IReadOnlyList<Entity> equations, IReadOnlyList<Variable> variables, Entity[] candidate)
        {
            var substitutions = new Dictionary<Variable, Entity>(variables.Count);
            for (var i = 0; i < variables.Count; i++)
                substitutions[variables[i]] = candidate[i];

            foreach (var equation in equations)
                if (equation.Substitute(substitutions).Evaled is not Number.Rational { IsZero: true })
                    return false;
            return true;
        }
    }
}
