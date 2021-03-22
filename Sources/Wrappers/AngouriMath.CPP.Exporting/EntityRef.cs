using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.CPP.Exporting
{
    partial class Exports
    {
        internal struct EntityRef
        {
            private readonly ulong handle;
            public EntityRef(ulong handle)
                => this.handle = handle;
            public EntityRef Next()
                => new EntityRef(handle + 1);
            public Entity Entity => ObjStorage<Entity>.Get(this);
            public static implicit operator EntityRef(Entity entity)
                => ObjStorage<Entity>.Alloc(entity);
        }
    }
}
