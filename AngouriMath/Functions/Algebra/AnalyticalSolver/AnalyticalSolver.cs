using AngouriMath.Core.Exceptions;
using AngouriMath.Core.TreeAnalysis;
using AngouriMath.Functions.Algebra.AnalyticalSolver;
using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath
{
    public abstract partial class Entity
    {
        /// <summary>
        /// Attempt to find analytical roots of a custom equation
        /// </summary>
        /// <param name="x"></param>
        /// <returns>
        /// Returns EntitySet. Work with it as with a list
        /// </returns>
        public EntitySet Solve(VariableEntity x)
        {
            var res = new EntitySet();
            AnalyticalSolver.Solve(this, x, res);
            return res;
        }
    }
}

namespace AngouriMath.Core.TreeAnalysis
{
    internal static partial class TreeAnalyzer
    {
        /// <summary>
        /// Searches for a subtree containing `ent` and being minimal possible size.
        /// For example, for expr = MathS.Sqr(x) + 2 * (MathS.Sqr(x) + 3) the result
        /// will be MathS.Sqr(x) while for MathS.Sqr(x) + x the minimum subtree is x.
        /// Further, it will be used for solving with variable replacing, for example,
        /// there's no pattern for solving equation like sin(x)^2 + sin(x) + 1 = 0,
        /// but we can first solve t^2 + t + 1 = 0, and then root = sin(x).
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        public static Entity GetMinimumSubtree(Entity expr, Entity ent)
        {
            // TODO: this function requires a lot of refactoring

            /*
            // If there's only one `x`, we don't have to look for a possibility
            // to make replacement
            if (expr.CountOccurances(ent.ToString()) <= 1)
                return ent;
                */
            
            // The idea is the following:
            // We must get a subtree that has more occurances than 1,
            // But at the same time it should cover all references to `ent`

            bool GoodSub(Entity sub)
            {
                return expr.CountOccurances(sub.ToString()) * sub.CountOccurances(ent.ToString()) == expr.CountOccurances(ent.ToString());
                /* if found subtree contains 3 mentions of `ent` and expression consists of 2 such substress, 
                then number of mentions of `ent` in expression should be 6*/
            }

            int depth = 1;
            Entity subtree;
            Entity best = null;
            int ocs = 0;
            while ((subtree = GetTreeByDepth(expr, ent, depth)) != ent)
            {
                depth++;
                int newocs;
                if (GoodSub(subtree) && (newocs = expr.CountOccurances(subtree.ToString())) > ocs)
                {
                    // we're looking for good subs with maximum number of occurances
                    // in order to minimize number of occurances of x in this sub
                    ocs = newocs;
                    best = subtree;
                }
            }
            return best == null || ocs == 1 ? ent : best;
        }

        private static Entity GetTreeByDepth(Entity expr, Entity ent, int depth)
        {
            while(depth > 0)
            {
                foreach (var child in expr.Children)
                    // We don't care about the order as once we encounter mention of `ent`,
                    // we need ALL subtrees be equal
                    if (child.FindSubtree(ent) != null)
                    {
                        expr = child;
                        break;
                    }
                depth -= 1;
                if (expr == ent)
                    return expr;
            }
            return expr;
        }

        /// <summary>
        /// Func MUST contain exactly ONE occurance of x,
        /// otherwise it won't work correctly
        /// </summary>
        /// <param name="func"></param>
        /// <param name="value"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static Entity ResolveInvertFunction(Entity func, Entity value, VariableEntity x)
        {
            if (func == x)
                return value;
            if (func is NumberEntity)
                throw new MathSException("This function must contain x");
            if (func is VariableEntity)
                return func;
            if (func is OperatorEntity)
                return InvertOperator(func as OperatorEntity, value, x);
            if (func is FunctionEntity)
                return InvertFunction(func as FunctionEntity, value, x);

            return value;
        }

        public static Entity InvertOperator(OperatorEntity func, Entity value, VariableEntity x)
        {
            Entity a, un;
            int arg;
            if (func.Children[0].FindSubtree(x) != null)
            {
                a = func.Children[1];
                un = func.Children[0];
                arg = 0;
            }
            else
            {
                a = func.Children[0];
                un = func.Children[1];
                arg = 1;
            }
            switch (func.Name)
            {
                case "sumf":
                    // x + a = value => x = value - a
                    return ResolveInvertFunction(un, value - a, x);
                case "minusf":
                    if (arg == 0)
                        // x - a = value => x = value + a
                        return ResolveInvertFunction(un, value + a, x);
                    else
                        // a - x = value => x = a - value
                        return ResolveInvertFunction(un, a - value, x);
                case "mulf":
                    // x * a = value => x = value / a
                    return ResolveInvertFunction(un, value / a, x);
                case "divf":
                    if (arg == 0)
                        // x / a = value => x = a * value
                        return ResolveInvertFunction(un, value * a, x);
                    else
                        // a / x = value => x = a / value
                        return ResolveInvertFunction(un, a / value, x);
                case "powf":
                    if (arg == 0)
                        // x ^ a = value => x = value ^ (1/a)
                        return ResolveInvertFunction(un, MathS.Pow(value, 1 / a), x);
                    else
                        // a ^ x = value => x = log(value, a)
                        return ResolveInvertFunction(un, MathS.Log(value, a), x);
                default:
                    throw new SysException("Unknown operator");
            }
        }

        public static Entity InvertFunction(FunctionEntity func, Entity value, VariableEntity x)
        {
            Entity a = func.Children[0];
            Entity b = func.Children.Count == 2 ? func.Children[1] : null;
            int arg = func.Children.Count == 2 && func.Children[1].FindSubtree(x) != null ? 1 : 0;
            switch (func.Name)
            {
                case "sinf":
                    // sin(x) = value => x = arcsin(value)
                    return ResolveInvertFunction(a, MathS.Arcsin(value), x);
                case "cosf":
                    // cos(x) = value => x = arccos(value)
                    return ResolveInvertFunction(a, MathS.Arccos(value), x);
                case "tanf":
                    // tan(x) = value => x = arctan(value)
                    return ResolveInvertFunction(a, MathS.Arctan(value), x);
                case "cotanf":
                    // cotan(x) = value => x = arccotan(value)
                    return ResolveInvertFunction(a, MathS.Arccotan(value), x);
                case "arcsinf":
                    // arcsin(x) = value => x = sin(value)
                    return ResolveInvertFunction(a, MathS.Sin(value), x);
                case "arccosf":
                    // arccos(x) = value => x = cos(value)
                    return ResolveInvertFunction(a, MathS.Cos(value), x);
                case "arctanf":
                    // arctan(x) = value => x = tan(value)
                    return ResolveInvertFunction(a, MathS.Tan(value), x);
                case "arccotanf":
                    // arccotan(x) = value => x = cotan(value)
                    return ResolveInvertFunction(a, MathS.Cotan(value), x);
                case "logf":
                    if (arg == 0)
                        // log(x, a) = value => x = a ^ value
                        return ResolveInvertFunction(a, MathS.Pow(b, value), x);
                    else
                        // log(a, x) = value => a = x ^ value => x = a ^ (1 / value)
                        return ResolveInvertFunction(a, 1 / MathS.Pow(b, value), x);
                default:
                    throw new SysException("Uknown function");
            }
        }
    }
}

namespace AngouriMath.Functions.Algebra.AnalyticalSolver
{
    using PatType = Entity.PatType;
    internal static class AnalyticalSolver
    {
        internal static void Solve(Entity expr, VariableEntity x, EntitySet dst)
        {
            if (expr is OperatorEntity)
            {
                switch (expr.Name)
                {
                    case "mulf":
                        Solve(expr.Children[0], x, dst);
                        Solve(expr.Children[1], x, dst);
                        return;
                    case "divf":
                        Solve(expr.Children[0], x, dst);
                        return;
                }
            }
            else if (expr is FunctionEntity)
            {
                switch (expr.Name)
                {
                    case "sinf":
                        // For now, we only consider cases 
                        // sin(0) = 0, sin(pi) = 0, sin(-pi) = 0
                        Solve(expr.Children[0], x, dst);
                        Solve(expr.Children[0] + MathS.pi, x, dst);
                        Solve(expr.Children[0] - MathS.pi, x, dst);
                        return;
                    case "cosf":
                        // For now, we only consider cases
                        // cos(pi/2) = 0. cos(-pi/2) = 0
                        Solve(expr.Children[0] + MathS.pi / 2, x, dst);
                        Solve(expr.Children[0] - MathS.pi / 2, x, dst);
                        return;
                }
            }
            
            if (expr == x)
            {
                dst.Add(0);
                return;
            } else
            {
                Entity actualVar = TreeAnalyzer.GetMinimumSubtree(expr, x).Simplify();
                var res = PolynomialSolver.SolveAsPolynomial(expr, actualVar);

                if (res != null)
                {
                    if (actualVar != x) // if we found a variable replacement
                    {
                        if (actualVar.CountOccurances(x.ToString()) == 1)
                        {
                            foreach (var r in res)
                                dst.Add(TreeAnalyzer.ResolveInvertFunction(actualVar, r, x).SimplifyIntelli());
                        }
                    }
                    else
                        dst.AddRange(res);
                }
                else
                {
                    EntitySet vars = new EntitySet();
                    TreeAnalyzer.GetUniqueVariables(expr, vars);
                    if (vars.Count == 1)
                        dst.Merge(expr.SolveNt(x));
                }
                    
            }
        }
    }
    
}