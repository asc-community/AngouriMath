using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Core.TreeAnalysis
{
    public class EntitySet : List<Entity>
    {
        private readonly HashSet<string> exsts = new HashSet<string>();
        public override string ToString()
        {
            return "[" + string.Join(", ", this) + "]";
        }
        public new void Add(Entity ent)
        {
            if (ent == null)
                return;
            if (ent.entType == Entity.EntType.NUMBER && ent.GetValue().IsNull)
                return;
            ent = ent.SimplifyIntelli();
            var hash = ent.ToString();
            if (!exsts.Contains(hash))
            {
                base.Add(ent);
                exsts.Add(hash);
            }
        }
        public void Merge(IEnumerable<Number> list)
        {
            foreach (var l in list)
                Add(l);
        }
        public void Merge(IEnumerable<Entity> list)
        {
            foreach (var l in list)
                Add(l);
        }
        public EntitySet(params Entity[] entites)
        {
            foreach (var el in entites)
                Add(el);
        }
        public EntitySet(IEnumerable<Entity> list)
        {
            foreach (var el in list)
                Add(el);
        }
    }
    internal static partial class TreeAnalyzer
    {
        internal static void GetUniqueVariables(Entity expr, EntitySet dst)
        {
            if (expr.entType == Entity.EntType.VARIABLE)
                dst.Add(expr);
            else
                foreach (var child in expr.Children)
                    GetUniqueVariables(child, dst);
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
    }
}
