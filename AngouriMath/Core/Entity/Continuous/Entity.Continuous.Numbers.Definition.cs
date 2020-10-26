/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using PeterO.Numbers;
using System;

namespace AngouriMath
{
    partial record Entity
    {
        /// <summary>Number node.
        /// This class represents all possible numerical values as a hierarchy,
        /// <list>
        ///   <see cref="Number"/>
        ///   <list type="bullet">
        ///     <see cref="Complex"/>
        ///       <list type="bullet">
        ///         <see cref="Real"/>
        ///         <list type="bullet">
        ///           <see cref="Rational"/>
        ///           <list type="bullet">
        ///             <see cref="Integer"/>
        ///           </list>
        ///         </list>
        ///       </list>
        ///     </list>
        ///   </list>
        /// </summary>
        public abstract partial record Number : NumericNode
        {
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(this);
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => Array.Empty<Entity>();

            /// <summary>
            /// Checks whether the number is not exposed to implicit rounding
            /// For example, integers and rationals are such
            /// </summary>
            public abstract bool IsExact { get; }
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            public static implicit operator Number(sbyte value) => Integer.Create(value);
            public static implicit operator Number(byte value) => Integer.Create(value);
            public static implicit operator Number(short value) => Integer.Create(value);
            public static implicit operator Number(ushort value) => Integer.Create(value);
            public static implicit operator Number(int value) => Integer.Create(value);
            public static implicit operator Number(uint value) => Integer.Create(value);
            public static implicit operator Number(long value) => Integer.Create(value);
            public static implicit operator Number(ulong value) => Integer.Create(value);
            public static implicit operator Number(EInteger value) => Integer.Create(value);
            public static implicit operator Number(ERational value) => Rational.Create(value);
            public static implicit operator Number(EDecimal value) => Real.Create(value);
            public static implicit operator Number(float value) => Real.Create(EDecimal.FromSingle(value));
            public static implicit operator Number(double value) => Real.Create(EDecimal.FromDouble(value));
            public static implicit operator Number(decimal value) => Real.Create(EDecimal.FromDecimal(value));
            public static implicit operator Number(System.Numerics.Complex value) =>
                Complex.Create(EDecimal.FromDouble(value.Real), EDecimal.FromDouble(value.Imaginary));
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        }
    }
}
