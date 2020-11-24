using FieldCacheNamespace;
using AngouriMath.Extensions;
using System;
using AngouriMath;
using System.Threading;
using Xunit;

namespace UnitTests.Core
{
    public sealed class CancelTest
    {
        const int ShouldLastAtLeast = 10000;

        public Action SomeLongLastingTask = () =>
                "a e sin(x ^ 14 + 3)3 + a b c d sin(x ^ 14 + 2)4 - k d sin(x ^ 14 + 3)2 + sin(x ^ 14 + 3) + e = 0"
                .Solve("x");

        /// <summary>
        /// We are going to cancel a long-lasting task. If for some reason, it is not that long lasting,
        /// this will become false
        /// </summary>
        public bool MakesSenseToPerformTest => makesSenseToPerformTest.GetValue(
            @this => !new TimeOutChecker().BeingCompletedForLessThan(
                SomeLongLastingTask
                , ShouldLastAtLeast), this);
        private FieldCache<bool> makesSenseToPerformTest;

        [Theory, CombinatorialData]
        public void Test(
            [CombinatorialValues(0, 100, 200, 300, 500, 1000, 1500, 2000, 4000, 6000)]
            int timeBeforeCancel, 
            [CombinatorialValues(100, 200, 300, 400, 1000)]
            long cancellationTimeout)
        {
            Assert.True(MakesSenseToPerformTest);
            var task = MathS.Multithreading.RunAsync(SomeLongLastingTask);
            Thread.Sleep(timeBeforeCancel);
            Assert.False(task.Task.IsCompleted);
            new TimeOutChecker().BeingCompletedForLessThan(
                () => task.Cancel(), cancellationTimeout
                );
        }
    }
}
