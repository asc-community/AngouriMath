using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Core
{
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class NativeExportAttribute : Attribute
    {
        public NativeExportAttribute() { }
    }
}
