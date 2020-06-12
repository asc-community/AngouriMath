using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using AngouriMath;
using AngouriMath.Core.Numerix;

namespace Samples
{
    class Program
    {
        static void Main(string[] _)
        {
            var x = MathS.Var("x");
            var eq = x.Pow(2) + 2 * x + 1;
            MathS.Settings.PrecisionErrorZeroRange.Set(1e-18m);
            MathS.Settings.PrecisionErrorCommon.Set(1e-8m);
            var roots = eq.SolveNt(x, precision: 100);
            MathS.Settings.PrecisionErrorCommon.Unset();
            MathS.Settings.PrecisionErrorZeroRange.Unset();
            Console.WriteLine(roots);
        }
    }
}
