//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
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
            => LevelledCache.Simplification.For(level, static l => new SimplificationTransformation(l));

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
                Rewriting(RewriteRules.PerfectSquare)
                    .Then(Rewriting(RewriteRules.Factorization))
                    .Then(InnerSimplification)
                    // Entity.Factorize has always run at least one pass, whatever it was asked for.
                    .Repeat(Math.Max(l, 1)));

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
        public static Transformation Canonicalisation { get; }
            = InnerSimplification
                .Then(Rewriting(RewriteRules.CanonicalOrderExact))
                .Then(InnerSimplification);

        /// <summary>
        /// Clears a surd out of a two-term denominator: <c>1 / (sqrt(3) + 5)</c> becomes
        /// <c>(sqrt(3) - 5) / (-22)</c>.
        /// </summary>
        public static Transformation Rationalisation { get; }
            = Rewriting(RewriteRules.RationaliseDenominator).Then(InnerSimplification);

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

            private readonly Transformation?[] cached = new Transformation?[Highest - Lowest + 1];

            internal Transformation For(int level, Func<int, Transformation> make)
                => level < Lowest || level > Highest
                    ? make(level)
                    : cached[level - Lowest] ??= make(level);
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

            internal SimplificationTransformation(int level) => this.level = level;

            public override string Name => $"simplify[{level}]";

            public override TransformationRelation Relation => TransformationRelation.Equivalence;

            public override Soundness Soundness => Soundness.SoundUnderAssumptions;

            protected override Entity? ApplyCore(Entity input) => Simplificator.Simplify(input, level);
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
