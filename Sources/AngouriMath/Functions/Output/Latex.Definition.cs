//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

namespace AngouriMath
{
    using Core;
    partial record Entity : ILatexizeable
    {
        /// <summary>
        /// Returns the expression in LaTeX
        /// </summary>
        /// <example>
        /// <code>
        /// Entity expr = "a / b + sqrt(c)";
        /// Console.WriteLine(expr.Latexize());
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
        /// Console.WriteLine(expr.Latexize());
        /// Entity expr2 = "a / b ^ limit(sin(x) - cosh(y), x, +oo)";
        /// Console.WriteLine(expr2);
        /// Console.WriteLine(expr2.Latexize());
        /// </code>
        /// Prints
        /// <code>
        /// sqrt(a) + integral(sin(x), x)
        /// \sqrt{a}+\int \left[\sin\left(x\right)\right] dx
        /// a / b ^ limit(sin(x) - (e ^ y + e ^ (-y)) / 2, x, +oo)
        /// \frac{a}{{b}^{\lim_{x\to \infty } \left[\sin\left(x\right)-\frac{{e}^{y}+{e}^{-y}}{2}\right]}}
        /// </code>
        /// </example>
        /// <remarks>
        /// A node whose <see cref="Codomain"/> is not its type's default is rendered as the node
        /// subscripted with the set, <c>{\left(x\right)}_{\mathbb{Z}}</c>. The parentheses are
        /// unconditional: a <see cref="Variable"/> already renders its own index as a subscript,
        /// so <c>x_{\mathbb{Z}}</c> would be indistinguishable from a variable named that way.
        /// <a href="https://github.com/asc-community/AngouriMath/issues/1022">#1022</a>
        /// </remarks>
        public string Latexize()
            => PrintsItsCodomain
                // No \mathbb for "no restriction" -- there is no set to render. #1048
                ? Codomain is Domain.Any
                    ? $@"{{\left({LatexizeNode()}\right)}}_{{\mathrm{{Any}}}}"
                    : $@"{{\left({LatexizeNode()}\right)}}_{{{Set.SpecialSet.Create(Codomain).Latexize()}}}"
                : LatexizeNode();

        /// <summary>
        /// Renders this node, and this node only: the codomain subscript is
        /// <see cref="Latexize()"/>'s to add, so that it is decided in one place.
        /// </summary>
        private protected abstract string LatexizeNode();

        /// <summary>
        /// Calculus operators, unlike other functions, have a <see cref="LatexPriority"/> between addition/subtraction
        /// and multiplication/division which is different from <see cref="Priority"/>.
        /// </summary>
        internal virtual Priority LatexPriority => Priority;

        /// <summary>Returns the expression in LaTeX (for example, a / b -> \frac{a}{b})</summary>
        /// <param name="parenthesesRequired">Whether to wrap it with parentheses</param>
        /// <remarks>
        /// A node that renders its codomain needs no brackets: the subscript already applies to a
        /// parenthesised group, so anything around it would be a second pair.
        /// </remarks>
        protected internal string Latexize(bool parenthesesRequired) =>
            parenthesesRequired && !PrintsItsCodomain ? @$"\left({Latexize()}\right)" : Latexize();
    }
}
