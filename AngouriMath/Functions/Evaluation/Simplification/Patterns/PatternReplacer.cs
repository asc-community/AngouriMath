
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


using AngouriMath.Core.Exceptions;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
 using AngouriMath.Core.Sys.Interfaces;
using System;
 using AngouriMath.Core.Numerix;
using AngouriMath.Core.TreeAnalysis;
using System.Linq;

[assembly: InternalsVisibleTo("UnitTests")]

namespace AngouriMath
{
    public abstract partial class Entity : ILatexiseable
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
        // TODO: Not using getters is a micro-optimization. Have performance measurements been carried out yet?
        internal Predicate<Entity>? Condition; // not a getter due to performance requirements
        internal static bool PatternMatches(Entity pattern, Entity tree)
        {
            var condition = pattern.Condition;
            if (condition is null)
                throw new ArgumentException("Not a pattern", nameof(pattern));
            if (!condition(tree))
                return false;
            return string.IsNullOrEmpty(pattern.Name) || pattern.Name == tree.Name;
        }

        /// <summary>
        /// Checks if a pattern or pattern tree matches an expression.
        /// Important to keep all constants inside Num()
        /// </summary>
        /// <param name="tree"></param>
        /// <returns>
        /// Whether it fits or not
        /// </returns>
        internal bool Match(Entity tree)
        {
            if (!(this is Pattern p))
                return this == tree;
            var PatternType = p.patType;

            if (!PatternMatches(this, tree))
                return false;

            if (PatternType == PatType.COMMON)
                return p.EqFits(tree) != null;
            
            if (PatternType == PatType.FUNCTION && PatternNumber != -1)
                return p.EqFits(tree) != null;
            if (Children.Count != tree.ChildrenCount)
                return false;
            for (int i = 0; i < Children.Count; i++)
                if (!Children[i].Match(tree.GetChild(i)))
                    return false;
            return p.EqFits(tree) != null;
        }

        /// <summary>
        /// Finds the first occurance of a subtree that fits a pattern
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns>
        /// Entity: first found subtree
        /// </returns>
        internal Entity? FindPatternSubtree(Pattern pattern)
        {
            return TreeAnalyzer.GetPatternEnumerator(this, pattern).FirstOrDefault();
        }

        /// <summary>
        /// Searchs for parent of the only argument
        /// </summary>
        /// <param name="kinder"></param>
        /// <returns></returns>
        internal Entity? FindParent(Entity kinder)
        {
            foreach (var child in Children)
            {
                if ((object)child == kinder)
                    return this;
                else
                {
                    var res = child.FindParent(kinder);
                    if (res is { })
                        return res;
                }
            }
            return null;
        }

        /// <summary>
        /// Searches for child's number
        /// </summary>
        /// <param name="kinder"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Unfolds the function into list of nodes. De facto not used yet.
        /// </summary>
        /// <returns></returns>
        public List<Entity> Unfold()
        {
            var res = new List<Entity>();
            var queue = new List<Entity>{ this };
            res.Add(this);
            while (queue.Count > 0)
            {
                var tmp = new List<Entity>();
                foreach (var q in queue)
                    tmp.AddRange(q.Children);
                res.AddRange(tmp);
                queue = tmp;
            }
            return res;
        }

        /// <summary>
        /// Not only checks but also finds subtrees for each key. It is necessary
        /// to keep equal subtrees with equal numbers.
        /// </summary>
        /// <param name="pattern"></param>
        /// <param name="matchings"></param>
        /// <returns></returns>
        internal bool PatternMakeMatch(Pattern pattern, Dictionary<int, Entity> matchings)
        {
            if (pattern.PatternNumber == -1)
            {
                if (pattern.ChildrenCount != Children.Count)
                    return false;
                for (int i = 0; i < Children.Count; i++)
                {
                    if (!(pattern.GetChild(i) is Pattern p))
                        throw new SysException("Numbers in pattern should look like Num(3)");
                    if (!Children[i].PatternMakeMatch(p, matchings))
                        return false;
                }
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

        /// <summary>
        /// We have pattern and we have keys. That is the function
        /// to get an expression from the pattern and keys.
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        internal Entity BuildTree(Dictionary<int, Entity> keys)
        {
            if (!(this is Pattern p))
                return this;
            if (keys.ContainsKey(PatternNumber))
                return keys[PatternNumber];
            var PatternType = p.patType;
            if (PatternType == PatType.NUMBER)
                return new NumberEntity(ComplexNumber.Parse(Name));
            if (PatternType == PatType.VARIABLE)
                return new VariableEntity(Name);
            var newChildren = new List<Entity>();
            foreach (var child in Children)
            {
                newChildren.Add(child.BuildTree(keys));
            }
            if (PatternType == PatType.FUNCTION)
            {
                var res = new FunctionEntity(Name)
                {
                    Children = newChildren
                };
                return res;
            }
            else if (PatternType == PatType.OPERATOR)
                return Name switch
                {
                    "sumf" => newChildren[0] + newChildren[1],
                    "minusf" => newChildren[0] - newChildren[1],
                    "mulf" => newChildren[0] * newChildren[1],
                    "divf" => newChildren[0] / newChildren[1],
                    "powf" => MathS.Pow(newChildren[0], newChildren[1]),
                    "logf" => MathS.Log(newChildren[0], newChildren[1]),
                    "sinf" => MathS.Sin(newChildren[0]),
                    "cosf" => MathS.Cos(newChildren[0]),
                    "tanf" => MathS.Tan(newChildren[0]),
                    "cotanf" => MathS.Cotan(newChildren[0]),
                    "arcsinf" => MathS.Arcsin(newChildren[0]),
                    "arccosf" => MathS.Arccos(newChildren[0]),
                    "arctanf" => MathS.Arctan(newChildren[0]),
                    "arccotanf" => MathS.Arccotan(newChildren[0]),
                    _ => throw new NotImplementedException(Name + " is not implemented"),
                };
            else
                throw new NotImplementedException(PatternType + " is not implemented");
        }
    }

    internal class Pattern : Entity
    {
        internal PatType patType;
        public Pattern(int num, PatType type, Predicate<Entity>? condition, string name = "") : base(name) {
            PatternNumber = num;
            Condition = condition;
            patType = type;
        }

        internal Dictionary<int, Entity>? EqFits(Entity tree)
        {
            // TODO: optimization
            var res = new Dictionary<int, Entity>();
            if (!tree.PatternMakeMatch(this, res))
                return null;
            else
                return res;
        }
        protected override Entity __copy()
            => new Pattern(this.PatternNumber, patType, Condition, Name);

        protected override bool EqualsTo(Entity obj) => throw new NoNeedToImplementException();
        public override int GetHashCode() => throw new NoNeedToImplementException();
        internal override Entity InnerEval() => throw new NoNeedToImplementException();
        internal override Entity InnerSimplify() => throw new NoNeedToImplementException();
        internal override void Check() => throw new NoNeedToImplementException();

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
}

namespace AngouriMath.Core.TreeAnalysis
{
    internal static partial class TreeAnalyzer
    {
        internal static void ReplaceOneInPlace(ref Entity source, Pattern oldPattern, Entity newPattern)
        {
            var sub = source.FindPatternSubtree(oldPattern);
            if (sub is null) return;

            Dictionary<int, Entity> nodeList;
            try
            {
                nodeList = oldPattern.EqFits(sub)
                    ?? throw new SysException($"Pattern from {nameof(source.FindPatternSubtree)} does not fit");
            }
            catch (SysException error)
            {
                throw new SysException("Error `" + error.Message + "` in pattern " + oldPattern.ToString());
            }
            var newNode = newPattern.BuildTree(nodeList);

            if (oldPattern.Match(source))
            {
                source = newNode;
            }
            else
            {
                var parent = source.FindParent(sub)
                    ?? throw new SysException($"{nameof(sub)} from {nameof(source.FindPatternSubtree)} has no parent");
                var number = source.FindChildrenNumber(sub);
                parent.SetChild(number, newNode);
            }
        }

        /// <summary>
        /// Processes an expression with appropriate rules
        /// </summary>
        /// <param name="rules">
        /// List of Pattern
        /// </param>
        /// <param name="source">
        /// Where to replace in/to
        /// </param>
        /// <returns></returns>
        internal static Entity Replace(RuleList rules, Entity source)
        {
            HashSet<string> replaced = new HashSet<string>();
            var res = source.DeepCopy();
            res.UpdateHash();
            string hash;
            while (!replaced.Contains(hash = res.Hash))
            {
                replaced.Add(hash);
                foreach (var (oldPattern, newPattern) in rules)
                {
                    ReplaceOneInPlace(ref res, oldPattern, newPattern);
                }
                res.UpdateHash();
            }
            return res;
        }

        internal static void ReplaceInPlace(RuleList rules, ref Entity source)
        {
            HashSet<string> replaced = new HashSet<string>();
            string hash;
            source.UpdateHash();
            while (!replaced.Contains(hash = source.Hash))
            {
                replaced.Add(hash);
                foreach (var (oldPattern, newPattern) in rules)
                {
                    ReplaceOneInPlace(ref source, oldPattern, newPattern);
                }
                source.UpdateHash();
            }
        }
    }
}
