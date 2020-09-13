using System;
using AngouriMath;

namespace Samples
{
    class Program
    {
        class A { }
        class B : A { }

        static void Main(string[] _)
        {
            Entity exprComplex = "x4 - 1";
            var exprReal = exprComplex.DomainFromComplexToReal();
            Console.WriteLine(exprComplex.SolveEquation("x"));
            Console.WriteLine(exprReal.SolveEquation("x"));
        }
    }
}