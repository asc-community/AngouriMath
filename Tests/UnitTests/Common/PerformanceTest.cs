using AngouriMath;
using Xunit;
using System;

namespace UnitTests.Common
{
    public class PerformanceTest
    {
        private readonly int ITERATIONS = 10;
        private readonly Entity.Variable x = MathS.Var(nameof(x));
        public long Measure(Func<object> func)
        {
            long milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            for (int i = 0; i < ITERATIONS; i++)
            {
                func();
            }
            return (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - milliseconds) / ITERATIONS;
        }
        [Fact]
        public void Test1() => Assert.True(Measure(() => MathS.FromString("x + x^2 + x^3")) < 4);
        [Fact]
        public void Test2() => Assert.True(Measure(() => MathS.FromString("x + log(2, 4)^2 * sin(cos(sin(cos(5)))) + x^3")) < 10);
        [Fact]
        public void Test4() => Assert.True(Measure(() => x / x) < 1);
        [Fact]
        public void Test5() => Assert.True(Measure(() => (x * MathS.Pow(MathS.e, x) * MathS.Ln(x) - MathS.Sqrt(x / (x * x - 1))).Derive(x)) < 30);
        [Fact]
        public void Test6() => Assert.True(Measure(() => (x * MathS.Pow(MathS.e, x) * MathS.Ln(x) - MathS.Sqrt(x / (x * x - 1))).Derive(x).Substitute(x, 3).Eval()) < 50);
    }
}
