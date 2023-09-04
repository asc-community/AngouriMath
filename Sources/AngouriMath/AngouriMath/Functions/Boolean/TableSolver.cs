//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Exceptions;
using System;
using static AngouriMath.Entity;

namespace AngouriMath.Functions.Boolean
{
    /// <summary>
    /// This is set of very simple algorithms
    /// It's an analogue of Newton Solver as it doesn't represent its answer
    /// symbolically
    /// Use 
    /// </summary>
    internal static class BooleanSolver
    {
        internal static bool Next(in Span<bool> states)
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
        /// <exception cref="WrongNumberOfArgumentsException"/>
        internal static Matrix? SolveTable(Entity expr, Variable[] variables)
        {
            var count = expr.Vars.Count;
            // TODO: we probably also should verify the uniqueness of the given variables
            if (count != variables.Length)
                throw new WrongNumberOfArgumentsException("Number of variables must equal number of variables in the expression");
            var states = new bool[variables.Length];
            var tb = new MatrixBuilder(count);
            var variablesStorage = new Dictionary<Variable, Entity>();
            do
            {
                for (int i = 0; i < count; i++)
                    variablesStorage[variables[i]] = states[i];
                if (expr.Substitute(variablesStorage).EvalBoolean())
                    tb.Add(states.Select(s => (Entity)s));
            }
            while (Next(states));

            return tb.ToMatrix();
        }

        internal static Matrix? BuildTruthTable(Entity expr, Variable[] variables)
        {
            var count = expr.Vars.Count;
            // TODO: we probably also should verify the uniqueness of the given variables
            if (count != variables.Length)
                throw new WrongNumberOfArgumentsException("Number of variables must equal number of variables in the expression");
            var states = new bool[variables.Length];
            var tb = new MatrixBuilder(count + 1);
            var variablesStorage = new Dictionary<Variable, Entity>();
            do
            {
                for (int i = 0; i < count; i++)
                    variablesStorage[variables[i]] = states[i];
                tb.Add(states.Select(s => (Entity)s).Append(expr.Substitute(variablesStorage).EvalBoolean()));
            }
            while (Next(states));

            return tb.ToMatrix();
        }
    }
}
