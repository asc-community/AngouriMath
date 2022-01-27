//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using PeterO.Numbers;
using System;
using Xunit;

namespace AngouriMath.Tests.Common
{
    public sealed class ImplicitOperators
    {
        internal void Test(Entity expr, Entity expected) => Assert.Equal(expected, expr);

        [Fact] public void FromByte() => Test((byte)3, "3");
        [Fact] public void FromSByte() => Test((sbyte)3, "3");
        [Fact] public void FromInt16() => Test((Int16)3, "3");
        [Fact] public void FromUInt16() => Test((UInt16)3, "3");
        [Fact] public void FromInt32() => Test((Int32)3, "3");
        [Fact] public void FromUInt32() => Test((UInt32)3, "3");
        [Fact] public void FromInt64() => Test((Int64)3, "3");
        [Fact] public void FromUInt64() => Test((UInt64)3, "3");
        [Fact] public void FromSingle() => Test((Single)3.5f, "3.5");
        [Fact] public void FromDouble() => Test((Double)3.5, "3.5");
        [Fact] public void FromDecimal() => Test((Decimal)3.5m, "3.5");
        [Fact] public void FromComplex() => Test(new System.Numerics.Complex(3, 5), Entity.Number.Complex.Create(3, 5));
        [Fact] public void FromBigInteger() => Test(System.Numerics.BigInteger.Parse("28374832742384"), "28374832742384");
        [Fact] public void FromEInteger() => Test(EInteger.FromString("32324"), "32324");
        [Fact] public void FromERational() => Test(ERational.Create(EInteger.FromString("32324"), EInteger.FromString("243244")), ((Entity)"32324/243244").InnerSimplified);
        [Fact] public void FromEDecimal() => Test(EDecimal.FromString("3.4"), "3.4");
    }
}
