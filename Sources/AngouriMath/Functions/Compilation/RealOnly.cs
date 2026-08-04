//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using NumericsComplex = System.Numerics.Complex;

namespace AngouriMath.Core.Compilation
{
    /// <summary>
    /// The operations that are defined on the reals and not on the complex numbers, for the
    /// compiler that carries every value as a <see cref="System.Numerics.Complex"/> whether it
    /// is real or not.
    /// </summary>
    internal static class RealOnly
    {
        /// <summary>
        /// The remainder of one number by another, or NaN where either of them is not real.
        /// </summary>
        /// <remarks>
        /// There is no one remainder of a complex number by another: which multiple of the
        /// divisor to subtract is a choice, and the two usual ones -- rounding the quotient to
        /// the nearest Gaussian integer, or truncating it -- disagree. The interpreter declines
        /// the question and leaves <c>a % b</c> unevaluated; the compiled path has no way to
        /// return an unevaluated node, so it answers NaN, which says the same thing.
        /// <para/>
        /// For real arguments this is the runtime's own <c>%</c>, and so takes the sign of the
        /// dividend, which is what <see cref="Entity.Evaled"/> does as well.
        /// </remarks>
        public static NumericsComplex Mod(NumericsComplex a, NumericsComplex b)
            => a.Imaginary is not 0 || b.Imaginary is not 0
                ? new NumericsComplex(double.NaN, double.NaN)
                : new NumericsComplex(a.Real % b.Real, 0);
    }
}
