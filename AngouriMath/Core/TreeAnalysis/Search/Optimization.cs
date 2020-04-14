
/* Copyright (c) 2019-2020 Angourisoft
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,
 * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software
 * is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */



﻿using System;
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
