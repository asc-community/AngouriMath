//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using PeterO.Numbers;

namespace AngouriMath
{
    partial record Entity
    {
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
        public static implicit operator Entity(System.Numerics.Complex value) =>
            Number.Complex.Create(EDecimal.FromDouble(value.Real), EDecimal.FromDouble(value.Imaginary));
        public static implicit operator Entity(System.Numerics.BigInteger bigInt)
            => Number.Integer.Create(EInteger.FromBytes(bigInt.ToByteArray(), littleEndian: true));
#pragma warning restore CS1591
    }
}
