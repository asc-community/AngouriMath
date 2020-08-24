
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
            Entity a = "x + 3 + sin(x) + sin(x)";
            var com = a.Compile("x");
        }
    }
}