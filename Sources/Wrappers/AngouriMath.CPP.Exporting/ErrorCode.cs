using AngouriMath.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace AngouriMath.CPP.Exporting
{
    /// <summary>
    /// Native structure to transfer exceptions
    /// </summary>
    public struct NErrorCode
    {
        private readonly IntPtr name;
        private readonly IntPtr stackTrace;
        private NErrorCode(Exception exception)
        {
            name = Marshal.StringToHGlobalAnsi(exception.GetType().FullName);
            stackTrace = Marshal.StringToHGlobalAnsi(exception.StackTrace);
        }
        public static NErrorCode Thrown(Exception exception)
            => new(exception);
        public static NErrorCode Ok
            => new();
    }
}
