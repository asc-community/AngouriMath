//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

// This file is auto-generated. Use generate_exports.bat to re-generate it, do not edit the file itself.

using System.Runtime.InteropServices;

namespace AngouriMath.CPP.Exporting
{
    unsafe partial class Exports
    {
        [UnmanagedCallersOnly(EntryPoint = "math_s_sin")]
        public static NErrorCode ESin(ObjRef arg0, ObjRef* res)
            => ExceptionEncode(res, (arg0), e => AngouriMath.MathS.Sin(e.AsEntity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_cos")]
        public static NErrorCode ECos(ObjRef arg0, ObjRef* res)
            => ExceptionEncode(res, (arg0), e => AngouriMath.MathS.Cos(e.AsEntity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_sec")]
        public static NErrorCode ESec(ObjRef arg0, ObjRef* res)
            => ExceptionEncode(res, (arg0), e => AngouriMath.MathS.Sec(e.AsEntity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_cosec")]
        public static NErrorCode ECosec(ObjRef arg0, ObjRef* res)
            => ExceptionEncode(res, (arg0), e => AngouriMath.MathS.Cosec(e.AsEntity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_log")]
        public static NErrorCode ELog(ObjRef arg0, ObjRef arg1, ObjRef* res)
            => ExceptionEncode(res, (arg0, arg1), e => AngouriMath.MathS.Log(e.arg0.AsEntity, e.arg1.AsEntity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_pow")]
        public static NErrorCode EPow(ObjRef arg0, ObjRef arg1, ObjRef* res)
            => ExceptionEncode(res, (arg0, arg1), e => AngouriMath.MathS.Pow(e.arg0.AsEntity, e.arg1.AsEntity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_sqrt")]
        public static NErrorCode ESqrt(ObjRef arg0, ObjRef* res)
            => ExceptionEncode(res, (arg0), e => AngouriMath.MathS.Sqrt(e.AsEntity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_cbrt")]
        public static NErrorCode ECbrt(ObjRef arg0, ObjRef* res)
            => ExceptionEncode(res, (arg0), e => AngouriMath.MathS.Cbrt(e.AsEntity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_sqr")]
        public static NErrorCode ESqr(ObjRef arg0, ObjRef* res)
            => ExceptionEncode(res, (arg0), e => AngouriMath.MathS.Sqr(e.AsEntity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_tan")]
        public static NErrorCode ETan(ObjRef arg0, ObjRef* res)
            => ExceptionEncode(res, (arg0), e => AngouriMath.MathS.Tan(e.AsEntity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_cotan")]
        public static NErrorCode ECotan(ObjRef arg0, ObjRef* res)
            => ExceptionEncode(res, (arg0), e => AngouriMath.MathS.Cotan(e.AsEntity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_arcsin")]
        public static NErrorCode EArcsin(ObjRef arg0, ObjRef* res)
            => ExceptionEncode(res, (arg0), e => AngouriMath.MathS.Arcsin(e.AsEntity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_arccos")]
        public static NErrorCode EArccos(ObjRef arg0, ObjRef* res)
            => ExceptionEncode(res, (arg0), e => AngouriMath.MathS.Arccos(e.AsEntity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_arctan")]
        public static NErrorCode EArctan(ObjRef arg0, ObjRef* res)
            => ExceptionEncode(res, (arg0), e => AngouriMath.MathS.Arctan(e.AsEntity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_arccotan")]
        public static NErrorCode EArccotan(ObjRef arg0, ObjRef* res)
            => ExceptionEncode(res, (arg0), e => AngouriMath.MathS.Arccotan(e.AsEntity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_arcsec")]
        public static NErrorCode EArcsec(ObjRef arg0, ObjRef* res)
            => ExceptionEncode(res, (arg0), e => AngouriMath.MathS.Arcsec(e.AsEntity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_arccosec")]
        public static NErrorCode EArccosec(ObjRef arg0, ObjRef* res)
            => ExceptionEncode(res, (arg0), e => AngouriMath.MathS.Arccosec(e.AsEntity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_ln")]
        public static NErrorCode ELn(ObjRef arg0, ObjRef* res)
            => ExceptionEncode(res, (arg0), e => AngouriMath.MathS.Ln(e.AsEntity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_factorial")]
        public static NErrorCode EFactorial(ObjRef arg0, ObjRef* res)
            => ExceptionEncode(res, (arg0), e => AngouriMath.MathS.Factorial(e.AsEntity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_gamma")]
        public static NErrorCode EGamma(ObjRef arg0, ObjRef* res)
            => ExceptionEncode(res, (arg0), e => AngouriMath.MathS.Gamma(e.AsEntity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_signum")]
        public static NErrorCode ESignum(ObjRef arg0, ObjRef* res)
            => ExceptionEncode(res, (arg0), e => AngouriMath.MathS.Signum(e.AsEntity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_abs")]
        public static NErrorCode EAbs(ObjRef arg0, ObjRef* res)
            => ExceptionEncode(res, (arg0), e => AngouriMath.MathS.Abs(e.AsEntity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_negation")]
        public static NErrorCode ENegation(ObjRef arg0, ObjRef* res)
            => ExceptionEncode(res, (arg0), e => AngouriMath.MathS.Negation(e.AsEntity));

        [UnmanagedCallersOnly(EntryPoint = "math_s_provided")]
        public static NErrorCode EProvided(ObjRef arg0, ObjRef arg1, ObjRef* res)
            => ExceptionEncode(res, (arg0, arg1), e => AngouriMath.MathS.Provided(e.arg0.AsEntity, e.arg1.AsEntity));


    }
}