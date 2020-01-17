using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Core.Exceptions
{
    internal class SysException : Exception
    {
        internal SysException(string msg) : base(msg) { }
    }
}
