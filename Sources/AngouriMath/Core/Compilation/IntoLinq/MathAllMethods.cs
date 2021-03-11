
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace AngouriMath.Core.Compilation.IntoLinq
{
    // TODO: to improve some of those functions
    internal static class MathAllMethods
    {

        /* POWERS */

        internal static Complex Log(Complex a, Complex b)
            => Complex.Log(a) / Complex.Log(b);
        internal static double Log(double a, double b)
            => Math.Log(a, b);
        internal static float Log(float a, float b)
            => (float)Math.Log(a, b);
        internal static long Log(long a, long b)
            => (long)Math.Log(a, b);
        internal static int Log(int a, int b)
            => (int)Math.Log(a, b);
        internal static BigInteger Log(BigInteger a, BigInteger b)
            => (BigInteger)Math.Log((double)a, (double)b);


        internal static Complex Pow(Complex a, Complex b)
            => Complex.Pow(a, b);
        internal static double Pow(double a, double b)
            => Math.Pow(a, b);
        internal static float Pow(float a, float b)
            => (float)Math.Pow(a, b);
        internal static long Pow(long a, long b)
            => (long)Math.Pow(a, b);
        internal static int Pow(int a, int b)
            => (int)Math.Pow(a, b);
        internal static BigInteger Pow(BigInteger a, BigInteger b)
            => (BigInteger)Math.Pow((double)a, (double)b);

        /* BUILT-IN TRIGONOMETRY */


        internal static Complex Sin(Complex a)
            => Complex.Sin(a);
        internal static double Sin(double a)
            => Math.Sin(a);
        internal static float Sin(float a)
            => (float)Math.Sin(a);
        internal static long Sin(long a)
            => (long)Math.Sin(a);
        internal static int Sin(int a)
            => (int)Math.Sin(a);
        internal static BigInteger Sin(BigInteger a)
            => (BigInteger)Math.Sin((double)a);


        internal static Complex Cos(Complex a)
            => Complex.Cos(a);
        internal static double Cos(double a)
            => Math.Cos(a);
        internal static float Cos(float a)
            => (float)Math.Cos(a);
        internal static long Cos(long a)
            => (long)Math.Cos(a);
        internal static int Cos(int a)
            => (int)Math.Cos(a);
        internal static BigInteger Cos(BigInteger a)
            => (BigInteger)Math.Cos((double)a);


        internal static Complex Tan(Complex a)
            => Complex.Tan(a);
        internal static double Tan(double a)
            => Math.Tan(a);
        internal static float Tan(float a)
            => (float)Math.Tan(a);
        internal static long Tan(long a)
            => (long)Math.Tan(a);
        internal static int Tan(int a)
            => (int)Math.Tan(a);
        internal static BigInteger Tan(BigInteger a)
            => (BigInteger)Math.Tan((double)a);


        internal static Complex Asin(Complex a)
            => Complex.Asin(a);
        internal static double Asin(double a)
            => Math.Asin(a);
        internal static float Asin(float a)
            => (float)Math.Asin(a);
        internal static long Asin(long a)
            => (long)Math.Asin(a);
        internal static int Asin(int a)
            => (int)Math.Asin(a);
        internal static BigInteger Asin(BigInteger a)
            => (BigInteger)Math.Asin((double)a);


        internal static Complex Acos(Complex a)
            => Complex.Acos(a);
        internal static double Acos(double a)
            => Math.Acos(a);
        internal static float Acos(float a)
            => (float)Math.Acos(a);
        internal static long Acos(long a)
            => (long)Math.Acos(a);
        internal static int Acos(int a)
            => (int)Math.Acos(a);
        internal static BigInteger Acos(BigInteger a)
            => (BigInteger)Math.Acos((double)a);


        internal static Complex Atan(Complex a)
            => Complex.Atan(a);
        internal static double Atan(double a)
            => Math.Atan(a);
        internal static float Atan(float a)
            => (float)Math.Atan(a);
        internal static long Atan(long a)
            => (long)Math.Atan(a);
        internal static int Atan(int a)
            => (int)Math.Atan(a);
        internal static BigInteger Atan(BigInteger a)
            => (BigInteger)Math.Atan((double)a);


        /* POST-INVERSE TRIGONOMETRY */


        internal static Complex Cotan(Complex a)
            => 1 / Complex.Tan(a);
        internal static double Cotan(double a)
            => 1 / Math.Tan(a);
        internal static float Cotan(float a)
            => (float)(1 / Math.Tan(a));
        internal static long Cotan(long a)
            => (long)(1 / Math.Tan(a));
        internal static int Cotan(int a)
            => (int)(1 / Math.Tan(a));
        internal static BigInteger Cotan(BigInteger a)
            => (BigInteger)(1 / Math.Tan((double)a));


        internal static Complex Sec(Complex a)
            => 1 / Complex.Cos(a);
        internal static double Sec(double a)
            => 1 / Math.Cos(a);
        internal static float Sec(float a)
            => (float)(1 / Math.Cos(a));
        internal static long Sec(long a)
            => (long)(1 / Math.Cos(a));
        internal static int Sec(int a)
            => (int)(1 / Math.Cos(a));
        internal static BigInteger Sec(BigInteger a)
            => (BigInteger)(1 / Math.Cos((double)a));


        internal static Complex Cosec(Complex a)
            => 1 / Complex.Sin(a);
        internal static double Cosec(double a)
            => 1 / Math.Sin(a);
        internal static float Cosec(float a)
            => (float)(1 / Math.Sin(a));
        internal static long Cosec(long a)
            => (long)(1 / Math.Sin(a));
        internal static int Cosec(int a)
            => (int)(1 / Math.Sin(a));
        internal static BigInteger Cosec(BigInteger a)
            => (BigInteger)(1 / Math.Sin((double)a));


        /* PRE-INVERSE TRIGONOMETRY */


        internal static Complex Acot(Complex a)
            => Complex.Atan(1 / a);
        internal static double Acot(double a)
            => Math.Atan(1 / a);
        internal static float Acot(float a)
            => (float)Math.Atan(1 / a);
        internal static long Acot(long a)
            => (long)Math.Atan(1 / a);
        internal static int Acot(int a)
            => (int)Math.Atan(1 / a);
        internal static BigInteger Acot(BigInteger a)
            => (BigInteger)Math.Atan(1 / (double)a);


        internal static Complex Asec(Complex a)
            => Complex.Acos(1 / a);
        internal static double Asec(double a)
            => Math.Acos(1 / a);
        internal static float Asec(float a)
            => (float)Math.Acos(1 / a);
        internal static long Asec(long a)
            => (long)Math.Acos(1 / a);
        internal static int Asec(int a)
            => (int)Math.Acos(1 / a);
        internal static BigInteger Asec(BigInteger a)
            => (BigInteger)Math.Acos(1 / (double)a);


        internal static Complex Acsc(Complex a)
            => Complex.Asin(1 / a);
        internal static double Acsc(double a)
            => Math.Asin(1 / a);
        internal static float Acsc(float a)
            => (float)Math.Asin(1 / a);
        internal static long Acsc(long a)
            => (long)Math.Asin(1 / a);
        internal static int Acsc(int a)
            => (int)Math.Asin(1 / a);
        internal static BigInteger Acsc(BigInteger a)
            => (BigInteger)Math.Asin(1 / (double)a);


    }
}
