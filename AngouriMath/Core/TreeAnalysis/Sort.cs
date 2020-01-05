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
            if (this is FunctionEntity)
                return this.Name + "_" + string.Join("_", from child in Children select child.Hash(level));
            else if (this is NumberEntity)
                return level == SortLevel.HIGH_LEVEL ? "" : this.Name + " ";
            else if (this is VariableEntity)
                return "v_" + Name;
            else
                return (level == SortLevel.LOW_LEVEL ? this.Name + "_" : "") + string.Join("_", from child in Children where child.Hash(level) != "" select child.Hash(level));

        }

        internal Entity Sort(SortLevel level)
        {
            OperatorEntity FuncIfSum(Entity child)
            {
                return new OperatorEntity("mulf", Const.PRIOR_MUL)
                {
                    Children = new List<Entity> {
                    -1,
                    child
                    }
                };
            }
            OperatorEntity FuncIfMul(Entity child)
            {
                return new OperatorEntity("powf", Const.PRIOR_POW)
                {
                    Children = new List<Entity> {
                    child,
                    -1
                    }
                };
            }
            Func<Entity, OperatorEntity> funcIfSum = FuncIfSum;
            Func<Entity, OperatorEntity> funcIfMul = FuncIfMul;
            for (int i = 0; i < Children.Count; i++)
                Children[i] = Children[i].Sort(level);
            if (Name != "sumf" && Name != "mulf" && Name != "minusf" && Name != "divf")
                return DeepCopy();
            var isSum = this.Name == "sumf" || this.Name == "minusf";
            var linChildren = TreeAnalyzer.LinearChildren(this, 
                                                          isSum ? "sumf" : "mulf", 
                                                          isSum ? "minusf" : "divf",
                                                          isSum ? funcIfSum : funcIfMul);
            var groups = TreeAnalyzer.GroupByHash(linChildren, level);
            var grouppedChildren = new List<Entity>();
            foreach (var list in groups)
                grouppedChildren.Add(TreeAnalyzer.MultiHang(list, 
                    isSum ? "sumf" : "mulf", isSum ? Const.PRIOR_SUM : Const.PRIOR_MUL));
            return TreeAnalyzer.MultiHang(grouppedChildren, isSum ? "sumf" : "mulf", isSum ? Const.PRIOR_SUM : Const.PRIOR_MUL);
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
    }
}