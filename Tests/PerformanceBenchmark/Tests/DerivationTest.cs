using AngouriMath;
using System;
using System.Collections.Generic;

namespace PerformanceBenchmark.Tests
{
    public class DerivationTest : CommonTest
    {
        public DerivationTest() : base(10000, new List<Func<object>> {
            () => x.Derive(x),
            () => (MathS.Cos(x) * MathS.Sin(x)).Derive(x),
            () => (MathS.Sqr(MathS.Sin(x + 2 * y)) + MathS.Sqr(MathS.Cos(x + 2 * y))).Derive(x),
            () => (x * MathS.Cos(x) / MathS.Sin(MathS.Sqrt(x / MathS.Ln(x)))
                * x * MathS.Cos(x) / MathS.Sin(MathS.Sqrt(x / MathS.Ln(x)))
                    * x * MathS.Cos(x) / MathS.Sin(MathS.Sqrt(x / MathS.Ln(x)))
                        * x * MathS.Cos(x) / MathS.Sin(MathS.Sqrt(x / MathS.Ln(x)))).Derive(x)
        })
        { }
    }
}
