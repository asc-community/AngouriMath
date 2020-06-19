using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Core.Numerix;
using AngouriMath.Extensions;

namespace Samples
{
    class Program
    {
        static void Main(string[] _)
        {
            
            //var a = "(-1 / 64) ^ (1 / 3)".ToEntity();
            //Console.WriteLine(a.Simplify());
            //Console.WriteLine(Number.Pow(Number.Pow(-1.0 / 64, Number.CreateRational(1, 3)), 3));
            //var f64 = Number.CreateRational(-1, 64);
            //var f3 = Number.CreateRational(1, 3);
            //Console.WriteLine(Number.Pow(f64, f3));
            Console.WriteLine(Number.Pow(2, Number.CreateRational(1, 3)));
        }
    }
}
