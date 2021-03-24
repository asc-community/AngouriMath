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
    }
}
