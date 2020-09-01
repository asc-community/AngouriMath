
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
            Entity a = "(x4 - 4x3 - 8x2 + 48x + 16) / (x - a)";
            Console.Write(a.Simplify());
        }
    }
}