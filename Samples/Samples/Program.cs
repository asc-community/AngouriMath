using System;
using System.Collections.Immutable;
using AngouriMath;
using static AngouriMath.Entity.Boolean;
using static AngouriMath.MathS;

namespace Samples
{
    class Program
    {
        static void Main(string[] _)
        {
            Entity expr = "A and B implies C xor D implies not B";
            Console.WriteLine(MathS.BuildTruthTable(expr, "A", "B", "C", "D"));
        }
    }
}