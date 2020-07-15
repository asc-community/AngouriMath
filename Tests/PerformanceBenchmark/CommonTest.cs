using System;
using System.Collections.Generic;
using System.Text;
using AngouriMath;
using System.Diagnostics;

namespace PerformanceBenchmark.Tests
{
    public abstract class CommonTest
    {
        protected List<Func<object>> tests;
        readonly int iterCount;
        protected static readonly VariableEntity x = MathS.Var("x");
        protected static readonly VariableEntity y = MathS.Var("y");
        protected static readonly VariableEntity z = MathS.Var("z");

        protected CommonTest(int iterCount, List<Func<object>> tests) =>
            (this.tests, this.iterCount) = (tests, iterCount);

        private decimal Watch(Func<object> func)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            for (int i = 0; i < iterCount; i++)
                func();
            stopWatch.Stop();
            return ((decimal)stopWatch.ElapsedMilliseconds) / iterCount;
        }
        internal string Evaluate()
        {
            var sb = new StringBuilder();
            foreach (var test in tests)
            {
                var time = Watch(test);
                sb.Append(": " + (time * 1000).ToString() + " microseconds");
                sb.Append("\n");
            }
            return sb.ToString();
        }
    }
}
