using System;
using AngouriMath;

namespace Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            var x = MathS.Var("x");
            var y = MathS.Var("y");
            var expr = MathS.Sqr(x) - MathS.Sqr(y);
            Console.WriteLine(expr.Collapse(1));
        }
    }
}
