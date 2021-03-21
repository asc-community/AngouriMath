using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.CPP.Exporting
{
    internal static class ExposedObjects<T>
    {
        private static EntityRef lastId = new(0);
        private readonly static Dictionary<EntityRef, T> allocations = new();
        internal static EntityRef Alloc(T obj)
        {
            lastId = lastId.Next();
            allocations[lastId] = obj;
            return lastId;
        }
        internal static void Dealloc(EntityRef ptr)
        {
            if (!allocations.ContainsKey(ptr))
                throw new DeallocationException();
            allocations.Remove(ptr);
        }
        internal static T Get(EntityRef ptr)
        {
            if (!allocations.ContainsKey(ptr))
                throw new NonExistentObjectAddressingException();
            return allocations[ptr];
        }
    }
}
