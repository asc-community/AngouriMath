using AngouriMath.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;
using static AngouriMath.Entity;

namespace AngouriMath.Core.Compilation.IntoLinq
{
    partial record CompilationProtocol
    {
        internal static CompilationProtocol Assume()
            => Create<Complex>();

        internal static CompilationProtocol Assume<T1>()
            => numericTypes.Contains(typeof(T1)) ? Create<T1>() : Assume();

        
        internal static CompilationProtocol Assume<T1, T2>()
            => numericTypes.Contains(typeof(T1)) ? Create<T1>() : Assume<T2>();

        
        internal static CompilationProtocol Assume<T1, T2, T3>()
            => numericTypes.Contains(typeof(T1)) ? Create<T1>() : Assume<T2, T3>();

        
        internal static CompilationProtocol Assume<T1, T2, T3, T4>()
            => numericTypes.Contains(typeof(T1)) ? Create<T1>() : Assume<T2, T3, T4>();

        
        internal static CompilationProtocol Assume<T1, T2, T3, T4, T5>()
            => numericTypes.Contains(typeof(T1)) ? Create<T1>() : Assume<T2, T3, T4, T5>();

        
        internal static CompilationProtocol Assume<T1, T2, T3, T4, T5, T6>()
            => numericTypes.Contains(typeof(T1)) ? Create<T1>() : Assume<T2, T3, T4, T5, T6>();

        
        internal static CompilationProtocol Assume<T1, T2, T3, T4, T5, T6, T7>()
            => numericTypes.Contains(typeof(T1)) ? Create<T1>() : Assume<T2, T3, T4, T5, T6, T7>();

        
        internal static CompilationProtocol Assume<T1, T2, T3, T4, T5, T6, T7, T8>()
            => numericTypes.Contains(typeof(T1)) ? Create<T1>() : Assume<T2, T3, T4, T5, T6, T7, T8>();

        
        internal static CompilationProtocol Assume<T1, T2, T3, T4, T5, T6, T7, T8, T9>()
            => numericTypes.Contains(typeof(T1)) ? Create<T1>() : Assume<T2, T3, T4, T5, T6, T7, T8, T9>();

        
    }
}