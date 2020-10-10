
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

using AngouriMath.Functions;
using System;
using static AngouriMath.Entity.Number;
using AngouriMath.Core;
using AngouriMath.Core.Exceptions;
using static AngouriMath.Entity.Set;

namespace AngouriMath
{
    public abstract partial record Entity
    {
        public partial record Number
        {
            protected override Entity InnerEval() => this;
            internal override Entity InnerSimplify() => this;
        }
            
        // Each function and operator processing
        public partial record Sumf
        {
            protected override Entity InnerEval() => (Augend.Evaled, Addend.Evaled) switch
            {
                (Complex n1, Complex n2) => n1 + n2,
                (Tensor n1, Tensor n2) => n1.Elementwise(n2, (n1, n2) => (n1 + n2).Evaled),
                (var n1, Tensor n2) => n2.Elementwise(n2 => (n1 + n2).Evaled),
                (Tensor n1, var n2) => n1.Elementwise(n1 => (n1 + n2).Evaled),
                (FiniteSet finite, var n2) when n2 is not Set => finite.Apply(c => (c + n2).Evaled),
                (var n2, FiniteSet finite) when n2 is not Set => finite.Apply(c => (n2 + c).Evaled),
                (Interval inter, var n2) when n2 is not Set => inter.New((inter.Left + n2).Evaled, (inter.Right + n2).Evaled),
                (var n2, Interval inter) when n2 is not Set => inter.New((n2 + inter.Left).Evaled, (n2 + inter.Right).Evaled),
                (var n1, var n2) => New(n1, n2)
            };
            internal override Entity InnerSimplify() =>
                Evaled is Number { IsExact: true } ? Evaled : (Augend.InnerSimplified, Addend.InnerSimplified) switch
                {
                    (Tensor n1, Tensor n2) => n1.Elementwise(n2, (n1, n2) => (n1 + n2).InnerSimplified),
                    (var n1, Tensor n2) => n2.Elementwise(n2 => (n1 + n2).InnerSimplified),
                    (Tensor n1, var n2) => n1.Elementwise(n1 => (n1 + n2).InnerSimplified),
                    (var n1, Integer(0)) => n1,
                    (Integer(0), var n2) => n2,
                    (FiniteSet finite, var n2) when n2 is not Set => finite.Apply(c => (c + n2).InnerSimplified),
                    (var n2, FiniteSet finite) when n2 is not Set => finite.Apply(c => (n2 + c).InnerSimplified),
                    (Interval inter, var n2) when n2 is not Set => inter.New((inter.Left + n2).InnerSimplified, (inter.Right + n2).InnerSimplified),
                    (var n2, Interval inter) when n2 is not Set => inter.New((n2 + inter.Left).InnerSimplified, (n2 + inter.Right).InnerSimplified),
                    (var n1, var n2) => n1 == n2 ? (2 * n1).InnerSimplified : New(n1, n2)
                };
        }
        public partial record Minusf
        {
            protected override Entity InnerEval() => (Subtrahend.Evaled, Minuend.Evaled) switch
            {
                (Complex n1, Complex n2) => n1 - n2,
                (Tensor n1, Tensor n2) => n1.Elementwise(n2, (n1, n2) => (n1 - n2).Evaled),
                (var n1, Tensor n2) => n2.Elementwise(n2 => (n1 - n2).Evaled),
                (Tensor n1, var n2) => n1.Elementwise(n1 => (n1 - n2).Evaled),
                (FiniteSet finite, var n2) when n2 is not Set => finite.Apply(c => (c - n2).Evaled),
                (var n2, FiniteSet finite) when n2 is not Set => finite.Apply(c => (n2 - c).Evaled),
                (Interval inter, var n2) when n2 is not Set => inter.New((inter.Left - n2).Evaled, (inter.Right - n2).Evaled),
                (var n2, Interval inter) when n2 is not Set => inter.New((n2 - inter.Left).Evaled, (n2 - inter.Right).Evaled),
                (var n1, var n2) => New(n1, n2)
            };
            internal override Entity InnerSimplify() =>
                Evaled is Number { IsExact: true } ? Evaled : (Subtrahend.InnerSimplified, Minuend.InnerSimplified) switch
                {
                    (Tensor n1, Tensor n2) => n1.Elementwise(n2, (n1, n2) => (n1 - n2).InnerSimplified),
                    (var n1, Tensor n2) => n2.Elementwise(n2 => (n1 - n2).InnerSimplified),
                    (Tensor n1, var n2) => n1.Elementwise(n1 => (n1 - n2).InnerSimplified),
                    (var n1, Integer(0)) => n1,
                    (Integer(0), var n2) => (-n2).InnerSimplified,
                    (FiniteSet finite, var n2) when n2 is not Set => finite.Apply(c => (c - n2).InnerSimplified),
                    (var n2, FiniteSet finite) when n2 is not Set => finite.Apply(c => (n2 - c).InnerSimplified),
                    (Interval inter, var n2) when n2 is not Set => inter.New((inter.Left - n2).InnerSimplified, (inter.Right - n2).InnerSimplified),
                    (var n2, Interval inter) when n2 is not Set => inter.New((n2 - inter.Left).InnerSimplified, (n2 - inter.Right).InnerSimplified),
                    (var n1, var n2) => n1 == n2 ? (Entity)0 : New(n1, n2)
                };
        }
        public partial record Mulf
        {
            protected override Entity InnerEval() => (Multiplier.Evaled, Multiplicand.Evaled) switch
            {
                (Complex n1, Complex n2) => n1 * n2,
                (Tensor n1, Tensor n2) => n1.Elementwise(n2, (n1, n2) => (n1 * n2).Evaled),
                (var n1, Tensor n2) => n2.Elementwise(n2 => (n1 * n2).Evaled),
                (Tensor n1, var n2) => n1.Elementwise(n1 => (n1 * n2).Evaled),
                (FiniteSet finite, var n2) when n2 is not Set => finite.Apply(c => (c * n2).Evaled),
                (var n2, FiniteSet finite) when n2 is not Set => finite.Apply(c => (n2 * c).Evaled),
                (var n1, var n2) => New(n1, n2)
            };
            internal override Entity InnerSimplify() =>
                Evaled is Number { IsExact: true } ? Evaled : (Multiplier.InnerSimplified, Multiplicand.InnerSimplified) switch
                {
                    (Integer minusOne, Mulf(var minusOne1, var any1)) when minusOne == Integer.MinusOne && minusOne1 == Integer.MinusOne => any1,
                    (Tensor n1, Tensor n2) => n1.Elementwise(n2, (n1, n2) => (n1 * n2).InnerSimplified),
                    (var n1, Tensor n2) => n2.Elementwise(n2 => (n1 * n2).InnerSimplified),
                    (Tensor n1, var n2) => n1.Elementwise(n1 => (n1 * n2).InnerSimplified),
                    (_, Integer(0)) or (Integer(0), _) => 0,
                    (var n1, Integer(1)) => n1,
                    (Integer(1), var n2) => n2,
                    (FiniteSet finite, var n2) when n2 is not Set => finite.Apply(c => (c * n2).InnerSimplified),
                    (var n2, FiniteSet finite) when n2 is not Set => finite.Apply(c => (n2 * c).InnerSimplified),
                    (var n1, var n2) => n1 == n2 ? new Powf(n1, 2).InnerSimplified : New(n1, n2)
                };
        }
        public partial record Divf
        {
            protected override Entity InnerEval() => (Dividend.Evaled, Divisor.Evaled) switch
            {
                (Complex n1, Complex n2) => n1 / n2,
                (Tensor n1, Tensor n2) => n1.Elementwise(n2, (n1, n2) => (n1 / n2).Evaled),
                (var n1, Tensor n2) => n2.Elementwise(n2 => (n1 / n2).Evaled),
                (Tensor n1, var n2) => n1.Elementwise(n1 => (n1 / n2).Evaled),
                (FiniteSet finite, var n2) when n2 is not Set => finite.Apply(c => (c / n2).Evaled),
                (var n2, FiniteSet finite) when n2 is not Set => finite.Apply(c => (n2 / c).Evaled),
                (var n1, var n2) => New(n1, n2)
            };
            internal override Entity InnerSimplify() =>
                Evaled is Number { IsExact: true } ? Evaled : (Dividend.InnerSimplified, Divisor.InnerSimplified) switch
                {
                    (Tensor n1, Tensor n2) => n1.Elementwise(n2, (n1, n2) => (n1 / n2).InnerSimplified),
                    (var n1, Tensor n2) => n2.Elementwise(n2 => (n1 / n2).InnerSimplified),
                    (Tensor n1, var n2) => n1.Elementwise(n1 => (n1 / n2).InnerSimplified),
                    (Integer(0), _) => 0,
                    (_, Integer(0)) => Real.NaN,
                    (var n1, Integer(1)) => n1.InnerSimplified,
                    (FiniteSet finite, var n2) when n2 is not Set => finite.Apply(c => (c / n2).InnerSimplified),
                    (var n2, FiniteSet finite) when n2 is not Set => finite.Apply(c => (n2 / c).InnerSimplified),
                    (var n1, var n2) => n1 == n2 ? (Entity)1 : New(n1, n2)
                };
        }
        public partial record Powf
        {
            protected override Entity InnerEval() => (Base.Evaled, Exponent.Evaled) switch
            {
                (Complex n1, Complex n2) => Number.Pow(n1, n2),
                (Tensor n1, Tensor n2) => n1.Elementwise(n2, (n1, n2) => n1.Pow(n2).Evaled),
                (var n1, Tensor n2) => n2.Elementwise(n2 => n1.Pow(n2).Evaled),
                (Tensor n1, var n2) => n1.Elementwise(n1 => n1.Pow(n2).Evaled),
                (FiniteSet finite, var n2) when n2 is not Set => finite.Apply(c => c.Pow(n2).Evaled),
                (var n2, FiniteSet finite) when n2 is not Set => finite.Apply(c => n2.Pow(c).Evaled),
                (var n1, var n2) => New(n1, n2)
            };
            internal override Entity InnerSimplify() =>
                Evaled is Number { IsExact: true } ? Evaled : (Base.InnerSimplified, Exponent.InnerSimplified) switch
                {
                    (Tensor n1, Tensor n2) => n1.Elementwise(n2, (n1, n2) => n1.Pow(n2).InnerSimplified),
                    (var n1, Tensor n2) => n2.Elementwise(n2 => n1.Pow(n2).InnerSimplified),
                    (Tensor n1, var n2) => n1.Elementwise(n1 => n1.Pow(n2).InnerSimplified),
                // 0^x is undefined for Re(x) <= 0
                    (Integer(1), _) => 0,
                    (var n1, Integer(-1)) => (1 / n1).InnerSimplified,
                    (_, Integer(0)) => 1,
                    (var n1, Integer(1)) => n1.InnerSimplified,
                    (FiniteSet finite, var n2) when n2 is not Set => finite.Apply(c => c.Pow(n2).InnerSimplified),
                    (var n2, FiniteSet finite) when n2 is not Set => finite.Apply(c => n2.Pow(c).InnerSimplified),
                    (var n1, var n2) => New(n1, n2)
                };
        }
        public partial record Sinf
        {
            protected override Entity InnerEval() => Argument.Evaled switch
            {
                Complex n => Number.Sin(n),
                Tensor n => n.Elementwise(n => n.Sin().Evaled),
                FiniteSet finite => finite.Apply(c => c.Sin().Evaled),
                var n => New(n)
            };
            internal override Entity InnerSimplify() =>
                Evaled is Number { IsExact: true } ? Evaled : Argument.InnerSimplified switch
                {
                    Tensor n => n.Elementwise(n => n.Sin().InnerSimplified),
                    { Evaled: Complex n } when TrigonometryTableValues.PullSin(n, out var res) => res,
                    FiniteSet finite => finite.Apply(c => c.Sin().InnerSimplified),
                    var n => New(n)
                };
        }
        public partial record Cosf
        {
            protected override Entity InnerEval() => Argument.Evaled switch
            {
                Complex n => Number.Cos(n),
                Tensor n => n.Elementwise(n => n.Cos().Evaled),
                FiniteSet finite => finite.Apply(c => c.Cos().Evaled),
                var n => New(n)
            };
            internal override Entity InnerSimplify() =>
                Evaled is Number { IsExact: true } ? Evaled : Argument.InnerSimplified switch
                {
                    Tensor n => n.Elementwise(n => n.Cos().InnerSimplified),
                    { Evaled: Complex n } when TrigonometryTableValues.PullCos(n, out var res) => res,
                    FiniteSet finite => finite.Apply(c => c.Cos().InnerSimplified),
                    var n => New(n)
                };
        }
        public partial record Tanf
        {
            protected override Entity InnerEval() => Argument.Evaled switch
            {
                Complex n => Number.Tan(n),
                Tensor n => n.Elementwise(n => n.Tan().Evaled),
                FiniteSet finite => finite.Apply(c => c.Tan().Evaled),
                var n => New(n)
            };
            internal override Entity InnerSimplify() =>
                Evaled is Number { IsExact: true } ? Evaled : Argument.InnerSimplified switch
                {
                    Tensor n => n.Elementwise(n => n.Tan().InnerSimplified),
                    { Evaled: Complex n } when TrigonometryTableValues.PullTan(n, out var res) => res,
                    FiniteSet finite => finite.Apply(c => c.Tan().InnerSimplified),
                    var n => New(n)
                };
        }
        public partial record Cotanf
        {
            protected override Entity InnerEval() => Argument.Evaled switch
            {
                Complex n => Number.Cotan(n),
                Tensor n => n.Elementwise(n => n.Cotan().Evaled),
                FiniteSet finite => finite.Apply(c => c.Cotan().Evaled),
                var n => New(n)
            };
            internal override Entity InnerSimplify() =>
                Evaled is Number { IsExact: true } ? Evaled : Argument.InnerSimplified switch
                {
                    Tensor n => n.Elementwise(n => n.Cotan().InnerSimplified),
                    { Evaled: Complex n } when TrigonometryTableValues.PullTan(n, out var res) => 1 / res,
                    FiniteSet finite => finite.Apply(c => c.Cotan().InnerSimplified),
                    var n => New(n)
                };
        }
        public partial record Logf
        {
            protected override Entity InnerEval() => (Base.Evaled, Antilogarithm.Evaled) switch
            {
                (Complex n1, Complex n2) => Number.Log(n1, n2),
                (Tensor n1, Tensor n2) => n1.Elementwise(n2, (n1, n2) => n1.Log(n2).Evaled),
                (var n1, Tensor n2) => n2.Elementwise(n2 => n1.Log(n2).Evaled),
                (Tensor n1, var n2) => n1.Elementwise(n1 => n1.Log(n2).Evaled),
                (FiniteSet finite, var n2) when n2 is not Set => finite.Apply(c => MathS.Log(c, n2).Evaled),
                (var n2, FiniteSet finite) when n2 is not Set => finite.Apply(c => MathS.Log(n2, c).Evaled),
                (var n1, var n2) => New(n1, n2)
            };
            internal override Entity InnerSimplify() =>
                Evaled is Number { IsExact: true } ? Evaled : (Base.InnerSimplified, Antilogarithm.InnerSimplified) switch
                {
                    (Tensor n1, Tensor n2) => n1.Elementwise(n2, (n1, n2) => n1.Log(n2).InnerSimplified),
                    (var n1, Tensor n2) => n2.Elementwise(n2 => n1.Log(n2).InnerSimplified),
                    (Tensor n1, var n2) => n1.Elementwise(n1 => n1.Log(n2).InnerSimplified),
                    (_, Integer(0)) => Real.NegativeInfinity,
                    (_, Integer(1)) => 0,
                    (FiniteSet finite, var n2) when n2 is not Set => finite.Apply(c => MathS.Log(c, n2).InnerSimplified),
                    (var n2, FiniteSet finite) when n2 is not Set => finite.Apply(c => MathS.Log(n2, c).InnerSimplified),
                    (var n1, var n2) => n1 == n2 ? (Entity)1 : New(n1, n2)
                };
        }
        public partial record Arcsinf
        {
            protected override Entity InnerEval() => Argument.Evaled switch
            {
                Complex n => Number.Arcsin(n),
                Tensor n => n.Elementwise(n => n.Arcsin().Evaled),
                FiniteSet finite => finite.Apply(c => c.Arcsin().Evaled),
                var n => New(n)
            };
            internal override Entity InnerSimplify() =>
                Evaled is Number { IsExact: true } ? Evaled : Argument.InnerSimplified switch
                {
                    Tensor n => n.Elementwise(n => n.Arcsin().InnerSimplified),
                    FiniteSet finite => finite.Apply(c => c.Arcsin().InnerSimplified),
                    var n => New(n)
                };
        }
        public partial record Arccosf
        {
            protected override Entity InnerEval() => Argument.Evaled switch
            {
                Complex n => Number.Arccos(n),
                Tensor n => n.Elementwise(n => n.Arccos().Evaled),
                FiniteSet finite => finite.Apply(c => c.Arccos().Evaled),
                var n => New(n)
            };
            internal override Entity InnerSimplify() =>
                Evaled is Number { IsExact: true } ? Evaled : Argument.InnerSimplified switch
                {
                    Tensor n => n.Elementwise(n => n.Arccos().InnerSimplified),
                    FiniteSet finite => finite.Apply(c => c.Arccos().InnerSimplified),
                    var n => New(n)
                };
        }
        public partial record Arctanf
        {
            protected override Entity InnerEval() => Argument.Evaled switch
            {
                Complex n => Number.Arctan(n),
                Tensor n => n.Elementwise(n => n.Arctan().Evaled),
                FiniteSet finite => finite.Apply(c => c.Arctan().Evaled),
                var n => New(n)
            };
            internal override Entity InnerSimplify() =>
                Evaled is Number { IsExact: true } ? Evaled : Argument.InnerSimplified switch
                {
                    Tensor n => n.Elementwise(n => n.Arctan().InnerSimplified),
                    FiniteSet finite => finite.Apply(c => c.Arctan().InnerSimplified),
                    var n => New(n)
                };
        }
        public partial record Arccotanf
        {
            protected override Entity InnerEval() => Argument.Evaled switch
            {
                Complex n => Number.Arccotan(n),
                Tensor n => n.Elementwise(n => n.Arccotan().Evaled),
                FiniteSet finite => finite.Apply(c => c.Arccotan().Evaled),
                var n => New(n)
            };
            internal override Entity InnerSimplify() =>
                Evaled is Number { IsExact: true } ? Evaled : Argument.InnerSimplified switch
                {
                    Tensor n => n.Elementwise(n => n.Arccotan().InnerSimplified),
                    FiniteSet finite => finite.Apply(c => c.Arccotan().InnerSimplified),
                    var n => New(n)
                };
        }
        public partial record Factorialf
        {
            protected override Entity InnerEval() => Argument.Evaled switch
            {
                Complex n => Number.Factorial(n),
                Tensor n => n.Elementwise(n => n.Factorial().Evaled),
                FiniteSet finite => finite.Apply(c => c.Factorial().InnerSimplified),
                var n => New(n)
            };
            internal override Entity InnerSimplify() =>
                Evaled is Number { IsExact: true } ? Evaled : Argument.InnerSimplified switch
                {
                    Tensor n => n.Elementwise(n => n.Factorial().InnerSimplified),
                    FiniteSet finite => finite.Apply(c => c.Factorial().InnerSimplified),
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
            protected override Entity InnerEval() => (Expression.Evaled, Var.Evaled, Iterations) switch
            {
                (Tensor expr, var var, var iters) => expr.Elementwise(n => new Derivativef(n, var, iters).Evaled),
                (var expr, _, 0) => expr,
                // TODO: consider Integral for negative cases
                (var expr, Variable var, var asInt) => expr.Derive(var, asInt),
                _ => this
            };
            internal override Entity InnerSimplify() =>
                Var.InnerSimplified is Variable var
                ? Iterations == 0
                    ? Expression.InnerSimplified
                    : Expression.Derive(var, Iterations)
                : this;
        }
        public partial record Integralf
        {
            protected override Entity InnerEval() => (Expression.Evaled, Var.Evaled, Iterations) switch
            {
                (Tensor expr, var var, var iters) => expr.Elementwise(n => new Integralf(n, var, iters).Evaled),
                (var expr, _, 0) => expr,
                // TODO: consider Derivative for negative cases
                (var expr, Variable var, var asInt) =>
                    throw FutureReleaseException.Raised("Integration is not implemented yet", "1.2.3"),
                _ => this
            };
            internal override Entity InnerSimplify() =>
                Var.InnerSimplified is Variable
                ? Iterations == 0
                    ? Expression.InnerSimplified
                    : throw FutureReleaseException.Raised("Integration is not implemented yet", "1.2.3")
                : this;
        }
        public partial record Limitf
        {
            protected override Entity InnerEval() => ApproachFrom switch
            {
                ApproachFrom.Left => throw new NotImplementedException("Limits unevaluable"), // TODO
                ApproachFrom.BothSides => throw new NotImplementedException("Limits unevaluable"), // TODO
                ApproachFrom.Right => throw new NotImplementedException("Limits unevaluable"), // TODO
                _ => this,
            };
            internal override Entity InnerSimplify() =>
                Var.InnerSimplified switch
                {
                    Entity.Variable x => MathS.Compute.Limit(Expression.InnerSimplified, x, Destination.InnerSimplified, ApproachFrom) ?? this,
                    var x => new Limitf(Expression.InnerSimplified, x, Destination.InnerSimplified, ApproachFrom)
                };

        }

        public partial record Signumf
        {
            protected override Entity InnerEval()
                => Argument.Evaled switch
                {
                    Complex n => Complex.Signum(n),
                    Tensor n => n.Elementwise(c => c.Signum().Evaled),
                    FiniteSet finite => finite.Apply(c => c.Signum().InnerSimplified),
                    var n => this
                };

            // TODO: probably we can simplify it further
            internal override Entity InnerSimplify()
                => Argument.Evaled is Number { IsExact: true } ? Argument.Evaled :
                Argument.InnerSimplified switch
                {
                    Tensor n => n.Elementwise(c => c.Signum().InnerSimplified),
                    FiniteSet finite => finite.Apply(c => c.Signum().InnerSimplified),
                    var n => this
                };
        }

        public partial record Absf
        {
            protected override Entity InnerEval()
                => Argument.Evaled switch
                {
                    Complex n => Complex.Abs(n),
                    Tensor n => n.Elementwise(c => c.Abs().Evaled),
                    FiniteSet finite => finite.Apply(c => c.Abs().Evaled),
                    var n => this
                };

            // TODO: probably we can simplify it further
            internal override Entity InnerSimplify()
                => Argument.Evaled is Number { IsExact: true } ? Argument.Evaled :
                Argument.InnerSimplified switch
                {
                    Tensor n => n.Elementwise(c => c.Signum().InnerSimplified),
                    FiniteSet finite => finite.Apply(c => c.Abs().InnerSimplified),
                    var n => this
                };
        }
    }
}
