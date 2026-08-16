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
    /// Every rule set the simplifier applies is registered here, and the simplifier reaches
    /// them through this registry rather than through <c>Patterns</c> directly. That is what
    /// lets an account of what the simplifier did to an expression be a complete one: a set
    /// reachable only by its method has no name to report and nothing to attribute a step to,
    /// so a derivation built while some sets were still unregistered would quietly omit
    /// whatever they had done.
    /// </para>
    /// <para>
    /// Every entry is declared <see cref="Soundness.SoundUnderAssumptions"/>. That is a claim
    /// about what has been argued, not about what is true — nothing here checks a tier, so
    /// the registry starts conservative and promoting an entry means making the case for it.
    /// </para>
    /// </remarks>
    public static class RewriteRules
    {
        #region Arrangement

        /// <summary>
        /// Puts the operands of commutative chains into a canonical order and groups equal
        /// ones together, so that <c>x + y</c> and <c>y + x</c> stop being different trees.
        /// Looks at variables and functions only, ignoring constants and operators.
        /// </summary>
        public static RewriteRuleSet CanonicalOrder { get; } = new(
            nameof(CanonicalOrder),
            "Sorts and groups the operands of sums, products, conjunctions, disjunctions and set operations, by variables and functions alone.",
            TransformationRelation.Equivalence,
            // Regrouping reads a quotient as a product with a negative power, which is the
            // same value wherever the divisor is not zero.
            Soundness.SoundUnderAssumptions,
            Patterns.SortRules(TreeAnalyzer.SortLevel.HIGH_LEVEL),
            Patterns.SortRulesArms(TreeAnalyzer.SortLevel.HIGH_LEVEL));

        /// <summary>
        /// <see cref="CanonicalOrder"/>, counting constants as well, so that terms differing
        /// only by a numeric factor are no longer grouped together.
        /// </summary>
        public static RewriteRuleSet CanonicalOrderCountingConstants { get; } = new(
            nameof(CanonicalOrderCountingConstants),
            "Sorts and groups commutative operands, distinguishing terms by their constants too.",
            TransformationRelation.Equivalence,
            Soundness.SoundUnderAssumptions,
            Patterns.SortRules(TreeAnalyzer.SortLevel.MIDDLE_LEVEL),
            Patterns.SortRulesArms(TreeAnalyzer.SortLevel.MIDDLE_LEVEL));

        /// <summary>
        /// <see cref="CanonicalOrder"/> over the whole subtree, so that only structurally
        /// identical operands are grouped.
        /// </summary>
        public static RewriteRuleSet CanonicalOrderExact { get; } = new(
            nameof(CanonicalOrderExact),
            "Sorts and groups commutative operands by the whole subtree.",
            TransformationRelation.Equivalence,
            Soundness.SoundUnderAssumptions,
            Patterns.SortRules(TreeAnalyzer.SortLevel.LOW_LEVEL),
            Patterns.SortRulesArms(TreeAnalyzer.SortLevel.LOW_LEVEL));

        /// <summary>
        /// Turns a negative power into a quotient: <c>a * b ^ (-1)</c> becomes <c>a / b</c>.
        /// </summary>
        public static RewriteRuleSet InvertNegativePowers { get; } = new(
            nameof(InvertNegativePowers),
            "Rewrites negative powers as quotients.",
            TransformationRelation.Equivalence,
            Soundness.SoundUnderAssumptions,
            Patterns.InvertNegativePowers,
            Patterns.InvertNegativePowersArms);

        /// <summary>
        /// Brings a negative numeric factor out in front of the term it multiplies.
        /// </summary>
        public static RewriteRuleSet InvertNegativeMultipliers { get; } = new(
            nameof(InvertNegativeMultipliers),
            "Moves a negative numeric factor out of a product into the sign of the term.",
            TransformationRelation.Equivalence,
            Soundness.SoundUnderAssumptions,
            Patterns.InvertNegativeMultipliers,
            Patterns.InvertNegativeMultipliersArms);

        /// <summary>
        /// The arithmetic housekeeping rules — collecting like terms, flattening nested
        /// quotients, moving numeric coefficients to the front.
        /// </summary>
        public static RewriteRuleSet Common { get; } = new(
            nameof(Common),
            "Collects like terms and normalises the arrangement of products and quotients.",
            TransformationRelation.Equivalence,
            Soundness.SoundUnderAssumptions,
            Patterns.CommonRules,
            Patterns.CommonRulesArms);

        /// <summary>
        /// Gets a quotient into the shape the division rules expect before they run.
        /// </summary>
        public static RewriteRuleSet DivisionPreparing { get; } = new(
            nameof(DivisionPreparing),
            "Lifts numeric factors out of a quotient so that the division rules can see it.",
            TransformationRelation.Equivalence,
            Soundness.SoundUnderAssumptions,
            Patterns.DivisionPreparingRules,
            Patterns.DivisionPreparingRulesArms);

        /// <summary>
        /// Cosmetic arrangement of signs, so that adding a negative reads as a difference.
        /// </summary>
        public static RewriteRuleSet NumericNeat { get; } = new(
            nameof(NumericNeat),
            "Arranges signs so that adding a negative is written as subtracting a positive.",
            TransformationRelation.Equivalence,
            Soundness.SoundUnderAssumptions,
            Patterns.NumericNeatRules,
            Patterns.NumericNeatRulesArms);

        #endregion

        #region Powers, products and sums

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
            Patterns.PowerRules,
            Patterns.PowerRulesArms);

        /// <summary>
        /// Multiplies products over sums out.
        /// </summary>
        public static RewriteRuleSet Expansion { get; } = new(
            nameof(Expansion),
            "Distributes products and powers over sums.",
            TransformationRelation.Equivalence,
            Soundness.SoundUnderAssumptions,
            Patterns.ExpandRules,
            Patterns.ExpandRulesArms);

        /// <summary>
        /// Takes common factors back out of a sum.
        /// </summary>
        public static RewriteRuleSet Factorization { get; } = new(
            nameof(Factorization),
            "Gathers common factors out of sums.",
            TransformationRelation.Equivalence,
            Soundness.SoundUnderAssumptions,
            Patterns.FactorizeRules,
            Patterns.FactorizeRulesArms);

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

        #endregion

        #region Quotients

        /// <summary>
        /// Clears a surd out of a two-term denominator.
        /// </summary>
        public static RewriteRuleSet RationalizeDenominator { get; } = new(
            nameof(RationalizeDenominator),
            "Multiplies a quotient by the conjugate of its denominator to clear a surd from it.",
            TransformationRelation.Equivalence,
            Soundness.SoundUnderAssumptions,
            Patterns.RationalizeDenominator);

        /// <summary>
        /// Brings a quotient of quotients down to a single one.
        /// </summary>
        public static RewriteRuleSet CollapseMultipleFractions { get; } = new(
            nameof(CollapseMultipleFractions),
            "Collapses nested quotients into a single numerator over a single denominator.",
            TransformationRelation.Equivalence,
            Soundness.SoundUnderAssumptions,
            Patterns.CollapseMultipleFractions);

        /// <summary>
        /// Puts a sum of quotients over one denominator, grouping the terms by variables and
        /// functions alone.
        /// </summary>
        public static RewriteRuleSet CommonDenominator { get; } = new(
            nameof(CommonDenominator),
            "Adds quotients by putting them over a common denominator.",
            TransformationRelation.Equivalence,
            Soundness.SoundUnderAssumptions,
            expr => Patterns.FractionCommonDenominatorRules(expr, TreeAnalyzer.SortLevel.HIGH_LEVEL));

        /// <summary>
        /// <see cref="CommonDenominator"/>, counting constants when it groups terms.
        /// </summary>
        public static RewriteRuleSet CommonDenominatorCountingConstants { get; } = new(
            nameof(CommonDenominatorCountingConstants),
            "Adds quotients over a common denominator, distinguishing terms by their constants too.",
            TransformationRelation.Equivalence,
            Soundness.SoundUnderAssumptions,
            expr => Patterns.FractionCommonDenominatorRules(expr, TreeAnalyzer.SortLevel.MIDDLE_LEVEL));

        /// <summary>
        /// <see cref="CommonDenominator"/>, grouping terms by the whole subtree.
        /// </summary>
        public static RewriteRuleSet CommonDenominatorExact { get; } = new(
            nameof(CommonDenominatorExact),
            "Adds quotients over a common denominator, grouping terms by the whole subtree.",
            TransformationRelation.Equivalence,
            Soundness.SoundUnderAssumptions,
            expr => Patterns.FractionCommonDenominatorRules(expr, TreeAnalyzer.SortLevel.LOW_LEVEL));

        /// <summary>
        /// Divides one polynomial by another, leaving a quotient plus a remainder.
        /// </summary>
        public static RewriteRuleSet PolynomialLongDivision { get; } = new(
            nameof(PolynomialLongDivision),
            "Divides a polynomial by a polynomial, giving the quotient plus the remainder.",
            TransformationRelation.Equivalence,
            Soundness.SoundUnderAssumptions,
            Patterns.PolynomialLongDivision,
            Patterns.PolynomialLongDivisionArms);

        /// <summary>
        /// Puts a quotient of polynomials into lowest terms.
        /// </summary>
        public static RewriteRuleSet PolynomialGcdCancellation { get; } = new(
            nameof(PolynomialGcdCancellation),
            "Cancels the greatest common divisor of a polynomial quotient's numerator and denominator.",
            TransformationRelation.Equivalence,
            Soundness.SoundUnderAssumptions,
            Patterns.PolynomialGcdCancellation,
            Patterns.PolynomialGcdCancellationArms);

        #endregion

        #region Trigonometry

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
            Patterns.TrigonometricRules,
            Patterns.TrigonometricRulesArms);

        /// <summary>
        /// Rewrites the derived trigonometric functions in terms of sine and cosine.
        /// </summary>
        public static RewriteRuleSet NormalTrigonometricForm { get; } = new(
            nameof(NormalTrigonometricForm),
            "Writes tangents, cotangents, secants and cosecants as sines and cosines.",
            TransformationRelation.Equivalence,
            Soundness.SoundUnderAssumptions,
            Patterns.NormalTrigonometricForm,
            Patterns.NormalTrigonometricFormArms);

        /// <summary>
        /// Gathers sines and cosines back into the derived functions where that is shorter.
        /// </summary>
        public static RewriteRuleSet CollapseTrigonometricFunctions { get; } = new(
            nameof(CollapseTrigonometricFunctions),
            "Recognises a quotient or reciprocal of sines and cosines as a tangent, cotangent, secant or cosecant.",
            TransformationRelation.Equivalence,
            Soundness.SoundUnderAssumptions,
            Patterns.CollapseTrigonometricFunctions,
            Patterns.CollapseTrigonometricFunctionsArms);

        /// <summary>
        /// Opens a trigonometric function of a sum into functions of its terms.
        /// </summary>
        public static RewriteRuleSet ExpandTrigonometric { get; } = new(
            nameof(ExpandTrigonometric),
            "Expands a sine or cosine of a sum into products of sines and cosines of its terms.",
            TransformationRelation.Equivalence,
            Soundness.SoundUnderAssumptions,
            Patterns.ExpandTrigonometricRules,
            Patterns.ExpandTrigonometricRulesArms);

        /// <summary>
        /// Opens a trigonometric function of a multiplied angle.
        /// </summary>
        /// <remarks>
        /// Written out, <c>sin(4x)</c> is far longer than it started, which is why the
        /// simplifier offers the result as a candidate rather than taking it.
        /// </remarks>
        public static RewriteRuleSet ExpandMultipleAngle { get; } = new(
            nameof(ExpandMultipleAngle),
            "Expands a sine or cosine of an integer multiple of an angle.",
            TransformationRelation.Equivalence,
            Soundness.SoundUnderAssumptions,
            Patterns.ExpandMultipleAngleRules,
            Patterns.ExpandMultipleAngleRulesArms);

        #endregion

        #region Statements, sets and number theory

        /// <summary>
        /// The rules of boolean algebra.
        /// </summary>
        public static RewriteRuleSet Boolean { get; } = new(
            nameof(Boolean),
            "Applies the identities of boolean algebra to conjunctions, disjunctions and negations.",
            TransformationRelation.Equivalence,
            Soundness.SoundUnderAssumptions,
            Patterns.BooleanRules,
            Patterns.BooleanRulesArms);

        /// <summary>
        /// Rules about equalities and inequalities.
        /// </summary>
        public static RewriteRuleSet InequalityEquality { get; } = new(
            nameof(InequalityEquality),
            "Rearranges equalities and inequalities into their usual form.",
            TransformationRelation.Equivalence,
            Soundness.SoundUnderAssumptions,
            Patterns.InequalityEqualityRules,
            Patterns.InequalityEqualityRulesArms);

        /// <summary>
        /// Rules about unions, intersections and set differences.
        /// </summary>
        public static RewriteRuleSet SetOperator { get; } = new(
            nameof(SetOperator),
            "Applies the identities of set algebra to unions, intersections and set differences.",
            TransformationRelation.Equivalence,
            Soundness.SoundUnderAssumptions,
            Patterns.SetOperatorRules,
            Patterns.SetOperatorRulesArms);

        /// <summary>
        /// Cancels a quotient of factorials down to the terms that survive.
        /// </summary>
        public static RewriteRuleSet ExpandFactorialDivisions { get; } = new(
            nameof(ExpandFactorialDivisions),
            "Cancels a quotient of factorials into the product of the terms that do not cancel.",
            TransformationRelation.Equivalence,
            Soundness.SoundUnderAssumptions,
            Patterns.ExpandFactorialDivisions);

        /// <summary>
        /// Recognises a product of consecutive terms as a factorial.
        /// </summary>
        public static RewriteRuleSet FactorizeFactorialMultiplications { get; } = new(
            nameof(FactorizeFactorialMultiplications),
            "Gathers a product of a factorial and its neighbouring terms back into one factorial.",
            TransformationRelation.Equivalence,
            Soundness.SoundUnderAssumptions,
            Patterns.FactorizeFactorialMultiplications);

        /// <summary>
        /// Rules about Euler's totient function.
        /// </summary>
        public static RewriteRuleSet PhiFunction { get; } = new(
            nameof(PhiFunction),
            "Applies the multiplicative identities of Euler's totient function.",
            TransformationRelation.Equivalence,
            Soundness.SoundUnderAssumptions,
            Patterns.PhiFunctionRules,
            Patterns.PhiFunctionRulesArms);

        #endregion

        /// <summary>
        /// Every rule set registered above, in a fixed order. Enumerable so that a property
        /// that should hold of all of them can be tested over all of them rather than over
        /// whichever ones somebody remembered.
        /// </summary>
        public static IReadOnlyList<RewriteRuleSet> All { get; } = new[]
        {
            CanonicalOrder,
            CanonicalOrderCountingConstants,
            CanonicalOrderExact,
            InvertNegativePowers,
            InvertNegativeMultipliers,
            Common,
            DivisionPreparing,
            NumericNeat,
            Power,
            Expansion,
            Factorization,
            PerfectSquare,
            RationalizeDenominator,
            CollapseMultipleFractions,
            CommonDenominator,
            CommonDenominatorCountingConstants,
            CommonDenominatorExact,
            PolynomialLongDivision,
            PolynomialGcdCancellation,
            Trigonometric,
            NormalTrigonometricForm,
            CollapseTrigonometricFunctions,
            ExpandTrigonometric,
            ExpandMultipleAngle,
            Boolean,
            InequalityEquality,
            SetOperator,
            ExpandFactorialDivisions,
            FactorizeFactorialMultiplications,
            PhiFunction,
        };

        /// <summary>
        /// The <see cref="CanonicalOrder"/> family, chosen by how finely it distinguishes
        /// operands. The simplifier picks the level from which pass it is on.
        /// </summary>
        internal static RewriteRuleSet CanonicalOrderAt(TreeAnalyzer.SortLevel level)
            => level switch
            {
                TreeAnalyzer.SortLevel.MIDDLE_LEVEL => CanonicalOrderCountingConstants,
                TreeAnalyzer.SortLevel.LOW_LEVEL => CanonicalOrderExact,
                _ => CanonicalOrder
            };

        /// <summary>
        /// The <see cref="CommonDenominator"/> family, chosen the same way.
        /// </summary>
        internal static RewriteRuleSet CommonDenominatorAt(TreeAnalyzer.SortLevel level)
            => level switch
            {
                TreeAnalyzer.SortLevel.MIDDLE_LEVEL => CommonDenominatorCountingConstants,
                TreeAnalyzer.SortLevel.LOW_LEVEL => CommonDenominatorExact,
                _ => CommonDenominator
            };
    }
}
