/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using System.Numerics;
using PeterO.Numbers;


namespace AngouriMath
{
    partial record Entity
    {
        /// <summary>
        /// Any node that might be counted as a number, derived, etc. is continuous
        /// TODO
        /// </summary>
        public abstract partial record NumericNode : Entity
        {

        }

#pragma warning disable CS1591
        public static implicit operator Entity(sbyte value) => Number.Integer.Create(value);
        public static implicit operator Entity(byte value) => Number.Integer.Create(value);
        public static implicit operator Entity(short value) => Number.Integer.Create(value);
        public static implicit operator Entity(ushort value) => Number.Integer.Create(value);
        public static implicit operator Entity(int value) => Number.Integer.Create(value);
        public static implicit operator Entity(uint value) => Number.Integer.Create(value);
        public static implicit operator Entity(long value) => Number.Integer.Create(value);
        public static implicit operator Entity(ulong value) => Number.Integer.Create(value);
        public static implicit operator Entity(EInteger value) => Number.Integer.Create(value);
        public static implicit operator Entity(ERational value) => Number.Rational.Create(value);
        public static implicit operator Entity(EDecimal value) => Number.Real.Create(value);
        public static implicit operator Entity(float value) => Number.Real.Create(EDecimal.FromSingle(value));
        public static implicit operator Entity(double value) => Number.Real.Create(EDecimal.FromDouble(value));
        public static implicit operator Entity(decimal value) => Number.Real.Create(EDecimal.FromDecimal(value));
        public static implicit operator Entity(Complex value) =>
            Number.Complex.Create(EDecimal.FromDouble(value.Real), EDecimal.FromDouble(value.Imaginary));
#pragma warning restore CS1591

        /// <summary>
        /// Hangs two nodes to a Sum node (i. e. building an expression)
        /// </summary>
        /// <param name="augend">The left node to add</param>
        /// <param name="addend">The right node to add</param>
        public static Entity operator +(Entity augend, Entity addend) => new Sumf(augend, addend);

        /// <summary>
        /// Does not do anything until an unary plus node added
        /// </summary>
        public static Entity operator +(Entity a) => a;

        /// <summary>
        /// Hangs two nodes to a Minus node (i. e. building an expression)
        /// </summary>
        /// <param name="subtrahend">The left node to add</param>
        /// <param name="minuend">The right node to add</param>
        public static Entity operator -(Entity subtrahend, Entity minuend) => new Minusf(subtrahend, minuend);

        /// <summary>
        /// Multiplies the only argument by -1
        /// </summary>
        /// <returns>Mul node of -1 and the only argument</returns>
        public static Entity operator -(Entity a) => new Mulf(-1, a);

        /// <summary>
        /// Hangs two nodes to a Mul node (i. e. building an expression)
        /// </summary>
        /// <param name="multiplier">The left node to add</param>
        /// <param name="multiplicand">The right node to add</param>
        public static Entity operator *(Entity multiplier, Entity multiplicand) => new Mulf(multiplier, multiplicand);

        /// <summary>
        /// Hangs two nodes to a Div node (i. e. building an expression)
        /// </summary>
        /// <param name="dividend">The left node to add</param>
        /// <param name="divisor">The right node to add</param>
        public static Entity operator /(Entity dividend, Entity divisor) => new Divf(dividend, divisor);

        public Entity Sin() => new Sinf(this);
        public Entity Cos() => new Cosf(this);
        public Entity Tan() => new Tanf(this);
        public Entity Cotan() => new Cotanf(this);
        public Entity Pow(Entity n) => new Powf(this, n);
        public Entity Log(Entity x) => new Logf(this, x);
        public Entity Arcsin() => new Arcsinf(this);
        public Entity Arccos() => new Arccosf(this);
        public Entity Arctan() => new Arctanf(this);
        public Entity Arccotan() => new Arccotanf(this);
        public Entity Factorial() => new Factorialf(this);
        public Entity Signum() => new Signumf(this);
        public Entity Abs() => new Absf(this);

    }
}
