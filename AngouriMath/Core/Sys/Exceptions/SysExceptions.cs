using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Core.Exceptions
{
    public class SysException : Exception
    {
        public SysException(string msg) : base(msg) { }
    }
}
