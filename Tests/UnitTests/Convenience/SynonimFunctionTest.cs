using AngouriMath;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Convenience
{
    [TestClass]
    public class SynonimFunctionTest
    {
        private readonly Entity x = MathS.Var("x");
        [TestMethod]
        public void TestSqrt()
        {
            var expr = MathS.FromString("sqrt(x)");
            Assert.AreEqual(MathS.Sqrt(x), expr);
        }
        [TestMethod]
        public void TestComplex()
        {
            var expr = MathS.FromString("ln(x) + sqrt(x) + tan(x) + sec(x) + cosec(x) + cotan(x)");
            var expected = MathS.Ln(x) + MathS.Sqrt(x) +
                MathS.Tan(x) + MathS.Sec(x) + MathS.Cosec(x) +
                MathS.Cotan(x);
            Assert.AreEqual(expected, expr);
        }
        [TestMethod]
        public void TestInner()
        {
            var expr = MathS.FromString("sqrt(x + sqrt(x))");
            var expected = MathS.Sqrt(x + MathS.Sqrt(x));
            Assert.AreEqual(expected, expr);
        }
        [TestMethod]
        public void TestInnerComplex()
        {
            var expr = MathS.FromString("cotan(x + tan(x))");
            var expected = MathS.Cotan(x + MathS.Tan(x));
            Assert.AreEqual(expected, expr);
        }
    }
}
