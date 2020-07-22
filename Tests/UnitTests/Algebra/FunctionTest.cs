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
        }
        [TestMethod]
        public void TestNSTo2()
        {
            Assert.AreEqual("-1000.001", MathS.ToBaseN(-8.125m, 2));
            Assert.AreEqual("-10.1", MathS.ToBaseN(-8.125m, 8));
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
