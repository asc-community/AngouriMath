using MathSharp;
using MathSharp.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class OperatorTest
    {
        [TestMethod]
        public void TestEq()
        {
            Assert.IsTrue(MathS.Var("x") == MathS.Var("x"));
        }
        [TestMethod]
        public void TestIneq()
        {
            Assert.IsTrue(MathS.Var("x") != MathS.Var("y"));
        }
        [TestMethod]
        public void TestR()
        {
            Assert.IsTrue(new Number(0, 1) == new Number(0, (1.0/3) * 3));
        }
    }
}
