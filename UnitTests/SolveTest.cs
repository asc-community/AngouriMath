using MathSharp;
using MathSharp.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class SolveTest
    {
        [TestMethod]
        public void Test1()
        {
            var x = MathS.Var("x");
            var eq = (x - 1) * (x - 2);
            var roots = eq.SolveNt(x);
            Assert.IsTrue(roots.Count == 2);
            var s = roots[0] + roots[1];
            Assert.IsTrue(s == 3);
        }
        [TestMethod]
        public void Test2()
        {
            var x = MathS.Var("x");
            var eq = MathS.Sqr(x) + 1;
            var roots = eq.SolveNt(x);
            Assert.IsTrue(roots.Count == 2);
            Assert.IsTrue(roots[0] == MathS.i);
            Assert.IsTrue(roots[1] == new Number(0, -1));
        }
        [TestMethod]
        public void Test3()
        {
            var x = MathS.Var("x");
            var eq = MathS.Num(1);
            var roots = eq.SolveNt(x);
            Assert.IsTrue(roots.Count == 0);
        }
        [TestMethod]
        public void Test4()
        {
            var x = MathS.Var("x");
            var eq = x.Pow(2) + 2 * x + 1;
            MathS.EQUALITY_THRESHOLD = 1.0e-6;
            var roots = eq.SolveNt(x, precision: 100);
            MathS.EQUALITY_THRESHOLD = 1.0e-11;
            Assert.IsTrue(roots.Count == 1);
        }
    }
}
