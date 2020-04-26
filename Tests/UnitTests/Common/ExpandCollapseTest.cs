using AngouriMath;
using AngouriMath.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

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
            Assert.IsTrue(expr.Expand().Substitute(x, 5).Substitute(y, 3).Eval() == 16);
        }
        [TestMethod]
        public void Test2()
        {
            var expr = (x + y + x + y) * (x - y + x - y);
            Assert.IsTrue(expr.Expand().Substitute(x, 5).Substitute(y, 3).Eval() == 64);
        }
        [TestMethod]
        public void Test3()
        {
            var expr = x * y + x;
            Assert.IsTrue(expr.Collapse() == x * (1 + y));
        }
    }
}
