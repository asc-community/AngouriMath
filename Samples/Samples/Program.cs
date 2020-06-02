using AngouriMath;
using AngouriMath.Convenience;
using AngouriMath.Core;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using AngouriMath.Core.Sets;
using AngouriMath.Core.Numerix;
using System.Numerics;

namespace Samples
{
    class Program
    {
        static void Main(string[] _)
        {
            Entity expr = "x^4 - sqrt(2) x^3 + (28 x^3)/15 - (28 sqrt(2) x^2)/15 - (137 x^2)/15 + (137 sqrt(2) x)/15 + (14 x)/5 - (14 sqrt(2))/5";
            Console.WriteLine(expr.SolveEquation("x"));
        }
    }
}
