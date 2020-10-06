using System;
using AngouriMath.Functions.Algebra;

namespace Samples
{
    class Program
    {
        static void Main(string[] _)
        {
            var t = "ln((a*x+b)^c)";
            var res = Integration.ComputeIndefiniteIntegral(t, "x");
            Console.WriteLine(res.Simplify());
        }
    }
}