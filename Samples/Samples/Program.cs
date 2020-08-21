
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
            Entity a = "3 * sin(e^x) + 5 * sin(e^x) + 3 * sin(e^x) ^ 5 + a * sin(e^x) + 3f * sin(e^x) - h * sin(e^x) ^ 5";

            a = AngouriMath.Functions.TreeAnalyzer.Factorize(a);
            Console.WriteLine(a);
        }
    }
}