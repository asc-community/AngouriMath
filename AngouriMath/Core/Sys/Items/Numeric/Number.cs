
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


using System;
using System.Linq;

namespace AngouriMath.Core.Numerix
{
    public abstract partial class Number : Sys.Interfaces.ILatexiseable
    {
        /// <summary>
        /// Copies a Number with respect due to its hierarchy type, but without implicit downcasting
        /// </summary>
        /// <returns>
        /// Safely copied instance of Number
        /// </returns>
        public static Number Copy(Number num)
            => SuperSwitch(
                (num) => new IntegerNumber(num[0] as Number),
                (num) => new RationalNumber(num[0]),
                (num) => new RealNumber(num[0] as Number),
                (num) => new ComplexNumber(num[0]),
                num.Type,
                num
            );
        public enum HierarchyLevel
        {
            INTEGER,
            RATIONAL,
            REAL,
            COMPLEX
        }

        /// <summary>
        /// Type of a Number, e. g. INTEGER or RATIONAL
        /// It's recommended to use method Is instead
        /// </summary>
        public HierarchyLevel Type { get; protected set; }

        /// <summary>
        /// The final value. Only useful for calculations
        /// </summary>
        public (decimal Re, decimal Im) Value => GetValue();
        protected abstract (decimal Re, decimal Im) GetValue();

        /// <summary>
        /// Checks affiliation of a number
        /// e. g.
        /// num.Is(Number.HierarchyLevel.INTEGER) would check whether a num is an IntegerNumber
        /// </summary>
        /// <param name="type">
        /// Number.HierarchyLevel {
        ///   INTEGER,
        ///   RATIONAL,
        ///   REAL,
        ///   COMPLEX
        /// }
        /// </param>
        /// <returns></returns>
        public bool Is(HierarchyLevel type)
            => (long) Type <= (long) type;

        /// <summary>
        /// This function serves not only convenience but also protects from unexpeceted cases, for example,
        /// if a new type added
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ifInt"></param>
        /// <param name="ifRat"></param>
        /// <param name="ifReal"></param>
        /// <param name="ifCom"></param>
        /// <param name="nums"></param>
        /// <returns></returns>
        protected static T SuperSwitch<T>(
            Func<IntegerNumber[], T> ifInt,
            Func<RationalNumber[], T> ifRat,
            Func<RealNumber[], T> ifReal,
            Func<ComplexNumber[], T> ifCom,
            HierarchyLevel type,
            params Number[] nums
        )
            => type switch
            {
                HierarchyLevel.INTEGER => ifInt(nums.Select(n => (n as IntegerNumber)).ToArray()),
                HierarchyLevel.RATIONAL => ifRat(nums.Select(n => (n as RationalNumber)).ToArray()),
                HierarchyLevel.REAL => ifReal(nums.Select(n => (n as RealNumber)).ToArray()),
                HierarchyLevel.COMPLEX => ifCom(nums.Select(n => (n as ComplexNumber)).ToArray()),
                _ => throw new NotSupportedException()
            };

        /// <summary>
        /// Gets a latexised version of a number
        /// </summary>
        /// <returns></returns>
        public string Latexise() => Latexise(false);
        internal string Latexise(bool needParentheses)
        {
            var str = SuperSwitch(
                num => num[0].InternalLatexise(),
                num => num[0].InternalLatexise(),
                num => num[0].InternalLatexise(),
                num => num[0].InternalLatexise(),
                Type,
                this
            );
            // If parentheses are required, they might be only required when complicated numbers are wrapped,
            // such as fractions and complex but not a single i
            return needParentheses && this.Value != (0, 1) && (this.IsImaginary() || this.IsFraction()) ? @"\left(" + str + @"\right)" : str;
        }

        public override string ToString()
            => ToString(false);

        internal string ToString(bool needParentheses)
        {
            var str = SuperSwitch(
                num => num[0].InternalToString(),
                num => num[0].InternalToString(),
                num => num[0].InternalToString(),
                num => num[0].InternalToString(),
                Type,
                this
            );
            return needParentheses && (this.IsImaginary() || this.IsFraction()) ? "(" + str + ")" : str;
        }

        /// <summary>
        /// Finds all complex roots of a number
        /// e. g. sqrt(1) = { -1, 1 }
        /// root(1, 4) = { -i, i, -1, 1 }
        /// </summary>
        /// <param name="value"></param>
        /// <param name="rootPower"></param>
        /// <returns></returns>

        
        public static Set GetAllRoots(ComplexNumber value, long rootPower)
        {
            MathS.Settings.FloatToRationalIterCount.Set(0);
            var res = new Set();
            decimal phi = (Number.Log(MathS.DecimalConst.e, value / value.Abs()) / MathS.i).Value.Re;
            decimal newMod = Number.Pow(Number.Abs(value), 1.0 / rootPower).Value.Re;
            var i = new ComplexNumber(0, 1);
            for (int n = 0; n < rootPower; n++)
            {
                decimal newPow = phi / rootPower + 2 * MathS.DecimalConst.pi * n / rootPower;
                res.Add(newMod * Number.Pow(MathS.DecimalConst.e, i * newPow));
            }
            MathS.Settings.FloatToRationalIterCount.Unset();
            return res;
        }

        public static Set GetAllRootsOf1(long rootPower)
        {
            var res = new Set();
            res.FastAddingMode = true;
            for (int i = 0; i < rootPower; i++)
            {
                var angle = (Number.CreateRational(i * 2, rootPower) * MathS.pi).InnerSimplify();
                res.Add((MathS.Cos(angle) + MathS.i * MathS.Sin(angle)).InnerSimplify());
            }
            res.FastAddingMode = false;
            return res;
        }

        /// <summary>
        /// Returns the absolute value of a complex number num, to be precise,
        /// if num = a + ib, num.Abs() -> sqrt(a^2 + b^2)
        /// </summary>
        /// <param name="num">
        /// RealNumber
        /// </param>
        /// <returns></returns>
        public static RealNumber Abs(ComplexNumber num)
            => num.Abs();

        /// <summary>
        /// Returns the absolute value of a real number, basically keeps the same if num is positive,
        /// inverts the sign otherwise
        /// </summary>
        /// <param name="num"></param>
        /// <returns>
        /// RealNumber
        /// </returns>
        public static IntegerNumber Abs(IntegerNumber num)
            => num.Value >= 0 ? num : new IntegerNumber(-num.Value);
    }

    public class InvalidNumberCastException : InvalidCastException
    {
        public InvalidNumberCastException(Number.HierarchyLevel typeFrom, Number.HierarchyLevel typeTo) 
            : base("Cannot cast from " + typeFrom + " to " + typeTo)
        {
            
        }
    }
}
