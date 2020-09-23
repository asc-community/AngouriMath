using System;
using System.Collections.Immutable;
using System.Linq;
using AngouriMath;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Boolean;
using static AngouriMath.MathS;
using AngouriMath.Extensions;

namespace Samples
{
    class Program
    {
        static void Main(string[] _)
        {
            //Console.WriteLine("(x - 1)(x + 3) = a and (x - 1)(x + 2) = 0 or x2 = 16".SolveEquation("x"));
            Console.WriteLine("abs(x) - 5".SolveEquation("x"));
        }
    }
}