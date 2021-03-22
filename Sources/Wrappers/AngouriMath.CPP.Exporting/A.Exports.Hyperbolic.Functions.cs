/* 
 * Copyright (c) 2019-2021 Angouri.
 * AngouriMath is licensed under MIT. 
 * Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
 * Website: https://am.angouri.org.
 */

// This file is auto-generated. Use generate_exports.bat to re-generate it, do not edit the file itself.

using System.Runtime.InteropServices;

namespace AngouriMath.CPP.Exporting
{
    partial class Exports
    {
        [UnmanagedCallersOnly(EntryPoint = "hyperbolic_sinh")]
        public static NErrorCode ESinh(EntityRef arg0, ref EntityRef res)
            => ExceptionEncode(ref res, (arg0), e => AngouriMath.MathS.Hyperbolic.Sinh(e.Entity));

        [UnmanagedCallersOnly(EntryPoint = "hyperbolic_cosh")]
        public static NErrorCode ECosh(EntityRef arg0, ref EntityRef res)
            => ExceptionEncode(ref res, (arg0), e => AngouriMath.MathS.Hyperbolic.Cosh(e.Entity));

        [UnmanagedCallersOnly(EntryPoint = "hyperbolic_tanh")]
        public static NErrorCode ETanh(EntityRef arg0, ref EntityRef res)
            => ExceptionEncode(ref res, (arg0), e => AngouriMath.MathS.Hyperbolic.Tanh(e.Entity));

        [UnmanagedCallersOnly(EntryPoint = "hyperbolic_cotanh")]
        public static NErrorCode ECotanh(EntityRef arg0, ref EntityRef res)
            => ExceptionEncode(ref res, (arg0), e => AngouriMath.MathS.Hyperbolic.Cotanh(e.Entity));

        [UnmanagedCallersOnly(EntryPoint = "hyperbolic_sech")]
        public static NErrorCode ESech(EntityRef arg0, ref EntityRef res)
            => ExceptionEncode(ref res, (arg0), e => AngouriMath.MathS.Hyperbolic.Sech(e.Entity));

        [UnmanagedCallersOnly(EntryPoint = "hyperbolic_cosech")]
        public static NErrorCode ECosech(EntityRef arg0, ref EntityRef res)
            => ExceptionEncode(ref res, (arg0), e => AngouriMath.MathS.Hyperbolic.Cosech(e.Entity));

        [UnmanagedCallersOnly(EntryPoint = "hyperbolic_arsinh")]
        public static NErrorCode EArsinh(EntityRef arg0, ref EntityRef res)
            => ExceptionEncode(ref res, (arg0), e => AngouriMath.MathS.Hyperbolic.Arsinh(e.Entity));

        [UnmanagedCallersOnly(EntryPoint = "hyperbolic_arcosh")]
        public static NErrorCode EArcosh(EntityRef arg0, ref EntityRef res)
            => ExceptionEncode(ref res, (arg0), e => AngouriMath.MathS.Hyperbolic.Arcosh(e.Entity));

        [UnmanagedCallersOnly(EntryPoint = "hyperbolic_artanh")]
        public static NErrorCode EArtanh(EntityRef arg0, ref EntityRef res)
            => ExceptionEncode(ref res, (arg0), e => AngouriMath.MathS.Hyperbolic.Artanh(e.Entity));

        [UnmanagedCallersOnly(EntryPoint = "hyperbolic_arcotanh")]
        public static NErrorCode EArcotanh(EntityRef arg0, ref EntityRef res)
            => ExceptionEncode(ref res, (arg0), e => AngouriMath.MathS.Hyperbolic.Arcotanh(e.Entity));

        [UnmanagedCallersOnly(EntryPoint = "hyperbolic_arsech")]
        public static NErrorCode EArsech(EntityRef arg0, ref EntityRef res)
            => ExceptionEncode(ref res, (arg0), e => AngouriMath.MathS.Hyperbolic.Arsech(e.Entity));

        [UnmanagedCallersOnly(EntryPoint = "hyperbolic_arcosech")]
        public static NErrorCode EArcosech(EntityRef arg0, ref EntityRef res)
            => ExceptionEncode(ref res, (arg0), e => AngouriMath.MathS.Hyperbolic.Arcosech(e.Entity));


    }
}