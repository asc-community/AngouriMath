//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Multithreading;
using PeterO.Numbers;
using System;
using System.Linq;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;

namespace AngouriMath.Functions.Algebra
{
    partial class LimitFunctional
    {
        private static Entity EquivalenceRules(Entity expr, Variable x, Entity dest)
            => expr switch
            {
                Sinf or Tanf or Arcsinf or Arctanf => expr.DirectChildren[0],
                _ => expr
            };
        private static Entity EvalAssumingContinuous(Entity expr) =>
            expr.Evaled switch
            {
                Providedf(var inner, _) => inner,
                var x => x
            };
        private static Entity ApplyFirstRemarkable(Entity expr, Variable x, Entity dest)
            => expr switch
            {
                Divf(var a, var b) div
                    when EvalAssumingContinuous(a.Limit(x, dest)) == 0 && EvalAssumingContinuous(b.Limit(x, dest)) == 0
                        => div.New(EquivalenceRules(a, x, dest), EquivalenceRules(b, x, dest)),

                _ => expr
            };

        private static Entity ApplySecondRemarkable(Entity expr, Variable x, Entity dest)
            => expr switch
            {
                // f(x)^g(x) for f(x) -> 1, g(x) -> +oo
                // => (1 + (f(x) - 1)) ^ g(x) = ((1 - (f(x) - 1)) ^ (1 / (f(x) - 1))) ^ (g(x) (f(x) - 1))
                // e ^ (g(x) * (f(x) - 1))
                Powf(var xPlusOne, var xPower) when
                xPlusOne.ContainsNode(x) && xPower.ContainsNode(x) &&
                EvalAssumingContinuous((xPlusOne - 1).Limit(x, dest)) == 0 && DivergesInMagnitude(xPower, x, dest) =>
                MathS.e.Pow(xPower * (xPlusOne - 1)),

                _ => expr
            };

        private static bool IsInfiniteNode(Entity expr)
            => expr.ContainsNode("+oo") || expr.ContainsNode("-oo"); // TODO: is it correct?

        /// <summary>
        /// Whether the exponent grows without bound, which is all the second remarkable
        /// limit needs.
        /// </summary>
        /// <remarks>
        /// Asking only for the two-sided limit is not enough: at <c>x -> 0</c> the
        /// exponent <c>1/x</c> tends to -oo on the left and +oo on the right, so the
        /// two-sided limit does not exist even though the magnitude diverges. That made
        /// <c>lim x-&gt;0 (1 + x)^(1/x)</c> answer 1 rather than e. Both one-sided
        /// limits diverging is enough, because the rewrite below only uses the product
        /// <c>g(x) * (f(x) - 1)</c>, which is well defined from either side.
        /// </remarks>
        private static bool DivergesInMagnitude(Entity power, Variable x, Entity dest)
        {
            if (IsInfiniteNode(power.Limit(x, dest)))
                return true;
            // Only worth asking one side at a time where there are two sides to ask about.
            // Approaching an infinite destination there is only one, so the two further
            // limits would be wasted work and would not mean anything either.
            if (IsInfiniteNode(dest))
                return false;
            return IsInfiniteNode(power.Limit(x, dest, ApproachFrom.Left))
                && IsInfiniteNode(power.Limit(x, dest, ApproachFrom.Right));
        }

        private static bool IsFiniteNode(Entity expr)
            => !IsInfiniteNode(expr) && expr != MathS.NaN;

        /// <summary>
        /// How many derivatives of the divisor to take looking for the order at which it
        /// vanishes. A divisor that is still flat after four of them is one whose sign this has
        /// no cheap reading of, and reading it wrongly would answer +oo where the truth is -oo,
        /// which is worse than not answering. The forms this is for vanish at the first order
        /// (sin(x), e^x - 1, x - a) or the second (1 - cos(x), x^2).
        /// </summary>
        private const int MaxVanishingOrder = 4;

        /// <summary>
        /// The infinity a quotient tends to when its divisor vanishes and its dividend does not,
        /// or <see langword="null"/> where that is not the shape or the side the divisor
        /// vanishes from cannot be read off.
        /// </summary>
        /// <remarks>
        /// The descent puts each part's own limit in place of the part, and for this shape that
        /// throws away the only thing that decides the answer. <c>cos(x) / sin(x)</c> at 0
        /// becomes <c>1 / 0</c>, which is NaN -- the claim that the limit does not exist -- where
        /// on the right it is +oo and on the left -oo.
        /// <para/>
        /// The side the divisor vanishes from is read off its first derivative that does not
        /// vanish with it. Where <c>g(a) = 0</c> and the first non-vanishing derivative there is
        /// the k-th, <c>g(x)</c> has the sign of <c>g_k(a) * (x - a)^k</c> near a, which is the
        /// sign of <c>g_k(a)</c> on the right and that times <c>(-1)^k</c> on the left. Nothing
        /// is claimed unless a derivative comes out finite and non-zero at the point: an
        /// expression that is not differentiable there, or whose derivative diverges as
        /// <c>sqrt(x)</c>'s does, is left alone.
        /// </remarks>
        internal static Entity? DivergesAtAVanishingDivisor(Entity dividend, Entity divisor, Variable x, Entity dest, ApproachFrom side)
        {
            if (side is not (ApproachFrom.Left or ApproachFrom.Right) || !dest.IsFinite || !divisor.ContainsNode(x))
                return null;
            if (EvalAssumingContinuous(divisor.Limit(x, dest, side)) is not Real { IsZero: true })
                return null;
            // A dividend that vanishes too makes the quotient indeterminate rather than
            // divergent, and one that diverges is a different question again. Only a dividend
            // with a definite non-zero size leaves the divisor to decide the answer.
            if (EvalAssumingContinuous(dividend.Limit(x, dest, side)) is not Real { IsFinite: true, IsZero: false } dividendLimit)
                return null;

            var derivative = divisor;
            for (var order = 1; order <= MaxVanishingOrder; order++)
            {
                MultithreadingFunctional.ExitIfCancelled();
                derivative = derivative.Differentiate(x).Simplify();
                var atThePoint = EvalAssumingContinuous(derivative.Substitute(x, dest).InnerSimplified);
                if (atThePoint is not Real { IsFinite: true } value)
                    return null;
                if (value.IsZero)
                    continue;
                // (x - a)^k is positive on the right whatever k is, and on the left it takes the
                // sign of (-1)^k, so approaching from the left at an odd order turns the sign of
                // the derivative around and nothing else does.
                var turnsAround = side is ApproachFrom.Left && order % 2 != 0;
                var divisorIsPositive = value.IsNegative == turnsAround;
                return dividendLimit.IsNegative == divisorIsPositive
                    ? Real.NegativeInfinity
                    : Real.PositiveInfinity;
            }
            return null;
        }

        /// <summary>
        /// How deep the rule may be applied to one limit. Differentiating both parts does not
        /// always make the quotient simpler -- x / sqrt(x^2 + 1) turns into sqrt(x^2 + 1) / x,
        /// which turns back, so the two stay indeterminate forever -- and without a bound the
        /// recursion below would not end. The bound has to leave room for the degree of a
        /// polynomial: x^10 / e^x takes ten steps.
        /// </summary>
        private const int MaxlHopitalDepth = 16;

        [ThreadStatic] private static int lHopitalDepth;

        /// <summary>
        /// How many times in all the rule may differentiate a quotient while one limit is being
        /// computed. Bounding the depth alone does not bound the work: each step asks what the
        /// two parts of its quotient tend to, and those are limits the rule may be applied to
        /// in turn, so the steps fan out rather than follow one another, and sixteen deep is
        /// far more than sixteen of them. This is a backstop rather than the answer to that --
        /// the two conditions below are what keep the fan-out from starting.
        /// </summary>
        private const int MaxlHopitalApplications = 64;

        [ThreadStatic] private static int lHopitalApplications;

        [ThreadStatic] private static List<Entity>? lHopitalChain;

        /// <summary>
        /// Whether the rule is already partway through this very quotient. Differentiating both
        /// parts of sqrt(x^2 - x) / x gives its reciprocal, and differentiating that gives the
        /// first back, so the rule can go round for as long as the bound on its depth allows.
        /// Each step of the way costs a simplification and two limits of its own, which is why
        /// bounding the depth is not by itself enough to stop that limit running for a minute.
        /// </summary>
        private static bool AlreadyBeingDifferentiated(Entity quotient)
            => (lHopitalChain ??= new()).Contains(quotient);

        /// <summary>
        /// Whether differentiating both parts has left a quotient bigger than the one it came
        /// from by more than the derivative of a power or a logarithm accounts for. Each step
        /// asks what the two parts of its quotient tend to, and a bigger quotient makes those
        /// two questions harder than the one being answered, so a step that grows is a step
        /// away from an answer rather than towards one. The room left over is measured: the
        /// steps that do reach an answer add two nodes, and the most any of them was seen to
        /// add is six, while one step of x^(3/2) * sqrt(1 + 1/x^2) / x^2 adds eighteen and
        /// leaves the rule tens of seconds of work that ends in nothing.
        /// </summary>
        private static bool GrewTooMuch(Entity quotient, Entity applied)
            => applied.Nodes.Count() > quotient.Nodes.Count() + 8;

        [ThreadStatic] private static bool suppresslHopital;

        /// <summary>
        /// One product broken into a numerator and a denominator, with every reciprocal factor
        /// taken into the latter. A product with no reciprocal factor in it comes back with a
        /// denominator of 1, which is what makes this usable term by term below.
        /// </summary>
        private static (Entity Numerator, Entity Denominator) SplitProduct(Entity expr)
        {
            Entity numerator = 1, denominator = 1;
            foreach (var factor in Mulf.LinearChildren(expr))
                switch (factor)
                {
                    case Powf(var @base, Real { IsNegative: true } power):
                        denominator *= @base.Pow(-power);
                        break;
                    case Powf(var @base, Mulf(Real { IsNegative: true } coefficient, var rest)):
                        denominator *= @base.Pow(-coefficient * rest);
                        break;
                    case Divf(var dividend, var divisor):
                        numerator *= dividend;
                        denominator *= divisor;
                        break;
                    default:
                        numerator *= factor;
                        break;
                }
            return (numerator, denominator);
        }

        /// <summary>
        /// How many terms of a sum are worth putting over a common denominator. Each one
        /// multiplies the numerator by every other term's denominator, so the expression grows
        /// with the square of the count, and the rule below has to differentiate whatever comes
        /// out of it. Two and three terms is what the forms that need this are written with.
        /// </summary>
        private const int MaxTermsOverACommonDenominator = 3;

        /// <summary>
        /// The expression rewritten as a single quotient, or <see langword="null"/> where it is
        /// not one to begin with and nothing is gained by writing it as one. The rule below only
        /// reads quotients, and two kinds of expression are quotients without being written as
        /// one.
        /// <para/>
        /// A product with a reciprocal factor: <c>lim x -&gt; +oo x^4 * e^(-x)</c> came out as
        /// NaN while the same limit written <c>x^4 / e^x</c> gave 0 --
        /// https://github.com/asc-community/AngouriMath/issues/596.
        /// <para/>
        /// And a sum of them, which is where the differences of two divergent parts at a finite
        /// point live: <c>1/x - 1/sin(x)</c> at 0 is +oo - +oo on the right and -oo - -oo on the
        /// left, and nothing in the descent takes a difference apart any further than asking
        /// what each half tends to. Over the common denominator it is
        /// <c>(sin(x) - x) / (x * sin(x))</c>, which is 0/0 and which the rule settles at 0 in
        /// three steps.
        /// </summary>
        private static Entity? AsQuotient(Entity expr, Variable x)
        {
            if (expr is Mulf)
            {
                var (numerator, denominator) = SplitProduct(expr);
                return denominator == 1 ? null : numerator / denominator;
            }
            if (expr is not (Sumf or Minusf))
                return null;
            var terms = Sumf.LinearChildren(expr).ToList();
            if (terms.Count > MaxTermsOverACommonDenominator)
                return null;
            Entity? combined = null, common = null;
            var worthIt = false;
            foreach (var term in terms)
            {
                var (numerator, denominator) = SplitProduct(term);
                // A denominator that does not contain x is a constant factor and putting the sum
                // over it gains nothing, while still costing the rule an expression to
                // differentiate. It is the denominators that vanish or diverge that make the
                // difference indeterminate, and those are the ones that contain x.
                worthIt |= denominator != 1 && denominator.ContainsNode(x);
                (combined, common) = combined is null || common is null
                    ? (numerator, denominator)
                    : (combined * denominator + numerator * common, common * denominator);
            }
            return worthIt && combined is { } && common is { } ? (combined / common).InnerSimplified : null;
        }

        /// <remarks>
        /// The side is carried through because the rule is stated one-sidedly to begin with --
        /// the two-sided case is the two one-sided ones agreeing -- so it is the same rule
        /// either way, asked of the same quotient under the same premises. Carrying it is what
        /// lets the rule answer where the two-sided reading has nothing to say, and the two
        /// questions the rule asks along the way are both of that kind: what the parts tend to,
        /// and what the differentiated quotient tends to. For <c>(1 - cos(x)) / x^3</c> at 0
        /// the second of those reaches <c>sin(x) / (3x^2)</c>, which is +oo on the right and
        /// -oo on the left, so asking about both sides at once gives NaN and the step is
        /// wasted. Asked one side at a time it answers.
        /// </remarks>
        private static Entity? ApplylHopitalRule(Entity expr, Variable x, Entity dest, ApproachFrom side = ApproachFrom.BothSides)
        {
            if (lHopitalDepth == 0)
                lHopitalApplications = 0;
            if (lHopitalDepth >= MaxlHopitalDepth || lHopitalApplications >= MaxlHopitalApplications || suppresslHopital)
                return null;
            // Held for the whole of the rule and not just for the recursive call below, since
            // asking what the two parts tend to is itself a limit that the rule may be applied
            // to, and counting only the recursion left those two free to nest without a bound.
            lHopitalDepth++;
            try { return ApplylHopitalRuleImpl(expr, x, dest, side); }
            finally { lHopitalDepth--; }
        }

        private static Entity? ApplylHopitalRuleImpl(Entity expr, Variable x, Entity dest, ApproachFrom side)
        {
            if (expr is not Divf && AsQuotient(expr, x) is { } quotient)
                expr = quotient;
            if (expr is Divf(var num, var den))
                if (EvalAssumingContinuous(num.Limit(x, dest, side)) is var numLimit && EvalAssumingContinuous(den.Limit(x, dest, side)) is var denLimit)
                    if (numLimit == 0 && denLimit == 0 ||
                            IsInfiniteNode(numLimit) && IsInfiniteNode(denLimit))
                        if (num is not Number && den is not Number)
                            if (num.ContainsNode(x) && den.ContainsNode(x))
                            {
                                // Simplified, because the shape of the quotient is what the
                                // machinery below matches on and differentiation does not
                                // produce it: d/dx ln(x)^2 is 2 * ln(x) * (1 / x), a product
                                // with a quotient inside rather than the quotient
                                // 2 * ln(x) / x, and only the second has a limit here.
                                var applied = (num.Differentiate(x) / den.Differentiate(x)).Simplify();
                                // The domain condition simplification leaves behind is what the
                                // derivative of a logarithm or a root carries, and a Providedf is
                                // not a continuous node, so the quotient would be turned away
                                // unread. Limits already treat the expression as continuous, as
                                // SimplifyAndComputeLimitToInfinity does with the same shape.
                                while (applied is Providedf(var body, _)) applied = body;
                                if (AlreadyBeingDifferentiated(applied) || GrewTooMuch(expr, applied))
                                    return null;
                                lHopitalApplications++;
                                MultithreadingFunctional.ExitIfCancelled();
                                lHopitalChain!.Add(applied);
                                try
                                {
                                    if (ComputeLimit(applied, x, dest, side) is { } resLim)
                                        return resLim;
                                }
                                finally { lHopitalChain.RemoveAt(lHopitalChain.Count - 1); }
                            }
            return null;
        }

        /// <summary>
        /// How deep one limit may go into rewriting itself. Each of the rewrites below hands
        /// back another limit to take, and that one is entitled to be rewritten in turn, so
        /// without a bound the work would multiply.
        /// </summary>
        private const int MaxRewriteDepth = 2;

        [ThreadStatic] private static int rewriteDepth;

        /// <summary>
        /// The readings that need the expression rewritten before any solver can see anything
        /// in it. Every one of them costs an expansion or a simplification of the whole
        /// expression, which is why this sits here rather than in the descent that visits each
        /// of its parts, and why it is reached only once it is settled that the expression as
        /// written has no answer.
        /// </summary>
        private static Entity? SolveByRewriting(Entity expr, Variable x, Entity dest)
        {
            if (rewriteDepth >= MaxRewriteDepth)
                return null;
            // -oo is +oo with -x written for x, the same substitution the solvers are handed.
            // Reading the growth of a root off x^d depends on it: x^d is positive in the one
            // direction only.
            if (dest.Evaled is Real { IsNegative: true })
                expr = expr.Substitute(x, -x);
            var toInfinity = Real.PositiveInfinity;
            var simplified = expr.Simplify();
            if (simplified is Providedf(var body, _))
                simplified = body;

            rewriteDepth++;
            try
            {
                // Simplification can hand back an expression of another kind altogether, and
                // which parts a limit is broken into is decided by that kind:
                // sqrt(x^2 + 1) / sqrt(x^2 + 3x) is broken up as a quotient, while what
                // simplifying gives is the single root sqrt((x^2 + 1) / (x^2 + 3x)), whose
                // argument can be read straight off.
                if (simplified.GetType() != expr.GetType()
                    && Settled(simplified.ComputeLimitDivideEtImpera(x, toInfinity, ApproachFrom.Left)) is { } byShape)
                    return byShape;

                if (ExtractRadicalGrowth(simplified, x) is { } extracted
                    && Settled(extracted.ComputeLimitDivideEtImpera(x, toInfinity, ApproachFrom.Left)) is { } byGrowth)
                    return byGrowth;

                return Settled(SolveAsDifferenceOfInfinities(simplified, x));
            }
            finally { rewriteDepth--; }

            // Rewriting brings domain conditions with it -- dividing by x^d is only the same
            // expression where x is not zero -- and a limit is taken of a continuous
            // expression regardless of the points where it is undefined, as the solvers
            // themselves do with the same shape.
            static Entity? Settled(Entity? limit)
            {
                while (limit is Providedf(var inner, _)) limit = inner;
                return limit is null || limit.Evaled == MathS.NaN ? null : limit;
            }
        }

        /// <summary>
        /// The whole expression with every root of a polynomial rewritten so that its growth is
        /// a factor of its own -- <c>sqrt(x^2 + x)</c> becomes <c>x * sqrt(1 + 1/x)</c> -- or
        /// <see langword="null"/> if it has no such root. Every solver reads a polynomial or a
        /// substitution of +oo, and neither can say anything about a root of a sum, so
        /// <c>lim x -&gt; +oo sqrt(x^2 + x) / x</c> was left unevaluated while the rewritten
        /// <c>sqrt(1 + 1/x)</c> is settled by substitution alone.
        /// </summary>
        /// <remarks>
        /// <c>P = x^d * (P / x^d)</c>, and <c>(uv)^r = u^r v^r</c> needs <c>u</c> to be positive,
        /// which <c>x^d</c> is for every x past some point on the way to +oo. That is the only
        /// direction this is used in: a destination of -oo has already been turned into +oo by
        /// substituting -x for x before any of this runs.
        /// </remarks>
        private static Entity? ExtractRadicalGrowth(Entity expr, Variable x)
        {
            if (!expr.Nodes.Any(IsRootOfASum))
                return null;
            var extracted = expr.Replace(RewriteRoot);
            return extracted == expr ? null : extracted.Simplify();

            bool IsRootOfASum(Entity node)
                => node is Powf(Sumf or Minusf, Number.Rational and not Number.Integer);

            Entity RewriteRoot(Entity node)
            {
                if (!IsRootOfASum(node)
                    || node is not Powf(var @base, var power)
                    || !TreeAnalyzer.TryGetPolynomial(@base, x, out var monomials))
                    return node;
                var degree = monomials.Keys.Aggregate(EInteger.Zero, EInteger.Max);
                if (degree.CompareTo(EInteger.One) < 0)
                    return node;
                var growth = MathS.Pow(x, Number.Integer.Create(degree));
                return MathS.Pow(x, Number.Integer.Create(degree) * power) * MathS.Pow((@base / growth).Simplify(), power);
            }
        }

        /// <summary>
        /// How many times a single limit may be broken down as a difference of two divergent
        /// parts. The conjugate below turns one difference into another, and every part of it
        /// is a limit in its own right, so without a bound the work would multiply.
        /// </summary>
        private const int MaxDifferenceDepth = 2;

        [ThreadStatic] private static int differenceDepth;

        /// <summary>
        /// Which infinity an already computed limit is, or 0 if it is finite or not a number.
        /// </summary>
        private static int InfiniteSign(Entity? limit)
            => limit?.Evaled is Real { IsFinite: false, IsNaN: false } real ? (real.IsNegative ? -1 : 1) : 0;

        /// <summary>
        /// oo - oo, which says nothing on its own: whichever of the two grows faster decides
        /// the answer, and if neither does the difference can still be finite. Two of the
        /// standard readings are covered -- one part outgrowing the other, and the conjugate
        /// for a difference containing a root.
        /// </summary>
        private static Entity? SolveAsDifferenceOfInfinities(Entity expr, Variable x)
        {
            if (differenceDepth >= MaxDifferenceDepth || expr is not (Sumf or Minusf))
                return null;
            var terms = Sumf.LinearChildren(expr).ToArray();
            if (terms.Length != 2)
                return null;
            var dest = Real.PositiveInfinity;
            differenceDepth++;
            try
            {
                var (firstSign, secondSign) =
                    (InfiniteSign(ComputeLimit(terms[0], x, dest)), InfiniteSign(ComputeLimit(terms[1], x, dest)));
                if (firstSign == 0 || secondSign != -firstSign)
                    return null;
                // Written as minuend - subtrahend with both parts tending to +oo, so that the
                // reading below does not have to carry the signs around with it.
                var (minuend, subtrahend) = firstSign > 0
                    ? (terms[0], -terms[1])
                    : (terms[1], -terms[0]);

                // The faster growing part decides the answer whenever there is one, which is
                // what the ratio of the two says: lim x -> +oo e^x - x is +oo because x / e^x
                // tends to 0, and -oo the other way round. This settles nothing when the two
                // grow alike, and the ratio then tends to 1 rather than to 0 or to infinity.
                if (ComputeLimit((subtrahend / minuend).Simplify(), x, dest) is { } ratio)
                {
                    if (ratio.Evaled == Number.Integer.Zero)
                        return Real.PositiveInfinity;
                    if (InfiniteSign(ratio) > 0)
                        return Real.NegativeInfinity;
                }

                // a - b = (a^2 - b^2) / (a + b), an identity wherever a + b is not zero, which
                // it is not on the way to +oo. It is only an improvement when squaring removes
                // a root, and then the leading terms cancel in the numerator and what is left
                // is an ordinary quotient: sqrt(x^2 + x) - x becomes x / (sqrt(x^2 + x) + x).
                if (!ContainsRadical(minuend, x) && !ContainsRadical(subtrahend, x))
                    return null;
                MultithreadingFunctional.ExitIfCancelled();
                var conjugate = ((minuend * minuend - subtrahend * subtrahend) / (minuend + subtrahend)).Simplify();
                if (conjugate == expr)
                    return null;
                // Without the roots the numerator is an ordinary polynomial and the denominator
                // is what it was, so the quotient either falls to the solvers directly or it is
                // no better than what it replaced. Differentiating it repeatedly is a long way
                // round to the same nothing, and it is most of the cost here.
                suppresslHopital = true;
                try { return ComputeLimit(conjugate, x, dest); }
                finally { suppresslHopital = false; }
            }
            finally { differenceDepth--; }
        }

        private static bool ContainsRadical(Entity expr, Variable x)
            => expr.Nodes.Any(node =>
                node is Powf(var @base, Number.Rational and not Number.Integer) && @base.ContainsNode(x));

        private static Entity ApplyTrivialTransformations(Entity expr, Variable x, Entity dest, Func<Entity, Entity, Entity> transformation)
            => expr switch
            {
                Sumf(var a, var b)
                    when ComputeLimit(a, x, dest) is { } aLim && ComputeLimit(b, x, dest) is { } bLim &&
                        IsFiniteNode(aLim.Evaled) && IsFiniteNode(bLim.Evaled)
                        => transformation(a, aLim) + transformation(b, bLim),
                Minusf(var a, var b)
                    when ComputeLimit(a, x, dest) is { } aLim && ComputeLimit(b, x, dest) is { } bLim &&
                        IsFiniteNode(aLim.Evaled) && IsFiniteNode(bLim.Evaled)
                        => transformation(a, aLim) - transformation(b, bLim),
                Mulf(var a, var b)
                    when ComputeLimit(a, x, dest) is { } aLim && ComputeLimit(b, x, dest) is { } bLim &&
                        IsFiniteNode(aLim.Evaled) && IsFiniteNode(bLim.Evaled)
                        => transformation(a, aLim) * transformation(b, bLim),
                _ => expr
            };

        /// <summary>
        /// A tangent or a cotangent written as the quotient it is. Kept apart from
        /// <see cref="TrivialTrigonometricReplacement"/>, which runs in front of the first
        /// remarkable limit, because that limit matches a quotient and this rewrite would turn
        /// the quotient a tangent sits under into a product.
        /// </summary>
        private static Entity AsSineOverCosine(Entity expr, Variable x)
            => expr switch
            {
                Tanf(var arg) when arg.ContainsNode(x) => MathS.Sin(arg) / MathS.Cos(arg),
                Cotanf(var arg) when arg.ContainsNode(x) => MathS.Cos(arg) / MathS.Sin(arg),
                _ => expr
            };

        private static Entity TrivialTrigonometricReplacement(Entity expr, Variable x)
            => expr switch
            {
                Secantf(var arg) when arg.ContainsNode(x) => 1 / MathS.Cos(arg),
                Cosecantf(var arg) when arg.ContainsNode(x) => 1 / MathS.Sin(arg),
                _ => expr
            };

    }
}
