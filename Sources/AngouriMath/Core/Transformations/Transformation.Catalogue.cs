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
using AngouriMath.Functions;
using AngouriMath.Functions.Algebra;
using static AngouriMath.Entity;

namespace AngouriMath.Core.Transformations
{
    partial class Transformation
    {
        /// <summary>
        /// Applies a rewrite rule set once over every node.
        /// </summary>
        /// <param name="ruleSet">The set to apply; see <see cref="RewriteRules"/>.</param>
        public static Transformation Rewriting(RewriteRuleSet ruleSet)
            => (ruleSet ?? throw new ArgumentNullException(nameof(ruleSet))).AsTransformation();

        /// <summary>
        /// One structural tidying pass — <see cref="Entity.InnerSimplified"/>. Cheap, and
        /// no pattern search.
        /// </summary>
        public static Transformation InnerSimplification { get; } = new InnerSimplificationTransformation();

        /// <summary>
        /// The full simplification pipeline at the default level, as
        /// <see cref="Entity.Simplify(int)"/> runs it.
        /// </summary>
        public static Transformation Simplification { get; } = SimplificationAtLevel(2);

        /// <summary>
        /// The full simplification pipeline at a chosen level.
        /// </summary>
        /// <param name="level">
        /// How hard to look; the same argument <see cref="Entity.Simplify(int)"/> takes.
        /// </param>
        public static Transformation SimplificationAtLevel(int level)
            => LevelledCache.Simplification.For(level, static l => new SimplificationTransformation(l, null));

        /// <summary>
        /// The full simplification pipeline at a chosen level, rating candidates by
        /// <paramref name="costModel"/> instead of <see cref="MathS.Settings.ComplexityCriteria"/>.
        /// </summary>
        /// <param name="level">
        /// How hard to look; the same argument <see cref="Entity.Simplify(int)"/> takes.
        /// </param>
        /// <param name="costModel">
        /// Which candidate counts as cheapest. <see cref="Core.CostModel.Default"/> here behaves
        /// exactly like <see cref="SimplificationAtLevel(int)"/> — passing it is how a caller
        /// states that on purpose rather than by leaving a parameter out.
        /// </param>
        /// <remarks>
        /// <a href="https://github.com/asc-community/AngouriMath/issues/746">#746</a> tier 2 named
        /// this remaining on its own row: "a cost model that reaches an API rather than an ambient
        /// setting". <see cref="MathS.Settings.ComplexityCriteria"/> already <i>is</i> an API in
        /// the sense that a caller can scope it explicitly and safely — it is backed by the same
        /// <see cref="Convenience.Setting{T}"/> <see cref="MathS.Settings.Budget"/> uses, async-local rather
        /// than thread-static, so one caller's override cannot leak into another's concurrent call.
        /// What it lacked was a place in <em>this</em> API, the addressable one <see cref="Transformation"/>
        /// is: composing <c>SimplificationAtLevel(2, costModel).Then(...)</c> names the choice
        /// where setting an ambient value around a call does not. This overload does not
        /// reimplement candidate search against an explicit parameter threaded through
        /// <c>Simplificator</c> -- that would be a second pipeline to keep in step with the one
        /// <see cref="Entity.Simplify(int)"/> actually runs. It scopes the existing, already-tested
        /// setting for the duration of this one call instead of introducing a second pipeline.
        /// </remarks>
        public static Transformation SimplificationAtLevel(int level, CostModel costModel)
            => new SimplificationTransformation(
                level, costModel ?? throw new ArgumentNullException(nameof(costModel)));

        /// <summary>
        /// Multiplies products over sums out, as <see cref="Entity.Expand(int)"/> does.
        /// </summary>
        public static Transformation Expansion { get; } = ExpansionAtLevel(2);

        /// <summary>
        /// Multiplies products over sums out, for a chosen number of passes.
        /// </summary>
        /// <param name="level">The number of passes; the argument <see cref="Entity.Expand(int)"/> takes.</param>
        public static Transformation ExpansionAtLevel(int level)
            => LevelledCache.Expansion.For(level, static l => new ExpansionTransformation(l));

        /// <summary>
        /// Gathers common factors back out, as <see cref="Entity.Factorize(int)"/> does.
        /// </summary>
        public static Transformation Factorization { get; } = FactorizationAtLevel(2);

        /// <summary>
        /// Gathers common factors back out, for a chosen number of passes.
        /// </summary>
        /// <param name="level">The number of passes; the argument <see cref="Entity.Factorize(int)"/> takes.</param>
        /// <remarks>
        /// Built out of the registry rather than written again: one pass is the perfect
        /// square rules, then the factorisation rules, then a tidying pass, and the level is
        /// how many times that runs.
        /// </remarks>
        public static Transformation FactorizationAtLevel(int level)
            => LevelledCache.Factorization.For(level, static l =>
                RuleBasedFactorizationAtLevel(l)
                    // And then the polynomial layer, which factors what no rule set has a rule
                    // for. Last, so that the rules keep every answer they already gave and this
                    // only ever adds one. See PolynomialFactorization.
                    .Then(PolynomialFactorization));

        /// <summary>
        /// The rule-based half of <see cref="FactorizationAtLevel"/>, without the polynomial
        /// layer.
        /// </summary>
        /// <remarks>
        /// Separate because <c>Simplify</c> offers a factorisation as a <b>candidate</b> and its
        /// cost model decides. The metric prefers the expanded form — <c>x ^ 6 - 1</c> rates 12
        /// expanded against 58 factored — so a factored candidate wins only where the two are
        /// closest, and those turn out to be the places a factored answer is least wanted:
        /// <c>x ^ 3 / 3 + x ^ 2 / 2</c> becomes <c>(3 + 2 * x) * x ^ 2 / 6</c>, an antiderivative
        /// in a form nobody writes. Offering the layer to that search is
        /// <a href="https://github.com/asc-community/AngouriMath/issues/746">#746</a> tier 2's
        /// pluggable cost model rather than
        /// <a href="https://github.com/asc-community/AngouriMath/issues/1018">#1018</a>.
        /// </remarks>
        public static Transformation RuleBasedFactorizationAtLevel(int level)
            => LevelledCache.RuleBasedFactorization.For(level, static l =>
                Rewriting(RewriteRules.PerfectSquare)
                    .Then(Rewriting(RewriteRules.Factorization))
                    .Then(InnerSimplification)
                    // Entity.Factorize has always run at least one pass, whatever it was asked for.
                    .Repeat(Math.Max(l, 1)));

        /// <summary>
        /// Factors a polynomial by the polynomial layer — square-free decomposition, Zassenhaus
        /// over <c>Q</c>, Kronecker's substitution and Hensel lifting — rather than by a rule.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A rule set factors what someone wrote a rule for. <c>x ^ 2 - 1</c> has one and
        /// <c>x ^ 3 - 1</c> does not, which is why <see cref="Entity.Factorize(int)"/> — the
        /// operation whose entire job is factorisation — was worse at it than the machinery that
        /// exists for it. <a href="https://github.com/asc-community/AngouriMath/issues/1018">#1018</a>
        /// </para>
        /// <para>
        /// <b>This is not the cost-model question.</b> The reason the polynomial layer is not
        /// wired into <c>Simplify</c> is that <c>SimplifiedRate</c> prefers the expanded
        /// form — <c>x ^ 6 - 1</c> rates 12 expanded against 58 factored — so a factored candidate
        /// could never win a search. There is no search here: <c>Factorize</c> is asked for the
        /// factored form and returns it.
        /// </para>
        /// <para>
        /// <b>Which variable.</b> The layer factors with respect to one, and an expression has
        /// several. Each of its variables is tried in turn and the first that yields a genuine
        /// product wins, which is deterministic because <see cref="Entity.Vars"/> is. Trying them
        /// all rather than guessing a main one is what makes <c>x ^ 4 - y ^ 4</c> come out whole
        /// however the caller wrote it.
        /// </para>
        /// <para>
        /// <b>What it declines.</b> Anything that is not a polynomial, anything the layer refuses,
        /// and anything whose answer is not a product — a refusal leaves the expression exactly as
        /// the rules left it, so nothing that factored before can stop factoring.
        /// </para>
        /// </remarks>
        /// <remarks>
        /// Held in a nested class rather than in a static field of this one. Static field
        /// initialisers run in declaration order, and <see cref="Factorization"/> — declared
        /// above — is eager, so a field here would still be <see langword="null"/> when its
        /// pipeline is built. That surfaces as <c>ArgumentNullException(nameof(next))</c> from a
        /// combinator rather than as anything naming the real cause, and this file has had that
        /// failure before. A nested type is initialised on first touch, whatever order this
        /// one's members are written in.
        /// </remarks>
        public static Transformation PolynomialFactorization => PolynomialFactorizationHolder.Instance;

        private static class PolynomialFactorizationHolder
        {
            [ConstantField]
            internal static readonly Transformation Instance = new PolynomialFactorizationTransformation();
        }

        /// <summary>
        /// Puts commutative chains into a canonical order, so that expressions which differ
        /// only in the arrangement of their operands become the same tree.
        /// </summary>
        /// <remarks>
        /// Not a simplification: it makes an expression comparable, not shorter. It has
        /// always been available inside <c>Simplify</c> and had no name of its own until
        /// this layer gave the rule set one.
        /// </remarks>
        public static Transformation Normalization { get; }
            = Rewriting(RewriteRules.CanonicalOrder).Then(InnerSimplification);

        /// <summary>
        /// A canonical form for <b>rational functions over <c>Q</c></b>: two expressions
        /// denoting the same quotient of polynomials become the identical tree, so equality on
        /// that sublanguage is decided by comparing nodes rather than by searching.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>It answers only where it can.</b> There is no canonical form for the whole
        /// language — zero-equivalence is undecidable once <c>pi</c>, the exponential, the
        /// trigonometric functions and <c>abs</c> are in play (Richardson, 1968) — so anything
        /// that is not a rational function over <c>Q</c> in its free variables gets no answer
        /// at all. That refusal is the point: a form whose value is that equal trees mean equal
        /// expressions must not quietly hand back a normalisation that merely resembles one.
        /// </para>
        /// <para>
        /// The expression is gathered into a single quotient — which is the part nothing else
        /// in the library does, and without which <c>1/x + 1/y</c> and <c>(x + y)/(x*y)</c>
        /// could never meet — then reduced by the multivariate greatest common divisor and
        /// scaled so the denominator is monic in the lexicographic monomial order.
        /// </para>
        /// <para>
        /// <b>Cancelling carries its condition.</b> <c>x/x</c> is not <c>1</c>, so where a
        /// factor of positive degree comes out the answer says the factor is nonzero, as the
        /// rest of the library already does. Gathering over a common denominator widens
        /// nothing by itself: a sum is defined exactly where its terms are.
        /// </para>
        /// <para>
        /// Nothing runs this by default. See
        /// <c>Docs/Contributing/CanonicalForm.md</c> §5 and
        /// <a href="https://github.com/asc-community/AngouriMath/issues/934">#934</a>.
        /// <see cref="Canonicalization"/> is the companion that handles the commutative
        /// structure of expressions generally.
        /// </para>
        /// </remarks>
        public static Transformation RationalCanonicalization { get; }
            = new RationalCanonicalizationTransformation();

        /// <summary>
        /// A canonical form for the commutative structure: two expressions differing only in
        /// how their sums, products, conjunctions, disjunctions and set operations are
        /// arranged or nested come out as the identical tree.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Normalise, order, normalise</b>, and the first of those is what makes it work.
        /// The order's key depends on a node's class and the normalisation changes classes:
        /// in <c>1/2 - x</c> the constant reaches an unprepared sort as <c>1 * 2 ^ (-1)</c>,
        /// a product, and is ordered against <c>-x</c> as one, but folds to the number
        /// <c>1/2</c> immediately afterwards — so the next pass orders it the other way and
        /// the two alternate. Sorting a tree that has already settled sorts what the tree is
        /// actually going to be.
        /// </para>
        /// <para>
        /// Measured over 834 generated expressions and 2738 ordered pairs by
        /// <c>work/canoncheck</c>: <b>idempotent, and independent of the order the operands
        /// were written in</b>, where ordering without the leading normalisation fails 21 of
        /// the first and <see cref="InnerSimplification"/> alone fails 2024 of the second.
        /// </para>
        /// <para>
        /// <b>What it is not.</b> It is not a canonical form for the language — no such thing
        /// exists, since zero-equivalence is undecidable here — and it is not a
        /// simplification, since it makes an expression comparable rather than shorter.
        /// Equal trees mean the expressions are equal; different trees mean nothing at all.
        /// <c>Docs/Contributing/CanonicalForm.md</c> states the boundary and what is still owed.
        /// </para>
        /// <para>
        /// <b>Nesting goes with order.</b> The sort works over commutative chains rather than
        /// over one node, so it flattens as it sorts: <c>(x + y) + a</c> and <c>x + (y + a)</c>
        /// both reach <c>a + x + y</c>, and reach it as the same tree. That is worth saying
        /// because <see cref="InnerSimplification"/> alone leaves those two as different trees
        /// which <i>print identically</i> — associativity being normalised on the way out
        /// rather than in the expression — so a comparison of printed forms cannot tell the
        /// two situations apart.
        /// </para>
        /// <para>
        /// Nothing in the library runs this: it is offered, not applied. Putting it inside
        /// <see cref="InnerSimplification"/> would move every commutative operand order in
        /// every printed answer at once, which is a decision for a release rather than for a
        /// transformation. <a href="https://github.com/asc-community/AngouriMath/issues/746">#746</a>
        /// tier 1.
        /// </para>
        /// </remarks>
        public static Transformation Canonicalization { get; }
            = InnerSimplification
                .Then(Rewriting(RewriteRules.CanonicalOrderExact))
                .Then(InnerSimplification);

        /// <summary>
        /// Clears a surd out of a two-term denominator: <c>1 / (sqrt(3) + 5)</c> becomes
        /// <c>(sqrt(3) - 5) / (-22)</c>.
        /// </summary>
        public static Transformation Rationalization { get; }
            = Rewriting(RewriteRules.RationalizeDenominator).Then(InnerSimplification);

        /// <summary>
        /// Explores the equalities the whole registry's rules reach from an expression at once,
        /// over an e-graph, and extracts the cheapest under <paramref name="costModel"/>.
        /// </summary>
        /// <param name="budget">
        /// What this call may spend before it settles for the best it has found so far.
        /// <see cref="WorkBudget.Steps"/> is charged once per e-node the graph actually creates;
        /// <see cref="WorkBudget.Time"/> is the wall-clock backstop. A caller who sets
        /// <see cref="MathS.Settings.Budget"/> overrides this, the same as every other bounded
        /// computation in the library.
        /// </param>
        /// <param name="costModel">Which candidate counts as cheapest once exploration stops.</param>
        /// <remarks>
        /// <para>
        /// <b>Nothing runs this by default</b> — the same standing as
        /// <see cref="RationalCanonicalization"/> and <see cref="Canonicalization"/>, and for a
        /// sharper reason: <c>Simplify</c> applies a rule set once, keeps a candidate and moves on,
        /// so an expanding rule and a collecting one never meet — the order they run in decides
        /// which wins. Equality saturation deletes that order and keeps every result, which is
        /// why it needs a budget rather than a pass count, and why only the rules a scheduler can
        /// prove will not run away are offered to it — see the next paragraph.
        /// </para>
        /// <para>
        /// <b>Only rules whose <see cref="Matching.MatchedRule.Growth"/> is exactly
        /// <see cref="RewriteRuleGrowth.Collects"/> or <see cref="RewriteRuleGrowth.Rearranges"/>,
        /// and whose <see cref="Matching.MatchedRule.Soundness"/> is at least
        /// <see cref="Soundness.SoundUnderAssumptions"/>, are used</b> — the population is
        /// <see cref="Matching.MatchedRules.All"/>, not the public registry. Growth is derived
        /// from the pattern tree for a rule whose replacement is itself a pattern, and declared
        /// explicitly by the rule's own author for a rule whose replacement is code, which is what
        /// let a further batch of code-built rules earn a place here without being able to lie
        /// about it. <see cref="RewriteRuleGrowth.Unknown"/> is withheld either way: a rule nobody
        /// has justified is not proven safe, and not proven safe is not the same as safe. This is
        /// <a href="https://github.com/asc-community/AngouriMath/issues/746">#746</a> tier 2's
        /// e-graph. As of this writing the filter passes 43 rules.
        /// </para>
        /// <para>
        /// <b>Real e-matching now runs wherever a rule's pattern supports it, and this only falls
        /// back to materialising a term where it cannot.</b> A rule's
        /// <see cref="Matching.MatchPattern.CanEMatch"/> on its <see cref="Matching.MatchedRule.Left"/>
        /// decides per rule: where it is true this asks the e-graph directly, which is what a
        /// production e-matcher over <see cref="Matching.MatchPattern"/> is supposed to do; where
        /// it is false this falls back to extracting a term and rewriting that instead — slower,
        /// and the only path the harness this is built from ever took. Of the 43 rules the filter
        /// above passes, roughly 27-28 can actually build a replacement today; the remaining ~15
        /// each build a boolean connective or a turned-around equality (<c>and</c>, <c>or</c>,
        /// <c>not</c>, <c>xor</c>, <c>implies</c>, <c>=</c>) and are correctly classified as safe,
        /// but cannot fire, because <see cref="EGraph"/>'s reconstruction whitelist has no entry
        /// for any of those node types yet and <c>EGraph.Extract</c> returns nothing for a
        /// class that needs one. That is a separate, already-known limitation of
        /// <see cref="EGraph"/> itself — see <c>Docs/Contributing/EqualitySaturationReviewFindings.md</c>
        /// — not a defect in how these 15 rules were classified. The day the whitelist widens to
        /// cover those six node types, all 15 go live at once, which is the moment to re-run
        /// <c>work/egraph</c>, not before.
        /// </para>
        /// <para>
        /// <b>What generalises and what does not.</b> The <c>work/egraph</c> harness's original
        /// measurement — a textbook corpus of 16 expressions, all of which saturated — was made
        /// under a much <i>larger</i> rule set (313 rules, off the public registry's string-length
        /// <see cref="RewriteRuleGrowth"/> proxy), before the <see cref="Soundness"/> filter existed
        /// and before any rule here could e-match at all, so it does not describe this population —
        /// a different source, a different filter, a different matcher — and should not be cited as
        /// though it still does without being re-run. Even re-run, a corpus
        /// saturating says the graph stopped growing on those inputs; it did not and could not say
        /// that of every expression <c>Simplify</c> is asked to handle. Pass a budget that reflects
        /// that this is still being found out, not one sized for how much the caller can afford to
        /// lose.
        /// </para>
        /// </remarks>
        public static Transformation EqualitySaturation(WorkBudget budget, CostModel costModel)
            => new EqualitySaturationTransformation(
                budget ?? throw new ArgumentNullException(nameof(budget)),
                costModel ?? throw new ArgumentNullException(nameof(costModel)));

        /// <summary>
        /// Test-only visibility into how many rules <see cref="EqualitySaturation"/> currently
        /// draws from -- the field itself is <see langword="private"/> on a
        /// <see langword="private"/> nested class, which a test in another assembly cannot reach
        /// any other way. Exists so a collapse back toward the old
        /// all-<see cref="RewriteRuleGrowth.Unknown"/> state — the rule population silently
        /// shrinking to nothing, the way it once did before this was measured — fails a test
        /// instead of going unmeasured again.
        /// </summary>
        internal static int EqualitySaturationSafeRuleCount => EqualitySaturationTransformation.SafeRuleCount;

        /// <summary>
        /// Replaces every occurrence of <paramref name="what"/> with
        /// <paramref name="with"/>, as <see cref="Entity.Substitute(Entity, Entity)"/> does.
        /// </summary>
        /// <param name="what">The subexpression to replace.</param>
        /// <param name="with">What to put in its place.</param>
        public static Transformation Substitution(Entity what, Entity with)
            => new SubstitutionTransformation(
                what ?? throw new ArgumentNullException(nameof(what)),
                with ?? throw new ArgumentNullException(nameof(with)));

        /// <summary>
        /// The symbolic derivative over <paramref name="variable"/>, as
        /// <see cref="Entity.Differentiate(Variable)"/> computes it.
        /// </summary>
        /// <param name="variable">The variable to differentiate over.</param>
        public static Transformation Differentiation(Variable variable)
            => new DifferentiationTransformation(variable ?? throw new ArgumentNullException(nameof(variable)));

        /// <summary>
        /// The antiderivative over <paramref name="variable"/>, <b>without</b> the constant
        /// of integration — <see cref="Entity.Integrate(Variable)"/> is what adds it.
        /// </summary>
        /// <param name="variable">The variable to integrate over.</param>
        /// <remarks>
        /// Where no antiderivative is found this says so, by having no answer.
        /// <see cref="Entity.Integrate(Variable)"/> hands back an unevaluated
        /// <see cref="Entity.Integralf"/> instead, which is the same claim in the shape 1.x
        /// callers expect.
        /// </remarks>
        public static Transformation Integration(Variable variable)
            => new IntegrationTransformation(variable ?? throw new ArgumentNullException(nameof(variable)));

        /// <summary>
        /// The limit over <paramref name="variable"/> as it approaches
        /// <paramref name="destination"/> from <paramref name="side"/>.
        /// </summary>
        /// <param name="variable">The variable that approaches.</param>
        /// <param name="destination">Where it approaches.</param>
        /// <param name="side">From which side.</param>
        /// <remarks>
        /// The limit as computed, not further simplified — <see cref="Entity.Limit(Variable, Entity, ApproachFrom)"/>
        /// is what tidies it. Where the limit cannot be settled this has no answer, rather
        /// than the unevaluated <see cref="Entity.Limitf"/> node the 1.x method returns;
        /// neither is a claim that the limit does not exist.
        /// </remarks>
        public static Transformation LimitAt(Variable variable, Entity destination, ApproachFrom side)
            => new LimitTransformation(
                variable ?? throw new ArgumentNullException(nameof(variable)),
                destination ?? throw new ArgumentNullException(nameof(destination)),
                side);

        /// <summary>
        /// The transformations that differ only by a level, each built at most once, so that
        /// an ordinary <c>Simplify()</c> allocates nothing to reach its transformation.
        /// </summary>
        /// <remarks>
        /// Filled on demand rather than in the static initialiser, and it has to be. Building
        /// the factorisation entry reads <see cref="RewriteRules"/>, whose sets in turn build
        /// transformations of their own; doing that while this type is still initialising
        /// makes the two types depend on each other's initialisation, and one of them then
        /// reads the other's fields before they are set. Two threads arriving together may
        /// each build the same entry; the entries are equivalent and immutable, so whichever
        /// reference lands is the one everyone then uses.
        /// </remarks>
        private sealed class LevelledCache
        {
            // The levels Simplify, Expand and Factorize are actually asked for: the default
            // 2, the negated -level Simplify passes itself, and a little room either side.
            private const int Lowest = -4;
            private const int Highest = 4;

            [ConcurrentField]
            internal static readonly LevelledCache Simplification = new();
            [ConcurrentField]
            internal static readonly LevelledCache Expansion = new();
            [ConcurrentField]
            internal static readonly LevelledCache Factorization = new();

            [ConstantField]
            internal static readonly LevelledCache RuleBasedFactorization = new();

            private readonly Transformation?[] cached = new Transformation?[Highest - Lowest + 1];

            internal Transformation For(int level, Func<int, Transformation> make)
                => level < Lowest || level > Highest
                    ? make(level)
                    : cached[level - Lowest] ??= make(level);
        }

        private sealed class EqualitySaturationTransformation : Transformation
        {
            /// <summary>
            /// Every rule in <see cref="Matching.MatchedRules.All"/> whose
            /// <see cref="Matching.MatchedRule.Growth"/> is known not to expand and whose
            /// <see cref="Matching.MatchedRule.Soundness"/> is at least <see cref="Soundness.SoundUnderAssumptions"/>
            /// -- the real pattern-tree classification (Task 4), not the public registry's
            /// string-length proxy, and a per-rule <see cref="Soundness"/> check the previous,
            /// public-surface-sourced version of this field had no way to make (it filtered by
            /// Growth alone). Computed once: the registry does not change while the process runs.
            /// </summary>
            [ConstantField]
            private static readonly IReadOnlyList<Matching.MatchedRule> SafeRules
                = Matching.MatchedRules.All
                    .SelectMany(set => set.Rules)
                    .Where(rule => rule.Growth is RewriteRuleGrowth.Collects or RewriteRuleGrowth.Rearranges)
                    .Where(rule => rule.Soundness is Soundness.Sound or Soundness.SoundUnderAssumptions)
                    .ToList();

            /// <summary>
            /// <see cref="SafeRules"/>.Count, for <see cref="Transformation.EqualitySaturationSafeRuleCount"/>
            /// to forward -- this class is <see langword="private"/>, so even an
            /// <see langword="internal"/> member here is only visible within
            /// <see cref="Transformation"/>'s own body, never from another assembly.
            /// </summary>
            internal static int SafeRuleCount => SafeRules.Count;

            private readonly WorkBudget budget;
            private readonly CostModel costModel;

            internal EqualitySaturationTransformation(WorkBudget budget, CostModel costModel)
                => (this.budget, this.costModel) = (budget, costModel);

            public override string Name => $"equality-saturation[{costModel.Name}]";

            public override TransformationRelation Relation => TransformationRelation.Equivalence;

            // The rules this draws from are a mix of Sound and SoundUnderAssumptions, and a
            // rule set's own tier is already the minimum over what it contains -- so the
            // weakest tier represented is the honest claim for the whole of what this used,
            // the same convention every rule set in the registry already follows.
            public override Soundness Soundness => Soundness.SoundUnderAssumptions;

            protected override Entity? ApplyCore(Entity input)
            {
                var graph = new EGraph();
                var root = graph.AddEntity(input);
                graph.Rebuild();

                var ledger = BudgetLedger.For(Name, budget);
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
                        if (!ChargeGrowthSinceLastCall()) break;
                        Entity? term = null;
                        var extracted = false;
                        bool TryTerm(out Entity value)
                        {
                            if (!extracted)
                            {
                                term = graph.Extract(id, costModel.Cost);
                                extracted = true;
                            }
                            value = term!;
                            return term is not null;
                        }

                        foreach (var rule in SafeRules)
                        {
                            int other;
                            if (rule.Left.CanEMatch)
                            {
                                // Mirrors the fallback branch below: a rule's `when` is arbitrary
                                // code asked about a witness this extracted rather than one the
                                // caller wrote, and a predicate that throws on a shape it did not
                                // expect must decline the candidate, not escape Apply.
                                bool matched;
                                try { matched = rule.TryEMatchApply(graph, id, costModel.Cost, out other); }
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
                    }
                    graph.Rebuild();
                    if (!merged) saturated = true;
                }

                ledger.Report();
                return graph.Extract(root, costModel.Cost) ?? input;
            }
        }

        private sealed class RationalCanonicalizationTransformation : Transformation
        {
            public override string Name => "rational-canonical-form";

            public override TransformationRelation Relation => TransformationRelation.Equivalence;

            public override Soundness Soundness => Soundness.SoundUnderAssumptions;

            protected override Entity? ApplyCore(Entity input)
                => RationalFunction.TryCanonicalize(input, out var canonical) ? canonical : null;
        }

        private sealed class PolynomialFactorizationTransformation : Transformation
        {
            public override string Name => "polynomial-factorization";

            public override TransformationRelation Relation => TransformationRelation.Equivalence;

            public override Soundness Soundness => Soundness.Sound;

            // Total rather than declining, because a step that returns null makes the whole
            // chain decline -- `Then` has no notion of an optional part, and `Factorization` is
            // a chain. `InnerSimplification` is total for the same reason. So "nothing to
            // factor" is the input handed back, not a refusal.
            protected override Entity? ApplyCore(Entity input)
            {
                // A product is taken apart and each factor asked separately, rather than being
                // handed over whole. Replacing the rules' product would change answers that were
                // never the complaint -- the order two factors come out in is arbitrary and
                // theirs is the one on record -- but *declining* it leaves a real gap: the rules
                // take a numeric content out and hand back `2 * (x ^ 3 - 1)`, and the remainder
                // is exactly the shape #1018 is about. Asking each factor keeps every factor the
                // rules found and splits the ones they could not.
                if (input is Entity.Mulf)
                {
                    Entity? rebuilt = null;
                    var moved = false;
                    foreach (var factor in Entity.Mulf.LinearChildren(input))
                    {
                        var piece = Factored(factor) ?? factor;
                        moved |= !ReferenceEquals(piece, factor);
                        rebuilt = rebuilt is null ? piece : rebuilt * piece;
                    }
                    return moved && rebuilt is not null ? rebuilt : input;
                }
                return Factored(input) ?? input;
            }

            /// <summary>
            /// <paramref name="input"/> factored by the polynomial layer, or <see langword="null"/>
            /// where it does not factor.
            /// </summary>
            /// <remarks>
            /// Each of its variables is tried in turn and the first that yields a genuine product
            /// wins, which is deterministic because <see cref="Entity.Vars"/> is. Trying them all
            /// rather than guessing a main one is what makes <c>x ^ 4 - y ^ 4</c> come out whole
            /// however the caller wrote it.
            /// </remarks>
            private static Entity? Factored(Entity input)
            {
                if (input is Entity.Mulf or Entity.Powf)
                    return null;
                foreach (var variable in input.Vars)
                    if (MathS.Polynomials.Factor(input, variable) is { } factored
                        && factored is Entity.Mulf or Entity.Powf)
                        return factored;
                return null;
            }
        }

        private sealed class InnerSimplificationTransformation : Transformation
        {
            public override string Name => "inner-simplify";

            public override TransformationRelation Relation => TransformationRelation.Equivalence;

            public override Soundness Soundness => Soundness.SoundUnderAssumptions;

            protected override Entity? ApplyCore(Entity input) => input.InnerSimplified;
        }

        private sealed class SimplificationTransformation : Transformation
        {
            private readonly int level;
            private readonly CostModel? costModel;

            internal SimplificationTransformation(int level, CostModel? costModel)
                => (this.level, this.costModel) = (level, costModel);

            public override string Name
                => costModel is null ? $"simplify[{level}]" : $"simplify[{level}, {costModel.Name}]";

            public override TransformationRelation Relation => TransformationRelation.Equivalence;

            public override Soundness Soundness => Soundness.SoundUnderAssumptions;

            protected override Entity? ApplyCore(Entity input)
            {
                if (costModel is null)
                    return Simplificator.Simplify(input, level);
                using var _ = MathS.Settings.ComplexityCriteria.Set(costModel.Cost);
                return Simplificator.Simplify(input, level);
            }
        }

        private sealed class ExpansionTransformation : Transformation
        {
            private readonly int level;

            internal ExpansionTransformation(int level) => this.level = level;

            public override string Name => $"expand[{level}]";

            public override TransformationRelation Relation => TransformationRelation.Equivalence;

            public override Soundness Soundness => Soundness.SoundUnderAssumptions;

            protected override Entity? ApplyCore(Entity input) => input.ExpandOverSum(level);
        }

        private sealed class SubstitutionTransformation : Transformation
        {
            private readonly Entity what, with;

            internal SubstitutionTransformation(Entity what, Entity with) => (this.what, this.with) = (what, with);

            public override string Name => $"substitute[{what.Stringize()} := {with.Stringize()}]";

            // The output is a different expression from the input, not another way of
            // writing it, so subtracting the two means nothing.
            public override TransformationRelation Relation => TransformationRelation.Derivation;

            // Replacing every occurrence of a subexpression by another is valid whatever
            // the two are; nothing about it is conditional.
            public override Soundness Soundness => Soundness.Sound;

            protected override Entity? ApplyCore(Entity input) => input.Substitute(what, with);
        }

        private sealed class DifferentiationTransformation : Transformation
        {
            private readonly Variable variable;

            internal DifferentiationTransformation(Variable variable) => this.variable = variable;

            public override string Name => $"differentiate[{variable.Name}]";

            public override TransformationRelation Relation => TransformationRelation.Derivation;

            // The derivative is the derivative where the expression is differentiable, and
            // an unevaluated Derivativef node where this library does not know a rule.
            public override Soundness Soundness => Soundness.SoundUnderAssumptions;

            protected override Entity? ApplyCore(Entity input) => input.DifferentiateOnce(variable);
        }

        private sealed class IntegrationTransformation : Transformation
        {
            private readonly Variable variable;

            internal IntegrationTransformation(Variable variable) => this.variable = variable;

            public override string Name => $"integrate[{variable.Name}]";

            public override TransformationRelation Relation => TransformationRelation.Derivation;

            public override Soundness Soundness => Soundness.SoundUnderAssumptions;

            protected override Entity? ApplyCore(Entity input)
                => Functions.Algebra.Integration.ComputeIndefiniteIntegral(input.InnerSimplified, variable)?.InnerSimplified;
        }

        private sealed class LimitTransformation : Transformation
        {
            private readonly Variable variable;
            private readonly Entity destination;
            private readonly ApproachFrom side;

            internal LimitTransformation(Variable variable, Entity destination, ApproachFrom side)
                => (this.variable, this.destination, this.side) = (variable, destination, side);

            public override string Name => $"limit[{variable.Name} -> {destination.Stringize()}, {side}]";

            public override TransformationRelation Relation => TransformationRelation.Derivation;

            public override Soundness Soundness => Soundness.SoundUnderAssumptions;

            protected override Entity? ApplyCore(Entity input)
                => LimitFunctional.ComputeLimit(input, variable, destination, side);
        }
    }
}
