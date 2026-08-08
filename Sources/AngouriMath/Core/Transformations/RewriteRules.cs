//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Functions;

namespace AngouriMath.Core.Transformations
{
    /// <summary>
    /// The rewrite rule sets this library ships, as data: named, described, attributed with
    /// what they claim, and enumerable through <see cref="All"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Registration is explicit and static — there is no assembly scanning and no
    /// <c>Activator</c>, so the registry survives trimming and NativeAOT, and
    /// <see cref="All"/> is in a fixed order that does not depend on hashing, reflection or
    /// which type happened to be loaded first.
    /// </para>
    /// <para>
    /// This is a slice, not the whole pattern table. The sets below are the ones the
    /// transformations in <see cref="Transformation"/> are built from; the rest of
    /// <c>Functions/Simplification/Patterns</c> is still reached only from
    /// <c>Simplificator</c>. Adding one here is five lines and gets it enumeration, a
    /// soundness label and the tests over <see cref="All"/> for free.
    /// </para>
    /// </remarks>
    public static class RewriteRules
    {
        /// <summary>
        /// Puts the operands of commutative chains into a canonical order and groups equal
        /// ones together, so that <c>x + y</c> and <c>y + x</c> stop being different trees.
        /// </summary>
        public static RewriteRuleSet CanonicalOrder { get; } = new(
            nameof(CanonicalOrder),
            "Sorts and groups the operands of sums, products, conjunctions, disjunctions and set operations.",
            TransformationRelation.Equivalence,
            // Regrouping reads a quotient as a product with a negative power, which is the
            // same value wherever the divisor is not zero.
            Soundness.SoundUnderAssumptions,
            Patterns.SortRules(TreeAnalyzer.SortLevel.HIGH_LEVEL));

        /// <summary>
        /// Turns a negative power into a quotient: <c>a * b ^ (-1)</c> becomes <c>a / b</c>.
        /// </summary>
        public static RewriteRuleSet InvertNegativePowers { get; } = new(
            nameof(InvertNegativePowers),
            "Rewrites negative powers as quotients.",
            TransformationRelation.Equivalence,
            Soundness.SoundUnderAssumptions,
            Patterns.InvertNegativePowers);

        /// <summary>
        /// Brings a negative numeric factor out in front of the term it multiplies.
        /// </summary>
        public static RewriteRuleSet InvertNegativeMultipliers { get; } = new(
            nameof(InvertNegativeMultipliers),
            "Moves a negative numeric factor out of a product into the sign of the term.",
            TransformationRelation.Equivalence,
            Soundness.SoundUnderAssumptions,
            Patterns.InvertNegativeMultipliers);

        /// <summary>
        /// The arithmetic housekeeping rules — collecting like terms, flattening nested
        /// quotients, moving numeric coefficients to the front.
        /// </summary>
        public static RewriteRuleSet Common { get; } = new(
            nameof(Common),
            "Collects like terms and normalises the arrangement of products and quotients.",
            TransformationRelation.Equivalence,
            Soundness.SoundUnderAssumptions,
            Patterns.CommonRules);

        /// <summary>
        /// Rules about powers, roots and logarithms.
        /// </summary>
        public static RewriteRuleSet Power { get; } = new(
            nameof(Power),
            "Gathers and splits powers, roots and logarithms.",
            TransformationRelation.Equivalence,
            // (a ^ b) ^ c is a ^ (b c) only on a branch; the rules guard for it, and the
            // guard is what the tier is stating.
            Soundness.SoundUnderAssumptions,
            Patterns.PowerRules);

        /// <summary>
        /// The trigonometric identities.
        /// </summary>
        public static RewriteRuleSet Trigonometric { get; } = new(
            nameof(Trigonometric),
            "Applies trigonometric identities to sines, cosines and their relatives.",
            TransformationRelation.Equivalence,
            // tan and cot bring poles with them, so an identity that introduces one holds
            // away from those points rather than everywhere.
            Soundness.SoundUnderAssumptions,
            Patterns.TrigonometricRules);

        /// <summary>
        /// Multiplies products over sums out.
        /// </summary>
        public static RewriteRuleSet Expansion { get; } = new(
            nameof(Expansion),
            "Distributes products and powers over sums.",
            TransformationRelation.Equivalence,
            Soundness.SoundUnderAssumptions,
            Patterns.ExpandRules);

        /// <summary>
        /// Takes common factors back out of a sum.
        /// </summary>
        public static RewriteRuleSet Factorization { get; } = new(
            nameof(Factorization),
            "Gathers common factors out of sums.",
            TransformationRelation.Equivalence,
            Soundness.SoundUnderAssumptions,
            Patterns.FactorizeRules);

        /// <summary>
        /// Recognises a perfect square written out, so that factorisation has something to
        /// gather.
        /// </summary>
        public static RewriteRuleSet PerfectSquare { get; } = new(
            nameof(PerfectSquare),
            "Collapses a written-out perfect square into a squared binomial.",
            TransformationRelation.Equivalence,
            Soundness.SoundUnderAssumptions,
            Patterns.PerfectSquareRules);

        /// <summary>
        /// Clears a surd out of a two-term denominator.
        /// </summary>
        public static RewriteRuleSet RationaliseDenominator { get; } = new(
            nameof(RationaliseDenominator),
            "Multiplies a quotient by the conjugate of its denominator to clear a surd from it.",
            TransformationRelation.Equivalence,
            Soundness.SoundUnderAssumptions,
            Patterns.RationaliseDenominator);

        /// <summary>
        /// Every rule set registered above, in a fixed order. Enumerable so that a property
        /// that should hold of all of them can be tested over all of them rather than over
        /// whichever ones somebody remembered.
        /// </summary>
        public static IReadOnlyList<RewriteRuleSet> All { get; } = new[]
        {
            CanonicalOrder,
            InvertNegativePowers,
            InvertNegativeMultipliers,
            Common,
            Power,
            Trigonometric,
            Expansion,
            Factorization,
            PerfectSquare,
            RationaliseDenominator,
        };
    }
}
