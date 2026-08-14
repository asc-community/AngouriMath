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
    /// <remarks>
    /// <para>
    /// A cons list rather than a dictionary, because of how it is used: a handful of holes, and a
    /// new set built on every step of a backtracking search. Sharing the tail makes
    /// <see cref="With"/> one small object instead of a copy of the whole map, which measured as
    /// the larger part of what a rule expressed as data cost over the same rule as a
    /// <c>switch</c> arm.
    /// </para>
    /// <para>
    /// Immutability is not an optimisation here but a correctness requirement: a branch that
    /// fails must leave nothing behind for the branch tried next, and sharing one mutable map
    /// across attempts is how a matcher silently starts accepting things it should not.
    /// </para>
    /// </remarks>
    internal sealed class Bindings
    {
        private readonly Bindings? tail;
        private readonly string? name;
        private readonly Entity? value;

        internal static Bindings Empty { get; } = new(null, null, null);

        private Bindings(Bindings? tail, string? name, Entity? value)
        {
            this.tail = tail;
            this.name = name;
            this.value = value;
        }

        internal bool TryGet(string wanted, out Entity found)
        {
            // Newest first, so a name bound twice reads as its most recent value. Nothing binds
            // a name twice today -- AnyPattern unifies instead -- but reading the newest is the
            // only answer that stays right if something ever does.
            for (var at = this; at is not null; at = at.tail)
                if (at.name == wanted)
                {
                    found = at.value!;
                    return true;
                }
            found = null!;
            return false;
        }

        internal Entity this[string wanted]
            => TryGet(wanted, out var found)
                ? found
                : throw new KeyNotFoundException($"nothing bound to '{wanted}'");

        internal int Count
        {
            get
            {
                var count = 0;
                for (var at = this; at?.name is not null; at = at.tail) count++;
                return count;
            }
        }

        /// <summary>A new set with one more name bound, sharing this one as its tail.</summary>
        internal Bindings With(string name, Entity value) => new(this, name, value);
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
    /// Commutativity over a <i>binary</i> node — <c>a + b</c> matches <c>b + a</c> — is
    /// <see cref="Commutative{T}"/>. Matching across a flattened chain, so that a rule about two
    /// terms finds them among five, is <see cref="Gathered{T}"/>: the n-ary half of #248.
    /// </para>
    /// </remarks>
    internal abstract class MatchPattern
    {
        /// <summary>
        /// Every way <paramref name="expr"/> can match, extending <paramref name="bindings"/>.
        /// Empty where it cannot. Lazy, so a caller that wants one solution does not pay for
        /// the rest.
        /// </summary>
        internal IEnumerable<Bindings> Match(Entity expr, Bindings bindings)
        {
            // The cheap rejection, before anything is allocated. A rewrite pass asks every rule
            // about every node, so nearly every call here is a miss, and a miss that costs one
            // type test and no allocation is the difference between this form being usable in
            // the pipeline and not: measured, the guard takes the miss from 58 ns and 296 B to
            // single-digit nanoseconds and nothing.
            //
            // It must never reject something MatchCore would have accepted, which is why
            // RootType is the *declared* node type and is null wherever that is not exact.
            if (RootType is { } required && !required.IsInstanceOfType(expr))
                return NoMatch;
            return MatchCore(expr, bindings);
        }

        /// <summary>Shared, so that returning "no" does not allocate an enumerator either.</summary>
        [ConstantField]
        private static readonly IEnumerable<Bindings> NoMatch = Array.Empty<Bindings>();

        /// <summary>Matching proper, reached only once <see cref="RootType"/> has been satisfied.</summary>
        private protected abstract IEnumerable<Bindings> MatchCore(Entity expr, Bindings bindings);

        /// <summary>
        /// The node type this pattern requires at its root, where requiring one is exactly
        /// right — or <see langword="null"/> where the pattern accepts more than one type, or
        /// any type at all. <b>A wrong answer here is a silently missed rewrite</b>, so anything
        /// less than certain answers null and pays the full attempt.
        /// </summary>
        private protected abstract Type? RootType { get; }

        /// <summary>
        /// Whether this pattern can match an expression in <b>at most one way</b>, so that a
        /// caller wanting a solution needs no enumeration and no backtracking.
        /// </summary>
        /// <remarks>
        /// <para>
        /// True for everything except <see cref="Commutative{T}"/> and <see cref="Gathered{T}"/>,
        /// and false for any node containing one of those, since a choice anywhere below makes
        /// the whole pattern a search.
        /// </para>
        /// <para>
        /// This is worth distinguishing because <b>most rules are in the deterministic subset</b>
        /// and enumeration is pure overhead for them: <see cref="MatchCore"/> is an iterator, so
        /// a state machine is allocated for every node of the pattern at every attempt, and a
        /// rewrite pass makes an attempt at every node of the tree. Measured on one rule set over
        /// one tree, that was ten times the time and seven times the allocation of the
        /// <c>switch</c> the set mirrors.
        /// </para>
        /// </remarks>
        internal virtual bool IsDeterministic => true;

        /// <summary>
        /// The single way this matches, without enumerating. Only meaningful where
        /// <see cref="IsDeterministic"/>; a pattern that can match several ways must be asked
        /// through <see cref="Match"/>, which is why this is not the only entry point.
        /// </summary>
        /// <remarks>
        /// It must agree with <see cref="Match"/> exactly — the same bindings, or no match where
        /// <see cref="Match"/> yields nothing. <c>DeterministicMatchingAgreesWithEnumeration</c>
        /// is the test that holds the two together, over generated expressions, because two
        /// implementations of one thing is how a matcher acquires a case where they differ.
        /// </remarks>
        internal bool TryMatchOnce(Entity expr, Bindings bindings, out Bindings result)
        {
            if (RootType is { } required && !required.IsInstanceOfType(expr))
            {
                result = bindings;
                return false;
            }
            return TryMatchOnceCore(expr, bindings, out result);
        }

        private protected abstract bool TryMatchOnceCore(
            Entity expr, Bindings bindings, out Bindings result);

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

        /// <summary>
        /// Matches an associative-commutative chain of <typeparamref name="T"/> — a sum or a
        /// product — by finding <paramref name="parts"/> among its operands <b>in any positions</b>
        /// and binding whatever is left over to <paramref name="restName"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is the n-ary half of
        /// <a href="https://github.com/asc-community/AngouriMath/issues/248">#248</a>, and it is
        /// what lets a rule about two terms fire on an expression with five. A binary pattern
        /// cannot: <c>sin(x)^2 + cos(x)^2</c> inside <c>a + sin(x)^2 + b + cos(x)^2</c> is not two
        /// children of any one node. The library's present answer is to sort the operands with
        /// <c>CanonicalOrder</c> before running the rules so that the pair lands adjacent — which
        /// works, and is a workaround for the matcher rather than a property of the mathematics.
        /// </para>
        /// <para>
        /// <b>When nothing is left over, <paramref name="restName"/> is bound to the operator's
        /// identity</b> — <c>0</c> for a sum, <c>1</c> for a product. That is not a convenience:
        /// the empty sum <i>is</i> zero and the empty product <i>is</i> one, so one rule covers
        /// both <c>sin(x)^2 + cos(x)^2</c> and the same pair buried in a longer sum, and the
        /// right-hand side never has to ask which case it is in.
        /// </para>
        /// <para>
        /// Operands come from <c>LinearChildren</c>, so subtraction and division are already
        /// normalised: <c>a - b</c> offers <c>a</c> and <c>(-1) * b</c>, and <c>a / b</c> offers
        /// <c>a</c> and <c>b ^ (-1)</c>. A rule therefore does not need a second arm for the
        /// subtractive spelling, which is one of the ways the <c>switch</c> sets multiplied.
        /// </para>
        /// <para>
        /// <b>Cost.</b> Assigning k parts to n operands is n!/(n-k)! attempts, so this is bounded
        /// by <see cref="MaxAssignments"/> measured work rather than by a reasoned size limit, as
        /// <a href="https://github.com/asc-community/AngouriMath/issues/921">#921</a> settled for
        /// the resultant. Past the bound it stops yielding, so the rule <i>does not apply</i>
        /// rather than taking unbounded time — declining is a legitimate answer where a wrong one
        /// or a hang is not.
        /// </para>
        /// </remarks>
        internal static MatchPattern Gathered<T>(string restName, params MatchPattern[] parts)
            where T : Entity
            => new GatheredPattern(typeof(T), restName, parts);

        /// <summary>
        /// The ceiling on how many part-to-operand assignments one <see cref="Gathered{T}"/> will
        /// try. Reached only by a rule with several holes over a long chain: two parts stay under
        /// it until 100 operands, three until 22, four until 11.
        /// </summary>
        internal const int MaxAssignments = 10_000;

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

            // Exactly the constraint this pattern imposes, so the guard can carry all of it.
            private protected override Type? RootType => required;

            private protected override IEnumerable<Bindings> MatchCore(Entity expr, Bindings bindings)
            {
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

            private protected override bool TryMatchOnceCore(
                Entity expr, Bindings bindings, out Bindings result)
            {
                result = bindings;
                if (where is not null && !where(expr)) return false;
                if (bindings.TryGet(name, out var already)) return already.Equals(expr);
                result = bindings.With(name, expr);
                return true;
            }
        }

        private sealed class ExactPattern : MatchPattern
        {
            private readonly Entity value;

            internal ExactPattern(Entity value) => this.value = value;

            internal override IEnumerable<string> BoundNames => Array.Empty<string>();

            /// <summary>
            /// Null deliberately. Two entities can be equal without being the same runtime type
            /// — a rational that reduced to an integer is the standing example — so a type guard
            /// derived from the literal could reject a match that
            /// <see cref="Entity.Equals(Entity)"/> would have accepted. Equality is the cheap
            /// test here anyway.
            /// </summary>
            private protected override Type? RootType => null;

            private protected override IEnumerable<Bindings> MatchCore(Entity expr, Bindings bindings)
            {
                if (value.Equals(expr)) yield return bindings;
            }

            private protected override bool TryMatchOnceCore(
                Entity expr, Bindings bindings, out Bindings result)
            {
                result = bindings;
                return value.Equals(expr);
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

            private protected override Type? RootType => nodeType;

            private protected override IEnumerable<Bindings> MatchCore(Entity expr, Bindings bindings)
            {
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

            /// <summary>
            /// A commutative node is a choice of two orders, and a node is only as determinate
            /// as the children it is made of.
            /// </summary>
            internal override bool IsDeterministic
                => !commutative && children.All(child => child.IsDeterministic);

            private protected override bool TryMatchOnceCore(
                Entity expr, Bindings bindings, out Bindings result)
            {
                result = bindings;
                var actual = expr.DirectChildren;
                if (actual.Count != children.Length) return false;
                // Left to right, threading the bindings through. No backtracking is needed
                // because no child offers a second answer -- that is what IsDeterministic means.
                for (var i = 0; i < children.Length; i++)
                    if (!children[i].TryMatchOnce(actual[i], result, out result))
                        return false;
                return true;
            }
        }

        private sealed class GatheredPattern : MatchPattern
        {
            private readonly Type nodeType;
            private readonly string restName;
            private readonly MatchPattern[] parts;

            internal GatheredPattern(Type nodeType, string restName, MatchPattern[] parts)
            {
                if (nodeType != typeof(Entity.Sumf) && nodeType != typeof(Entity.Mulf))
                    throw new ArgumentException(
                        "gathering is over the associative operators, which are Sumf and Mulf",
                        nameof(nodeType));
                if (parts.Length == 0)
                    throw new ArgumentException("a gathered pattern with no parts matches nothing "
                        + "in particular", nameof(parts));
                this.nodeType = nodeType;
                this.restName = restName ?? throw new ArgumentNullException(nameof(restName));
                this.parts = parts;
            }

            private bool OverSum => nodeType == typeof(Entity.Sumf);

            internal override IEnumerable<string> BoundNames
                => parts.SelectMany(part => part.BoundNames).Append(restName);

            /// <summary>
            /// Null, because two types are admissible rather than one: a sum chain is
            /// <c>Sumf</c> <i>or</i> <c>Minusf</c>. <see cref="MatchCore"/> makes the test
            /// itself, and it is the first thing it does.
            /// </summary>
            private protected override Type? RootType => null;

            private protected override IEnumerable<Bindings> MatchCore(Entity expr, Bindings bindings)
            {
                // The subtractive and divisive spellings are the same chain, which is why this
                // admits them: `a - b` is a sum whose second operand LinearChildren has already
                // negated. Anything else is a chain of one and cannot hold two parts.
                var isChain = OverSum
                    ? expr is Entity.Sumf or Entity.Minusf
                    : expr is Entity.Mulf or Entity.Divf;
                if (!isChain) yield break;

                var operands = (OverSum
                    ? Entity.Sumf.LinearChildren(expr)
                    : Entity.Mulf.LinearChildren(expr)).ToList();
                if (operands.Count < parts.Length) yield break;

                var used = new bool[operands.Count];
                var chosen = new int[parts.Length];
                var budget = new Budget(MaxAssignments);
                foreach (var solution in Assign(operands, used, chosen, 0, bindings, budget))
                    yield return solution;
            }

            /// <summary>
            /// Every injective assignment of parts to operand positions, depth first, with each
            /// part matched against its operand before the next is placed — so an assignment that
            /// cannot match is abandoned rather than completed and then rejected.
            /// </summary>
            private IEnumerable<Bindings> Assign(
                List<Entity> operands, bool[] used, int[] chosen, int part,
                Bindings bindings, Budget budget)
            {
                if (part == parts.Length)
                {
                    yield return bindings.With(restName, Leftover(operands, used));
                    yield break;
                }
                for (var i = 0; i < operands.Count; i++)
                {
                    if (used[i]) continue;
                    if (!budget.Spend()) yield break;
                    used[i] = true;
                    chosen[part] = i;
                    foreach (var head in parts[part].Match(operands[i], bindings))
                        foreach (var full in Assign(operands, used, chosen, part + 1, head, budget))
                            yield return full;
                    used[i] = false;
                }
            }

            /// <summary>
            /// The operands no part claimed, folded back into one expression — or the operator's
            /// identity where every operand was claimed, because the empty sum is zero and the
            /// empty product is one.
            /// </summary>
            private Entity Leftover(List<Entity> operands, bool[] used)
            {
                Entity? rest = null;
                for (var i = 0; i < operands.Count; i++)
                {
                    if (used[i]) continue;
                    rest = rest is null
                        ? operands[i]
                        : OverSum ? rest + operands[i] : rest * operands[i];
                }
                return rest ?? (OverSum ? Entity.Number.Integer.Zero : Entity.Number.Integer.One);
            }

            /// <summary>
            /// Shared across one <see cref="Match"/> call so the bound is on the whole search and
            /// not on each branch of it, which is the difference between a bound and a suggestion.
            /// </summary>
            /// <summary>Choosing which operands the parts claim is the whole of what it does.</summary>
            internal override bool IsDeterministic => false;

            private protected override bool TryMatchOnceCore(
                Entity expr, Bindings bindings, out Bindings result)
                => throw new InvalidOperationException(
                    "a gathered pattern is a search and has to be asked through Match");

            private sealed class Budget
            {
                private int left;
                internal Budget(int total) => left = total;
                internal bool Spend() => left-- > 0;
            }
        }
    }
}
