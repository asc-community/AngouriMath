using System;
using AngouriMath;

namespace Samples
{
    class Program
    {
        static void Main(string[] _)
        {
            Entity t = "e^(a*x) / 0.5 / a + e^(2*a*x) + 1";
            Console.WriteLine(t.SolveEquation("x"));
        }
    }
}