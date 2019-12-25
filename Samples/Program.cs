using System;
using AngouriMath;
using AngouriMath.Core;
using System.Diagnostics;

namespace Samples
{
    class Program
    {
        static Number Deriv0(Entity expr, VariableEntity x, Number a)
        {
            var t1 = dexprdx.Substitute(x, a);
            var t2 = t1.Eval();
            var t3 = t2.GetValue();
            return t3;
        }
        static Number Deriv1(Entity expr, VariableEntity x, Number a)
        {
            return expr.Derive(x)
                .Substitute(x, a)
                .Eval()
                .GetValue();
        }
        static Number Deriv2(Entity expr, VariableEntity x, Number a)
        {
            var dx = new Number(1.0e-6, 1.0e-6);
            return (expr.Substitute(x, a + dx).Eval().GetValue() - expr.Substitute(x, a).Eval().GetValue()) / dx;
        }
        static VariableEntity x = MathS.Var("x");
        static Entity expr = x * MathS.Cos(x) / MathS.Sin(MathS.Sqrt(x / MathS.Ln(x)))
            * x * MathS.Cos(x) / MathS.Sin(MathS.Sqrt(x / MathS.Ln(x)))
                * x * MathS.Cos(x) / MathS.Sin(MathS.Sqrt(x / MathS.Ln(x)))
                    * x * MathS.Cos(x) / MathS.Sin(MathS.Sqrt(x / MathS.Ln(x)));
        static Entity dexprdx = expr.Derive(x).Simplify();
        static void Main(string[] args)
        {
            var stopWatch = new Stopwatch();
            var iter = 1000;

            stopWatch.Start();
            for (int i = 0; i < iter; i++)
                Deriv0(expr, x, 3);
            stopWatch.Stop();
            Console.Write("Anal static ");
            Console.WriteLine(stopWatch.ElapsedMilliseconds);
            stopWatch.Reset();

            stopWatch.Start();
            for (int i = 0; i < iter; i++)
                Deriv1(expr, x, 3);
            stopWatch.Stop();
            Console.Write("Anal dynamic ");
            Console.WriteLine(stopWatch.ElapsedMilliseconds);
            stopWatch.Reset();

            stopWatch.Start();
            for (int i = 0; i < iter; i++)
                Deriv2(expr, x, 3);
            stopWatch.Stop();
            Console.Write("Numerical ");
            Console.WriteLine(stopWatch.ElapsedMilliseconds);
        }
    }
}
