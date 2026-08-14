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
    /// The right-hand side is a builder over the bindings rather than a second pattern. That is
    /// a deliberate first step and not the end state: a rule whose right-hand side is also data
    /// can be read backwards, which is what tier 2's "direction" field wants. Building it as
    /// data before the matching half is proven would be guessing at two shapes at once.
    /// </para>
    /// </remarks>
    internal sealed class MatchedRule
    {
        internal MatchedRule(
            string name,
            MatchPattern left,
            Func<Bindings, Entity> right,
            Soundness soundness,
            Func<Bindings, bool>? when = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Left = left ?? throw new ArgumentNullException(nameof(left));
            this.right = right ?? throw new ArgumentNullException(nameof(right));
            Soundness = soundness;
            this.when = when;
        }

        private readonly Func<Bindings, Entity> right;
        private readonly Func<Bindings, bool>? when;

        /// <summary>What to call this rule in a report, a test or a bug.</summary>
        internal string Name { get; }

        /// <summary>The shape it fires on.</summary>
        internal MatchPattern Left { get; }

        /// <summary>How well justified this rule's claim is — per rule, which is the point.</summary>
        internal Soundness Soundness { get; }

        /// <summary>
        /// The rewritten expression, or <see langword="null"/> where the rule does not apply.
        /// Never throws: a builder that fails on the bindings it was handed is a rule that did
        /// not apply, which is a refusal rather than an error.
        /// </summary>
        internal Entity? TryApply(Entity expr)
        {
            // Every way the pattern matches, in order, and the first that also satisfies the
            // side condition wins. Taking only the first *match* would be wrong: commutativity
            // means `b*a + c*a` matches `k*p + k*q` several ways and only some of them bind
            // `k` to the factor the condition is about.
            foreach (var bindings in Left.Match(expr, Bindings.Empty))
            {
                if (when is not null && !when(bindings))
                    continue;
                try { return right(bindings); }
                catch { return null; }
            }
            return null;
        }
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
