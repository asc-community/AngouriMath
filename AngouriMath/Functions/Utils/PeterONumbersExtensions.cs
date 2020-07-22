// Based on https://github.com/raminrahimzada/CSharp-Helper-Classes/blob/ffbb33c1ee90ce12357c72fa65f2510a09834693/Math/DecimalMath/DecimalMath.cs

using System;
using System.Collections.Generic;
using System.Text;
using PeterO.Numbers;

namespace AngouriMath
{
    // Default visibility for classes is internal so we can use public for methods as we like
    static class PeterONumbersExtensions
    {
        public class ConstantCache
        {
            public static ConstantCache Lookup(EContext context)
            {
                if (!Constants.TryGetValue(context, out var cache))
                {
                    cache = new ConstantCache(context);
                    Constants.Add(context, cache);
                }
                return cache;
            }
            static Dictionary<EContext, ConstantCache> Constants { get; } = new Dictionary<EContext, ConstantCache>();
            ConstantCache(EContext context)
            {
                Half = EDecimal.One.Divide(2, context);
                Pi = EDecimal.PI(context);
                TwoPi = Pi.Multiply(2, context);
                HalfPi = Pi.Multiply(Half, context);
                QuarterPi = HalfPi.Multiply(Half, context);
                E = EDecimal.One.Exp(context);
            }
            /// <summary>Represents <see cref="Math.PI"/></summary>
            public EDecimal Pi { get; }
            /// <summary>Represents 2 * <see cref="Math.PI"/></summary>
            public EDecimal TwoPi { get; }
            /// <summary>Represents <see cref="Math.PI"/> / 2</summary>
            public EDecimal HalfPi { get; }
            /// <summary>Represents <see cref="Math.PI"/> / 4</summary>
            public EDecimal QuarterPi { get; }
            /// <summary>Represents <see cref="Math.E"/></summary>
            public EDecimal E { get; }
            /// <summary>Represents 0.5</summary>
            public EDecimal Half { get; }
        }

        // https://en.wikipedia.org/wiki/Least_common_multiple#Using_the_greatest_common_divisor
        public static EInteger Lcm(this EInteger bigintFirst, EInteger bigintSecond)
        {
            if (bigintFirst.IsZero && bigintSecond.IsZero) return EInteger.Zero;
            return bigintFirst.Abs().Divide(bigintFirst.Gcd(bigintSecond)).Multiply(bigintSecond.Abs());
        }
        /// <summary>Max iterations count in Taylor series</summary>
        /// <remarks>Defined as 100 originally, here we experimentally do not put a limit on the Taylor series</remarks>
        const int MaxIteration = int.MaxValue;

        /// <summary>Use until https://github.com/peteroupc/Numbers/issues/15 is fixed</summary>
        public static bool EqualsBugFix(this EDecimal bigDecimalOne, EDecimal bigDecimalTwo) =>
            bigDecimalOne.CompareTo(bigDecimalTwo) == 0;
        public static bool GreaterThan(this EDecimal bigDecimalOne, EDecimal bigDecimalTwo) =>
            bigDecimalOne.CompareTo(bigDecimalTwo) > 0;
        public static bool GreaterThanOrEquals(this EDecimal bigDecimalOne, EDecimal bigDecimalTwo) =>
            bigDecimalOne.CompareTo(bigDecimalTwo) >= 0;
        public static bool LessThan(this EDecimal bigDecimalOne, EDecimal bigDecimalTwo) =>
            bigDecimalOne.CompareTo(bigDecimalTwo) < 0;
        public static bool LessThanOrEquals(this EDecimal bigDecimalOne, EDecimal bigDecimalTwo) =>
            bigDecimalOne.CompareTo(bigDecimalTwo) <= 0;

        /// <summary>Analogy of <see cref="Math.Cos(double)"/></summary>
        public static EDecimal Cos(this EDecimal x, EContext context)
        {
            var consts = ConstantCache.Lookup(context);

            //truncating to  [-2*PI;2*PI]
            TruncateToPeriodicInterval(ref x, consts, context);

            // now x in (-2pi,2pi)
            if (x.GreaterThanOrEquals(consts.Pi) && x.LessThanOrEquals(consts.TwoPi))
            {
                return -Cos(x.Subtract(consts.Pi, context), context);
            }
            if (x.GreaterThanOrEquals(-consts.TwoPi) && x.LessThanOrEquals(-consts.Pi))
            {
                return -Cos(x.Add(consts.Pi, context), context);
            }

            x = x.Multiply(x, context);
            //y=1-x/2!+x^2/4!-x^3/6!...
            var xx = -x.Multiply(consts.Half, context);
            var y = xx.Increment();
            var cachedY = y.Decrement();//init cache  with different value
            for (var i = 1; !cachedY.EqualsBugFix(y) && i < MaxIteration; i++)
            {
                cachedY = y;
                EDecimal factor = i * ((i << 1) + 3) + 1; //2i^2+2i+i+1=2i^2+3i+1
                factor = -consts.Half.Divide(factor, context);
                xx = xx.Multiply(x.Multiply(factor, context), context);
                y = y.Add(xx, context);
            }
            return y;
        }

        /// <summary>Analogy of <see cref="Math.Tan(double)"/></summary>
        public static EDecimal Tan(this EDecimal x, EContext context)
        {
            var consts = ConstantCache.Lookup(context);
            var cos = Cos(x, context);
            if (cos.IsZero) throw new ArgumentException(nameof(x));
            //calculate sin using cos
            var sin = CalculateSinFromCos(x, cos, consts, context);
            return sin.Divide(cos, context);
        }
        /// <summary>Helper function for calculating sin(x) from cos(x)</summary>
        static EDecimal CalculateSinFromCos(EDecimal x, EDecimal cos, ConstantCache consts, EContext context)
        {
            static bool IsSignOfSinePositive(EDecimal x, ConstantCache consts, EContext context)
            {
                //truncating to  [-2*PI;2*PI]
                TruncateToPeriodicInterval(ref x, consts, context);

                //now x in [-2*PI;2*PI]
                if (x.GreaterThanOrEquals(-consts.TwoPi) && x.LessThanOrEquals(-consts.Pi)) return true;
                if (x.GreaterThanOrEquals(-consts.Pi) && (x.IsNegative || x.IsZero)) return false;
                if (!x.IsNegative && x.LessThanOrEquals(consts.Pi)) return true;
                if (x.GreaterThanOrEquals(consts.Pi) && x.LessThanOrEquals(consts.TwoPi)) return false;

                throw new Core.Exceptions.SysException("Should not be reached");
            }
            var moduleOfSin = cos.MultiplyAndAdd(-cos, EDecimal.One, context).Sqrt(context);
            var sineIsPositive = IsSignOfSinePositive(x, consts, context);
            if (sineIsPositive) return moduleOfSin;
            return -moduleOfSin;
        }
        /// <summary>Analogy of <see cref="Math.Sin(double)"/></summary>
        public static EDecimal Sin(this EDecimal x, EContext context)
        {
            var consts = ConstantCache.Lookup(context);
            var cos = Cos(x, context);
            return CalculateSinFromCos(x, cos, consts, context);
        }


        /// <summary>Truncates <paramref name="x"/> to [-2*<see cref="Math.PI"/>, 2*<see cref="Math.PI"/>] </summary>
        private static void TruncateToPeriodicInterval(ref EDecimal x, ConstantCache consts, EContext context)
        {
            while (x.GreaterThanOrEquals(consts.TwoPi))
            {
                EDecimal divide = x.Divide(consts.TwoPi, context).ToEInteger().Abs();
                x = divide.MultiplyAndAdd(-consts.TwoPi, x, context);
            }

            while (x.LessThanOrEquals(-consts.TwoPi))
            {
                EDecimal divide = x.Divide(consts.TwoPi, context).ToEInteger().Abs();
                x = divide.MultiplyAndAdd(consts.TwoPi, x, context);
            }
        }

        /// <summary>Analogy of <see cref="Math.Asin(double)"/></summary>
        public static EDecimal Asin(this EDecimal x, EContext context)
        {
            if (x.GreaterThan(EDecimal.One) || x.LessThan(-EDecimal.One))
                return EDecimal.NaN;
            var consts = ConstantCache.Lookup(context);

            //known values
            if (x.IsZero) return x;
            if (x.EqualsBugFix(EDecimal.One)) return consts.HalfPi;
            //asin function is odd function
            if (x.IsNegative) return -Asin(-x, context);

            //my optimize trick here

            // used a math formula to speed up :
            // asin(x)=0.5*(pi/2-asin(1-2*x*x)) 
            // if x>=0 is true

            var newX = x.Multiply(-2, context).MultiplyAndAdd(x, EDecimal.One, context);

            //for calculating new value near to zero than current
            //because we gain more speed with values near to zero
            if (x.Abs().GreaterThan(newX.Abs()))
            {
                var t = Asin(newX, context);
                return consts.Half.Multiply(consts.HalfPi.Subtract(t, context), context);
            }
            var result = x;
            EDecimal cachedResult;
            var i = 1;
            var y = result;
            var xx = x.Multiply(x, context);
            do
            {
                cachedResult = result;
                result = consts.Half.Divide(-i, context).Increment().Multiply(xx, context).Multiply(result, context);
                y = result.Divide((i << 1) + 1, context).Add(y, context);
                i++;
            } while (!cachedResult.EqualsBugFix(result));
            return y;
        }

        /// <summary>Analogy of <see cref="Math.Atan(double)"/></summary>
        public static EDecimal Atan(this EDecimal x, EContext context)
        {
            var consts = ConstantCache.Lookup(context);
            if (x.IsZero) return x;
            if (x.EqualsBugFix(EDecimal.One)) return consts.QuarterPi;
            return Asin(x.Divide(x.MultiplyAndAdd(x, EDecimal.One, context).Sqrt(context), context), context);
        }
        /// <summary>Analogy of <see cref="Math.Acos(double)"/></summary>
        public static EDecimal Acos(this EDecimal x, EContext context)
        {
            var consts = ConstantCache.Lookup(context);
            if (x.IsZero) return consts.HalfPi;
            if (x.EqualsBugFix(EDecimal.One)) return EDecimal.Zero;
            if (x.IsNegative) return consts.Pi.Subtract(Acos(-x, context), context);
            return consts.HalfPi.Subtract(Asin(x, context), context);
        }

        /// <summary>
        /// Analogy of <see cref="Math.Atan2(double, double)"/> for more see this
        /// <seealso cref="http://i.imgur.com/TRLjs8R.png"/>
        /// </summary>
        public static EDecimal Atan2(this EDecimal y, EDecimal x, EContext context)
        {
            var consts = ConstantCache.Lookup(context);
            return (x.Sign, y.Sign) switch
            {
                (1, _) => Atan(y.Divide(x, context), context),
                (-1, -1) => Atan(y.Divide(x, context), context).Subtract(consts.Pi, context),
                (-1, _) => Atan(y.Divide(x, context), context).Add(consts.Pi, context),
                (0, 1) => consts.HalfPi,
                (0, -1) => -consts.HalfPi,
                _ => throw new ArgumentException("Invalid atan2 arguments"),
            };
        }


        /// <summary>Analogy of <see cref="Math.Sinh(double)"/></summary>
        public static EDecimal Sinh(this EDecimal x, EContext context)
        {
            var y = x.Exp(context);
            var yy = EDecimal.One.Divide(y, context);
            return y.Subtract(yy, context).Divide(2, context);
        }

        /// <summary>Analogy of <see cref="Math.Cosh(double)"/></summary>
        public static EDecimal Cosh(this EDecimal x, EContext context)
        {
            var y = x.Exp(context);
            var yy = EDecimal.One.Divide(y, context);
            return y.Add(yy, context).Divide(2, context);
        }

        /// <summary>Analogy of <see cref="Math.Tanh(double)"/></summary>
        public static EDecimal Tanh(this EDecimal x, EContext context)
        {
            var y = x.Exp(context);
            var yy = EDecimal.One.Divide(y, context);
            return y.Subtract(yy, context).Divide(y.Add(yy, context), context);
        }
    }
}