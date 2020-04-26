using AngouriMath;
using AngouriMath.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Convenience
{
    [TestClass]
    public class CompilationTest
    {
        private readonly static VariableEntity x = MathS.Var("x");
        private readonly static VariableEntity y = MathS.Var("y");
        [TestMethod]
        public void Test1()
        {
            var func = (x + MathS.Sqrt(x)).Compile(x);
            Assert.IsTrue(func.Substitute(4) == 6);
        }
        [TestMethod]
        public void Test2()
        {
            var func = (MathS.Sin(x) + MathS.Cos(x)).Compile(x);
            Assert.IsTrue(func.Substitute(0) == 1);
        }
        [TestMethod]
        public void Test3()
        {
            var func = (x / y).Compile(x, y);
            Assert.IsTrue(func.Substitute(1, 2) == 0.5);
        }
        [TestMethod]
        public void Test4()
        {
            var func = (x / y).Compile(y, x);
            Assert.IsTrue(func.Substitute(1, 2) == 2.0);
        }
        [TestMethod]
        public void Test5()
        {
            var func = ((x + y) / (x - 3)).Compile(x, y);
            Assert.IsTrue(func.Substitute(4, 3) == 7.0);
        }
        [TestMethod]
        public void Test6()
        {
            var expr = (MathS.Sqr(x) + MathS.Sqr(x)) / MathS.Sqr(x) + MathS.Sqrt(x);
            var func = expr.Compile(x);
            Assert.IsTrue(func.Call(4) == 4);
        }
        [TestMethod]
        public void Test7()
        {
            var expr = MathS.pi + MathS.e + x;
            var func = expr.Compile(x);
            Assert.IsTrue(func.Call(3).Re > 7 && func.Call(3).Re < 10);
        }
    }
}
