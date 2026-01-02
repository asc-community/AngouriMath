//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Convenience;
using System;

namespace AngouriMath
{
    partial class MathS
    {
        /// <summary>
        /// This class is used for diagnostic and debug of the library itself.
        /// Usually, you do not want to use it in production code.
        /// </summary>
        public static class Diagnostic
        {
            /// <summary>
            /// Explicit output for ToString, that is, no signs or parentheses will be omitted. Useful
            /// for debugging and diagnostic.
            /// </summary>
            public static Setting<bool> OutputExplicit => outputExplicit ??= false;
            [ThreadStatic] private static Setting<bool>? outputExplicit;

            /// <summary>
            /// Set a predicate on the state of <see cref="Entity"/> so that once
            /// the predicate turns true in method <see cref="Entity.Simplify"/>,
            /// an exception <see cref="DiagnosticCatchException"/> is thrown.
            /// </summary>
            public static Setting<Func<Entity, bool>> CatchOnSimplify => catchOnSimplify ??= (Func<Entity, bool>)(a => false);
            [ThreadStatic] private static Setting<Func<Entity, bool>>? catchOnSimplify;

            /// <summary>
            /// Will only occur in debug mode,
            /// is used if a case defined by Diagnostic settings turns true
            /// (e. g. if you got unexpected result in simplify, change catchOnSimplify to this result
            /// and see, at which point it becomes such)
            /// </summary>
            public sealed class DiagnosticCatchException : Exception
            {
                internal DiagnosticCatchException(string message) : base(message) { }
                internal DiagnosticCatchException() : base() { }
            }
        }
    }
}
