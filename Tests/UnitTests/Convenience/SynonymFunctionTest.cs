using AngouriMath;
using Xunit;

namespace UnitTests.Convenience
{
    public class SynonymFunctionTest
    {
        private readonly Entity x = MathS.Var(nameof(x));
        [Fact]
        public void TestSqrt()
        {
            var expr = MathS.FromString("sqrt(x)");
            Assert.Equal(MathS.Sqrt(x), expr);
        }
        [Fact]
        public void TestComplex()
        {
            var expr = MathS.FromString("ln(x) + sqrt(x) + tan(x) + sec(x) + cosec(x) + cotan(x)");
            var expected = MathS.Ln(x) + MathS.Sqrt(x) +
                MathS.Tan(x) + MathS.Sec(x) + MathS.Cosec(x) +
                MathS.Cotan(x);
            Assert.Equal(expected, expr);
        }
        [Fact]
        public void TestInner()
        {
            var expr = MathS.FromString("sqrt(x + sqrt(x))");
            var expected = MathS.Sqrt(x + MathS.Sqrt(x));
            Assert.Equal(expected, expr);
        }
        [Fact]
        public void TestInnerComplex()
        {
            var expr = MathS.FromString("cotan(x + tan(x))");
            var expected = MathS.Cotan(x + MathS.Tan(x));
            Assert.Equal(expected, expr);
        }
    }
}
