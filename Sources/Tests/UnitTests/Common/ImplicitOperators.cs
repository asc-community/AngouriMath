using AngouriMath;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace UnitTests.Common
{
    public sealed class ImplicitOperators
    {
        public void Test(Entity expr, Entity expected) => Assert.Equal(expected, expr);

        [Fact] public void FromByte() => Test((byte)3, "3");
        [Fact] public void FromSByte() => Test((sbyte)3, "3");
        [Fact] public void FromInt16() => Test((Int16)3, "3");
        [Fact] public void FromUInt16() => Test((UInt16)3, "3");
        [Fact] public void FromInt32() => Test((Int32)3, "3");
        [Fact] public void FromUInt32() => Test((UInt32)3, "3");
        [Fact] public void FromInt64() => Test((Int64)3, "3");
        [Fact] public void FromUInt64() => Test((UInt64)3, "3");
    }
}
