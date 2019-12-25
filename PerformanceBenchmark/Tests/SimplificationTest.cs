using AngouriMath;
using System;
using System.Collections.Generic;
using System.Text;

namespace PerformanceBenchmark.Tests
{
    public class SimplificationTest : CommonTest
    {
        public SimplificationTest()
        {
            IterCount = 300;
            tests = new List<Func<object>> { 
                () => (x * MathS.Sin(x)).Simplify(),
                () => (MathS.Cos(x) * MathS.Sin(x)).Simplify(),
                () => (MathS.Sqr(MathS.Sin(x + 2 * y)) + MathS.Sqr(MathS.Cos(x + 2 * y))).Simplify(),
                () => (x * MathS.Cos(x) / MathS.Sin(MathS.Sqrt(x / MathS.Ln(x)))
            * x * MathS.Cos(x) / MathS.Sin(MathS.Sqrt(x / MathS.Ln(x)))
                * x * MathS.Cos(x) / MathS.Sin(MathS.Sqrt(x / MathS.Ln(x)))
                    * x * MathS.Cos(x) / MathS.Sin(MathS.Sqrt(x / MathS.Ln(x)))).Simplify()
            };
        }
    }
}
