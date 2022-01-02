//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

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
        public abstract partial record Number : ContinuousNode
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
            public static implicit operator Number(System.Numerics.Complex value)
                => Complex.Create(EDecimal.FromDouble(value.Real), EDecimal.FromDouble(value.Imaginary));
            public static implicit operator Number(System.Numerics.BigInteger bigInt)
                => Integer.Create(EInteger.FromString(bigInt.ToByteArray()));
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        }
    }
}
