using System;
using System.Collections.Immutable;
using System.Linq;
using AngouriMath;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Boolean;
using static AngouriMath.MathS;
using AngouriMath.Extensions;
using System.Threading;

namespace Samples
{
    class Program
    {
        static void Main(string[] _)
        {
            Entity expr = "x > 3 and x2 = 16";
            Console.WriteLine(expr.Solve("x"));
        }
    }
}