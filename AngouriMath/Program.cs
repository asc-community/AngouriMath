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
            var expr1 = MathS.Sqr(MathS.Cos(x + 2)) + MathS.Sqr(MathS.Cos(x + 3));
            var expr2 = MathS.Pow(MathS.e, MathS.Log(x + 3, MathS.e));
            Console.WriteLine(expr1.Simplify());
            Console.WriteLine(expr2);
            Console.WriteLine(expr2.Simplify());
        }
    }
}
