using MathSharp.Core;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace MathSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            var x = MathS.Var("x");
            var equation = (x - 1) * (x - 2) * (MathS.Sqr(x) + 1);
            foreach (var re in equation.SolveNt(x))
                Console.Write(re.ToString() + "  ");
        }
    }
}
