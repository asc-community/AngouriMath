//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;

namespace AngouriMath.Core.Exceptions
{
    /// <summary>If one was thrown, the exception is probably not foreseen by AM. Report it is an issue</summary>
    public sealed class AngouriBugException : AngouriMathBaseException
    { 
        internal AngouriBugException(string msg) : base(msg + "\n please report about it to the official repository (https://github.com/asc-community/AngouriMath, https://am.angouri.org)") { } 
    }

    /// <summary>
    /// In case if AM or other parts do not support something, 
    /// for example, it may occur if either AM or SymPy does not
    /// support some specific feature
    /// </summary>
    public sealed class NotSufficientlySupportedException : AngouriMathBaseException
    {
        internal NotSufficientlySupportedException(string msg) : base(msg) { }
    }
}