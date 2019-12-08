using AngouriMath;
using AngouriMath.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class SubstituteTest
    {
        [TestMethod]
        public void Test1()
        {
            var x = MathS.Var("x");
            var expr = x * x + MathS.Sin(x) * 0;
            Assert.IsTrue(expr.Substitute(x, 0).Simplify() == 0);
        }
        [TestMethod]
        public void Test2()
        {
            var x = MathS.Var("x");
            var y = MathS.Var("y");
            var expr = x.Pow(x) - MathS.Sqrt(x - 3) / x + MathS.Sin(x);
            var expected = (3 * y).Pow(3 * y) - MathS.Sqrt(3 * y - 3) / (3 * y) + MathS.Sin(3 * y);
            var actual = expr.Substitute(x, 3 * y);
            Assert.IsTrue(expected == actual);
        }
    }
}
