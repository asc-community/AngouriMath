using AngouriMath;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Convenience
{
    [TestClass]
    public class CompilationTest
    {
        private static readonly VariableEntity x = MathS.Var("x");
        private static readonly VariableEntity y = MathS.Var("y");
        [TestMethod]
        public void Test1()
        {
            var func = (x + MathS.Sqrt(x)).Compile(x);
            Assert.AreEqual(6, func.Substitute(4));
        }
        [TestMethod]
        public void Test2()
        {
            var func = (MathS.Sin(x) + MathS.Cos(x)).Compile(x);
            Assert.AreEqual(1, func.Substitute(0));
        }
        [TestMethod]
        public void Test3()
        {
            var func = (x / y).Compile(x, y);
            Assert.AreEqual(0.5, func.Substitute(1, 2));
        }
        [TestMethod]
        public void Test4()
        {
            var func = (x / y).Compile(y, x);
            Assert.AreEqual(2.0, func.Substitute(1, 2));
        }
        [TestMethod]
        public void Test5()
        {
            var func = ((x + y) / (x - 3)).Compile(x, y);
            Assert.AreEqual(7.0, func.Substitute(4, 3));
        }
        [TestMethod]
        public void Test6()
        {
            var expr = (MathS.Sqr(x) + MathS.Sqr(x)) / MathS.Sqr(x) + MathS.Sqrt(x);
            var func = expr.Compile(x);
            Assert.AreEqual(4, func.Call(4));
        }
        [TestMethod]
        public void Test7()
        {
            var expr = MathS.pi + MathS.e + x;
            var func = expr.Compile(x);
            Assert.IsTrue(func.Call(3).Real > 7 && func.Call(3).Real < 10);
        }
    }
}
