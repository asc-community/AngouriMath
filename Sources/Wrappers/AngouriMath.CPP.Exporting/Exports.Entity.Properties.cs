//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Linq;
using System.Runtime.InteropServices;

namespace AngouriMath.CPP.Exporting
{
    unsafe partial class Exports
    {
        [UnmanagedCallersOnly(EntryPoint = "entity_nodes")]
        public static NErrorCode EntityNodes(ObjRef exprPtr, NativeArray* res)
            => ExceptionEncode(res, exprPtr,
                exprPtr => NativeArray.Alloc(exprPtr.AsEntity.Nodes)
            );

        [UnmanagedCallersOnly(EntryPoint = "entity_direct_children")]
        public static NErrorCode EntityDirectChildren(ObjRef exprPtr, NativeArray* res)
            => ExceptionEncode(res, exprPtr,
                exprPtr => NativeArray.Alloc(exprPtr.AsEntity.DirectChildren)
            );

        [UnmanagedCallersOnly(EntryPoint = "entity_vars")]
        public static NErrorCode EntityVars(ObjRef exprPtr, NativeArray* res)
            => ExceptionEncode(res, exprPtr,
                exprPtr => NativeArray.Alloc(exprPtr.AsEntity.Vars.Select(v => (Entity)v))
            );

        [UnmanagedCallersOnly(EntryPoint = "entity_vars_and_constants")]
        public static NErrorCode EntityVarsAndConstants(ObjRef exprPtr, NativeArray* res)
            => ExceptionEncode(res, exprPtr,
                exprPtr => NativeArray.Alloc(exprPtr.AsEntity.VarsAndConsts.Select(v => (Entity)v))
            );

        [UnmanagedCallersOnly(EntryPoint = "entity_evaled")]
        public static NErrorCode EntityEvaled(ObjRef exprPtr, ObjRef* res)
            => ExceptionEncode(res, exprPtr,
                exprPtr => exprPtr.AsEntity.Evaled
            );

        [UnmanagedCallersOnly(EntryPoint = "entity_inner_simplified")]
        public static NErrorCode EntityInnerSimplified(ObjRef exprPtr, ObjRef* res)
            => ExceptionEncode(res, exprPtr,
                exprPtr => exprPtr.AsEntity.InnerSimplified
            );
    }
}
