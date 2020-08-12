
/* Copyright (c) 2019-2020 Angourisoft
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,
 * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software
 * is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System.Text;
using AngouriMath.Core.Sys.Interfaces;

namespace AngouriMath
{
    public abstract partial record Entity : ILatexiseable
    {
        /// <summary>Generates Python code that you can use with sympy</summary>
        internal abstract string ToSymPy();
        protected internal string ToSymPy(bool parenthesesRequired) =>
            parenthesesRequired ? @$"({ToSymPy()})" : ToSymPy();
    }

    public partial record NumberEntity
    {
        internal override string ToSymPy() => ToString().Replace("i", "sympy.I");
    }

    public partial record VariableEntity
    {
        internal override string ToSymPy() => Name;
    }

    public partial record Tensor
    {
        internal override string ToSymPy() => InnerTensor.ToString();
    }

    public partial record Sumf
    {
        internal override string ToSymPy() =>
            Augend.ToSymPy(Augend.Priority < Const.Priority.Sum) + " + " + Addend.ToSymPy(Addend.Priority < Const.Priority.Sum);
    }
    public partial record Minusf
    {
        internal override string ToSymPy() =>
            Subtrahend.ToSymPy(Subtrahend.Priority < Const.Priority.Minus) + " - " + Minuend.ToSymPy(Minuend.Priority <= Const.Priority.Minus);
    }
    public partial record Mulf
    {
        internal override string ToSymPy() =>
            Multiplier.ToSymPy(Multiplier.Priority < Const.Priority.Mul) + " * " + Multiplicand.ToSymPy(Multiplicand.Priority < Const.Priority.Mul);
    }
    public partial record Divf
    {
        internal override string ToSymPy() =>
            Dividend.ToSymPy(Dividend.Priority < Const.Priority.Div) + " / " + Divisor.ToSymPy(Divisor.Priority <= Const.Priority.Div);
    }
    public partial record Sinf
    {
        internal override string ToSymPy() => "sympy.sin(" + Argument.ToSymPy() + ")";
    }
    public partial record Cosf
    {
        internal override string ToSymPy() => "sympy.cos(" + Argument.ToSymPy() + ")";
    }
    public partial record Tanf
    {
        internal override string ToSymPy() => "sympy.tan(" + Argument.ToSymPy() + ")";
    }
    public partial record Cotanf
    {
        internal override string ToSymPy() => "sympy.cot(" + Argument.ToSymPy() + ")";
    }
    public partial record Logf
    {
        internal override string ToSymPy() => "sympy.log(" + Antilogarithm.ToSymPy() + ", " + Base.ToSymPy() + ")";
    }
    public partial record Powf
    {
        internal override string ToSymPy() =>
            Exponent == 0.5m
            ? "sympy.sqrt(" + Base.ToSymPy() + ")"
            : Base.ToSymPy(Base.Priority < Const.Priority.Pow) + " ** " + Exponent.ToSymPy(Exponent.Priority < Const.Priority.Pow);
    }
    public partial record Arcsinf
    {
        internal override string ToSymPy() => "sympy.asin(" + Argument.ToSymPy() + ")";
    }
    public partial record Arccosf
    {
        internal override string ToSymPy() => "sympy.acos(" + Argument.ToSymPy() + ")";
    }
    public partial record Arctanf
    {
        internal override string ToSymPy() => "sympy.atan(" + Argument.ToSymPy() + ")";
    }
    public partial record Arccotanf
    {
        internal override string ToSymPy() => "sympy.acot(" + Argument.ToSymPy() + ")";
    }

    public partial record Factorialf
    {
        internal override string ToSymPy() => "sympy.factorial(" + Argument.ToSymPy() + ")";
    }

    public partial record Derivativef
    {
        internal override string ToSymPy() => $"sympy.diff({Expression.ToSymPy()}, {Variable.ToSymPy()}, {Iterations.ToSymPy()})";
    }

    public partial record Integralf
    { 
        // TODO: The 3rd parameter of sympy.integrate is not interpreted as iterations, unlike sympy.diff
        // which allows both sympy.diff(expr, var, iterations) and sympy.diff(expr, var1, var2, var3...)
        internal override string ToSymPy() => $"sympy.integrate({Expression.ToSymPy()}, {Variable.ToSymPy()}, {Iterations.ToSymPy()})";
    }

    public partial record Limitf
    {
        internal override string ToSymPy() =>
            @$"sympy.limit({Expression.ToSymPy()}, {Variable.ToSymPy()}, {Destination.ToSymPy()}{ApproachFrom switch
            {
                Limits.ApproachFrom.Left => ", '-'",
                Limits.ApproachFrom.BothSides => "",
                Limits.ApproachFrom.Right => ", '+'",
                _ => throw new System.ComponentModel.InvalidEnumArgumentException
                  (nameof(ApproachFrom), (int) ApproachFrom, typeof(Limits.ApproachFrom))
            }})";
    }
    internal static class ToSympy
    {
        /// <param name="expr"></param>
        /// <returns></returns>
        internal static string GenerateCode(Entity expr)
        {
            var sb = new StringBuilder();
            var vars = expr.Vars;
            sb.Append("import sympy\n\n");
            foreach (var f in vars)
                sb.Append($"{f} = sympy.Symbol('{f}')\n");
            sb.Append("\n");
            sb.Append("expr = " + expr.ToSymPy());
            return sb.ToString();
        }
    }
}
