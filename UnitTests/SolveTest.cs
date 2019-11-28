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
            Assert.IsTrue(s.Eval() == 3);
        }
        [TestMethod]
        public void Test2()
        {
            var x = MathS.Var("x");
            var eq = MathS.Sqr(x) + 1;
            var roots = eq.SolveNt(x);
            Assert.IsTrue(roots.Count == 1);
            Assert.IsTrue(roots[0].Eval() == MathS.i);
        }
    }
}
