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
        protected static readonly VariableEntity x = MathS.Var("x");
        protected static readonly VariableEntity y = MathS.Var("y");
        protected static readonly VariableEntity z = MathS.Var("z");
        protected static int IterCount { get; set; } = 1000;
        private double Watch(Func<object> func)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            for (int i = 0; i < IterCount; i++)
                func();
            stopWatch.Stop();
            return ((double)stopWatch.ElapsedMilliseconds) / IterCount;
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
