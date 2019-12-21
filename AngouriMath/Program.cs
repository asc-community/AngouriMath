using AngouriMath.Core;
using AngouriMath.Core.FromString;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace AngouriMath
{
    class Program
    {
        static void Main(string[] args)
        {
            var x = MathS.Var("x");
            var y = MathS.Var("y");
            var expr = (MathS.Sqr(MathS.Sin(x + 2 * y)) + MathS.Sqr(MathS.Cos(x + 2 * y))) / (MathS.Sin(x - y) * MathS.Cos(x - y) + 1);
            Console.WriteLine(expr);
            Console.WriteLine();
            Console.WriteLine(expr.Simplify());
            
        }
    }
}
