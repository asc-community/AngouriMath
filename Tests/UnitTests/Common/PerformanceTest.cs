using AngouriMath;
using AngouriMath.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTests
{
    [TestClass]
    public class PerformanceTest
    {
        private readonly int ITERATIONS = 10;
        private readonly VariableEntity x = MathS.Var("x");
        public long Measure(Func<object> func)
        {
            long milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            for (int i = 0; i < ITERATIONS; i++)
            {
                func();
            }
            return (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - milliseconds) / ITERATIONS;
        }
        [TestMethod]
        public void Test1() => Assert.IsTrue(Measure(() => MathS.FromString("x + x^2 + x^3")) < 4);
        [TestMethod]
        public void Test2() => Assert.IsTrue(Measure(() => MathS.FromString("x + log(2, 4)^2 * sin(cos(sin(cos(5)))) + x^3")) < 10);
        [TestMethod]
        public void Test4() => Assert.IsTrue(Measure(() => x / x) < 1);
        [TestMethod]
        public void Test5() => Assert.IsTrue(Measure(() => (x * MathS.Pow(MathS.e, x) * MathS.Ln(x) - MathS.Sqrt(x / (x * x - 1))).Derive(x)) < 30);
        [TestMethod]
        public void Test6() => Assert.IsTrue(Measure(() => (x * MathS.Pow(MathS.e, x) * MathS.Ln(x) - MathS.Sqrt(x / (x * x - 1))).Derive(x).Substitute(x, 3).Eval()) < 50);
    }
}
