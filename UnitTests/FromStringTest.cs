using AngouriMath;
using AngouriMath.Core;
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
        [TestMethod]
        public void Test7()
        {
            Assert.IsTrue(MathS.FromString("x^2+1").SolveNt(MathS.Var("x"))[0] == MathS.i);
        }
        [TestMethod]
        public void Test8()
        {
            Assert.IsTrue(MathS.FromString("cos(sin(0))").Eval() == 1);
        }
        [TestMethod]
        public void Test9()
        {
            Assert.IsTrue(MathS.FromString("2i + 2 * 2 - 1i").Eval() == new Number(4, 1));
        }
        [TestMethod]
        public void Test10()
        {
            Assert.IsTrue(MathS.FromString("i^2").Eval() == -1);
        }
        [TestMethod]
        public void Test11()
        {
            Assert.IsTrue(MathS.FromString("x^2-1").Substitute(MathS.Var("x"), 2).Eval() == 3);
        }
    }
}
