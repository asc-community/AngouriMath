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
            //
            // True for a positive base whatever the exponents, and for any base when the
            // outer exponent is a whole number -- (a^b)^3 is a^b * a^b * a^b however a is
            // signed. Outside those two it is false, and it was applied unconditionally:
            //
            //     sqrt(x^2)      came back as x,    which at -0.63 is -0.63 where it is 0.63
            //     (x^2)^(3/2)    came back as x^3,  which at -2 is -8 where it is 8
            //
            // https://github.com/asc-community/AngouriMath/issues/752
            Powf(Powf(var any1, var any2), var any3)
                when any3 is Integer || any1.Evaled is Real { IsPositive: true }
                => new Powf(any1, any2 * any3),

            // {1} ^ n * {2} ^ n = ({1} * {2}) ^ n
            //
            // The same condition the ({}^{})^{} rule above carries, and missing here for the
            // same reason -- both were written unconditionally and #752 only looked at one of
            // them. True for a whole n whatever the signs, since a^3 b^3 is (ab)(ab)(ab), and
            // for positive real bases whatever the exponent. Outside those two the two sides
            // can differ by a full turn of the argument:
            //
            //     sqrt(x) * sqrt(y)   came back as sqrt(x * y), and at x = y = -1 the first
            //                         is i * i = -1 while the second is sqrt(1) = 1
            //
            // https://github.com/asc-community/AngouriMath/issues/801
            Mulf(Powf(var any1, var any3), Powf(var any2, var any3a))
                when any3 == any3a
                     && (any3 is Integer
                         || (any1.Evaled is Real { IsPositive: true }
                             && any2.Evaled is Real { IsPositive: true }))
                => new Powf(any1 * any2, any3),
            // Same condition, same reason -- sqrt(2) / sqrt(-3) is -0.8165i where
            // (2 / -3)^(1/2) is +0.8165i. https://github.com/asc-community/AngouriMath/issues/802
            //
            // This gathering was what let the limit machinery read a 1^oo out of a quotient,
            // so guarding it here used to cost `(x^2 + 1)^x / (x^2)^x` its limit. The limit
            // reader now recognises the quotient itself, where the identity is checkable
            // because there is a destination to be near and the bases can be required to be
            // positive on the way to it -- see ApplySecondRemarkable.
            Divf(Powf(var any1, var any3), Powf(var any2, var any3a))
                when any3 == any3a
                     && (any3 is Integer
                         || (any1.Evaled is Real { IsPositive: true }
                             && any2.Evaled is Real { IsPositive: true }))
                => new Powf(any1 / any2, any3),

            // {1} ^ n / {2} ^ (c * n) = ({1} / {2} ^ c) ^ n, and the same the other way up.
            //
            // The rule above pairs two powers by their exponents, and the ({}^{})^{} rule
            // further up rewrites (b^c)^n as b^(c*n) -- on the child, on the way up, so it
            // has already happened by the time the pair is looked at. Where it applies to
            // only one of the two, which is whenever only one base is itself a power, the
            // exponents stop matching and the pair is lost: (a^2)^x / (b^2)^x gathers,
            // because both sides moved together, while (a^2)^x / b^x does not.
            // These read that pair back. Restricted to a whole c so that nothing gains a
            // root it did not have -- b^c goes into the base, rather than the exponent
            // being divided. https://github.com/asc-community/AngouriMath/issues/740
            Divf(Powf(var any1, var any3), Powf(var any2, Mulf(Integer { IsPositive: true } const1, var any3a)))
                when any3 == any3a => new Powf(any1 / new Powf(any2, const1), any3),
            Divf(Powf(var any1, Mulf(Integer { IsPositive: true } const1, var any3)), Powf(var any2, var any3a))
                when any3 == any3a => new Powf(new Powf(any1, const1) / any2, any3),

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
            //
            // Taking a factor out from under a root needs that factor to be positive, or the
            // root to be a whole power. With a negative one it moves the branch: sqrt(-x)
            // became sqrt(-1) * sqrt(x) = i * sqrt(x), and at x = -0.63 the first is 0.7937
            // while the second is -0.7937 -- the negation, not the value. A positive constant
            // is safe whatever the sign of x, which is why the rule is narrowed rather than
            // removed. https://github.com/asc-community/AngouriMath/issues/752
            Powf(Mulf(Number const1, var any1), Number const2)
                when const2 is Integer || const1 is Real { IsPositive: true } =>
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

            // log_b(a^c) = c * log_b(a) holds where c * ln(a) stays inside the strip
            // Im in (-pi, pi] that ln maps onto, and not in general: ln(e^(3*pi*i)) is pi*i
            // while 3*pi*i is not, the two differing by exactly the 2*pi*i the principal branch
            // discards. A base that is a positive real makes ln(a) real, and a real exponent then
            // keeps the product real, so there is nothing to discard. Anything else is left as
            // written -- including a symbolic exponent under the default complex reading, where
            // the question is not decidable.
            // https://github.com/asc-community/AngouriMath/issues/902
            Logf(var any1, Powf(var any2, var any3))
                when IsPositiveReal(any2) && MayBeTakenAsReal(any3) => any3 * MathS.Log(any1, any2),
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
        /// Gathers the factors of a product that are powers of one base, wherever they sit
        /// in it: <c>a^n * c * a^m</c> becomes <c>a^(n+m) * c</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <see cref="PowerRules"/> already has <c>{}^n * {}^m = {}^(n+m)</c>, but it pairs
        /// two sibling nodes, and a product is a tree rather than a list -- in
        /// <c>sin(x)^4 * (-6) * sin(x)^2</c> the constant sits between the two powers, so
        /// they are never siblings and the rule never fires. Full <see cref="Entity.Simplify"/>
        /// gets these because it reassociates and sorts the factors first; nothing that
        /// normalises without sorting does.
        /// </para>
        /// <para>
        /// <c>a^n * a^m = a^(n+m)</c> needs no condition: with <c>a^n</c> read as
        /// <c>e^(n Log a)</c> on the principal branch, the two sides are
        /// <c>e^(n Log a) * e^(m Log a)</c> and <c>e^((n+m) Log a)</c>, which are equal for
        /// every complex <c>n</c> and <c>m</c>. This is what makes it unlike
        /// <c>(a^b)^c = a^(b*c)</c>, which moves the branch and is guarded above.
        /// The one point it does not cover is <c>a = 0</c> with a negative exponent, where
        /// <c>0^2 * 0^(-1)</c> is undefined and <c>0^1</c> is 0 -- so this is applied to
        /// integrands, where an antiderivative may differ on a measure-zero set, and not in
        /// <see cref="PowerRules"/>. https://github.com/asc-community/AngouriMath/issues/781
        /// </para>
        /// </remarks>
        internal static Entity GatherPowersOfOneBase(Entity x)
        {
            if (x is not (Mulf or Divf))
                return x;

            // Exponent null marks a factor that is carried through untouched.
            var factors = new List<(Entity Base, Entity? Exponent)>();
            var merged = false;
            foreach (var factor in Mulf.LinearChildren(x))
            {
                var (@base, exponent) = Decompose(factor);
                // Numeric factors are left to evaluation, which folds 2 * 2 into 4. Gathering
                // them here would write it as 2^2 and call that progress.
                if (@base is Number)
                {
                    factors.Add((factor, null));
                    continue;
                }
                var found = false;
                for (var i = 0; i < factors.Count; i++)
                    if (factors[i].Exponent is { } already && factors[i].Base == @base)
                    {
                        // Only the exponent is folded, never the product around it.
                        // InnerSimplified on the whole expression rewrites x^(-2) back into
                        // 1/x^2, and SolveAsPolynomialTerm turns a 1/x^n it is handed into
                        // Pow(x, -n) again -- the two normalisations chase each other until
                        // the stack runs out.
                        factors[i] = (@base, (already + exponent).InnerSimplified);
                        merged = found = true;
                        break;
                    }
                if (!found)
                    factors.Add((@base, exponent));
            }

            // Rebuilding unconditionally would rewrite every quotient in the tree as a
            // negative power, since LinearChildren flattens a / b into a * b^(-1).
            if (!merged)
                return x;

            Entity? result = null;
            foreach (var (@base, exponent) in factors)
            {
                var factor = exponent switch
                {
                    null => @base,
                    Integer(1) => @base,
                    _ => new Powf(@base, exponent)
                };
                result = result is null ? factor : result * factor;
            }
            return result ?? x;
        }

        /// <summary>
        /// Reads a factor of a product as a base and an exponent.
        /// </summary>
        /// <remarks>
        /// The nested case is not cosmetic: <see cref="Mulf.LinearChildren"/> writes a
        /// divisor as <c>(...)^(-1)</c>, so <c>x^3 / x^2</c> arrives as
        /// <c>x^3 * (x^2)^(-1)</c>, whose second factor has base <c>x^2</c> rather than
        /// <c>x</c> and would be read as an unrelated base. Unwrapping it is
        /// <c>(a^b)^n = a^(b*n)</c>, which holds for a whole <c>n</c> whatever the sign of
        /// <c>a</c> -- the same guard the <c>({}^{})^{}</c> rule above carries, and for the
        /// same reason. https://github.com/asc-community/AngouriMath/issues/752
        /// </remarks>
        /// <summary>
        /// Whether <paramref name="entity"/> is a real strictly above zero, decided rather than
        /// assumed. <c>ln</c> of such a number is a real, so a real multiple of it stays on the
        /// real line and inside <c>ln</c>'s principal strip.
        /// </summary>
        /// <remarks>
        /// Finiteness is checked separately because <see cref="Real.IsPositive"/> is
        /// <c>!IsNegative &amp;&amp; !IsZero</c>, which <c>NaN</c> and <c>+oo</c> both satisfy.
        /// </remarks>
        private static bool IsPositiveReal(Entity entity)
            => entity.Evaled is Real { EDecimal.IsFinite: true } value && value.IsPositive;

        /// <summary>
        /// Whether this operand may be taken as real: because the expression is being read as a
        /// real-valued one, because the node's own declared codomain says so, or because its value
        /// is a real to begin with.
        /// </summary>
        /// <remarks>
        /// The first two are the disjunction <c>Patterns.EqualityInequality.cs</c> uses to ask the
        /// same question. A bare <see cref="Variable"/> is <c>Domain.Any</c>, so a symbol under the
        /// default complex reading answers <see langword="false"/> here -- which is the point.
        /// </remarks>
        private static bool MayBeTakenAsReal(Entity entity)
            => MathS.Settings.Codomain.Value is AngouriMath.Core.Domain.Real
               || IsKnownReal(entity)
               || entity.Evaled is Real { EDecimal.IsFinite: true };

        private static (Entity Base, Entity Exponent) Decompose(Entity factor)
        {
            if (factor is not Powf(var @base, var exponent))
                return (factor, 1);
            while (@base is Powf(var inner, var innerExponent)
                   && (exponent is Integer || inner.Evaled is Real { IsPositive: true }))
            {
                exponent = innerExponent * exponent;
                @base = inner;
            }
            return (@base, exponent);
        }

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
