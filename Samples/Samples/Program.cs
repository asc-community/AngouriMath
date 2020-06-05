using System;
using AngouriMath;

namespace Samples
{
    class Program
    {
        static void Main(string[] _)
        {
            Entity expr = "arcsin(x / pi) + 0.6";
            var roots = expr.SolveEquation("x");
            Console.WriteLine(roots);
        }
    }
}
