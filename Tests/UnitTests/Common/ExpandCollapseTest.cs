using AngouriMath;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Common
{
    [TestClass]
    public class ExpandCollapseTest
    {
        public static readonly Entity.Var x = MathS.Var("x");
        public static readonly Entity.Var y = MathS.Var("y");
        [TestMethod]
        public void Test1()
        {
            var expr = (x + y) * (x - y);
            Assert.AreEqual(16, expr.Expand().Substitute(x, 5).Substitute(y, 3).Eval());
        }
        [TestMethod]
        public void Test2()
        {
            var expr = (x + y + x + y) * (x - y + x - y);
            Assert.AreEqual(64, expr.Expand().Substitute(x, 5).Substitute(y, 3).Eval());
        }
        [TestMethod]
        public void Test3()
        {
            var expr = x * y + x;
            Assert.AreEqual(x * (1 + y), expr.Collapse());
        }
        [TestMethod]
        public void Factorial() 
        {
            var expr = MathS.Factorial(x + 3) / MathS.Factorial(x + 1);
            Assert.AreEqual(x * x + x * 3 + 2 * x + 6, expr.Expand());
            expr = MathS.Factorial(x + -3) / MathS.Factorial(x + -1);
            Assert.AreEqual(1 / (x + -2) / (x + -1), expr.Expand());
        }
    }
}
