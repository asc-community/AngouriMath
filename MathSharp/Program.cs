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
            var exp = MathS.Ln(5) * x - MathS.Ln(5) * x;
            Console.WriteLine(exp.Simplify());
        }
    }
}
