//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AngouriMath;
using AngouriMath.Core.Transformations;
using Xunit;

namespace AngouriMath.Tests.Core.Transformations
{
    /// <summary>
    /// Recording the rewrites that fire — <a href="https://github.com/asc-community/AngouriMath/issues/28">#28</a>.
    /// </summary>
    [Trait("Area", "Transformations")]
    public sealed class RewriteRecordingTest
    {
        // Uncached, so that each expression is a fresh tree. An Entity memoises
        // InnerSimplified on itself, so a cached one handed back a second time has already
        // done part of the work and would record fewer steps for the same input.
        private static Entity Parse(string raw) => MathS.FromString(raw, useCache: false);

        [Fact]
        public void ARecordingCollectsTheRewritesThatFired()
        {
            using var recording = RewriteRecording.Start();
            Parse("a / (b / c)").Simplify();

            Assert.NotEmpty(recording.Steps);
            foreach (var step in recording.Steps)
            {
                // A rule set that changed nothing records nothing, so every step is a change.
                Assert.NotEqual(step.Before, step.After);
                // and every step names a set the registry knows about
                Assert.Contains(step.RuleSet, RewriteRules.All);
            }
        }

        [Fact]
        public void AStepSaysWhatItClaimsAndHowWellJustifiedItIs()
        {
            using var recording = RewriteRecording.Start();
            Parse("a / (b / c)").Simplify();

            var step = recording.Steps[0];
            Assert.Equal(step.RuleSet.Relation, step.Relation);
            Assert.Equal(step.RuleSet.Soundness, step.Soundness);
            Assert.Equal(TransformationRelation.Equivalence, step.Relation);
            Assert.Equal(Soundness.SoundUnderAssumptions, step.Soundness);
            Assert.Contains("->", step.ToString());
            Assert.Contains(step.RuleSet.Name, step.ToString());
        }

        [Fact]
        public void NothingIsRecordedWhenNobodyIsListening()
        {
            // Not an assertion about a counter -- there is nothing to count when no
            // recording is open. What is pinned is that opening one afterwards starts empty,
            // so the previous work left nothing behind in a static.
            Parse("a / (b / c)").Simplify();

            using var recording = RewriteRecording.Start();
            Assert.Empty(recording.Steps);
        }

        [Fact]
        public void RecordingDoesNotChangeTheAnswer()
        {
            var withoutRecording = Parse("(x ^ 3 + 3 * x ^ 2 * y + 3 * x * y ^ 2 + y ^ 3) / (x + y)").Simplify();

            Entity withRecording;
            using (var _ = RewriteRecording.Start())
                withRecording = Parse("(x ^ 3 + 3 * x ^ 2 * y + 3 * x * y ^ 2 + y ^ 3) / (x + y)").Simplify();

            Assert.Equal(withoutRecording, withRecording);
        }

        [Fact]
        public void TheSameComputationRecordsTheSameStepsEveryTime()
        {
            static IReadOnlyList<string> Record()
            {
                using var recording = RewriteRecording.Start();
                Parse("sin(x) / tan(x) + a / (b / c)").Simplify();
                return recording.Steps.Select(s => s.ToString()).ToList();
            }

            Assert.Equal(Record(), Record());
        }

        [Fact]
        public void ClosingARecordingStopsIt()
        {
            var recording = RewriteRecording.Start();
            Parse("a / (b / c)").Simplify();
            var afterFirst = recording.Steps.Count;
            recording.Dispose();

            Parse("sin(x) / tan(x)").Simplify();

            Assert.Equal(afterFirst, recording.Steps.Count);
        }

        [Fact]
        public void DisposingTwiceIsHarmless()
        {
            var recording = RewriteRecording.Start();
            recording.Dispose();
            recording.Dispose();

            using var next = RewriteRecording.Start();
            Parse("a / (b / c)").Simplify();
            Assert.NotEmpty(next.Steps);
        }

        [Fact]
        public void AnInnerRecordingDoesNotFeedTheOuterOne()
        {
            using var outer = RewriteRecording.Start();
            Parse("a / (b / c)").Simplify();
            var outerBeforeInner = outer.Steps.Count;

            using (var inner = RewriteRecording.Start())
            {
                Parse("sin(x) / tan(x)").Simplify();
                Assert.NotEmpty(inner.Steps);
            }

            Assert.Equal(outerBeforeInner, outer.Steps.Count);

            // and the outer one is listening again once the inner has closed
            Parse("u / (v / w)").Simplify();
            Assert.True(outer.Steps.Count > outerBeforeInner);
        }

        [Fact]
        public void ARecordingOnOneThreadDoesNotSeeAnother()
        {
            using var recording = RewriteRecording.Start();
            Parse("a / (b / c)").Simplify();
            var mine = recording.Steps.Count;

            // A thread joined synchronously rather than an awaited task. Awaiting would
            // yield this thread while the recording is still open on it, and this thread is
            // exactly where the test runner is free to start something else -- which the
            // recording would then collect, because it follows the thread and not the call.
            var other = new Thread(() => Parse("sin(x) / tan(x) + u / (v / w)").Simplify());
            other.Start();
            other.Join();

            Assert.Equal(mine, recording.Steps.Count);
        }

        [Fact]
        public void AClosedRecordingIgnoresWhateverItIsStillHanded()
        {
            // Closing on a different thread than the one that opened it leaves the opening
            // thread still holding the reference. It must then be inert: neither growing a
            // list nobody will read, nor adding to a result already handed back.
            RewriteRecording? opened = null;
            using var recorded = new ManualResetEventSlim();
            using var closed = new ManualResetEventSlim();

            var opener = new Thread(() =>
            {
                opened = RewriteRecording.Start();
                Parse("a / (b / c)").Simplify();
                recorded.Set();
                closed.Wait();
                // This thread's `current` still points at the recording that was closed
                // from elsewhere; none of this may reach it.
                Parse("u / (v / w)").Simplify();
            });
            opener.Start();
            recorded.Wait();

            var recording = Assert.IsType<RewriteRecording>(opened);
            var collected = recording.Steps.Count;
            Assert.NotEqual(0, collected);

            recording.Dispose();
            closed.Set();
            opener.Join();

            Assert.Equal(collected, recording.Steps.Count);
        }

        [Fact]
        public void EveryRecordedStepIsAnEquivalenceUnderStatedAssumptions()
        {
            using var recording = RewriteRecording.Start();
            Parse("sin(x) / tan(x) + a / (b / c) + (x + 1) ^ 2").Simplify();

            Assert.NotEmpty(recording.Steps);
            foreach (var step in recording.Steps)
            {
                // Nothing the simplifier applies may claim a proof it has not got, and
                // nothing it applies may claim to have produced a different object.
                Assert.Equal(TransformationRelation.Equivalence, step.Relation);
                Assert.NotEqual(Soundness.Sound, step.Soundness);
            }
        }
    }
}
