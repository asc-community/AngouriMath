using AngouriMath.Core;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace AngouriMath
{
    internal static class CompiledMathFunctions
    {
        internal static readonly Dictionary<string, int> func2Num = new Dictionary<string, int>
        {
            { "sumf", 0 },
            { "minusf", 1 },
            { "mulf", 2 },
            { "divf", 3 },
            { "powf", 4 },
            { "sinf", 5 },
            { "cosf", 6 },
            { "tanf", 7 },
            { "cotanf", 8 },
            { "logf", 9 },
            { "arcsinf", 10 },
            { "arccosf", 11 },
            { "arctanf", 12 },
            { "arccotanf", 13 },
        };
    }
}
