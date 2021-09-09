﻿namespace AngouriMath
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
        public abstract string Latexise();

        /// <summary>Returns the expression in LaTeX (for example, a / b -> \frac{a}{b})</summary>
        /// <param name="parenthesesRequired">Whether to wrap it with parentheses</param>
        protected internal string Latexise(bool parenthesesRequired) =>
            parenthesesRequired ? @$"\left({Latexise()}\right)" : Latexise();
    }
}
