using System;
using System.Diagnostics;
using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Core.Sys.Interfaces;
using AngouriMath.Extensions;

namespace Samples
{
    class Program
    {
        static void Main(string[] _)
        {
            var f = ("a - 1", "b * 0").SolveSystem("a", "b");
        }
    }
}
