using AngouriMath.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
