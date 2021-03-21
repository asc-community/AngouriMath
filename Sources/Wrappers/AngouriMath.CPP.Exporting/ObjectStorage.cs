using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.CPP.Exporting
{
    using EntityRef = System.UInt64;

    internal static class ExposedObjects<T>
    {
        private static EntityRef lastId = 0;
        private readonly static Dictionary<EntityRef, T> allocations = new();
        internal static EntityRef Alloc(T obj)
        {
            lastId++;
            allocations[lastId] = obj;
            return lastId;
        }
        internal static void Dealloc(EntityRef ptr)
            => allocations.Remove(ptr);
        internal static T Get(EntityRef ptr)
            => allocations[ptr];
    }
}
