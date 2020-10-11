using AngouriMath.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using static AngouriMath.Entity.Set;

namespace AngouriMath.Functions.Algebra.AnalyticalSolving
{
    class ExponentialSolver
    {
        internal static Entity.Set? SolveLinear(Entity expr, Entity.Variable x)
        {
            var replacement = Entity.Variable.CreateTemp(expr.Vars);

            Func<Entity, Entity> preparator = e => e switch
            {
                Entity.Powf(var @base, var arg) when
                    TreeAnalyzer.TryGetPolyLinear(arg, x, out var a, out var b) =>
                        MathS.Pow(@base, b) * MathS.Pow(MathS.Pow(MathS.e, x), MathS.Ln(@base) * a),

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

            if (expr.ContainsNode(x)) return null; // cannot be solved, not a pure exponential

            expr = expr.InnerSimplified;
            if (AnalyticalEquationSolver.Solve(expr, replacement) is FiniteSet els && els.Any())
                return (Entity.Set)els.Select(sol => MathS.Pow(MathS.e, x).Invert(sol, x).ToSet()).Unite().InnerSimplified;
            else
                return null;
        }

        internal static Entity.Set? SolveMultiplicative(Entity expr, Entity.Variable x)
        {
            Entity? substitution = null;
            var powers = new List<Entity>();
            Entity ApplyPowerTransform(Entity @base, Entity arg)
            {
                var mults = Entity.Mulf.LinearChildren(arg);
                if (mults.Count() == 0) return MathS.Pow(@base, arg);

                Entity innerPower = 1;
                Entity outerPower = 1;
                foreach (var mult in mults)
                {
                    if (mult.InnerSimplified is Entity.Number)
                        outerPower = outerPower * mult;
                    else
                        innerPower = innerPower * mult;
                }
                substitution = MathS.Pow(@base, innerPower);
                powers.Add(innerPower);
                return MathS.Pow(substitution, outerPower.InnerSimplified);
            }

            Func<Entity, Entity> powerTransform = e => e switch
            {
                Entity.Powf(var @base, var arg)
                    when @base == x && !arg.ContainsNode(x) =>
                        ApplyPowerTransform(@base, arg),

                _ => e,
            };

            expr = expr.Replace(powerTransform);
            if (substitution is null) return null;

            var replacement = Entity.Variable.CreateTemp(expr.Vars);

            if (powers.All(e => e.EvaluableNumerical))
            {
                var minPow = powers.Aggregate((a, b)
                    => (a.EvalNumerical() < b.EvalNumerical()).EvalBoolean() ? a : b);

                foreach (var pow in powers)
                {
                    var divided = (pow / minPow).InnerSimplified;
                    expr = expr.Substitute(MathS.Pow(x, pow), MathS.Pow(MathS.Pow(x, minPow), divided));
                }
            }

            expr = expr.Substitute(substitution, replacement);
            if (expr.ContainsNode(x)) return null; // cannot be solved, not a multiplicative exponenial equation

            expr = expr.InnerSimplified;
            if (AnalyticalEquationSolver.Solve(expr, replacement) is FiniteSet els && els.Any())
                return (Entity.Set)els.Select(sol => substitution.Invert(sol, x).ToSet()).Unite().InnerSimplified;
            else
                return null;
        }
    }
}
