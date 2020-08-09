
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
            
            Entity eqs = "log(x, 32) - 5";
            var sols = eqs.SolveEquation("x");
            Console.WriteLine(sols);
            
        }
    }
}
