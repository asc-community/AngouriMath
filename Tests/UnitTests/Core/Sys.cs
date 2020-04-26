using AngouriMath;
using AngouriMath.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Core
{
    [TestClass]
    public class Sys
    {
        [TestMethod]
        public void NumberTest1()
        {
            Number a = 3;
            Number b = MathS.i;
            Number c = new Number(a + b);
            Assert.IsTrue(c == new Number(3, 1));
        }

        Number dst;

        [TestMethod]
        public void TestNumber1()
            => Assert.IsFalse(Number.TryParse("quack", out dst));

        [TestMethod]
        public void TestNumber2()
            => Assert.IsFalse(Number.TryParse("i1", out dst));

        [TestMethod]
        public void TestNumber3()
            => Assert.IsFalse(Number.TryParse("ii", out dst));

        [TestMethod]
        public void TestNumber4()
            => Assert.IsFalse(Number.TryParse("", out dst));

        [TestMethod]
        public void TestNumber5()
            => Assert.IsTrue(Number.TryParse("233i", out dst));

        [TestMethod]
        public void TestNumber6()
            => Assert.IsTrue(Number.TryParse("-4i", out dst));

        [TestMethod]
        public void TestNumber7()
            => Assert.IsTrue(Number.TryParse("-i", out dst));

        public void TestNumber8()
            => Assert.IsTrue(Number.TryParse("-5.3i", out dst));

        public void TestNumber9()
            => Assert.IsTrue(Number.TryParse("5.3", out dst));
    }
}
