
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
 using AngouriMath.Core.Sys.Interfaces;

namespace AngouriMath
{
    using SortLevel = TreeAnalyzer.SortLevel;
    public abstract partial class Entity : ILatexiseable
    {
        /// <summary>
        /// Hash that is convenient to sort with
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        internal string SortHash(SortLevel level)
        {
            if (this.entType == Entity.EntType.FUNCTION)
                return this.Name + "_" + string.Join("_", from child in Children select child.SortHash(level));
            else if (this.entType == EntType.NUMBER)
            {
                if (level == SortLevel.HIGH_LEVEL)
                    return "";
                else if (level == SortLevel.MIDDLE_LEVEL)
                    return (this as NumberEntity).Value.Type.ToString();
                else
                    return this.Name + " ";
            }
            else if (this.entType == Entity.EntType.VARIABLE)
                return "v_" + Name;
            else
                return (level == SortLevel.LOW_LEVEL ? this.Name + "_" : "") + string.Join("_", 
                    from child in Children
                    let hash = child.SortHash(level)
                    where !string.IsNullOrEmpty(hash)
                    select hash);

        }
    }
}

namespace AngouriMath.Core.TreeAnalysis
{
    internal static partial class TreeAnalyzer
    {
        internal enum SortLevel
        {
            HIGH_LEVEL, // Variables, functions. Doesn't pay attention to constants or ops
            MIDDLE_LEVEL, // Contants are now countable
            LOW_LEVEL, // De facto full hash
        }

        /// <summary>
        /// One group - one hash,
        /// Different hashes - different groups
        /// </summary>
        /// <param name="entities"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        internal static List<List<Entity>> GroupByHash(List<Entity> entities, SortLevel level)
        {
            var dict = new Dictionary<string, List<Entity>>();
            foreach (var ent in entities)
            {
                var hash = ent.SortHash(level);
                if (!dict.ContainsKey(hash))
                    dict[hash] = new List<Entity>();
                dict[hash].Add(ent);
            }
            var res = new List<List<Entity>>();
            var keys = dict.Keys.ToList();
            keys.Sort();
            foreach (var key in keys)
                res.Add(dict[key]);
            return res;
        }

        /// <summary>
        /// Linear multi hanging
        /// (1 + (1 + (1 + 1)))
        /// </summary>
        /// <param name="children"></param>
        /// <param name="opName"></param>
        /// <param name="opPrior"></param>
        /// <returns></returns>
        internal static Entity MultiHangLinear(List<Entity> children, string opName, int opPrior)
        {
            if (children.Count == 0)
                throw new TreeException("At least 1 child required");
            if (children.Count == 1)
                return children[0];
            Entity res = new OperatorEntity(opName, opPrior);
            res.Children.Add(children[0]);
            res.Children.Add(MultiHangLinear(children.GetRange(1, children.Count - 1), opName, opPrior));
            return res;
        }

        /// <summary>
        /// Binary multi hanging
        /// ((1 + 1) + (1 + 1))
        /// </summary>
        /// <param name="children"></param>
        /// <param name="opName"></param>
        /// <param name="opPrior"></param>
        /// <returns></returns>
        internal static Entity MultiHangBinary(List<Entity> children, string opName, int opPrior)
        {
            if (children.Count == 0)
                throw new TreeException("At least 1 child required");
            if (children.Count == 1)
                return children[0];
            Entity res = new OperatorEntity(opName, opPrior);
            res.Children.Add(
                MultiHangBinary(
                    children.GetRange(0, children.Count / 2)
                    , opName, opPrior)
                );
            res.Children.Add(
                MultiHangBinary(
                    children.GetRange(children.Count / 2, children.Count - (children.Count / 2))
                , opName, opPrior)
                );
            return res;
        }

        /// <summary>
        /// Actual sorting with sortHash
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="level"></param>
        internal static void Sort(ref Entity tree, SortLevel level)
        {
            Func<Entity, OperatorEntity> funcIfSum = Const.FuncIfSum;
            Func<Entity, OperatorEntity> funcIfMul = Const.FuncIfMul;
            for (int i = 0; i < tree.Children.Count; i++)
            {
                Entity tmp = tree.Children[i];
                Sort(ref tmp, level);
                tree.Children[i] = tmp;
            }
            if (tree.Name != "sumf" && tree.Name != "mulf" && tree.Name != "minusf" && tree.Name != "divf")
                return;
            var isSum = tree.Name == "sumf" || tree.Name == "minusf";
            var linChildren = TreeAnalyzer.LinearChildren(tree,
                                                          isSum ? "sumf" : "mulf",
                                                          isSum ? "minusf" : "divf",
                                                          isSum ? funcIfSum : funcIfMul);
            var groups = TreeAnalyzer.GroupByHash(linChildren, level);
            var grouppedChildren = new List<Entity>();
            foreach (var list in groups)
                grouppedChildren.Add(TreeAnalyzer.MultiHangLinear(list,
                    isSum ? "sumf" : "mulf", isSum ? Const.PRIOR_SUM : Const.PRIOR_MUL));
            tree = TreeAnalyzer.MultiHangLinear(grouppedChildren, isSum ? "sumf" : "mulf", isSum ? Const.PRIOR_SUM : Const.PRIOR_MUL);
        }
    }
}