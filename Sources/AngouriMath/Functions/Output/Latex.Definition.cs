//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

namespace AngouriMath
{
    using Core;
    partial record Entity : ILatexiseable
    {
        /// <summary>
        /// Returns the expression in LaTeX
        /// </summary>
        /// <example>
        /// <code>
        /// Entity expr = "a / b + sqrt(c)";
        /// Console.WriteLine(expr.Latexise());
        /// </code>
        /// Output:
        /// <code>
        /// \frac{a}{b}+\sqrt{c}
        /// </code>
        /// </example>
        /// <example>
        /// <code>
        /// using System;
        /// using AngouriMath;
        /// using static AngouriMath.MathS;
        /// 
        /// Entity expr = "sqrt(a) + integral(sin(x), x)";
        /// Console.WriteLine(expr);
        /// Console.WriteLine(expr.Latexise());
        /// Entity expr2 = "a / b ^ limit(sin(x) - cosh(y), x, +oo)";
        /// Console.WriteLine(expr2);
        /// Console.WriteLine(expr2.Latexise());
        /// </code>
        /// Prints
        /// <code>
        /// sqrt(a) + integral(sin(x), x)
        /// \sqrt{a}+\int \left[\sin\left(x\right)\right] dx
        /// a / b ^ limit(sin(x) - (e ^ y + e ^ (-y)) / 2, x, +oo)
        /// \frac{a}{{b}^{\lim_{x\to \infty } \left[\sin\left(x\right)-\frac{{e}^{y}+{e}^{-y}}{2}\right]}}
        /// </code>
        /// </example>
        public abstract string Latexise();

        /// <summary>Returns the expression in LaTeX (for example, a / b -> \frac{a}{b})</summary>
        /// <param name="parenthesesRequired">Whether to wrap it with parentheses</param>
        protected internal string Latexise(bool parenthesesRequired) =>
            parenthesesRequired ? @$"\left({Latexise()}\right)" : Latexise();
    }
}
