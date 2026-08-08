//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using AngouriMath;
using AngouriMath.Core.Transformations;
using Xunit;

namespace AngouriMath.Tests.Core.Transformations
{
    /// <summary>
    /// That applying a rule set with nobody recording stays free.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the one claim about the layer that no ordinary test can see: the code reads
    /// as though the fast path returns before any of the recording machinery, and it did,
    /// and it still allocated on every call — because the closure on the recording path
    /// captures the rule set and the recording, and the compiler allocates the object
    /// holding them where they come into scope. It cost <c>Simplify</c> a fifth of its
    /// allocation with recording switched off, and only a benchmark caught it.
    /// </para>
    /// <para>
    /// A benchmark is not run in CI, so the guard is here. It is written to be blunt rather
    /// than precise: applying a rule set to a <b>leaf</b>, which no rule matches, rebuilds
    /// no nodes and so should allocate essentially nothing. One stray per-call allocation
    /// then dominates the measurement instead of hiding inside it, which is what makes the
    /// threshold below safe to assert across runtimes rather than a source of flakes.
    /// </para>
    /// </remarks>
    [Trait("Area", "Transformations")]
    public sealed class RewriteAllocationTest
    {
        private const int Iterations = 20_000;

        /// <summary>
        /// Generous on purpose. The measurement is a handful of bytes per call at most; a
        /// reintroduced per-call closure costs on the order of thirty, which is roughly a
        /// hundredfold over this budget. Anything in between is not something this test is
        /// trying to have an opinion about.
        /// </summary>
        private const long BudgetBytes = 8 * Iterations;

        [Fact]
        public void ApplyingARuleSetToALeafAllocatesEssentiallyNothing()
        {
            Entity leaf = MathS.Var("x");

            // Warm the caches the first call fills, so that what is measured is the
            // steady-state cost and not one-off construction.
            for (var i = 0; i < 100; i++)
                RewriteRules.Common.ApplyOnce(leaf);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var before = GC.GetAllocatedBytesForCurrentThread();
            for (var i = 0; i < Iterations; i++)
                RewriteRules.Common.ApplyOnce(leaf);
            var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.True(
                allocated <= BudgetBytes,
                $"applying a rule set with no recording open allocated {allocated} bytes over {Iterations} calls, "
                + $"which is over the {BudgetBytes} budget. Something on the fast path is allocating per call -- "
                + "a closure in the method is the usual cause, and moving it into its own method is the usual fix.");
        }

        [Fact]
        public void RecordingIsWhatCostsSomething()
        {
            // The other half of the claim: the budget above is not passing because the
            // measurement is broken. With a recording open the same calls do allocate,
            // because each rewrite becomes a step.
            Entity expression = MathS.FromString("a / (b / c)", useCache: false);

            using var recording = RewriteRecording.Start();

            var before = GC.GetAllocatedBytesForCurrentThread();
            for (var i = 0; i < 1_000; i++)
                RewriteRules.Common.ApplyOnce(expression);
            var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.True(allocated > 8 * 1_000, $"recording allocated only {allocated} bytes; the measurement is not measuring anything");
            Assert.NotEmpty(recording.Steps);
        }
    }
}
