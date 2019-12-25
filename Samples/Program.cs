using System;
using AngouriMath;
using AngouriMath.Core;
using System.Diagnostics;

namespace Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            var x = MathS.Var("x");
            var y = MathS.Var("y");

            var expr = MathS.Sin(x) * MathS.Sqrt(x + MathS.Cos(x)) / 3;
            //Console.WriteLine(func.Eval(1, 2));
            //return;

            var iter = 1000;
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            for (int i = 0; i < iter; i++)
                expr.DefiniteIntegral(x, 1, 10);
            stopWatch.Stop();
            Console.Write(((double)stopWatch.ElapsedMilliseconds) / iter * 1000000);
            Console.WriteLine(" ns");
        }
    }
}
