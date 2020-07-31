using System;
using System.Diagnostics;
using System.Threading.Tasks;
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
            //Entity expr = "1 / (sin(a) + e ^ (sin(x) / x))";
            Entity expr = "1 / (x - a)";
            VariableEntity x = "x";
            Entity dest = "a";
            Console.WriteLine(expr);
            Console.WriteLine(Limit.ComputeLimit(expr, x, dest, ApproachFrom.Left));
        }
    }
}
