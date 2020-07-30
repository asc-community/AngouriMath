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
            // x! + e^x + x^n + sin(x)
            // (x^3 + a*x^2) / (b*x^2 + 1)
            var limit = Limit.ComputeLimit("(x + 2) / (x - 1)", "x", "1", ApproachFrom.Left);
            //var limit = Limit.ComputeLimit("(x - 3) / (x - 1)", "x", 1, ApproachFrom.Right);
            //var limit = Limit.ComputeLimit("1 / (x - a)", "x", "a", ApproachFrom.Right);
            Console.WriteLine(limit?.Simplify());
        }
    }
}
