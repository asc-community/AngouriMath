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
        public EntityList Solve(VariableEntity x)
        {
            var res = new EntityList();
            AnalyticalSolver.varp.Name = x.Name; // We change it each time we solve equations
            AnalyticalSolver.Solve(this.Sort(Core.TreeAnalysis.TreeAnalyzer.SortLevel.LOW_LEVEL).SimplifyIntelli().Expand(), x, res);
            return res;
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
        internal static void Solve(Entity expr, VariableEntity x, EntityList dst)
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
                
            }
        }
    }
    public class EntityList : List<Entity>
    {

    }
}