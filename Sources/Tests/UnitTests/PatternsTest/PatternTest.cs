using Xunit;
using AngouriMath.Extensions;

namespace UnitTests.PatternsTest
{
    public sealed class PatternTest
    {
        private const int GithubIssue170Timeout = 1600; // we don't know the target machines

        // GitHub issue #170
        [Fact]
        public void TestSimplifyHangs1() =>
            Assert.True(new TimeOutChecker().
                BeingCompletedForLessThan(
                    () => "1 + 1 / x".Simplify(), GithubIssue170Timeout));

        [Fact]
        public void TestSimplifyHangs2() =>
            Assert.True(new TimeOutChecker().
                BeingCompletedForLessThan(
                    () => "1 + 1 / (a + b)".Simplify(), GithubIssue170Timeout));

        [Fact]
        public void TestSimplifyHangs3() =>
            Assert.True(new TimeOutChecker().
                BeingCompletedForLessThan(
                    () => "1 / x + 1".Simplify(), GithubIssue170Timeout));

        [Fact]
        public void TestSimplifyHangs4() =>
            Assert.True(new TimeOutChecker().
                BeingCompletedForLessThan(
                    () => "(1 + 1 / x)^2 / (1 + 1 / x)".Simplify(), GithubIssue170Timeout));
    }
}
