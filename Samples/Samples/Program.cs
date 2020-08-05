
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
            Entity expr = "1+2x*-1+2x*2+x^2+2x+2x*-4+2x*4+2x*2x*-1+2x*2x*2+2x*x^2+x^2+x^2*-4+x^2*4+x^2*2*x*-1+x^2*2x*2+x^2*x^2";
            Console.WriteLine(expr.Simplify());
            //VariableEntity x = "x";
            //var expr = MathS.Factorial(1 + x) / MathS.Factorial(-2 + x);
        }
    }
}
