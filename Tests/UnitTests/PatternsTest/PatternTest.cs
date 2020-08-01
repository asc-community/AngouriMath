using System;
using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Core.TreeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using AngouriMath.Extensions;

namespace UnitTests.PatternsTest
{
    [TestClass]
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
        [TestMethod]
        public void TestSimplifyHangs1()
            =>
                Assert.IsTrue(
                    BeingCompletedForLessThan(
                        () => "1 + 1 / x".Simplify(), GithubIssue170Timeout));

        [TestMethod]
        public void TestSimplifyHangs2()
            =>
                Assert.IsTrue(
                    BeingCompletedForLessThan(
                        () => "1 + 1 / (a + b)".Simplify(), GithubIssue170Timeout));

        [TestMethod]
        public void TestSimplifyHangs3()
            =>
                Assert.IsTrue(
                    BeingCompletedForLessThan(
                        () => "1 / x + 1".Simplify(), GithubIssue170Timeout));

        [TestMethod]
        public void TestSimplifyHangs4()
            =>
                Assert.IsTrue(
                    BeingCompletedForLessThan(
                        () => "(1 + 1 / x)^2 / (1 + 1 / x)".Simplify(), GithubIssue170Timeout));

                [TestMethod]
        public void TestEnumerator1()
        {
            Entity expr = "a^x + b^y + c^z";
            var pattern = Powf.PHang(Patterns.any1, Patterns.any2);

            var matches = new Set();
            foreach(var match in TreeAnalyzer.GetPatternEnumerator(expr, pattern))
            {
                matches.Add(match);
            }
            Assert.AreEqual(3, matches.Count);
            foreach(var match in matches.FiniteSet())
            {
                Assert.IsNotNull(expr.FindSubtree(match), "match is not in expression");
            }
        }

        [TestMethod]
        public void TestEnumerator2()
        {
            Entity expr = "a^x + ((b^y + c^z)^2)^x";
            var pattern = Powf.PHang(Patterns.any1, Patterns.any2);

            var matches = new Set();
            foreach (var match in TreeAnalyzer.GetPatternEnumerator(expr, pattern))
            {
                matches.Add(match);
            }
            Assert.AreEqual(5, matches.Count);
            foreach (var match in matches.FiniteSet())
            {
                Assert.IsNotNull(expr.FindSubtree(match), "match is not in expression");
            }
        }

        [TestMethod]
        public void TestEnumerator3()
        {
            Entity expr = "2 + 3 + x";
            var pattern = Powf.PHang(Patterns.any1, Patterns.any2);

            var matches = new Set();
            foreach (var match in TreeAnalyzer.GetPatternEnumerator(expr, pattern))
            {
                matches.Add(match);
            }
            Assert.AreEqual(0, matches.Count);
        }
    }
}
