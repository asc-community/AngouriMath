
using System;
using System.Diagnostics;
using AngouriMath;
using AngouriMath.Extensions;

namespace Samples
{
    class Program
    {
        static void Main(string[] _)
        {
            Entity expr = "(x - b) / (x + a) + c / (x + a)";
            var roots = expr.SolveEquation("x");
        }
    }
}
