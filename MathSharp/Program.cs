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
            var expr = MathS.Log(MathS.Ln(x), x);
            var res = expr.Derive(x);
            Console.WriteLine(res.Simplify());
        }
    }
}
