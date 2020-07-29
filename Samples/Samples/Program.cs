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
            Entity expr = "(x^2 + x) / (a*x + 1)";
            var limit = Limit.ComputeLimit(expr, "x", RealNumber.PositiveInfinity, ApproachFrom.Left);
            Console.WriteLine(limit);
        }
    }
}
