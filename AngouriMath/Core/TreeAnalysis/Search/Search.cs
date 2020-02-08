using AngouriMath.Core.TreeAnalysis;
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
            => expr.Name == name || expr.Children.Select(p => ContainsName(p, name)).Any();
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

        internal bool IsTensoric() => ContainsName("tensort");
    }
}
