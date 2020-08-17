using System;
using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Core.TreeAnalysis;
using Xunit;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using AngouriMath.Extensions;
using System.Linq;

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

                [Fact]
        public void TestEnumerator1()
        {
            Entity expr = "a^x + b^y + c^z";

            var matches = new Set();
            foreach(var match in expr.Where(x => x is Entity.Powf))
                matches.Add(match);
            Assert.Equal(3, matches.Count);
            foreach(var match in matches.FiniteSet())
                Assert.True(expr.Contains(match));
        }

        [Fact]
        public void TestEnumerator2()
        {
            Entity expr = "a^x + ((b^y + c^z)^2)^x";

            var matches = new Set();
            foreach (var match in expr.Where(x => x is Entity.Powf))
                matches.Add(match);
            Assert.Equal(5, matches.Count);
            foreach (var match in matches.FiniteSet())
                Assert.True(expr.Contains(match));
        }

        [Fact]
        public void TestEnumerator3()
        {
            Entity expr = "2 + 3 + x";

            var matches = new Set();
            foreach (var match in expr.Where(x => x is Entity.Powf))
                matches.Add(match);
            Assert.Empty(matches);
        }
    }
}
