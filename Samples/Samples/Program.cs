using System;
using System.Diagnostics;
using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Core.Sys.Interfaces;
using AngouriMath.Extensions;

using AngouriMath.Experimental.Limits;
using AngouriMath.Core.Numerix;
using Antlr4.Runtime.Misc;

namespace Samples
{
    class Program
    {
        static void Main(string[] _)
        {
            var limit = Limit.ComputeLimit("log((2x - 1)/(x + 1), (x - 1)/(2x - 1))", "x", RealNumber.PositiveInfinity);
            Console.WriteLine(limit);
        }
    }
}
