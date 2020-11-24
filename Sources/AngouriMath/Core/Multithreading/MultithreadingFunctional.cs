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
        [ThreadStatic] private static CancellationToken globalCancellationToken;
        [ConstantField] private static CancellationToken alwaysNotRequested = new CancellationTokenSource().Token;

        internal static Task RunAsync(Action action, CancellationToken token)
        {
            return Task.Run(
                () =>
                {
                    globalCancellationToken = token;
                    try
                    {
                        action();
                    }
                    finally
                    {
                        globalCancellationToken = alwaysNotRequested;
                    }
                },
                token);
        }

        internal static Task<T> RunAsync<T>(Func<T> action, CancellationToken token)
        {
            return Task.Run(
                () =>
                {
                    globalCancellationToken = token;
                    try
                    {
                        return action();
                    }
                    finally
                    {
                        globalCancellationToken = alwaysNotRequested;
                    }
                },
                token);
        }

        // Inject this code in places where the function might potentially get stuck
        internal static void ExitIfCancelled()
        {
            globalCancellationToken.ThrowIfCancellationRequested();
        }
    }
}
