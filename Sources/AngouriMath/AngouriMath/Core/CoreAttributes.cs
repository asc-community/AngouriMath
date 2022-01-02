//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;

namespace AngouriMath.Core
{
    /// <summary>
    /// Use this attribute on those static fields that do not require thread static attribute
    /// because they are constant
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    internal sealed class ConstantFieldAttribute : Attribute { }
    
    /// <summary>
    /// Use this attribute on those static fields that are already synchronized
    /// internally or explicitly
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    internal sealed class ConcurrentFieldAttribute : Attribute { }
}
