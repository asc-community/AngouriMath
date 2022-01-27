//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using Xunit;
using AngouriMath;
using AngouriMath.Extensions;
using HonkSharp.Laziness;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace AngouriMath.Tests.Core.Multithreading
{
    public sealed class MultithreadingCancel
    {
        private static void SomeLongLastingTask()
        {
            Entity t;
            for (int i = 0; i < 10; i++)
                t = 
            "b c d e f g a e cos(x ^ 4 + 3)1 + a f c d cos(x ^ 4 + 2)2 - k d cos(x ^ 4 + 3)2 + sin(x ^ 4 + 3) + e = 0"
            .Solve("x");
        }

        const int ShouldLastAtLeast = 3000;

        /// <summary>
        /// We are going to cancel a long-lasting task. If for some reason, it is not that long lasting,
        /// this will become false
        /// </summary>
        public static bool MakesSenseToPerformTest => makesSenseToPerformTest.GetValue(
            static @this => !new TimeOutChecker().BeingCompletedForLessThan(
                SomeLongLastingTask
                , ShouldLastAtLeast), someReference);
        private static object someReference = new();
        private static LazyPropertyA<bool> makesSenseToPerformTest;

        [Theory]
        [InlineData(false, true)]
        [InlineData(false, false)]
        [InlineData(false, true, true)]
        [InlineData(false, true, false)]
        [InlineData(false, false, false)]
        [InlineData(false, false, true)]
        [InlineData(false, true, true, true)]
        [InlineData(false, true, true, false)]
        [InlineData(false, false, false, false, false)]
        [InlineData(false, false, false, false, true)]
        [InlineData(false, false, true, false, false)]
        [InlineData(false, false, true, true, true)]
        [InlineData(false, true, true, true, true)]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(true, true, true)]
        [InlineData(true, true, false)]
        [InlineData(true, false, false)]
        [InlineData(true, false, true)]
        [InlineData(true, true, true, true)]
        [InlineData(true, true, true, false)]
        [InlineData(true, false, false, false, false)]
        [InlineData(true, false, false, false, true)]
        [InlineData(true, false, true, false, false)]
        [InlineData(true, false, true, true, true)]
        [InlineData(true, true, true, true, true)]
        public async void TestThoseOnlyCancel(bool generateChild, params bool[] threadToCancel)
        {
            Assert.True(MakesSenseToPerformTest, $"The given task completed too soon, consider lowering the constant {ShouldLastAtLeast}");

            (CancellationTokenSource, Task) GenTask(int c)
            {
                var cts = new CancellationTokenSource();
                var token = cts.Token;
                MathS.Multithreading.SetLocalCancellationToken(token);
                var task = Task.Run(async
                    () => {
                        if (generateChild)
                            await Task.Run(SomeLongLastingTask);
                        SomeLongLastingTask();
                    }, token);
                var res = (cts, task);
                return res;
            }

            var tasksAndCts = Enumerable.Range(0, threadToCancel.Length).Select(GenTask).ToArray();
            var tasks = tasksAndCts.Select(c => c.Item2).ToArray();
            var ctss = tasksAndCts.Select(c => c.Item1).ToArray();
            try
            {
                for (int i = 0; i < threadToCancel.Length; i++)
                    if (threadToCancel[i])
                        ctss[i].CancelAfter(ShouldLastAtLeast / 4);
                var waitAndSwallow = new WaiterAndSwallower(5);
                await waitAndSwallow.Wait(ShouldLastAtLeast / 4);
                await waitAndSwallow.WaitWhile(() => tasks.Any(c => c.Status == TaskStatus.WaitingToRun)); // wait while at least one task is waiting for execution start
                await waitAndSwallow.Wait(ShouldLastAtLeast / 4); // wait some more time
            }
            catch (OperationCanceledException) { } // we are going to check their states in finally
            finally
            {
                for (int i = 0; i < threadToCancel.Length; i++)
                    Assert.True(threadToCancel[i] == tasks[i].IsCanceled, $"Task number {i}: {threadToCancel[i]}, but instead {tasks[i].IsCanceled}. Status: {tasks[i].Status}");
                foreach (var cts in ctss)
                    cts.Cancel();
                try
                {
                    await Task.WhenAll(tasks);
                }
                catch (OperationCanceledException) { }
            }
        }
    }
}
