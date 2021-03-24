using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace AngouriMath.CPP.Exporting
{
    partial class Exports
    {
        [UnmanagedCallersOnly(EntryPoint = "op_entity_add")]
        public static NErrorCode Add(ObjRef left, ObjRef right, ref ObjRef res)
            => ExceptionEncode(ref res, (left, right),
                e => e.left.AsEntity + e.right.AsEntity
                );

        [UnmanagedCallersOnly(EntryPoint = "op_entity_sub")]
        public static NErrorCode Subtract(ObjRef left, ObjRef right, ref ObjRef res)
            => ExceptionEncode(ref res, (left, right),
                e => e.left.AsEntity - e.right.AsEntity
                );

        [UnmanagedCallersOnly(EntryPoint = "op_entity_mul")]
        public static NErrorCode Multiply(ObjRef left, ObjRef right, ref ObjRef res)
            => ExceptionEncode(ref res, (left, right),
                e => e.left.AsEntity * e.right.AsEntity
                );

        [UnmanagedCallersOnly(EntryPoint = "op_entity_div")]
        public static NErrorCode Divide(ObjRef left, ObjRef right, ref ObjRef res)
            => ExceptionEncode(ref res, (left, right),
                e => e.left.AsEntity / e.right.AsEntity
                );
    }
}
