using AngouriMath.Core.TreeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AngouriMath
{
    using SortLevel = TreeAnalyzer.SortLevel;
    public abstract partial class Entity
    {
        internal string Hash(SortLevel level)
        {
            if (this.entType == Entity.EntType.FUNCTION)
                return this.Name + "_" + string.Join("_", from child in Children select child.Hash(level));
            else if (this.entType == EntType.NUMBER)
                return level == SortLevel.HIGH_LEVEL ? "" : this.Name + " ";
            else if (this.entType == Entity.EntType.VARIABLE)
                return "v_" + Name;
            else
                return (level == SortLevel.LOW_LEVEL ? this.Name + "_" : "") + string.Join("_", 
                    from child in Children
                    let hash = child.Hash(level)
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
        internal static List<List<Entity>> GroupByHash(List<Entity> entities, SortLevel level)
        {
            var dict = new Dictionary<string, List<Entity>>();
            foreach (var ent in entities)
            {
                var hash = ent.Hash(level);
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

        internal static Entity MultiHang(List<Entity> children, string opName, int opPrior)
        {
            if (children.Count == 1)
                return children[0];
            Entity res = new OperatorEntity(opName, opPrior);
            res.Children.Add(children[0]);
            res.Children.Add(MultiHang(children.GetRange(1, children.Count - 1), opName, opPrior));
            return res;
        }

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
                grouppedChildren.Add(TreeAnalyzer.MultiHang(list,
                    isSum ? "sumf" : "mulf", isSum ? Const.PRIOR_SUM : Const.PRIOR_MUL));
            tree = TreeAnalyzer.MultiHang(grouppedChildren, isSum ? "sumf" : "mulf", isSum ? Const.PRIOR_SUM : Const.PRIOR_MUL);
        }
    }
}