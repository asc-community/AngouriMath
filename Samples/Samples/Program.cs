using System;
using System.Collections.Immutable;
using System.Linq;
using AngouriMath;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Boolean;
using static AngouriMath.MathS;
using AngouriMath.Extensions;
using AngouriMath.Functions.Algebra;

namespace Samples
{
    class Program
    {
        static void Main(string[] _)
        {
            var integ = "(x2 + 2x + 3) * sin(x)";
            var ans = Integration.ComputeIndefiniteIntegral(integ, "x");
            Console.WriteLine(ans.Simplify());
        }
    }
}