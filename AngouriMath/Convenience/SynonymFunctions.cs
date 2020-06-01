
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

namespace AngouriMath
{
    using FuncTable = Dictionary<string, Func<List<Entity>, Entity>>;
    using SynTable = Dictionary<string, string>;
    internal static class SynonymFunctions
    {
        /// <summary>
        /// While parsing, we want to understand functions like "sqrt". After parsing, we replace
        /// nodes with this name by the appropriate expression
        /// </summary>
        internal static readonly FuncTable SynFunctions = new FuncTable
        {
            { "sqrtf", args => MathS.Pow(args[0], 0.5) },
            { "sqrf", args => MathS.Pow(args[0], 2) },
            { "lnf", args => MathS.Log(MathS.e, args[0]) },
            { "secf", args => MathS.Sec(args[0]) },
            { "cosecf", args => MathS.Cosec(args[0]) },
            { "arcsecf", args => MathS.Arcsec(args[0]) },
            { "arccosecf", args => MathS.Arccosec(args[0]) },
        };

        /// <summary>
        /// Expects a tree with "sqrt" and some other unresolved functions. Returns
        /// that with all "sqrt" and other replaced
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        internal static Entity Synonymize(Entity tree)
        {
            for (int i = 0; i < tree.Children.Count; i++)
            {
                tree.Children[i] = Synonymize(tree.Children[i]);
            }
            if (SynFunctions.ContainsKey(tree.Name))
                return SynFunctions[tree.Name](tree.Children);
            else
                return tree;
        }
    }
}
