using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace AngouriMath.CPP.Exporting
{
    partial class Exports
    {
        [UnmanagedCallersOnly(EntryPoint = "free_entity")]
        public static NErrorCode Free(EntityRef handle)
            => ExceptionEncode(handle, static h => ObjStorage<Entity>.Dealloc(h));
    }
}
