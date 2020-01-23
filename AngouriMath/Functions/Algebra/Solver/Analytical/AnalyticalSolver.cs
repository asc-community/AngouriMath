using AngouriMath.Core.Exceptions;
using AngouriMath.Core.TreeAnalysis;
using AngouriMath.Functions.Algebra.AnalyticalSolver;
using System;
using System.Collections.Generic;
using System.Linq;
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
                if (subtree.Children.Count == 0) return subtree;

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
        public static EntitySet FindInvertExpression(Entity func, Entity value, Entity x)
        {
            if (func == x)
                return new EntitySet(value);
            if (func.entType == Entity.EntType.NUMBER)
                throw new MathSException("This function must contain x");
            if (func.entType == Entity.EntType.VARIABLE)
                return new EntitySet(func);
            if (func.entType == Entity.EntType.OPERATOR)
                return InvertOperatorEntity(func as OperatorEntity, value, x);
            if (func.entType == Entity.EntType.FUNCTION)
                return InvertFunctionEntity(func as FunctionEntity, value, x);

            return new EntitySet(value);
        }

        public static EntitySet InvertOperatorEntity(OperatorEntity func, Entity value, Entity x)
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
                    return FindInvertExpression(un, value - a, x);
                case "minusf":
                    if (arg == 0)
                        // x - a = value => x = value + a
                        return FindInvertExpression(un, value + a, x);
                    else
                        // a - x = value => x = a - value
                        return FindInvertExpression(un, a - value, x);
                case "mulf":
                    // x * a = value => x = value / a
                    return FindInvertExpression(un, value / a, x);
                case "divf":
                    if (arg == 0)
                        // x / a = value => x = a * value
                        return FindInvertExpression(un, value * a, x);
                    else
                        // a / x = value => x = a / value
                        return FindInvertExpression(un, a / value, x);
                case "powf":
                    if (arg == 0)
                    {
                        // x ^ a = value => x = value ^ (1/a)
                        if (value.entType == Entity.EntType.NUMBER && a.entType == Entity.EntType.NUMBER && a.GetValue().IsInteger())
                        {
                            var res = new EntitySet();
                            foreach (var root in Number.GetAllRoots(value.GetValue(), (int)(a.GetValue().Re)))
                                res.AddRange(FindInvertExpression(un, root, x));
                            return res;
                        }
                        else
                            return FindInvertExpression(un, MathS.Pow(value, 1 / a), x);
                    }
                    else
                        // a ^ x = value => x = log(value, a)
                        return FindInvertExpression(un, MathS.Log(value, a), x);
                default:
                    throw new SysException("Unknown operator");
            }
        }

        public static EntitySet InvertFunctionEntity(FunctionEntity func, Entity value, Entity x)
        {
            Entity a = func.Children[0];
            Entity b = func.Children.Count == 2 ? func.Children[1] : null;
            int arg = func.Children.Count == 2 && func.Children[1].FindSubtree(x) != null ? 1 : 0;
            var n = MathS.Var("n");
            var res = new EntitySet();
            var pi = MathS.pi;

            EntitySet GetNotNullEntites(EntitySet set)
            {
                return new EntitySet(set.Where(el => el.entType != Entity.EntType.NUMBER || !el.GetValue().IsNull));
            }

            switch (func.Name)
            {
                // Consider case when sin(sin(x)) where double-mention of n occures
                case "sinf":
                    {
                        var inverted = FindInvertExpression(a, MathS.Arcsin(value), x);
                        // sin(x) = value => x = arcsin(value) + 2pi * n
                        res.AddRange(GetNotNullEntites(inverted) + 2 * pi * n);
                        // sin(x) = value => x = pi - arcsin(value) + 2pi * n
                        res.AddRange(pi - GetNotNullEntites(inverted) + 2 * pi * n);
                        return res;
                    }
                case "cosf":
                    {
                        var inverted = FindInvertExpression(a, MathS.Arccos(value), x);
                        // cos(x) = value => x = arccos(value) + 2pi * n
                        res.AddRange(GetNotNullEntites(inverted) + 2 * pi * n);
                        // cos(x) = value => x = -arccos(value) + 2pi * n
                        res.AddRange(-1 * GetNotNullEntites(inverted) + 2 * pi * n);
                        return res;
                    }
                case "tanf":
                    {
                        var inverted = FindInvertExpression(a, MathS.Arctan(value), x);
                        // tan(x) = value => x = arctan(value) + pi * n
                        res.AddRange(GetNotNullEntites(inverted) + pi * n);
                        return res;
                    }
                case "cotanf":
                    {
                        var inverted = FindInvertExpression(a, MathS.Arccotan(value), x);
                        // cotan(x) = value => x = arccotan(value)
                        res.AddRange(GetNotNullEntites(inverted) + pi * n);
                        return res;
                    }
                case "arcsinf":
                    // arcsin(x) = value => x = sin(value)
                    return GetNotNullEntites(FindInvertExpression(a, MathS.Sin(value), x));
                case "arccosf":
                    // arccos(x) = value => x = cos(value)
                    return GetNotNullEntites(FindInvertExpression(a, MathS.Cos(value), x));
                case "arctanf":
                    // arctan(x) = value => x = tan(value)
                    return GetNotNullEntites(FindInvertExpression(a, MathS.Tan(value), x));
                case "arccotanf":
                    // arccotan(x) = value => x = cotan(value)
                    return GetNotNullEntites(FindInvertExpression(a, MathS.Cotan(value), x));
                case "logf":
                    if (arg == 0)
                        // log(x, a) = value => x = a ^ value
                        return GetNotNullEntites(FindInvertExpression(a, MathS.Pow(b, value), x));
                    else
                        // log(a, x) = value => a = x ^ value => x = a ^ (1 / value)
                        return GetNotNullEntites(FindInvertExpression(a, 1 / MathS.Pow(b, value), x));
                default:
                    throw new SysException("Unknown function");
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
            if (expr.entType == Entity.EntType.OPERATOR)
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
                    case "powf":
                        Solve(expr.Children[0], x, dst);
                        return;
                }
            }
            else if (expr.entType == Entity.EntType.FUNCTION)
            {
                switch (expr.Name)
                {
                    case "sinf":
                        Solve(expr.Children[0] + "n" * MathS.pi, x, dst);
                        return;
                    case "cosf":
                        Solve(expr.Children[0] + MathS.pi / 2 + "n" * MathS.pi, x, dst);
                        return;
                    case "tanf":
                        Solve(expr.Children[0] + "n" * MathS.pi, x, dst);
                        return;
                    case "cotanf":
                        Solve(expr.Children[0] + MathS.pi / 2 + "n" * MathS.pi, x, dst);
                        return;
                    case "arcsinf":
                        Solve(expr.Children[0], x, dst);
                        return;
                    case "arccosf":
                        Solve(expr.Children[0] - 1, x, dst);
                        return;
                    case "arctanf":
                        Solve(expr.Children[0], x, dst);
                        return;
                    case "logf":
                        // x ^ n
                        if (expr.Children[0].FindSubtree(x) != null && expr.Children[1].FindSubtree(x) == null)
                        {
                            Solve(expr.Children[0], x, dst);
                            return;
                        }
                        break;
                }
            }
            
            if (expr == x)
            {
                dst.Add(0);
            } else
            {
                Entity actualVar = TreeAnalyzer.GetMinimumSubtree(expr.Simplify(), x);
                var res = PolynomialSolver.SolveAsPolynomial(expr, actualVar);

                if (res != null)
                {
                    if (actualVar != x) // if we found a variable replacement
                    {
                        if (actualVar.CountOccurances(x.ToString()) == 1)
                        {
                            foreach (var r in res)
                                dst.AddRange(TreeAnalyzer.FindInvertExpression(actualVar, r, x));
                        }
                    }
                    else
                        dst.AddRange(res);
                }
                else
                {
                    EntitySet vars = new EntitySet();
                    TreeAnalyzer.GetUniqueVariables(expr.SubstituteConstants() /* otherwise it will count `pi`, `e` as variables */, vars);
                    if (vars.Count == 1)
                        dst.Merge(expr.SolveNt(x));
                }
            }
        }
    }
    
}