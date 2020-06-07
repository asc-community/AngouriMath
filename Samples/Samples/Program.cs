using System;
using System.Diagnostics;
using AngouriMath;
using AngouriMath.Core.Numerix;

namespace Samples
{
    class Program
    {
        static void Main(string[] _)
        {
            Entity expr = "(a + 3)^(-1/3)";
            expr = expr.Simplify();
            Console.WriteLine(expr.Latexise());
        }
    }
}
