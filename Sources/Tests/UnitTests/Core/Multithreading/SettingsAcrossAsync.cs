//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Threading;
using System.Threading.Tasks;
using AngouriMath;
using Xunit;

namespace AngouriMath.Tests.Core.Multithreading
{
    /// <summary>
    /// A setting scope belongs to the flow that opened it, not to the thread that happened
    /// to be running it. These are the four properties that distinguishes: it survives an
    /// await, it is not visible to a sibling, it does not escape upwards, and it unwinds in
    /// whatever order the scopes are disposed.
    /// </summary>
    [Trait("Area", "Core")]
    public sealed class SettingsAcrossAsync
    {
        static long Current => MathS.Settings.MaxExpansionTermCount.Value;
        static long Default => MathS.Settings.MaxExpansionTermCount.Default;

        /// <summary>
        /// The regression. Backed by <c>[ThreadStatic]</c> the continuation resumed on a pool
        /// thread that had never seen the scope, and the setting silently read its default.
        /// </summary>
        [Fact]
        public async Task ScopeSurvivesAnAwait()
        {
            Assert.NotEqual(27L, Default);
            using var _ = MathS.Settings.MaxExpansionTermCount.Set(27);
            Assert.Equal(27, Current);
            await Task.Yield();
            Assert.Equal(27, Current);
            await Task.Delay(20).ConfigureAwait(false);
            Assert.Equal(27, Current);
        }

        /// <summary>
        /// What the per-thread field did give, and what a naive <c>AsyncLocal&lt;Setting&lt;T&gt;&gt;</c>
        /// would have taken away: two flows sharing one setting object would have pushed onto
        /// one stack. The barrier makes both scopes open before either reads.
        /// </summary>
        [Fact]
        public async Task SiblingFlowsDoNotSeeEachOther()
        {
            using var barrier = new Barrier(2);
            async Task<long> Branch(long value)
            {
                await Task.Yield();
                using var _ = MathS.Settings.MaxExpansionTermCount.Set(value);
                barrier.SignalAndWait();
                await Task.Delay(20).ConfigureAwait(false);
                return Current;
            }
            var both = await Task.WhenAll(Branch(101), Branch(202));
            Assert.Equal(new[] { 101L, 202L }, both);
        }

        /// <summary>
        /// The direction of the change that can surprise someone: work started under a scope
        /// runs under it. With the per-thread field the pool thread had never seen the scope
        /// and the child read the default instead.
        /// </summary>
        [Fact]
        public async Task WorkStartedUnderAScopeInheritsIt()
        {
            using var _ = MathS.Settings.MaxExpansionTermCount.Set(77);
            Assert.Equal(77, await Task.Run(() => Current));
            Assert.Equal(77, await Task.Run(async () => { await Task.Yield(); return Current; }));
        }

        /// <summary>A scope opened inside a task is gone once that task is.</summary>
        [Fact]
        public async Task ScopeDoesNotEscapeUpwards()
        {
            await Task.Run(async () =>
            {
                using var _ = MathS.Settings.MaxExpansionTermCount.Set(55);
                await Task.Yield();
                Assert.Equal(55, Current);
            });
            Assert.Equal(Default, Current);
        }

        [Fact]
        public void NestedScopesUnwindInOrder()
        {
            using (var _ = MathS.Settings.MaxExpansionTermCount.Set(1))
            {
                Assert.Equal(1, Current);
                using (var __ = MathS.Settings.MaxExpansionTermCount.Set(2))
                    Assert.Equal(2, Current);
                Assert.Equal(1, Current);
            }
            Assert.Equal(Default, Current);
        }

        /// <summary>
        /// Disposal is normally the reverse of opening, so popping the head covers it. This
        /// is the other path: releasing the outer scope first has to rebuild the chain above
        /// it and leave the inner one standing.
        /// </summary>
        [Fact]
        public void OutOfOrderDisposalKeepsTheOtherScope()
        {
            var outer = MathS.Settings.MaxExpansionTermCount.Set(7);
            var inner = MathS.Settings.MaxExpansionTermCount.Set(9);
            Assert.Equal(9, Current);

            outer.Dispose();
            Assert.Equal(9, Current);

            inner.Dispose();
            Assert.Equal(Default, Current);
        }

        [Fact]
        public void DisposingTwiceIsHarmless()
        {
            var scope = MathS.Settings.MaxExpansionTermCount.Set(31);
            using (var other = MathS.Settings.MaxExpansionTermCount.Set(32))
            {
                scope.Dispose();
                scope.Dispose();
                Assert.Equal(32, Current);
            }
            Assert.Equal(Default, Current);
        }

        /// <summary>A setting nobody has touched reports its default and says so.</summary>
        [Fact]
        public void UntouchedSettingReadsItsDefault()
        {
            Assert.Equal(Default, Current);
            using (var _ = MathS.Settings.MaxExpansionTermCount.Set(Default + 1))
                Assert.Equal(Default + 1, Current);
            Assert.Equal(Default, Current);
        }
    }
}
