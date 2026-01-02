//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

/* THIS FILE IS AUTO-GENERATED */

using System;
using System.Numerics;

namespace AngouriMath.Core.Compilation.IntoLinq
{
    // TODO: to improve some of those functions
    internal static class MathAllMethods
    {

        /* POWERS */

        public static System.Numerics.Complex Log(System.Numerics.Complex a, System.Numerics.Complex b)
            => System.Numerics.Complex.Log(b) / System.Numerics.Complex.Log(a);
        public static double Log(double a, double b)
            => Math.Log(b, a);
        public static float Log(float a, float b)
            => (float)Math.Log(b, a);
        public static long Log(long a, long b)
            => (long)Math.Log(b, a);
        public static int Log(int a, int b)
            => (int)Math.Log(b, a);
        public static BigInteger Log(BigInteger a, BigInteger b)
            => (BigInteger)Math.Log((double)b, (double)a);


        public static System.Numerics.Complex Pow(System.Numerics.Complex a, System.Numerics.Complex b)
            => System.Numerics.Complex.Pow(a, b);
        public static double Pow(double a, double b)
            => Math.Pow(a, b);
        public static float Pow(float a, float b)
            => (float)Math.Pow(a, b);
        public static long Pow(long a, long b)
            => (long)Math.Pow(a, b);
        public static int Pow(int a, int b)
            => (int)Math.Pow(a, b);
        public static BigInteger Pow(BigInteger a, BigInteger b)
            => (BigInteger)Math.Pow((double)a, (double)b);

        /* BUILT-IN TRIGONOMETRY */


        public static System.Numerics.Complex Sin(System.Numerics.Complex a)
            => System.Numerics.Complex.Sin(a);
        public static double Sin(double a)
            => Math.Sin(a);
        public static float Sin(float a)
            => (float)Math.Sin(a);
        public static long Sin(long a)
            => (long)Math.Sin(a);
        public static int Sin(int a)
            => (int)Math.Sin(a);
        public static BigInteger Sin(BigInteger a)
            => (BigInteger)Math.Sin((double)a);


        public static System.Numerics.Complex Cos(System.Numerics.Complex a)
            => System.Numerics.Complex.Cos(a);
        public static double Cos(double a)
            => Math.Cos(a);
        public static float Cos(float a)
            => (float)Math.Cos(a);
        public static long Cos(long a)
            => (long)Math.Cos(a);
        public static int Cos(int a)
            => (int)Math.Cos(a);
        public static BigInteger Cos(BigInteger a)
            => (BigInteger)Math.Cos((double)a);


        public static System.Numerics.Complex Tan(System.Numerics.Complex a)
            => System.Numerics.Complex.Tan(a);
        public static double Tan(double a)
            => Math.Tan(a);
        public static float Tan(float a)
            => (float)Math.Tan(a);
        public static long Tan(long a)
            => (long)Math.Tan(a);
        public static int Tan(int a)
            => (int)Math.Tan(a);
        public static BigInteger Tan(BigInteger a)
            => (BigInteger)Math.Tan((double)a);


        public static System.Numerics.Complex Asin(System.Numerics.Complex a)
            => System.Numerics.Complex.Asin(a);
        public static double Asin(double a)
            => Math.Asin(a);
        public static float Asin(float a)
            => (float)Math.Asin(a);
        public static long Asin(long a)
            => (long)Math.Asin(a);
        public static int Asin(int a)
            => (int)Math.Asin(a);
        public static BigInteger Asin(BigInteger a)
            => (BigInteger)Math.Asin((double)a);


        public static System.Numerics.Complex Acos(System.Numerics.Complex a)
            => System.Numerics.Complex.Acos(a);
        public static double Acos(double a)
            => Math.Acos(a);
        public static float Acos(float a)
            => (float)Math.Acos(a);
        public static long Acos(long a)
            => (long)Math.Acos(a);
        public static int Acos(int a)
            => (int)Math.Acos(a);
        public static BigInteger Acos(BigInteger a)
            => (BigInteger)Math.Acos((double)a);


        public static System.Numerics.Complex Atan(System.Numerics.Complex a)
            => System.Numerics.Complex.Atan(a);
        public static double Atan(double a)
            => Math.Atan(a);
        public static float Atan(float a)
            => (float)Math.Atan(a);
        public static long Atan(long a)
            => (long)Math.Atan(a);
        public static int Atan(int a)
            => (int)Math.Atan(a);
        public static BigInteger Atan(BigInteger a)
            => (BigInteger)Math.Atan((double)a);


        /* POST-INVERSE TRIGONOMETRY */


        public static System.Numerics.Complex Cot(System.Numerics.Complex a)
            => 1 / System.Numerics.Complex.Tan(a);
        public static double Cot(double a)
            => 1 / Math.Tan(a);
        public static float Cot(float a)
            => (float)(1 / Math.Tan(a));
        public static long Cot(long a)
            => (long)(1 / Math.Tan(a));
        public static int Cot(int a)
            => (int)(1 / Math.Tan(a));
        public static BigInteger Cot(BigInteger a)
            => (BigInteger)(1 / Math.Tan((double)a));


        public static System.Numerics.Complex Sec(System.Numerics.Complex a)
            => 1 / System.Numerics.Complex.Cos(a);
        public static double Sec(double a)
            => 1 / Math.Cos(a);
        public static float Sec(float a)
            => (float)(1 / Math.Cos(a));
        public static long Sec(long a)
            => (long)(1 / Math.Cos(a));
        public static int Sec(int a)
            => (int)(1 / Math.Cos(a));
        public static BigInteger Sec(BigInteger a)
            => (BigInteger)(1 / Math.Cos((double)a));


        public static System.Numerics.Complex Csc(System.Numerics.Complex a)
            => 1 / System.Numerics.Complex.Sin(a);
        public static double Csc(double a)
            => 1 / Math.Sin(a);
        public static float Csc(float a)
            => (float)(1 / Math.Sin(a));
        public static long Csc(long a)
            => (long)(1 / Math.Sin(a));
        public static int Csc(int a)
            => (int)(1 / Math.Sin(a));
        public static BigInteger Csc(BigInteger a)
            => (BigInteger)(1 / Math.Sin((double)a));


        /* PRE-INVERSE TRIGONOMETRY */


        public static System.Numerics.Complex Acot(System.Numerics.Complex a)
            => System.Numerics.Complex.Atan(1 / a);
        public static double Acot(double a)
            => Math.Atan(1 / a);
        public static float Acot(float a)
            => (float)Math.Atan(1 / a);
        public static long Acot(long a)
            => (long)Math.Atan(1 / a);
        public static int Acot(int a)
            => (int)Math.Atan(1 / a);
        public static BigInteger Acot(BigInteger a)
            => (BigInteger)Math.Atan(1 / (double)a);


        public static System.Numerics.Complex Asec(System.Numerics.Complex a)
            => System.Numerics.Complex.Acos(1 / a);
        public static double Asec(double a)
            => Math.Acos(1 / a);
        public static float Asec(float a)
            => (float)Math.Acos(1 / a);
        public static long Asec(long a)
            => (long)Math.Acos(1 / a);
        public static int Asec(int a)
            => (int)Math.Acos(1 / a);
        public static BigInteger Asec(BigInteger a)
            => (BigInteger)Math.Acos(1 / (double)a);


        public static System.Numerics.Complex Acsc(System.Numerics.Complex a)
            => System.Numerics.Complex.Asin(1 / a);
        public static double Acsc(double a)
            => Math.Asin(1 / a);
        public static float Acsc(float a)
            => (float)Math.Asin(1 / a);
        public static long Acsc(long a)
            => (long)Math.Asin(1 / a);
        public static int Acsc(int a)
            => (int)Math.Asin(1 / a);
        public static BigInteger Acsc(BigInteger a)
            => (BigInteger)Math.Asin(1 / (double)a);

        
        /* OTHER */
        public static System.Numerics.Complex Abs(System.Numerics.Complex a) => System.Numerics.Complex.Abs(a);
        public static double Abs(double a) => Math.Abs(a);
        public static float Abs(float a) => Math.Abs(a);
        public static long Abs(long a) => Math.Abs(a);
        public static int Abs(int a) => Math.Abs(a);
        public static BigInteger Abs(BigInteger a) => BigInteger.Abs(a);

        public static System.Numerics.Complex Sgn(System.Numerics.Complex a) => a / System.Numerics.Complex.Abs(a);
        public static double Sgn(double a) => a switch { > 0 or double.PositiveInfinity => 1, 0 => 0, < 0 or double.NegativeInfinity => -1, _ => double.NaN };
        public static float Sgn(float a) => a switch { > 0 or float.PositiveInfinity => 1, 0 => 0, < 0 or float.NegativeInfinity => -1, _ => float.NaN };
        public static long Sgn(long a) => a switch { > 0 => 1, 0 => 0, < 0 => -1 };
        public static int Sgn(int a) => a switch { > 0 => 1, 0 => 0, < 0 => -1 };
        public static BigInteger Sgn(BigInteger a)
        {
            if (a > BigInteger.Zero)
                return 1;
            if (a < BigInteger.Zero)
                return -1;
            return 0;
        }
    }
}
