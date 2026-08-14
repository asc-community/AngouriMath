//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Collections.Generic;
using System.Linq;

namespace AngouriMath.Core.Transformations.Matching
{
    /// <summary>
    /// The left-hand side of a rewrite rule, as a <b>value</b> rather than as an arm of a
    /// <c>switch</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <a href="https://github.com/asc-community/AngouriMath/issues/746">#746</a> v1.0 asks for
    /// "pattern matching as a data structure, not a <c>switch</c>: matchable, enumerable,
    /// testable". Every rewrite in this library is currently a C# pattern in a <c>switch</c>
    /// expression, and three separate things tier 2 wants are blocked on that one fact — a rule
    /// cannot carry its own justification tier, a rule cannot be addressed individually
    /// (<a href="https://github.com/asc-community/AngouriMath/issues/825">#825</a>), and an
    /// e-graph cannot match against an e-class because there is no pattern to match with.
    /// </para>
    /// <para>
    /// A pattern binds its named holes as it matches, and <b>a name used twice must match the
    /// same subexpression both times</b> — which is what the <c>when any1 == any1a</c> guards
    /// on the existing rules are spelling out by hand.
    /// </para>
    /// <para>
    /// Deliberately not a general term-rewriting language. There is no associativity or
    /// commutativity in the matcher yet, so <c>a + b</c> does not match <c>b + a</c>; #248 is
    /// where that belongs, and doing it here before the plain case is proven would be
    /// guessing at the shape. What is here is the smallest thing that can express real rules
    /// and be checked against the <c>switch</c> that already expresses them.
    /// </para>
    /// </remarks>
    internal abstract class MatchPattern
    {
        /// <summary>
        /// Whether <paramref name="expr"/> matches, extending <paramref name="bindings"/> with
        /// whatever the named holes stood for. On failure the bindings may have been written
        /// to, so a caller that tries several patterns starts each with a fresh dictionary.
        /// </summary>
        internal abstract bool TryMatch(Entity expr, Dictionary<string, Entity> bindings);

        /// <summary>The names this pattern binds, so a rule can be checked for a typo in its right-hand side.</summary>
        internal abstract IEnumerable<string> BoundNames { get; }

        /// <summary>Matches anything and binds it.</summary>
        internal static MatchPattern Any(string name) => new AnyPattern(name, null);

        /// <summary>Matches anything of the given node type and binds it.</summary>
        internal static MatchPattern Any<T>(string name) where T : Entity
            => new AnyPattern(name, typeof(T));

        /// <summary>
        /// Matches anything of the given node type that also satisfies <paramref name="where"/>,
        /// and binds it.
        /// </summary>
        /// <remarks>
        /// This is the C# property pattern — <c>Integer { IsPositive: true }</c> — as data. It
        /// is a predicate on the node rather than on the bindings, which is the distinction
        /// that matters: a condition about *this hole* travels with the hole and can be read
        /// off a rule, where a condition about the match as a whole belongs in the rule's
        /// <c>when</c> and cannot.
        /// </remarks>
        internal static MatchPattern Any<T>(string name, Func<T, bool> where) where T : Entity
            => new AnyPattern(name, typeof(T), node => where((T)node));

        /// <summary>Matches exactly this expression, binding nothing.</summary>
        internal static MatchPattern Exact(Entity value) => new ExactPattern(value);

        /// <summary>Matches a node of the given type whose children match, in order.</summary>
        internal static MatchPattern Node<T>(params MatchPattern[] children) where T : Entity
            => new NodePattern(typeof(T), children);

        private sealed class AnyPattern : MatchPattern
        {
            private readonly string name;
            private readonly Type? required;
            private readonly Func<Entity, bool>? where;

            internal AnyPattern(string name, Type? required, Func<Entity, bool>? where = null)
            {
                this.name = name;
                this.required = required;
                this.where = where;
            }

            internal override IEnumerable<string> BoundNames => new[] { name };

            internal override bool TryMatch(Entity expr, Dictionary<string, Entity> bindings)
            {
                if (required is not null && !required.IsInstanceOfType(expr))
                    return false;
                if (where is not null && !where(expr))
                    return false;
                // A repeated name is the `when any1 == any1a` guard, made structural: the
                // second occurrence matches only what the first one already stood for.
                if (bindings.TryGetValue(name, out var already))
                    return already.Equals(expr);
                bindings[name] = expr;
                return true;
            }
        }

        private sealed class ExactPattern : MatchPattern
        {
            private readonly Entity value;

            internal ExactPattern(Entity value) => this.value = value;

            internal override IEnumerable<string> BoundNames => Array.Empty<string>();

            internal override bool TryMatch(Entity expr, Dictionary<string, Entity> bindings)
                => value.Equals(expr);
        }

        private sealed class NodePattern : MatchPattern
        {
            private readonly Type nodeType;
            private readonly MatchPattern[] children;

            internal NodePattern(Type nodeType, MatchPattern[] children)
            {
                this.nodeType = nodeType;
                this.children = children;
            }

            internal override IEnumerable<string> BoundNames => children.SelectMany(c => c.BoundNames);

            internal override bool TryMatch(Entity expr, Dictionary<string, Entity> bindings)
            {
                if (!nodeType.IsInstanceOfType(expr))
                    return false;
                var actual = expr.DirectChildren;
                if (actual.Count != children.Length)
                    return false;
                for (var i = 0; i < children.Length; i++)
                    if (!children[i].TryMatch(actual[i], bindings))
                        return false;
                return true;
            }
        }
    }
}
