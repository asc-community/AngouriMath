using AngouriMath;
using System;
using System.Collections.Generic;

namespace PerformanceBenchmark.Tests
{
    public class SimplificationTest : CommonTest
    {
        public SimplificationTest() : base(1500, new List<Func<object>> {
            () => (x * MathS.Sin(x)).Simplify(),
            () => (MathS.Cos(x) * MathS.Sin(x)).Simplify(),
            () => (MathS.Sqr(MathS.Sin(x + 2 * y)) + MathS.Sqr(MathS.Cos(x + 2 * y))).Simplify(),
            () => (x * MathS.Cos(x) / MathS.Sin(MathS.Sqrt(x / MathS.Ln(x)))
                * x * MathS.Cos(x) / MathS.Sin(MathS.Sqrt(x / MathS.Ln(x)))
                    * x * MathS.Cos(x) / MathS.Sin(MathS.Sqrt(x / MathS.Ln(x)))
                        * x * MathS.Cos(x) / MathS.Sin(MathS.Sqrt(x / MathS.Ln(x)))).Simplify()
        })
        { }
    }
}
