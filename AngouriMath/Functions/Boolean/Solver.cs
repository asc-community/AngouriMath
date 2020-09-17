using AngouriMath;
using AngouriMath.Core;
using System;
using System.Linq;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Boolean;

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
