/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using AngouriMath.Functions;
using static AngouriMath.Entity.Number;
using AngouriMath.Core;
using static AngouriMath.Entity.Set;
using AngouriMath.Functions.Algebra;

namespace AngouriMath
{
    public abstract partial record Entity
    {
        public partial record Number
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() => this;
            /// <inheritdoc/>
            protected override Entity InnerSimplify() => this;
        }
            
        // Each function and operator processing
        public partial record Sumf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() => (Augend, Addend).Unpack2Eval() switch
            {
                (Complex n1, Complex n2) => n1 + n2,
                (Tensor n1, Tensor n2) => n1.Elementwise(n2, (n1, n2) => (n1 + n2).Unpack1Eval()),
                (var n1, Tensor n2) => n2.Elementwise(n2 => (n1 + n2).Unpack1Eval()),
                (Tensor n1, var n2) => n1.Elementwise(n1 => (n1 + n2).Unpack1Eval()),
                (FiniteSet finite, var n2) when n2 is not Set => finite.Apply(c => (c + n2).Unpack1Eval()),
                (var n2, FiniteSet finite) when n2 is not Set => finite.Apply(c => (n2 + c).Unpack1Eval()),
                (Interval inter, var n2) when n2 is not Set => inter.New((inter.Left + n2).Unpack1Eval(), (inter.Right + n2).Unpack1Eval()),
                (var n2, Interval inter) when n2 is not Set => inter.New((n2 + inter.Left).Unpack1Eval(), (n2 + inter.Right).Unpack1Eval()),
                (var n1, var n2) => New(n1, n2)
            };
            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                Evaled is Number { IsExact: true } ? Evaled : (Augend, Addend).Unpack2Simplify() switch
                {
                    (Tensor n1, Tensor n2) => n1.Elementwise(n2, (n1, n2) => (n1 + n2).Unpack1Simplify()),
                    (var n1, Tensor n2) => n2.Elementwise(n2 => (n1 + n2).Unpack1Simplify()),
                    (Tensor n1, var n2) => n1.Elementwise(n1 => (n1 + n2).Unpack1Simplify()),
                    (var n1, Integer(0)) => n1,
                    (Integer(0), var n2) => n2,
                    (FiniteSet finite, var n2) when n2 is not Set => finite.Apply(c => (c + n2).Unpack1Simplify()),
                    (var n2, FiniteSet finite) when n2 is not Set => finite.Apply(c => (n2 + c).Unpack1Simplify()),
                    (Interval inter, var n2) when n2 is not Set => inter.New((inter.Left + n2).Unpack1Simplify(), (inter.Right + n2).Unpack1Simplify()),
                    (var n2, Interval inter) when n2 is not Set => inter.New((n2 + inter.Left).Unpack1Simplify(), (n2 + inter.Right).Unpack1Simplify()),
                    (var n1, var n2) => n1 == n2 ? (2 * n1).Unpack1Simplify() : New(n1, n2)
                };
        }
        public partial record Minusf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() => (Subtrahend, Minuend).Unpack2Eval() switch
            {
                (Complex n1, Complex n2) => n1 - n2,
                (Tensor n1, Tensor n2) => n1.Elementwise(n2, (n1, n2) => (n1 - n2).Unpack1Eval()),
                (var n1, Tensor n2) => n2.Elementwise(n2 => (n1 - n2).Unpack1Eval()),
                (Tensor n1, var n2) => n1.Elementwise(n1 => (n1 - n2).Unpack1Eval()),
                (FiniteSet finite, var n2) when n2 is not Set => finite.Apply(c => (c - n2).Unpack1Eval()),
                (var n2, FiniteSet finite) when n2 is not Set => finite.Apply(c => (n2 - c).Unpack1Eval()),
                (Interval inter, var n2) when n2 is not Set => inter.New((inter.Left - n2).Unpack1Eval(), (inter.Right - n2).Unpack1Eval()),
                (var n2, Interval inter) when n2 is not Set => inter.New((n2 - inter.Left).Unpack1Eval(), (n2 - inter.Right).Unpack1Eval()),
                (var n1, var n2) => New(n1, n2)
            };
            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                Evaled is Number { IsExact: true } ? Evaled : (Subtrahend, Minuend).Unpack2Simplify() switch
                {
                    (Tensor n1, Tensor n2) => n1.Elementwise(n2, (n1, n2) => (n1 - n2).Unpack1Simplify()),
                    (var n1, Tensor n2) => n2.Elementwise(n2 => (n1 - n2).Unpack1Simplify()),
                    (Tensor n1, var n2) => n1.Elementwise(n1 => (n1 - n2).Unpack1Simplify()),
                    (var n1, Integer(0)) => n1,
                    (Integer(0), var n2) => (-n2).Unpack1Simplify(),
                    (FiniteSet finite, var n2) when n2 is not Set => finite.Apply(c => (c - n2).Unpack1Simplify()),
                    (var n2, FiniteSet finite) when n2 is not Set => finite.Apply(c => (n2 - c).Unpack1Simplify()),
                    (Interval inter, var n2) when n2 is not Set => inter.New((inter.Left - n2).Unpack1Simplify(), (inter.Right - n2).Unpack1Simplify()),
                    (var n2, Interval inter) when n2 is not Set => inter.New((n2 - inter.Left).Unpack1Simplify(), (n2 - inter.Right).Unpack1Simplify()),
                    (var n1, var n2) => n1 == n2 ? (Entity)0 : New(n1, n2)
                };
        }
        public partial record Mulf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() => (Multiplier, Multiplicand).Unpack2Eval() switch
            {
                (Complex n1, Complex n2) => n1 * n2,
                (Tensor n1, Tensor n2) => n1.Elementwise(n2, (n1, n2) => (n1 * n2).Unpack1Eval()),
                (var n1, Tensor n2) => n2.Elementwise(n2 => (n1 * n2).Unpack1Eval()),
                (Tensor n1, var n2) => n1.Elementwise(n1 => (n1 * n2).Unpack1Eval()),
                (FiniteSet finite, var n2) when n2 is not Set => finite.Apply(c => (c * n2).Unpack1Eval()),
                (var n2, FiniteSet finite) when n2 is not Set => finite.Apply(c => (n2 * c).Unpack1Eval()),
                (var n1, var n2) => New(n1, n2)
            };
            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                Evaled is Number { IsExact: true } ? Evaled : (Multiplier, Multiplicand).Unpack2Simplify() switch
                {
                    (Integer minusOne, Mulf(var minusOne1, var any1)) when minusOne == Integer.MinusOne && minusOne1 == Integer.MinusOne => any1,
                    (Tensor n1, Tensor n2) => n1.Elementwise(n2, (n1, n2) => (n1 * n2).Unpack1Simplify()),
                    (var n1, Tensor n2) => n2.Elementwise(n2 => (n1 * n2).Unpack1Simplify()),
                    (Tensor n1, var n2) => n1.Elementwise(n1 => (n1 * n2).Unpack1Simplify()),
                    (_, Integer(0)) or (Integer(0), _) => 0,
                    (var n1, Integer(1)) => n1,
                    (Integer(1), var n2) => n2,
                    (FiniteSet finite, var n2) when n2 is not Set => finite.Apply(c => (c * n2).Unpack1Simplify()),
                    (var n2, FiniteSet finite) when n2 is not Set => finite.Apply(c => (n2 * c).Unpack1Simplify()),
                    (var n1, var n2) => n1 == n2 ? new Powf(n1, 2).Unpack1Simplify() : New(n1, n2)
                };
        }
        public partial record Divf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() => (Dividend, Divisor).Unpack2Eval() switch
            {
                (Complex n1, Complex n2) => n1 / n2,
                (Tensor n1, Tensor n2) => n1.Elementwise(n2, (n1, n2) => (n1 / n2).Unpack1Eval()),
                (var n1, Tensor n2) => n2.Elementwise(n2 => (n1 / n2).Unpack1Eval()),
                (Tensor n1, var n2) => n1.Elementwise(n1 => (n1 / n2).Unpack1Eval()),
                (FiniteSet finite, var n2) when n2 is not Set => finite.Apply(c => (c / n2).Unpack1Eval()),
                (var n2, FiniteSet finite) when n2 is not Set => finite.Apply(c => (n2 / c).Unpack1Eval()),
                (var n1, var n2) => New(n1, n2)
            };
            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                Evaled is Number { IsExact: true } ? Evaled : (Dividend, Divisor).Unpack2Simplify() switch
                {
                    (Tensor n1, Tensor n2) => n1.Elementwise(n2, (n1, n2) => (n1 / n2).Unpack1Simplify()),
                    (var n1, Tensor n2) => n2.Elementwise(n2 => (n1 / n2).Unpack1Simplify()),
                    (Tensor n1, var n2) => n1.Elementwise(n1 => (n1 / n2).Unpack1Simplify()),
                    (Integer(0), _) => 0,
                    (_, Integer(0)) => Real.NaN,
                    (var n1, Integer(1)) => n1.Unpack1Simplify(),
                    (FiniteSet finite, var n2) when n2 is not Set => finite.Apply(c => (c / n2).Unpack1Simplify()),
                    (var n2, FiniteSet finite) when n2 is not Set => finite.Apply(c => (n2 / c).Unpack1Simplify()),
                    (var n1, var n2) => n1 == n2 ? (Entity)1 : New(n1, n2)
                };
        }
        public partial record Powf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() => (Base, Exponent).Unpack2Eval() switch
            {
                (Complex n1, Complex n2) => Number.Pow(n1, n2),
                (Tensor n1, Tensor n2) => n1.Elementwise(n2, (n1, n2) => n1.Pow(n2).Unpack1Eval()),
                (var n1, Tensor n2) => n2.Elementwise(n2 => n1.Pow(n2).Unpack1Eval()),
                (Tensor n1, var n2) => n1.Elementwise(n1 => n1.Pow(n2).Unpack1Eval()),
                (FiniteSet finite, var n2) when n2 is not Set => finite.Apply(c => c.Pow(n2).Unpack1Eval()),
                (var n2, FiniteSet finite) when n2 is not Set => finite.Apply(c => n2.Pow(c).Unpack1Eval()),
                (var n1, var n2) => New(n1, n2)
            };
            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                Evaled is Number { IsExact: true } ? Evaled : (Base, Exponent).Unpack2Simplify() switch
                {
                    (Tensor n1, Tensor n2) => n1.Elementwise(n2, (n1, n2) => n1.Pow(n2).Unpack1Simplify()),
                    (var n1, Tensor n2) => n2.Elementwise(n2 => n1.Pow(n2).Unpack1Simplify()),
                    (Tensor n1, var n2) => n1.Elementwise(n1 => n1.Pow(n2).Unpack1Simplify()),
                // 0^x is undefined for Re(x) <= 0
                    (Integer(1), _) => 0,
                    (var n1, Integer(-1)) => (1 / n1).Unpack1Simplify(),
                    (_, Integer(0)) => 1,
                    (var n1, Integer(1)) => n1.Unpack1Simplify(),
                    (FiniteSet finite, var n2) when n2 is not Set => finite.Apply(c => c.Pow(n2).Unpack1Simplify()),
                    (var n2, FiniteSet finite) when n2 is not Set => finite.Apply(c => n2.Pow(c).Unpack1Simplify()),
                    (var n1, var n2) => New(n1, n2)
                };
        }
        public partial record Sinf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() => Argument.Unpack1Eval() switch
            {
                Complex n => Number.Sin(n),
                Tensor n => n.Elementwise(n => n.Sin().Unpack1Eval()),
                FiniteSet finite => finite.Apply(c => c.Sin().Unpack1Eval()),
                var n => New(n)
            };
            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                Evaled is Number { IsExact: true } ? Evaled : Argument.Unpack1Simplify() switch
                {
                    Tensor n => n.Elementwise(n => n.Sin().Unpack1Simplify()),
                    { Evaled: Complex n } when TrigonometryTableValues.PullSin(n, out var res) => res,
                    FiniteSet finite => finite.Apply(c => c.Sin().Unpack1Simplify()),
                    var n => New(n)
                };
        }

        public partial record Cosf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() => Argument.Unpack1Eval() switch
            {
                Complex n => Number.Cos(n),
                Tensor n => n.Elementwise(n => n.Cos().Unpack1Eval()),
                FiniteSet finite => finite.Apply(c => c.Cos().Unpack1Eval()),
                var n => New(n)
            };
            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                Evaled is Number { IsExact: true } ? Evaled : Argument.Unpack1Simplify() switch
                {
                    Tensor n => n.Elementwise(n => n.Cos().Unpack1Simplify()),
                    { Evaled: Complex n } when TrigonometryTableValues.PullCos(n, out var res) => res,
                    FiniteSet finite => finite.Apply(c => c.Cos().Unpack1Simplify()),
                    var n => New(n)
                };
        }

        public partial record Secantf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() => Argument.Unpack1Eval() switch
            {
                Complex n => Number.Secant(n),
                Tensor n => n.Elementwise(n => n.Sec().Unpack1Eval()),
                FiniteSet finite => finite.Apply(c => c.Sec().Unpack1Eval()),
                var n => New(n)
            };
            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                Evaled is Number { IsExact: true } ? Evaled : Argument.Unpack1Simplify() switch
                {
                    Tensor n => n.Elementwise(n => n.Sec().Unpack1Simplify()),
                    { Evaled: Complex n } when TrigonometryTableValues.PullCos(n, out var res) => (1 / res).Unpack1Simplify(),
                    FiniteSet finite => finite.Apply(c => c.Sec().Unpack1Simplify()),
                    var n => New(n)
                };
        }

        public partial record Cosecantf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() => Argument.Unpack1Eval() switch
            {
                Complex n => Number.Cosecant(n),
                Tensor n => n.Elementwise(n => n.Cosec().Unpack1Eval()),
                FiniteSet finite => finite.Apply(c => c.Cosec().Unpack1Eval()),
                var n => New(n)
            };
            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                Evaled is Number { IsExact: true } ? Evaled : Argument.Unpack1Simplify() switch
                {
                    Tensor n => n.Elementwise(n => n.Cosec().Unpack1Simplify()),
                    { Evaled: Complex n } when TrigonometryTableValues.PullSin(n, out var res) => (1 / res).Unpack1Simplify(),
                    FiniteSet finite => finite.Apply(c => c.Cosec().Unpack1Simplify()),
                    var n => New(n)
                };
        }

        public partial record Arcsecantf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() => Argument.Unpack1Eval() switch
            {
                Complex n => Number.Arcsecant(n),
                Tensor n => n.Elementwise(n => n.Arcsec().Unpack1Eval()),
                FiniteSet finite => finite.Apply(c => c.Arcsec().Unpack1Eval()),
                var n => New(n)
            };
            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                Evaled is Number { IsExact: true } ? Evaled : Argument.Unpack1Simplify() switch
                {
                    Tensor n => n.Elementwise(n => n.Arcsec().Unpack1Simplify()),
                    FiniteSet finite => finite.Apply(c => c.Arcsec().Unpack1Simplify()),
                    var n => New(n)
                };
        }

        public partial record Arccosecantf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() => Argument.Unpack1Eval() switch
            {
                Complex n => Number.Arccosecant(n),
                Tensor n => n.Elementwise(n => n.Arccosec().Unpack1Eval()),
                FiniteSet finite => finite.Apply(c => c.Arccosec().Unpack1Eval()),
                var n => New(n)
            };
            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                Evaled is Number { IsExact: true } ? Evaled : Argument.Unpack1Simplify() switch
                {
                    Tensor n => n.Elementwise(n => n.Arccosec().Unpack1Simplify()),
                    FiniteSet finite => finite.Apply(c => c.Arccosec().Unpack1Simplify()),
                    var n => New(n)
                };
        }

        public partial record Tanf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() => Argument.Unpack1Eval() switch
            {
                Complex n => Number.Tan(n),
                Tensor n => n.Elementwise(n => n.Tan().Unpack1Eval()),
                FiniteSet finite => finite.Apply(c => c.Tan().Unpack1Eval()),
                var n => New(n)
            };
            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                Evaled is Number { IsExact: true } ? Evaled : Argument.Unpack1Simplify() switch
                {
                    Tensor n => n.Elementwise(n => n.Tan().Unpack1Simplify()),
                    { Evaled: Complex n } when TrigonometryTableValues.PullTan(n, out var res) => res,
                    FiniteSet finite => finite.Apply(c => c.Tan().Unpack1Simplify()),
                    var n => New(n)
                };
        }
        public partial record Cotanf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() => Argument.Unpack1Eval() switch
            {
                Complex n => Number.Cotan(n),
                Tensor n => n.Elementwise(n => n.Cotan().Unpack1Eval()),
                FiniteSet finite => finite.Apply(c => c.Cotan().Unpack1Eval()),
                var n => New(n)
            };
            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                Evaled is Number { IsExact: true } ? Evaled : Argument.Unpack1Simplify() switch
                {
                    Tensor n => n.Elementwise(n => n.Cotan().Unpack1Simplify()),
                    { Evaled: Complex n } when TrigonometryTableValues.PullTan(n, out var res) => 1 / res,
                    FiniteSet finite => finite.Apply(c => c.Cotan().Unpack1Simplify()),
                    var n => New(n)
                };
        }
        public partial record Logf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() => (Base.Unpack1Eval(), Antilogarithm.Unpack1Eval()) switch
            {
                (Complex n1, Complex n2) => Number.Log(n1, n2),
                (Tensor n1, Tensor n2) => n1.Elementwise(n2, (n1, n2) => n1.Log(n2).Unpack1Eval()),
                (var n1, Tensor n2) => n2.Elementwise(n2 => n1.Log(n2).Unpack1Eval()),
                (Tensor n1, var n2) => n1.Elementwise(n1 => n1.Log(n2).Unpack1Eval()),
                (FiniteSet finite, var n2) when n2 is not Set => finite.Apply(c => MathS.Log(c, n2).Unpack1Eval()),
                (var n2, FiniteSet finite) when n2 is not Set => finite.Apply(c => MathS.Log(n2, c).Unpack1Eval()),
                (var n1, var n2) => New(n1, n2)
            };
            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                Evaled is Number { IsExact: true } ? Evaled : (Base.Unpack1Simplify(), Antilogarithm.Unpack1Simplify()) switch
                {
                    (Tensor n1, Tensor n2) => n1.Elementwise(n2, (n1, n2) => n1.Log(n2).Unpack1Simplify()),
                    (var n1, Tensor n2) => n2.Elementwise(n2 => n1.Log(n2).Unpack1Simplify()),
                    (Tensor n1, var n2) => n1.Elementwise(n1 => n1.Log(n2).Unpack1Simplify()),
                    (_, Integer(0)) => Real.NegativeInfinity,
                    (_, Integer(1)) => 0,
                    (FiniteSet finite, var n2) when n2 is not Set => finite.Apply(c => MathS.Log(c, n2).Unpack1Simplify()),
                    (var n2, FiniteSet finite) when n2 is not Set => finite.Apply(c => MathS.Log(n2, c).Unpack1Simplify()),
                    (var n1, var n2) => n1 == n2 ? (Entity)1 : New(n1, n2)
                };
        }
        public partial record Arcsinf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() => Argument.Unpack1Eval() switch
            {
                Complex n => Number.Arcsin(n),
                Tensor n => n.Elementwise(n => n.Arcsin().Unpack1Eval()),
                FiniteSet finite => finite.Apply(c => c.Arcsin().Unpack1Eval()),
                var n => New(n)
            };
            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                Evaled is Number { IsExact: true } ? Evaled : Argument.Unpack1Simplify() switch
                {
                    Tensor n => n.Elementwise(n => n.Arcsin().Unpack1Simplify()),
                    FiniteSet finite => finite.Apply(c => c.Arcsin().Unpack1Simplify()),
                    var n => New(n)
                };
        }
        public partial record Arccosf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() => Argument.Unpack1Eval() switch
            {
                Complex n => Number.Arccos(n),
                Tensor n => n.Elementwise(n => n.Arccos().Unpack1Eval()),
                FiniteSet finite => finite.Apply(c => c.Arccos().Unpack1Eval()),
                var n => New(n)
            };
            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                Evaled is Number { IsExact: true } ? Evaled : Argument.Unpack1Simplify() switch
                {
                    Tensor n => n.Elementwise(n => n.Arccos().Unpack1Simplify()),
                    FiniteSet finite => finite.Apply(c => c.Arccos().Unpack1Simplify()),
                    var n => New(n)
                };
        }
        public partial record Arctanf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() => Argument.Unpack1Eval() switch
            {
                Complex n => Number.Arctan(n),
                Tensor n => n.Elementwise(n => n.Arctan().Unpack1Eval()),
                FiniteSet finite => finite.Apply(c => c.Arctan().Unpack1Eval()),
                var n => New(n)
            };
            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                Evaled is Number { IsExact: true } ? Evaled : Argument.Unpack1Simplify() switch
                {
                    Tensor n => n.Elementwise(n => n.Arctan().Unpack1Simplify()),
                    FiniteSet finite => finite.Apply(c => c.Arctan().Unpack1Simplify()),
                    var n => New(n)
                };
        }
        public partial record Arccotanf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() => Argument.Unpack1Eval() switch
            {
                Complex n => Number.Arccotan(n),
                Tensor n => n.Elementwise(n => n.Arccotan().Unpack1Eval()),
                FiniteSet finite => finite.Apply(c => c.Arccotan().Unpack1Eval()),
                var n => New(n)
            };
            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                Evaled is Number { IsExact: true } ? Evaled : Argument.Unpack1Simplify() switch
                {
                    Tensor n => n.Elementwise(n => n.Arccotan().Unpack1Simplify()),
                    FiniteSet finite => finite.Apply(c => c.Arccotan().Unpack1Simplify()),
                    var n => New(n)
                };
        }
        public partial record Factorialf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() => Argument.Unpack1Eval() switch
            {
                Complex n => Number.Factorial(n),
                Tensor n => n.Elementwise(n => n.Factorial().Unpack1Eval()),
                FiniteSet finite => finite.Apply(c => c.Factorial().Unpack1Simplify()),
                var n => New(n)
            };
            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                Evaled is Number { IsExact: true } ? Evaled : Argument.Unpack1Simplify() switch
                {
                    Tensor n => n.Elementwise(n => n.Factorial().Unpack1Simplify()),
                    FiniteSet finite => finite.Apply(c => c.Factorial().Unpack1Simplify()),
                    Rational({ Numerator: var num, Denominator: var den }) when den.Equals(2) && (num + 1) / 2 is var en => (
                        en > 0
                        // (+n - 1/2)! = (2n-1)!/(2^(2n-1)(n-1)!)*sqrt(pi)
                        // also 2n-1 is the numerator
                        ? Rational.Create(num.Factorial(), PeterO.Numbers.EInteger.FromInt32(2).Pow(num) * (en - 1).Factorial())
                        // (-n - 1/2)! = (-4)^n*n!/(2n)!*sqrt(pi)
                        : Rational.Create(PeterO.Numbers.EInteger.FromInt32(-4).Pow(-en) * (-en).Factorial(), (2 * -en).Factorial())
                    ) * MathS.Sqrt(MathS.pi),
                    var n => New(n)
                };
        }
        public partial record Derivativef
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() => (Expression, Var, Iterations).Unpack3EvalT() switch
            {
                (Tensor expr, var var, var iters) => expr.Elementwise(n => new Derivativef(n, var, iters).Unpack1Eval()),
                (var expr, _, 0) => expr,
                // TODO: consider Integral for negative cases
                (var expr, Variable var, var asInt) => expr.Derive(var, asInt),
                _ => this
            };
            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                Var.Unpack1Simplify() is Variable var
                ? Iterations == 0
                    ? Expression.Unpack1Simplify()
                    : Expression.Derive(var, Iterations)
                : this;
        }
        public partial record Integralf
        {
            private Entity SequentialIntegrating(Entity expr, Variable var, int iterations)
            {
                if (iterations < 0)
                    return this;
                var changed = expr;
                for (int i = 0; i < iterations; i++)
                    changed = Integration.ComputeIndefiniteIntegral(changed.Unpack1Simplify(), var);
                return changed;
            }

            /// <inheritdoc/>
            protected override Entity InnerEval() => (Expression, Var, Iterations).Unpack3EvalT() switch
            {
                (Tensor expr, var var, var iters) => expr.Elementwise(n => new Integralf(n, var, iters).Unpack1Eval()),
                (var expr, _, 0) => expr,
                // TODO: consider Derivative for negative cases
                (var expr, Variable var, int asInt) => SequentialIntegrating(expr, var, asInt),

                _ => this
            };
            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
               Var.Unpack1Simplify() is Variable var ? SequentialIntegrating(Expression.Unpack1Simplify(), var, Iterations) : this;
        }
        public partial record Limitf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() => ApproachFrom switch
            {
                ApproachFrom.Left => New(Expression.Unpack1Eval(), Var, Destination.Unpack1Eval(), ApproachFrom),
                ApproachFrom.BothSides => New(Expression.Unpack1Eval(), Var, Destination.Unpack1Eval(), ApproachFrom),
                ApproachFrom.Right => New(Expression.Unpack1Eval(), Var, Destination.Unpack1Eval(), ApproachFrom),
                _ => this,
            };
            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                Var.Unpack1Simplify() switch
                {
                    // if it cannot compute it, it will anyway return the node
                    Entity.Variable x => Expression.Unpack1Simplify().Limit(x, Destination.Unpack1Simplify(), ApproachFrom),
                    var x => new Limitf(Expression.Unpack1Simplify(), x, Destination.Unpack1Simplify(), ApproachFrom)
                };

        }

        public partial record Signumf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval()
                => Argument.Unpack1Eval() switch
                {
                    Complex n => Complex.Signum(n),
                    Tensor n => n.Elementwise(c => c.Signum().Unpack1Eval()),
                    FiniteSet finite => finite.Apply(c => c.Signum().Unpack1Simplify()),
                    var n => this
                };

            // TODO: probably we can simplify it further
            /// <inheritdoc/>
            protected override Entity InnerSimplify()
                => Argument.Unpack1Eval() is Number { IsExact: true } ? Argument.Unpack1Eval() :
                Argument.Unpack1Simplify() switch
                {
                    Tensor n => n.Elementwise(c => c.Signum().Unpack1Simplify()),
                    FiniteSet finite => finite.Apply(c => c.Signum().Unpack1Simplify()),
                    var n => this
                };
        }

        public partial record Absf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval()
                => Argument.Unpack1Eval() switch
                {
                    Complex n => Complex.Abs(n),
                    Tensor n => n.Elementwise(c => c.Abs().Unpack1Eval()),
                    FiniteSet finite => finite.Apply(c => c.Abs().Unpack1Eval()),
                    var n => this
                };

            // TODO: probably we can simplify it further
            /// <inheritdoc/>
            protected override Entity InnerSimplify()
                => Argument.Unpack1Eval() is Number { IsExact: true } ? Argument.Unpack1Eval() :
                Argument.Unpack1Simplify() switch
                {
                    Tensor n => n.Elementwise(c => c.Signum().Unpack1Simplify()),
                    FiniteSet finite => finite.Apply(c => c.Abs().Unpack1Simplify()),
                    var n => this
                };
        }
    }
}
