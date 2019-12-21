using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath
{
    public abstract partial class Entity
    {
        internal enum PatType
        {
            NONE,
            COMMON, 
            NUMBER,
            VARIABLE,
            FUNCTION,
            OPERATOR
        }
        internal int PatternNumber { get; set; }
        internal PatType Type { get; set; }
        internal static bool PatternMatches(Entity pattern, Entity tree)
        {
            if (!( pattern.Type == PatType.NUMBER && tree is NumberEntity ||
                   pattern.Type == PatType.FUNCTION && tree is FunctionEntity ||
                   pattern.Type == PatType.OPERATOR && tree is OperatorEntity ||
                   pattern.Type == PatType.VARIABLE && tree is VariableEntity))
                return false;
            return pattern.Name == "" || pattern.Name == tree.Name;
        }
        public bool Match(Entity tree)
        {
            if (this.Type == PatType.NONE)
                //throw new InvalidOperationException("You need an instance of Pattern to call Match()");
                return this == tree;
            if (Type == PatType.COMMON)
                return true;
            if (!PatternMatches(this, tree))
                return false;
            if (Children.Count != tree.Children.Count)
                return false;
            for (int i = 0; i < Children.Count; i++)
                if (!Children[i].Match(tree.Children[i]))
                    return false;
            return (this as Pattern).EqFits(tree) != null;
        }
        internal Entity FindSubtree(Pattern pattern)
        {
            if (pattern.Match(this) && pattern.EqFits(this) != null)
                return this;
            foreach(var child in Children)
            {
                var res = child.FindSubtree(pattern);
                if (!(res == null))
                    return res;
            }
            return null;
        }
        internal Entity FindParent(Entity kinder)
        {
            foreach (var child in Children)
            {
                if ((object)child == kinder)
                    return this;
                else
                {
                    var res = child.FindParent(kinder);
                    if (res != null)
                        return res;
                }
            }
            return null;
        }
        internal int FindChildrenNumber(Entity kinder)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                if ((object)Children[i] == (object)kinder)
                    return i;
                else
                {
                    var tmp = Children[i].FindChildrenNumber(kinder);
                    if (tmp != -1)
                        return tmp;
                }
            }
            return -1;
        }
        public List<Entity> Unfold()
        {
            var res = new List<Entity>();
            var queue = new List<Entity>();
            queue.Add(this);
            res.Add(this);
            while(queue.Count > 0)
            {
                var tmp = new List<Entity>();
                foreach (var q in queue)
                    tmp.AddRange(q.Children);
                res.AddRange(tmp);
                queue = tmp;
            }
            return res;
        }
        internal bool PatternMakeMatch(Pattern pattern, Dictionary<int, Entity> matchings)
        {
            if (pattern.PatternNumber == -1)
            {
                if (pattern.Children.Count != Children.Count)
                    return false;
                for (int i = 0; i < Children.Count; i++)
                    if (!Children[i].PatternMakeMatch((pattern.Children[i] as Pattern), matchings))
                        return false;
            }
            else
            {
                if (!matchings.ContainsKey(pattern.PatternNumber) || matchings[pattern.PatternNumber] == this)
                    matchings[pattern.PatternNumber] = this;
                else
                    return false;
            }
            return true;
        }
        internal Entity BuildTree(Dictionary<int, Entity> keys)
        {
            if (!(this is Pattern))
                return this;
            if (keys.ContainsKey(PatternNumber))
                return keys[PatternNumber];
            var newChildren = new List<Entity>();
            foreach (var child in Children)
                newChildren.Add((child as Pattern).BuildTree(keys));
            Entity res;
            if (Type == PatType.FUNCTION)
                res = new FunctionEntity(Name);
            else if (Type == PatType.OPERATOR)
                switch (Name)
                {
                    case "sumf": return newChildren[0] + newChildren[1];
                    case "minusf": return newChildren[0] - newChildren[1];
                    case "mulf": return newChildren[0] * newChildren[1];
                    case "divf": return newChildren[0] / newChildren[1];
                    case "powf": return MathS.Pow(newChildren[0], newChildren[1]);
                    case "logf": return MathS.Log(newChildren[0], newChildren[1]);
                    case "sinf": return MathS.Sin(newChildren[0]);
                    case "cosf": return MathS.Cos(newChildren[0]);
                }
            return null;
        }
    }


    internal class Pattern : Entity
    {
        public Pattern(int num, PatType type, string name="") : base(name) {
            PatternNumber = num;
            Type = type;
        }
        internal Dictionary<int, Entity> EqFits(Entity tree)
        {
            var res = new Dictionary<int, Entity>();
            if (!tree.PatternMakeMatch(this, res))
                return null;
            else
                return res;
        }
        public static Pattern operator +(Pattern a, Pattern b) => Sumf.PHang(a, b);
        public static Pattern operator +(Pattern a, Entity b) => Sumf.PHang(a, b);
        public static Pattern operator +(Entity a, Pattern b) => Sumf.PHang(a, b);
        public static Pattern operator -(Pattern a, Pattern b) => Minusf.PHang(a, b);
        public static Pattern operator -(Pattern a, Entity b) => Minusf.PHang(a, b);
        public static Pattern operator -(Entity a, Pattern b) => Minusf.PHang(a, b);
        public static Pattern operator *(Pattern a, Pattern b) => Mulf.PHang(a, b);
        public static Pattern operator *(Pattern a, Entity b) => Mulf.PHang(a, b);
        public static Pattern operator *(Entity a, Pattern b) => Mulf.PHang(a, b);
        public static Pattern operator /(Pattern a, Pattern b) => Divf.PHang(a, b);
        public static Pattern operator /(Pattern a, Entity b) => Divf.PHang(a, b);
        public static Pattern operator /(Entity a, Pattern b) => Divf.PHang(a, b);
    }

    public class PatternReplacer
    {
        internal static Entity ReplaceOne(Entity source, Pattern oldPattern, Entity newPattern)
        {
            var src = source.DeepCopy();
            var sub = src.FindSubtree(oldPattern);
            if (sub == null)
                return src;
            var nodeList = oldPattern.EqFits(sub);
            var newNode = newPattern.BuildTree(nodeList);
            if (oldPattern.Match(source))
                return newNode;
            else
            {
                var parent = src.FindParent(sub);
                var number = src.FindChildrenNumber(sub);
                parent.Children[number] = newNode;
                return src;
            }
        }
        internal static Entity Replace(Entity source)
        {
            var res = source.DeepCopy();
            foreach (var pair in Patterns.patterns)
                while (res.FindSubtree(pair.Key) != null)
                    res = ReplaceOne(res, pair.Key, pair.Value);
            return res;
        }
    }
}
