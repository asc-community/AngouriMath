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
            var expr = (x + y) * (x + 2 * y) * ((x - y) * (x + y));
            Console.WriteLine(expr);
            Console.WriteLine(expr.Expand().Simplify());
        }
    }
}
