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
            Entity exprComplex = "sqrt(-1)";
            var exprReal = exprComplex.DomainFromComplexToReal();
            Console.WriteLine(exprComplex.EvalNumerical());
            Console.WriteLine(exprReal.EvalNumerical());
        }
    }
}