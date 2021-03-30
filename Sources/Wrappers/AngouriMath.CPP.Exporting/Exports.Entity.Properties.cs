using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace AngouriMath.CPP.Exporting
{
    partial class Exports
    {
        [UnmanagedCallersOnly(EntryPoint = "entity_nodes")]
        public static NErrorCode EntityNodes(ObjRef exprPtr, ref NativeArray res)
            => ExceptionEncode(ref res, exprPtr,
                exprPtr => NativeArray.Alloc(exprPtr.AsEntity.Nodes)
            );

        [UnmanagedCallersOnly(EntryPoint = "entity_vars")]
        public static NErrorCode EntityVars(ObjRef exprPtr, ref NativeArray res)
            => ExceptionEncode(ref res, exprPtr,
                exprPtr => NativeArray.Alloc(exprPtr.AsEntity.Vars)
            );

        [UnmanagedCallersOnly(EntryPoint = "entity_vars_and_constants")]
        public static NErrorCode EntityVarsAndConstants(ObjRef exprPtr, ref NativeArray res)
            => ExceptionEncode(ref res, exprPtr,
                exprPtr => NativeArray.Alloc(exprPtr.AsEntity.VarsAndConsts)
            );

        [UnmanagedCallersOnly(EntryPoint = "entity_alternate")]
        public static NErrorCode EntityVarsAdndConstants(ObjRef exprPtr, ref NativeArray res)
            => ExceptionEncode(ref res, exprPtr,
                exprPtr => NativeArray.Alloc(exprPtr.AsEntity.Alternate(4))
            );
    }
}
