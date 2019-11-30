using MathSharp.Core;
using MathSharp.Core.FromString;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace MathSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            var str = "x^3 + x^2 - 4x - 4";
            for (int i = 0; i < 5; i++)
            {
                long milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                var expr = MathS.FromString(str);
                var sol = expr.SolveNt(MathS.Var("x"), precision: 400);
                Console.WriteLine(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond - milliseconds);
                Console.WriteLine(sol.Count);
            }
        }
    }
}
