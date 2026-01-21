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
            private protected override Entity IntrinsicCondition => Boolean.True;
            
            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact) => this;
        }

        // Each function and operator processing
        public partial record Sumf
        {
            private protected override Entity IntrinsicCondition => Boolean.True;
            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact) =>
                ExpandOnTwoArguments(Augend, Addend,
                    (augend, addend) => (augend, addend) switch
                    { 
                        (Complex a, Complex b) when !isExact => a + b,
                        (var n1, Integer(0)) => n1,
                        (Integer(0), var n2) => n2,
                        (Interval inter, var n2) when n2 is not Set => inter.New((inter.Left + n2).InnerSimplified(isExact), (inter.Right + n2).InnerSimplified(isExact)),
                        (var n2, Interval inter) when n2 is not Set => inter.New((n2 + inter.Left).InnerSimplified(isExact), (n2 + inter.Right).InnerSimplified(isExact)),
                        _ => null
                    },
                    (@this, a, b) => ((Sumf)@this).New(a, b), isExact);
        }
        public partial record Minusf
        {
            private protected override Entity IntrinsicCondition => Boolean.True;

            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact) =>
                ExpandOnTwoArguments(Subtrahend, Minuend,
                    (augend, addend) => (augend, addend) switch
                    {
                        (Complex a, Complex b) when !isExact => a - b,
                        (var n1, Integer(0)) => n1,
                        (Integer(0), var n2) => -n2,
                        (Interval inter, var n2) when n2 is not Set => inter.New((inter.Left - n2).InnerSimplified(isExact), (inter.Right - n2).InnerSimplified(isExact)),
                        (var n2, Interval inter) when n2 is not Set => inter.New((n2 - inter.Left).InnerSimplified(isExact), (n2 - inter.Right).InnerSimplified(isExact)),
                        _ => null
                    },
                    (@this, a, b) => ((Minusf)@this).New(a, b), isExact);
        }
        public partial record Mulf
        {
            private protected override Entity IntrinsicCondition => Boolean.True;
            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact) =>
                ExpandOnTwoArguments(Multiplier, Multiplicand,
                    (a, b) => (a, b) switch
                    {
                        (Matrix m1, Matrix m2) when m1.ColumnCount == m2.RowCount => (m1 * m2).InnerSimplified(isExact),
                        (Matrix m1, Matrix m2) => a * b,
                        (Integer(-1), Mulf(Integer(-1), var any1)) => any1,
                        (Matrix m, Integer(0)) => m.With((_, _, _) => 0),
                        (Integer(0), Matrix m) => m.With((_, _, _) => 0),
                        (Complex n1, Complex n2) when !isExact => n1 * n2,
                        ({ DomainCondition: var condition }, Integer(0)) => Integer.Zero.Provided(condition),
                        (Integer(0), { DomainCondition: var condition }) => Integer.Zero.Provided(condition),
                        (var n1, Integer(1)) => n1,
                        (Integer(1), var n2) => n2,
                        (var n1, var n2) when n1 == n2 => new Powf(n1, 2).InnerSimplified(isExact),
                        _ => null
                    },
                    (@this, a, b) => ((Mulf)@this).New(a, b), isExact);
        }
        public partial record Divf
        {
            // Division is undefined when the divisor equals zero
            private protected override Entity IntrinsicCondition => !Divisor.EqualTo(0);

            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact) =>
                ExpandOnTwoArguments(Dividend, Divisor,
                (a, b) => (a, b) switch
                {
                    (Integer(0), var n0) =>
                        n0.Evaled is Complex c
                        ? c.IsZero ? MathS.NaN : 0
                        : new Providedf(0, new Notf(new Equalsf(n0, 0))),
                    (_, Integer(0)) => Real.NaN,
                    (Complex n1, Complex n2) when !isExact => n1 / n2,
                    (var n1, Integer(1)) => n1,
                    _ => null
                },
                (@this, a, b) => ((Divf)@this).New(a, b), isExact);
        }
        public partial record Powf
        {
            // Power is undefined in two cases:
            // - 0^0 is indeterminate
            // - 0^(negative) is undefined (division by zero)
            private protected override Entity IntrinsicCondition => 
                (!Base.EqualTo(0) | Exponent > 0);
            
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

            // Re(x) = x/2 * (1 + 1/sgn(x)^2)
            internal static Entity Re(Entity x) => x / 2 * (1 + 1 / new Signumf(x).Pow(2));

            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact) =>
                ExpandOnTwoArguments(Base, Exponent,
                (a, b) => (a, b) switch
                {
                    (Matrix m, Integer(var exp)) when exp is { } expNotNull && TryPower(m, expNotNull, out var res) => res.InnerSimplified(isExact),
                    (Integer(0), var x) =>
                        (isExact ? x.Evaled : x) is Complex c
                        ? c.RealPart.IsPositive
                          ? 0
                          : MathS.NaN
                        : new Providedf(0, Re(x) > 0),
                    (Complex n1, Complex n2) when !isExact => Number.Pow(n1, n2), // returns 1 for 0^0 and 0 for 0^(negative real part), we need to handle these cases above
                    (Integer(1), { DomainCondition: var condition }) => Integer.One.Provided(condition),
                    (var n1, Integer(-1)) => (1 / n1).InnerSimplified(isExact),
                    (var x, Integer(0)) =>
                        x.Evaled is Complex c
                        ? c.IsZero
                          ? throw new AngouriBugException("Should have already been handled by the above case")
                          : 1
                        : new Providedf(1, !x.EqualTo(0)),
                    (var n1, Integer(1)) => n1,
                    _ => null
                },
                (@this, a, b) => ((Powf)@this).New(a, b), isExact);
        }
        
        public partial record Logf
        {
            // Logarithm is undefined when:
            // - base <= 0 or base = 1
            // - antilogarithm <= 0
            // For complex logarithms, we use the principal branch
            private protected override Entity IntrinsicCondition => 
                Base > 0 & !Base.EqualTo(1) & Antilogarithm > 0;
            
            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact) => 
                ExpandOnTwoArguments(Base, Antilogarithm,
                    (a, b) => (a, b) switch
                    {
                        (Complex n1, Complex n2) when !isExact => Number.Log(n1, n2),
                        ({ DomainCondition: var condition }, Integer(0)) => Real.NegativeInfinity.Provided(condition),
                        ({ DomainCondition: var condition }, Integer(1)) => Integer.Zero.Provided(condition),
                        _ => null
                    },
                    (@this, a, b) => ((Logf)@this).New(a, b), isExact);
        }
        
        public partial record Factorialf
        {
            // Factorial is defined for non-negative integers in the traditional sense,
            // but extends to complex numbers via the gamma function: n! = Γ(n+1)
            // The gamma function has poles at negative integers, so factorial is undefined there
            private protected override Entity IntrinsicCondition => 
                Argument.In(MathS.Sets.R) & (Argument >= 0 | !Argument.In(MathS.Sets.Z));

            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact) =>
                ExpandOnOneArgument(Argument,
                    a => a switch
                    {
                        Complex c when !isExact => Number.Factorial(c),
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
                    (@this, a) => ((Factorialf)@this).New(a), isExact);
        }

        public partial record Signumf
        {
            // Signum is defined everywhere in the complex plane
            // sgn(0) = 0, sgn(z) = z/|z| for z ≠ 0
            private protected override Entity IntrinsicCondition => Boolean.True;

            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact)
                => ExpandOnOneArgument(Argument,
                    a => a switch
                    {
                        Real n => n.EDecimal.Sign,
                        Complex n when !isExact => Number.Signum(n),
                        Absf({ DomainCondition: var condition }) => Integer.One.Provided(condition),
                        Signumf signum => signum,
                        _ => null
                    },
                    (@this, a) => ((Signumf)@this).New(a), isExact);
        }

        public partial record Absf
        {
            // Absolute value is defined everywhere in the complex plane
            // For complex z, |z| = sqrt(Re(z)^2 + Im(z)^2)
            private protected override Entity IntrinsicCondition => Boolean.True;

            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact)
                => ExpandOnOneArgument(Argument,
                    a => a switch
                    {
                        Matrix m when m.IsVector => Sumf.Sum(m.Select(c => c.Pow(2))).Pow(0.5).InnerSimplified,
                        Complex n when !isExact => Number.Abs(n),
                        Absf abs => abs,
                        Signumf({ DomainCondition: var condition }) => Integer.One.Provided(condition),
                        _ => null
                    },
                    (@this, a) => ((Absf)@this).New(a), isExact);
        }
    }
}
