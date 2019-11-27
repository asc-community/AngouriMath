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
            var exp = x ^ 0;
            Console.WriteLine(exp.Simplify());
        }
    }
}
