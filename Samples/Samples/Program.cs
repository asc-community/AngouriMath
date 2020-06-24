using System;
using AngouriMath;
using AngouriMath.Core.Numerix;
using AngouriMath.Extensions;
using PeterO.Numbers;

namespace Samples
{
    class Program
    {
        static void Main(string[] _)
        {
            //var x = MathS.Var("x");
            /*
            Console.WriteLine("2 + 2 * x^1".Simplify());
            Number a = 3;
            Number b = 5;
            var c = a == b;
            */
            Console.WriteLine(MathS.FromBaseN("-A.4", 16).ToString());
            //EDecimal a = 0.14m;
            //EDecimal b = 1.0m;
            //Console.WriteLine(b / a);
        }
    }
}
