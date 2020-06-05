using System;
using AngouriMath;
using AngouriMath.Core.Numerix;

namespace Samples
{
    class Program
    {
        static void Main(string[] _)
        {
            /*
            Entity expr = "arcsin(x / pi) + 0.6";
            var roots = expr.SolveEquation("x");
            Console.WriteLine(roots);
            */
            var a = MathS.Sqrt(1 - MathS.Pow(Number.CreateRational(1, 6) * (-1 + MathS.Pow((7 + 21 * MathS.Sqrt(-3)) / 2, Number.CreateRational(1, 3)) + MathS.Pow((7 - 21 * MathS.Sqrt(-3)) / 2, Number.CreateRational(1, 3))), 2));
            Console.WriteLine(a);
            Entity e = "sin(2x + 2) + sin(x + 1) - a";
            Console.WriteLine(e.SolveEquation("x"));
        }
    }
}
