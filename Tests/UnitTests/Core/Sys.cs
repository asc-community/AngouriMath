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
    }
}
