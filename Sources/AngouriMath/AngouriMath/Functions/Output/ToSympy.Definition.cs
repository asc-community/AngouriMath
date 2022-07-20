//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

namespace AngouriMath
{
    partial record Entity
    {
        /// <summary>Generates Python code that you can use with sympy</summary>
        internal abstract string ToSymPy();
        
        /// <summary>
        /// Generates python code without any additional symbols that can be run in SymPy
        /// </summary>
        /// <param name="parenthesesRequired">
        /// Whether to wrap it with parentheses
        /// Usually depends on its parental nodes
        /// </param>
        /// <example>
        /// <code>
        /// using System;
        /// using static AngouriMath.MathS;
        /// 
        /// var (x, y, a) = Var("x", "y", "a");
        /// var expr = Limit(Integral(Sin(x) / (Cos(x) + Tan(y)), x) / a, y, +oo);
        /// Console.WriteLine(expr);
        /// Console.WriteLine("----------------------------");
        /// Console.WriteLine(ToSympyCode(expr));
        /// </code>
        /// Prints
        /// <code>
        /// limit(integral(sin(x) / (cos(x) + tan(y)), x) / a, y, +oo)
        /// ----------------------------
        /// import sympy
        /// 
        /// x = sympy.Symbol('x')
        /// y = sympy.Symbol('y')
        /// a = sympy.Symbol('a')
        /// 
        /// expr = sympy.limit(sympy.integrate(sympy.sin(x) / (sympy.cos(x) + sympy.tan(y)), x, 1) / a, y, +oo)
        /// </code>
        /// </example>
        protected string ToSymPy(bool parenthesesRequired) =>
            parenthesesRequired ? @$"({ToSymPy()})" : ToSymPy();
    }
}
