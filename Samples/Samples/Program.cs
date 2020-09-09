using System;
using AngouriMath;

namespace Samples
{
    class Program
    {
        static void Main(string[] _)
        {
            Entity ex = "sgn(x) + 5";
            Console.WriteLine(ex.SolveEquation("x"));
        }
    }
}