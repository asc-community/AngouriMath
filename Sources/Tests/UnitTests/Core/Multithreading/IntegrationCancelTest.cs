//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Threading;
using System.Threading.Tasks;
using AngouriMath.Extensions;
using AngouriMath.Functions;
using Xunit;

namespace AngouriMath.Tests.Core.Multithreading
{
    /// <summary>
    /// An integration stops when the budget it runs under cancels it, including inside the
    /// numeric checks of the perfect-square collapse, which evaluate exactly at sample points.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// Rubi 1.1.2.4 #565 spends three and a half minutes in one <c>Simplify</c> of a
    /// substitution, nearly all of it evaluating sums with coefficients like <c>b^335</c> at
    /// those points, and ran on for a minute after its budget was cancelled, since no
    /// evaluation asked.
    /// </remarks>
    [Trait("Area", "Core")]
    public sealed class IntegrationCancelTest
    {
        private static readonly TimeSpan Budget = TimeSpan.FromSeconds(8);

        /// <summary>
        /// The mechanism, with no clock in it: every cross term of this sum is told apart from
        /// its candidate at the sample points, so the collapse reaches no <c>Simplify</c> that
        /// would have stopped it, and answers null unless an evaluation asks first.
        /// </summary>
        [Fact]
        public void ACancelledPerfectSquareTestStopsBeforeItEvaluates()
        {
            using var source = new CancellationTokenSource();
            source.Cancel();
            var task = OnItsOwnThread(source.Token, () => Patterns.CollapseToPerfectSquare("x + 2 * sqrt(x * y) + y".ToEntity()));
            var thrown = Assert.Throws<AggregateException>(() => task.Wait());
            Assert.IsAssignableFrom<OperationCanceledException>(thrown.InnerException);
        }

        /// <summary>
        /// The integrand, under a budget that lands inside those checks. A machine that answers
        /// it within the budget has nothing to show here.
        /// </summary>
        [Fact]
        public void ACancelledIntegrationStopsWithinThreeTimesItsBudget()
        {
            using var source = new CancellationTokenSource();
            var task = OnItsOwnThread(source.Token, () => "x^(7/2)/((a+b*x^2)^2*(c+d*x^2)^3)".ToEntity().Integrate("x"));
            if (task.Wait(Budget))
                return;
            source.Cancel();
            Assert.True(StopsWithin(task, 2 * Budget),
                $"still running {(2 * Budget).TotalSeconds} s after its {Budget.TotalSeconds} s budget was cancelled");
        }

        // On a thread of its own, since the token is local to the thread that sets it, and a
        // dedicated one, so that work on the pool cannot hold the wait up.
        private static Task<Entity?> OnItsOwnThread(CancellationToken token, Func<Entity?> work)
            => Task.Factory.StartNew(() =>
            {
                MathS.Multithreading.SetLocalCancellationToken(token);
                return work();
            }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);

        private static bool StopsWithin(Task task, TimeSpan patience)
        {
            try
            {
                return task.Wait(patience);
            }
            catch (AggregateException stopped) when (stopped.InnerException is OperationCanceledException)
            {
                return true;
            }
        }
    }
}
