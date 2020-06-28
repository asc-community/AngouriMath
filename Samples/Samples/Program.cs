using System;
using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Core.Sys.Interfaces;
using AngouriMath.Extensions;

namespace Samples
{
    class Program
    {
        static string ToWeb(ILatexiseable l)
            => "<img src=\"https://render.githubusercontent.com/render/math?math=" +
               l.Latexise().Replace("+", "%2B")
               + "\">";
        static void Main(string[] _)
        {
            var system = MathS.Equations(
                "x2 + y + a",
                "y - 0.1x + b"
            );
            //Console.WriteLine(system);
            var solutions = system.Solve("x", "y");
            Console.WriteLine(solutions.Latexise());
        }
    }
}
