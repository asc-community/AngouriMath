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
            
            var expr = (x * MathS.Cos(x) / MathS.Sin(MathS.Sqrt(x / MathS.Ln(x)))
            * x * MathS.Cos(x) / MathS.Sin(MathS.Sqrt(x / MathS.Ln(x)))
                * x * MathS.Cos(x) / MathS.Sin(MathS.Sqrt(x / MathS.Ln(x)))
                    * x * MathS.Cos(x) / MathS.Sin(MathS.Sqrt(x / MathS.Ln(x))));//*/
            //var expr = MathS.Sqr(x - 4) + MathS.Log(3, MathS.Sin(x));
            var func = expr.Compile("x");

            var iter = 10000000;
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var a = 0.0;
            var cont = 3;
            for (int i = 0; i < iter; i++)
                func.Eval(3);
                //expr.Substitute(x, 3).Eval();
                //a = Math.Sqrt(cont - 4) + Math.Log(cont, Math.Sin(cont));
            stopWatch.Stop();
            Console.Write(((double)stopWatch.ElapsedMilliseconds) / iter * 1000000);
            Console.WriteLine(" ns");
        }
    }
}
