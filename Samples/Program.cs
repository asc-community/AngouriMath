using System;
using AngouriMath;

namespace Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            var x = MathS.Var("x");
            var expr = x;
            Console.WriteLine(Math.Abs(expr.DefiniteIntegral(expr, x, 0, 1).Re));
        }
    }
}
