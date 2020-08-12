
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
using AngouriMath.Core.Sys.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace AngouriMath.Core.TreeAnalysis
{
    internal static partial class TreeAnalyzer
    {
        [System.Obsolete("NO", true)]
        /// <summary>
        /// Replaces all entries of oldsubtree with newsubtree
        /// </summary>
        /// <param name="originTree"></param>
        /// <param name="oldSubtree"></param>
        /// <param name="newSubtree"></param>
        internal static void FindAndReplace(ref Entity originTree, Entity? oldSubtree, Entity newSubtree)
        {
            if (oldSubtree is null)
                return;
            if (originTree == oldSubtree)
            {
                originTree = newSubtree;
                return;
            }
            for (int i = 0; i < originTree.ChildrenCount; i++)
            {
                var child = originTree.GetChild(i);
                FindAndReplace(ref child, oldSubtree, newSubtree);
                originTree.SetChild(i, child);
            }
        }

        [System.Obsolete("NO", true)]
        internal static bool ContainsName(Entity expr, string name)
        {
            if (name == expr.Name)
                return true;
            foreach (var child in expr.ChildrenReadonly)
                if (ContainsName(child, name))
                    return true;
            return false;
        }

        [System.Obsolete("NO", true)]
        internal static IEnumerable<Entity> GetPatternEnumerator(Entity expr, Pattern p)
        {
            if (p.Match(expr) && p.EqFits(expr) != null)
                yield return expr;

            for (int i = 0; i < expr.ChildrenCount; i++)
            {
                foreach(var res in GetPatternEnumerator(expr.GetChild(i), p))
                {
                    yield return res;
                }
            }
        }
    }
}

namespace AngouriMath
{
    public abstract partial record Entity : ILatexiseable
    {
        [System.Obsolete("NO", true)]
        /// <summary>
        /// Checks whether a subtree can be found in a tree
        /// </summary>
        public bool SubtreeIsFound(Entity subtree)
        {
            if (subtree is VariableEntity)
                return HasVar(subtree.Name);
            return FindSubtree(subtree) is {};
        }

        [System.Obsolete("NO", true)]
        /// <summary>
        /// Finds a subtree in the tree
        /// </summary>
        /// <returns></returns>
        public Entity? FindSubtree(Entity subtree)
        {
            if (this == subtree)
                return this;
            else
                foreach (var child in Children)
                {
                    var found = child.FindSubtree(subtree);
                    if (found is { })
                        return found;
                }
            return null;
        }

        [System.Obsolete("NO", true)]
        /// <summary>
        /// Finds out whether "name" is mentioned at least once
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal bool ContainsName(string name) => TreeAnalyzer.ContainsName(this, name);

    }
}
