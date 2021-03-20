/* 
 * Copyright (c) 2019-2021 Angouri.
 * AngouriMath is licensed under MIT. 
 * Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
 * Website: https://am.angouri.org.
 */
using System.ComponentModel;
namespace System.Runtime.CompilerServices
{
    // C# 9 requires this class to be defined to act as modreq in records and init-only members.
    // This is defined in .NET 5 but not lower-level targets like .NET Standard 2.0.
    /// <summary>
    /// Reserved to be used by the compiler for tracking metadata.
    /// This class should not be used by developers in source code.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class IsExternalInit { }
}

namespace System.Runtime.InteropServices
{
    /// <summary>
    /// Any method marked with <see cref="System.Runtime.InteropServices.UnmanagedCallersOnlyAttribute" /> can be directly called from
    /// native code. The function token can be loaded to a local variable using the <see href="https://docs.microsoft.com/dotnet/csharp/language-reference/operators/pointer-related-operators#address-of-operator-">address-of</see> operator
    /// in C# and passed as a callback to a native method.
    /// </summary>
    /// <remarks>
    /// Methods marked with this attribute have the following restrictions:
    ///   * Method must be marked "static".
    ///   * Must not be called from managed code.
    ///   * Must only have <see href="https://docs.microsoft.com/dotnet/framework/interop/blittable-and-non-blittable-types">blittable</see> arguments.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class UnmanagedCallersOnlyAttribute : Attribute
    {
        public UnmanagedCallersOnlyAttribute()
        {
        }

        /// <summary>
        /// Optional. If omitted, the runtime will use the default platform calling convention.
        /// </summary>
        /// <remarks>
        /// Supplied types must be from the official "System.Runtime.CompilerServices" namespace and
        /// be of the form "CallConvXXX".
        /// </remarks>
        public Type[]? CallConvs;

        /// <summary>
        /// Optional. If omitted, no named export is emitted during compilation.
        /// </summary>
        public string? EntryPoint;
    }
}