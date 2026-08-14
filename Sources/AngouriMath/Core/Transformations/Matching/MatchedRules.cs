//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;

namespace AngouriMath.Core.Transformations.Matching
{
    /// <summary>
    /// Rule sets written as data, a few at a time and deliberately so: the value of this file is
    /// that a set expressed here can be checked against the <c>switch</c> that already expresses
    /// it, so the migration is proven one set at a time rather than asserted wholesale.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>MatchedRulesAgreeWithTheSwitchTest</c> is that check. It runs both forms over
    /// generated expressions and requires them to agree on every one, which is what makes
    /// replacing the <c>switch</c> a mechanical step rather than a leap.
    /// </para>
    /// <para>
    /// <see cref="PythagoreanIdentity"/> is the exception, and is here for the opposite reason:
    /// there is nothing for it to agree with. It uses n-ary matching to say something the
    /// <c>switch</c> has no way of saying, so it is checked against the mathematics rather than
    /// against the code it would replace.
    /// </para>
    /// </remarks>
    internal static class MatchedRules
    {
        /// <summary>
        /// <see cref="Functions.Patterns.DivisionPreparingRules"/>, as data.
        /// </summary>
        /// <remarks>
        /// Chosen first because it is three rules with no side conditions, so it exercises node
        /// matching, a literal, a typed hole and a repeated-free binding without also needing
        /// commutativity — which the matcher does not have and which #248 is about.
        /// </remarks>
        internal static MatchedRuleSet DivisionPreparing { get; } = new(
            nameof(DivisionPreparing),

            // a * (1 / b) -> a / b
            new MatchedRule(
                "reciprocal-factor-becomes-a-quotient",
                MatchPattern.Node<Mulf>(
                    MatchPattern.Any("a"),
                    MatchPattern.Node<Divf>(MatchPattern.Exact(Integer.Create(1)), MatchPattern.Any("b"))),
                bound => bound["a"] / bound["b"],
                // a * (1/b) and a/b are undefined at exactly the same points, but the quotient
                // is a quotient either way, so this inherits division's own condition rather
                // than adding one. Left at the conservative tier until the audit reaches it.
                Soundness.SoundUnderAssumptions),

            // (c * a) / b -> c * (a / b), for a numeric c
            new MatchedRule(
                "numeric-factor-out-of-a-quotient",
                MatchPattern.Node<Divf>(
                    MatchPattern.Node<Mulf>(MatchPattern.Any<Number>("c"), MatchPattern.Any("a")),
                    MatchPattern.Any("b")),
                bound => bound["c"] * (bound["a"] / bound["b"]),
                Soundness.SoundUnderAssumptions),

            // (c / a) * b -> c * (b / a), for a numeric c
            new MatchedRule(
                "numeric-numerator-out-of-a-product",
                MatchPattern.Node<Mulf>(
                    MatchPattern.Node<Divf>(MatchPattern.Any<Number>("c"), MatchPattern.Any("a")),
                    MatchPattern.Any("b")),
                bound => bound["c"] * (bound["b"] / bound["a"]),
                Soundness.SoundUnderAssumptions));

        /// <summary>
        /// <see cref="Functions.Patterns.CollapseMultipleFractions"/>, as data.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The second set expressed here, and chosen because it is harder in three ways that
        /// test whether the shape generalises rather than whether it works once. It has eight
        /// rules instead of three; it is <b>order-dependent</b>, since
        /// <c>Mulf(Divf, Divf)</c> has to be tried before <c>Mulf(a, Divf)</c> or the more
        /// general rule would swallow the special one; and it needs a <b>predicate on a
        /// hole</b> — <c>Integer { IsPositive: true }</c> — which the matcher did not have.
        /// </para>
        /// <para>
        /// One feature was added for it and nothing else changed, which is the answer to the
        /// question this set was picked to ask.
        /// </para>
        /// </remarks>
        internal static MatchedRuleSet CollapseMultipleFractions { get; } = new(
            nameof(CollapseMultipleFractions),

            // (a / b) ^ c -> a^c / b^c, for a positive whole c
            new MatchedRule(
                "positive-power-of-a-quotient-distributes",
                MatchPattern.Node<Powf>(
                    MatchPattern.Node<Divf>(MatchPattern.Any("a"), MatchPattern.Any("b")),
                    MatchPattern.Any<Integer>("c", whole => whole.IsPositive)),
                bound => bound["a"].Pow(bound["c"]) / bound["b"].Pow(bound["c"]),
                Soundness.SoundUnderAssumptions),

            // (a * b) ^ c -> a^c * b^c, for a positive whole c
            new MatchedRule(
                "positive-power-of-a-product-distributes",
                MatchPattern.Node<Powf>(
                    MatchPattern.Node<Mulf>(MatchPattern.Any("a"), MatchPattern.Any("b")),
                    MatchPattern.Any<Integer>("c", whole => whole.IsPositive)),
                bound => bound["a"].Pow(bound["c"]) * bound["b"].Pow(bound["c"]),
                Soundness.SoundUnderAssumptions),

            // (a/b) * (c/d) -> (a*c) / (b*d). Before the two below it, which are more general.
            new MatchedRule(
                "product-of-two-quotients",
                MatchPattern.Node<Mulf>(
                    MatchPattern.Node<Divf>(MatchPattern.Any("a"), MatchPattern.Any("b")),
                    MatchPattern.Node<Divf>(MatchPattern.Any("c"), MatchPattern.Any("d"))),
                bound => bound["a"] * bound["c"] / (bound["b"] * bound["d"]),
                Soundness.SoundUnderAssumptions),

            new MatchedRule(
                "product-with-a-quotient-on-the-right",
                MatchPattern.Node<Mulf>(
                    MatchPattern.Any("a"),
                    MatchPattern.Node<Divf>(MatchPattern.Any("b"), MatchPattern.Any("c"))),
                bound => bound["a"] * bound["b"] / bound["c"],
                Soundness.SoundUnderAssumptions),

            new MatchedRule(
                "product-with-a-quotient-on-the-left",
                MatchPattern.Node<Mulf>(
                    MatchPattern.Node<Divf>(MatchPattern.Any("a"), MatchPattern.Any("b")),
                    MatchPattern.Any("c")),
                bound => bound["a"] * bound["c"] / bound["b"],
                Soundness.SoundUnderAssumptions),

            // (a/b) / (c/d) -> (a*d) / (b*c). Likewise before the two below it.
            new MatchedRule(
                "quotient-of-two-quotients",
                MatchPattern.Node<Divf>(
                    MatchPattern.Node<Divf>(MatchPattern.Any("a"), MatchPattern.Any("b")),
                    MatchPattern.Node<Divf>(MatchPattern.Any("c"), MatchPattern.Any("d"))),
                bound => bound["a"] * bound["d"] / (bound["b"] * bound["c"]),
                Soundness.SoundUnderAssumptions),

            new MatchedRule(
                "quotient-whose-numerator-is-a-quotient",
                MatchPattern.Node<Divf>(
                    MatchPattern.Node<Divf>(MatchPattern.Any("a"), MatchPattern.Any("b")),
                    MatchPattern.Any("c")),
                bound => bound["a"] / (bound["b"] * bound["c"]),
                Soundness.SoundUnderAssumptions),

            new MatchedRule(
                "quotient-whose-denominator-is-a-quotient",
                MatchPattern.Node<Divf>(
                    MatchPattern.Any("a"),
                    MatchPattern.Node<Divf>(MatchPattern.Any("b"), MatchPattern.Any("c"))),
                bound => bound["a"] * bound["c"] / bound["b"],
                Soundness.SoundUnderAssumptions));

        /// <summary>
        /// The one rule from <see cref="Functions.Patterns.PowerRules"/> that carries a real
        /// side condition, as data.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The third set expressed here, and the one that exercises the last piece of the
        /// design: a <b>condition about the match as a whole</b> rather than about one hole.
        /// <c>(a^b)^c = a^(b*c)</c> is true for a positive base whatever the exponents, and for
        /// any base when the outer exponent is whole — and false outside those two, which is
        /// <a href="https://github.com/asc-community/AngouriMath/issues/752">#752</a>: applied
        /// unconditionally it turned <c>sqrt(x^2)</c> into <c>x</c>, which at -0.63 is -0.63
        /// where the expression is 0.63.
        /// </para>
        /// <para>
        /// It is also the first rule here whose <see cref="Soundness"/> carries information
        /// rather than repeating its neighbours'. The condition is what makes it
        /// <see cref="Soundness.SoundUnderAssumptions"/>, and a reader can see the condition
        /// and the tier in one place — which is the whole argument for rules being data, since
        /// in the <c>switch</c> the tier lives on the set and the condition lives forty lines
        /// away from it.
        /// </para>
        /// </remarks>
        internal static MatchedRuleSet PowerOfPower { get; } = new(
            nameof(PowerOfPower),

            new MatchedRule(
                "power-of-a-power-multiplies-its-exponents",
                MatchPattern.Node<Powf>(
                    MatchPattern.Node<Powf>(MatchPattern.Any("a"), MatchPattern.Any("b")),
                    MatchPattern.Any("c")),
                bound => new Powf(bound["a"], bound["b"] * bound["c"]),
                Soundness.SoundUnderAssumptions,
                // Two bindings at once, which no predicate on a single hole can express.
                when: bound => bound["c"] is Integer
                    || bound["a"].Evaled is Real { IsPositive: true }));

        /// <summary>
        /// <c>k*p + k*q = k*(p + q)</c>, written once, where the <c>switch</c> writes it four
        /// times.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is what <a href="https://github.com/asc-community/AngouriMath/issues/248">#248</a>
        /// is for. <c>Patterns.CommonRules</c> spells the same identity out in four arms —
        /// <c>(k*p) + (k*q)</c>, <c>(p*k) + (k*q)</c>, <c>(k*p) + (q*k)</c>, <c>(p*k) + (q*k)</c>
        /// — because a C# pattern cannot say "either way round". One commutative pattern says
        /// it, and the four arms become one rule.
        /// </para>
        /// <para>
        /// It is also the <b>first rule here that is <see cref="Soundness.Sound"/></b>.
        /// Distributivity holds for every complex <c>k</c>, <c>p</c> and <c>q</c> with no side
        /// condition and no branch to choose, so the tier says something its neighbours' does
        /// not — which is the whole reason a tier belongs on a rule rather than on a set.
        /// </para>
        /// </remarks>
        internal static MatchedRuleSet SharedFactor { get; } = new(
            nameof(SharedFactor),

            new MatchedRule(
                "a-shared-factor-comes-out-of-a-sum",
                MatchPattern.Commutative<Sumf>(
                    MatchPattern.Commutative<Mulf>(MatchPattern.Any("k"), MatchPattern.Any("p")),
                    MatchPattern.Commutative<Mulf>(MatchPattern.Any("k"), MatchPattern.Any("q"))),
                bound => bound["k"] * (bound["p"] + bound["q"]),
                Soundness.Sound));

        /// <summary>
        /// The Pythagorean identity, written once and firing wherever the two terms sit.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This set exists to show what <see cref="MatchPattern.Gathered{T}"/> buys, because the
        /// <c>switch</c> cannot express it. <c>Patterns.TrigonometricRules</c> spends <b>two</b>
        /// arms on this identity — <c>Sumf(Powf(Sinf, 2), Powf(Cosf, 2))</c> and the same with the
        /// operands the other way round — because it has no commutative matching, and both match
        /// only two children of one <c>Sumf</c>. So the pair is found in
        /// <c>sin(x)^2 + cos(x)^2</c> and missed in <c>a + sin(x)^2 + b + cos(x)^2</c>, where the
        /// two terms are not siblings. The library's answer is to sort the operands with
        /// <c>CanonicalOrder</c> before the rules run, so the pair becomes adjacent. That works,
        /// and it is the matcher's limitation showing through as a pipeline stage.
        /// </para>
        /// <para>
        /// Here the rule says what it means: <i>among the terms of this sum, find one squared
        /// sine and one squared cosine of the same argument</i>. The rest of the sum comes back
        /// bound, and is <c>0</c> when there is none, so the same rule covers both shapes.
        /// </para>
        /// <para>
        /// <see cref="Soundness.Sound"/>: <c>sin²+cos² = 1</c> holds for every complex argument,
        /// with no branch to choose and no point excluded.
        /// </para>
        /// </remarks>
        internal static MatchedRuleSet PythagoreanIdentity { get; } = new(
            nameof(PythagoreanIdentity),

            // ... + sin(x)^2 + ... + cos(x)^2 + ...  ->  1 + (everything else)
            new MatchedRule(
                "squared-sine-and-cosine-of-one-argument-sum-to-one",
                MatchPattern.Gathered<Sumf>(
                    "rest",
                    MatchPattern.Node<Powf>(
                        MatchPattern.Node<Sinf>(MatchPattern.Any("x")),
                        MatchPattern.Exact(Integer.Create(2))),
                    MatchPattern.Node<Powf>(
                        MatchPattern.Node<Cosf>(MatchPattern.Any("x")),
                        MatchPattern.Exact(Integer.Create(2)))),
                // "x" is bound by the first part and re-matched by the second, so the two terms
                // are required to be about the same argument rather than merely both squared.
                bound => 1 + bound["rest"],
                Soundness.Sound));
    }
}
