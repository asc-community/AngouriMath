using System;
using AngouriMath;

namespace Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            //var Expr = MathS.FromString("(x^2 - 4)/(x + 2)");
            //Console.WriteLine(Expr.Collapse());
            var expr = MathS.FromString("x * y * x * x");
            Console.WriteLine(expr.Simplify(4).ToString());
        }
    }
}
