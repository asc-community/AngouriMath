//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Collections.Generic;
using System.Linq;
using AngouriMath.Core.Budgets;
using AngouriMath.Core.Transformations.Matching;

namespace AngouriMath.Core.Transformations
{
    /// <summary>
    /// Runs rules over an e-graph until nothing more merges or the budget runs out — the shared
    /// half of every transformation built on the rewrite graph.
    /// </summary>
    /// <remarks>
    /// Shared rather than written once per caller because the loop is subtle in ways that are not
    /// visible from a second copy of it: what is charged and when, which rules can be skipped
    /// without an attempt, and which of two matching paths a rule takes. Two transformations
    /// already want it — one extracting the cheapest member of the root class and one extracting
    /// the least — and they differ only in that extraction, not in any of this.
    /// </remarks>
    internal static class Saturation
    {
        /// <summary>
        /// The rules of <see cref="MatchedRules"/> whose justification is at least
        /// <see cref="Soundness.SoundUnderAssumptions"/> and whose
        /// <see cref="MatchedRule.Growth"/> is no wider than <paramref name="widest"/>.
        /// </summary>
        /// <param name="widest">
        /// The widest growth admitted, as a ceiling in the order
        /// <see cref="RewriteRuleGrowth.Collects"/>, <see cref="RewriteRuleGrowth.Rearranges"/>,
        /// <see cref="RewriteRuleGrowth.Expands"/>, <see cref="RewriteRuleGrowth.Unknown"/> —
        /// increasing risk, and the order the values are declared in.
        /// </param>
        /// <remarks>
        /// <para>
        /// <b><see cref="RewriteRuleGrowth.Unknown"/> is the last stop rather than an excluded
        /// one, and that is a measurement rather than a preference.</b> It reads like the value a
        /// ceiling should refuse — it means the rule's growth was not judged, because its
        /// replacement is code rather than a written pattern, so admitting it accepts a rewrite
        /// nobody measured. But it is where the rules are: of the sound rules in
        /// <see cref="MatchedRules"/>, <b>26 collect, 17 rearrange, 9 expand and 270 are
        /// unjudged</b>. A ceiling that refuses the fourth value refuses 84% of the library, and
        /// what is left cannot do much — over six expression pairs equal only through a larger
        /// intermediate form, the <see cref="RewriteRuleGrowth.Rearranges"/> ceiling proved two,
        /// and <see cref="RewriteRuleGrowth.Expands"/> — all nine expanding rules added — proved
        /// <i>the same two</i>. The third was proved only with the unjudged rules admitted.
        /// </para>
        /// <para>
        /// So the dial that matters is not how far a rule may enlarge; it is whether rules nobody
        /// classified may fire at all. Naming that as the widest setting makes it a request a
        /// caller has to make on purpose, which is the most this can honestly do about it.
        /// </para>
        /// </remarks>
        internal static IReadOnlyList<MatchedRule> RulesUpTo(RewriteRuleGrowth widest)
            => MatchedRules.All
                .SelectMany(set => set.Rules)
                .Where(rule => rule.Soundness is Soundness.Sound or Soundness.SoundUnderAssumptions)
                .Where(rule => rule.Growth <= widest)
                .ToList();

        /// <summary>
        /// Merges into <paramref name="graph"/> every equality <paramref name="rules"/> reach from
        /// what it already holds, until a pass changes nothing or <paramref name="ledger"/> stops
        /// it. Answers whether it reached that fixed point rather than the ceiling.
        /// </summary>
        /// <param name="graph">The graph to saturate, in place.</param>
        /// <param name="rules">Which rules to fire; see <see cref="RulesUpTo"/>.</param>
        /// <param name="ledger">What bounds the work, and what records how much was done.</param>
        /// <param name="witnessCost">
        /// How a representative is chosen when one is needed — for a rule whose left-hand side
        /// cannot e-match, which has to be shown a term, and for a rule's <c>when</c> predicate,
        /// which is arbitrary code that has to be asked about something. It ranks a
        /// <i>witness</i>, never the answer: what the caller finally extracts is the caller's own
        /// business, and is where a cheapest-form and a canonical-form caller differ.
        /// </param>
        internal static bool Run(EGraph graph, IReadOnlyList<MatchedRule> rules,
            BudgetLedger ledger, Func<Entity, double> witnessCost)
        {
            var chargedNodes = graph.NodeCount;
            bool ChargeGrowthSinceLastCall()
            {
                var delta = graph.NodeCount - chargedNodes;
                chargedNodes = graph.NodeCount;
                return ledger.Spend(delta);
            }

            var saturated = false;
            while (!saturated && !ledger.Exhausted)
            {
                var merged = false;
                foreach (var id in graph.Classes.ToList())
                {
                    if (ledger.Exhausted) break;

                    // Which node types this class holds, gathered once for the whole sweep rather
                    // than once per rule. A pattern that requires a root type cannot match a class
                    // holding no node of it -- RequiredRootType is a necessary condition, not a
                    // sufficient one, which is the direction that makes it a filter: it licenses
                    // skipping a rule, never firing one.
                    //
                    // A union later in this same sweep can add a node type this set does not have,
                    // so a rule can be skipped in a pass where it had just become applicable. That
                    // costs nothing: a union is exactly what sets `merged`, so there is another
                    // pass, and the set is gathered again there.
                    var held = new HashSet<Type>();
                    foreach (var node in graph.NodesOf(id)) held.Add(EGraph.RuntimeType(node));

                    Entity? term = null;
                    var extracted = false;
                    bool TryTerm(out Entity value)
                    {
                        if (!extracted)
                        {
                            term = graph.Extract(id, witnessCost);
                            extracted = true;
                        }
                        value = term!;
                        return term is not null;
                    }

                    foreach (var rule in rules)
                    {
                        // Before the charge, because a rule this skips was never attempted: a step
                        // is a match attempt, and a type that cannot match is not one. Written as a
                        // loop rather than as `held.Any(t => ...)`: the lambda would capture
                        // `required` and so allocate a closure every time the exact-type test
                        // missed, which is most rules of most classes -- and paying an allocation
                        // to avoid a pattern match is not a pre-filter.
                        if (rule.Left.RequiredRootType is { } required && !held.Contains(required))
                        {
                            var reachable = false;
                            foreach (var type in held)
                                if (required.IsAssignableFrom(type))
                                {
                                    reachable = true;
                                    break;
                                }
                            if (!reachable) continue;
                        }

                        // One step per match attempt, charged before the attempt -- the unit
                        // Buchberger, FGLM and MatchPattern all charge, and the work this loop
                        // actually does. Charging the node-count delta alone (below) billed
                        // nothing on ordinary input, because the rules that do not expand rarely
                        // grow the graph at all: a budget of zero steps ran this sweep to
                        // saturation and then reported that it had completed.
                        if (!ledger.Spend()) break;
                        int other;
                        if (rule.Left.CanEMatch)
                        {
                            // Mirrors the fallback branch below: a rule's `when` is arbitrary code
                            // asked about a witness this extracted rather than one the caller
                            // wrote, and a predicate that throws on a shape it did not expect must
                            // decline the candidate, not escape Apply.
                            bool matched;
                            try { matched = rule.TryEMatchApply(graph, id, witnessCost, out other); }
                            catch { continue; }
                            if (!matched) continue;
                        }
                        else
                        {
                            if (!TryTerm(out var t)) continue;
                            Entity? rewritten;
                            try { rewritten = rule.TryApply(t); }
                            catch { continue; }
                            if (rewritten is null || rewritten.Equals(t)) continue;
                            try { other = graph.AddEntity(rewritten); }
                            catch { continue; }
                        }
                        if (graph.Union(id, other)) merged = true;
                    }

                    // What the sweep grew, charged after it rather than before the next class's,
                    // so that the ceiling bounds the growth that has happened.
                    if (!ChargeGrowthSinceLastCall()) break;
                }

                // Rebuild rescans every node of every class, repeatedly until nothing more becomes
                // congruent -- a round of it is work, and it had no ledger interaction anywhere.
                if (!ledger.Spend()) break;
                graph.Rebuild();
                if (!merged) saturated = true;
            }
            return saturated;
        }

        /// <summary>
        /// Whether <paramref name="rules"/> prove <paramref name="left"/> and
        /// <paramref name="right"/> equal, within <paramref name="budget"/>. A
        /// <see langword="false"/> means <i>not proved</i>, never <i>unequal</i>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>This is the reliable half, and canonicalising is not.</b> Bringing two expressions
        /// separately to a canonical form and comparing the results only decides equality where
        /// the rules are <i>confluent</i>: otherwise one side can reach a form the other cannot,
        /// and two equal expressions end in different forms having each been canonicalised
        /// correctly. Measured, on <c>(x + y) * (x - y)</c> against <c>x ^ 2 - y ^ 2</c>: the
        /// product's graph reaches the difference of squares, the difference of squares' graph
        /// does not reach the product, and the two forms are the same size — so each canonicalises
        /// to itself.
        /// </para>
        /// <para>
        /// Asking in one graph has no such gap. Both expressions are inserted, every equality the
        /// rules reach from <i>either</i> is merged, and the question is whether they ended in one
        /// class — which does not depend on the rules being confluent, only on their reaching far
        /// enough within the budget.
        /// </para>
        /// </remarks>
        internal static bool ProvesEqual(Entity left, Entity right,
            IReadOnlyList<MatchedRule> rules, WorkBudget budget)
        {
            if (left is null) throw new ArgumentNullException(nameof(left));
            if (right is null) throw new ArgumentNullException(nameof(right));
            if (left.Equals(right)) return true;

            var graph = new EGraph();
            int a, b;
            try
            {
                a = graph.AddEntity(left);
                b = graph.AddEntity(right);
            }
            // A node the graph cannot hold is not a proof of anything, and not an error either.
            catch (Exception) { return false; }
            graph.Rebuild();

            var ledger = BudgetLedger.For(nameof(ProvesEqual), budget);
            Run(graph, rules, ledger, CostModel.Default.Cost);
            ledger.Report();
            return graph.Find(a) == graph.Find(b);
        }
    }
}
