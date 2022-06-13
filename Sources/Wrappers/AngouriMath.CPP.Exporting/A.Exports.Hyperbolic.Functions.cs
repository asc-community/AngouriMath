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
        [UnmanagedCallersOnly(EntryPoint = "hyperbolic_sinh")]
        public static NErrorCode ESinh(ObjRef arg0, ObjRef* res)
            => ExceptionEncode(res, (arg0), e => AngouriMath.MathS.Hyperbolic.Sinh(e.AsEntity));

        [UnmanagedCallersOnly(EntryPoint = "hyperbolic_cosh")]
        public static NErrorCode ECosh(ObjRef arg0, ObjRef* res)
            => ExceptionEncode(res, (arg0), e => AngouriMath.MathS.Hyperbolic.Cosh(e.AsEntity));

        [UnmanagedCallersOnly(EntryPoint = "hyperbolic_tanh")]
        public static NErrorCode ETanh(ObjRef arg0, ObjRef* res)
            => ExceptionEncode(res, (arg0), e => AngouriMath.MathS.Hyperbolic.Tanh(e.AsEntity));

        [UnmanagedCallersOnly(EntryPoint = "hyperbolic_cotanh")]
        public static NErrorCode ECotanh(ObjRef arg0, ObjRef* res)
            => ExceptionEncode(res, (arg0), e => AngouriMath.MathS.Hyperbolic.Cotanh(e.AsEntity));

        [UnmanagedCallersOnly(EntryPoint = "hyperbolic_sech")]
        public static NErrorCode ESech(ObjRef arg0, ObjRef* res)
            => ExceptionEncode(res, (arg0), e => AngouriMath.MathS.Hyperbolic.Sech(e.AsEntity));

        [UnmanagedCallersOnly(EntryPoint = "hyperbolic_cosech")]
        public static NErrorCode ECosech(ObjRef arg0, ObjRef* res)
            => ExceptionEncode(res, (arg0), e => AngouriMath.MathS.Hyperbolic.Cosech(e.AsEntity));

        [UnmanagedCallersOnly(EntryPoint = "hyperbolic_arsinh")]
        public static NErrorCode EArsinh(ObjRef arg0, ObjRef* res)
            => ExceptionEncode(res, (arg0), e => AngouriMath.MathS.Hyperbolic.Arsinh(e.AsEntity));

        [UnmanagedCallersOnly(EntryPoint = "hyperbolic_arcosh")]
        public static NErrorCode EArcosh(ObjRef arg0, ObjRef* res)
            => ExceptionEncode(res, (arg0), e => AngouriMath.MathS.Hyperbolic.Arcosh(e.AsEntity));

        [UnmanagedCallersOnly(EntryPoint = "hyperbolic_artanh")]
        public static NErrorCode EArtanh(ObjRef arg0, ObjRef* res)
            => ExceptionEncode(res, (arg0), e => AngouriMath.MathS.Hyperbolic.Artanh(e.AsEntity));

        [UnmanagedCallersOnly(EntryPoint = "hyperbolic_arcotanh")]
        public static NErrorCode EArcotanh(ObjRef arg0, ObjRef* res)
            => ExceptionEncode(res, (arg0), e => AngouriMath.MathS.Hyperbolic.Arcotanh(e.AsEntity));

        [UnmanagedCallersOnly(EntryPoint = "hyperbolic_arsech")]
        public static NErrorCode EArsech(ObjRef arg0, ObjRef* res)
            => ExceptionEncode(res, (arg0), e => AngouriMath.MathS.Hyperbolic.Arsech(e.AsEntity));

        [UnmanagedCallersOnly(EntryPoint = "hyperbolic_arcosech")]
        public static NErrorCode EArcosech(ObjRef arg0, ObjRef* res)
            => ExceptionEncode(res, (arg0), e => AngouriMath.MathS.Hyperbolic.Arcosech(e.AsEntity));


    }
}