
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



﻿using AngouriMath.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Functions.NumberSystem
{
    internal static class NumberSystem
    {
        // TODO: add more digits
        internal static readonly string ALPHABET_TOCHAR = "0123456789ABCDEF";
        internal static readonly Dictionary<char, int> ALPHABET_FROMCHAR = new Dictionary<char, int> {
            { '0', 0 },
            { '1', 1 },
            { '2', 2 },
            { '3', 3 },
            { '4', 4 },
            { '5', 5 },
            { '6', 6 },
            { '7', 7 },
            { '8', 8 },
            { '9', 9 },
            { 'A', 10 },
            { 'B', 11 },
            { 'C', 12 },
            { 'D', 13 },
            { 'E', 14 },
            { 'F', 15 },
        };

        /// <summary>
        /// Transforms an integer to the corresponding base (in string)
        /// </summary>
        /// <param name="num"></param>
        /// <param name="N"></param>
        /// <returns></returns>
        internal static string IntToBaseN(int num, int N)
        {
            if (num < 0)
                throw new SysException("Error in IntToBaseN");
            string res = "";
            while (num > 0)
            {
                res = ALPHABET_TOCHAR[num % N] + res;
                num /= N;
            }
            return res;
        }

        /// <summary>
        /// Transforms a floating number, but this number should be in [0; 1]
        /// </summary>
        /// <param name="num"></param>
        /// <param name="N"></param>
        /// <returns></returns>
        internal static string FloatToBaseN(decimal num /*should be < 1*/, int N)
        {
            if (num > 1 || num < 0)
                throw new SysException("Error in FloatToBaseN");
            string res = "";
            while (num > 0)
            {
                num *= N;
                int intPart = (int)Math.Floor(num);
                res += ALPHABET_TOCHAR[intPart];
                num -= intPart;
            }
            return res;
        }

        /// <summary>
        /// if a number is A + B where A is integer and B is in [0; 1], it performs operations
        /// for A and B separately and then concatenates
        /// </summary>
        /// <param name="num"></param>
        /// <param name="N"></param>
        /// <returns></returns>
        internal static string ToBaseN(decimal num, int N)
        {
            if (N > ALPHABET_TOCHAR.Length)
                throw new MathSException("N should be <= than " + ALPHABET_TOCHAR.Length);
            string sign = num < 0 ? "-" : "";
            num = Math.Abs(num);
            int intPart = (int)Math.Floor(num);
            decimal floatPart = num - intPart;

            string rightPart = floatPart != 0 ? "." + FloatToBaseN(floatPart, N) : "";
            string leftPart = sign + IntToBaseN(intPart, N);

            return leftPart + rightPart;
        }

        /// <summary>
        /// Gets an integer from a string (which should not contain any ,.-)
        /// </summary>
        /// <param name="num"></param>
        /// <param name="N"></param>
        /// <returns></returns>
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

        /// <summary>
        /// if num is ABC, the initial number was 0.ABC
        /// </summary>
        /// <param name="num"></param>
        /// <param name="N"></param>
        /// <returns></returns>
        internal static decimal FloatFromBaseN(string num, int N)
        {
            decimal res = 0;
            for (int i = 0; i < num.Length; i++)
            {
                char digit = num[i];
                res += ALPHABET_FROMCHAR[digit] / (decimal)Math.Pow(N, i + 1);
            }
            return res;
        }

        /// <summary>
        /// Performs operations on both the integer and floating parts of a number and concatenates
        /// </summary>
        /// <param name="num"></param>
        /// <param name="N"></param>
        /// <returns></returns>
        internal static decimal FromBaseN(string num, int N)
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
