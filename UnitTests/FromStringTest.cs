using MathSharp;
using MathSharp.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class FromStringTest
    {
        [TestMethod]
        public void Test1()
        {
            Assert.IsTrue(MathS.FromString("1 + 1").Eval() == 2);
        }
        [TestMethod]
        public void Test2()
        {
            Assert.IsTrue(MathS.FromString("sin(0)").Eval() == 0);
        }
        [TestMethod]
        public void Test3()
        {
            Assert.IsTrue(MathS.FromString("log(4, 2)").Eval() == 2);
        }
        [TestMethod]
        public void Test4()
        {
            Assert.IsTrue(MathS.FromString("sin(x)").Derive(MathS.Var("x")).Simplify() == MathS.Cos(MathS.Var("x")));
        }
        [TestMethod]
        public void Test5()
        {
            Assert.IsTrue(MathS.FromString("3 ^ 3 ^ 3").Eval().GetValue().Re > 10000);
        }
        [TestMethod]
        public void Test6()
        {
            Assert.IsTrue(MathS.FromString("2 + 2 * 2").Eval() == 6);
        }
    }
}
