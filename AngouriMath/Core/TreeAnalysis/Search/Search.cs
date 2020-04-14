
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



﻿using AngouriMath.Core.TreeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AngouriMath.Core.TreeAnalysis
{
    internal static partial class TreeAnalyzer
    {
        internal static void GetUniqueVariables(Entity expr, EntitySet dst)
        {
            // If it is a variable, we will add it
            // 1 2 1 1 1 2 1
            if (expr.entType == Entity.EntType.VARIABLE)
            {
                // But if it is a constant, we ignore it
                if (!MathS.ConstantList.ContainsKey(expr.Name))
                    dst.Add(expr);
            }
            else
                // Otherwise, we will try to find unique variables from its children
                foreach (var child in expr.Children)
                    GetUniqueVariables(child, dst);
        }
        

        internal static void FindAndReplace(ref Entity originTree, Entity oldSubtree, Entity newSubtree)
        {
            if (originTree == oldSubtree)
            {
                originTree = newSubtree;
                return;
            }
            for (int i = 0; i < originTree.Children.Count; i++)
            {
                var child = originTree.Children[i];
                FindAndReplace(ref child, oldSubtree, newSubtree);
                originTree.Children[i] = child;
            }
        }

        internal static bool ContainsName(Entity expr, string name)
        {
            if (name == expr.Name)
                return true;
            foreach (var child in expr.Children)
                if (ContainsName(child, name))
                    return true;
            return false;
        }
    }
}

namespace AngouriMath
{
    public abstract partial class Entity
    {
        /// <summary>
        /// Finds a subtree in the tree
        /// </summary>
        /// <returns></returns>
        public Entity FindSubtree(Entity subtree)
        {
            if (this == subtree)
                return this;
            else
                foreach (var child in Children)
                {
                    Entity found = child.FindSubtree(subtree);
                    if (found != null)
                        return found;
                }
            return null;
        }

        /// <summary>
        /// Finds out whether "name" is mentioned at least once
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal bool ContainsName(string name) => TreeAnalyzer.ContainsName(this, name);

        /// <summary>
        /// Finds out whether an expression contains at least one tensor
        /// </summary>
        /// <returns></returns>
        internal bool IsTensoric() => ContainsName("tensort");
    }
}
