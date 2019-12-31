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
            TreeAnalyzer.Sort(linChildren);
            Entity res = new OperatorEntity(Name, isSum ? Const.PRIOR_SUM : Const.PRIOR_MUL);
            res.Children.Add(linChildren[0]);
            if (linChildren.Count != 2)
            {
                res.Children.Add(new OperatorEntity(Name, isSum ? Const.PRIOR_SUM : Const.PRIOR_MUL));
                var currParent = res;
                for (int i = 1; i < linChildren.Count - 1; i++)
                {
                    if (i == linChildren.Count - 2) // de facto if child needs to be added
                    {
                        currParent.Children[1].Children.Add(linChildren[i]);
                        currParent.Children[1].Children.Add(linChildren[i + 1]);
                        break; // Unnecessary though
                    }
                    else   // if an operator needs to be added
                    {
                        currParent.Children[1].Children.Add(linChildren[i]);
                        currParent.Children[1].Children.Add(new OperatorEntity(Name, isSum ? Const.PRIOR_SUM : Const.PRIOR_MUL));
                        currParent = currParent.Children[1];
                    }
                }
            }
            else
                res.Children.Add(linChildren[1]);
            return res;
        }
    }
}
