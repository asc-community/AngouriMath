using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace AngouriMath.CPP.Exporting
{
    partial class Exports
    {
        [UnmanagedCallersOnly(EntryPoint = "maths_from_string")]
        public static NErrorCode Parse(IntPtr strPtr, ref EntityRef res)
            => ExceptionEncode(ref res, strPtr, static strPtr =>
            {
                var str = Marshal.PtrToStringAnsi(strPtr);
                return ObjStorage<Entity>.Alloc(str);
            });

        [UnmanagedCallersOnly(EntryPoint = "entity_to_string")]
        public static NErrorCode EntityToString(EntityRef exprPtr, ref IntPtr res)
            => ExceptionEncode(ref res, exprPtr,
                exprPtr => Marshal.StringToHGlobalAnsi(exprPtr.Entity.ToString())
            );

        [UnmanagedCallersOnly(EntryPoint = "entity_latexise")]
        public static NErrorCode EntityToLatex(EntityRef exprPtr, ref IntPtr res)
            => ExceptionEncode(ref res, exprPtr,
                exprPtr => Marshal.StringToHGlobalAnsi(exprPtr.Entity.Latexise())
            );

    }
}
