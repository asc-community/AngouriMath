using AngouriMath;
using AngouriMath.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTests
{
    [TestClass]
    public class IntegrationTest
    {
        [TestMethod]
        public void Test1()
        {
            var x = MathS.Var("x");
            var expr = x;
            Assert.IsTrue(Math.Abs(expr.DefiniteIntegral(expr, x, 0, 1).Re - 1.0/2) < 0.1);
        }
        [TestMethod]
        public void Test2()
        {
            var x = MathS.Var("x");
            var expr = MathS.Sin(x);
            Assert.IsTrue(expr.DefiniteIntegral(expr, x, -1, 1) == 0);
        }
        [TestMethod]
        public void Test3()
        {
            var x = MathS.Var("x");
            var expr = MathS.Sin(x);
            Assert.IsTrue(expr.DefiniteIntegral(expr, x, 0, 3).Re > 1.5);
        }
    }
}
