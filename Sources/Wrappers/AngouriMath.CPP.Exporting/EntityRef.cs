using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.CPP.Exporting
{
    internal struct EntityRef
    {
        private readonly ulong handle;
        public EntityRef(ulong handle)
            => this.handle = handle;
        public EntityRef Next()
            => new EntityRef(handle + 1);
    }
}
