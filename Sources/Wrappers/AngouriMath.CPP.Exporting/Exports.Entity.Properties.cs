using System.Linq;
using System.Runtime.InteropServices;

namespace AngouriMath.CPP.Exporting
{
    partial class Exports
    {
        [UnmanagedCallersOnly(EntryPoint = "entity_nodes")]
        public static NErrorCode EntityNodes(ObjRef exprPtr, ref NativeArray res)
            => ExceptionEncode(ref res, exprPtr,
                exprPtr => NativeArray.Alloc(exprPtr.AsEntity.Nodes)
            );

        [UnmanagedCallersOnly(EntryPoint = "entity_direct_children")]
        public static NErrorCode EntityDirectChildren(ObjRef exprPtr, ref NativeArray res)
            => ExceptionEncode(ref res, exprPtr,
                exprPtr => NativeArray.Alloc(exprPtr.AsEntity.DirectChildren)
            );

        [UnmanagedCallersOnly(EntryPoint = "entity_vars")]
        public static NErrorCode EntityVars(ObjRef exprPtr, ref NativeArray res)
            => ExceptionEncode(ref res, exprPtr,
                exprPtr => NativeArray.Alloc(exprPtr.AsEntity.Vars.Select(v => (Entity)v))
            );

        [UnmanagedCallersOnly(EntryPoint = "entity_vars_and_constants")]
        public static NErrorCode EntityVarsAndConstants(ObjRef exprPtr, ref NativeArray res)
            => ExceptionEncode(ref res, exprPtr,
                exprPtr => NativeArray.Alloc(exprPtr.AsEntity.VarsAndConsts.Select(v => (Entity)v))
            );

        [UnmanagedCallersOnly(EntryPoint = "entity_evaled")]
        public static NErrorCode EntityEvaled(ObjRef exprPtr, ref ObjRef res)
            => ExceptionEncode(ref res, exprPtr,
                exprPtr => exprPtr.AsEntity.Evaled
            );

        [UnmanagedCallersOnly(EntryPoint = "entity_inner_simplified")]
        public static NErrorCode EntityInnerSimplified(ObjRef exprPtr, ref ObjRef res)
            => ExceptionEncode(ref res, exprPtr,
                exprPtr => exprPtr.AsEntity.InnerSimplified
            );
    }
}
