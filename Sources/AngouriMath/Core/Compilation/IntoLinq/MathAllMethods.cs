using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace AngouriMath.Core.Compilation.IntoLinq
{
    // TODO: to improve some of those functions
    internal static class MathAllMethods
    {
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
    }
}
