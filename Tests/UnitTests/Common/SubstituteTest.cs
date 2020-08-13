using AngouriMath;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Common
{
    [TestClass]
    public class SubstituteTest
    {
        static readonly Entity.Var x = nameof(x);
        [TestMethod]
        public void Test1()
        {
            var expr = x * x + MathS.Sin(x) * 0;
            Assert.IsTrue(expr.Substitute(x, 0).Simplify() == 0);
        }
        [TestMethod]
        public void Test2()
        {
            var y = MathS.Var("y");
            var expr = x.Pow(x) - MathS.Sqrt(x - 3) / x + MathS.Sin(x);
            var expected = (3 * y).Pow(3 * y) - MathS.Sqrt(3 * y - 3) / (3 * y) + MathS.Sin(3 * y);
            var actual = expr.Substitute(x, 3 * y);
            Assert.IsTrue(expected == actual);
        }
        [TestMethod]
        public void Test3()
        {
            var expr = x;
            Assert.IsTrue(expr.Substitute(x, 0) == 0);
        }
    }
}
