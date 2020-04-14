
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
    internal static partial class TreeAnalyzer
    {
        internal static List<Entity> LinearChildren(Entity tree,
                                             string funcName /*e. g. "sumf" */,
                                             string badFuncName /* e. g. "minusf" */,
                                             Func<Entity, OperatorEntity> supCreator)
        {
            var res = new List<Entity>();
            if (tree.Name == funcName || tree.Name == badFuncName)
            {
                res.AddRange(LinearChildren(tree.Children[0], funcName, badFuncName, supCreator));
                if (tree.Name == funcName)
                    res.AddRange(LinearChildren(tree.Children[1], funcName, badFuncName, supCreator));
                else
                {
                    // Here we need to inverse
                    var badRes = LinearChildren(tree.Children[1], funcName, badFuncName, supCreator);
                    var goodRes = new List<Entity>();
                    foreach (var child in badRes)
                        goodRes.Add(supCreator(child));
                    res.AddRange(goodRes);
                }
            }
            else
                res.Add(tree);
            return res;
        }
        internal static void Sort(List<Entity> children, SortLevel level) => children.Sort((a, b) => a.Hash(level).CompareTo(b.Hash(level)));        
    }
}
