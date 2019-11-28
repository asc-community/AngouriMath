using System;
using System.Collections.Generic;
using System.Text;

namespace MathSharp
{
    class Program
    {
        static double Now()
        {
            return ((double)DateTime.Now.Ticks) / TimeSpan.TicksPerMillisecond;
        }
        static void Main(string[] args)
        {
            var x = MathS.Var("x");
            var y = MathS.Var("y");
            var expr = MathS.Tan(x) * x + (y + x).Pow(y + x) - MathS.Sqrt(MathS.Sqr(x) - y) / MathS.Log(0.5 * x, y);
            double start = Now();
            int iters = 1000;
            for(int i = 0; i < iters; i++)
            {
                expr.Derive(x).Substitute(x, 3 + 4).Substitute(y, 3).Simplify();
            }
            double timeSpent = Now() - start;
            Console.WriteLine((((double)timeSpent) / iters).ToString() + "ms per iter");
        }
    }
}
