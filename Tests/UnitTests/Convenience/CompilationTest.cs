using AngouriMath;
using Xunit;

namespace UnitTests.Convenience
{
    public class CompilationTest
    {
        private static readonly Entity.Variable x = MathS.Var(nameof(x));
        private static readonly Entity.Variable y = MathS.Var(nameof(y));
        [Fact]
        public void Test1()
        {
            var func = (x + MathS.Sqrt(x)).Compile(x);
            Assert.Equal(6, func.Substitute(4));
        }

        [Fact]
        public void Test2()
        {
            var func = (MathS.Sin(x) + MathS.Cos(x)).Compile(x);
            Assert.Equal(1, func.Substitute(0));
        }

        [Fact]
        public void Test3()
        {
            var func = (x / y).Compile(x, y);
            Assert.Equal(0.5, func.Substitute(1, 2));
        }

        [Fact]
        public void Test4()
        {
            var func = (x / y).Compile(y, x);
            Assert.Equal(2.0, func.Substitute(1, 2));
        }

        [Fact]
        public void Test5()
        {
            var func = ((x + y) / (x - 3)).Compile(x, y);
            Assert.Equal(7.0, func.Substitute(4, 3));
        }

        [Fact]
        public void Test6()
        {
            // Caching with one value
            var expr = (MathS.Sqr(x) + MathS.Sqr(x)) / MathS.Sqr(x) + MathS.Sqrt(x);
            var func = expr.Compile(x);
            Assert.Equal(4, func.Call(4));
        }

        [Fact]
        public void TestLong()
        {
            // Caching with multiple values
            var expr = (MathS.Sqr(x) + MathS.Sqr(x)) / MathS.Sqr(x)
                       + MathS.Sqrt(x) + MathS.Cbrt(x) * MathS.Cbrt(x) + MathS.Sqrt(x);
            var func = expr.Compile(x);
            Assert.Equal(34, func.Call(64));
        }

        [Fact]
        public void TestLin()
        {
            var expr = MathS.pi + MathS.e + x;
            var func = expr.Compile(x);
            Assert.True(func.Call(3).Real > 7 && func.Call(3).Real < 10);
        }

        [Fact]
        public void TestSignum1()
        {
            var expr = MathS.Signum(x) + 1;
            var func = expr.Compile(x);
            Assert.Equal(2, func.Call(3));
        }

        [Fact]
        public void TestSignum2()
        {
            var expr = MathS.Signum(x) + 1;
            var func = expr.Compile(x);
            Assert.Equal(0, func.Call(-3));
        }
    }
}
