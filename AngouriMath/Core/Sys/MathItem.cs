using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Core.Sys
{
    public abstract class MathItem
    {
        public enum Type
        {
            SCALAR,
            VECTOR,
            MATRIX,
            TENSOR
        }
    }
}
