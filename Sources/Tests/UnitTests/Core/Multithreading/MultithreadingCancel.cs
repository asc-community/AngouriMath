using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using AngouriMath;
using AngouriMath.Extensions;
using FieldCacheNamespace;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace UnitTests.Core.Multithreading
{
    public sealed class MultithreadingCancel
    {
        private static void SomeLongLastingTask() =>
                "b c d e f g a e cos(x ^ 4 + 3)1 + a f c d cos(x ^ 4 + 2)2 - k d cos(x ^ 4 + 3)2 + sin(x ^ 4 + 3) + e = 0"
                .Solve("x");

        const int ShouldLastAtLeast = 1500;

        /// <summary>
        /// We are going to cancel a long-lasting task. If for some reason, it is not that long lasting,
        /// this will become false
        /// </summary>
        public static bool MakesSenseToPerformTest => makesSenseToPerformTest.GetValue(
            @this => !new TimeOutChecker().BeingCompletedForLessThan(
                SomeLongLastingTask
                , ShouldLastAtLeast), someReference);
        private static object someReference = new();
        private static FieldCache<bool> makesSenseToPerformTest;

        private static object mutex = new();

        [Theory]
        [InlineData(false, false, false, false)]
        [InlineData(false, false, false, true)]
        [InlineData(false, true, false, false)]
        [InlineData(false, true, true, true)]
        [InlineData(true, true, true, true)]
        [InlineData(true)]
        public async void TestThoseOnlyCancel(params bool[] threadToCancel)
        {
            Assert.True(MakesSenseToPerformTest);

            var tasksAndCts = Enumerable.Range(0, threadToCancel.Length).Select(
                c => 
                { 
                    var cts = new CancellationTokenSource(); 
                    var token = cts.Token;
                    var task = Task.Run(
                        () => { 
                            MathS.Multithreading.SetLocalCancellationToken(token); 
                            SomeLongLastingTask(); 
                            }, token);
                    var res = (cts, task);
                    //Thread.Sleep(100);
                    //res.Item1.Cancel();
                    return res; 
                });
            tasksAndCts.First().Item1.Cancel();
            var tasks = tasksAndCts.Select(c => c.Item2).ToArray();
            var ctss = tasksAndCts.Select(c => c.Item1).ToArray();
            try
            {
                for (int i = 0; i < threadToCancel.Length; i++)
                    if (threadToCancel[i])
                        //ctss[i].CancelAfter(500);
                        ctss[i].Cancel();
                await Task.WhenAll(tasks);
            }
            catch (AggregateException) { } // we are going to check their states in finally
            finally
            {
                for (int i = 0; i < threadToCancel.Length; i++)
                    Assert.Equal(threadToCancel[i], tasks[i].IsCanceled);
            }
        }
    }
}
