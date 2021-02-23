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
            // /// <inheritdoc/>
            // protected override Entity InnerEval() => (Augend, Addend).Unpack2Eval() switch
            // {
            //     (Complex n1, Complex n2) => n1 + n2,
            //     (Tensor n1, Tensor n2) => n1.Elementwise(n2, (n1, n2) => (n1 + n2)),
            //     (var n1, Tensor n2) => n2.Elementwise(n2 => (n1 + n2)),
            //     (Tensor n1, var n2) => n1.Elementwise(n1 => (n1 + n2)),
            //     (FiniteSet finite, var n2) when n2 is not Set => finite.Apply(c => (c + n2)),
            //     (var n2, FiniteSet finite) when n2 is not Set => finite.Apply(c => (n2 + c)),
            //     (Interval inter, var n2) when n2 is not Set => inter.New((inter.Left + n2), (inter.Right + n2)),
            //     (var n2, Interval inter) when n2 is not Set => inter.New((n2 + inter.Left), (n2 + inter.Right)),
            //     (var n1, var n2) => New(n1, n2)
            // };
            /// <inheritdoc/>
            protected override Entity InnerEval() =>
                ExpandOnTwoArguments(Augend.Evaled, Addend.Evaled,
                    (augend, addend) => (augend, addend) switch
                    { 
                        (Complex a, Complex b) => a + b,
                        (Interval inter, var n2) when n2 is not Set => inter.New((inter.Left + n2).Evaled, (inter.Right + n2).Evaled),
                        (var n2, Interval inter) when n2 is not Set => inter.New((n2 + inter.Left).Evaled, (n2 + inter.Right).Evaled),
                        _ => null
                    },
                    (a, b) => a + b
                    );


            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                    ExpandOnTwoArguments(Augend.InnerSimplified, Addend.InnerSimplified,
                        (augend, addend) => (augend, addend) switch
                        {
                            (Complex a, Complex b) => a + b,
                            (var n1, Integer(0)) => n1,
                            (Integer(0), var n2) => n2,
                            (Interval inter, var n2) when n2 is not Set => inter.New((inter.Left + n2).InnerSimplified, (inter.Right + n2).InnerSimplified),
                            (var n2, Interval inter) when n2 is not Set => inter.New((n2 + inter.Left).InnerSimplified, (n2 + inter.Right).InnerSimplified),
                            _ => null
                        },
                        (a, b) => a + b,
                        true);
        }
        public partial record Minusf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() => ExpandOnTwoArguments(Subtrahend.Evaled, Minuend.Evaled,
                    (augend, addend) => (augend, addend) switch
                    {
                        (Complex a, Complex b) => a - b,
                        (Interval inter, var n2) when n2 is not Set => inter.New((inter.Left - n2).Evaled, (inter.Right - n2).Evaled),
                        (var n2, Interval inter) when n2 is not Set => inter.New((n2 - inter.Left).Evaled, (n2 - inter.Right).Evaled),
                        _ => null
                    },
                    (a, b) => a - b
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                ExpandOnTwoArguments(Subtrahend.InnerSimplified, Minuend.InnerSimplified,
                    (augend, addend) => (augend, addend) switch
                    {
                        (var n1, Integer(0)) => n1,
                        (Integer(0), var n2) => (-n2),
                        (Interval inter, var n2) when n2 is not Set => inter.New((inter.Left - n2).InnerSimplified, (inter.Right - n2).InnerSimplified),
                        (var n2, Interval inter) when n2 is not Set => inter.New((n2 - inter.Left).InnerSimplified, (n2 - inter.Right).InnerSimplified),
                        _ => null
                    },
                    (a, b) => a - b,
                    true);
        }
        public partial record Mulf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() => ExpandOnTwoArguments(Multiplier.Evaled, Multiplicand.Evaled,
                (a, b) => (a, b) switch
                {
                    (Complex n1, Complex n2) => n1 * n2,
                    _ => null
                },
                (a, b) => a * b
                );
            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                ExpandOnTwoArguments(Multiplier.InnerSimplified, Multiplicand.InnerSimplified,
                    (a, b) => (a, b) switch
                    {
                        (Integer minusOne, Mulf(var minusOne1, var any1)) when minusOne == Integer.MinusOne && minusOne1 == Integer.MinusOne => any1,
                        (_, Integer(0)) or (Integer(0), _) => 0,
                        (var n1, Integer(1)) => n1,
                        (Integer(1), var n2) => n2,
                        (var n1, var n2) when n1 == n2 => new Powf(n1, 2).InnerSimplified,
                        _ => null
                    },
                    (a, b) => a * b,
                    true);
        }
        public partial record Divf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() =>
                ExpandOnTwoArguments(Dividend.Evaled, Divisor.Evaled,
                    (a, b) => (a, b) switch
                    {
                        (Complex n1, Complex n2) => n1 / n2,
                        _ => null
                    }, 
                    (a, b) => a / b
                    );
            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                ExpandOnTwoArguments(Dividend.InnerSimplified, Divisor.InnerSimplified,
                (a, b) => (a, b) switch
                {
                    (Integer(0), _) => 0,
                    (_, Integer(0)) => Real.NaN,
                    (var n1, Integer(1)) => n1,
                    _ => null
                },
                (a, b) => a / b,
                true);
        }
        public partial record Powf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() =>
                ExpandOnTwoArguments(Base.Evaled, Exponent.Evaled,
                    (a, b) => (a, b) switch
                {
                    (Complex n1, Complex n2) => Number.Pow(n1, n2),
                    _ => null
                },
                (a, b) => a.Pow(b)
                );

            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                ExpandOnTwoArguments(Base.InnerSimplified, Exponent.InnerSimplified,
                (a, b) => (a, b) switch
                {
                    (Integer(1), _) => 1,
                    (Integer(0), Integer(0)) => 1,
                    (Integer(0), _) => 0,
                    (var n1, Integer(-1)) => (1 / n1).InnerSimplified,
                    (_, Integer(0)) => 1,
                    (var n1, Integer(1)) => n1,
                    _ => null
                },
                (a, b) => a.Pow(b),
                true);
        }
        public partial record Sinf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() => 
                ExpandOnOneArgument(Argument.Evaled,
                    a => a switch
                    {
                        Complex n => Number.Sin(n),
                        _ => null
                    },
                    a => a.Sin()
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                ExpandOnOneArgument(Argument.InnerSimplified,
                    a => a switch
                    {
                        { Evaled: Complex n } when TrigonometryTableValues.PullSin(n, out var res) => res,
                        _ => null
                    },
                    a => a.Sin(),
                    true);
        }

        public partial record Cosf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() =>
                ExpandOnOneArgument(Argument.Evaled,
                    a => a switch
                    {
                        Complex n => Number.Cos(n),
                        _ => null
                    },
                    a => a.Cos()
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                ExpandOnOneArgument(Argument.InnerSimplified,
                    a => a switch
                    {
                        { Evaled: Complex n } when TrigonometryTableValues.PullCos(n, out var res) => res,
                        _ => null
                    },
                    a => a.Cos(),
                    true);
        }

        public partial record Secantf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() =>
                ExpandOnOneArgument(Argument.Evaled,
                    a => a switch
                    {
                        Complex n => Number.Secant(n),
                        _ => null
                    },
                    a => a.Sec()
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                ExpandOnOneArgument(Argument.InnerSimplified,
                    a => a switch
                    {
                        { Evaled: Complex n } when TrigonometryTableValues.PullCos(n, out var res) => (1 / res).InnerSimplified,
                        _ => null
                    },
                    a => a.Sec(),
                    true);
        }

        public partial record Cosecantf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() =>
                ExpandOnOneArgument(Argument.Evaled,
                    a => a switch
                    {
                        Complex n => Number.Cosecant(n),
                        _ => null
                    },
                    a => a.Cosec()
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                ExpandOnOneArgument(Argument.InnerSimplified,
                    a => a switch
                    {
                        { Evaled: Complex n } when TrigonometryTableValues.PullSin(n, out var res) => (1 / res).InnerSimplified,
                        _ => null
                    },
                    a => a.Cosec()
                    , true);
        }

        public partial record Arcsecantf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() =>
                ExpandOnOneArgument(Argument.Evaled,
                    a => a switch
                    {
                        Complex n => Number.Arcsecant(n),
                        _ => null
                    },
                    a => a.Arcsec()
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                ExpandOnOneArgument(Argument.InnerSimplified,
                    a => a switch
                    {
                        _ => null
                    },
                    a => a.Arcsec(),
                    true);
        }

        public partial record Arccosecantf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() =>
                ExpandOnOneArgument(Argument.Evaled,
                    a => a switch
                    {
                        Complex n => Number.Arccosecant(n),
                        _ => null
                    },
                    a => a.Arccosec()
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                ExpandOnOneArgument(Argument.InnerSimplified,
                    a => a switch
                    {
                        _ => null
                    },
                    a => a.Arccosec(),
                    true);
        }

        public partial record Tanf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() =>
                ExpandOnOneArgument(Argument.Evaled,
                    a => a switch
                    {
                        Complex n => Number.Tan(n),
                        _ => null
                    },
                    a => a.Tan()
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                ExpandOnOneArgument(Argument.InnerSimplified,
                    a => a switch
                    {
                        { Evaled: Complex n } when TrigonometryTableValues.PullTan(n, out var res) => res,
                        _ => null
                    },
                    a => a.Tan()
                    , true);
        }
        public partial record Cotanf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() =>
                ExpandOnOneArgument(Argument.Evaled,
                    a => a switch
                    {
                        Complex n => Number.Cotan(n),
                        _ => null
                    },
                    a => a.Cotan()
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                ExpandOnOneArgument(Argument.InnerSimplified,
                    a => a switch
                    {
                        { Evaled: Complex n } when TrigonometryTableValues.PullTan(n, out var res) => (1 / res).InnerSimplified,
                        _ => null
                    },
                    a => a.Cotan()
                    , true);
        }
        public partial record Logf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() => 
                ExpandOnTwoArguments(Base.Evaled, Antilogarithm.Evaled,
                    (a, b) => (a, b) switch
                    {
                        (Complex n1, Complex n2) => Number.Log(n1, n2),
                        _ => null
                    },
                    (a, b) => a.Log(b)
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                ExpandOnTwoArguments(Base.InnerSimplified, Antilogarithm.InnerSimplified,
                (a, b) => (a, b) switch
                {
                    (_, Integer(0)) => Real.NegativeInfinity,
                    (_, Integer(1)) => 0,
                    _ => null
                },
                (a, b) => a.Log(b),
                    true);
        }
        public partial record Arcsinf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() => 
                ExpandOnOneArgument(Argument.Evaled, 
                a => a switch
                {
                    Complex n => Number.Arcsin(n),
                    _ => null
                },
                a => a.Arcsin()
                );
            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                ExpandOnOneArgument(Argument.InnerSimplified,
                a => a switch
                {
                    _ => null
                },
                a => a.Arcsin(), 
                    true);
        }
        public partial record Arccosf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() =>
                ExpandOnOneArgument(Argument.Evaled,
                a => a switch
                {
                    Complex n => Number.Arccos(n),
                    _ => null
                },
                a => a.Arccos()
                );
            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                ExpandOnOneArgument(Argument.InnerSimplified,
                a => a switch
                {
                    _ => null
                },
                a => a.Arccos(), 
                    true);
        }
        public partial record Arctanf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() =>
                ExpandOnOneArgument(Argument.Evaled,
                a => a switch
                {
                    Complex n => Number.Arctan(n),
                    _ => null
                },
                a => a.Arctan()
                );
            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                ExpandOnOneArgument(Argument.InnerSimplified,
                a => a switch
                {
                    _ => null
                },
                a => a.Arctan()
                , true);
        }
        public partial record Arccotanf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() =>
                ExpandOnOneArgument(Argument.Evaled,
                a => a switch
                {
                    Complex n => Number.Arccotan(n),
                    _ => null
                },
                a => a.Arccotan()
                );
            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                ExpandOnOneArgument(Argument.InnerSimplified,
                a => a switch
                {
                    _ => null
                },
                a => a.Arccotan()
                , true);
        }
        public partial record Factorialf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() => 
                ExpandOnOneArgument(Argument.Evaled,
                    a => a switch                
                    {
                        Complex n => Number.Factorial(n),
                        _ => null
                    },
                    a => a.Factorial()
                    );
            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                ExpandOnOneArgument(Argument.InnerSimplified,
                    a => a switch
                    {
                        Rational({ Numerator: var num, Denominator: var den }) when den.Equals(2) && (num + 1) / 2 is var en => (
                            en > 0
                            // (+n - 1/2)! = (2n-1)!/(2^(2n-1)(n-1)!)*sqrt(pi)
                            // also 2n-1 is the numerator
                            ? Rational.Create(num.Factorial(), PeterO.Numbers.EInteger.FromInt32(2).Pow(num) * (en - 1).Factorial())
                            // (-n - 1/2)! = (-4)^n*n!/(2n)!*sqrt(pi)
                            : Rational.Create(PeterO.Numbers.EInteger.FromInt32(-4).Pow(-en) * (-en).Factorial(), (2 * -en).Factorial())
                        ) * MathS.Sqrt(MathS.pi),
                        _ => null
                    },
                    a => a.Factorial()
                    , true);
        }
        public partial record Derivativef
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() =>
                ExpandOnTwoAndTArguments(Expression.Evaled, Var.Evaled, Iterations,
                    (a, b, c) => (a, b, c) switch
                    {
                        (var expr, _, 0) => expr,
                        // TODO: consider Integral for negative cases
                        // TODO: should we call InnerSimlified here?
                        (var expr, Variable var, var asInt) => expr.Derive(var, asInt),
                        _ => null
                    },
                    (a, b, c) => new Derivativef(a, b, c)
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                Var is Variable var
                ? Iterations == 0
                    ? Expression
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
                    changed = Integration.ComputeIndefiniteIntegral(changed, var);
                return changed;
            }

            /// <inheritdoc/>
            protected override Entity InnerEval() =>
                ExpandOnTwoAndTArguments(Expression.Evaled, Var.Evaled, Iterations,
                    (a, b, c) => (a, b, c) switch
                    {
                        (var expr, _, 0) => expr,
                        // TODO: consider Derivative for negative cases
                        (var expr, Variable var, int asInt) => SequentialIntegrating(expr, var, asInt),
                        _ => null
                    },
                    (a, b, c) => new Integralf(a, b, c)
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
               Var is Variable var ?

                ExpandOnTwoAndTArguments(Expression.InnerSimplified, Var, Iterations,
                    (a, b, c) => (a, b, c) switch
                    {
                        (var expr, _, 0) => expr,
                        // TODO: consider Derivative for negative cases
                        // TODO: should we apply InnerSimplified?
                        (var expr, Variable var, int asInt) => SequentialIntegrating(expr, var, asInt),
                        _ => null
                    },
                    (a, b, c) => new Integralf(a, b, c)
                    )

                : this;
        }


        // TODO: rewrite this part too
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
                Var switch
                {
                    // if it cannot compute it, it will anyway return the node
                    Variable x => Expression.InnerSimplified.Limit(x, Destination.InnerSimplified, ApproachFrom),
                    var x => new Limitf(Expression.InnerSimplified, x, Destination.InnerSimplified, ApproachFrom)
                };

        }

        public partial record Signumf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval()
                => ExpandOnOneArgument(Argument.Evaled,
                    a => a switch
                    {
                        Complex n => Number.Signum(n),
                        _ => null
                    },
                    a => a.Signum()
                    );

            // TODO: probably we can simplify it further
            /// <inheritdoc/>
            protected override Entity InnerSimplify()
                => ExpandOnOneArgument(Argument.InnerSimplified,
                        a => a switch
                        {
                            _ => null
                        },
                        a => a.Signum()
                        , true);
        }

        public partial record Absf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval()
                => ExpandOnOneArgument(Argument.Evaled,
                    a => a switch
                    {
                        Complex n => Number.Abs(n),
                        _ => null
                    },
                    a => a.Abs()
                    );

            // TODO: probably we can simplify it further
            /// <inheritdoc/>
            protected override Entity InnerSimplify()
                   => ExpandOnOneArgument(Argument.InnerSimplified,
                        a => a switch
                        {
                            _ => null
                        },
                        a => a.Abs()
                        , true);
        }
    }
}
