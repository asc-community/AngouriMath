//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Threading;

namespace AngouriMath.Core.Multithreading
{
    internal static class MultithreadingFunctional
    {
#pragma warning disable ThreadSafety // Because it is AsyncLocal
        private static readonly AsyncLocal<CancellationToken?> globalCancellationToken = new();
#pragma warning restore ThreadSafety // AMAnalyzer

        internal static void SetLocalCancellationToken(CancellationToken? token)
            => globalCancellationToken.Value = token;

        // Inject this code in places where the function might potentially get stuck
        internal static void ExitIfCancelled()
        {
            var token = globalCancellationToken.Value;
            if (token is { } tok)
                tok.ThrowIfCancellationRequested();
        }
    }
}
