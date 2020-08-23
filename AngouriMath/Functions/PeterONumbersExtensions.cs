// Based on https://github.com/raminrahimzada/CSharp-Helper-Classes/blob/ffbb33c1ee90ce12357c72fa65f2510a09834693/Math/DecimalMath/DecimalMath.cs

using System;
using System.Collections.Generic;
using PeterO.Numbers;
[assembly:System.Runtime.CompilerServices.InternalsVisibleTo("UnitTests")]

namespace AngouriMath
{
    // Default visibility for classes is internal so we can use public for methods as we like
    static class PeterONumbersExtensions
    {
        public class ConstantCache
        {
            public static ConstantCache Lookup(EContext context)
            {
                if (!constants.TryGetValue(context, out var cache))
                    lock (constants)
                        if (!constants.TryGetValue(context, out cache))
                        {
                            cache = new ConstantCache(context);
                            constants.Add(context, cache);
                        }
                return cache;
            }
            static readonly Dictionary<EContext, ConstantCache> constants = new Dictionary<EContext, ConstantCache>();
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

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Least_common_multiple#Using_the_greatest_common_divisor"/>
        /// </summary>
        public static EInteger Lcm(this EInteger bigintFirst, EInteger bigintSecond) =>
            bigintFirst.IsZero && bigintSecond.IsZero
            ? EInteger.Zero
            : bigintFirst.Abs().Divide(bigintFirst.Gcd(bigintSecond)).Multiply(bigintSecond.Abs());
        /// <summary><a href="https://en.wikipedia.org/wiki/Combination"/>, equivalent to nCr
        /// or <a href="https://en.wikipedia.org/wiki/Binomial_coefficient"/></summary>
        public static EInteger Combinations(this EInteger n, EInteger k) =>
            n.Factorial() / ((n - k).Factorial() * k.Factorial());

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
            if (!x.IsFinite) return EDecimal.NaN;

            // TODO: this check should be here to improve the performance a little bit, but tests won't work with that
            // if (Utils.IsGoodAsDouble(x)) return EDecimal.FromDouble(Math.Cos(x.ToDouble()));

            // Of course this will fail! Math.Cos works with double - only ~15 digits of accuracy.
            // We have precision of 100 by default - which is over 95 digits of accuracy,
            // albeit the last few digits are off. You have rejected the use of double early on
            // - as well as the entirety of System.Math.                        -- Happypig375

            var consts = ConstantCache.Lookup(context);

            //truncating to  [-2*PI;2*PI]
            TruncateToPeriodicInterval(ref x, consts, context);

            // now x in (-2pi,2pi)
            if (x.GreaterThanOrEquals(consts.Pi) && x.LessThanOrEquals(consts.TwoPi))
                return -Cos(x.Subtract(consts.Pi, context), context);
            if (x.GreaterThanOrEquals(-consts.TwoPi) && x.LessThanOrEquals(-consts.Pi))
                return -Cos(x.Add(consts.Pi, context), context);

            x = x.Multiply(x, context);
            //y=1-x/2!+x^2/4!-x^3/6!...
            var xx = -x.Multiply(consts.Half, context);
            var y = xx.Increment();
            var cachedY = y.Decrement();//init cache  with different value
            for (var i = 1; !cachedY.Equals(y); i++)
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
            if (!x.IsFinite) return EDecimal.NaN;
            var consts = ConstantCache.Lookup(context);
            var cos = Cos(x, context);
            if (cos.IsZero) return EDecimal.NaN;
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

                throw new Core.Exceptions.AngouriBugException("Should not be reached");
            }
            var moduleOfSin = cos.MultiplyAndAdd(-cos, EDecimal.One, context).Sqrt(context);
            var sineIsPositive = IsSignOfSinePositive(x, consts, context);
            if (sineIsPositive) return moduleOfSin;
            return -moduleOfSin;
        }
        /// <summary>Analogy of <see cref="Math.Sin(double)"/></summary>
        public static EDecimal Sin(this EDecimal x, EContext context)
        {
            if (!x.IsFinite) return EDecimal.NaN;
            var consts = ConstantCache.Lookup(context);
            var cos = Cos(x, context);
            return CalculateSinFromCos(x, cos, consts, context);
        }

        /// <summary>Truncates <paramref name="x"/> to [-2*<see cref="Math.PI"/>, 2*<see cref="Math.PI"/>] </summary>
        private static void TruncateToPeriodicInterval(ref EDecimal x, ConstantCache consts, EContext context)
        {
            while (x.GreaterThanOrEquals(consts.TwoPi))
            {
                var divide = x.Divide(consts.TwoPi, context).Floor().Abs();
                x = divide.MultiplyAndAdd(-consts.TwoPi, x, context);
            }

            while (x.LessThanOrEquals(-consts.TwoPi))
            {
                var divide = x.Divide(consts.TwoPi, context).Floor().Abs();
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
            if (x.Equals(EDecimal.One)) return consts.HalfPi;
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
            } while (!cachedResult.Equals(result));
            return y;
        }

        /// <summary>Analogy of <see cref="Math.Atan(double)"/></summary>
        public static EDecimal Atan(this EDecimal x, EContext context)
        {
            if (x.IsNaN()) return EDecimal.NaN;
            var consts = ConstantCache.Lookup(context);
            if (x.IsInfinity()) return x.Sign * consts.HalfPi;
            if (x.IsZero) return x;
            if (x.Equals(EDecimal.One)) return consts.QuarterPi;
            return Asin(x.Divide(x.MultiplyAndAdd(x, EDecimal.One, context).Sqrt(context), context), context);
        }
        /// <summary>Analogy of <see cref="Math.Acos(double)"/></summary>
        public static EDecimal Acos(this EDecimal x, EContext context)
        {
            var consts = ConstantCache.Lookup(context);
            if (x.IsZero) return consts.HalfPi;
            if (x.Equals(EDecimal.One)) return EDecimal.Zero;
            if (x.IsNegative) return consts.Pi.Subtract(Acos(-x, context), context);
            return consts.HalfPi.Subtract(Asin(x, context), context);
        }

        /// <summary>
        /// Analogy of <see cref="Math.Atan2(double, double)"/> for more see this
        /// <seealso cref="http://i.imgur.com/TRLjs8R.png"/>
        /// </summary>
        public static EDecimal Atan2(this EDecimal y, EDecimal x, EContext context)
        {
            if (y.IsNaN() || x.IsNaN()) return EDecimal.NaN;
            var consts = ConstantCache.Lookup(context);
            const int inf = 100;

            // Values for infinity: https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Math/atan2
            return (x.Sign * (x.IsInfinity() ? inf : 1), y.Sign * (y.IsInfinity() ? inf : 1)) switch
            {
                (inf, inf) => consts.QuarterPi,
                (inf, -inf) => -consts.QuarterPi,
                (-inf, inf) => consts.QuarterPi * 3,
                (-inf, -inf) => -consts.QuarterPi * 3,
                (_, inf) => consts.HalfPi,
                (_, -inf) => -consts.HalfPi,
                (-inf, -1) => -consts.Pi,
                (-inf, 0) => y.IsNegative ? -consts.Pi : consts.Pi,
                (-inf, 1) => consts.Pi,
                (inf, _) => EDecimal.Zero,
                (1, _) => Atan(y.Divide(x, context), context),
                (-1, -1) => Atan(y.Divide(x, context), context).Subtract(consts.Pi, context),
                (-1, 0) => y.IsNegative ? -consts.Pi : consts.Pi,
                (-1, 1) => Atan(y.Divide(x, context), context).Add(consts.Pi, context),
                (0, 1) => consts.HalfPi,
                (0, -1) => -consts.HalfPi,
                (0, 0) => (x.IsNegative, y.IsNegative) switch
                {
                    (true, true) => -consts.Pi,
                    (false, true) => EDecimal.NegativeZero,
                    (true, false) => consts.Pi,
                    (false, false) => EDecimal.Zero,
                },
                _ => throw new Core.Exceptions.AngouriBugException("Unexpected scenario"),
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
            if (x.IsNaN())
                return EDecimal.NaN;
            if (x.IsInfinity())
                return x.Sign;
            var y = x.Exp(context);
            var yy = EDecimal.One.Divide(y, context);
            return y.Subtract(yy, context).Divide(y.Add(yy, context), context);
        }

        // End of https://github.com/raminrahimzada/CSharp-Helper-Classes/blob/ffbb33c1ee90ce12357c72fa65f2510a09834693/Math/DecimalMath/DecimalMath.cs

        /// <summary>Rounds half up to nearest integer</summary>
        public static EDecimal Round(this EDecimal x) => x.RoundToExponent(0, ERounding.HalfUp);
        /// <summary>Rounds towards zero to nearest integer</summary>
        public static EDecimal Truncate(this EDecimal x) => x.RoundToExponent(0, ERounding.Down);
        /// <summary>Splits decimal into integral part and fractional part.
        /// The fractional part is guaranteed to be positive.</summary>
        public static (EInteger Integral, EDecimal Fractional) SplitDecimal(this EDecimal x)
        {
            var integral = x.Floor().ToEInteger();
            return (integral, x - integral);
        }
        /// <summary>If there is a fractional part, returns the next smallest integer</summary>
        public static EDecimal Ceiling(this EDecimal x) => x.RoundToExponent(0, ERounding.Ceiling);
        /// <summary>If there is a fractional part, returns the previous largest integer</summary>
        public static EDecimal Floor(this EDecimal x) => x.RoundToExponent(0, ERounding.Floor);

        // Based on https://github.com/eobermuhlner/big-math/blob/ba75e9a80f040224cfeef3c2ac06390179712443/ch.obermuhlner.math.big/src/main/java/ch/obermuhlner/math/big/BigDecimalMath.java

        static IEnumerable<EInteger> GenerateFactorials()
        {
            var i = 0;
            var result = EInteger.One;
            yield return result;
            while (true)
                yield return result *= ++i;
        }
        static readonly IEnumerator<EInteger> factorialCacheGenerator = GenerateFactorials().GetEnumerator();
        static readonly List<EInteger> factorialCache = new List<EInteger>();
        static readonly Dictionary<int, EDecimal[]> spougeFactorialConstantsCache = new Dictionary<int, EDecimal[]>();

        /**
            <summary>
            Calculates the factorial of the specified integer argument.
            <para>factorial = 1 * 2 * 3 * ... n</para>
            </summary>
            <param name="n">The <see cref="int"/>.</param>
            <returns>The factorial <see cref="EInteger"/>.</returns>
            <exception cref="ArgumentOutOfRangeException">Thrown if x &lt; 0</exception>
        */
        public static EInteger Factorial(int n)
        {
            if (n < 0)
                throw new ArgumentOutOfRangeException(nameof(n), "Illegal factorial(n) for n < 0: n = " + n);
            if (n < factorialCache.Count)
                return factorialCache[n];
            lock (factorialCache)
            {
                if (n < factorialCache.Count)
                    return factorialCache[n];
                for (var i = factorialCache.Count - 1; i < n; i++)
                {
                    factorialCacheGenerator.MoveNext();
                    factorialCache.Add(factorialCacheGenerator.Current);
                }
            }
            return factorialCache[n];
        }

        public static EInteger Factorial(this EInteger n) =>
            n.CanFitInInt32() ? Factorial(n.ToInt32Checked()) : throw new OutOfMemoryException();

        /**
         * <summary>
         * Calculates the factorial of the specified <see cref="EDecimal"/>.
         *
         * <para>This implementation uses
         * <a href="https://en.wikipedia.org/wiki/Spouge%27s_approximation">Spouge's approximation</a>
         * to calculate the factorial for non-integer values.</para>
         *
         * <para>This involves calculating a series of constants that depend on the desired precision.
         * Since this constant calculation is quite expensive (especially for higher precisions),
         * the constants for a specific precision will be cached
         * and subsequent calls to this method with the same precision will be much faster.</para>
         *
         * <para>It is therefore recommended to do one call to this method with the standard precision of your application during the startup phase
         * and to avoid calling it with many different precisions.</para>
         *
         * <para>See: <a href="https://en.wikipedia.org/wiki/Factorial#Extension_of_factorial_to_non-integer_values_of_argument">Wikipedia: Factorial - Extension of factorial to non-integer values of argument</a></para>
         * </summary>
         *
         * <param name="x">The <see cref="EDecimal"/></param>
         * <param name="mathContext">The <see cref="EContext"/> used for the result</param>
         * <returns>The factorial <see cref="EDecimal"/></returns>
         * <exception cref="ArgumentOutOfRangeException">Thrown when the precision of the <paramref name="mathContext"/> is outside the int32 range</exception>
         * <seealso cref="Factorial(int)"/>
         * <seealso cref="Gamma(EDecimal, EContext)"/>
         */
        public static EDecimal Factorial(this EDecimal x, EContext mathContext)
        {
            if (x.IsPositiveInfinity())
                return x;
            if (!x.IsFinite)
                return EDecimal.NaN;
            try
            {
                var @int = x.ToInt32IfExact();
                if (@int < 0) return EDecimal.NaN; // Will become ±∞ if we don't insert this line
                return EDecimal.FromEInteger(Factorial(@int)).RoundToPrecision(mathContext);
            }
            catch { } // EDecimal does not fit in an int32

            if (!mathContext.Precision.CanFitInInt32())
                throw new ArgumentOutOfRangeException($"The precision of the {nameof(mathContext)} is outside the int32 range");

            // https://en.wikipedia.org/wiki/Spouge%27s_approximation
            var mc = mathContext.WithBigPrecision(mathContext.Precision << 1);

            var a = mathContext.Precision.ToInt32Checked() * 13 / 10;
            var constants = GetSpougeFactorialConstants(a);

            var negative = false;
            var factor = constants[0];
            for (int k = 1; k < a; k++)
            {
                factor = factor.Add(constants[k].Divide(x.Add(k), mc));
                negative = !negative;
            }

            var result = x.Add(a).Pow(x.Add(0.5m), mc);
            result = result.Multiply(x.Negate().Subtract(a).Exp(mc));
            result = result.Multiply(factor);

            return result.RoundToPrecision(mathContext);
        }

        internal static EDecimal[] GetSpougeFactorialConstants(int a)
        {
            if (spougeFactorialConstantsCache.TryGetValue(a, out var list))
                return list;
            lock (spougeFactorialConstantsCache)
            {
                if (spougeFactorialConstantsCache.TryGetValue(a, out list))
                    return list;
                var constants = new EDecimal[a];
                var mc = EContext.ForPrecision(a * 15 / 10);

                constants[0] = EDecimal.PI(mc).Multiply(2, mc).Sqrt(mc);

                var negative = false;
                for (int k = 1; k < a; k++)
                {
                    var deltaAK = EDecimal.FromInt32(a - k);
                    var ck = deltaAK.Pow(EDecimal.FromInt32(k).Subtract(0.5m), mc);
                    ck = deltaAK.Exp(mc).Multiply(ck, mc);
                    ck = ck.Divide(Factorial(k - 1), mc);
                    if (negative)
                        ck = ck.Negate();
                    constants[k] = ck;
                    negative = !negative;
                }

                spougeFactorialConstantsCache.Add(a, constants);
                return constants;
            }
        }
        /**
	     * <summary>
	     * Calculates the gamma function of the specified <see cref="EDecimal"/>.
	     *
	     * <para>This implementation uses <see cref="Factorial(EDecimal, EContext)"/> internally,
	     * therefore the performance implications described there apply also for this method.
	     *
	     * <para>See: <a href="https://en.wikipedia.org/wiki/Gamma_function">Wikipedia: Gamma function</a></para>
	     * </summary>
	     *
	     * <param name="x">The <see cref="EDecimal"/></param>
	     * <param name="mathContext">The <see cref="EContext"/> used for the result</param>
	     * <returns>The gamma <see cref="EDecimal"/></returns>
         * <exception cref="ArgumentOutOfRangeException">Thrown when the precision of the <paramref name="mathContext"/> is outside the <see cref="int"/> range</exception>
	     */
        public static EDecimal Gamma(this EDecimal x, EContext mathContext) => Factorial(x.Subtract(EDecimal.One), mathContext);
    }
}