using System;
using AngouriMath;

namespace Samples
{
    class Program
    {
        static void Main(string[] _)
        {
            Entity t = "2^x + 4^x - 2^(3x) - 4";
            Console.WriteLine(t.SolveEquation("x"));
        }
    }
}