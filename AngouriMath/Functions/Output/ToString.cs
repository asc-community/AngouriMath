
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

namespace AngouriMath
{
    using Core;
    using static Entity.Number;
    public abstract partial record Entity
    {
        /// <summary>Converts an expression into a string</summary>
        public override string ToString() => Stringize();
        internal abstract string Stringize();
        protected internal string Stringize(bool parenthesesRequired) =>
            parenthesesRequired ? @$"({Stringize()})" : Stringize();
        public partial record Number { }
        public partial record Variable
        {
            internal override string Stringize() => Name;
        }
        public partial record Tensor
        {
            internal override string Stringize() => InnerTensor.ToString();
        }
        public partial record Sumf
        {
            internal override string Stringize() =>
                Augend.Stringize(Augend.Priority < Priority.Sum) + " + " + Addend.Stringize(Addend.Priority < Priority.Sum);
        }
        public partial record Minusf
        {
            internal override string Stringize() =>
                Subtrahend.Stringize(Subtrahend.Priority < Priority.Minus) + " - " + Minuend.Stringize(Minuend.Priority <= Priority.Minus);
        }
        public partial record Mulf
        {
            internal override string Stringize() =>
                (Multiplier is Integer(-1) ? "-"
                 : Multiplier.Stringize(Multiplier.Priority < Priority.Mul) + " * ")
                + Multiplicand.Stringize(Multiplicand.Priority < Priority.Mul);
        }
        public partial record Divf
        {
            internal override string Stringize() =>
                Dividend.Stringize(Dividend.Priority < Priority.Div) + " / " + Divisor.Stringize(Divisor.Priority <= Priority.Div);
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
                : Base.Stringize(Base.Priority < Priority.Pow) + " ^ " + Exponent.Stringize(Exponent.Priority < Priority.Pow);
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
            internal override string Stringize() => Argument.Stringize(Argument.Priority < Priority.Number) + "!";
        }
        public partial record Derivativef
        {
            internal override string Stringize() => $"derive({Expression}, {Var}, {Iterations})";
        }
        public partial record Integralf
        {
            internal override string Stringize() => $"integrate({Expression}, {Var}, {Iterations})";
        }
        public partial record Limitf
        {
            internal override string Stringize() =>
                ApproachFrom switch
                {
                    ApproachFrom.Left => "limitleft",
                    ApproachFrom.BothSides => "limit",
                    ApproachFrom.Right => "limitright",
                    _ => throw new System.ComponentModel.InvalidEnumArgumentException
                      (nameof(ApproachFrom), (int)ApproachFrom, typeof(ApproachFrom))
                } + $"({Expression}, {Var}, {Destination})";
        }
        public partial record Signumf
        {
            internal override string Stringize() => $"sgn({Argument})";
        }
        public partial record Absf
        {
            internal override string Stringize() => $"abs({Argument})";
        }
    }
}