/* 
 * Copyright (c) 2019-2021 Angouri.
 * AngouriMath is licensed under MIT. 
 * Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
 * Website: https://am.angouri.org.
 */
using System.Collections.Generic;
using AngouriMath.Functions;

namespace AngouriMath
{
    partial class MathS
    {
        /// <summary>
        /// Features that might become stable
        /// in the future, but are not guaranteed
        /// to do anything useful or correctly
        /// at the current moment.
        /// </summary>
        public static class ExperimentalFeatures
        {
            /// <summary>
            /// Solves an equation
            /// a x + b y = c
            /// </summary>
            /// <returns>
            /// x and y if found, null otherwise
            /// </returns>
            public static (Entity.Number.Integer x, Entity.Number.Integer y)? SolveDiophantineEquation(Entity.Number.Integer a, Entity.Number.Integer b, Entity.Number.Integer c)
                => Diophantine.Solve(a, b, c);

            /// <summary>
            /// Decomposes an arbitrary rational
            /// number into sum of rationals a_i / p_i^k,
            /// where p_i is a prime number. Evaluates
            /// lazily.
            /// </summary>
            public static IEnumerable<(Entity.Number.Integer numerator, Entity.Number.Integer denPrime, Entity.Number.Integer denPower)> DecomposeRational(Entity.Number.Integer num, Entity.Number.Integer den)
                => Fraction.Decompose(num, den);

            /// <summary>
            /// Decomposes an arbitrary rational
            /// number into sum of rationals a_i / p_i^k,
            /// where p_i is a prime number. Evaluates
            /// lazily.
            /// </summary>
            public static IEnumerable<(Entity.Number.Integer numerator, Entity.Number.Integer denPrime, Entity.Number.Integer denPower)> DecomposeRational(Entity.Number.Rational rational)
                => DecomposeRational(rational.Numerator, rational.Denominator);
            
            /// <summary>
            /// Finds the symbolic form of sine, if can
            /// For example, sin(9/14) is sin(1/2 + 1/7) which
            /// can be expanded as a sine of sum and hence
            /// an analytical (symbolic) form.
            /// </summary>
            /// <param name="angle">
            /// The angle in radians
            /// </param>
            /// <returns>
            /// The sine's symbolic form
            /// or null if cannot find it
            /// </returns>
            public static Entity? SymbolicFormOfSine(Entity angle)
                => TrigonometricAngleExpansion.SymbolicFormOfSine(angle);
        }
    }
}