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
            var exp = (x + 3).Pow(y);
            Console.WriteLine(exp.Latexise());
        }
    }
}
