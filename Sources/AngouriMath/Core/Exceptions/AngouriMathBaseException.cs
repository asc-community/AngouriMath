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
