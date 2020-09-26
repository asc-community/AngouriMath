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
            var set = "x2 = 16 and x > 0 or x = a".Solve("x");
            Console.WriteLine(set);
            //if (set.IsFiniteSet(out var roots))
            //    foreach (var root in roots)
            //        //Console.WriteLine(root.InnerSimplifyWithCheck());
            //        Console.WriteLine(root.InnerSimplifyWithCheck());
        }
    }
}