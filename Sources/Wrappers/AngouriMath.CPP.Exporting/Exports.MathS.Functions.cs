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

        [UnmanagedCallersOnly(EntryPoint = "maths_sin")]
        public static NErrorCode Sin(EntityRef expr, ref EntityRef res)
            => ExceptionEncode(ref res, expr, expr => MathS.Sin(expr.Entity));
    }
}
