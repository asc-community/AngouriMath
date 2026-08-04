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
        {
            if (a.Imaginary is not 0 || b.Imaginary is not 0)
                return new NumericsComplex(double.NaN, double.NaN);
            var truncated = a.Real % b.Real;
            // The runtime's own % truncates, where the node is the floored remainder that takes
            // the sign of the divisor. They differ by exactly one divisor, and only where the
            // remainder and the divisor disagree in sign.
            // The + 0.0 is not idle: the runtime's % gives -0.0 for -6 % 3, and while that
            // compares equal to zero it prints as -0 and is not what the interpreter answers.
            return new NumericsComplex(
                (truncated is 0 || truncated < 0 == b.Real < 0
                    ? truncated
                    : truncated + b.Real) + 0.0,
                0);
        }
    }
}
