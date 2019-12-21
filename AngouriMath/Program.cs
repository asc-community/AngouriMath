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
            var expr = 2 / (x / 3) + (MathS.Sqr(MathS.Cos(x + 3)) + MathS.Sqr(MathS.Sin(x + 3)));
            Console.WriteLine(expr);
            Console.WriteLine(expr.Simplify());
        }
    }
}
