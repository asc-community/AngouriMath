//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Exceptions;
using static AngouriMath.Entity.Set;

namespace AngouriMath
{
    partial record Entity
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
            protected override Entity InnerEval() =>
                ExpandOnTwoArguments(Augend.Evaled, Addend.Evaled,
                    (augend, addend) => (augend, addend) switch
                    { 
                        (Complex a, Complex b) => a + b,
                        (Interval inter, var n2) when n2 is not Set => inter.New((inter.Left + n2).Evaled, (inter.Right + n2).Evaled),
                        (var n2, Interval inter) when n2 is not Set => inter.New((n2 + inter.Left).Evaled, (n2 + inter.Right).Evaled),
                        _ => null
                    },
                    (@this, a, b) => ((Sumf)@this).New(a, b)
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
                        (@this, a, b) => ((Sumf)@this).New(a, b),
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
                    (@this, a, b) => ((Minusf)@this).New(a, b)
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
                    (@this, a, b) => ((Minusf)@this).New(a, b),
                    true);
        }
        public partial record Mulf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() => ExpandOnTwoArguments(Multiplier.Evaled, Multiplicand.Evaled,
                (a, b) => (a, b) switch
                {
                    (Matrix m1, Matrix m2) when m1.ColumnCount == m2.RowCount => (m1 * m2).Evaled,
                    (Matrix m1, Matrix m2) => a * b,
                    (Matrix m, Integer(0)) => m.With((_, _, _) => 0),
                    (Integer(0), Matrix m) => m.With((_, _, _) => 0),
                    (Complex n1, Complex n2) => n1 * n2,
                    (_, Integer(0)) or (Integer(0), _) => 0,
                    (var n1, Integer(1)) => n1,
                    (Integer(1), var n2) => n2,
                    _ => null
                },
                (@this, a, b) => ((Mulf)@this).New(a, b)
                );
            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                ExpandOnTwoArguments(Multiplier.InnerSimplified, Multiplicand.InnerSimplified,
                    (a, b) => (a, b) switch
                    {
                        (Matrix m1, Matrix m2) when m1.ColumnCount == m2.RowCount => (m1 * m2).InnerSimplified,
                        (Matrix m1, Matrix m2) => a * b,
                        (Integer minusOne, Mulf(var minusOne1, var any1)) when minusOne == Integer.MinusOne && minusOne1 == Integer.MinusOne => any1,
                        (Matrix m, Integer(0)) => m.With((_, _, _) => 0),
                        (Integer(0), Matrix m) => m.With((_, _, _) => 0),
                        (_, Integer(0)) or (Integer(0), _) => 0,
                        (var n1, Integer(1)) => n1,
                        (Integer(1), var n2) => n2,
                        (var n1, var n2) when n1 == n2 => new Powf(n1, 2).InnerSimplified,
                        _ => null
                    },
                    (@this, a, b) => ((Mulf)@this).New(a, b),
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
                        (Integer(0), _) => 0,
                        (_, Integer(0)) => Real.NaN,
                        _ => null
                    },
                    (@this, a, b) => ((Divf)@this).New(a, b)
                    );
            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                ExpandOnTwoArguments(Dividend.InnerSimplified, Divisor.InnerSimplified,
                (a, b) => (a, b) switch
                {
                    (Integer(0), _) => 0,
                    (var n1, Integer(1)) => n1,
                    _ => null
                },
                (@this, a, b) => ((Divf)@this).New(a, b),
                true,
                allowNaNAsExact: false);
        }
        public partial record Powf
        {
            private static bool TryPower(Matrix m, int exp, out Entity res)
            {
                res = 0;
                try
                {
                    res = m.Pow(exp);
                    return true;
                }
                catch (InvalidMatrixOperationException)
                {
                    return false;
                }
            }

            /// <inheritdoc/>
            protected override Entity InnerEval() =>
                ExpandOnTwoArguments(Base.Evaled, Exponent.Evaled,
                    (a, b) => (a, b) switch
                {
                    (Matrix m, Integer(var exp)) when exp is { } expNotNull && TryPower(m, expNotNull, out var res) => res.Evaled,
                    (Complex n1, Complex n2) => Number.Pow(n1, n2),
                    (Integer(1), _) => 1,
                    (Integer(0), _) => 0,
                    (var n1, Integer(-1)) => (1 / n1).Evaled,
                    (_, Integer(0)) => 1,
                    _ => null
                },
                (@this, a, b) => ((Powf)@this).New(a, b)
                );

            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                ExpandOnTwoArguments(Base.InnerSimplified, Exponent.InnerSimplified,
                (a, b) => (a, b) switch
                {
                    (Matrix m, Integer(var exp)) when exp is { } expNotNull && TryPower(m, expNotNull, out var res) => res.InnerSimplified,
                    (Integer(1), _) => 1,
                    (Integer(0), Integer(0)) => 1,
                    (Integer(0), _) => 0,
                    (var n1, Integer(-1)) => (1 / n1).InnerSimplified,
                    (_, Integer(0)) => 1,
                    (var n1, Integer(1)) => n1,
                    _ => null
                },
                (@this, a, b) => ((Powf)@this).New(a, b),
                true);
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
                    (@this, a, b) => ((Logf)@this).New(a, b)
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
                    (@this, a, b) => ((Logf)@this).New(a, b),
                    true);
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
                    (@this, a) => ((Factorialf)@this).New(a)
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
                    (@this, a) => ((Factorialf)@this).New(a)
                    , true);
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
                    (@this, a) => ((Signumf)@this).New(a)
                    );

            // TODO: probably we can simplify it further
            /// <inheritdoc/>
            protected override Entity InnerSimplify()
                => ExpandOnOneArgument(Argument.InnerSimplified,
                    a => a switch
                    {
                        _ => null
                    },
                    (@this, a) => ((Signumf)@this).New(a)
                    , true);
        }

        public partial record Absf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval()
                => ExpandOnOneArgument(Argument.Evaled,
                    a => a switch
                    {
                        Matrix m when m.IsVector => Sumf.Sum(m.Select(c => c.Pow(2))).Pow(0.5).Evaled,
                        Complex n => Number.Abs(n),
                        _ => null
                    },
                    (@this, a) => ((Absf)@this).New(a)
                    );

            // TODO: probably we can simplify it further
            /// <inheritdoc/>
            protected override Entity InnerSimplify()
                => ExpandOnOneArgument(Argument.InnerSimplified,
                    a => a switch
                    {
                        Matrix m when m.IsVector => Sumf.Sum(m.Select(c => c.Pow(2))).Pow(0.5).InnerSimplified,
                        _ => null
                    },
                    (@this, a) => ((Absf)@this).New(a)
                    , true);
        }
    }
}
