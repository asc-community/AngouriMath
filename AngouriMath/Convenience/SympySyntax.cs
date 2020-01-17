using AngouriMath.Core.TreeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Convenience
{
    /// <summary>
    /// Sympy - is a super-powerful library for algebra cumputation,
    /// multi-variable expressions, physics, matrices, etc.
    /// But it is only available for python. If you need the same syntax for C#,
    /// this module is for you.
    /// </summary>
    public static class SySyn
    {
        /// Here sympy-like syntax implementation goes

        /// <summary>
        /// Differentiation
        /// </summary>
        /// <param name="expr">
        /// Expression to differentiate
        /// </param>
        /// <param name="vars">
        /// Variable to differentiate over. If you need more than first derivative,
        /// specify as many variables as you need
        /// </param>
        /// <returns></returns>
        public static Entity Diff(Entity expr, params VariableEntity[] vars)
        {
            foreach (var v in vars)
                expr = expr.Derive(v);
            return expr.Simplify();
        }

        /// <summary>
        /// Simplification of expression
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        public static Entity Simplify(Entity expr) => expr.SimplifyIntelli();

        /// <summary>
        /// Attempt to find analytical roots of a custom equation
        /// </summary>
        /// <param name="x"></param>
        /// <returns>
        /// Returns EntitySet. Work with it as with a list
        /// </returns>
        public static EntitySet Solve(Entity expr, VariableEntity x) => expr.Solve(x);

        /// <summary>
        /// Expands an equation trying to eliminate all the parentheses ( e. g. 2 * (x + 3) = 2 * x + 2 * 3 )
        /// </summary>
        /// <returns>
        /// An expanded Entity
        /// </returns>
        public static Entity Expand(Entity expr) => expr.Expand();

        /// <summary>
        /// Collapses an equation trying to eliminate as many power-uses as possible ( e. g. x * 3 + x * y = x * (3 + y) )
        /// </summary>
        /// <returns></returns>
        public static Entity Collapse(Entity expr) => expr.Collapse();

        /// <summary>
        /// Simplification synonim. Recommended to use in case of 
        /// computing a concrete number, knowing that you don't have
        /// any other symbols but numbers and functions.
        /// </summary>
        /// <returns></returns>
        public static Entity Evalf(Entity expr) => expr.Eval();

        /// <summary>
        /// Returns the expression in format of latex (for example, a / b -> \frac{a}{b})
        /// </summary>
        /// <returns></returns>
        public static Entity Latex(Entity expr) => expr.Latexise();

        /// <summary>
        /// Creates an instance of variable entity.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static VariableEntity Symbol(string name) => new VariableEntity(name);

        /// <summary>
        /// e ^ power
        /// </summary>
        /// <param name="power"></param>
        /// <returns></returns>
        public static Entity Exp(Entity power) => MathS.Pow(MathS.e, power);
    }
}
