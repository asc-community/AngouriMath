using AngouriMath.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace AngouriMath.Functions.Algebra.AnalyticalSolving
{
    class ExponentialSolver
    {
        internal static Entity.SetNode? SolveLinear(Entity expr, Entity.Variable x)
        {
            var replacement = Entity.Variable.CreateTemp(expr.Vars);

            Func<Entity, Entity> preparator = e => e switch
            {
                Entity.Powf(var @base, var arg) when
                    TreeAnalyzer.TryGetPolyLinear(arg, x, out var a, out var b) =>
                        MathS.Pow(MathS.e, b) * MathS.Pow(MathS.Pow(MathS.e, x), MathS.Ln(@base) * a),

                _ => e,
            };

            Func<Entity, Entity> replacer = e => e switch
            {
                Entity.Powf(var @base, var arg)
                    when @base == MathS.e && arg == x 
                        => replacement,

                _ => e,
            };

            expr = expr.Replace(preparator);
            expr = expr.Replace(replacer);

            if (expr.Contains(x)) return null; // cannot be solved, not a pure exponential

            expr = expr.InnerSimplify();
            if (AnalyticalEquationSolver.Solve(expr, replacement).IsFiniteSet(out var els) && els.Any())
                return els.Select(sol => MathS.Pow(MathS.e, x).Invert(sol, x).ToSetNode()).Unite();
            else
                return null;
        }

        internal static Entity.SetNode? SolveMultiplicative(Entity expr, Entity.Variable x)
        {
            Entity? substitution = null;
            Entity ApplyPowerTransform(Entity @base, Entity arg)
            {
                var mults = Entity.Mulf.LinearChildren(arg);
                if (mults.Count() == 0) return MathS.Pow(@base, arg);

                Entity innerPower = 1;
                Entity outerPower = 1;
                foreach(var mult in mults)
                {
                    if (mult.InnerSimplify() is Entity.Number)
                        outerPower = outerPower * mult;
                    else
                        innerPower = innerPower * mult;
                }
                substitution = MathS.Pow(@base, innerPower);
                return MathS.Pow(substitution, outerPower.InnerSimplify());
            }

            Func<Entity, Entity> powerTransform = e => e switch
            {
                Entity.Powf(var @base, var arg) 
                    when @base == x && !arg.Contains(x) =>
                        ApplyPowerTransform(@base, arg),  

                _ => e,
            };

            expr = expr.Replace(powerTransform);
            if (substitution is null) return null;

            var replacement = Entity.Variable.CreateTemp(expr.Vars);
            expr = expr.Substitute(substitution, replacement);

            if (expr.Contains(x)) return null; // cannot be solved, not a multiplicative exponenial equation

            expr = expr.InnerSimplify();
            if (AnalyticalEquationSolver.Solve(expr, replacement).IsFiniteSet(out var els) && els.Any())
                return els.Select(sol => substitution.Invert(sol, x).ToSetNode()).Unite();
            else
                return null;
        }
    }
}
