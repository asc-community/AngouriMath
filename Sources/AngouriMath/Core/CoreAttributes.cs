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

    /// <summary>
    /// Marks a rewrite rule <c>switch</c> whose arms are to be generated as individually
    /// addressable <see cref="Transformations.RewriteRule"/> values, in a field named after the
    /// method with <c>Arms</c> appended.
    /// </summary>
    /// <remarks>
    /// The method must be expression-bodied with a body of the form <c>parameter switch { ... }</c>;
    /// anything else is a build error rather than an empty list, since a rule set that silently
    /// has no rules reads exactly like one that has been checked.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    internal sealed class AddressableRulesAttribute : Attribute { }
}
