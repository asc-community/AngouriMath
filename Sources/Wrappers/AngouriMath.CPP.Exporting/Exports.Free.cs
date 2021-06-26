using System;
using System.Runtime.InteropServices;

namespace AngouriMath.CPP.Exporting
{
    partial class Exports
    {
        private static void Free(IntPtr ptr)
        {
            if (ptr.ToInt64() == 0)
                return;
            Marshal.FreeHGlobal(ptr);
        }

        [UnmanagedCallersOnly(EntryPoint = "free_entity")]
        public static NErrorCode FreeEntity(ObjRef handle)
            => ExceptionEncode(handle, static h => ObjStorage<Entity>.Dealloc(h));

        [UnmanagedCallersOnly(EntryPoint = "free_error_code")]
        public static NErrorCode FreeErrorCode(NErrorCode code)
            => ExceptionEncode(code, static code => code.Free() );

        [UnmanagedCallersOnly(EntryPoint = "free_native_array")]
        public static NErrorCode FreeNativeArray(NativeArray arr)
            => ExceptionEncode(arr, static arr => arr.Free() );

        [UnmanagedCallersOnly(EntryPoint = "free_string")]
        public static NErrorCode FreeNativeArray(IntPtr s)
            => ExceptionEncode(s, static s => Free(s) );
    }
}
