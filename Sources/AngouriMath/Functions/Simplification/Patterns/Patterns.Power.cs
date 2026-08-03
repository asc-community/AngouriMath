//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using PeterO.Numbers;
using static AngouriMath.Entity;

namespace AngouriMath.Functions
{
    partial class Patterns
    {
        /// <summary>a ^ (-1) => 1 / a</summary>
        internal static Entity InvertNegativePowers(Entity expr) =>
            expr is Powf(var @base, Integer { IsNegative: true } pow)
            ? 1 / MathS.Pow(@base, -1 * pow)
            : expr;
        /// <summary>1 + (-x) => 1 - x</summary>
        internal static Entity InvertNegativeMultipliers(Entity expr) =>
            expr is Sumf(var any1, Mulf(Real { IsNegative: true } const1, var any2))
            ? any1 - (-1 * const1) * any2
            : expr;

        internal static Entity PowerRules(Entity x) => x switch
        {
            // {} / {} = 1 provided not {} = 0
            Divf(var any1, var any1a) when any1 == any1a => new Providedf(1, !any1.EqualTo(0)),

            // {1}^({2} / log({3}, {1})) = {3}^{2}
            Powf(var any1, Divf(var any2, Logf(var any3, var any1a))) when any1 == any1a => new Powf(any3, any2),

            // {} ^ n * {}
            Mulf(Powf(var any1, var any2), var any1a) when any1 == any1a => new Powf(any1, any2 + 1),
            Mulf(var any1, Powf(var any1a, var any2)) when any1 == any1a => new Powf(any1, any2 + 1),

            // {} ^ n * {} ^ m = {} ^ (n + m)
            Mulf(Powf(var any1, var any2), Powf(var any1a, var any3)) when any1 == any1a => new Powf(any1, any2 + any3),

            // {} ^ n / {} ^ m = {} ^ (n - m)
            Divf(Powf(var any1, var any2), Powf(var any1a, var any3)) when any1 == any1a => new Powf(any1, any2 - any3),

            // ({} ^ {}) ^ {} = {} ^ ({} * {})
            Powf(Powf(var any1, var any2), var any3) => new Powf(any1, any2 * any3),

            // {1} ^ n * {2} ^ n = ({1} * {2}) ^ n
            Mulf(Powf(var any1, var any3), Powf(var any2, var any3a)) when any3 == any3a => new Powf(any1 * any2, any3),
            Divf(Powf(var any1, var any3), Powf(var any2, var any3a)) when any3 == any3a => new Powf(any1 / any2, any3),

            // x / x^n
            Divf(var any1, Powf(var any1a, var any2)) when any1 == any1a => new Powf(any1, 1 - any2),

            // x^n / x
            Divf(Powf(var any1, var any2), var any1a) when any1 == any1a => new Powf(any1, any2 - 1),

            // x^n / x^m
            Divf(Powf(var any1, var any2), Powf(var any1a, var any3)) when any1 == any1a => new Powf(any1, any2 - any3),

            // c ^ log(c, a) = a
            Powf(Number const1, Logf(Number const1a, var any1)) when const1 == const1a => any1,

            Mulf(Powf(var any1, var any3), Mulf(var any1a, var any2)) when any1 == any1a => new Powf(any1, any3 + 1) * any2,
            Mulf(Powf(var any1, var any3), Mulf(var any2, var any1a)) when any1 == any1a => new Powf(any1, any3 + 1) * any2,
            Mulf(Mulf(var any1, var any2), Powf(var any1a, var any3)) when any1 == any1a => new Powf(any1, any3 + 1) * any2,
            Mulf(Mulf(var any2, var any1), Powf(var any1a, var any3)) when any1 == any1a => new Powf(any1, any3 + 1) * any2,

            // (a * x) ^ c = a^c * x^c
            Powf(Mulf(Number const1, var any1), Number const2) =>
                new Powf(const1, const2) * new Powf(any1, const2),

            // {1} ^ (-1) = 1 / {1}
            Powf(var any1, Integer(-1)) => 1 / any1,

            // (a / {})^b * {} = a^b * {}^(1-b)
            Mulf(Powf(Divf(Number const1, var any1), Number const2), var any1a) when any1 == any1a =>
                new Powf(const1, const2) * new Powf(any1, 1 - const2),
            Mulf(Powf(Divf(Number const1, var any1), Number const2), Powf(var any1a, Number const3))
                when any1 == any1a => new Powf(const1, const2) * new Powf(any1, const3 - const2),

            // {1} / {2} / {2}
            Divf(Divf(var any1, var any2), var any2a) when any2 == any2a =>
                any1 / new Powf(any2, 2),
            Divf(Divf(var any1, Powf(var any2, var any3)), var any2a) when any2 == any2a =>
                any1 / new Powf(any2, any3 + 1),
            Divf(Divf(var any1, var any2), Powf(var any2a, var any3)) when any2 == any2a =>
                any1 / new Powf(any2, any3 + 1),
            Divf(Divf(var any1, Powf(var any2, var any4)), Powf(var any2a, var any3)) when any2 == any2a =>
                any1 / new Powf(any2, any3 + any4),

            // x * {} ^ {} = {} ^ {} * x
            Mulf(Variable var1, Powf(var any1, var any2)) => new Powf(any1, any2) * var1,

            Logf(var any1, Powf(var any2, var any3)) => any3 * MathS.Log(any1, any2),
            Logf(var any1, var any1a) when any1 == any1a => new Providedf(1, any1 > 0),
            Logf(Divf(Integer(1), var any1), Divf(Integer(1), var any2)) => MathS.Log(any1, any2),
            Logf(var any1, Divf(Integer(1), var any2)) => -MathS.Log(any1, any2),
            Logf(Divf(Integer(1), var any1), var any2) => -MathS.Log(any1, any2),
            

            Sumf(Logf(var any3, var any1), Logf(var any3a, var any2)) when any3 == any3a => any3.Log(any1 * any2),
            Minusf(Logf(var any3, var any1), Logf(var any3a, var any2)) when any3 == any3a => any3.Log(any1 / any2),

            // sqrt(8) = 2 * sqrt(2), cbrt(54) = 3 * cbrt(2)
            Powf(Integer { IsPositive: true } radicand, Rational and not Integer and var power)
                when ReduceRadical(radicand, power) is { } reduced => reduced,

            _ => x
        };

        /// <summary>
        /// Largest divisor tried when reducing a radical. Reducing sqrt(n) exactly would
        /// mean factoring n, which is not something a simplification pass can afford to
        /// do on every node. Trial division by small divisors catches every radical that
        /// turns up in practice; anything left stays under the root, which is still a
        /// correct answer, just not a fully reduced one.
        /// </summary>
        private const int MaxRadicalTrialDivisor = 1000;

        /// <summary>
        /// Rewrites <c>n ^ (p/q)</c> as <c>a ^ p * b ^ (p/q)</c> where <c>n = a^q * b</c>,
        /// i.e. pulls every q-th power out from under the root.
        /// </summary>
        /// <returns>
        /// <see langword="null"/> when nothing can be pulled out, so that the rule does
        /// not fire and rewriting terminates.
        /// </returns>
        private static Entity? ReduceRadical(Integer radicand, Rational power)
        {
            var denominator = power.ERational.Denominator;
            if (denominator < 2 || denominator > 64)
                return null;
            var root = denominator.ToInt32Checked();

            var inside = radicand.EInteger;
            if (inside <= 1)
                return null;

            var outside = EInteger.One;
            for (int divisor = 2; divisor <= MaxRadicalTrialDivisor; divisor++)
            {
                var divisorPower = EInteger.FromInt32(divisor).Pow(root);
                if (divisorPower > inside)
                    break;
                while (inside.Remainder(divisorPower).IsZero)
                {
                    inside /= divisorPower;
                    outside *= EInteger.FromInt32(divisor);
                }
            }

            if (outside.Equals(EInteger.One))
                return null; // already reduced; firing here would loop forever

            // n^(p/q) = (a^q * b)^(p/q) = a^p * b^(p/q)
            return new Powf(Integer.Create(outside), Integer.Create(power.ERational.Numerator))
                 * new Powf(Integer.Create(inside), power);
        }
    }
}
