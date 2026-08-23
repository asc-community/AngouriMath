//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Exceptions;
using System;
using PeterO.Numbers;

namespace AngouriMath.Functions
{
    using static Entity;
    internal static class BaseConversion
    {
        // TODO: add more digits
        [ConstantField] internal static readonly string ALPHABET_TOCHAR = "0123456789ABCDEF";
        [ConstantField] internal static readonly Dictionary<char, int> ALPHABET_FROMCHAR =
            ALPHABET_TOCHAR.ToDictionary(c => c, ALPHABET_TOCHAR.IndexOf);

        /// <summary>Transforms an integer to the corresponding base (in string)</summary>
        internal static string IntToBaseN(EInteger num, int N)
        {
            if (num < 0)
                throw new AngouriBugException("Error in IntToBaseN");
            if (num.IsZero)
                return "0";
            var res = new System.Text.StringBuilder();
            while (num > 0)
            {
                res.Append(ALPHABET_TOCHAR[(num % N).ToInt32Checked()]);
                num /= N;
            }
            Reverse(res);
            return res.ToString();
            // https://stackoverflow.com/a/47944113/5429648
            static void Reverse(System.Text.StringBuilder sb)
            {
                char t;
                int end = sb.Length - 1;
                int start = 0;

                while (end - start > 0)
                {
                    t = sb[end];
                    sb[end] = sb[start];
                    sb[start] = t;
                    start++;
                    end--;
                }
            }
        }

        /// <summary>
        /// How many base-<paramref name="N"/> fractional digits carry as much information as
        /// the currently configured decimal precision, i. e. ceil(precision / log10(N)).
        /// Used to bound expansions that never terminate.
        /// </summary>
        private static int MaxFractionalDigits(int N)
        {
            var precision = MathS.Settings.DecimalPrecisionContext.Value.Precision;
            // An unlimited context reports a precision of 0; fall back to the default.
            var digits = precision.IsZero ? 100 : precision.ToInt32Checked();
            return (int)Math.Ceiling(digits / Math.Log10(N));
        }

        /// <summary>Transforms a floating number, but this number should be in [0; 1]</summary>
        /// <remarks>
        /// Multiplying an <see cref="EDecimal"/> by an integer is exact and never grows its
        /// scale, so the digits are produced exactly and the sequence is eventually periodic.
        /// A terminating expansion ends on its own; a repeating one (e. g. 0.125 in base 5,
        /// which is 0.0303...) is cut off at <see cref="MaxFractionalDigits"/> digits.
        /// </remarks>
        internal static string FloatToBaseN(EDecimal num /*should be < 1*/, int N)
        {
            if (num.GreaterThan(EDecimal.One) || num.IsNegative)
                throw new AngouriBugException("Error in FloatToBaseN");
            var res = new System.Text.StringBuilder();
            var maxDigits = MaxFractionalDigits(N);
            while (!num.IsZero && res.Length < maxDigits)
            {
                num = num.Multiply(N);

                EInteger intPart;
                (intPart, num) = num.SplitDecimal();
                res.Append(ALPHABET_TOCHAR[intPart.ToInt32Checked()]);
            }
            return res.ToString();
        }

        /// <summary>
        /// if a number is A + B where A is integer and B is in [0; 1], it performs operations
        /// for A and B separately and then concatenates
        /// </summary>
        internal static string ToBaseN(EDecimal num, int N)
        {
            if (N > ALPHABET_TOCHAR.Length)
                throw new InvalidNumericSystemException(
                    $"The base has to be at most {ALPHABET_TOCHAR.Length}, since that is how many digits there are, and {N} was given");
            string sign = num.IsNegative ? "-" : "";
            num = num.Abs();
            var (intPart, floatPart) = num.SplitDecimal();

            string rightPart = !floatPart.IsZero ? "." + FloatToBaseN(floatPart, N) : "";
            string leftPart = sign + IntToBaseN(intPart, N);

            return leftPart + rightPart;
        }

        /// <summary>Gets an integer from a string which should not contain any of ,.-</summary>
        internal static int IntFromBaseN(string num, int N)
        {
            int res = 0;
            for (int i = 0; i < num.Length; i++)
            {
                int id = num.Length - i - 1;
                char digit = num[id];
                res += ALPHABET_FROMCHAR[digit] * (int)Math.Pow(N, i);
            }
            return res;
        }

        /// <summary>if num is ABC, the initial number was 0.ABC</summary>
        internal static EDecimal FloatFromBaseN(string num, int N)
        {
            EDecimal res = 0;
            for (int i = 0; i < num.Length; i++)
            {
                char digit = num[i];
                res = Number.CtxAdd(res, Number.CtxDivide(ALPHABET_FROMCHAR[digit], EDecimal.FromInt32(N).Pow(i + 1)));
            }
            return res;
        }

        /// <summary>
        /// Performs operations on both the integer and floating parts of a number and concatenates
        /// </summary>
        internal static EDecimal FromBaseN(string num, int N)
        {
            int sign = num[0] == '-' ? -1 : 1;
            num = num[0] == '-' ? num.Substring(1) : num;
            int pos = num.IndexOf('.');
            string leftPart;
            string rightPart;
            if (pos != -1)
            {
                leftPart = num.Substring(0, pos);
                rightPart = num.Substring(pos + 1);
            }
            else
            {
                leftPart = num;
                rightPart = "";
            }
            return sign * (IntFromBaseN(leftPart, N) + (rightPart == "" ? 0 : FloatFromBaseN(rightPart, N)));
        }
    }
}
