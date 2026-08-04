//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Numerics;
using Xunit;

namespace AngouriMath.Tests.Common
{
    /// <summary>
    /// A compiled expression kept its working stack and its cache as instance fields, so two
    /// threads calling one of them interleaved their pushes and pops. See
    /// https://github.com/asc-community/AngouriMath/issues/637, which reports it as a
    /// compilation problem -- compiling in parallel is fine, it is calling that was not.
    /// </summary>
    public sealed class ParallelCompiledCallTest
    {
        private const int Threads = 16;

        private static void InParallel(int count, Action<int> body)
        {
            var trouble = new ConcurrentBag<string>();
            Parallel.For(0, count, new ParallelOptions { MaxDegreeOfParallelism = Threads }, i =>
            {
                try { body(i); }
                catch (Exception e) { trouble.Add($"{e.GetType().Name}: {e.Message}"); }
            });
            Assert.True(trouble.IsEmpty, string.Join("\n", trouble));
        }

        /// <summary>
        /// Threw <c>AngouriBugException("Unused values remain in the stack")</c> on all but a
        /// handful of 400000 calls, and answered a wrong number rather than throwing on a few
        /// of those -- the reported failure is the kinder half of it.
        /// </summary>
        [Fact]
        public void OneCompiledExpressionCalledFromManyThreads()
        {
            var f = "x ^ 2 + 3 * x + 1".ToEntity().Compile("x");
            InParallel(200_000, i =>
            {
                double v = (i % 100) + 1;
                var got = f.Call(new Complex(v, 0));
                Assert.Equal(v * v + 3 * v + 1, got.Real, 9);
                Assert.Equal(0, got.Imaginary, 9);
            });
        }

        /// <summary>
        /// The same for an expression that repeats a subexpression, which is what makes the
        /// compiler emit the cache instructions. The cache was per expression as well.
        /// </summary>
        [Fact]
        public void AnExpressionWithARepeatedPartCalledFromManyThreads()
        {
            var f = "sin(x ^ 2 + 1) * cos(x ^ 2 + 1) + (x ^ 2 + 1) ^ 2 + sin(x ^ 2 + 1)".ToEntity().Compile("x");
            InParallel(100_000, i =>
            {
                double v = (i % 50) + 1, u = v * v + 1;
                Assert.Equal(Math.Sin(u) * Math.Cos(u) + u * u + Math.Sin(u), f.Call(new Complex(v, 0)).Real, 9);
            });
        }

        /// <summary>
        /// The stack was never emptied on the way in, and the count check throws before
        /// anything is popped, so one racing call left its leftovers behind for good: every
        /// later call failed too, on one thread or on many.
        /// </summary>
        [Fact]
        public void ARacingCallDoesNotPoisonTheExpressionForLater()
        {
            var f = "x ^ 2 + 1".ToEntity().Compile("x");
            Parallel.For(0, 20_000, new ParallelOptions { MaxDegreeOfParallelism = Threads },
                i => { try { f.Call(new Complex(2, 0)); } catch { /* the point is what comes after */ } });
            Assert.Equal(10, f.Call(new Complex(3, 0)).Real, 9);
        }

        /// <summary>
        /// Compiling in parallel, which is what the issue title says, was never the broken
        /// part. Kept so that it stays unbroken.
        /// </summary>
        [Fact]
        public void ManyExpressionsCompiledInParallel() =>
            InParallel(2_000, i =>
            {
                var f = $"x ^ 2 + {i % 7} * x + 1".ToEntity().Compile("x");
                Assert.Equal(4 + (i % 7) * 2 + 1, f.Call(new Complex(2, 0)).Real, 9);
            });
    }
}
