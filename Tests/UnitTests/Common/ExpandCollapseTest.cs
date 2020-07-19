using AngouriMath;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Common
{
    [TestClass]
    public class ExpandCollapseTest
    {
        public static readonly VariableEntity x = MathS.Var("x");
        public static readonly VariableEntity y = MathS.Var("y");
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
    }
}
