//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AngouriMath.Core.Transformations.Matching
{
    /// <summary>
    /// One rewrite rule, addressable on its own: a name, a pattern to match, a side condition,
    /// what to build, and the tier its claim is justified at.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is what <a href="https://github.com/asc-community/AngouriMath/issues/825">#825</a>
    /// asks for and what a <c>switch</c> arm cannot be. A rule here can be listed, named in a
    /// bug report, tested by itself, and — the part that matters most —
    /// <b>carry its own <see cref="Soundness"/></b>. Today the tier is declared per rule *set*,
    /// and since a set's tier is the minimum over its arms, one conditional arm drags eighteen
    /// unconditional ones down with it; that is why all thirty sets in the registry declare the
    /// same value and the field distinguishes nothing.
    /// </para>
    /// <para>
    /// <b>Where the right-hand side is a pattern too, the rule has two directions rather than
    /// one.</b> <see cref="Reversed"/> is the same rule read the other way, and
    /// <see cref="Reversal"/> says why a rule has no such reading when it has none. That is
    /// <a href="https://github.com/asc-community/AngouriMath/issues/746">#746</a> tier 2's first
    /// missing piece, and <c>Docs/Contributing/ReversibleRules.md</c> is the argument for when a
    /// reversal is licensed.
    /// </para>
    /// </remarks>
    internal sealed class MatchedRule
    {
        /// <summary>A rule whose right-hand side is a pattern, and which therefore has two directions.</summary>
        internal MatchedRule(
            string name,
            MatchPattern left,
            MatchPattern right,
            Soundness soundness,
            Func<Bindings, bool>? when = null,
            [CallerLineNumber] int line = 0)
            : this(name, left, null, right ?? throw new ArgumentNullException(nameof(right)), soundness, when, line)
        {
            // A name the replacement reads and the pattern never binds is a typo, and it is a
            // typo that would otherwise show up as a rule that silently never fires. Only a
            // right-hand side written as data can be checked for it at all.
            foreach (var wanted in right.BoundNames)
                if (!left.BoundNames.Contains(wanted))
                    throw new ArgumentException(
                        $"'{name}' builds '{wanted}', which its pattern does not bind", nameof(right));
            // And the same failure mode from the other direction. A node type is matchable
            // without being buildable -- see MatchPattern.Construct -- so a template naming one
            // is a rule that matches, builds nothing, and is indistinguishable at run time from a
            // rule that did not apply. It cost an afternoon and a twenty-eight-row agreement
            // failure before this check existed, which is the argument for the check.
            if (!right.IsBuildable)
                throw new ArgumentException(
                    $"'{name}' has a replacement this cannot build: some node type in it is "
                    + "matchable but not constructible. Add it to MatchPattern.Construct, or "
                    + "write the replacement as code.", nameof(right));
        }

        /// <summary>
        /// A rule whose right-hand side is code. One-way: see <see cref="RuleReversal.ReplacementIsCode"/>.
        /// </summary>
        internal MatchedRule(
            string name,
            MatchPattern left,
            Func<Bindings, Entity> right,
            Soundness soundness,
            Func<Bindings, bool>? when = null,
            [CallerLineNumber] int line = 0)
            : this(name, left, right ?? throw new ArgumentNullException(nameof(right)), null, soundness, when, line)
        {
        }

        private MatchedRule(
            string name,
            MatchPattern left,
            Func<Bindings, Entity>? rightCode,
            MatchPattern? rightPattern,
            Soundness soundness,
            Func<Bindings, bool>? when,
            int line)
        {
            SourceLine = line;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Left = left ?? throw new ArgumentNullException(nameof(left));
            this.right = rightCode;
            Right = rightPattern;
            Soundness = soundness;
            this.when = when;
            // Settled here rather than on first ask. It depends on nothing that changes
            // afterwards, and a lazily cached enum is a field wider than a word: two threads
            // arriving together could read a half-written one, where a reference cannot tear.
            Reversal = Classify();
        }

        private readonly Func<Bindings, Entity>? right;
        private readonly Func<Bindings, bool>? when;

        /// <summary>What to call this rule in a report, a test or a bug.</summary>
        internal string Name { get; }

        /// <summary>
        /// The line this rule is declared on, taken from the compiler rather than written down.
        /// </summary>
        /// <remarks>
        /// A rule written as data has a real source location like a <c>switch</c> arm does, and
        /// <c>[CallerLineNumber]</c> is how it keeps one without anybody maintaining it. It is
        /// what lets a data rule appear in the registry beside the arms.
        /// </remarks>
        internal int SourceLine { get; }

        /// <summary>Whether this rule carries a side condition beyond its pattern.</summary>
        internal bool HasCondition => when is not null;

        /// <summary>The shape it fires on.</summary>
        internal MatchPattern Left { get; }

        /// <summary>
        /// What it puts there instead, where that is a pattern — or <see langword="null"/> where
        /// the replacement is code and there is nothing to match against.
        /// </summary>
        internal MatchPattern? Right { get; }

        /// <summary>How well justified this rule's claim is — per rule, which is the point.</summary>
        internal Soundness Soundness { get; }

        /// <summary>
        /// The rewritten expression, or <see langword="null"/> where the rule does not apply.
        /// Never throws: a builder that fails on the bindings it was handed is a rule that did
        /// not apply, which is a refusal rather than an error.
        /// </summary>
        internal Entity? TryApply(Entity expr)
        {
            // A pattern with no choice in it has at most one match, so asking for a sequence of
            // them allocates an iterator per pattern node to deliver either zero answers or one.
            // Most rules are of that kind, and a rewrite pass makes an attempt at every node of
            // the tree, so this path is worth having separately.
            if (Left.IsDeterministic)
            {
                if (!Left.TryMatchOnce(expr, Bindings.Empty, out var only)) return null;
                if (when is not null && !when(only)) return null;
                return Build(only);
            }

            // Every way the pattern matches, in order, and the first that also satisfies the
            // side condition wins. Taking only the first *match* would be wrong: commutativity
            // means `b*a + c*a` matches `k*p + k*q` several ways and only some of them bind
            // `k` to the factor the condition is about.
            foreach (var bindings in Left.Match(expr, Bindings.Empty))
            {
                if (when is not null && !when(bindings))
                    continue;
                // A match whose replacement cannot be built is a match that does not apply, and
                // another way of matching may still. Only the last of them decides that the rule
                // declines.
                if (Build(bindings) is { } rewritten)
                    return rewritten;
            }
            return null;
        }

        /// <summary>
        /// The replacement under these bindings, or <see langword="null"/> where it cannot be
        /// built — which is the rule declining rather than an error, whichever form the
        /// right-hand side takes.
        /// </summary>
        private Entity? Build(Bindings bindings)
        {
            if (Right is not null)
                return Right.TryBuild(bindings, out var built) ? built : null;
            try { return right!(bindings); }
            catch { return null; }
        }

        /// <summary>
        /// Whether this rule can be read backwards, and where it cannot, why not.
        /// </summary>
        /// <remarks>
        /// <b>Derived from the two sides, not declared.</b> Every clause below is a property of
        /// what the rule is written as, so the distinction between a one-way rewrite and a
        /// two-way one is something a test can fail on rather than something a comment asserts.
        /// </remarks>
        internal RuleReversal Reversal { get; }

        private RuleReversal Classify()
        {
            if (Right is null)
                return RuleReversal.ReplacementIsCode;
            // Set equality, and the direction that can fail is the one checked: a name the
            // replacement builds and the pattern does not bind is refused in the constructor, so
            // what is left is a name the pattern binds and the replacement throws away.
            foreach (var bound in Left.BoundNames)
                if (!Right.BoundNames.Contains(bound))
                    return RuleReversal.ReplacementDropsHoles;
            if (!Left.IsBuildable || !Right.IsBuildable)
                return RuleReversal.PatternCannotBeBuilt;
            return RuleReversal.Reversible;
        }

        /// <summary>
        /// This rule read the other way, or <see langword="null"/> where it has no such reading.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The two sides swap and nothing else does. The side condition is carried over
        /// unchanged, because it is a predicate on the bindings and both directions produce the
        /// same bindings; and <see cref="Soundness"/> is carried over unchanged, because what a
        /// rewrite rule claims is an equality and an equality is symmetric.
        /// </para>
        /// <para>
        /// <b>What does not carry over is termination.</b> A rule that collects becomes one that
        /// expands, so a reversed rule composed with the rule it came from does not reach a fixed
        /// point — <c>k*p + k*q</c> and <c>k*(p + q)</c> rewrite to each other forever. A reversed
        /// set is a thing to ask questions of, not one to run to stability.
        /// </para>
        /// </remarks>
        internal MatchedRule? Reversed
            => Reversal is RuleReversal.Reversible
                ? reversed ??= new MatchedRule($"reversed {Name}", Right!, Left, Soundness, when)
                : null;

        private MatchedRule? reversed;
    }

    /// <summary>
    /// Whether a <see cref="MatchedRule"/> can be read backwards, and where it cannot, what stops
    /// it.
    /// </summary>
    /// <remarks>
    /// <b>Not every rewrite has an inverse worth having, and this says which do.</b>
    /// <c>x - x -&gt; 0</c> is a rewrite nobody wants to read backwards and nobody could: from
    /// <c>0</c> there is no recovering which <c>x</c> was cancelled. That is
    /// <see cref="ReplacementDropsHoles"/>, and it is a fact about the rule rather than a
    /// judgement about it.
    /// </remarks>
    internal enum RuleReversal
    {
        /// <summary>Both sides are patterns over the same holes, and both can be built.</summary>
        Reversible,

        /// <summary>
        /// The replacement is a builder over the bindings rather than a pattern, so there is
        /// nothing to match an expression against.
        /// </summary>
        ReplacementIsCode,

        /// <summary>
        /// The replacement does not mention every hole the pattern binds, so reading it backwards
        /// would have to invent what the forward direction discarded. The Pythagorean identity is
        /// the standing example: <c>sin(x)^2 + cos(x)^2 -&gt; 1</c> forgets the angle.
        /// </summary>
        ReplacementDropsHoles,

        /// <summary>
        /// One of the two sides is over a node type this cannot construct, so it can be matched
        /// and not written. See <c>MatchPattern.Construct</c>.
        /// </summary>
        PatternCannotBeBuilt
    }

    /// <summary>
    /// An ordered list of <see cref="MatchedRule"/>, applied first-match-wins over every node —
    /// the same discipline the <c>switch</c>-based rule sets follow, so that one can be
    /// exchanged for the other and the two compared.
    /// </summary>
    internal sealed class MatchedRuleSet
    {
        internal MatchedRuleSet(string name, params MatchedRule[] rules)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            this.rules = rules ?? throw new ArgumentNullException(nameof(rules));
        }

        /// <summary>
        /// Held as the array as well as exposed as a list, and walked as the array in
        /// <see cref="ApplyHere"/>. <c>foreach</c> over an <see cref="IReadOnlyList{T}"/> goes
        /// through the interface and boxes an enumerator; over the array it does not. That is
        /// 32 bytes per rule set per node, on a path a rewrite pass takes for every node in the
        /// tree, and it was the whole of what remained after the match itself stopped allocating.
        /// </summary>
        private readonly MatchedRule[] rules;

        // Indexing the rules by the node type each one requires -- the thing a `switch` cannot
        // do and a set of values can -- was tried here and is deliberately absent. Measured, a
        // per-runtime-type cache of the applicable rules cost 24 bytes per node and 912 bytes on
        // a pass, deterministically and reproducibly, and bought a time improvement that sat
        // inside this machine's run-to-run drift. Where the allocation came from was never
        // accounted for, and an optimisation that makes the deterministic column worse for an
        // unexplained reason is not one to keep. The cost being fought is in the match attempt
        // rather than in the dispatch, so an index is aimed at the wrong half.

        internal string Name { get; }

        /// <summary>The rules, in the order they are tried. <b>Enumerable</b>, which is the whole point.</summary>
        internal IReadOnlyList<MatchedRule> Rules => rules;

        /// <summary>
        /// The rules of this set that can be read backwards, read backwards — so that
        /// <see cref="ApplyHere(Entity)"/> on it answers <i>what could this expression have come
        /// from</i> rather than <i>what does it become</i>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Smaller than the set it comes from whenever some of its rules are one-way, and
        /// <see cref="MatchedRule.Reversal"/> on each says which. A set none of whose rules
        /// reverses has no rules here, which is the difference being reported rather than hidden.
        /// </para>
        /// <para>
        /// <b>Not a rule set to run.</b> It is first-match-wins like any other, so applying it and
        /// then the set it came from need not return what you started with — the two sets order
        /// their rules independently. And every rule in it undoes one in the set it came from, so
        /// composing the two does not terminate. What it is for is asking.
        /// </para>
        /// </remarks>
        internal MatchedRuleSet Reversed
            => reversed ??= new MatchedRuleSet(
                $"reversed {Name}",
                rules.Select(rule => rule.Reversed).OfType<MatchedRule>().ToArray());

        private MatchedRuleSet? reversed;

        /// <summary>
        /// The weakest tier any of its rules is justified at — derived rather than declared,
        /// so it cannot drift from the rules it is about.
        /// </summary>
        internal Soundness Soundness
            => rules.Length == 0 ? Soundness.Sound : rules.Max(rule => rule.Soundness);

        /// <summary>The first rule that applies at this node, or null.</summary>
        internal MatchedRule? FirstMatching(Entity expr)
        {
            foreach (var rule in rules)
                if (rule.TryApply(expr) is not null)
                    return rule;
            return null;
        }

        /// <summary>One rewrite at this node only, leaving children alone.</summary>
        /// <summary>
        /// These rules as <see cref="RewriteRule"/> values, so that a set written as data can
        /// appear in the registry beside the sets written as a <c>switch</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>RuleRegistryGenerator</c> reads a <c>switch</c>'s arms and cannot read this, which
        /// is the half of
        /// <a href="https://github.com/asc-community/AngouriMath/issues/825">#825</a> still open.
        /// Its cost is not only that a converted set keeps its <c>switch</c>: a set the generator
        /// <i>declines</i> — an ordinary method with branches and locals — has <b>no addressable
        /// rules at all</b>, and nothing about it can be listed, checked or named.
        /// </para>
        /// <para>
        /// This is the same problem approached from the other side. The rules here are already
        /// values; what was missing was their rendering. Everything is read off the rule rather
        /// than restated: the pattern and the replacement render themselves, the line comes from
        /// the compiler through <c>[CallerLineNumber]</c>, and the node type comes from what the
        /// pattern requires at its root. A side condition is a delegate and cannot be quoted, so
        /// it is reported as being there.
        /// </para>
        /// </remarks>
        internal IReadOnlyList<RewriteRule> AsAddressable()
        {
            var built = new RewriteRule[rules.Length];
            for (var i = 0; i < rules.Length; i++)
            {
                var rule = rules[i];
                var pattern = rule.Left.ToString() ?? "";
                var replacement = rule.Right?.ToString();
                built[i] = new RewriteRule(
                    source: Name,
                    index: i,
                    name: rule.Name,
                    description: null,
                    nodeTypes: rule.Left.RequiredRootType is { } root
                        ? new[] { root }
                        : Array.Empty<Type>(),
                    patternSource: pattern,
                    guardSource: rule.HasCondition ? "a side condition on the bindings" : null,
                    replacementSource: replacement ?? "(built by code)",
                    growth: replacement is null ? RewriteRuleGrowth.Unknown
                        : replacement.Length > pattern.Length ? RewriteRuleGrowth.Expands
                        : replacement.Length < pattern.Length ? RewriteRuleGrowth.Collects
                        : RewriteRuleGrowth.Rearranges,
                    sourceLine: rule.SourceLine,
                    apply: rule.TryApply);
            }
            return built;
        }

        internal Entity ApplyHere(Entity expr)
        {
            // The array rather than the IReadOnlyList property: see the note on `rules`.
            foreach (var rule in rules)
                if (rule.TryApply(expr) is { } rewritten)
                    return rewritten;
            return expr;
        }
    }
}
