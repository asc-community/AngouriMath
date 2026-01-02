//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//


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
            /// We are given angle theta and sin(theta)
            /// This function returns sin(theta / 2).
            /// </summary>
            /// <param name="theta">
            /// An angle, sine of half of which
            /// will be computed (that is, if
            /// you have sin(2x), then pass 2x here).
            /// </param>
            /// <param name="sin2x">
            /// The value of the sine of
            /// doubled angle.
            /// </param>
            /// <returns>
            /// The value of sine of half of the
            /// given angle if can (null otherwise)
            /// </returns>
            public static Entity? GetSineOfHalvedAngle(Entity theta, Entity sin2x)
                => UnsafeAndInternal.DivideByEntityStrict(theta, MathS.pi)
                    ?.Inject(sin2x).Pipe(
                        static (thetaRat, sin2x) => 
                            TrigonometricAngleExpansion.GetSineOfHalvedAngle(thetaRat, sin2x));
            
            /// <summary>
            /// We are given angle theta and cos(theta)
            /// This function returns cos(theta / 2).
            /// </summary>
            /// <param name="theta">
            /// An angle, cosine of half of which
            /// will be computed (that is, if
            /// you have cos(2x), then pass 2x here).
            /// </param>
            /// <param name="cos2x">
            /// The value of the cosine of
            /// doubled angle.
            /// </param>
            /// <returns>
            /// The value of cosine of half of the
            /// given angle if can (null otherwise)
            /// </returns>
            public static Entity? GetCosineOfHalvedAngle(Entity theta, Entity cos2x)
                => UnsafeAndInternal.DivideByEntityStrict(theta, MathS.pi)
                    ?.Inject(cos2x).Pipe(
                        static (thetaRat, cos2x) => 
                            TrigonometricAngleExpansion.GetCosineOfHalvedAngle(thetaRat, cos2x));
            
            /// <summary>
            /// Assume you have sin(n x), where
            /// n is an integer number. Then
            /// sin(n x) can be easily represented
            /// as a combination of arithmetic operations
            /// of sin(x) and cos(x), which is exactly what
            /// this function does.
            /// </summary>
            /// <param name="sinx">
            /// The value of sin(x)
            /// </param>
            /// <param name="cosx">
            /// The value of cos(x)
            /// </param>
            /// <param name="n">
            /// The integer multiplier of the
            /// angle in the original sin(n x)
            /// </param>
            /// <returns>
            /// Expanded sine.
            /// </returns>
            public static Entity ExpandSineArgumentMultiplied(Entity sinx, Entity cosx, int n)
                => TrigonometricAngleExpansion.ExpandSineArgumentMultiplied(sinx, cosx, n);
                
            /// <summary>
            /// Assume you have cos(n x), where
            /// n is an integer number. Then
            /// cos(n x) can be easily represented
            /// as a combination of arithmetic operations
            /// of sin(x) and cos(x), which is exactly what
            /// this function does.
            /// </summary>
            /// <param name="sinx">
            /// The value of sin(x)
            /// </param>
            /// <param name="cosx">
            /// The value of cos(x)
            /// </param>
            /// <param name="n">
            /// The integer multiplier of the
            /// angle in the original cos(n x)
            /// </param>
            /// <returns>
            /// Expanded cosine.
            /// </returns>
            public static Entity ExpandCosineArgumentMultiplied(Entity sinx, Entity cosx, int n)
                => TrigonometricAngleExpansion.ExpandCosineArgumentMultiplied(sinx, cosx, n);
            
            /// <summary>
            /// Expands a sine over terms
            /// via binary expansion.
            /// TODO: more docs
            /// </summary>
            public static Entity ExpandSineOfSum(IReadOnlyList<(Entity SinX, Entity CosX)> terms)
                => TrigonometricAngleExpansion.ExpandSineOfSum(terms, 0, terms.Count - 1);
            
            /// <summary>
            /// Expands a cosine over terms
            /// via binary expansion.
            /// TODO: more docs
            /// </summary>
            public static Entity ExpandCosineOfSum(IReadOnlyList<(Entity SinX, Entity CosX)> terms)
                => TrigonometricAngleExpansion.ExpandCosineOfSum(terms, 0, terms.Count - 1);
            
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
                => TrigonometricAngleExpansion.SymbolicFormOfSine(angle)?.InnerSimplified;
            
            /// <summary>
            /// Finds the symbolic form of cosine, if can
            /// For example, cos(9/14) is cos(1/2 + 1/7) which
            /// can be expanded as a cosine of sum and hence
            /// an analytical (symbolic) form.
            /// </summary>
            /// <param name="angle">
            /// The angle in radians
            /// </param>
            /// <returns>
            /// The cosine's symbolic form
            /// or null if cannot find it
            /// </returns>
            public static Entity? SymbolicFormOfCosine(Entity angle)
                => TrigonometricAngleExpansion.SymbolicFormOfCosine(angle)?.InnerSimplified;
        }
    }
}