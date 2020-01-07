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
        /// <returns></returns>
        public EntitySet Solve(VariableEntity x)
        {
            var res = new EntitySet();
            AnalyticalSolver.varp.Name = x.Name; // We change it each time we solve equations
            AnalyticalSolver.Solve(this, x, res);
            return res;
        }
    }
}

namespace AngouriMath.Core.TreeAnalysis
{
    public static partial class TreeAnalyzer
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
            // If there's only one `x`, we don't have to look for a possibility
            // to make replacement
            if (expr.CountOccurances(ent.ToString()) <= 1)
                return ent;
            
            // The idea is the following:
            // We must get a subtree that has more occurances than 1,
            // But at the same time it should cover all references to `ent`

            bool GoodSub(Entity sub)
            {
                return expr.CountOccurances(sub.ToString()) * sub.CountOccurances(ent.ToString()) == expr.CountOccurances(ent.ToString());
                /* if found subtree contains 3 mentions of `ent` and expression consists of 2 such substress, 
                then number of mentions of `ent` in expression should be 6*/
            }

            bool IsRelevant(Entity sub)
            {
                return expr.CountOccurances(sub.ToString()) >= 2;
            }

            /* in some time we finally get x as its occurances is > 1*/
            bool ChangeIsRelevant(int dep, out Entity sub)
            {
                return IsRelevant(sub = GetTreeByDepth(expr, ent, dep));
            }

            int depth = 0;
            Entity subtree;
            while (!ChangeIsRelevant(depth, out subtree) && GoodSub(subtree))
            {
                depth++;
            }
            if (GoodSub(subtree) && IsRelevant(subtree))
                return subtree;
            else
            {
                return ent;
            }
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
    }
}

namespace AngouriMath.Functions.Algebra.AnalyticalSolver
{
    using PatType = Entity.PatType;
    internal static class AnalyticalSolver
    {
        internal static readonly Pattern varp = new Pattern(300, PatType.VARIABLE);
        static readonly Pattern any1 = new Pattern(100, PatType.COMMON);
        static readonly Pattern any2 = new Pattern(101, PatType.COMMON);
        static readonly Pattern any3 = new Pattern(102, PatType.COMMON);
        //static readonly Pattern LinearEquation = varp
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
                    dst.AddRange(res);
            }
        }
    }
    public class EntitySet : List<Entity>
    {
        public override string ToString()
        {
            return "[" + string.Join(", ", this) + "]";
        }
        public new void Add(Entity ent)
        {
            base.Add(ent.Simplify());
        }
    }
}