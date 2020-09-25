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
            //Console.WriteLine("x4 = 1 and x8 = 1".Simplify());
            //Entity expr = "x4 = 1 and x8 = 1";
            Console.WriteLine("0 < x".Simplify());
        }
    }
}