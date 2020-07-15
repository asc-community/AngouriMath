using AngouriMath;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;

namespace PerformanceBenchmark.Tests
{
    public class CompiledFunctionTest : CommonTest
    {
        public CompiledFunctionTest() : base(3000000, new[] {
            x.Compile(x),
            (MathS.Cos(x) * MathS.Sin(x)).Compile(x),
            (MathS.Sqr(MathS.Sin(x + 2 * x)) + MathS.Sqr(MathS.Cos(x + 2 * x))).Compile(x),
            (x * MathS.Cos(x) / MathS.Sin(MathS.Sqrt(x / MathS.Ln(x)))
                * x * MathS.Cos(x) / MathS.Sin(MathS.Sqrt(x / MathS.Ln(x)))
                    * x * MathS.Cos(x) / MathS.Sin(MathS.Sqrt(x / MathS.Ln(x)))
                        * x * MathS.Cos(x) / MathS.Sin(MathS.Sqrt(x / MathS.Ln(x)))).Compile(x)
        }.Select(expr => new Func<object>(() => expr.Call(3))).ToList())
        { }
    }
}
