using System;
using Xunit;
using System.Diagnostics;
using System.Threading;
using AngouriMath.Extensions;

namespace UnitTests.PatternsTest
{
    public class PatternTest
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

        private const int GithubIssue170Timeout = 1600; // we don't know the target machines

        // GitHub issue #170
        [Fact]
        public void TestSimplifyHangs1() =>
            Assert.True(
                BeingCompletedForLessThan(
                    () => "1 + 1 / x".Simplify(), GithubIssue170Timeout));

        [Fact]
        public void TestSimplifyHangs2() =>
            Assert.True(
                BeingCompletedForLessThan(
                    () => "1 + 1 / (a + b)".Simplify(), GithubIssue170Timeout));

        [Fact]
        public void TestSimplifyHangs3() =>
            Assert.True(
                BeingCompletedForLessThan(
                    () => "1 / x + 1".Simplify(), GithubIssue170Timeout));

        [Fact]
        public void TestSimplifyHangs4() =>
            Assert.True(
                BeingCompletedForLessThan(
                    () => "(1 + 1 / x)^2 / (1 + 1 / x)".Simplify(), GithubIssue170Timeout));
    }
}
