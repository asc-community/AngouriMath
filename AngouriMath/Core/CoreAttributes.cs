using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Core
{
    /// <summary>
    /// Use this attribute on those static fields that do not require thread static attribute
    /// because they are constant
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    internal sealed class ConstantFieldAttribute : Attribute
    {

    }
}
