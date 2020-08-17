using AngouriMath;
using AngouriMath.Core.Numerix;
using Xunit;

namespace UnitTests.Algebra
{
    public class FunctionTest
    {
        // Testing function GetAllRoots
        [Fact]
        public void TestRoots0()
        {
            var num = 3;
            var pow = 3;
            foreach (var root in Entity.Number.GetAllRoots(num, pow))
                Assert.Equal(num, Entity.Number.Pow(root, pow));
        }
        [Fact]
        public void TestRoots1()
        {
            var num = 5 + MathS.i * 5;
            var pow = 4;
            foreach (var root in Entity.Number.GetAllRoots(num, pow))
                Assert.Equal(num, Entity.Number.Pow(root, pow));
        }
        [Fact]
        public void TestRoots2()
        {
            var num = -3 + MathS.i * 8;
            var pow = 5;
            foreach (var root in Entity.Number.GetAllRoots(num, pow))
                Assert.Equal(num, Entity.Number.Pow(root, pow));
        }
        [Fact]
        public void TestRoots3()
        {
            var num = -3 + MathS.i * 8;
            var pow = 8;
            foreach (var root in Entity.Number.GetAllRoots(num, pow))
                Assert.Equal(num, Entity.Number.Pow(root, pow));
        }

        // Testing functions of Base Convert
        [Fact]
        public void TestBaseConvertTo0()
        {
            Assert.Equal("101", MathS.ToBaseN(5, 2));
            Assert.Equal("F", MathS.ToBaseN(15, 16));
        }
        [Fact]
        public void TestBaseConvertTo1()
        {
            Assert.Equal("-101", MathS.ToBaseN(-5, 2));
            Assert.Equal("-F", MathS.ToBaseN(-15, 16));
            Assert.Equal("-10001100", MathS.ToBaseN(-140, 2));
            Assert.Equal("-12012", MathS.ToBaseN(-140, 3));
            Assert.Equal("-2030", MathS.ToBaseN(-140, 4));
            Assert.Equal("-1030", MathS.ToBaseN(-140, 5));
            Assert.Equal("-352", MathS.ToBaseN(-140, 6));
            Assert.Equal("-260", MathS.ToBaseN(-140, 7));
            Assert.Equal("-214", MathS.ToBaseN(-140, 8));
            Assert.Equal("-165", MathS.ToBaseN(-140, 9));
            Assert.Equal("-140", MathS.ToBaseN(-140, 10));
            Assert.Equal("-118", MathS.ToBaseN(-140, 11));
            Assert.Equal("-B8", MathS.ToBaseN(-140, 12));
            Assert.Equal("-AA", MathS.ToBaseN(-140, 13));
            Assert.Equal("-A0", MathS.ToBaseN(-140, 14));
            Assert.Equal("-95", MathS.ToBaseN(-140, 15));
            Assert.Equal("-8C", MathS.ToBaseN(-140, 16));
        }
        [Fact]
        public void TestBaseConvertTo2()
        {
            Assert.Equal("1000.001", MathS.ToBaseN(8.125m, 2));
            Assert.Equal("20.02", MathS.ToBaseN(8.125m, 4));
            Assert.Equal("12.043", MathS.ToBaseN(8.125m, 6));
            Assert.Equal("10.1", MathS.ToBaseN(8.125m, 8));
            Assert.Equal("8.125", MathS.ToBaseN(8.125m, 10));
            Assert.Equal("8.16", MathS.ToBaseN(8.125m, 12));
            Assert.Equal("8.1A7", MathS.ToBaseN(8.125m, 14));
            Assert.Equal("8.2", MathS.ToBaseN(8.125m, 16));
            Assert.Equal("1000.111", MathS.ToBaseN(8.875m, 2));
            Assert.Equal("20.32", MathS.ToBaseN(8.875m, 4));
            Assert.Equal("12.513", MathS.ToBaseN(8.875m, 6));
            Assert.Equal("10.7", MathS.ToBaseN(8.875m, 8));
            Assert.Equal("8.875", MathS.ToBaseN(8.875m, 10));
            Assert.Equal("8.A6", MathS.ToBaseN(8.875m, 12));
            Assert.Equal("8.C37", MathS.ToBaseN(8.875m, 14));
            Assert.Equal("8.E", MathS.ToBaseN(8.875m, 16));
        }
        [Fact]
        public void TestBaseConvertTo3()
        {
            Assert.Equal("-1000.001", MathS.ToBaseN(-8.125m, 2));
            Assert.Equal("-20.02", MathS.ToBaseN(-8.125m, 4));
            Assert.Equal("-12.043", MathS.ToBaseN(-8.125m, 6));
            Assert.Equal("-10.1", MathS.ToBaseN(-8.125m, 8));
            Assert.Equal("-8.125", MathS.ToBaseN(-8.125m, 10));
            Assert.Equal("-8.16", MathS.ToBaseN(-8.125m, 12));
            Assert.Equal("-8.1A7", MathS.ToBaseN(-8.125m, 14));
            Assert.Equal("-8.2", MathS.ToBaseN(-8.125m, 16));
            Assert.Equal("-1000.111", MathS.ToBaseN(-8.875m, 2));
            Assert.Equal("-20.32", MathS.ToBaseN(-8.875m, 4));
            Assert.Equal("-12.513", MathS.ToBaseN(-8.875m, 6));
            Assert.Equal("-10.7", MathS.ToBaseN(-8.875m, 8));
            Assert.Equal("-8.875", MathS.ToBaseN(-8.875m, 10));
            Assert.Equal("-8.A6", MathS.ToBaseN(-8.875m, 12));
            Assert.Equal("-8.C37", MathS.ToBaseN(-8.875m, 14));
            Assert.Equal("-8.E", MathS.ToBaseN(-8.875m, 16));
        }
        [Fact]
        public void TestBaseConvertFrom0()
        {
            Assert.Equal(10, MathS.FromBaseN("A", 16));
            Assert.Equal(10, MathS.FromBaseN("1010", 2));
        }
        [Fact]
        public void TestBaseConvertFrom1()
        {
            Assert.Equal(-10.25m, MathS.FromBaseN("-A.4", 16));
            Assert.Equal(-140, MathS.FromBaseN("-A0", 14));
        }
        [Fact]
        public void TestBaseConvertFrom2()
        {
            Assert.Equal(-0.125m, MathS.FromBaseN("-0.125", 10));
            Assert.Equal(0.25m, MathS.FromBaseN("0.3", 12));
        }
    }
}