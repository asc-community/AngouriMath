//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Multithreading;
using System;
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
            => IsInfiniteNode(power.Limit(x, dest))
                || (IsInfiniteNode(power.Limit(x, dest, ApproachFrom.Left))
                    && IsInfiniteNode(power.Limit(x, dest, ApproachFrom.Right)));

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
            if (lHopitalDepth >= MaxlHopitalDepth)
                return null;
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
                                MultithreadingFunctional.ExitIfCancelled();
                                lHopitalDepth++;
                                try
                                {
                                    if (ComputeLimit(applied, x, dest) is { } resLim)
                                        return resLim;
                                }
                                finally { lHopitalDepth--; }
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
