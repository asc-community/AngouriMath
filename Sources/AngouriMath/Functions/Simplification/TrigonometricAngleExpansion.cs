/* 
 * Copyright (c) 2019-2021 Angouri.
 * AngouriMath is licensed under MIT. 
 * Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
 * Website: https://am.angouri.org.
 */

using System;
using System.Collections.Generic;
using AngouriMath.Core;
using AngouriMath.Core.Exceptions;
using AngouriMath.Extensions;
using HonkSharp.Fluency;
using static AngouriMath.Entity.Number;

namespace AngouriMath.Functions
{
    internal static class TrigonometricAngleExpansion
    {
        [ConstantField] private static readonly Rational OneHalf = 0.5.ToNumber().Downcast<Rational>();
        
        /// <summary>
        /// We are given angle thetaRat and sin(theta)
        /// where thetaRat = theta / pi.
        /// This function returns sin(theta / 2).
        /// </summary>
        /// <param name="thetaRat">
        /// An angle already divided by pi
        /// </param>
        /// <returns>
        /// The value of sine of half of the
        /// given angle if can (null otherwise)
        /// </returns>
        internal static Entity? GetSineOfHalvedAngle(Entity thetaRat, Entity sin2x)
        {
            // sin(2x) = 2sin(x)cos(x)
            // 
            // c = sin(2x)
            //

            if ((thetaRat / 2).Evaled is not Rational x)
                return null;
           
            var mod = x % 2;
            if (mod.IsZero)
                return 0;
            if (mod == OneHalf)
                return 1;
            if (mod == 1)
                return 0;
            if (mod == 1.5)
                return -1;

            // cos(x) = cosSign * sqrt(1 - sin(x)2)
            //
            // c = 2 sin(x) sqrt(1 - sin(x)2) * cosSign
            // c2 = 4 sin(x)2 (1 - sin(x)2)
            // sin(x)4 - sin(x)2 + c2/4 = 0
            //
            // sin(x) = +- sqrt( 1/2 * ( 1 -+ sqrt( 1 - c2 ) ) )


            var sinSign = 1;
            
            
            if (mod > 1)
                sinSign = -1;
             
//------I.
//                 
//      sin(2x) = 2 sin(x) cos(x)
//      
//      For simplicity, let c = sin(2x).
//      
        
        
//------II.
//      
//      sin(x)2 + cos(x)2 = 1 =>
//      
//                    //================|
//      cos(x) = \\  //  1 - sin(x)^2       * cosSign
//                \\//
//      
//      For some cosSign in { -1, 1 }
//       

//------III.
//                        //================|            
//      c = 2 sin(x) \\  //  1 - sin(x)^2       * cosSign
//                    \\//                       
//

//------IV.
//
//      For now we will ignore cosSign
//
//      c^2 = 4 sin(x)^2 (1 - sin(x)^2)
//
//      c^2
//     -----  = sin(x)^2 (1 - sin(x)^2)
//       4
//
//                             c^2 
//      sin(x)^4 - sin(x)^2 + ----- = 0
//                              4  
//

//------V.
//                                   //========================================||
//                                  //   1        1     /------------------|
//      sin(x) = sinSign     \\    //   ---  +-  ---   / 1 -   sin(2x) ^ 2
//                            \\  //     2        2  \/
//                             \\//
//

            var sin1 = (sinSign * MathS.Sqrt(OneHalf - OneHalf * MathS.Sqrt(1 - sin2x.Pow(2)))).InnerSimplified;
            var sin2 = (sinSign * MathS.Sqrt(OneHalf + OneHalf * MathS.Sqrt(1 - sin2x.Pow(2)))).InnerSimplified;
//                                                  ^ that's where we change the sign

            if ((MathS.Sin(x * MathS.pi) - sin1).Abs().EvalNumerical().Downcast<Real>() < 0.01)
                return sin1;
            if ((MathS.Sin(x * MathS.pi) - sin2).Abs().EvalNumerical().Downcast<Real>() < 0.01)
                return sin2;
            throw new AngouriBugException("");
        }
        
        [ConstantField] private static readonly Dictionary<Integer, Entity> angles = new() 
        {
            { 2, "1" },
            { 3, "sqrt(3) / 2" },
            { 4, "sqrt(2) / 2" },
            { 5, "sqrt(5/8 - sqrt(5)/8)" },
            // { 7, GetSineOfHalvedAngle("2/7", MathS.Sqrt(1 - MathS.Pow("1/6" * (-1 + MathS.Cbrt((7 + 21 * Sqrt(-3)) / 2) + MathS.Cbrt((7 - 21 * Sqrt(-3)) / 2)), 2))) ?? throw new AngouriBugException("Oh no!") }
        };
        
        internal static Entity? SymbolicFormOfSine(Entity angle)
        {
            var piCoef = MathS.UnsafeAndInternal.DivideByEntityStrict(angle, MathS.pi);
            if (piCoef is not Rational rationalCoef)
                return null;
            return null; 
        }
    }
}