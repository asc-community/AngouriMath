using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Core
{
    struct XorHashBuilder
    {
        private int hash;

        public XorHashBuilder Combine<T>(T obj)
        {
            if (obj == null)
            {
                return this;
            }

            hash ^= obj.GetHashCode();
            return this;
        }

        public override int GetHashCode()
        {
            return hash;
        }

    }
}
