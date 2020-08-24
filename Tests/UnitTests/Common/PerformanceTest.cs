using AngouriMath;
using Xunit;
using System;

namespace UnitTests.Common
{
    /// <summary>
    /// It should be removed once we add benchmark workflow
    /// https://github.com/asc-community/AngouriMath/issues/194
    /// </summary>
    public class PerformanceTest
    {
        public PerformanceTest() => MathS.FromString("x"); // Get rid of overhead
        private const int ITERATIONS = 100;
        private readonly Entity.Variable x = MathS.Var(nameof(x));
        void Measure(Func<object> func, TimeSpan maxTime)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < ITERATIONS; i++)
                func();
            stopwatch.Stop();
            Assert.InRange(stopwatch.Elapsed / ITERATIONS, TimeSpan.Zero, maxTime);
        }
        [Fact] public void Test1() => Measure(() => MathS.FromString("x + x^2 + x^3"), TimeSpan.FromMilliseconds(2.6));
        [Fact] public void Test2() => Measure(() => MathS.FromString("x + log(2, 4)^2 * sin(cos(sin(cos(5)))) + x^3"), TimeSpan.FromMilliseconds(7.4));
        [Fact] public void Test3() =>
            MathS.Settings.NewtonSolver.As(new() { Precision = 400 }, () => Measure(() => (x.Pow(3) + x.Pow(2) - 4 * x - 4).SolveNt(x), TimeSpan.FromMilliseconds(310)));
        [Fact] public void Test4() => Measure(() => (x / x).Simplify(), TimeSpan.FromMilliseconds(0.05));
        [Fact] public void Test5() => Measure(() => (x * MathS.Pow(MathS.e, x) * MathS.Ln(x) - MathS.Sqrt(x / (x * x - 1))).Derive(x), TimeSpan.FromMilliseconds(0.5));
        [Fact] public void Test6() => Measure(() => (x * MathS.Pow(MathS.e, x) * MathS.Ln(x) - MathS.Sqrt(x / (x * x - 1))).Derive(x).Substitute(x, 3).Eval(), TimeSpan.FromMilliseconds(55));
    }
}
