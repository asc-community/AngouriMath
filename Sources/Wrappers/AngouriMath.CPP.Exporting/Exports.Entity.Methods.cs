/* 
 * Copyright (c) 2019-2021 Angouri.
 * AngouriMath is licensed under MIT. 
 * Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
 * Website: https://am.angouri.org.
 */

using AngouriMath.Core;
using System;
using System.Runtime.InteropServices;
using static AngouriMath.Entity;

namespace AngouriMath.CPP.Exporting
{
    partial class Exports
    {
        #region String as output

        [UnmanagedCallersOnly(EntryPoint = "entity_to_string")]
        public static NErrorCode EntityToString(ObjRef exprPtr, ref IntPtr res)
            => ExceptionEncode(ref res, exprPtr,
                exprPtr => Marshal.StringToHGlobalAnsi(exprPtr.AsEntity.ToString())
            );

        [UnmanagedCallersOnly(EntryPoint = "entity_latexise")]
        public static NErrorCode EntityToLatex(ObjRef exprPtr, ref IntPtr res)
            => ExceptionEncode(ref res, exprPtr,
                exprPtr => Marshal.StringToHGlobalAnsi(exprPtr.AsEntity.Latexise())
            );

        #endregion

        #region Calculus
        [UnmanagedCallersOnly(EntryPoint = "entity_differentiate")]
        public static NErrorCode Differentiate(ObjRef exprPtr, ObjRef varPtr, ref ObjRef res)
            => ExceptionEncode(ref res, (exprPtr, varPtr), static e =>
                {
                    var expr = e.exprPtr.AsEntity;
                    var var = (Variable)e.varPtr.AsEntity;
                    return expr.Differentiate(var);
                });

        [UnmanagedCallersOnly(EntryPoint = "entity_integrate")]
        public static NErrorCode Integrate(ObjRef exprPtr, ObjRef varPtr, ref ObjRef res)
            => ExceptionEncode(ref res, (exprPtr, varPtr), static e =>
            {
                var expr = e.exprPtr.AsEntity;
                var var = (Variable)e.varPtr.AsEntity;
                return expr.Integrate(var);
            });

        [UnmanagedCallersOnly(EntryPoint = "entity_limit")]
        public static NErrorCode Limit(ObjRef exprPtr, ObjRef varPtr, ObjRef dest, ApproachFrom from, ref ObjRef res)
            => ExceptionEncode(ref res, (exprPtr, varPtr, dest, from), static e =>
            {
                var expr = e.exprPtr.AsEntity;
                var var = (Variable)e.varPtr.AsEntity;
                var dest = e.dest.AsEntity;
                return expr.Limit(var, dest, e.from);
            });
        #endregion

        #region Solvers
        [UnmanagedCallersOnly(EntryPoint = "entity_solve")]
        public static NErrorCode Solve(ObjRef exprPtr, ObjRef varPtr, ref ObjRef res)
            => ExceptionEncode(ref res, (exprPtr, varPtr), static e =>
            {
                var expr = ObjStorage<Entity>.Get(e.exprPtr);
                var var = (Variable)ObjStorage<Entity>.Get(e.varPtr);
                return ObjStorage<Entity>.Alloc(expr.Solve(var));
            });

        [UnmanagedCallersOnly(EntryPoint = "entity_solve_equation")]
        public static NErrorCode SolveEquation(ObjRef exprPtr, ObjRef varPtr, ref ObjRef res)
            => ExceptionEncode(ref res, (exprPtr, varPtr), static e =>
            {
                var expr = ObjStorage<Entity>.Get(e.exprPtr);
                var var = (Variable)ObjStorage<Entity>.Get(e.varPtr);
                return ObjStorage<Entity>.Alloc(expr.SolveEquation(var));
            });
        #endregion
    }
}