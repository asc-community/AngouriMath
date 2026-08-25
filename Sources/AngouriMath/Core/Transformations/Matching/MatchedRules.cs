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
    /// <para>
    /// <b>Both sides of every rule here are patterns</b>, so thirteen of the fourteen can be read
    /// backwards — <see cref="MatchedRule.Reversed"/>, and
    /// <c>Docs/Contributing/ReversibleRules.md</c> for what that requires and what it does not
    /// claim. The fourteenth is the Pythagorean identity, which cannot, for a reason that is about
    /// the mathematics rather than about the encoding.
    /// </para>
    /// <para>
    /// <b>What it costs to use one of these has been measured end to end, and the first answer
    /// was wrong about the reason.</b> The first exchange cost about <b>5% of
    /// <c>Simplify</c>'s time</b> for one set, which was recorded here as the price of the idea
    /// and as the argument against doing it wholesale. It was not the idea: <c>NodePattern</c>
    /// recomputed <see cref="MatchPattern.IsDeterministic"/> on <i>every attempt</i>, walking the
    /// whole pattern tree behind a delegate before any matching began, so the case that does the
    /// least work — a rule that does not fire — paid the most for it. Settled once instead, a
    /// miss goes 29.99 ns → 13.48 ns and a whole pass 1182.9 ns → 659.9 ns at identical
    /// allocation.
    /// </para>
    /// <para>
    /// Re-measured against <c>Simplify</c> itself, three arms in one process with the third a
    /// second copy of the first, the exchange is <b>inside the noise floor on time</b> — six
    /// samples each, medians +0.12% apart where the same binary spreads 1.90% — and
    /// <b>+0.14% on allocation</b>, which is the one figure that reproduces: two copies of the
    /// same assembly agree to 5 bytes in 45.6 MB. So a set is exchanged when its rules want to
    /// be data, and the cost is no longer the reason not to.
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
                MatchPattern.Node<Divf>(MatchPattern.Any("a"), MatchPattern.Any("b")),
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
                MatchPattern.Node<Mulf>(
                    MatchPattern.Any("c"),
                    MatchPattern.Node<Divf>(MatchPattern.Any("a"), MatchPattern.Any("b"))),
                Soundness.SoundUnderAssumptions),

            // (c / a) * b -> c * (b / a), for a numeric c
            new MatchedRule(
                "numeric-numerator-out-of-a-product",
                MatchPattern.Node<Mulf>(
                    MatchPattern.Node<Divf>(MatchPattern.Any<Number>("c"), MatchPattern.Any("a")),
                    MatchPattern.Any("b")),
                MatchPattern.Node<Mulf>(
                    MatchPattern.Any("c"),
                    MatchPattern.Node<Divf>(MatchPattern.Any("b"), MatchPattern.Any("a"))),
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
                MatchPattern.Node<Divf>(
                    MatchPattern.Node<Powf>(MatchPattern.Any("a"), MatchPattern.Any("c")),
                    MatchPattern.Node<Powf>(MatchPattern.Any("b"), MatchPattern.Any("c"))),
                Soundness.SoundUnderAssumptions),

            // (a * b) ^ c -> a^c * b^c, for a positive whole c
            new MatchedRule(
                "positive-power-of-a-product-distributes",
                MatchPattern.Node<Powf>(
                    MatchPattern.Node<Mulf>(MatchPattern.Any("a"), MatchPattern.Any("b")),
                    MatchPattern.Any<Integer>("c", whole => whole.IsPositive)),
                MatchPattern.Node<Mulf>(
                    MatchPattern.Node<Powf>(MatchPattern.Any("a"), MatchPattern.Any("c")),
                    MatchPattern.Node<Powf>(MatchPattern.Any("b"), MatchPattern.Any("c"))),
                Soundness.SoundUnderAssumptions),

            // (a/b) * (c/d) -> (a*c) / (b*d). Before the two below it, which are more general.
            new MatchedRule(
                "product-of-two-quotients",
                MatchPattern.Node<Mulf>(
                    MatchPattern.Node<Divf>(MatchPattern.Any("a"), MatchPattern.Any("b")),
                    MatchPattern.Node<Divf>(MatchPattern.Any("c"), MatchPattern.Any("d"))),
                MatchPattern.Node<Divf>(
                    MatchPattern.Node<Mulf>(MatchPattern.Any("a"), MatchPattern.Any("c")),
                    MatchPattern.Node<Mulf>(MatchPattern.Any("b"), MatchPattern.Any("d"))),
                Soundness.SoundUnderAssumptions),

            new MatchedRule(
                "product-with-a-quotient-on-the-right",
                MatchPattern.Node<Mulf>(
                    MatchPattern.Any("a"),
                    MatchPattern.Node<Divf>(MatchPattern.Any("b"), MatchPattern.Any("c"))),
                MatchPattern.Node<Divf>(
                    MatchPattern.Node<Mulf>(MatchPattern.Any("a"), MatchPattern.Any("b")),
                    MatchPattern.Any("c")),
                Soundness.SoundUnderAssumptions),

            new MatchedRule(
                "product-with-a-quotient-on-the-left",
                MatchPattern.Node<Mulf>(
                    MatchPattern.Node<Divf>(MatchPattern.Any("a"), MatchPattern.Any("b")),
                    MatchPattern.Any("c")),
                MatchPattern.Node<Divf>(
                    MatchPattern.Node<Mulf>(MatchPattern.Any("a"), MatchPattern.Any("c")),
                    MatchPattern.Any("b")),
                Soundness.SoundUnderAssumptions),

            // (a/b) / (c/d) -> (a*d) / (b*c). Likewise before the two below it.
            new MatchedRule(
                "quotient-of-two-quotients",
                MatchPattern.Node<Divf>(
                    MatchPattern.Node<Divf>(MatchPattern.Any("a"), MatchPattern.Any("b")),
                    MatchPattern.Node<Divf>(MatchPattern.Any("c"), MatchPattern.Any("d"))),
                MatchPattern.Node<Divf>(
                    MatchPattern.Node<Mulf>(MatchPattern.Any("a"), MatchPattern.Any("d")),
                    MatchPattern.Node<Mulf>(MatchPattern.Any("b"), MatchPattern.Any("c"))),
                Soundness.SoundUnderAssumptions),

            new MatchedRule(
                "quotient-whose-numerator-is-a-quotient",
                MatchPattern.Node<Divf>(
                    MatchPattern.Node<Divf>(MatchPattern.Any("a"), MatchPattern.Any("b")),
                    MatchPattern.Any("c")),
                MatchPattern.Node<Divf>(
                    MatchPattern.Any("a"),
                    MatchPattern.Node<Mulf>(MatchPattern.Any("b"), MatchPattern.Any("c"))),
                Soundness.SoundUnderAssumptions),

            new MatchedRule(
                "quotient-whose-denominator-is-a-quotient",
                MatchPattern.Node<Divf>(
                    MatchPattern.Any("a"),
                    MatchPattern.Node<Divf>(MatchPattern.Any("b"), MatchPattern.Any("c"))),
                MatchPattern.Node<Divf>(
                    MatchPattern.Node<Mulf>(MatchPattern.Any("a"), MatchPattern.Any("c")),
                    MatchPattern.Any("b")),
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
                MatchPattern.Node<Powf>(
                    MatchPattern.Any("a"),
                    MatchPattern.Node<Mulf>(MatchPattern.Any("b"), MatchPattern.Any("c"))),
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
                MatchPattern.Node<Mulf>(
                    MatchPattern.Any("k"),
                    MatchPattern.Node<Sumf>(MatchPattern.Any("p"), MatchPattern.Any("q"))),
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
        /// <para>
        /// <b>The one rule here with no backwards reading</b>, and the reason is the identity
        /// rather than the way it is written: <c>1</c> does not say which angle it came from, so
        /// <c>x</c> is bound on the left and mentioned nowhere on the right.
        /// <see cref="MatchedRule.Reversal"/> is <see cref="RuleReversal.ReplacementDropsHoles"/>
        /// and <see cref="MatchedRule.Reversed"/> is <see langword="null"/>.
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
                MatchPattern.Node<Sumf>(MatchPattern.Exact(Integer.Create(1)), MatchPattern.Any("rest")),
                Soundness.Sound));

        /// <summary>
        /// <see cref="Functions.Patterns.InvertNegativePowers"/>, as data.
        /// </summary>
        /// <remarks>
        /// One rule, and the first here whose replacement is <b>code rather than a pattern</b>:
        /// <c>-1 * n</c> is arithmetic on the bound integer and not a tree built around it, so
        /// writing it as a pattern would produce <c>a ^ (-1 * -3)</c> where the <c>switch</c>
        /// produces <c>a ^ 3</c>. That costs the reversal —
        /// <see cref="RuleReversal.ReplacementIsCode"/> — which is the honest trade and is
        /// recorded by the type rather than by a comment.
        /// </remarks>
        internal static MatchedRuleSet InvertNegativePowers { get; } = new(
            nameof(InvertNegativePowers),

            // a ^ n -> 1 / a ^ (-n), for a negative integer n
            new MatchedRule(
                "a-negative-integer-power-becomes-a-reciprocal",
                MatchPattern.Node<Powf>(
                    MatchPattern.Any("a"),
                    MatchPattern.Any<Integer>("n", n => n.IsNegative)),
                bound => 1 / MathS.Pow(bound["a"], -1 * (Integer)bound["n"]),
                // Both sides are undefined at exactly one place, a = 0, and equal everywhere
                // else in the complex plane: this is the definition of a negative power rather
                // than an identity that needs one. Measured at 0 as well as away from it before
                // the tier was written down.
                Soundness.Sound));

        /// <summary>
        /// <see cref="Functions.Patterns.InvertNegativeMultipliers"/>, as data.
        /// </summary>
        /// <remarks>
        /// Two rules that differ in the way that matters here. The first negates a bound real,
        /// so its replacement is code and it has no reversal. The second only rearranges what it
        /// bound — <c>-(a - b) = b - a</c> — so both of its sides are patterns and it reads
        /// backwards, which is the distinction <see cref="MatchedRule.Reversal"/> exists to make
        /// visible within one set.
        /// </remarks>
        internal static MatchedRuleSet InvertNegativeMultipliers { get; } = new(
            nameof(InvertNegativeMultipliers),

            // a + (c * b) -> a - (-c) * b, for a negative real c
            new MatchedRule(
                "a-negative-factor-in-a-sum-becomes-a-difference",
                MatchPattern.Node<Sumf>(
                    MatchPattern.Any("a"),
                    MatchPattern.Node<Mulf>(
                        MatchPattern.Any<Real>("c", c => c.IsNegative),
                        MatchPattern.Any("b"))),
                bound => bound["a"] - (-1 * (Real)bound["c"]) * bound["b"],
                Soundness.Sound),

            // (-1) * (a - b) -> b - a
            new MatchedRule(
                "a-negated-difference-is-the-difference-the-other-way",
                MatchPattern.Node<Mulf>(
                    MatchPattern.Exact(Integer.Create(-1)),
                    MatchPattern.Node<Minusf>(MatchPattern.Any("a"), MatchPattern.Any("b"))),
                MatchPattern.Node<Minusf>(MatchPattern.Any("b"), MatchPattern.Any("a")),
                Soundness.Sound));

        /// <summary>
        /// <see cref="Functions.Patterns.NormalTrigonometricForm"/>, as data.
        /// </summary>
        /// <remarks>
        /// Four rules, and <b>the first set here where every rule reverses</b>: each writes one
        /// of the four derived trigonometric functions in terms of sine and cosine, and both
        /// sides are patterns, so reading a rule backwards recognises the quotient and puts the
        /// name back. That direction is not run by anything today — it is the e-graph's
        /// inverse-pair table that would — but it is now a property of the rules rather than a
        /// second set someone has to write.
        /// </remarks>
        internal static MatchedRuleSet NormalTrigonometricForm { get; } = new(
            nameof(NormalTrigonometricForm),

            // tan(a) -> sin(a) / cos(a)
            new MatchedRule(
                "tangent-is-sine-over-cosine",
                MatchPattern.Node<Tanf>(MatchPattern.Any("a")),
                MatchPattern.Node<Divf>(
                    MatchPattern.Node<Sinf>(MatchPattern.Any("a")),
                    MatchPattern.Node<Cosf>(MatchPattern.Any("a"))),
                // The quotient is undefined exactly where the tangent is -- at a zero of the
                // cosine -- so the domain neither widens nor narrows. Left at the conservative
                // tier with its three neighbours until the audit reaches them together.
                Soundness.SoundUnderAssumptions),

            // cotan(a) -> cos(a) / sin(a)
            new MatchedRule(
                "cotangent-is-cosine-over-sine",
                MatchPattern.Node<Cotanf>(MatchPattern.Any("a")),
                MatchPattern.Node<Divf>(
                    MatchPattern.Node<Cosf>(MatchPattern.Any("a")),
                    MatchPattern.Node<Sinf>(MatchPattern.Any("a"))),
                Soundness.SoundUnderAssumptions),

            // sec(a) -> 1 / cos(a)
            new MatchedRule(
                "secant-is-one-over-cosine",
                MatchPattern.Node<Secantf>(MatchPattern.Any("a")),
                MatchPattern.Node<Divf>(
                    MatchPattern.Exact(Integer.Create(1)),
                    MatchPattern.Node<Cosf>(MatchPattern.Any("a"))),
                Soundness.SoundUnderAssumptions),

            // cosec(a) -> 1 / sin(a)
            new MatchedRule(
                "cosecant-is-one-over-sine",
                MatchPattern.Node<Cosecantf>(MatchPattern.Any("a")),
                MatchPattern.Node<Divf>(
                    MatchPattern.Exact(Integer.Create(1)),
                    MatchPattern.Node<Sinf>(MatchPattern.Any("a"))),
                Soundness.SoundUnderAssumptions));

        /// <summary>
        /// <see cref="Functions.Patterns.PhiFunctionRules"/>, as data.
        /// </summary>
        /// <remarks>
        /// One rule, and the first whose <b>predicate on a hole is a mathematical property</b>
        /// rather than a sign or a type: <c>phi(p ^ k) = p ^ (k - 1) * (p - 1)</c> holds for a
        /// prime <c>p</c> and for no other integer, so primality is the condition and the hole
        /// carries it. The replacement is arithmetic on the bound prime, so it is code and the
        /// rule does not reverse.
        /// </remarks>
        internal static MatchedRuleSet PhiFunction { get; } = new(
            nameof(PhiFunction),

            // phi(p ^ k) -> p ^ (k - 1) * (p - 1), for a prime p
            new MatchedRule(
                "eulers-totient-of-a-prime-power",
                MatchPattern.Node<Phif>(
                    MatchPattern.Node<Powf>(
                        MatchPattern.Any<Integer>("p", p => p.IsPrime),
                        MatchPattern.Any("k"))),
                // (Integer) on the prime and not on the exponent, and the difference is the
                // point: p - 1 is arithmetic that folds to a number, k - 1 is a tree because k
                // is whatever was bound. Written without the cast the rule builds 2 ^ (5 - 1) *
                // (2 - 1) where the switch builds 2 ^ (5 - 1) * 1, which is what the agreement
                // test caught.
                bound => new Powf(bound["p"], bound["k"] - 1) * ((Integer)bound["p"] - 1),
                // Euler's product formula, which is a theorem and not an assumption: for a prime
                // p the integers below p^k sharing a factor with it are exactly the multiples of
                // p, of which there are p^(k-1).
                Soundness.Sound));

        /// <summary>
        /// <see cref="Functions.Patterns.CollapseTrigonometricFunctions"/>, as data.
        /// </summary>
        /// <remarks>
        /// The reverse of <see cref="NormalTrigonometricForm"/>, and the pair is worth having as
        /// data together: they are each other read backwards, and the type says so instead of a
        /// comment. Order is load-bearing — the two named quotients must be tried before the
        /// general reciprocal rules, or <c>sin(x) / cos(x)</c> becomes <c>sin(x) * sec(x)</c>
        /// rather than <c>tan(x)</c>.
        /// </remarks>
        internal static MatchedRuleSet CollapseTrigonometricFunctions { get; } = new(
            nameof(CollapseTrigonometricFunctions),

            // sin(a) / cos(a) -> tan(a). "a" is bound by the numerator and re-matched by the
            // denominator, which is how the pattern says "of the same argument".
            new MatchedRule(
                "sine-over-cosine-is-the-tangent",
                MatchPattern.Node<Divf>(
                    MatchPattern.Node<Sinf>(MatchPattern.Any("a")),
                    MatchPattern.Node<Cosf>(MatchPattern.Any("a"))),
                MatchPattern.Node<Tanf>(MatchPattern.Any("a")),
                Soundness.SoundUnderAssumptions),

            // cos(a) / sin(a) -> cotan(a)
            new MatchedRule(
                "cosine-over-sine-is-the-cotangent",
                MatchPattern.Node<Divf>(
                    MatchPattern.Node<Cosf>(MatchPattern.Any("a")),
                    MatchPattern.Node<Sinf>(MatchPattern.Any("a"))),
                MatchPattern.Node<Cotanf>(MatchPattern.Any("a")),
                Soundness.SoundUnderAssumptions),

            // a / sin(b) -> a * cosec(b)
            new MatchedRule(
                "a-quotient-by-a-sine-is-a-cosecant",
                MatchPattern.Node<Divf>(
                    MatchPattern.Any("a"),
                    MatchPattern.Node<Sinf>(MatchPattern.Any("b"))),
                MatchPattern.Node<Mulf>(
                    MatchPattern.Any("a"),
                    MatchPattern.Node<Cosecantf>(MatchPattern.Any("b"))),
                Soundness.SoundUnderAssumptions),

            // a / cos(b) -> a * sec(b)
            new MatchedRule(
                "a-quotient-by-a-cosine-is-a-secant",
                MatchPattern.Node<Divf>(
                    MatchPattern.Any("a"),
                    MatchPattern.Node<Cosf>(MatchPattern.Any("b"))),
                MatchPattern.Node<Mulf>(
                    MatchPattern.Any("a"),
                    MatchPattern.Node<Secantf>(MatchPattern.Any("b"))),
                Soundness.SoundUnderAssumptions));

        /// <summary>
        /// <see cref="Functions.Patterns.ExpandRules"/>, as data.
        /// </summary>
        /// <remarks>
        /// The angle-sum identities, and both sides of each are patterns — so read backwards they
        /// are the angle-<i>gathering</i> identities, which the <c>switch</c> would need a second
        /// set to say.
        /// </remarks>
        internal static MatchedRuleSet Expansion { get; } = new(
            nameof(Expansion),

            // sin(a + b) -> sin(a)cos(b) + sin(b)cos(a)
            new MatchedRule(
                "sine-of-a-sum",
                MatchPattern.Node<Sinf>(
                    MatchPattern.Node<Sumf>(MatchPattern.Any("a"), MatchPattern.Any("b"))),
                MatchPattern.Node<Sumf>(
                    MatchPattern.Node<Mulf>(
                        MatchPattern.Node<Sinf>(MatchPattern.Any("a")),
                        MatchPattern.Node<Cosf>(MatchPattern.Any("b"))),
                    MatchPattern.Node<Mulf>(
                        MatchPattern.Node<Sinf>(MatchPattern.Any("b")),
                        MatchPattern.Node<Cosf>(MatchPattern.Any("a")))),
                // Holds for every complex a and b: the identity is the addition theorem, which
                // has no branch to fall off.
                Soundness.Sound),

            // sin(a - b) -> sin(a)cos(b) - sin(b)cos(a)
            new MatchedRule(
                "sine-of-a-difference",
                MatchPattern.Node<Sinf>(
                    MatchPattern.Node<Minusf>(MatchPattern.Any("a"), MatchPattern.Any("b"))),
                MatchPattern.Node<Minusf>(
                    MatchPattern.Node<Mulf>(
                        MatchPattern.Node<Sinf>(MatchPattern.Any("a")),
                        MatchPattern.Node<Cosf>(MatchPattern.Any("b"))),
                    MatchPattern.Node<Mulf>(
                        MatchPattern.Node<Sinf>(MatchPattern.Any("b")),
                        MatchPattern.Node<Cosf>(MatchPattern.Any("a")))),
                Soundness.Sound));

        /// <summary>
        /// <see cref="Functions.Patterns.ExpandTrigonometricRules"/>, as data.
        /// </summary>
        internal static MatchedRuleSet ExpandTrigonometric { get; } = new(
            nameof(ExpandTrigonometric),

            // (1/2) * sin(2a) -> sin(a) * cos(a)
            new MatchedRule(
                "half-the-sine-of-a-doubled-angle",
                MatchPattern.Node<Mulf>(
                    MatchPattern.Exact(Rational.Create(1, 2)),
                    MatchPattern.Node<Sinf>(
                        MatchPattern.Node<Mulf>(
                            MatchPattern.Exact(Integer.Create(2)),
                            MatchPattern.Any("a")))),
                MatchPattern.Node<Mulf>(
                    MatchPattern.Node<Sinf>(MatchPattern.Any("a")),
                    MatchPattern.Node<Cosf>(MatchPattern.Any("a"))),
                Soundness.Sound),

            // cos(2a) -> cos(a)^2 - sin(a)^2
            new MatchedRule(
                "cosine-of-a-doubled-angle",
                MatchPattern.Node<Cosf>(
                    MatchPattern.Node<Mulf>(
                        MatchPattern.Exact(Integer.Create(2)),
                        MatchPattern.Any("a"))),
                MatchPattern.Node<Minusf>(
                    MatchPattern.Node<Powf>(
                        MatchPattern.Node<Cosf>(MatchPattern.Any("a")),
                        MatchPattern.Exact(Integer.Create(2))),
                    MatchPattern.Node<Powf>(
                        MatchPattern.Node<Sinf>(MatchPattern.Any("a")),
                        MatchPattern.Exact(Integer.Create(2)))),
                Soundness.Sound));

        /// <summary>
        /// <see cref="Functions.Patterns.ExpandMultipleAngleRules"/>, as data.
        /// </summary>
        /// <remarks>
        /// The replacement is a Chebyshev-style expansion computed from the multiplier, so it is
        /// code and the rule does not reverse. The predicate on the hole asks
        /// <see cref="Functions.Patterns.IsWorthExpanding"/> rather than repeating its bound,
        /// which is how two copies of a constant start disagreeing.
        /// </remarks>
        internal static MatchedRuleSet ExpandMultipleAngle { get; } = new(
            nameof(ExpandMultipleAngle),

            // sin(n * a) -> the expansion, for a whole n worth expanding
            new MatchedRule(
                "sine-of-a-whole-multiple-of-an-angle",
                MatchPattern.Node<Sinf>(
                    MatchPattern.Node<Mulf>(
                        MatchPattern.Any<Integer>("n", Functions.Patterns.IsWorthExpanding),
                        MatchPattern.Any("a"))),
                bound => TrigonometricAngleExpansion.ExpandSineArgumentMultiplied(
                    new Sinf(bound["a"]), new Cosf(bound["a"]),
                    ((Integer)bound["n"]).EInteger.ToInt32Checked()),
                Soundness.Sound),

            // cos(n * a) -> the expansion, for a whole n worth expanding
            new MatchedRule(
                "cosine-of-a-whole-multiple-of-an-angle",
                MatchPattern.Node<Cosf>(
                    MatchPattern.Node<Mulf>(
                        MatchPattern.Any<Integer>("n", Functions.Patterns.IsWorthExpanding),
                        MatchPattern.Any("a"))),
                bound => TrigonometricAngleExpansion.ExpandCosineArgumentMultiplied(
                    new Sinf(bound["a"]), new Cosf(bound["a"]),
                    ((Integer)bound["n"]).EInteger.ToInt32Checked()),
                Soundness.Sound));

        /// <summary>
        /// <see cref="Functions.Patterns.PolynomialLongDivision"/>, as data.
        /// </summary>
        /// <remarks>
        /// One rule with <b>no side condition</b>, deliberately. The work that decides whether it
        /// applies is the division itself, and asking it in a guard would run it twice on a path
        /// the simplifier takes for every quotient; the helper hands back the expression
        /// unchanged where it declines, which <see cref="MatchedRuleSet.ApplyHere"/> reads as no
        /// rewrite exactly as the <c>switch</c>'s own fallthrough does.
        /// </remarks>
        internal static MatchedRuleSet PolynomialLongDivision { get; } = new(
            nameof(PolynomialLongDivision),

            new MatchedRule(
                "a-quotient-of-polynomials-is-divided-out",
                MatchPattern.Node<Divf>(MatchPattern.Any("n"), MatchPattern.Any("d")),
                (node, bound) => TreeAnalyzer.PolynomialLongDivision(bound["n"], bound["d"])
                    is var (divided, remainder)
                    ? divided + remainder
                    : node,
                Soundness.SoundUnderAssumptions));

        /// <summary>
        /// <see cref="Functions.Patterns.PolynomialGcdCancellation"/>, as data.
        /// </summary>
        /// <remarks>
        /// No side condition, for the reason <see cref="PolynomialLongDivision"/> gives: the
        /// cancellation is the test.
        /// </remarks>
        internal static MatchedRuleSet PolynomialGcdCancellation { get; } = new(
            nameof(PolynomialGcdCancellation),

            new MatchedRule(
                "a-quotient-of-polynomials-is-put-in-lowest-terms",
                MatchPattern.Node<Divf>(MatchPattern.Any("n"), MatchPattern.Any("d")),
                (node, bound) => PolynomialGcd.TryCancel(bound["n"], bound["d"], out var cancelled)
                    ? cancelled
                    : node,
                Soundness.SoundUnderAssumptions));

        /// <summary>
        /// <see cref="Functions.Patterns.ExpandFactorialDivisions"/>, as data.
        /// </summary>
        /// <remarks>
        /// <b>Eight arms become three.</b> Four of the eight are one rule written for every way a
        /// sum can be spelled — <c>x + c</c> or <c>c + x</c>, on each side of the quotient — which
        /// is what <see cref="MatchPattern.Commutative{T}"/> says once. The other four are the
        /// cases where one factorial has no constant at all, and they stay separate because the
        /// constant they pass is <c>0</c> rather than a binding.
        /// </remarks>
        internal static MatchedRuleSet ExpandFactorialDivisions { get; } = new(
            nameof(ExpandFactorialDivisions),

            // (x + a)! / (y + b)!
            new MatchedRule(
                "a-quotient-of-shifted-factorials",
                MatchPattern.Node<Divf>(
                    MatchPattern.Node<Factorialf>(MatchPattern.Commutative<Sumf>(
                        MatchPattern.Any("x"), MatchPattern.Any<Number>("a"))),
                    MatchPattern.Node<Factorialf>(MatchPattern.Commutative<Sumf>(
                        MatchPattern.Any("y"), MatchPattern.Any<Number>("b")))),
                (node, bound) => Functions.Patterns.CancelFactorials(
                    node, bound["x"], bound["y"], (Number)bound["a"], (Number)bound["b"]),
                Soundness.SoundUnderAssumptions),

            // x! / (y + b)!
            new MatchedRule(
                "a-quotient-of-a-plain-factorial-by-a-shifted-one",
                MatchPattern.Node<Divf>(
                    MatchPattern.Node<Factorialf>(MatchPattern.Any("x")),
                    MatchPattern.Node<Factorialf>(MatchPattern.Commutative<Sumf>(
                        MatchPattern.Any("y"), MatchPattern.Any<Number>("b")))),
                (node, bound) => Functions.Patterns.CancelFactorials(
                    node, bound["x"], bound["y"], Integer.Create(0), (Number)bound["b"]),
                Soundness.SoundUnderAssumptions),

            // (x + a)! / y!
            new MatchedRule(
                "a-quotient-of-a-shifted-factorial-by-a-plain-one",
                MatchPattern.Node<Divf>(
                    MatchPattern.Node<Factorialf>(MatchPattern.Commutative<Sumf>(
                        MatchPattern.Any("x"), MatchPattern.Any<Number>("a"))),
                    MatchPattern.Node<Factorialf>(MatchPattern.Any("y"))),
                (node, bound) => Functions.Patterns.CancelFactorials(
                    node, bound["x"], bound["y"], (Number)bound["a"], Integer.Create(0)),
                Soundness.SoundUnderAssumptions));

        /// <summary>
        /// <see cref="Functions.Patterns.FactorizeFactorialMultiplications"/>, as data.
        /// </summary>
        /// <remarks>
        /// The same eight-into-three collapse as <see cref="ExpandFactorialDivisions"/>, and for
        /// the same reason.
        /// </remarks>
        internal static MatchedRuleSet FactorizeFactorialMultiplications { get; } = new(
            nameof(FactorizeFactorialMultiplications),

            // (x + a)! * (y + b)
            new MatchedRule(
                "a-shifted-factorial-times-the-next-term",
                MatchPattern.Node<Mulf>(
                    MatchPattern.Node<Factorialf>(MatchPattern.Commutative<Sumf>(
                        MatchPattern.Any("x"), MatchPattern.Any<Number>("a"))),
                    MatchPattern.Commutative<Sumf>(
                        MatchPattern.Any("y"), MatchPattern.Any<Number>("b"))),
                (node, bound) => Functions.Patterns.GatherFactorial(
                    node, bound["x"], bound["y"], (Number)bound["a"], (Number)bound["b"]),
                Soundness.SoundUnderAssumptions),

            // x! * (y + b)
            new MatchedRule(
                "a-plain-factorial-times-the-next-term",
                MatchPattern.Node<Mulf>(
                    MatchPattern.Node<Factorialf>(MatchPattern.Any("x")),
                    MatchPattern.Commutative<Sumf>(
                        MatchPattern.Any("y"), MatchPattern.Any<Number>("b"))),
                (node, bound) => Functions.Patterns.GatherFactorial(
                    node, bound["x"], bound["y"], Integer.Create(0), (Number)bound["b"]),
                Soundness.SoundUnderAssumptions),

            // (x + a)! * y
            new MatchedRule(
                "a-shifted-factorial-times-a-bare-term",
                MatchPattern.Node<Mulf>(
                    MatchPattern.Node<Factorialf>(MatchPattern.Commutative<Sumf>(
                        MatchPattern.Any("x"), MatchPattern.Any<Number>("a"))),
                    MatchPattern.Any("y")),
                (node, bound) => Functions.Patterns.GatherFactorial(
                    node, bound["x"], bound["y"], (Number)bound["a"], Integer.Create(0)),
                Soundness.SoundUnderAssumptions));

        /// <summary>
        /// <see cref="Functions.Patterns.PerfectSquareRules"/>, as data.
        /// </summary>
        /// <remarks>
        /// <para>
        /// One rule, and it is here to settle a question rather than for its own sake. The
        /// <c>switch</c> arm is <c>x is Sumf or Minusf</c> — an <b>alternation of node types</b>,
        /// which <see cref="MatchPattern.Node{T}"/> cannot say and which was recorded as needing
        /// an addition to the matcher.
        /// </para>
        /// <para>
        /// <b>It does not.</b> A typed hole with a predicate says it:
        /// <c>Any&lt;Entity&gt;(name, e =&gt; e is Sumf or Minusf)</c> matches either and binds the
        /// whole node, which is exactly what the arm does. The same shape covers every other
        /// construct on that list — <c>var x and not Integer(1)</c> is a predicate,
        /// <c>Rational and not Integer</c> is a predicate on a typed hole, and
        /// <c>not Set and not Matrix</c> is a predicate. So the matcher was never the thing
        /// standing in the way of those sets.
        /// </para>
        /// </remarks>
        internal static MatchedRuleSet PerfectSquare { get; } = new(
            nameof(PerfectSquare),

            new MatchedRule(
                "a-sum-or-difference-that-is-a-perfect-square",
                MatchPattern.Any<Entity>("x", node => node is Sumf or Minusf),
                (node, _) => Functions.Patterns.CollapseToPerfectSquare(node) ?? node,
                // sqrt(u)^2 is u for every complex u, so the identity itself is unconditional.
                // What is not is the test for whether the cross term matches, which needs
                // Simplify -- see the remark on Patterns.CollapseToPerfectSquare.
                Soundness.SoundUnderAssumptions));

        /// <summary>
        /// <see cref="Functions.Patterns.RationalizeDenominator"/>, as data.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This set is why <see cref="MatchedRuleSet.AsAddressable"/> exists. It is an ordinary
        /// method with branches and locals rather than a <c>switch</c>, so
        /// <c>RuleRegistryGenerator</c> declines it — and it was the one set in the registry with
        /// <b>no addressable rules at all</b>. Nothing about it could be listed, named in a
        /// report or checked by the tooling that reads arms.
        /// </para>
        /// <para>
        /// Neither rule carries a side condition, for the reason
        /// <see cref="PolynomialLongDivision"/> gives: the work that decides whether the rewrite
        /// applies <i>is</i> the rewrite, and each helper hands the expression back where it
        /// declines.
        /// </para>
        /// </remarks>
        internal static MatchedRuleSet RationalizeDenominator { get; } = new(
            nameof(RationalizeDenominator),

            // k * (value / d) -> (k * value) / d, where the numerator carries a surd
            new MatchedRule(
                "a-numeric-coefficient-is-gathered-over-a-surd",
                MatchPattern.Node<Mulf>(
                    MatchPattern.Any<Rational>("k"),
                    MatchPattern.Node<Divf>(MatchPattern.Any("value"), MatchPattern.Any<Rational>("d"))),
                (node, _) => Functions.Patterns.GatherNumericCoefficientOverASurd(node),
                Soundness.SoundUnderAssumptions),

            // num / (a + b) -> num * (a - b) / (a^2 - b^2)
            new MatchedRule(
                "a-two-term-denominator-is-multiplied-by-its-conjugate",
                MatchPattern.Node<Divf>(MatchPattern.Any("num"), MatchPattern.Any("den")),
                (node, _) => Functions.Patterns.MultiplyByTheConjugate(node),
                Soundness.SoundUnderAssumptions));

        /// <summary>
        /// <see cref="Functions.Patterns.NumericNeatRules"/>, as data.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Sixteen arms become eleven.</b> Six of the sixteen are three rules written twice, once
        /// for each side a negative factor can sit on inside a product — which is
        /// <see cref="MatchPattern.Commutative{T}"/> — and two more are the one-sided sum and
        /// difference rules, whose two spellings are one rule about "the other operand".
        /// </para>
        /// <para>
        /// <b>Order is load-bearing and is kept exactly.</b> The both-negative rules come first,
        /// because a commutative one-sided rule matches a both-negative sum too and would take
        /// it. Every replacement negates a bound number, so all eleven are one-way.
        /// </para>
        /// </remarks>
        internal static MatchedRuleSet NumericNeat { get; } = new(
            nameof(NumericNeat),

            // (-a) + (-b) -> -(a + b), where a and b are the magnitudes
            new MatchedRule(
                "a-sum-of-two-negatives-is-a-negated-sum",
                MatchPattern.Node<Sumf>(
                    MatchPattern.Any<Real>("l", value => value.IsNegative),
                    MatchPattern.Any<Real>("r", value => value.IsNegative)),
                bound => -((-(Real)bound["l"]) + (Entity)(-(Real)bound["r"])),
                Soundness.Sound),

            // x + (-a) -> x - a, either way round
            new MatchedRule(
                "a-negative-added-is-subtracted",
                MatchPattern.Commutative<Sumf>(
                    MatchPattern.Any("x"),
                    MatchPattern.Any<Real>("n", value => value.IsNegative)),
                bound => bound["x"] - -(Real)bound["n"],
                Soundness.Sound),

            // (-a) - (-b) -> b - a
            new MatchedRule(
                "a-difference-of-two-negatives-turns-round",
                MatchPattern.Node<Minusf>(
                    MatchPattern.Any<Real>("l", value => value.IsNegative),
                    MatchPattern.Any<Real>("r", value => value.IsNegative)),
                bound => (-(Real)bound["r"]) - (Entity)(-(Real)bound["l"]),
                Soundness.Sound),

            // x - (-a) -> x + a
            new MatchedRule(
                "a-negative-subtracted-is-added",
                MatchPattern.Node<Minusf>(
                    MatchPattern.Any("x"),
                    MatchPattern.Any<Real>("n", value => value.IsNegative)),
                bound => bound["x"] + -(Real)bound["n"],
                Soundness.Sound),

            // (-a) - x -> -(x + a)
            new MatchedRule(
                "a-negative-minuend-comes-out-in-front",
                MatchPattern.Node<Minusf>(
                    MatchPattern.Any<Real>("n", value => value.IsNegative),
                    MatchPattern.Any("x")),
                bound => -(bound["x"] + -(Real)bound["n"]),
                Soundness.Sound),

            // (-a) * (-b) -> a * b
            new MatchedRule(
                "a-product-of-two-negatives-is-positive",
                MatchPattern.Node<Mulf>(
                    MatchPattern.Any<Real>("l", value => value.IsNegative),
                    MatchPattern.Any<Real>("r", value => value.IsNegative)),
                bound => (-(Real)bound["l"]) * (Entity)(-(Real)bound["r"]),
                Soundness.Sound),

            // (-a) / (-b) -> a / b
            new MatchedRule(
                "a-quotient-of-two-negatives-is-positive",
                MatchPattern.Node<Divf>(
                    MatchPattern.Any<Real>("l", value => value.IsNegative),
                    MatchPattern.Any<Real>("r", value => value.IsNegative)),
                bound => (-(Real)bound["l"]) / (Entity)(-(Real)bound["r"]),
                Soundness.Sound),

            // (-a * x) * y -> -(a * (x * y)), the negative factor either way round
            new MatchedRule(
                "a-negative-factor-in-a-left-product-comes-out",
                MatchPattern.Node<Mulf>(
                    MatchPattern.Commutative<Mulf>(
                        MatchPattern.Any<Real>("n", value => value.IsNegative),
                        MatchPattern.Any("x")),
                    MatchPattern.Any("y")),
                bound => -((-(Real)bound["n"]) * (bound["x"] * bound["y"])),
                Soundness.Sound),

            // (-a * x) / y -> -(a * (x / y))
            new MatchedRule(
                "a-negative-factor-in-a-numerator-comes-out",
                MatchPattern.Node<Divf>(
                    MatchPattern.Commutative<Mulf>(
                        MatchPattern.Any<Real>("n", value => value.IsNegative),
                        MatchPattern.Any("x")),
                    MatchPattern.Any("y")),
                bound => -((-(Real)bound["n"]) * (bound["x"] / bound["y"])),
                Soundness.Sound),

            // y * (-a * x) -> -(a * (x * y))
            new MatchedRule(
                "a-negative-factor-in-a-right-product-comes-out",
                MatchPattern.Node<Mulf>(
                    MatchPattern.Any("y"),
                    MatchPattern.Commutative<Mulf>(
                        MatchPattern.Any<Real>("n", value => value.IsNegative),
                        MatchPattern.Any("x"))),
                bound => -((-(Real)bound["n"]) * (bound["x"] * bound["y"])),
                Soundness.Sound),

            // y / (-a * x) -> -(y / (a * x)). What is left stays under the line: written the
            // other way, as the numerator rules above are, the quotient came back inverted --
            // https://github.com/asc-community/AngouriMath/issues/936 and the note on the switch.
            new MatchedRule(
                "a-negative-factor-in-a-denominator-comes-out",
                MatchPattern.Node<Divf>(
                    MatchPattern.Any("y"),
                    MatchPattern.Commutative<Mulf>(
                        MatchPattern.Any<Real>("n", value => value.IsNegative),
                        MatchPattern.Any("x"))),
                bound => -(bound["y"] / ((-(Real)bound["n"]) * bound["x"])),
                Soundness.Sound));
    }
}
