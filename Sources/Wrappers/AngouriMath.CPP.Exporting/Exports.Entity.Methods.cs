//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core;
using System;
using System.Runtime.InteropServices;
using static AngouriMath.Entity;

namespace AngouriMath.CPP.Exporting
{
    unsafe partial class Exports
    {
        #region String as output

        [UnmanagedCallersOnly(EntryPoint = "entity_to_string")]
        public static NErrorCode EntityToString(ObjRef exprPtr, IntPtr* res)
            => ExceptionEncode(res, exprPtr,
                exprPtr => Marshal.StringToHGlobalAnsi(exprPtr.AsEntity.ToString())
            );

        [UnmanagedCallersOnly(EntryPoint = "entity_latexise")]
        public static NErrorCode EntityToLatex(ObjRef exprPtr, IntPtr* res)
            => ExceptionEncode(res, exprPtr,
                exprPtr => Marshal.StringToHGlobalAnsi(exprPtr.AsEntity.Latexise())
            );

        #endregion

        #region Calculus
        [UnmanagedCallersOnly(EntryPoint = "entity_differentiate")]
        public static NErrorCode Differentiate(ObjRef exprPtr, ObjRef varPtr, ObjRef* res)
            => ExceptionEncode(res, (exprPtr, varPtr), static e =>
                {
                    var expr = e.exprPtr.AsEntity;
                    var var = (Variable)e.varPtr.AsEntity;
                    return expr.Differentiate(var);
                });

        [UnmanagedCallersOnly(EntryPoint = "entity_integrate")]
        public static NErrorCode Integrate(ObjRef exprPtr, ObjRef varPtr, ObjRef* res)
            => ExceptionEncode(res, (exprPtr, varPtr), static e =>
            {
                var expr = e.exprPtr.AsEntity;
                var var = (Variable)e.varPtr.AsEntity;
                return expr.Integrate(var);
            });

        [UnmanagedCallersOnly(EntryPoint = "entity_limit")]
        public static NErrorCode Limit(ObjRef exprPtr, ObjRef varPtr, ObjRef dest, ApproachFrom from, ObjRef* res)
            => ExceptionEncode(res, (exprPtr, varPtr, dest, from), static e =>
            {
                var expr = e.exprPtr.AsEntity;
                var var = (Variable)e.varPtr.AsEntity;
                var dest = e.dest.AsEntity;
                return expr.Limit(var, dest, e.from);
            });
        #endregion

        #region Solvers
        [UnmanagedCallersOnly(EntryPoint = "entity_solve")]
        public static NErrorCode Solve(ObjRef exprPtr, ObjRef varPtr, ObjRef* res)
            => ExceptionEncode(res, (exprPtr, varPtr), static e =>
            {
                var expr = ObjStorage<Entity>.Get(e.exprPtr);
                var var = (Variable)ObjStorage<Entity>.Get(e.varPtr);
                return ObjStorage<Entity>.Alloc(expr.Solve(var));
            });

        [UnmanagedCallersOnly(EntryPoint = "entity_solve_equation")]
        public static NErrorCode SolveEquation(ObjRef exprPtr, ObjRef varPtr, ObjRef* res)
            => ExceptionEncode(res, (exprPtr, varPtr), static e =>
            {
                var expr = ObjStorage<Entity>.Get(e.exprPtr);
                var var = (Variable)ObjStorage<Entity>.Get(e.varPtr);
                return ObjStorage<Entity>.Alloc(expr.SolveEquation(var));
            });
        #endregion

        #region Casts

        [UnmanagedCallersOnly(EntryPoint = "entity_to_long")]
        public static NErrorCode ToLong(ObjRef expr, long* res)
            => ExceptionEncode(res, expr, static e => (long)(Number)e.AsEntity);


        [UnmanagedCallersOnly(EntryPoint = "entity_to_rational")]
        public static NErrorCode ToRational(ObjRef expr, (long, long)* res)
            => ExceptionEncode(res, expr, static e =>
            {
                var rat = (Number.Rational)e.AsEntity;
                return ((long)rat.Numerator, (long)rat.Denominator);
            });

        [UnmanagedCallersOnly(EntryPoint = "entity_to_double")]
        public static NErrorCode ToDouble(ObjRef expr, double* res)
            => ExceptionEncode(res, expr, static e => (double)(Number)e.AsEntity);


        [UnmanagedCallersOnly(EntryPoint = "entity_to_complex")]
        public static NErrorCode ToComplex(ObjRef expr, (double, double)* res)
            => ExceptionEncode(res, expr, static e =>
            {
                var rat = (Number.Complex)e.AsEntity;
                return ((double)rat.RealPart, (double)rat.ImaginaryPart);
            });

        #endregion

        #region Simplification

        [UnmanagedCallersOnly(EntryPoint = "entity_alternate")]
        public static NErrorCode EntityAlternate(ObjRef exprPtr, NativeArray* res)
            => ExceptionEncode(res, exprPtr,
                exprPtr => NativeArray.Alloc(exprPtr.AsEntity.Alternate(4))
            );

        [UnmanagedCallersOnly(EntryPoint = "entity_simplify")]
        public static NErrorCode EntitySimplify(ObjRef exprPtr, ObjRef* res)
            => ExceptionEncode(res, exprPtr,
                exprPtr => exprPtr.AsEntity.Simplify()
            );
        #endregion
    }
}