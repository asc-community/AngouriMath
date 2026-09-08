//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using PeterO.Numbers;
using static AngouriMath.Entity.Number;

namespace AngouriMath.Functions
{
    internal static partial class TreeAnalyzer
    {
        /// <summary>
        /// The largest exponent this looks for. A perfect power past it is one nobody wrote
        /// by hand.
        /// </summary>
        private const int LargestExponentToRead = 64;

        /// <summary>
        /// <paramref name="value"/> as <c>root ^ exponent</c> for a positive rational
        /// <paramref name="root"/> and the largest whole <paramref name="exponent"/> of at least
        /// 2 that fits: <c>16/9</c> is <c>(4/3)^2</c>, <c>64</c> is <c>2^6</c>, <c>1/8</c> is
        /// <c>(1/2)^3</c>. Only for a positive value whose numerator and denominator both fit in
        /// 64 bits; anything else is not a perfect power for this purpose.
        /// </summary>
        /// <remarks>
        /// Positive only, deliberately: <c>-8</c> is <c>(-2)^3</c> and <c>ln(-8)</c> is
        /// <c>ln 8 + i pi</c> while <c>3 ln(-2)</c> is <c>3 ln 2 + 3 i pi</c>, so the identity a
        /// caller wants this for does not survive a negative root.
        /// </remarks>
        internal static bool TryPerfectPower(Rational value, out Rational root, out int exponent)
        {
            root = value;
            exponent = 1;
            var numerator = value.ERational.Numerator;
            var denominator = value.ERational.Denominator;
            if (numerator.Sign <= 0 || !numerator.CanFitInInt64() || !denominator.CanFitInInt64())
                return false;
            if (numerator.Equals(EInteger.One) && denominator.Equals(EInteger.One))
                return false;
            for (var e = LargestExponentToRead; e >= 2; e--)
            {
                if (IntegerRoot(numerator, e) is not { } top || IntegerRoot(denominator, e) is not { } bottom)
                    continue;
                root = Rational.Create(ERational.Create(top, bottom));
                exponent = e;
                return true;
            }
            return false;
        }

        /// <summary>
        /// The whole <c>e</c>-th root of <paramref name="n"/>, or <see langword="null"/> where
        /// there is none: a floating estimate, then the neighbours checked exactly.
        /// </summary>
        private static EInteger? IntegerRoot(EInteger n, int e)
        {
            if (n.Equals(EInteger.One))
                return EInteger.One;
            var estimate = (long)System.Math.Round(System.Math.Pow((double)n.ToInt64Checked(), 1.0 / e));
            for (var candidate = System.Math.Max(2, estimate - 1); candidate <= estimate + 1; candidate++)
            {
                var whole = EInteger.FromInt64(candidate);
                if (whole.Pow(e).Equals(n))
                    return whole;
            }
            return null;
        }
    }
}
