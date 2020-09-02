
using System;
using System.Diagnostics;
using AngouriMath;
using AngouriMath.Extensions;
using PeterO.Numbers;

namespace Samples
{
    class Program
    {
        static void Main(string[] _)
        {
            //var d = MathS.Numbers.Create(1e-20m);
            //Console.Write(d);
            EDecimal a = 1e-20m;
            var n = MathS.Numbers.Create(1e-20m);
            Console.Write(a + " " + n.ToString());
        }
    }
}