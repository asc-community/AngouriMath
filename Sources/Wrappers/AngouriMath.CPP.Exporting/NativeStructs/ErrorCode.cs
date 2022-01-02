//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Runtime.InteropServices;

namespace AngouriMath.CPP.Exporting
{
    partial class Exports
    {
        /// <summary>
        /// Native structure to transfer exceptions
        /// </summary>
        public struct NErrorCode : IFreeable
        {
            private readonly IntPtr name;
            private readonly IntPtr message;
            private readonly IntPtr stackTrace;
            private NErrorCode(Exception exception)
            {
                name = Marshal.StringToHGlobalAnsi(exception.GetType().FullName);
                message = Marshal.StringToHGlobalAnsi(exception.Message);
                stackTrace = Marshal.StringToHGlobalAnsi(exception.StackTrace);
            }
            public static NErrorCode Thrown(Exception exception)
                => new(exception);
            public static NErrorCode Ok
                => new();
            public void Free()
            {
                Exports.Free(name);
                Exports.Free(message);
                Exports.Free(stackTrace);
            }
        }
    }
}
