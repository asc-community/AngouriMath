using AngouriMath.Core.TreeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AngouriMath
{
    public abstract partial class Entity
    {
        internal string Hash()
        {
            if (this is FunctionEntity)
                return this.Name + "_" + string.Join("_", from child in Children select child.Hash());
            else if (this is NumberEntity)
                return ""; // Constants do not influence sorting
            else if (this is VariableEntity)
                return "v_" + Name;
            else
                return string.Join("_", from child in Children where child.Hash() != "" select child.Hash()); // Operators only influence through children

        }
        public Entity Sort()
        {
            foreach (var child in Children)
                child.Sort();
            if (Name != "sumf" && Name != "mulf" && Name != "minusf" && Name != "divf")
                return DeepCopy();
            var isSum = this.Name == "sumf" || this.Name == "minusf";
            var linChildren = TreeAnalyzer.LinearChildren(this, 
                                                          isSum ? "sumf" : "mulf", 
                                                          isSum ? "minusf" : "divf",
                                                          isSum ?  "mulf" : "powf",
                                                          isSum ? Const.PRIOR_MUL : Const.PRIOR_POW);
            var groups = TreeAnalyzer.GroupByHash(linChildren);
            var grouppedChildren = new List<Entity>();
            foreach (var pair in groups)
                grouppedChildren.Add(TreeAnalyzer.MultiHang(pair.Value, 
                    isSum ? "sumf" : "mulf", isSum ? Const.PRIOR_SUM : Const.PRIOR_MUL));
            return TreeAnalyzer.MultiHang(grouppedChildren, isSum ? "sumf" : "mulf", isSum ? Const.PRIOR_SUM : Const.PRIOR_MUL);
        }
    }
}

namespace AngouriMath.Core.TreeAnalysis
{
    internal static partial class TreeAnalyzer
    {
        internal static Dictionary<string, List<Entity>> GroupByHash(List<Entity> entities)
        {
            var res = new Dictionary<string, List<Entity>>();
            foreach (var ent in entities)
            {
                var hash = ent.Hash();
                if (!res.ContainsKey(hash))
                    res[hash] = new List<Entity>();
                res[hash].Add(ent);
            }
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