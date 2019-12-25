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
            var expr = x + 3;
            expr.Substitute(x, 4);
            Console.WriteLine(expr);
            Console.WriteLine((MathS.Sqr(x) + 1).SolveNt(x));
        }
    }
}
