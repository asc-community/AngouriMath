using System;
using System.Collections.Generic;
using System.Text;

namespace MathSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            var x = MathS.Var("x");
            var y = MathS.Var("y");
            var expr = x.Pow(y) + MathS.Sqrt(x + y / 4) * (6 / x);
            Console.WriteLine(expr.Latexise());
        }
    }
}
