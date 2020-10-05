

using AngouriMath;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Set;

namespace Samples
{
    class Program
    {
        static void Main(string[] _)
        {
            var a = MathS.FromString("abs(x)");
            a.Stringize();
        }
    }
}