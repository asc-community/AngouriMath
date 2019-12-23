using System;
using AngouriMath;

namespace Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            var expr = MathS.FromString("phi");
            var phi = MathS.Var("phi");
            Console.WriteLine(expr.Substitute(phi, 3).Eval());
        }
    }
}
