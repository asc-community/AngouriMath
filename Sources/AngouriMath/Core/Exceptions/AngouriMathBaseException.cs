//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;

namespace AngouriMath.Core.Exceptions
{
    /// <summary>
    /// This is the base exception of all exceptions thrown by AngouriMath. 
    /// If one needs to catch all exceptions from AngouriMath, it is enough
    /// to catch this one
    /// </summary>
    public abstract class AngouriMathBaseException : Exception
    {
        internal AngouriMathBaseException() { }
        internal AngouriMathBaseException(string msg) : base(msg) { }
    }
}
