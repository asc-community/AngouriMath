//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Multithreading;
using System;
using System.Linq;
using static AngouriMath.Entity;

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

        /// <summary>
        /// A product that has a reciprocal factor in it, rewritten as a quotient, or
        /// <see langword="null"/> if it has none. The rule below only reads quotients, so
        /// <c>lim x -&gt; +oo x^4 * e^(-x)</c> came out as NaN while the same limit written
        /// <c>x^4 / e^x</c> gave 0 --
        /// https://github.com/asc-community/AngouriMath/issues/596.
        /// </summary>
        private static Entity? AsQuotient(Entity expr)
        {
            if (expr is not Mulf)
                return null;
            Entity numerator = 1, denominator = 1;
            var reciprocal = false;
            foreach (var factor in Mulf.LinearChildren(expr))
                switch (factor)
                {
                    case Powf(var @base, Real { IsNegative: true } power):
                        denominator *= @base.Pow(-power);
                        reciprocal = true;
                        break;
                    case Powf(var @base, Mulf(Real { IsNegative: true } coefficient, var rest)):
                        denominator *= @base.Pow(-coefficient * rest);
                        reciprocal = true;
                        break;
                    case Divf(var dividend, var divisor):
                        numerator *= dividend;
                        denominator *= divisor;
                        reciprocal = true;
                        break;
                    default:
                        numerator *= factor;
                        break;
                }
            return reciprocal ? numerator / denominator : null;
        }

        private static Entity? ApplylHopitalRule(Entity expr, Variable x, Entity dest)
        {
            if (lHopitalDepth == 0)
                lHopitalApplications = 0;
            if (lHopitalDepth >= MaxlHopitalDepth || lHopitalApplications >= MaxlHopitalApplications)
                return null;
            // Held for the whole of the rule and not just for the recursive call below, since
            // asking what the two parts tend to is itself a limit that the rule may be applied
            // to, and counting only the recursion left those two free to nest without a bound.
            lHopitalDepth++;
            try { return ApplylHopitalRuleImpl(expr, x, dest); }
            finally { lHopitalDepth--; }
        }

        private static Entity? ApplylHopitalRuleImpl(Entity expr, Variable x, Entity dest)
        {
            if (expr is not Divf && AsQuotient(expr) is { } quotient)
                expr = quotient;
            if (expr is Divf(var num, var den))
                if (EvalAssumingContinuous(num.Limit(x, dest)) is var numLimit && EvalAssumingContinuous(den.Limit(x, dest)) is var denLimit)
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
                                    if (ComputeLimit(applied, x, dest) is { } resLim)
                                        return resLim;
                                }
                                finally { lHopitalChain.RemoveAt(lHopitalChain.Count - 1); }
                            }
            return null;
        }

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

        private static Entity TrivialTrigonometricReplacement(Entity expr, Variable x)
            => expr switch
            {
                Secantf(var arg) when arg.ContainsNode(x) => 1 / MathS.Cos(arg),
                Cosecantf(var arg) when arg.ContainsNode(x) => 1 / MathS.Sin(arg),
                _ => expr
            };

    }
}
