//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Core.Transformations
{
    /// <summary>
    /// Turns a rewrite into a sentence, in one place so that a step, a pass and a whole derivation
    /// all say things the same way.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <a href="https://github.com/asc-community/AngouriMath/issues/746">#746</a> tier 2's last
    /// requirement is "transformation metadata rich enough that v5.0 can render a step as a
    /// sentence". This is the check on that claim: metadata is rich enough to render a sentence
    /// exactly when a sentence can be rendered from it, and nothing else settles it.
    /// </para>
    /// <para>
    /// <b>Every word here comes off a rule.</b> There is no table of phrasings, no per-rule English
    /// written a second time, and no verb chosen by looking at what a rewrite did — which is the
    /// shape this would have taken if the metadata had not been there, and the shape that goes
    /// stale the first time a rule changes. A rule's name is already a clause a person wrote, its
    /// description is already the identity, and the only work is joining them.
    /// </para>
    /// </remarks>
    internal static class Explanation
    {
        /// <summary>
        /// Whether a rule's name is a phrase in English, rather than a rendered pattern.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>The distinction is real and this is not a heuristic about it.</b> A rule written as
        /// data is named by whoever wrote it, in words:
        /// <c>dividing-by-a-quotient-multiplies-by-its-reciprocal</c>. A rule read off a
        /// <c>switch</c> is named by <c>RuleRegistryGenerator</c>, which has no words to use and
        /// names the arm by its own pattern:
        /// <c>Divf(Sinf(var any1), Cosf(var any1a)) when any1 == any1a</c>. The two are told apart
        /// exactly, by whether the name is lower-case letters and hyphens — a rendered pattern has
        /// capitals, brackets and spaces, and cannot be mistaken for one.
        /// </para>
        /// <para>
        /// It matters because the first version of this rendered every name as a clause, and
        /// produced <i>"Divf(Sinf(var any1), Cosf(var any1a)) when any1 == any1a, so sin(x) /
        /// cos(x) becomes tan(x)"</i> — a sentence that quotes a matcher at the reader, which the
        /// remark on <see cref="Sentence"/> says not to do while doing it. A name that is not prose
        /// is not made into prose here; the sentence falls back to the identity, and then to naming
        /// the set.
        /// </para>
        /// </remarks>
        internal static bool IsProse(string? ruleName)
        {
            if (string.IsNullOrEmpty(ruleName)) return false;
            var hyphens = 0;
            foreach (var ch in ruleName!)
            {
                if (ch == '-') { hyphens++; continue; }
                if (ch < 'a' || ch > 'z') return false;
            }
            return hyphens > 0;
        }

        /// <summary>
        /// A rule's name as the clause it was written as:
        /// <c>dividing-by-a-quotient-multiplies-by-its-reciprocal</c> is <i>dividing by a quotient
        /// multiplies by its reciprocal</i>, with the first letter raised to open a sentence.
        /// </summary>
        /// <remarks>
        /// <b>A rename rather than a translation, and deliberately nothing more.</b> Replacing the
        /// hyphens is the whole of it: every one of the rule names written as data is already a
        /// phrase in English, because that is what they were written as. A rule whose name does not
        /// read as prose is a name to fix — <c>RuleProseTest</c> is where that is held — and not a
        /// case for a table of exceptions here, which would put the readable version somewhere the
        /// author of the rule does not look.
        /// </remarks>
        internal static string AsSentenceOpening(string ruleName)
        {
            var clause = ruleName.Replace('-', ' ');
            return clause.Length == 0 ? clause : char.ToUpperInvariant(clause[0]) + clause.Substring(1);
        }

        /// <summary>
        /// <paramref name="before"/> turning into <paramref name="after"/>, said in a way that is
        /// true even when the two print the same.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Two different trees can print identically, and a derivation full of "X becomes X" is
        /// what that produces if nobody looks.</b> <c>1 + (x + y)</c> and <c>(1 + x) + y</c> are
        /// not the same tree and print the same way, so a normalisation that regroups a chain has a
        /// real before and after and nothing to show for it. It happened on two of the first six
        /// derivations this was tried on.
        /// </para>
        /// <para>
        /// Saying so is better than hiding it. A reader who sees a step reshaping a chain without
        /// the printed form moving has learned something true about the engine; a reader who sees
        /// <c>a becomes a</c> has learned to distrust the feature.
        /// </para>
        /// </remarks>
        internal static string Transition(Entity before, Entity after)
        {
            var written = before.Stringize();
            var becomes = after.Stringize();
            return written == becomes
                ? $"{written} is reshaped into an equal tree that prints the same way"
                : $"{written} becomes {becomes}";
        }

        /// <summary>
        /// One rewrite as a sentence: why it was allowed, then what it did.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>The reason comes first.</b> A reader following a derivation already knows the
        /// expression — it is the answer to the step above — and what they are looking for is the
        /// identity that moved it. Leading with the rewrite and appending the reason makes the
        /// reader hold a transformation in mind until they are told why it happened.
        /// </para>
        /// <para>
        /// Three shapes, and which one is used is a fact about what the rule carries rather than a
        /// choice: a rule with a name in prose is said in its own words, with its identity in
        /// brackets where it has one; a rule named by its rendered pattern is said by its identity
        /// instead; and a rewrite with neither is <i>attributed</i> to its set rather than dressed
        /// up as a reason it does not have.
        /// </para>
        /// </remarks>
        internal static string Sentence(RewriteRuleSet set, RewriteRule? rule, Entity before, Entity after)
        {
            var transition = Transition(before, after);
            if (rule is not null && IsProse(rule.Name))
            {
                var text = new StringBuilder(AsSentenceOpening(rule.Name));
                if (rule.Description is { } identity)
                    text.Append(" (").Append(identity).Append(')');
                return text.Append(", so ").Append(transition).Append('.').ToString();
            }
            if (rule?.Description is { } stated)
                return $"By {stated}, {transition}.";
            return $"{transition}, by {set.Name}.";
        }

        /// <summary>
        /// What a run of steps is justified by, as a sentence — or <see langword="null"/> where
        /// nothing is known about any of them.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Not repeated on every line, and that is the point of it being here.</b> A tier
        /// printed against each step is noise on the steps that share it, and a reader stops seeing
        /// it — which is the failure mode of a caveat, not the discharge of one. Said once over the
        /// whole path, with a count, it is information.
        /// </para>
        /// <para>
        /// <b>The count is over the steps that have a tier, and it says so.</b> A derivation's
        /// steps are not all rule sets — inner simplification, factoring and polynomial
        /// rearrangement are steps and declare nothing — so a path of four steps may have one tier
        /// between them. Reporting that as "the step holds under assumptions" reads as though there
        /// had been one step, which was the first version of this and was wrong twice over: it hid
        /// three steps and it implied the other three were justified.
        /// </para>
        /// </remarks>
        internal static string? TierNote(IReadOnlyList<Soundness> tiers, int steps)
        {
            if (tiers.Count == 0) return null;
            var conditional = 0;
            foreach (var tier in tiers)
                if (tier is not Soundness.Sound)
                    conditional++;

            // How the tiered steps are referred to, which depends on whether they are all of them.
            var subject = tiers.Count == steps
                ? (steps == 1 ? "The one step" : $"All {steps} steps")
                : (tiers.Count == 1
                    ? $"One of the {steps} steps is a registered rule set, and it"
                    : $"{tiers.Count} of the {steps} steps are registered rule sets, and they");

            if (conditional == 0)
                return $"{subject} hold{(tiers.Count == 1 ? "s" : "")} for every value, with nothing assumed.";
            if (conditional == tiers.Count)
                return $"{subject} hold{(tiers.Count == 1 ? "s" : "")} under assumptions rather than universally.";
            return $"{subject} carry a tier: {conditional} hold under assumptions rather than "
                + "universally, and the rest hold for every value.";
        }
    }
}
