using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

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
                return new() { Length = arr.Length, Ptr = allocated.AddrOfPinnedObject() };
            }
            public void Free()
                => Exports.Free(Ptr);
        }
    }
}
