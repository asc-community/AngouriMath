using System;
using System.Collections.Immutable;
using System.Linq;
using AngouriMath;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Boolean;
using static AngouriMath.MathS;

namespace Samples
{
    class Program
    {
        static void Main(string[] _)
        {
            Entity expr = "a -> true";
            Console.WriteLine(MathS.SolveBooleanTable(expr, "a"));
        }
    }
}