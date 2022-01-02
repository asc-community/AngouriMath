//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Multithreading;
using AngouriMath.Extensions;
using System;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Set;

namespace AngouriMath.Functions.Algebra.AnalyticalSolving
{
    internal static class ExponentialSolver
    {
        internal static Set? SolveLinear(Entity expr, Entity.Variable x)
        {
            var replacement = Variable.CreateTemp(expr.Vars);

            Func<Entity, Entity> preparator = e => e switch
            {
                Powf(var @base, var arg) when
                    TreeAnalyzer.TryGetPolyLinear(arg, x, out var a, out var b) =>
                        MathS.Pow(@base, b) * MathS.Pow(MathS.Pow(MathS.e, x), MathS.Ln(@base) * a),

                _ => e,
            };

            Func<Entity, Entity> replacer = e => e switch
            {
                Powf(var @base, var arg)
                    when @base == MathS.e && arg == x
                        => replacement,

                _ => e,
            };

            expr = expr.Replace(preparator);
            expr = expr.Replace(replacer);

            if (expr.ContainsNode(x)) return null; // cannot be solved, not a pure exponential

            expr = expr.InnerSimplified;
            if (AnalyticalEquationSolver.Solve(expr, replacement) is FiniteSet els && els.Any())
                return (Set)els.Select(sol => MathS.Pow(MathS.e, x).Invert(sol, x).ToSet()).Unite().InnerSimplified;
            else
                return null;
        }

        internal static Entity GetConstantOutOfLogarithm(Entity expr)
            => expr switch
            {
                Logf(var anyBase1, Powf(var anyBase2, Integer cst))
                    => cst * new Logf(anyBase1, anyBase2),
                _ => expr
            };

        internal static Set? SolveMultiplicative(Entity expr, Variable x)
        {
            Entity? substitution = null;
            var innerPowerList = new List<Entity>();
            var outerPowerList = new List<Entity>();
            Entity ApplyPowerTransform(Entity @base, Entity arg)
            {
                MultithreadingFunctional.ExitIfCancelled();

                arg = arg.Replace(GetConstantOutOfLogarithm);
                var mults = Entity.Mulf.LinearChildren(arg);
                if (!mults.Any()) return MathS.Pow(@base, arg);

                Entity innerPower = 1;
                Entity outerPower = 1;
                foreach (var mult in mults)
                {
                    if (mult.EvaluableNumerical)
                        outerPower *= mult;
                    else
                        innerPower *= mult;
                }
                substitution = innerPower == 1 ? @base : MathS.Pow(@base, innerPower);
                if(innerPower != 1) innerPowerList.Add(innerPower);
                if(outerPower != 1) outerPowerList.Add(outerPower);
                return MathS.Pow(substitution, outerPower.InnerSimplified);
            }

            Func<Entity, Entity> powerTransform = e => e switch
            {
                Powf(var @base, var arg)
                    when @base == x && !arg.ContainsNode(x) =>
                        ApplyPowerTransform(@base, arg),

                _ => e,
            };

            expr = expr.Replace(powerTransform);
            if (substitution is null) return null;

            var replacement = Variable.CreateTemp(expr.Vars);
            
            if(innerPowerList.Count == 0) 
                (innerPowerList, outerPowerList) = (outerPowerList, innerPowerList);

            // handle special case when all bases are numerical
            if (innerPowerList.All(e => e.EvaluableNumerical && e.Evaled is Real))
            {
                var minPow = innerPowerList.Aggregate((a, b)
                    => (a.EvalNumerical() < b.EvalNumerical()).EvalBoolean() ? a : b);

                substitution = MathS.Pow(x, minPow).InnerSimplified;

                foreach (var pow in innerPowerList)
                {
                    var divided = (pow / minPow).InnerSimplified;
                    expr = expr.Substitute(MathS.Pow(x, pow.InnerSimplified), MathS.Pow(substitution, divided));
                }

                MultithreadingFunctional.ExitIfCancelled();
            }
            expr = expr.Substitute(substitution, replacement);
            if (expr.ContainsNode(x)) return null; // cannot be solved, not a multiplicative exponenial equation

            expr = expr.InnerSimplified;
            if (AnalyticalEquationSolver.Solve(expr, replacement) is FiniteSet els && els.Any())
                return (Set)els.Select(sol => substitution.Invert(sol, x).ToSet()).Unite().InnerSimplified;
            else
                return null;
        }
    }
}
