using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTests
{
    /// <summary>
    /// When waits, ignores all the operation canceled exceptions
    /// </summary>
    public sealed class WaiterAndSwallower
    {
        private readonly int precision;
        public WaiterAndSwallower(int precisionMs)
        {
            this.precision = precisionMs;
        }

        public async Task Wait(int timeMs)
        {
            var precLocal = precision;
            while (timeMs > 0)
            {
                try
                {
                    await Task.Delay(precLocal);
                    timeMs -= precLocal;
                }
                catch (OperationCanceledException) { }
            }
        }

        public async Task WaitWhile(Func<bool> predicate)
        {
            while (predicate())
                try
                {
                    await Task.Delay(10);
                }
                catch (OperationCanceledException) { }
        }
    }

    public sealed class TimeOutChecker
    {
        public bool BeingCompletedForLessThan(Action action, long timeoutMs)
        {
            var stopped = false;
            var th = new Thread(() =>
            {
                action();
                stopped = true;
            });
            th.Start();
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (!stopped && stopwatch.ElapsedMilliseconds < timeoutMs)
                Thread.Sleep(5);
            return stopped;
        }
    }

    public sealed class ThreadingChecker
    {
        private readonly Action<int> action;
        public ThreadingChecker(Action<int> action)
        {
            this.action = action;
        }

        public void Run(int iterCount = 1, int threadCount = 4)
        {
            var tasks = new Task[threadCount];
            for (var i = 0; i < threadCount; i++)
                tasks[i] = Task.Run(
                    () =>
                    {
                        var iterCountLocal = iterCount;
                        for (var j = 0; j < iterCountLocal; j++)
                            action(i);
                    }
                );
            Task.WaitAll(tasks);
        }
    }
}
