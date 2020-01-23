using AngouriMath.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Functions.NumberSystem
{
    internal static class NumberSystem
    {
        // TODO: add more digits
        internal static string ALPHABET_TOCHAR = "0123456789ABCDEF";
        internal static Dictionary<char, int> ALPHABET_FROMCHAR = new Dictionary<char, int> {
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

        internal static string FloatToBaseN(double num /*should be < 1*/, int N)
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

        internal static string ToBaseN(double num, int N)
        {
            if (N > ALPHABET_TOCHAR.Length)
                throw new MathSException("N should be <= than " + ALPHABET_TOCHAR.Length);
            string sign = num < 0 ? "-" : "";
            num = Math.Abs(num);
            int intPart = (int)Math.Floor(num);
            double floatPart = num - intPart;
            string res = "";

            string rightPart = floatPart != 0 ? "." + FloatToBaseN(floatPart, N) : "";
            string leftPart = sign + IntToBaseN(intPart, N);

            return leftPart + rightPart;
        }

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

        internal static double FloatFromBaseN(string num, int N)
        {
            double res = 0;
            for (int i = 0; i < num.Length; i++)
            {
                char digit = num[i];
                res += ALPHABET_FROMCHAR[digit] / Math.Pow(N, i + 1);
            }
            return res;
        }

        internal static double FromBaseN(string num, int N)
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
