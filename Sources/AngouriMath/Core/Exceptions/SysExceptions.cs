//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Reflection;

namespace AngouriMath.Core.Exceptions
{
    /// <summary>If one was thrown, the exception is probably not foreseen by AM. Report it is an issue</summary>
    public sealed class AngouriBugException : AngouriMathBaseException
    { 
        internal AngouriBugException(string msg) : base(msg + "\n please report about it to the official repository (https://github.com/asc-community/AngouriMath, https://am.angouri.org)") { } 
    }

    /// <summary>
    /// Is thrown when the requested feature is still under developing
    /// or not considered to be developed at all
    /// </summary>
    public sealed class FutureReleaseException : AngouriMathBaseException
    {
        private FutureReleaseException(string msg) : base(msg) {}

        internal static Exception Raised(string feature, string? plannedVersion = null)
        {
            var currVersion = Assembly.GetExecutingAssembly().GetName().Version;
            if (plannedVersion is { } version && currVersion > new Version(version))
                return new AngouriBugException($"{feature} was planned for {version} but hasn't been released by {version}");
            else if (plannedVersion is null)
                return new FutureReleaseException($"It is unclear when {feature} will be added/fixed");
            else
                return new FutureReleaseException($"Feature {feature} will be completed by {plannedVersion}. You are on {currVersion}");
        }
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