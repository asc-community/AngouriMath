//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using PeterO.Numbers;
using static AngouriMath.Entity;

namespace AngouriMath.Numerics
{
    /// <summary>
    /// An interval of decimals at a working precision the evaluation carries: every operation
    /// rounds its lower bound down and its upper bound up in that many digits, so the exact value
    /// is inside. <see cref="Interval"/>'s second tier, for where doubles cannot tell.
    /// </summary>
    internal readonly struct PreciseInterval
    {
        internal readonly EDecimal Low;
        internal readonly EDecimal High;

        internal PreciseInterval(EDecimal low, EDecimal high)
        {
            Low = low;
            High = high;
        }

        internal static PreciseInterval Exactly(EDecimal value) => new(value, value);

        internal bool IsFinite => Low.IsFinite && High.IsFinite;
        internal bool IsZero => Low.IsZero && High.IsZero;
        internal bool ContainsZero => Low.Sign <= 0 && High.Sign >= 0;
        internal EDecimal Magnitude => EDecimal.Max(Low.Abs(), High.Abs());
        internal EDecimal Mignitude => ContainsZero ? EDecimal.Zero : EDecimal.Min(Low.Abs(), High.Abs());

        internal PreciseInterval Negate() => new(High.Negate(), Low.Negate());
    }

    /// <summary>A rectangle of two <see cref="PreciseInterval"/>s.</summary>
    internal readonly struct PreciseComplexInterval
    {
        internal readonly PreciseInterval Re;
        internal readonly PreciseInterval Im;

        internal PreciseComplexInterval(PreciseInterval re, PreciseInterval im)
        {
            Re = re;
            Im = im;
        }

        internal bool IsFinite => Re.IsFinite && Im.IsFinite;
        internal bool IsReal => Im.IsZero;
    }

    /// <summary>
    /// <see cref="IntervalEvaluation"/> at a requested number of significant digits, for the
    /// points where double intervals are too wide to tell -- the digits lost to cancellation --
    /// or land on a cut by a rounding. The precision is an argument, never a setting.
    /// </summary>
    internal sealed class PreciseEvaluation
    {
        private readonly int digits;
        private readonly EContext down;
        private readonly EContext up;
        private readonly EContext near;
        private readonly EDecimal unit;
        private readonly PreciseComplexInterval undefined =
            new(new(EDecimal.NaN, EDecimal.NaN), new(EDecimal.NaN, EDecimal.NaN));

        private PreciseEvaluation(int digits)
        {
            this.digits = digits;
            down = EContext.ForPrecisionAndRounding(digits, ERounding.Floor);
            up = EContext.ForPrecisionAndRounding(digits, ERounding.Ceiling);
            near = EContext.ForPrecisionAndRounding(digits + 10, ERounding.HalfEven);
            unit = EDecimal.FromString("1E-" + (digits - 2));
        }

        /// <summary>The interval <paramref name="expr"/> evaluates to in <paramref name="digits"/> digits, or null.</summary>
        internal static PreciseComplexInterval? Of(Entity expr, int digits)
        {
            var value = new PreciseEvaluation(digits).Evaluate(expr);
            return value.IsFinite ? value : null;
        }

        /// <summary>The three-way agreement of <see cref="IntervalEvaluation.Agree"/>, in decimals.</summary>
        internal static bool? Agree(PreciseComplexInterval left, PreciseComplexInterval right, double relativeTolerance)
        {
            var context = EContext.ForPrecision(30);
            var reDifference = new PreciseInterval(left.Re.Low.Subtract(right.Re.High, context), left.Re.High.Subtract(right.Re.Low, context));
            var imDifference = new PreciseInterval(left.Im.Low.Subtract(right.Im.High, context), left.Im.High.Subtract(right.Im.Low, context));
            var scale = EDecimal.Max(EDecimal.One, EDecimal.Max(
                EDecimal.Max(left.Re.Magnitude, left.Im.Magnitude), EDecimal.Max(right.Re.Magnitude, right.Im.Magnitude)));
            var allowed = scale.Multiply(EDecimal.FromDouble(relativeTolerance), context);
            var largest = EDecimal.Max(reDifference.Magnitude, imDifference.Magnitude);
            if (largest.CompareTo(allowed) <= 0)
                return true;
            var smallest = EDecimal.Max(reDifference.Mignitude, imDifference.Mignitude);
            if (smallest.CompareTo(allowed) > 0)
                return false;
            return null;
        }

        // ---- real intervals -------------------------------------------------------------

        private PreciseInterval Add(PreciseInterval a, PreciseInterval b)
            => a.IsZero ? b : b.IsZero ? a : new(a.Low.Add(b.Low, down), a.High.Add(b.High, up));

        private PreciseInterval Subtract(PreciseInterval a, PreciseInterval b) => Add(a, b.Negate());

        private PreciseInterval Multiply(PreciseInterval a, PreciseInterval b)
        {
            if (a.IsZero || b.IsZero)
                return PreciseInterval.Exactly(EDecimal.Zero);
            var lows = new[] { a.Low.Multiply(b.Low, down), a.Low.Multiply(b.High, down), a.High.Multiply(b.Low, down), a.High.Multiply(b.High, down) };
            var highs = new[] { a.Low.Multiply(b.Low, up), a.Low.Multiply(b.High, up), a.High.Multiply(b.Low, up), a.High.Multiply(b.High, up) };
            return new(Min(lows), Max(highs));
        }

        private PreciseInterval Divide(PreciseInterval a, PreciseInterval b)
        {
            if (b.ContainsZero)
                return new(EDecimal.NaN, EDecimal.NaN);
            if (a.IsZero)
                return PreciseInterval.Exactly(EDecimal.Zero);
            var lows = new[] { a.Low.Divide(b.Low, down), a.Low.Divide(b.High, down), a.High.Divide(b.Low, down), a.High.Divide(b.High, down) };
            var highs = new[] { a.Low.Divide(b.Low, up), a.Low.Divide(b.High, up), a.High.Divide(b.Low, up), a.High.Divide(b.High, up) };
            return new(Min(lows), Max(highs));
        }

        private PreciseInterval Square(PreciseInterval a)
        {
            if (a.ContainsZero)
                return new(EDecimal.Zero, EDecimal.Max(a.Low.Multiply(a.Low, up), a.High.Multiply(a.High, up)));
            var small = a.Mignitude;
            var large = a.Magnitude;
            return new(small.Multiply(small, down), large.Multiply(large, up));
        }

        /// <summary>
        /// A function's value over the interval, from its value at the middle, computed with ten
        /// digits to spare, and a bound on its slope there: the value moves no further than the
        /// slope times the half-width, and the evaluation's own error is a unit or two in the last
        /// place of the working precision.
        /// </summary>
        private PreciseInterval AroundTheMiddle(PreciseInterval a, Func<EDecimal, EDecimal> function, EDecimal slope)
        {
            var middle = a.Low.Add(a.High, near).Divide(EDecimal.FromInt32(2), near);
            var halfWidth = a.High.Subtract(middle, up).Abs();
            var value = function(middle);
            if (!value.IsFinite)
                return new(EDecimal.NaN, EDecimal.NaN);
            var reach = slope.Multiply(halfWidth, up).Add(value.Abs().Multiply(unit, up), up)
                .Add(EDecimal.FromString("1E-" + (digits + 30)), up);
            return new(value.Subtract(reach, down), value.Add(reach, up));
        }

        private PreciseInterval Exp(PreciseInterval a)
            => AroundTheMiddle(a, x => x.Exp(near), a.High.Exp(up).Abs());

        private PreciseInterval Log(PreciseInterval a)
            => a.Low.Sign > 0 ? AroundTheMiddle(a, x => x.Log(near), EDecimal.One.Divide(a.Low, up)) : new(EDecimal.NaN, EDecimal.NaN);

        private PreciseInterval Sqrt(PreciseInterval a)
            => a.Low.Sign > 0 ? AroundTheMiddle(a, x => x.Sqrt(near), EDecimal.One.Divide(a.Low.Sqrt(down).Multiply(EDecimal.FromInt32(2), down), up))
             : a.IsZero ? a : new(EDecimal.NaN, EDecimal.NaN);

        private PreciseInterval Sin(PreciseInterval a) => AroundTheMiddle(a, x => x.Sin(near), EDecimal.One);
        private PreciseInterval Cos(PreciseInterval a) => AroundTheMiddle(a, x => x.Cos(near), EDecimal.One);
        private PreciseInterval Atan(PreciseInterval a) => AroundTheMiddle(a, x => x.Arctan(near), EDecimal.One);

        private PreciseInterval Asin(PreciseInterval a)
            => InsideTheUnitInterval(a) is { } slope ? AroundTheMiddle(a, x => x.Arcsin(near), slope) : new(EDecimal.NaN, EDecimal.NaN);

        private PreciseInterval Acos(PreciseInterval a)
            => InsideTheUnitInterval(a) is { } slope ? AroundTheMiddle(a, x => x.Acos(near), slope) : new(EDecimal.NaN, EDecimal.NaN);

        /// <summary>The slope bound of the arcsine and arccosine, <c>1/sqrt(1 - x^2)</c>, strictly inside <c>(-1, 1)</c>.</summary>
        private EDecimal? InsideTheUnitInterval(PreciseInterval a)
        {
            var largest = a.Magnitude;
            if (largest.CompareTo(EDecimal.One) >= 0)
                return null;
            return EDecimal.One.Divide(EDecimal.One.Subtract(largest.Multiply(largest, up), down).Sqrt(down), up);
        }

        private static EDecimal Min(EDecimal[] values)
        {
            var result = values[0];
            foreach (var value in values)
                if (value.IsNaN() || value.CompareTo(result) < 0) result = value;
            return result;
        }

        private static EDecimal Max(EDecimal[] values)
        {
            var result = values[0];
            foreach (var value in values)
                if (value.IsNaN() || value.CompareTo(result) > 0) result = value;
            return result;
        }

        // ---- complex rectangles ---------------------------------------------------------

        private PreciseComplexInterval Real(PreciseInterval re) => new(re, PreciseInterval.Exactly(EDecimal.Zero));

        private PreciseComplexInterval Add(PreciseComplexInterval a, PreciseComplexInterval b) => new(Add(a.Re, b.Re), Add(a.Im, b.Im));
        private PreciseComplexInterval Subtract(PreciseComplexInterval a, PreciseComplexInterval b) => new(Subtract(a.Re, b.Re), Subtract(a.Im, b.Im));

        private PreciseComplexInterval Multiply(PreciseComplexInterval a, PreciseComplexInterval b)
            => a.IsReal && b.IsReal ? Real(Multiply(a.Re, b.Re))
             : new(Subtract(Multiply(a.Re, b.Re), Multiply(a.Im, b.Im)), Add(Multiply(a.Re, b.Im), Multiply(a.Im, b.Re)));

        private PreciseComplexInterval Divide(PreciseComplexInterval a, PreciseComplexInterval b)
        {
            if (b.IsReal)
                return new(Divide(a.Re, b.Re), a.IsReal ? PreciseInterval.Exactly(EDecimal.Zero) : Divide(a.Im, b.Re));
            var denominator = Add(Square(b.Re), Square(b.Im));
            return new(Divide(Add(Multiply(a.Re, b.Re), Multiply(a.Im, b.Im)), denominator),
                       Divide(Subtract(Multiply(a.Im, b.Re), Multiply(a.Re, b.Im)), denominator));
        }

        private PreciseComplexInterval Exp(PreciseComplexInterval a)
        {
            var modulus = Exp(a.Re);
            return a.IsReal ? Real(modulus) : new(Multiply(modulus, Cos(a.Im)), Multiply(modulus, Sin(a.Im)));
        }

        private PreciseComplexInterval Log(PreciseComplexInterval a)
        {
            if (a.IsReal && a.Re.Low.Sign > 0)
                return Real(Log(a.Re));
            if (a.IsReal && a.Re.High.Sign < 0)
                return new(Log(a.Re.Negate()), Pi());
            if (a.Re.High.Sign <= 0 && a.Im.ContainsZero || a.Re.ContainsZero && a.Im.ContainsZero)
                return undefined;
            var logModulus = Log(Add(Square(a.Re), Square(a.Im)));
            var half = EDecimal.FromString("0.5");
            var re = new PreciseInterval(logModulus.Low.Multiply(half, down), logModulus.High.Multiply(half, up));
            // The argument over a rectangle off the cut takes its extremes at the corners.
            var corners = new[]
            {
                a.Im.Low.Arctan2(a.Re.Low, near), a.Im.Low.Arctan2(a.Re.High, near),
                a.Im.High.Arctan2(a.Re.Low, near), a.Im.High.Arctan2(a.Re.High, near),
            };
            var low = Min(corners);
            var high = Max(corners);
            var slack = EDecimal.Max(low.Abs(), high.Abs()).Multiply(unit, up).Add(EDecimal.FromString("1E-" + (digits + 30)), up);
            return new(re, new(low.Subtract(slack, down), high.Add(slack, up)));
        }

        private PreciseComplexInterval Pow(PreciseComplexInterval a, long exponent)
        {
            if (exponent < 0)
                return Divide(Real(PreciseInterval.Exactly(EDecimal.One)), Pow(a, -exponent));
            var result = Real(PreciseInterval.Exactly(EDecimal.One));
            var power = a;
            while (exponent > 0)
            {
                if ((exponent & 1) == 1)
                    result = Multiply(result, power);
                exponent >>= 1;
                if (exponent > 0)
                    power = Multiply(power, power);
            }
            return result;
        }

        private PreciseComplexInterval Pow(PreciseComplexInterval a, PreciseComplexInterval exponent)
        {
            if (a.IsReal && a.Re.Low.Sign >= 0 && exponent.IsReal && exponent.Re.Low.CompareTo(EDecimal.FromString("0.5")) == 0
                && exponent.Re.High.CompareTo(EDecimal.FromString("0.5")) == 0)
                return Real(Sqrt(a.Re));
            if (a.IsReal && a.Re.Low.Sign > 0 && exponent.IsReal)
                return Real(Exp(Multiply(exponent.Re, Log(a.Re))));
            return Exp(Multiply(exponent, Log(a)));
        }

        private PreciseComplexInterval Sin(PreciseComplexInterval a)
        {
            if (a.IsReal)
                return Real(Sin(a.Re));
            var (cosh, sinh) = CoshSinh(a.Im);
            return new(Multiply(Sin(a.Re), cosh), Multiply(Cos(a.Re), sinh));
        }

        private PreciseComplexInterval Cos(PreciseComplexInterval a)
        {
            if (a.IsReal)
                return Real(Cos(a.Re));
            var (cosh, sinh) = CoshSinh(a.Im);
            return new(Multiply(Cos(a.Re), cosh), Multiply(Sin(a.Re), sinh).Negate());
        }

        private (PreciseInterval Cosh, PreciseInterval Sinh) CoshSinh(PreciseInterval b)
        {
            var expUp = Exp(b);
            var expDown = Exp(b.Negate());
            var half = PreciseInterval.Exactly(EDecimal.FromString("0.5"));
            return (Multiply(Add(expUp, expDown), half), Multiply(Subtract(expUp, expDown), half));
        }

        private PreciseInterval Pi()
        {
            var pi = EDecimal.FromString(
                "3.14159265358979323846264338327950288419716939937510582097494459230781640628620899862803482534211706798214808651328230664709384460955058223172535940812848111745028410270193852110555964462294895493038196");
            return new(pi.RoundToPrecision(down), pi.RoundToPrecision(up));
        }

        private PreciseInterval E()
        {
            var e = EDecimal.FromString(
                "2.71828182845904523536028747135266249775724709369995957496696762772407663035354759457138217852516642742746639193200305992181741359662904357290033429526059563073813232862794349076323382988075319525101901");
            return new(e.RoundToPrecision(down), e.RoundToPrecision(up));
        }

        private PreciseComplexInterval Evaluate(Entity expr)
        {
            switch (expr)
            {
                case Number.Rational rational:
                {
                    var value = rational.ERational;
                    return Real(new(value.ToEDecimal(down), value.ToEDecimal(up)));
                }
                case Number.Complex number:
                {
                    var re = number.RealPart.EDecimal;
                    var im = number.ImaginaryPart.EDecimal;
                    return new(new(re.RoundToPrecision(down), re.RoundToPrecision(up)), new(im.RoundToPrecision(down), im.RoundToPrecision(up)));
                }
                case Variable variable when variable == MathS.pi:
                    return Real(Pi());
                case Variable variable when variable == MathS.e:
                    return Real(E());
                case Sumf(var left, var right):
                    return Add(Evaluate(left), Evaluate(right));
                case Minusf(var left, var right):
                    return Subtract(Evaluate(left), Evaluate(right));
                case Mulf(var left, var right):
                    return Multiply(Evaluate(left), Evaluate(right));
                case Divf(var left, var right):
                    return Divide(Evaluate(left), Evaluate(right));
                case Powf(var @base, Number.Integer power) when power.EInteger.CanFitInInt64():
                    return Pow(Evaluate(@base), power.EInteger.ToInt64Unchecked());
                case Powf(var @base, var exponent):
                    return @base == MathS.e ? Exp(Evaluate(exponent)) : Pow(Evaluate(@base), Evaluate(exponent));
                case Sinf(var argument):
                    return Sin(Evaluate(argument));
                case Cosf(var argument):
                    return Cos(Evaluate(argument));
                case Tanf(var argument):
                {
                    var inner = Evaluate(argument);
                    return Divide(Sin(inner), Cos(inner));
                }
                case Cotanf(var argument):
                {
                    var inner = Evaluate(argument);
                    return Divide(Cos(inner), Sin(inner));
                }
                case Secantf(var argument):
                    return Divide(Real(PreciseInterval.Exactly(EDecimal.One)), Cos(Evaluate(argument)));
                case Cosecantf(var argument):
                    return Divide(Real(PreciseInterval.Exactly(EDecimal.One)), Sin(Evaluate(argument)));
                case Logf(var @base, var antilogarithm):
                    return Divide(Log(Evaluate(antilogarithm)), Log(Evaluate(@base)));
                case Arctanf(var argument):
                    return RealOnly(Evaluate(argument), Atan);
                case Arcsinf(var argument):
                    return RealOnly(Evaluate(argument), Asin);
                case Arccosf(var argument):
                    return RealOnly(Evaluate(argument), Acos);
                case Absf(var argument):
                {
                    var inner = Evaluate(argument);
                    if (!inner.IsReal)
                        return Real(Sqrt(Add(Square(inner.Re), Square(inner.Im))));
                    var re = inner.Re;
                    return Real(re.ContainsZero ? new(EDecimal.Zero, re.Magnitude) : re.Low.Sign > 0 ? re : re.Negate());
                }
                case Signumf(var argument):
                {
                    var inner = Evaluate(argument);
                    if (!inner.IsFinite || !inner.IsReal || inner.Re.ContainsZero)
                        return undefined;
                    return Real(PreciseInterval.Exactly(inner.Re.Low.Sign > 0 ? EDecimal.One : EDecimal.One.Negate()));
                }
                case Providedf(var inner, var predicate):
                    return Decide(predicate) == true ? Evaluate(inner) : undefined;
                case Piecewise piecewise:
                    foreach (var @case in piecewise.Cases)
                        switch (Decide(@case.Predicate))
                        {
                            case true:
                                return Evaluate(@case.Expression);
                            case null:
                                return undefined;
                        }
                    return undefined;
                default:
                    return undefined;
            }
        }

        private PreciseComplexInterval RealOnly(PreciseComplexInterval argument, Func<PreciseInterval, PreciseInterval> function)
            => argument.IsReal && argument.Re.IsFinite ? Real(function(argument.Re)) : undefined;

        private bool? Decide(Entity predicate)
        {
            switch (predicate)
            {
                case Entity.Boolean value:
                    return value == Entity.Boolean.True;
                case Notf(var argument):
                    return !Decide(argument);
                case Andf(var left, var right):
                {
                    var l = Decide(left);
                    if (l == false) return false;
                    var r = Decide(right);
                    if (r == false) return false;
                    return l == true && r == true ? true : null;
                }
                case Orf(var left, var right):
                {
                    var l = Decide(left);
                    if (l == true) return true;
                    var r = Decide(right);
                    if (r == true) return true;
                    return l == false && r == false ? false : null;
                }
                case Xorf(var left, var right):
                    return Decide(left) is { } first && Decide(right) is { } second ? first != second : null;
                case Impliesf(var assumption, var conclusion):
                {
                    var a = Decide(assumption);
                    if (a == false) return true;
                    var c = Decide(conclusion);
                    if (c == true) return true;
                    return a == true && c == false ? false : null;
                }
                case Equalsf(var left, var right):
                {
                    var difference = Subtract(Evaluate(left), Evaluate(right));
                    if (!difference.IsFinite)
                        return null;
                    if (difference.Re.Mignitude.Sign > 0 || difference.Im.Mignitude.Sign > 0)
                        return false;
                    return difference.Re.IsZero && difference.Im.IsZero ? true : null;
                }
                case Greaterf(var left, var right):
                    return Compare(left, right) is { } greater ? greater > 0 : null;
                case GreaterOrEqualf(var left, var right):
                    return Compare(left, right, orEqual: true) is { } atLeast ? atLeast >= 0 : null;
                case Lessf(var left, var right):
                    return Compare(right, left) is { } less ? less > 0 : null;
                case LessOrEqualf(var left, var right):
                    return Compare(right, left, orEqual: true) is { } atMost ? atMost >= 0 : null;
                default:
                    return null;
            }
        }

        private int? Compare(Entity left, Entity right, bool orEqual = false)
        {
            var l = Evaluate(left);
            var r = Evaluate(right);
            if (!l.IsFinite || !r.IsFinite || !l.IsReal || !r.IsReal)
                return null;
            if (l.Re.Low.CompareTo(r.Re.High) > 0)
                return 1;
            if (l.Re.High.CompareTo(r.Re.Low) < 0)
                return -1;
            if (orEqual && l.Re.Low.CompareTo(l.Re.High) == 0 && r.Re.Low.CompareTo(r.Re.High) == 0 && l.Re.Low.CompareTo(r.Re.Low) == 0)
                return 0;
            return null;
        }
    }
}
