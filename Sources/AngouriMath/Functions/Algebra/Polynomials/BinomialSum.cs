//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;

namespace AngouriMath.Functions
{
    /// <summary>
    /// A summation over <c>k</c> from <c>0</c> to <c>N</c> of the binomial coefficient
    /// <c>N! / (k! (N - k)!)</c> times a power or a trigonometric weight, in closed form:
    /// <c>sum(N! / (k! (N - k)!) x^k, k, 0, N)</c> is <c>(1 + x)^N</c>, and
    /// <c>sum(N! / (k! (N - k)!) cos(k t), k, 0, N)</c> is <c>(2 cos(t/2))^N cos(N t / 2)</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The binomial theorem read backwards. With weights <c>x^k y^(N-k)</c> the sum is
    /// <c>(x + y)^N</c>, either power optional. With <c>cos(k t)</c> it is the real part of
    /// <c>(1 + e^(i t))^N</c>, and <c>1 + e^(i t) = 2 cos(t/2) e^(i t/2)</c> exactly, so the sum is
    /// <c>(2 cos(t/2))^N cos(N t/2)</c>; with <c>sin(k t)</c> the same with the sine. Both hold for
    /// every complex <c>t</c>, since <c>cos z = (e^(iz) + e^(-iz)) / 2</c> is the definition
    /// and the two conjugate sums add: no assumption on <c>t</c> is owed.
    /// </para>
    /// <para>
    /// The one condition is the range: for <c>N</c> below zero the summation is empty and this
    /// library answers an empty range with 0, while the formula does not, so a symbolic
    /// <c>N</c> gets the same piecewise <see cref="PolynomialSummation"/> attaches. Recognised
    /// structurally, on the factors of the simplified summand: the three factorials with
    /// <c>N</c> the upper bound as written, at most one power in <c>k</c>, at most one in
    /// <c>N - k</c>, at most one cosine or sine of <c>k</c> times something free of it, and
    /// nothing else that mentions <c>k</c>. A trigonometric weight beside a power is declined:
    /// the closed form then needs the modulus and argument of <c>y + x e^(i t)</c>, which is
    /// not a simplification. Question II.3 of
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1212">#1212</a>.
    /// </para>
    /// </remarks>
    internal static class BinomialSum
    {
        internal static Entity? ClosedForm(Entity expression, Entity var, Entity from, Entity to)
        {
            if (var is not Variable index)
                return null;
            if (from.Evaled is not Integer { IsZero: true })
                return null;
            if (to.ContainsNode(index) || !PolynomialSummation.IsWholeOrSymbolic(to))
                return null;
            var upper = to.InnerSimplified;

            var sawTop = false;
            var sawBottomK = false;
            var sawBottomRest = false;
            Entity? powerOfK = null;
            Entity? powerOfRest = null;
            Entity? angle = null;
            var sine = false;
            Entity constant = Integer.One;
            foreach (var factor in Mulf.LinearChildren(expression.InnerSimplified))
            {
                if (!factor.ContainsNode(index))
                {
                    if (!sawTop && factor is Factorialf(var top) && top == upper)
                        sawTop = true;
                    else
                        constant *= factor;
                    continue;
                }
                switch (factor)
                {
                    case Powf(Factorialf(var argument), var power) when power == Integer.MinusOne && argument == index && !sawBottomK:
                        sawBottomK = true;
                        break;
                    case Powf(Factorialf(var argument), var power) when power == Integer.MinusOne && IsUpperMinusIndex(argument, upper, index) && !sawBottomRest:
                        sawBottomRest = true;
                        break;
                    case Powf(var b, var e) when !b.ContainsNode(index) && e == index && powerOfK is null:
                        powerOfK = b;
                        break;
                    case Powf(var b, var e) when !b.ContainsNode(index) && IsUpperMinusIndex(e, upper, index) && powerOfRest is null:
                        powerOfRest = b;
                        break;
                    case Cosf(var argument) when angle is null && TryReadAngle(argument, index, out angle):
                        break;
                    case Sinf(var argument) when angle is null && TryReadAngle(argument, index, out angle):
                        sine = true;
                        break;
                    default:
                        return null;
                }
            }
            if (!sawBottomK || !sawBottomRest)
                return null;
            if (angle is not null && (powerOfK is not null || powerOfRest is not null))
                return null;

            Entity closed;
            if (angle is null)
                closed = MathS.Pow((powerOfK ?? Integer.One) + (powerOfRest ?? Integer.One), upper);
            else
            {
                var half = angle / 2;
                var magnitude = MathS.Pow(2 * MathS.Cos(half), upper);
                closed = magnitude * (sine ? MathS.Sin(upper * half) : MathS.Cos(upper * half));
            }
            // sum(x^k / (k! (N - k)!)) is (1 + x)^N / N!: the top factorial is a constant the
            // theorem does not need, and a concrete one has folded into a number before the
            // factors are read -- 300! is not a Factorialf by the time it gets here.
            if (!sawTop)
                closed /= MathS.Factorial(upper);
            closed = (constant * closed).InnerSimplified;

            // A concrete N is whole and non-negative here, or the range would have been
            // written out or refused; a symbolic one may still be anything.
            if (upper.Evaled is Integer)
                return closed;
            return MathS.Piecewise(new[]
            {
                new Providedf(closed, new GreaterOrEqualf(upper, Integer.Zero)),
                new Providedf(Integer.Zero, Entity.Boolean.True),
            }).InnerSimplified;
        }

        /// <summary>Whether <paramref name="e"/> is <c>upper - index</c> as written.</summary>
        private static bool IsUpperMinusIndex(Entity e, Entity upper, Variable index)
            => e is Minusf(var left, var right) && left == upper && right == index;

        /// <summary>
        /// <paramref name="argument"/> as <c>index * angle</c> for an <paramref name="angle"/>
        /// free of the index: the index alone (angle 1), or a product of it with something.
        /// </summary>
        private static bool TryReadAngle(Entity argument, Variable index, out Entity angle)
        {
            angle = Integer.One;
            if (argument == index)
                return true;
            if (argument is Mulf(var left, var right))
            {
                if (left == index && !right.ContainsNode(index))
                {
                    angle = right;
                    return true;
                }
                if (right == index && !left.ContainsNode(index))
                {
                    angle = left;
                    return true;
                }
                // k * pi / 3 arrives as (k * pi) / 3, a quotient over a product.
            }
            if (argument is Divf(var dividend, var divisor) && !divisor.ContainsNode(index)
                && TryReadAngle(dividend, index, out var inner))
            {
                angle = inner / divisor;
                return true;
            }
            return false;
        }
    }
}
