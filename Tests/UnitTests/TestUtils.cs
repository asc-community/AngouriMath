using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests
{
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
