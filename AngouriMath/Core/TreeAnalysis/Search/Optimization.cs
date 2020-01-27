using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Core.TreeAnalysis
{
    using FunctionCategory = HashSet<string>;
    internal static partial class TreeAnalyzer
    {
        internal static class Optimization
        {
            internal static readonly FunctionCategory Trigonometry = new FunctionCategory { 
                "sinf",
                "cosf",
                "tanf",
                "cotanf",
                "arcsinf",
                "arccosf",
                "arctanf",
                "arccotanf"
            };
            internal static readonly FunctionCategory Power = new FunctionCategory { 
                "powf"
                // TODO: to add logarithm?
            };
            private static bool Contains(Entity expr, FunctionCategory cat)
            {
                if (cat.Contains(expr.Name))
                    return true;
                foreach (var child in expr.Children)
                    if (Contains(child, cat))
                        return true;
                return false;
            }
            internal static bool ContainsTrigonometric(Entity expr) => Contains(expr, Trigonometry);
            internal static bool ContainsPower(Entity expr) => Contains(expr, Power);
        }
    }
}
