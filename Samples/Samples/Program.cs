using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using AngouriMath;
using AngouriMath.Core.Numerix;

namespace Samples
{
    class Program
    {
        static void Main(string[] _)
        {
            //Console.WriteLine(MathS.Quack("(x - b) / (x + a)2 + e^x / (x + a) + (x - i) / (y*x) + b", "x").Simplify());
            Console.WriteLine(MathS.SolveEquation("(x - b) / (x + a) + c", "x"));
        }
    }
}
