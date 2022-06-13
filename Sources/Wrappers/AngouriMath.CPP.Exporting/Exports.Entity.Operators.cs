//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Runtime.InteropServices;

namespace AngouriMath.CPP.Exporting
{
    unsafe partial class Exports
    {
        [UnmanagedCallersOnly(EntryPoint = "op_entity_add")]
        public static NErrorCode Add(ObjRef left, ObjRef right, ObjRef* res)
            => ExceptionEncode(res, (left, right),
                e => e.left.AsEntity + e.right.AsEntity
                );

        [UnmanagedCallersOnly(EntryPoint = "op_entity_sub")]
        public static NErrorCode Subtract(ObjRef left, ObjRef right, ObjRef* res)
            => ExceptionEncode(res, (left, right),
                e => e.left.AsEntity - e.right.AsEntity
                );

        [UnmanagedCallersOnly(EntryPoint = "op_entity_mul")]
        public static NErrorCode Multiply(ObjRef left, ObjRef right, ObjRef* res)
            => ExceptionEncode(res, (left, right),
                e => e.left.AsEntity * e.right.AsEntity
                );

        [UnmanagedCallersOnly(EntryPoint = "op_entity_div")]
        public static NErrorCode Divide(ObjRef left, ObjRef right, ObjRef* res)
            => ExceptionEncode(res, (left, right),
                e => e.left.AsEntity / e.right.AsEntity
                );

        [UnmanagedCallersOnly(EntryPoint = "op_entity_less")]
        public static NErrorCode Less(ObjRef left, ObjRef right, ObjRef* res)
            => ExceptionEncode(res, (left, right),
                e => e.left.AsEntity < e.right.AsEntity
                );

        [UnmanagedCallersOnly(EntryPoint = "op_entity_greater")]
        public static NErrorCode Greater(ObjRef left, ObjRef right, ObjRef* res)
            => ExceptionEncode(res, (left, right),
                e => e.left.AsEntity > e.right.AsEntity
                );

        [UnmanagedCallersOnly(EntryPoint = "op_entity_less_or_equal")]
        public static NErrorCode LessOrEqual(ObjRef left, ObjRef right, ObjRef* res)
            => ExceptionEncode(res, (left, right),
                e => e.left.AsEntity <= e.right.AsEntity
                );

        [UnmanagedCallersOnly(EntryPoint = "op_entity_greater_or_equal")]
        public static NErrorCode GreaterOrEqual(ObjRef left, ObjRef right, ObjRef* res)
            => ExceptionEncode(res, (left, right),
                e => e.left.AsEntity >= e.right.AsEntity
                );

        [UnmanagedCallersOnly(EntryPoint = "op_entity_equal")]
        public static NErrorCode OpEqual(ObjRef left, ObjRef right, NativeBool* res)
            => ExceptionEncode(res, (left, right),
                e => e.left.AsEntity == e.right.AsEntity
                );
    }
}
