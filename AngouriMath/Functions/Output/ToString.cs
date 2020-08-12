
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



using AngouriMath.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using AngouriMath.Core.Exceptions;
using AngouriMath.Core.Numerix;
using AngouriMath.Core.Sys.Interfaces;

namespace AngouriMath
{
    public abstract partial record Entity : ILatexiseable
    {
        /// <summary>Converts an expression into a string</summary>
        public override string ToString() => Stringize();
        internal abstract string Stringize();
        protected internal string Stringize(bool parenthesesRequired) =>
            parenthesesRequired ? @$"({Stringize()})" : Stringize();
    }

    public partial record NumberEntity
    {
        internal override string Stringize() => Value.ToString();
    }

    public partial record VariableEntity
    {
        internal override string Stringize() => Name;
    }

    public partial record Tensor
    {
        internal override string Stringize() => innerTensor.ToString();
    }

    public partial record Sumf
    {
        internal override string Stringize() =>
            Augend.Stringize(Augend.Priority < Const.Priority.Sum) + " + " + Addend.Stringize(Addend.Priority < Const.Priority.Sum);
    }
    public partial record Minusf
    {
        internal override string Stringize() =>
            Subtrahend.Stringize(Subtrahend.Priority < Const.Priority.Minus) + " - " + Minuend.Stringize(Minuend.Priority <= Const.Priority.Minus);
    }
    public partial record Mulf
    {
        internal override string Stringize() =>
            Multiplier.Stringize(Multiplier.Priority < Const.Priority.Mul) + " * " + Multiplicand.Stringize(Multiplicand.Priority < Const.Priority.Mul);
    }
    public partial record Divf
    {
        internal override string Stringize() =>
            Dividend.Stringize(Dividend.Priority < Const.Priority.Div) + " / " + Divisor.Stringize(Divisor.Priority <= Const.Priority.Div);
    }
    public partial record Sinf
    {
        internal override string Stringize() => "sin(" + Argument.Stringize() + ")";
    }
    public partial record Cosf
    {
        internal override string Stringize() => "cos(" + Argument.Stringize() + ")";
    }
    public partial record Tanf
    {
        internal override string Stringize() => "tan(" + Argument.Stringize() + ")";
    }
    public partial record Cotanf
    {
        internal override string Stringize() => "cotan(" + Argument.Stringize() + ")";
    }
    public partial record Logf
    {
        internal override string Stringize() => "log(" + Base.Stringize() + ", " + Antilogarithm.Stringize() + ")";
    }
    public partial record Powf
    {
        internal override string Stringize() =>
            Exponent == 0.5m
            ? "sqrt(" + Base.Stringize() + ")"
            : Base.Stringize(Base.Priority < Const.Priority.Pow) + " ^ " + Exponent.Stringize(Exponent.Priority < Const.Priority.Pow);
    }
    public partial record Arcsinf
    {
        internal override string Stringize() => "arcsin(" + Argument.Stringize() + ")";
    }
    public partial record Arccosf
    {
        internal override string Stringize() => "arccos(" + Argument.Stringize() + ")";
    }
    public partial record Arctanf
    {
        internal override string Stringize() => "arctan(" + Argument.Stringize() + ")";
    }
    public partial record Arccotanf
    {
        internal override string Stringize() => "arccotan(" + Argument.Stringize() + ")";
    }

    public partial record Factorialf
    {
        internal override string Stringize() => Argument.Stringize(Argument.Priority < Const.Priority.Num) + "!";
    }

    public partial record Derivativef
    {
        internal override string Stringize() => $"derive({Expression}, {Variable}, {Iterations})";
    }

    public partial record Integralf
    {
        internal override string Stringize() => $"integrate({Expression}, {Variable}, {Iterations})";
    }

    public partial record Limitf
    {
        internal override string Stringize() =>
            ApproachFrom switch
            {
                Limits.ApproachFrom.Left => "limitleft",
                Limits.ApproachFrom.BothSides => "limit",
                Limits.ApproachFrom.Right => "limitright",
                _ => throw new System.ComponentModel.InvalidEnumArgumentException
                  (nameof(ApproachFrom), (int)ApproachFrom, typeof(Limits.ApproachFrom))
            } + $"({Expression}, {Variable}, {Destination})";
    }

    internal static class SetToString
    {
        static readonly List<string> Operators = new List<string>
        {
            "|",
            "&",
            @"\"
        };
        internal static string OperatorToString(OperatorSet set)
        {
            // TODO: add check whether set is correct (add TreeAnalyzer.CheckSet)
            if (set.ConnectionType == OperatorSet.OperatorType.INVERSION)
                return "!" + set.Children[0].ToString();
            return "(" + set.Children[0].ToString() + ")" + Operators[(int)set.ConnectionType] + "(" + set.Children[1].ToString() + ")";
        }

        internal static string LinearSetToString(Set set)
        => set.IsEmpty() ? "{}" :
            string.Join("|", set.Pieces.Select(c => c.ToString()));
    }
}
