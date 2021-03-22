using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace AngouriMath.CPP.Exporting
{
    partial class Exports
    {
        [UnmanagedCallersOnly(EntryPoint = "op_entity_add")]
        public static NErrorCode Add(EntityRef left, EntityRef right, ref EntityRef res)
            => ExceptionEncode(ref res, (left, right),
                e => e.left.Entity + e.right.Entity
                );

        [UnmanagedCallersOnly(EntryPoint = "op_entity_sub")]
        public static NErrorCode Subtract(EntityRef left, EntityRef right, ref EntityRef res)
            => ExceptionEncode(ref res, (left, right),
                e => e.left.Entity - e.right.Entity
                );

        [UnmanagedCallersOnly(EntryPoint = "op_entity_mul")]
        public static NErrorCode Multiply(EntityRef left, EntityRef right, ref EntityRef res)
            => ExceptionEncode(ref res, (left, right),
                e => e.left.Entity * e.right.Entity
                );

        [UnmanagedCallersOnly(EntryPoint = "op_entity_div")]
        public static NErrorCode Divide(EntityRef left, EntityRef right, ref EntityRef res)
            => ExceptionEncode(ref res, (left, right),
                e => e.left.Entity / e.right.Entity
                );
    }
}
