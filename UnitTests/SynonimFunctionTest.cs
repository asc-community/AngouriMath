using MathSharp;
using MathSharp.Core;
using MathSharp.Core.FromString;
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
    }
}
