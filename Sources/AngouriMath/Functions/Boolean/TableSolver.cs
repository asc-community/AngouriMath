//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Exceptions;
using AngouriMath.Core.Multithreading;
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

        /// <summary>What a partial assignment already settles about a subexpression.</summary>
        private enum Verdict { False, True, Unknown }

        private const int Unassigned = -1;

        /// <summary>
        /// Returns a tensor of solutions over <paramref name="variables"/> so that
        /// the expression turns into a True when evaled.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Assigns the variables one at a time and asks after each what the expression
        /// already is. A prefix that makes it false rules out every completion of itself at
        /// once, and a prefix that makes it true admits all of them, so neither has to be
        /// walked. Only a prefix that settles nothing is branched on. Enumerating all
        /// <c>2^n</c> rows and testing each — which is what this did — is the case where no
        /// prefix ever settles anything, and is now the worst case rather than the only one.
        /// </para>
        /// <para>
        /// The order of the rows is unchanged: assigning false before true, with the last
        /// variable moving fastest, is the same order counting through the table produced.
        /// </para>
        /// <para>
        /// This enumerates models, not satisfiability, so the result can still be
        /// exponentially large — a tautology over n variables has 2^n solutions and they all
        /// have to be written down. What is gone is paying that price for the search when the
        /// answer is small.
        /// </para>
        /// </remarks>
        /// <exception cref="WrongNumberOfArgumentsException"/>
        internal static Matrix? SolveTable(Entity expr, Variable[] variables)
        {
            var count = expr.Vars.Count;
            // TODO: we probably also should verify the uniqueness of the given variables
            if (count != variables.Length)
                throw new WrongNumberOfArgumentsException("Number of variables must equal number of variables in the expression");

            var index = new Dictionary<Variable, int>(count);
            for (var i = 0; i < variables.Length; i++)
                index[variables[i]] = i;

            var assignment = new int[count];
            for (var i = 0; i < count; i++)
                assignment[i] = Unassigned;

            var tb = new MatrixBuilder(count);
            Search(expr, variables, index, assignment, 0, tb);
            return tb.ToMatrix();
        }

        static void Search(Entity expr, Variable[] variables, Dictionary<Variable, int> index,
                           int[] assignment, int depth, MatrixBuilder tb)
        {
            MultithreadingFunctional.ExitIfCancelled();
            switch (Evaluate(expr, index, assignment))
            {
                case Verdict.False:
                    return;
                case Verdict.True:
                    EmitEveryCompletion(assignment, depth, tb);
                    return;
            }

            if (depth == assignment.Length)
            {
                // Everything is assigned and the three-valued reading still cannot say. That
                // is a node shape it does not know rather than a real undecidability, so fall
                // back to substituting and evaluating for real.
                if (Concretely(expr, variables, assignment))
                    EmitEveryCompletion(assignment, depth, tb);
                return;
            }

            assignment[depth] = 0;
            Search(expr, variables, index, assignment, depth + 1, tb);
            assignment[depth] = 1;
            Search(expr, variables, index, assignment, depth + 1, tb);
            assignment[depth] = Unassigned;
        }

        /// <summary>
        /// Writes out every way of filling in the variables from <paramref name="depth"/> on,
        /// in counting order. Called where the expression is already true whatever they are.
        /// </summary>
        /// <remarks>
        /// This is where the cost of the method's shape lands. The search is cheap, but every
        /// model has to be written down, and a formula that most assignments satisfy has a
        /// great many: a tautology over 22 variables is four million rows and 2.4 GB. So this
        /// is the loop that most needs to be interruptible — the caller cannot know in advance
        /// that the answer will not fit.
        /// </remarks>
        static void EmitEveryCompletion(int[] assignment, int depth, MatrixBuilder tb)
        {
            var free = assignment.Length - depth;
            var total = 1L << free;
            for (long combination = 0; combination < total; combination++)
            {
                MultithreadingFunctional.ExitIfCancelled();
                var row = new Entity[assignment.Length];
                for (var i = 0; i < depth; i++)
                    row[i] = assignment[i] == 1;
                for (var j = 0; j < free; j++)
                    row[depth + j] = ((combination >> (free - 1 - j)) & 1) == 1;
                tb.Add(row);
            }
        }

        static bool Concretely(Entity expr, Variable[] variables, int[] assignment)
        {
            var storage = new Dictionary<Variable, Entity>(variables.Length);
            for (var i = 0; i < variables.Length; i++)
                storage[variables[i]] = assignment[i] == 1;
            return expr.Substitute(storage).EvalBoolean();
        }

        /// <summary>
        /// Reads the expression under a partial assignment. Anything it does not recognise is
        /// <see cref="Verdict.Unknown"/>, which costs pruning and never costs correctness --
        /// the caller falls back to a real evaluation once everything is assigned.
        /// </summary>
        static Verdict Evaluate(Entity expr, Dictionary<Variable, int> index, int[] assignment)
        {
            switch (expr)
            {
                case Entity.Boolean b:
                    return b.Value ? Verdict.True : Verdict.False;

                case Variable v:
                    if (!index.TryGetValue(v, out var i))
                        return Verdict.Unknown;
                    return assignment[i] switch
                    {
                        0 => Verdict.False,
                        1 => Verdict.True,
                        _ => Verdict.Unknown
                    };

                case Notf not:
                    return Evaluate(not.Argument, index, assignment) switch
                    {
                        Verdict.True => Verdict.False,
                        Verdict.False => Verdict.True,
                        _ => Verdict.Unknown
                    };

                case Andf and:
                {
                    // One false settles it without reading the other side.
                    var left = Evaluate(and.Left, index, assignment);
                    if (left is Verdict.False) return Verdict.False;
                    var right = Evaluate(and.Right, index, assignment);
                    if (right is Verdict.False) return Verdict.False;
                    return left is Verdict.True && right is Verdict.True ? Verdict.True : Verdict.Unknown;
                }

                case Orf or:
                {
                    var left = Evaluate(or.Left, index, assignment);
                    if (left is Verdict.True) return Verdict.True;
                    var right = Evaluate(or.Right, index, assignment);
                    if (right is Verdict.True) return Verdict.True;
                    return left is Verdict.False && right is Verdict.False ? Verdict.False : Verdict.Unknown;
                }

                case Xorf xor:
                {
                    // Neither side alone settles an xor.
                    var left = Evaluate(xor.Left, index, assignment);
                    if (left is Verdict.Unknown) return Verdict.Unknown;
                    var right = Evaluate(xor.Right, index, assignment);
                    if (right is Verdict.Unknown) return Verdict.Unknown;
                    return left == right ? Verdict.False : Verdict.True;
                }

                case Impliesf implies:
                {
                    var assumption = Evaluate(implies.Assumption, index, assignment);
                    if (assumption is Verdict.False) return Verdict.True;
                    var conclusion = Evaluate(implies.Conclusion, index, assignment);
                    if (conclusion is Verdict.True) return Verdict.True;
                    return assumption is Verdict.True && conclusion is Verdict.False
                        ? Verdict.False
                        : Verdict.Unknown;
                }

                case Equalsf equals:
                {
                    // Only where both sides read as booleans; a comparison of anything else
                    // leaves its operands Unknown and falls out here as Unknown too.
                    var left = Evaluate(equals.Left, index, assignment);
                    if (left is Verdict.Unknown) return Verdict.Unknown;
                    var right = Evaluate(equals.Right, index, assignment);
                    if (right is Verdict.Unknown) return Verdict.Unknown;
                    return left == right ? Verdict.True : Verdict.False;
                }

                default:
                    return Verdict.Unknown;
            }
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
                // A truth table is all 2^n rows by definition, so there is no pruning to be
                // had here and the only mercy available is being able to stop.
                MultithreadingFunctional.ExitIfCancelled();
                for (int i = 0; i < count; i++)
                    variablesStorage[variables[i]] = states[i];
                tb.Add(states.Select(s => (Entity)s).Append(expr.Substitute(variablesStorage).EvalBoolean()));
            }
            while (Next(states));

            return tb.ToMatrix();
        }
    }
}
