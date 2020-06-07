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
            Entity n1 = Number.Create(RealNumber.UndefinedState.POSITIVE_INFINITY);
            Entity n2 = Number.Create(1);
            var expr = n1 + n2;
            
            Console.WriteLine(expr.Simplify());
        }
    }
}
