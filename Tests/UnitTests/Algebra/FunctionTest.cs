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
                Assert.IsTrue(Number.Pow(root, pow) == num, "Found root " + root + " and it is equal to " + Number.Pow(root, pow));
        }
        [TestMethod]
        public void TestRoots1()
        {
            var num = 5 + MathS.i * 5;
            var pow = 4;
            foreach (var root in Number.GetAllRoots(num, pow).Select(piece => ((NumberEntity)piece).Value))
                Assert.IsTrue(Number.Pow(root, pow) == num, "Found root " + root + " and it is equal to " + Number.Pow(root, pow));
        }
        [TestMethod]
        public void TestRoots2()
        {
            var num = -3 + MathS.i * 8;
            var pow = 5;
            foreach (var root in Number.GetAllRoots(num, pow).Select(piece => ((NumberEntity)piece).Value))
                Assert.IsTrue(Number.Pow(root, pow) == num, "Found root " + root + " and it is equal to " + Number.Pow(root, pow));
        }
        [TestMethod]
        public void TestRoots3()
        {
            var num = -3 + MathS.i * 8;
            var pow = 8;
            foreach (var root in Number.GetAllRoots(num, pow).Select(piece => ((NumberEntity)piece).Value))
                Assert.IsTrue(Number.Pow(root, pow) == num, "Found root " + root + " and it is equal to " + Number.Pow(root, pow));
        }


        // Testing functions of Number System
        [TestMethod]
        public void TestNSTo0()
        {
            Assert.IsTrue(MathS.ToBaseN(5, 2) == "101");
            Assert.IsTrue(MathS.ToBaseN(15, 16) == "F");
        }
        [TestMethod]
        public void TestNSTo1()
        {
            Assert.IsTrue(MathS.ToBaseN(-5, 2) == "-101");
            Assert.IsTrue(MathS.ToBaseN(-15, 16) == "-F");
        }
        [TestMethod]
        public void TestNSTo2()
        {
            Assert.IsTrue(MathS.ToBaseN(-8.125m, 2) == "-1000.001");
            Assert.IsTrue(MathS.ToBaseN(-8.125m, 8) == "-10.1");
        }
        [TestMethod]
        public void TestNSFrom0()
        {
            Assert.IsTrue(MathS.FromBaseN("A", 16) == 10);
            Assert.IsTrue(MathS.FromBaseN("1010", 2) == 10);
        }
        [TestMethod]
        public void TestNSFrom1()
        {
            Assert.IsTrue(MathS.FromBaseN("-A.4", 16) == -10.25m);
            Assert.IsTrue(MathS.FromBaseN("-A0", 14) == -140);
        }
        [TestMethod]
        public void TestNSFrom2()
        {
            Assert.IsTrue(MathS.FromBaseN("-0.125", 10) == -0.125m);
            Assert.IsTrue(MathS.FromBaseN("0.3", 12) == 0.25m);
        }
    }
}
