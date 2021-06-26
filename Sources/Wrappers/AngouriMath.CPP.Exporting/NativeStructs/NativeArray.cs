using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace AngouriMath.CPP.Exporting
{
    partial class Exports
    {
        public struct NativeArray : IFreeable
        {
            public int Length { get; init; }
            public IntPtr Ptr { get; init; }
            internal static NativeArray Alloc<T>(IEnumerable<T> elements)
            {
                var arr = elements.Select(c => ObjStorage<T>.Alloc(c)).ToArray();
                var allocated = GCHandle.Alloc(arr, GCHandleType.Pinned);
                var ptr = allocated.AddrOfPinnedObject();
                ObjStorage<GCHandle>.Alloc(new((ulong)ptr), allocated);
                return new() { Length = arr.Length, Ptr = ptr };
            }
            public void Free()
            {
                var handle = ObjStorage<GCHandle>.Get(new((ulong)Ptr));
                handle.Free();
                ObjStorage<GCHandle>.Dealloc(new((ulong)Ptr));
            }
        }
    }
}
