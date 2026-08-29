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
    /// The e-graph counterpart of <see cref="Bindings"/>: a set of named holes, each standing for
    /// an e-class id rather than an <see cref="Entity"/>. Same cons-list shape, for the same reason
    /// -- see <see cref="Bindings"/>'s own remarks -- plus one concrete win it gets for free: a name
    /// bound twice (<c>x - x -&gt; 0</c>'s repeated <c>x</c>) becomes an O(1) class-id comparison
    /// instead of an <see cref="Entity.Equals(Entity)"/> call.
    /// </summary>
    internal sealed class EBindings
    {
        private readonly EBindings? tail;
        private readonly string? name;
        private readonly int value;

        internal static EBindings Empty { get; } = new(null, null, 0);

        private EBindings(EBindings? tail, string? name, int value)
        {
            this.tail = tail;
            this.name = name;
            this.value = value;
        }

        internal bool TryGet(string wanted, out int found)
        {
            for (var at = this; at is not null; at = at.tail)
                if (at.name == wanted)
                {
                    found = at.value;
                    return true;
                }
            found = 0;
            return false;
        }

        /// <summary>A new set with one more name bound, sharing this one as its tail.</summary>
        internal EBindings With(string name, int value) => new(this, name, value);
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
        /// The node type this pattern requires at its root, where it requires one — the same
        /// question <see cref="RootType"/> answers, offered to the registry so that a rule
        /// written as data can say which node it fires on.
        /// </summary>
        internal Type? RequiredRootType => RootType;

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

        /// <summary>
        /// How many candidate matches this pattern can offer at most, or
        /// <see cref="Unbounded"/> when it cannot say.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Between <see cref="IsDeterministic"/> — exactly one — and <see cref="Match"/> —
        /// however many — sits the case that is neither and is very common: a
        /// <see cref="Commutative{T}"/> node of deterministic children, which offers the written
        /// order and the swapped one and nothing else. Enumerating two candidates through
        /// <see cref="MatchCore"/> allocates an iterator state machine per pattern node, and a
        /// rewrite pass makes an attempt at every node of the tree, so on a set that runs on
        /// every pass that is measurable: 165.05 MB to 171.37 MB of <c>SolveMediumHard</c>, +3.8%,
        /// for two commutative rules in <c>RewriteRules.Power</c>.
        /// <a href="https://github.com/asc-community/AngouriMath/issues/1079">#1079</a>
        /// </para>
        /// <para>
        /// This is an <b>upper bound</b>, not a count: a child whose name is already bound offers
        /// one candidate or none depending on what it is asked to match, which is not known until
        /// it is asked. <see cref="TryMatchChoice"/> answers <see langword="false"/> for an index
        /// that does not exist, so a caller walks every index and skips the misses.
        /// </para>
        /// </remarks>
        internal virtual int ChoiceCount => 1;

        /// <summary>A pattern that cannot bound its candidates, and must be enumerated.</summary>
        internal const int Unbounded = 0;

        /// <summary>
        /// The <paramref name="choice"/>th way this matches, counted the way
        /// <see cref="Match"/> yields them — so choice <c>i</c> is the <c>i</c>th element of that
        /// sequence, once the indices that do not exist are skipped.
        /// </summary>
        /// <remarks>
        /// It must agree with <see cref="Match"/> in content and in order.
        /// <c>BoundedMatchingAgreesWithEnumeration</c> is the test that holds the two together
        /// over generated expressions, for the reason the deterministic path has one: two
        /// implementations of one thing is how a matcher acquires a case where they differ.
        /// </remarks>
        internal bool TryMatchChoice(Entity expr, Bindings bindings, int choice, out Bindings result)
        {
            if (RootType is { } required && !required.IsInstanceOfType(expr))
            {
                result = bindings;
                return false;
            }
            return TryMatchChoiceCore(expr, bindings, choice, out result);
        }

        /// <summary>
        /// One candidate by index. The default is the deterministic one, which is right for every
        /// pattern that offers a single match; <see cref="NodePattern"/> overrides it.
        /// </summary>
        private protected virtual bool TryMatchChoiceCore(
            Entity expr, Bindings bindings, int choice, out Bindings result)
        {
            result = bindings;
            return choice == 0 && TryMatchOnceCore(expr, bindings, out result);
        }

        /// <summary>The names this pattern binds, so a right-hand side can be checked for a typo.</summary>
        internal abstract IEnumerable<string> BoundNames { get; }

        /// <summary>
        /// Whether this pattern can be read as a <b>template</b> as well as a pattern — whether
        /// <see cref="TryBuild"/> can put an expression together out of it.
        /// </summary>
        /// <remarks>
        /// Structural, and independent of any bindings, so a rule is classified once where it is
        /// written rather than once per expression it is asked about. False only where a node type
        /// is one <see cref="Construct"/> does not build.
        /// </remarks>
        internal abstract bool IsBuildable { get; }

        /// <summary>
        /// The expression this pattern stands for under <paramref name="bindings"/>, or
        /// <see langword="false"/> where those bindings do not satisfy it.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The other direction of <see cref="Match"/>, at the grain of one pattern: matching takes
        /// an expression apart into bindings, and this puts one together out of them. A rule whose
        /// two sides are both patterns therefore has two directions rather than one, which is what
        /// <a href="https://github.com/asc-community/AngouriMath/issues/746">#746</a> tier 2 means
        /// by a rule that can be read backwards.
        /// </para>
        /// <para>
        /// <b>A hole's own constraint is checked here as well as when matching.</b> That is what
        /// makes reversal lose nothing: a constraint is written once, on the side that states it,
        /// and is enforced whenever that side is built — so the reverse of
        /// <c>(a/b)^c -&gt; a^c/b^c</c> may match any <c>c</c> at all and still refuses to build
        /// the quotient-of-powers unless <c>c</c> is the positive integer the forward rule
        /// required.
        /// </para>
        /// </remarks>
        internal abstract bool TryBuild(Bindings bindings, out Entity built);

        /// <summary>
        /// Builds a node of <paramref name="nodeType"/> over <paramref name="children"/>, or
        /// <see langword="null"/> where this cannot -- see the remarks on <see cref="Construct"/>,
        /// which this exposes. <see cref="EGraph"/> uses this to rebuild an extracted e-node's
        /// type without a second, drifting list of the same operators.
        /// </summary>
        internal static Entity? ConstructNode(Type nodeType, Entity[] children) => Construct(nodeType, children);

        /// <summary>
        /// A node of <paramref name="nodeType"/> over <paramref name="children"/>, or
        /// <see langword="null"/> where that is not a node this builds.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Written out rather than reflected. <c>Activator.CreateInstance</c> would build any node
        /// type and would make this un-trimmable and un-AOT-publishable, which
        /// <c>Docs/Contributing/Trimming.md</c> forbids for the kernel; and a node's constructor is
        /// not reachable through <c>IUnaryNode</c> or <c>IBinaryNode</c>, which expose the children
        /// and nothing else.
        /// </para>
        /// <para>
        /// A node type absent from here is <b>matchable but not buildable</b>: a pattern over it is
        /// still a rule's left-hand side, and <see cref="IsBuildable"/> is what says it cannot be a
        /// right-hand side or be reached by reading a rule backwards. Adding a type is one line,
        /// and <c>EveryDataRuleIsBuildableOnBothSides</c> is the test that says when one is owed.
        /// </para>
        /// </remarks>
        private static Entity? Construct(Type nodeType, Entity[] children)
        {
            if (children.Length == 1)
            {
                var only = children[0];
                return
                    nodeType == typeof(Entity.Sinf) ? new Entity.Sinf(only) :
                    nodeType == typeof(Entity.Cosf) ? new Entity.Cosf(only) :
                    nodeType == typeof(Entity.Tanf) ? new Entity.Tanf(only) :
                    nodeType == typeof(Entity.Cotanf) ? new Entity.Cotanf(only) :
                    nodeType == typeof(Entity.Secantf) ? new Entity.Secantf(only) :
                    nodeType == typeof(Entity.Cosecantf) ? new Entity.Cosecantf(only) :
                    nodeType == typeof(Entity.Absf) ? new Entity.Absf(only) :
                    nodeType == typeof(Entity.Signumf) ? new Entity.Signumf(only) :
                    (Entity?)null;
            }
            if (children.Length == 2)
            {
                Entity first = children[0], second = children[1];
                return
                    nodeType == typeof(Entity.Sumf) ? new Entity.Sumf(first, second) :
                    nodeType == typeof(Entity.Minusf) ? new Entity.Minusf(first, second) :
                    nodeType == typeof(Entity.Mulf) ? new Entity.Mulf(first, second) :
                    nodeType == typeof(Entity.Divf) ? new Entity.Divf(first, second) :
                    nodeType == typeof(Entity.Powf) ? new Entity.Powf(first, second) :
                    nodeType == typeof(Entity.Logf) ? new Entity.Logf(first, second) :
                    (Entity?)null;
            }
            return null;
        }

        /// <summary>
        /// Whether <see cref="Construct"/> builds this type at this arity, asked by building one
        /// over placeholders rather than by listing the types a second time — a list written twice
        /// is a list that drifts.
        /// </summary>
        private static bool CanConstruct(Type nodeType, int arity)
        {
            if (arity is not (1 or 2))
                return false;
            var placeholders = new Entity[arity];
            for (var i = 0; i < arity; i++)
                placeholders[i] = Entity.Number.Integer.Zero;
            return Construct(nodeType, placeholders) is not null;
        }

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

            public override string ToString()
                => required is null
                    ? (where is null ? $"var {name}" : $"var {name} where")
                    : (where is null ? $"{required.Name} {name}" : $"{required.Name} {name} where");

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

            internal override bool IsBuildable => true;

            internal override bool TryBuild(Bindings bindings, out Entity built)
            {
                built = null!;
                if (!bindings.TryGet(name, out var value)) return false;
                // The same two constraints the match imposes, imposed again on the way out. A
                // rule read backwards binds this hole from its other side, where the constraint
                // is not written, so this is the only place left to enforce it.
                if (required is not null && !required.IsInstanceOfType(value)) return false;
                if (where is not null && !where(value)) return false;
                built = value;
                return true;
            }
        }

        private sealed class ExactPattern : MatchPattern
        {
            private readonly Entity value;

            internal ExactPattern(Entity value) => this.value = value;

            internal override IEnumerable<string> BoundNames => Array.Empty<string>();

            public override string ToString() => value.Stringize();

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

            internal override bool IsBuildable => true;

            internal override bool TryBuild(Bindings bindings, out Entity built)
            {
                built = value;
                return true;
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
                buildable = CanConstruct(nodeType, children.Length)
                    && children.All(child => child.IsBuildable);
                deterministic = !commutative && children.All(child => child.IsDeterministic);
            }

            /// <summary>Settled here because it depends on nothing that changes afterwards.</summary>
            private readonly bool buildable;

            /// <summary>
            /// Settled here for the same reason as <see cref="buildable"/>, and it matters more:
            /// <c>MatchedRule.TryApply</c> reads it on <b>every attempt</b>, and a rewrite pass
            /// makes an attempt at every node of the tree with every rule of the set. Computed on
            /// each read it walked the whole pattern tree behind a delegate before any matching
            /// began -- so the cost was paid most heavily by the case that does the least work,
            /// a rule that does not fire.
            /// </summary>
            private readonly bool deterministic;

            internal override IEnumerable<string> BoundNames => children.SelectMany(c => c.BoundNames);

            public override string ToString()
                => (commutative ? "~" : "") + nodeType.Name
                   + "(" + string.Join(", ", children.Select(child => child.ToString())) + ")";

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
            internal override bool IsDeterministic => deterministic;

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

            /// <summary>
            /// The product over the children, doubled for a commutative node because it offers
            /// the written order and the swapped one. <see cref="Unbounded"/> as soon as one
            /// child is, which is how a <see cref="GatheredPattern"/> anywhere inside makes the
            /// whole pattern something to enumerate.
            /// </summary>
            internal override int ChoiceCount
            {
                get
                {
                    if (choices != -1) return choices;
                    long total = 1;
                    foreach (var child in children)
                    {
                        var count = child.ChoiceCount;
                        if (count == Unbounded) return choices = Unbounded;
                        total *= count;
                        if (total > MaxChoices) return choices = Unbounded;
                    }
                    if (commutative) total *= 2;
                    return choices = total > MaxChoices ? Unbounded : (int)total;
                }
            }

            /// <summary>
            /// Past this a pattern is treated as unbounded. It is not a correctness limit — the
            /// indexing is exact at any size — but a bound past which walking every index is no
            /// longer obviously cheaper than enumerating, and a guard against a pattern whose
            /// product overflows.
            /// </summary>
            private const int MaxChoices = 64;

            private int choices = -1;

            private protected override bool TryMatchChoiceCore(
                Entity expr, Bindings bindings, int choice, out Bindings result)
            {
                result = bindings;
                var actual = expr.DirectChildren;
                if (actual.Count != children.Length) return false;

                // `MatchCore` yields every solution in the written order first and then, for a
                // commutative node, every solution in the swapped one -- so the low half of the
                // index space is the written order and the high half is the swapped one.
                var perOrder = ChoiceCount;
                if (perOrder == Unbounded) return false;
                var swapped = false;
                if (commutative)
                {
                    perOrder /= 2;
                    if (choice >= perOrder) { swapped = true; choice -= perOrder; }
                }

                if (choice < 0 || choice >= perOrder) return false;

                // Mixed radix over the children, with the first child the most significant digit
                // -- because `MatchInOrder` makes it the outermost loop, so it is the one that
                // varies slowest in the sequence this has to agree with. The suffix product is
                // recomputed rather than stored: there are one to three children, and an array
                // here would be the allocation this whole path exists to avoid.
                for (var i = 0; i < children.Length; i++)
                {
                    var suffix = 1;
                    for (var j = i + 1; j < children.Length; j++) suffix *= children[j].ChoiceCount;
                    var digit = choice / suffix % children[i].ChoiceCount;
                    var against = swapped ? actual[children.Length - 1 - i] : actual[i];
                    if (!children[i].TryMatchChoice(against, result, digit, out result))
                        return false;
                }
                return true;
            }

            internal override bool IsBuildable => buildable;

            internal override bool TryBuild(Bindings bindings, out Entity built)
            {
                built = null!;
                // The written order, and for a commutative node too. Both orders denote the same
                // value, a template has to write one of them down, and the one the rule was
                // written in is the one its author meant to read.
                var parts = new Entity[children.Length];
                for (var i = 0; i < children.Length; i++)
                    if (!children[i].TryBuild(bindings, out parts[i]))
                        return false;
                if (Construct(nodeType, parts) is not { } node)
                    return false;
                built = node;
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

            public override string ToString()
                => nodeType.Name + "(" + string.Join(", ", parts.Select(part => part.ToString()))
                   + ", ... var " + restName + ")";

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

            /// <summary>
            /// Unbounded, and that is the point of it: how many ways k parts sit among n operands
            /// is a property of the expression rather than of the pattern, so this is the one
            /// shape that has to be enumerated.
            /// </summary>
            internal override int ChoiceCount => Unbounded;

            private protected override bool TryMatchOnceCore(
                Entity expr, Bindings bindings, out Bindings result)
                => throw new InvalidOperationException(
                    "a gathered pattern is a search and has to be asked through Match");

            internal override bool IsBuildable => parts.All(part => part.IsBuildable);

            internal override bool TryBuild(Bindings bindings, out Entity built)
            {
                built = null!;
                Entity? chain = null;
                foreach (var part in parts)
                {
                    if (!part.TryBuild(bindings, out var one)) return false;
                    chain = chain is null ? one : Combine(chain, one);
                }
                if (!bindings.TryGet(restName, out var rest)) return false;
                // The identity is dropped rather than written back. Matching a chain that holds
                // nothing but the parts binds the rest to 0 or to 1 -- see Leftover -- and putting
                // that in would build `sin(x)^2 + cos(x)^2 + 0`, which is the same value written
                // worse. There is at least one part, so the chain is never empty.
                built = IsIdentity(rest) ? chain! : Combine(chain!, rest);
                return true;
            }

            private Entity Combine(Entity left, Entity right)
                => OverSum ? new Entity.Sumf(left, right) : new Entity.Mulf(left, right);

            private bool IsIdentity(Entity operand)
                => operand.Equals(OverSum ? Entity.Number.Integer.Zero : Entity.Number.Integer.One);

            private sealed class Budget
            {
                private int left;
                internal Budget(int total) => left = total;
                internal bool Spend() => left-- > 0;
            }
        }
    }
}
