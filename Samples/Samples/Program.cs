
using System;
using AngouriMath;
using AngouriMath.Core.Numerix;

namespace Samples
{
    class Program
    {
        static void Main(string[] _)
        {
            VariableEntity x = "x";
            Entity expr = MathS.Log(2, x) - 5;
            var roots = expr.SolveEquation(x);
            Console.WriteLine(roots);
        }
    }
}
