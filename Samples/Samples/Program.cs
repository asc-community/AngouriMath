using System;
using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Extensions;

namespace Samples
{
    class Program
    {
        static void Main(string[] _)
        {

            var system = MathS.Equations(
                "x2 + y + a",
                "y - 0.1x + b"
            );
            Console.WriteLine(system.Latexise());
            var solutions = system.Solve("x", "y");
            Console.WriteLine(solutions.Latexise());
        }
    }
}
