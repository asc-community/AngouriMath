using System;
using System.Collections.Immutable;
using System.Linq;
using AngouriMath;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Boolean;
using static AngouriMath.MathS;
using AngouriMath.Extensions;
using System.Threading;

namespace Samples
{
    class Program
    {
        static void Main(string[] _)
        {
            Entity expr1 = "1.0000000000000000000001";
            Entity expr2 = "1";
            Console.WriteLine(expr1 + expr2);
        }
    }
}