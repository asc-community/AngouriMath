
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
 using System.Linq;
 using System.Text;
 using AngouriMath.Core.Sys.Interfaces;
 using AngouriMath.Core.TreeAnalysis;


namespace AngouriMath
{
    public abstract partial class Entity : ILatexiseable
    {
        internal string Hash { get; private set; }
        internal int HashOccurances { get; set; }
        internal string UpdateHash()
        {
            var ownHash = Name;
            var sb = new StringBuilder();
            foreach (var ch in Children)
                sb.Append(ch.UpdateHash());
            Hash = Const.HashString(sb.ToString() + ownHash);
            return Hash;
        }

        internal static void HashOccurancesUpdate(Entity expr)
        {
            var unfolded = expr.Unfold();

            // First, we count number of occurances for each hash
            var counts = new Dictionary<string, int>();
            foreach (var node in unfolded)
            {
                if (!counts.ContainsKey(node.Hash))
                    counts[node.Hash] = 0;
                counts[node.Hash]++;
            }

            // Second, we assign those numbers to each node
            foreach (var node in unfolded)
                node.HashOccurances = counts[node.Hash];
        }
    }
}

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

            internal static void OptimizeTree(ref Entity expr)
            {
                if (expr.entType == Entity.EntType.OPERATOR)
                {
                    if (expr.Name == "sumf" || expr.Name == "minusf")
                    {
                        var children = TreeAnalyzer.LinearChildren(expr, "sumf", "minusf", Const.FuncIfSum);
                        expr = TreeAnalyzer.MultiHangBinary(children, "sumf", expr.Priority);
                    }
                    if (expr.Name == "mulf" || expr.Name == "divf")
                    {
                        var children = TreeAnalyzer.LinearChildren(expr, "mulf", "divf", Const.FuncIfMul);
                        expr = TreeAnalyzer.MultiHangBinary(children, "mulf", expr.Priority);
                    }
                }

                for (int i = 0; i < expr.Children.Count; i++)
                {
                    var tmp = expr.Children[i];
                    OptimizeTree(ref tmp);
                    expr.Children[i] = tmp;
                }
            }

            internal static int CountDepth(Entity expr)
                => 1 + (expr.Children.Count == 0 ? 0 : expr.Children.Select(CountDepth).Max());
        }
    }
}
