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
            if (expr is not Divf && AsQuotient(expr, x) is { } quotient)
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
