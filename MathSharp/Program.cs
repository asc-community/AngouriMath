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
            var expr = MathS.Tan(x);
            var der = expr.Derive(x);
            Console.WriteLine(der);
            Console.WriteLine(der.Simplify());
        }
    }
}
