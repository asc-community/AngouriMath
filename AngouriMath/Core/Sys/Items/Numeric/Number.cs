
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
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace AngouriMath.Core.Numerix
{
    public abstract partial class Number
    {
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
        public HierarchyLevel Type { get; protected set; }
        public (decimal Re, decimal Im) Value { get => GetValue(); }
        protected abstract (decimal Re, decimal Im) GetValue();
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
            return needParentheses ? "(" + str + ")" : str;
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
            return needParentheses ? "(" + str + ")" : str;
        }

        public static Set GetAllRoots(ComplexNumber value, long rootPower)
        {
            var res = new Set();
            decimal phi = (Number.Log(Math.E, value / value.Abs()) / MathS.i).Value.Re;
            decimal newMod = Number.Pow(Number.Abs(value), 1.0 / rootPower).Value.Re;
            var i = new ComplexNumber(0, 1);
            for (int n = 0; n < rootPower; n++)
            {
                decimal newPow = phi / rootPower + 2 * (decimal)Math.PI * n / rootPower;
                res.Add(newMod * ComplexNumber.Pow(Math.E, i * newPow));
            }
            return res;
        }

        public static RealNumber Abs(Number num)
            => (num as ComplexNumber).Abs();
    }

    public class InvalidNumberCastException : InvalidCastException
    {
        public InvalidNumberCastException(Number.HierarchyLevel typeFrom, Number.HierarchyLevel typeTo) 
            : base("Cannot cast from " + typeFrom + " to " + typeTo)
        {
            
        }
    }
}
