//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using HonkSharp.Laziness;
using AngouriMath.Extensions;
using System;
using AngouriMath;
using System.Threading;
using Xunit;
using System.Threading.Tasks;

namespace AngouriMath.Tests.Core.Multithreading
{
    public sealed class CancelTest
    {
        const int ShouldLastAtLeast = 10000;

        public static Action SomeLongLastingTask = () =>
                "a e sin(x ^ 14 + 3)3 + a b c d sin(x ^ 14 + 2)4 - k d sin(x ^ 14 + 3)2 + sin(x ^ 14 + 3) + e = 0"
                .Solve("x");

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

        [Theory, CombinatorialData]
        public void Test(
            [CombinatorialValues(0, 100, 200, 300, 500, 1000, 1500, 2000, 4000, 6000)]
            int timeBeforeCancel, 
            [CombinatorialValues(100, 200, 300, 400, 1000)]
            long cancellationTimeout)
        {
            Assert.True(MakesSenseToPerformTest);
            var source = new CancellationTokenSource();

            MathS.Multithreading.SetLocalCancellationToken(source.Token);

            var task = Task.Run(SomeLongLastingTask, source.Token);
            Thread.Sleep(timeBeforeCancel);
            Assert.False(task.IsCompleted);
            new TimeOutChecker().BeingCompletedForLessThan(
                () =>
                {
                    try
                    {
                        source.Cancel();
                        task.Wait();
                    }
                    catch (AggregateException aggregate)
                    {
                        if (aggregate.InnerException is not OperationCanceledException)
                            throw aggregate.InnerException ?? aggregate;
                    }
                }, cancellationTimeout
                );
        }
    }
}
