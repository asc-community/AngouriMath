using AngouriMath;
using System;
using System.Collections.Generic;
using System.Text;

namespace PerformanceBenchmark.Tests
{
    public class CompiledFunctionTest : CommonTest
    {
        private readonly List<FastExpression> exprs = new List<FastExpression> {
            x.Compile(x),
            (MathS.Cos(x) * MathS.Sin(x)).Compile(x),
            (MathS.Sqr(MathS.Sin(x + 2 * x)) + MathS.Sqr(MathS.Cos(x + 2 * x))).Compile(x),
            (x * MathS.Cos(x) / MathS.Sin(MathS.Sqrt(x / MathS.Ln(x)))
            * x * MathS.Cos(x) / MathS.Sin(MathS.Sqrt(x / MathS.Ln(x)))
                * x * MathS.Cos(x) / MathS.Sin(MathS.Sqrt(x / MathS.Ln(x)))
                    * x * MathS.Cos(x) / MathS.Sin(MathS.Sqrt(x / MathS.Ln(x)))).Compile(x)
        };
        public CompiledFunctionTest()
        {
            IterCount = 3000000;
            tests = new List<Func<object>>();
            foreach (var expr in exprs)
                tests.Add(() => expr.Call(3));
        }
    }
}
