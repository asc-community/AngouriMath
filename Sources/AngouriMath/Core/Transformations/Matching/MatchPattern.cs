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
    /// <summary>A set of named holes and what they stood for.</summary>
    internal sealed class Bindings
    {
        private readonly Dictionary<string, Entity> bound;

        internal static Bindings Empty { get; } = new(new Dictionary<string, Entity>());

        private Bindings(Dictionary<string, Entity> bound) => this.bound = bound;

        internal bool TryGet(string name, out Entity value) => bound.TryGetValue(name, out value!);

        internal Entity this[string name] => bound[name];

        internal int Count => bound.Count;

        /// <summary>
        /// A new set with one more name bound. Copied rather than mutated because matching
        /// backtracks: a branch that fails must leave nothing behind for the branch tried next,
        /// and sharing one dictionary across attempts is how a matcher silently starts
        /// accepting things it should not.
        /// </summary>
        internal Bindings With(string name, Entity value)
        {
            var copy = new Dictionary<string, Entity>(bound) { [name] = value };
            return new Bindings(copy);
        }
    }

    /// <summary>
    /// The left-hand side of a rewrite rule, as a <b>value</b> rather than as an arm of a
    /// <c>switch</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <a href="https://github.com/asc-community/AngouriMath/issues/746">#746</a> v1.0 asks for
    /// "pattern matching as a data structure, not a <c>switch</c>: matchable, enumerable,
    /// testable, with commutative and n-ary matching handled by the engine"
    /// (<a href="https://github.com/asc-community/AngouriMath/issues/248">#248</a>). Three
    /// things tier 2 wants are blocked on rules not being values: a rule cannot carry its own
    /// justification tier, a rule cannot be addressed individually
    /// (<a href="https://github.com/asc-community/AngouriMath/issues/825">#825</a>), and an
    /// e-graph cannot match against an e-class because there is no pattern to match with.
    /// </para>
    /// <para>
    /// <b>Matching enumerates solutions rather than returning one</b>, and that is not a
    /// refinement — it is what commutativity requires. <c>b*a + c*a</c> has to match
    /// <c>k*p + k*q</c> with <c>k = a</c>, and a matcher that commits to the first way of
    /// matching the left operand binds <c>k = b</c> and then fails on the right, in both
    /// orders of the sum. Only backtracking finds it, so every pattern yields every way it can
    /// match and the caller takes the first that survives to the end.
    /// </para>
    /// <para>
    /// Commutativity is over a <i>binary</i> node: <c>a + b</c> matches <c>b + a</c>. Matching
    /// across a flattened chain — <c>a + b + c</c> against <c>x + y</c> with <c>x = a + b</c> —
    /// is the n-ary half of #248 and is not here; the associative case wants the operands
    /// gathered first and is a larger change than the commutative one.
    /// </para>
    /// </remarks>
    internal abstract class MatchPattern
    {
        /// <summary>
        /// Every way <paramref name="expr"/> can match, extending <paramref name="bindings"/>.
        /// Empty where it cannot. Lazy, so a caller that wants one solution does not pay for
        /// the rest.
        /// </summary>
        internal abstract IEnumerable<Bindings> Match(Entity expr, Bindings bindings);

        /// <summary>The names this pattern binds, so a right-hand side can be checked for a typo.</summary>
        internal abstract IEnumerable<string> BoundNames { get; }

        /// <summary>Whether it matches at all, which is <see cref="Match"/> asked for one answer.</summary>
        internal bool Matches(Entity expr) => Match(expr, Bindings.Empty).Any();

        /// <summary>Matches anything and binds it.</summary>
        internal static MatchPattern Any(string name) => new AnyPattern(name, null, null);

        /// <summary>Matches anything of the given node type and binds it.</summary>
        internal static MatchPattern Any<T>(string name) where T : Entity
            => new AnyPattern(name, typeof(T), null);

        /// <summary>
        /// Matches anything of the given node type that also satisfies <paramref name="where"/>.
        /// </summary>
        /// <remarks>
        /// The C# property pattern — <c>Integer { IsPositive: true }</c> — as data. A predicate
        /// on the node travels with the hole and can be read off a rule, where a condition about
        /// the match as a whole belongs in the rule's <c>when</c> and cannot.
        /// </remarks>
        internal static MatchPattern Any<T>(string name, Func<T, bool> where) where T : Entity
            => new AnyPattern(name, typeof(T), node => where((T)node));

        /// <summary>Matches exactly this expression, binding nothing.</summary>
        internal static MatchPattern Exact(Entity value) => new ExactPattern(value);

        /// <summary>Matches a node of the given type whose children match, in order.</summary>
        internal static MatchPattern Node<T>(params MatchPattern[] children) where T : Entity
            => new NodePattern(typeof(T), children, commutative: false);

        /// <summary>
        /// Matches a two-child node of the given type whose children match <b>in either
        /// order</b>. One of these replaces the four arms a <c>switch</c> needs to say the same
        /// thing about a commutative operator.
        /// </summary>
        internal static MatchPattern Commutative<T>(MatchPattern left, MatchPattern right) where T : Entity
            => new NodePattern(typeof(T), new[] { left, right }, commutative: true);

        private sealed class AnyPattern : MatchPattern
        {
            private readonly string name;
            private readonly Type? required;
            private readonly Func<Entity, bool>? where;

            internal AnyPattern(string name, Type? required, Func<Entity, bool>? where)
            {
                this.name = name;
                this.required = required;
                this.where = where;
            }

            internal override IEnumerable<string> BoundNames => new[] { name };

            internal override IEnumerable<Bindings> Match(Entity expr, Bindings bindings)
            {
                if (required is not null && !required.IsInstanceOfType(expr)) yield break;
                if (where is not null && !where(expr)) yield break;
                // A repeated name is the `when any1 == any1a` guard, made structural: the
                // second occurrence matches only what the first one already stood for.
                if (bindings.TryGet(name, out var already))
                {
                    if (already.Equals(expr)) yield return bindings;
                    yield break;
                }
                yield return bindings.With(name, expr);
            }
        }

        private sealed class ExactPattern : MatchPattern
        {
            private readonly Entity value;

            internal ExactPattern(Entity value) => this.value = value;

            internal override IEnumerable<string> BoundNames => Array.Empty<string>();

            internal override IEnumerable<Bindings> Match(Entity expr, Bindings bindings)
            {
                if (value.Equals(expr)) yield return bindings;
            }
        }

        private sealed class NodePattern : MatchPattern
        {
            private readonly Type nodeType;
            private readonly MatchPattern[] children;
            private readonly bool commutative;

            internal NodePattern(Type nodeType, MatchPattern[] children, bool commutative)
            {
                this.nodeType = nodeType;
                this.children = children;
                this.commutative = commutative;
                if (commutative && children.Length != 2)
                    throw new ArgumentException("commutative matching is over a two-child node",
                        nameof(children));
            }

            internal override IEnumerable<string> BoundNames => children.SelectMany(c => c.BoundNames);

            internal override IEnumerable<Bindings> Match(Entity expr, Bindings bindings)
            {
                if (!nodeType.IsInstanceOfType(expr)) yield break;
                var actual = expr.DirectChildren;
                if (actual.Count != children.Length) yield break;

                foreach (var solution in MatchInOrder(actual, bindings, 0))
                    yield return solution;
                if (!commutative) yield break;
                // The other way round. Yielded second so that a rule reading the first solution
                // sees the same one a `switch` arm written in this order would have produced.
                var swapped = new[] { actual[1], actual[0] };
                foreach (var solution in MatchInOrder(swapped, bindings, 0))
                    yield return solution;
            }

            /// <summary>
            /// The cross product over the children: every way the first child matches, times
            /// every way the rest match given that. This is where backtracking happens, and it
            /// is why <see cref="Match"/> returns a sequence.
            /// </summary>
            private IEnumerable<Bindings> MatchInOrder(
                IReadOnlyList<Entity> actual, Bindings bindings, int index)
            {
                if (index == children.Length)
                {
                    yield return bindings;
                    yield break;
                }
                foreach (var head in children[index].Match(actual[index], bindings))
                    foreach (var rest in MatchInOrder(actual, head, index + 1))
                        yield return rest;
            }
        }
    }
}
