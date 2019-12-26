using System;
using AngouriMath;
using AngouriMath.Core;
using System.Diagnostics;

namespace Samples
{
    class Program
    {
        static void Sample1()
        {
            var inp = "1 + 2 * log(9, 3)";
            var expr = MathS.FromString(inp);
            Console.WriteLine(expr.Eval());
        }
        static void Sample2()
        {
            var x = MathS.Var("x");
            var y = MathS.Var("y");
            var c = x * y + x / y;
            Console.WriteLine(MathS.Sqr(c));
        }
        static void Sample3()
        {
            var x = MathS.Var("x");
            var expr = x * 2 + MathS.Sin(x) / MathS.Sin(MathS.Pow(2, x));
            var subs = expr.Substitute(x, 0.3);
            Console.WriteLine(subs.Simplify());
        }
        static void Sample4()
        {
            var x = MathS.Var("x");
            var func = MathS.Sqr(x) + MathS.Ln(MathS.Cos(x) + 3) + 4 * x;
            var derivative = func.Derive(x);
            Console.WriteLine(derivative.Simplify());
        }
        static void Sample5()
        {
            var x = MathS.Var("x");
            var expr = (x + 3).Pow(x + 4);
            Func<NumberEntity, Entity> wow = v => expr.Substitute(x, v).Simplify();
            Console.WriteLine(wow(4));
            Console.WriteLine(wow(5));
            Console.WriteLine(wow(6));
        }
        static void Sample6()
        {
            var x = MathS.Var("x");
            var y = MathS.Var("y");
            var expr = x.Pow(y) + MathS.Sqrt(x + y / 4) * (6 / x);
            Console.WriteLine(expr.Latexise());
        }
        static void Sample7()
        {
            var expr = MathS.Pow(MathS.e, MathS.pi * MathS.i);
            Console.WriteLine(expr);
            Console.WriteLine(expr.Eval());
        }
        static void Sample8()
        {
            var x = MathS.Var("x");
            var equation = (x - 1) * (x - 2) * (MathS.Sqr(x) + 1);
            Console.Write(equation.SolveNt(x));
        }
        static void Sample9()
        {
            var x = MathS.Var("x");
            var expr = MathS.Sin(x) + MathS.Sqrt(x) / (MathS.Sqrt(x) + MathS.Cos(x)) + MathS.Pow(x, 3);
            Console.WriteLine(expr.DefiniteIntegral(x, -3, 3));
            var expr2 = MathS.Sin(x);
            Console.WriteLine(expr2.DefiniteIntegral(x, 0, MathS.pi));
        }
        static void Sample10()
        {
            var x = MathS.Var("x");
            var expr = MathS.Sin(x) + MathS.Sqrt(x) / (MathS.Sqrt(x) + MathS.Cos(x)) + MathS.Pow(x, 3);
            var func = expr.Compile(x);
            Console.WriteLine(func.Substitute(3));
        }
        static void Main(string[] args)
        {
            //Sample1();
            var x = MathS.Var("x");

            var func = (x + MathS.Sqrt(x)).Compile(x);
            Console.WriteLine(func.Substitute(4));
            return;
            var expr = x * x + (x - 3 * (2 * x));
            //var func = expr.Compile(x);
            var watch = new Stopwatch();
            watch.Start();
            for (int i = 0; i < 10000000; i++)
                func.Call(3);
            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
        }
    }
}
