/* 
 * Copyright (c) 2019-2021 Angouri.
 * AngouriMath is licensed under MIT. 
 * Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
 * Website: https://am.angouri.org.
 */

// This file is auto-generated. Use export_cs_build.bat to re-generate it, do not edit the file itself.

using System.Runtime.InteropServices;

namespace AngouriMath.CPP.Exporting
{
    partial class Exports
    {
        [UnmanagedCallersOnly(EntryPoint = "math_s_sin")]
        public static NErrorCode ESin(EntityRef arg0, ref EntityRef res)
            => ExceptionEncode(ref res, (arg0), e => AngouriMath.MathS.Sin(e.Entity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_cos")]
        public static NErrorCode ECos(EntityRef arg0, ref EntityRef res)
            => ExceptionEncode(ref res, (arg0), e => AngouriMath.MathS.Cos(e.Entity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_sec")]
        public static NErrorCode ESec(EntityRef arg0, ref EntityRef res)
            => ExceptionEncode(ref res, (arg0), e => AngouriMath.MathS.Sec(e.Entity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_cosec")]
        public static NErrorCode ECosec(EntityRef arg0, ref EntityRef res)
            => ExceptionEncode(ref res, (arg0), e => AngouriMath.MathS.Cosec(e.Entity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_log")]
        public static NErrorCode ELog(EntityRef arg0, EntityRef arg1, ref EntityRef res)
            => ExceptionEncode(ref res, (arg0, arg1), e => AngouriMath.MathS.Log(e.arg0.Entity, e.arg1.Entity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_log")]
        public static NErrorCode ELog(EntityRef arg0, ref EntityRef res)
            => ExceptionEncode(ref res, (arg0), e => AngouriMath.MathS.Log(e.Entity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_pow")]
        public static NErrorCode EPow(EntityRef arg0, EntityRef arg1, ref EntityRef res)
            => ExceptionEncode(ref res, (arg0, arg1), e => AngouriMath.MathS.Pow(e.arg0.Entity, e.arg1.Entity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_sqrt")]
        public static NErrorCode ESqrt(EntityRef arg0, ref EntityRef res)
            => ExceptionEncode(ref res, (arg0), e => AngouriMath.MathS.Sqrt(e.Entity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_cbrt")]
        public static NErrorCode ECbrt(EntityRef arg0, ref EntityRef res)
            => ExceptionEncode(ref res, (arg0), e => AngouriMath.MathS.Cbrt(e.Entity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_sqr")]
        public static NErrorCode ESqr(EntityRef arg0, ref EntityRef res)
            => ExceptionEncode(ref res, (arg0), e => AngouriMath.MathS.Sqr(e.Entity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_tan")]
        public static NErrorCode ETan(EntityRef arg0, ref EntityRef res)
            => ExceptionEncode(ref res, (arg0), e => AngouriMath.MathS.Tan(e.Entity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_cotan")]
        public static NErrorCode ECotan(EntityRef arg0, ref EntityRef res)
            => ExceptionEncode(ref res, (arg0), e => AngouriMath.MathS.Cotan(e.Entity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_arcsin")]
        public static NErrorCode EArcsin(EntityRef arg0, ref EntityRef res)
            => ExceptionEncode(ref res, (arg0), e => AngouriMath.MathS.Arcsin(e.Entity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_arccos")]
        public static NErrorCode EArccos(EntityRef arg0, ref EntityRef res)
            => ExceptionEncode(ref res, (arg0), e => AngouriMath.MathS.Arccos(e.Entity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_arctan")]
        public static NErrorCode EArctan(EntityRef arg0, ref EntityRef res)
            => ExceptionEncode(ref res, (arg0), e => AngouriMath.MathS.Arctan(e.Entity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_arccotan")]
        public static NErrorCode EArccotan(EntityRef arg0, ref EntityRef res)
            => ExceptionEncode(ref res, (arg0), e => AngouriMath.MathS.Arccotan(e.Entity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_arcsec")]
        public static NErrorCode EArcsec(EntityRef arg0, ref EntityRef res)
            => ExceptionEncode(ref res, (arg0), e => AngouriMath.MathS.Arcsec(e.Entity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_arccosec")]
        public static NErrorCode EArccosec(EntityRef arg0, ref EntityRef res)
            => ExceptionEncode(ref res, (arg0), e => AngouriMath.MathS.Arccosec(e.Entity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_ln")]
        public static NErrorCode ELn(EntityRef arg0, ref EntityRef res)
            => ExceptionEncode(ref res, (arg0), e => AngouriMath.MathS.Ln(e.Entity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_factorial")]
        public static NErrorCode EFactorial(EntityRef arg0, ref EntityRef res)
            => ExceptionEncode(ref res, (arg0), e => AngouriMath.MathS.Factorial(e.Entity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_gamma")]
        public static NErrorCode EGamma(EntityRef arg0, ref EntityRef res)
            => ExceptionEncode(ref res, (arg0), e => AngouriMath.MathS.Gamma(e.Entity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_signum")]
        public static NErrorCode ESignum(EntityRef arg0, ref EntityRef res)
            => ExceptionEncode(ref res, (arg0), e => AngouriMath.MathS.Signum(e.Entity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_abs")]
        public static NErrorCode EAbs(EntityRef arg0, ref EntityRef res)
            => ExceptionEncode(ref res, (arg0), e => AngouriMath.MathS.Abs(e.Entity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_negation")]
        public static NErrorCode ENegation(EntityRef arg0, ref EntityRef res)
            => ExceptionEncode(ref res, (arg0), e => AngouriMath.MathS.Negation(e.Entity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_provided")]
        public static NErrorCode EProvided(EntityRef arg0, EntityRef arg1, ref EntityRef res)
            => ExceptionEncode(ref res, (arg0, arg1), e => AngouriMath.MathS.Provided(e.arg0.Entity, e.arg1.Entity));


    }
}