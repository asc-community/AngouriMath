using System;
using AngouriMath;
using static AngouriMath.Entity.Boolean;

namespace Samples
{
    class Program
    {
        class A { }
        class B : A { }

        static void Main(string[] _)
        {
            Entity exprComplex = "not x and y -> x";
            Console.WriteLine(exprComplex);
            //var exprReal = exprComplex.DomainFromComplexToReal();
            //Console.WriteLine(exprComplex.SolveEquation("x"));
            //Console.WriteLine(exprReal.SolveEquation("x"));
        }
    }
}