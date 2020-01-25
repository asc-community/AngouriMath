using System;
using System.Collections.Generic;
using System.Linq;
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
        public new void Add(Entity ent) => Add(ent, true);
        public void Add(Entity ent, bool check)
        {
            if (!check)
            {
                base.Add(ent);
                return;
            }
            if (ent == null)
                return;
            if (ent.entType == Entity.EntType.NUMBER && ent.GetValue().IsNull)
                return;
            ent = ent.Simplify();
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
        // TODO: needs optimization
        public static EntitySet operator +(EntitySet set, Entity a) => new EntitySet(set.Select(el => el + a));
        public static EntitySet operator -(EntitySet set, Entity a) => new EntitySet(set.Select(el => el - a));
        public static EntitySet operator *(EntitySet set, Entity a) => new EntitySet(set.Select(el => el * a));
        public static EntitySet operator /(EntitySet set, Entity a) => new EntitySet(set.Select(el => el / a));
        public static EntitySet operator +(Entity a, EntitySet set) => new EntitySet(set.Select(el => a + el));
        public static EntitySet operator -(Entity a, EntitySet set) => new EntitySet(set.Select(el => a - el));
        public static EntitySet operator *(Entity a, EntitySet set) => new EntitySet(set.Select(el => a * el));
        public static EntitySet operator /(Entity a, EntitySet set) => new EntitySet(set.Select(el => a / el));
    }
    internal static partial class TreeAnalyzer
    {
        internal static void GetUniqueVariables(Entity expr, EntitySet dst)
        {
            // If it is a variable, we will add it
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
        internal static bool IsZero(Entity e) => MathS.CanBeEvaluated(e) && e.Eval() == 0;

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
