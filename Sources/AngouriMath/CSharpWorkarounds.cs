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
