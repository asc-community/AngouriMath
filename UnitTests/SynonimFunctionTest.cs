using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Core.FromString;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class SynonimFunctionTest
    {
        private readonly Entity x = MathS.Var("x");
        private readonly Entity y = MathS.Var("y");
        [TestMethod]
        public void TestSqrt()
        {
            var expr = MathS.FromString("sqrt(x)");
            Assert.IsTrue(expr == MathS.Sqrt(x));
        }
        [TestMethod]
        public void TestComplex()
        {
            var expr = MathS.FromString("ln(x) + sqrt(x) + tan(x) + b(x) + sec(x) + cosec(x) + cotan(x)");
            var expected = MathS.Ln(x) + MathS.Sqrt(x) +
                MathS.Tan(x) + MathS.B(x) + MathS.Sec(x) + MathS.Cosec(x) +
                MathS.Cotan(x);
            Assert.IsTrue(expr == expected);
        }
        [TestMethod]
        public void TestInner()
        {
            var expr = MathS.FromString("sqrt(x + sqrt(x))");
            var expected = MathS.Sqrt(x + MathS.Sqrt(x));
            Assert.IsTrue(expr == expected);
        }
        [TestMethod]
        public void TestInnerComplex()
        {
            var expr = MathS.FromString("cotan(x + tan(x))");
            var expected = MathS.Cotan(x + MathS.Tan(x));
            Assert.IsTrue(expr == expected);
        }
    }
}
