using AngouriMath;
using System;
using System.Collections.Generic;
using System.Text;

namespace PerformanceBenchmark.Tests
{
    public class SubsTest : CommonTest
    {
        public SubsTest()
        {
            IterCount = 1000;
            tests = new List<Func<object>> {
                () => (x * MathS.Sin(x)).Substitute(x, 3).Eval(),
                () => (MathS.Cos(x) * MathS.Sin(x)).Substitute(x, 3).Eval(),
                () => (MathS.Sqr(MathS.Sin(x + 2 * x)) + MathS.Sqr(MathS.Cos(x + 2 * x))).Substitute(x, 3).Eval(),
                () => (x * MathS.Cos(x) / MathS.Sin(MathS.Sqrt(x / MathS.Ln(x)))
            * x * MathS.Cos(x) / MathS.Sin(MathS.Sqrt(x / MathS.Ln(x)))
                * x * MathS.Cos(x) / MathS.Sin(MathS.Sqrt(x / MathS.Ln(x)))
                    * x * MathS.Cos(x) / MathS.Sin(MathS.Sqrt(x / MathS.Ln(x)))).Substitute(x, 3).Eval()
            };
        }
    }
}
