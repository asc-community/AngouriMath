using System;
using AngouriMath;

namespace Samples
{
    class Program
    {
        static void Main(string[] _)
        {
            Entity expr = "x + sin(3)";
            Console.WriteLine(expr.SolveEquation("x"));
        }
    }
}
