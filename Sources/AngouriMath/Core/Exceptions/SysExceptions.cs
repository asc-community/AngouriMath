/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
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