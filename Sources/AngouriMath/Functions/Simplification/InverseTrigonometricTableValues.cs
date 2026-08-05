//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Diagnostics.CodeAnalysis;

namespace AngouriMath.Functions
{
    using static Entity;
    using static Entity.Number;

    /// <summary>
    /// The trigonometric tables read backwards: given a value, the angle in the principal
    /// range whose sine or tangent it is.
    /// https://github.com/asc-community/AngouriMath/issues/569
    /// https://github.com/asc-community/AngouriMath/issues/179
    /// </summary>
    internal static class InverseTrigonometricTableValues
    {
        /// <summary>
        /// The double is a sieve and nothing more -- every hit is confirmed against the exact
        /// value -- so this only has to be tighter than the gap between two table entries,
        /// which is about a hundredth.
        /// </summary>
        private const double SieveWidth = 1e-9;

        private static Entity Sqrt(Entity a) => MathS.Sqrt(a);
        [ConstantField] private static readonly Entity pi = MathS.pi;

        /// <summary>
        /// Written out rather than inverted from <see cref="TrigonometryTableValues"/> at run
        /// time, because inverting it needs the principal branch and that table holds every
        /// angle around the circle: sin(2pi/3) and sin(2pi/6) are the same number, and only the
        /// second of them is an answer arcsin may give.
        /// </summary>
        [ConstantField] private static readonly List<(double approximation, Entity value, Entity angle)> arcsinTable = new()
        {
            (0d,                    0,                              (Entity)0),
            (0.25881904510252074d,  (Sqrt(6) - Sqrt(2)) / 4,        pi / 12),
            (0.3090169943749474d,   (Sqrt(5) - 1) / 4,              pi / 10),
            (0.3826834323650898d,   Sqrt(2 - Sqrt(2)) / 2,          pi / 8),
            (0.5d,                  Rational.Create(1, 2),          pi / 6),
            (0.5877852522924731d,   Sqrt(10 - 2 * Sqrt(5)) / 4,     pi / 5),
            (0.7071067811865476d,   Sqrt(2) / 2,                    pi / 4),
            (0.8090169943749475d,   (Sqrt(5) + 1) / 4,              3 * pi / 10),
            (0.8660254037844386d,   Sqrt(3) / 2,                    pi / 3),
            (0.9238795325112867d,   Sqrt(2 + Sqrt(2)) / 2,          3 * pi / 8),
            (0.9510565162951535d,   Sqrt(10 + 2 * Sqrt(5)) / 4,     2 * pi / 5),
            (0.9659258262890683d,   (Sqrt(6) + Sqrt(2)) / 4,        5 * pi / 12),
            (1d,                    1,                              pi / 2)
        };

        [ConstantField] private static readonly List<(double approximation, Entity value, Entity angle)> arctanTable = new()
        {
            (0d,                    0,                              (Entity)0),
            (0.2679491924311227d,   2 - Sqrt(3),                    pi / 12),
            (0.3249196962329063d,   Sqrt(25 - 10 * Sqrt(5)) / 5,    pi / 10),
            (0.41421356237309503d,  Sqrt(2) - 1,                    pi / 8),
            (0.5773502691896257d,   Sqrt(3) / 3,                    pi / 6),
            (0.7265425280053609d,   Sqrt(5 - 2 * Sqrt(5)),          pi / 5),
            (1d,                    1,                              pi / 4),
            (1.3763819204711736d,   Sqrt(25 + 10 * Sqrt(5)) / 5,    3 * pi / 10),
            (1.7320508075688772d,   Sqrt(3),                        pi / 3),
            (2.414213562373095d,    Sqrt(2) + 1,                    3 * pi / 8),
            (3.0776835371752527d,   Sqrt(5 + 2 * Sqrt(5)),          2 * pi / 5),
            (3.7320508075688776d,   2 + Sqrt(3),                    5 * pi / 12)
        };

        internal static bool PullArcsin(Complex arg, [NotNullWhen(true)] out Entity? res)
            => TryPull(arcsinTable, arg, out res);

        internal static bool PullArctan(Complex arg, [NotNullWhen(true)] out Entity? res)
            => TryPull(arctanTable, arg, out res);

        /// <summary>
        /// arccos(x) = pi/2 - arcsin(x), which is also the range arccos is stated over.
        /// </summary>
        internal static bool PullArccos(Complex arg, [NotNullWhen(true)] out Entity? res)
            => Complement(PullArcsin(arg, out var arcsin), arcsin, out res);

        /// <summary>
        /// arccotan(x) = pi/2 - arctan(x), the same reading of it the simplification rule for
        /// arctan(x) + arccotan(x) already takes.
        /// </summary>
        internal static bool PullArccotan(Complex arg, [NotNullWhen(true)] out Entity? res)
            => Complement(PullArctan(arg, out var arctan), arctan, out res);

        private static bool Complement(bool found, Entity? angle, [NotNullWhen(true)] out Entity? res)
        {
            if (!found || angle is null)
            {
                res = null;
                return false;
            }
            res = (pi / 2 - angle).InnerSimplified;
            return true;
        }

        private static bool TryPull(List<(double approximation, Entity value, Entity angle)> table,
            Complex arg, [NotNullWhen(true)] out Entity? res)
        {
            res = null;
            // Only the real branch. The table says nothing about arcsin(2), and answering the
            // complex cases from it would mean choosing a branch cut here rather than where the
            // numeric functions already choose one.
            if (arg is not Real { EDecimal: var given } || !given.IsFinite)
                return false;
            var approximation = given.ToDouble();
            var negative = approximation < 0;
            if (negative)
                approximation = -approximation;
            foreach (var (candidate, value, angle) in table)
            {
                if (System.Math.Abs(candidate - approximation) > SieveWidth)
                    continue;
                // Near a table value is not the same as being one: 0.4999999 must come back
                // unanswered rather than as pi/6, so the sieve hit is checked against the exact
                // value at the precision the caller asked for.
                if (value.Evaled is not Real { EDecimal: var exact })
                    return false;
                if (!Number.IsZero(negative ? exact + given : exact - given))
                    return false;
                // Both functions in the table are odd, so a negative argument is the same angle
                // the other way round.
                res = negative ? (-angle).InnerSimplified : angle;
                return true;
            }
            return false;
        }
    }
}
