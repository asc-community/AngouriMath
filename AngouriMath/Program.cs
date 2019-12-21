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
            var expr = (x + 1) * (x + 2) * (x + 3) / ((x + 2) * (x + 3));
            Console.WriteLine(expr.Simplify());
        }
    }
}
