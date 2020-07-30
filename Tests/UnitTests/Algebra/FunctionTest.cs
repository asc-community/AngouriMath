using AngouriMath;
using AngouriMath.Core.Numerix;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Algebra
{
    [TestClass]
    public class FunctionTest
    {
        // Testing function GetAllRoots
        [TestMethod]
        public void TestRoots0()
        {
            var num = 3;
            var pow = 3;
            foreach (var root in Number.GetAllRoots(num, pow).Select(piece => ((NumberEntity)piece).Value))
                Assert.AreEqual(num, Number.Pow(root, pow));
        }
        [TestMethod]
        public void TestRoots1()
        {
            var num = 5 + MathS.i * 5;
            var pow = 4;
            foreach (var root in Number.GetAllRoots(num, pow).Select(piece => ((NumberEntity)piece).Value))
                Assert.AreEqual(num, Number.Pow(root, pow));
        }
        [TestMethod]
        public void TestRoots2()
        {
            var num = -3 + MathS.i * 8;
            var pow = 5;
            foreach (var root in Number.GetAllRoots(num, pow).Select(piece => ((NumberEntity)piece).Value))
                Assert.AreEqual(num, Number.Pow(root, pow));
        }
        [TestMethod]
        public void TestRoots3()
        {
            var num = -3 + MathS.i * 8;
            var pow = 8;
            foreach (var root in Number.GetAllRoots(num, pow).Select(piece => ((NumberEntity)piece).Value))
                Assert.AreEqual(num, Number.Pow(root, pow));
        }


        // Testing functions of Number System
        [TestMethod]
        public void TestNSTo0()
        {
            Assert.AreEqual("101", MathS.ToBaseN(5, 2));
            Assert.AreEqual("F", MathS.ToBaseN(15, 16));
        }
        [TestMethod]
        public void TestNSTo1()
        {
            Assert.AreEqual("-101", MathS.ToBaseN(-5, 2));
            Assert.AreEqual("-F", MathS.ToBaseN(-15, 16));
            Assert.AreEqual("-10001100", MathS.ToBaseN(-140, 2));
            Assert.AreEqual("-12012", MathS.ToBaseN(-140, 3));
            Assert.AreEqual("-2030", MathS.ToBaseN(-140, 4));
            Assert.AreEqual("-1030", MathS.ToBaseN(-140, 5));
            Assert.AreEqual("-352", MathS.ToBaseN(-140, 6));
            Assert.AreEqual("-260", MathS.ToBaseN(-140, 7));
            Assert.AreEqual("-214", MathS.ToBaseN(-140, 8));
            Assert.AreEqual("-165", MathS.ToBaseN(-140, 9));
            Assert.AreEqual("-140", MathS.ToBaseN(-140, 10));
            Assert.AreEqual("-118", MathS.ToBaseN(-140, 11));
            Assert.AreEqual("-B8", MathS.ToBaseN(-140, 12));
            Assert.AreEqual("-AA", MathS.ToBaseN(-140, 13));
            Assert.AreEqual("-A0", MathS.ToBaseN(-140, 14));
            Assert.AreEqual("-95", MathS.ToBaseN(-140, 15));
            Assert.AreEqual("-8C", MathS.ToBaseN(-140, 16));
        }
        [TestMethod]
        public void TestNSTo2()
        {
            Assert.AreEqual("1000.001", MathS.ToBaseN(8.125m, 2));
            Assert.AreEqual("20.02", MathS.ToBaseN(8.125m, 4));
            Assert.AreEqual("12.043", MathS.ToBaseN(8.125m, 6));
            Assert.AreEqual("10.1", MathS.ToBaseN(8.125m, 8));
            Assert.AreEqual("8.125", MathS.ToBaseN(8.125m, 10));
            Assert.AreEqual("8.16", MathS.ToBaseN(8.125m, 12));
            Assert.AreEqual("8.1A7", MathS.ToBaseN(8.125m, 14));
            Assert.AreEqual("8.2", MathS.ToBaseN(8.125m, 16));
            Assert.AreEqual("1000.111", MathS.ToBaseN(8.875m, 2));
            Assert.AreEqual("20.32", MathS.ToBaseN(8.875m, 4));
            Assert.AreEqual("12.513", MathS.ToBaseN(8.875m, 6));
            Assert.AreEqual("10.7", MathS.ToBaseN(8.875m, 8));
            Assert.AreEqual("8.875", MathS.ToBaseN(8.875m, 10));
            Assert.AreEqual("8.A6", MathS.ToBaseN(8.875m, 12));
            Assert.AreEqual("8.C37", MathS.ToBaseN(8.875m, 14));
            Assert.AreEqual("8.E", MathS.ToBaseN(8.875m, 16));
        }
        [TestMethod]
        public void TestNSTo3()
        {
            Assert.AreEqual("-1000.001", MathS.ToBaseN(-8.125m, 2));
            Assert.AreEqual("-20.02", MathS.ToBaseN(-8.125m, 4));
            Assert.AreEqual("-12.043", MathS.ToBaseN(-8.125m, 6));
            Assert.AreEqual("-10.1", MathS.ToBaseN(-8.125m, 8));
            Assert.AreEqual("-8.125", MathS.ToBaseN(-8.125m, 10));
            Assert.AreEqual("-8.16", MathS.ToBaseN(-8.125m, 12));
            Assert.AreEqual("-8.1A7", MathS.ToBaseN(-8.125m, 14));
            Assert.AreEqual("-8.2", MathS.ToBaseN(-8.125m, 16));
            Assert.AreEqual("-1000.111", MathS.ToBaseN(-8.875m, 2));
            Assert.AreEqual("-20.32", MathS.ToBaseN(-8.875m, 4));
            Assert.AreEqual("-12.513", MathS.ToBaseN(-8.875m, 6));
            Assert.AreEqual("-10.7", MathS.ToBaseN(-8.875m, 8));
            Assert.AreEqual("-8.875", MathS.ToBaseN(-8.875m, 10));
            Assert.AreEqual("-8.A6", MathS.ToBaseN(-8.875m, 12));
            Assert.AreEqual("-8.C37", MathS.ToBaseN(-8.875m, 14));
            Assert.AreEqual("-8.E", MathS.ToBaseN(-8.875m, 16));
        }
        [TestMethod]
        public void TestNSFrom0()
        {
            Assert.AreEqual(10, MathS.FromBaseN("A", 16));
            Assert.AreEqual(10, MathS.FromBaseN("1010", 2));
        }
        [TestMethod]
        public void TestNSFrom1()
        {
            Assert.AreEqual(-10.25m, MathS.FromBaseN("-A.4", 16));
            Assert.AreEqual(-140, MathS.FromBaseN("-A0", 14));
        }
        [TestMethod]
        public void TestNSFrom2()
        {
            Assert.AreEqual(-0.125m, MathS.FromBaseN("-0.125", 10));
            Assert.AreEqual(0.25m, MathS.FromBaseN("0.3", 12));
        }
    }
}
