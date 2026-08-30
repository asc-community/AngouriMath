//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        /// <summary>
        /// The reporter's expression on
        /// <a href="https://github.com/asc-community/AngouriMath/issues/28">#28</a>, and the rule
        /// they wrote out by hand — <c>any1 / (any2 / any3) -> any1 * any3 / any2</c> — is in the
        /// derivation, named.
        /// </summary>
        [Fact]
        public void TheDerivationNamesTheRewritesTheReporterAskedFor()
        {
            using var recording = RewriteRecording.Start();
            Parse("x^(-1)/(y/z)").Simplify();

            var derivation = recording.Derivation;
            Assert.NotEmpty(derivation);
            // Named in the reporter's terms and then some: the rule they wrote out by hand is
            // `dividing-by-a-quotient-multiplies-by-its-reciprocal`, and it says so itself. This
            // matched the `switch` arm's rendered pattern and replacement text until `Common` was
            // described by the rules it runs.
            Assert.Contains(derivation, step =>
                step.Rule?.Name == "dividing-by-a-quotient-multiplies-by-its-reciprocal"
                && step.Rule?.Description == "a / (b / c) = a * c / b");
        }

        /// <summary>
        /// The point of the view: the raw list is dominated by the engine tidying up between
        /// rewrites, and by the same rewrite recurring down every candidate branch.
        /// </summary>
        [Fact]
        public void TheDerivationIsFarShorterThanTheRawRecording()
        {
            using var recording = RewriteRecording.Start();
            Parse("x^(-1)/(y/z)").Simplify();

            Assert.True(recording.Steps.Count > 100,
                $"the raw recording is only {recording.Steps.Count} steps, so this proves nothing");
            Assert.True(recording.Derivation.Count < 15,
                $"the derivation is {recording.Derivation.Count} steps: "
                + string.Join("; ", recording.Derivation.Select(s => s.Rule?.Name ?? s.RuleSet.Name)));
        }

        /// <summary>Normalisation is what the view drops, so none of it may survive.</summary>
        [Fact]
        public void TheDerivationHasNoNormalisation()
        {
            using var recording = RewriteRecording.Start();
            Parse("x^(-1)/(y/z)").Simplify();

            // Stated rather than assumed: if the raw recording had no normalisation in it the
            // assertion below would hold while testing nothing.
            Assert.Contains(recording.Steps, step => step.RuleSet.IsNormalization);
            Assert.DoesNotContain(recording.Derivation, step => step.RuleSet.IsNormalization);
        }

        /// <summary>
        /// A rewrite that takes one step is reported as one step, with the rule that did it.
        /// </summary>
        /// <remarks>
        /// <b>Both are named in words now, and neither was when this was written.</b> It asserted
        /// the replacement's C# source text — <c>"2 * any1"</c> and <c>"1"</c> — which is what the
        /// registry had while it described the <c>switch</c> each set had stopped running. As the
        /// sets were repointed the names became the rules' own and the replacements became
        /// <c>(built by code)</c>, so the assertion moved to the name and the identity: what a
        /// derivation reports, and what a reader of one is looking for.
        /// </remarks>
        [Theory]
        [InlineData("x + x", "a-term-added-to-itself-doubles", "k + k = 2 * k")]
        [InlineData("sin(x)^2 + cos(x)^2", "a-squared-sine-and-cosine-of-one-angle-sum-to-one",
            "sin(a)^2 + cos(a)^2 = 1")]
        public void AOneStepRewriteIsOneStep(string expr, string ruleName, string? identity)
        {
            using var recording = RewriteRecording.Start();
            Parse(expr).Simplify();

            var step = Assert.Single(recording.Derivation);
            Assert.Equal(ruleName, step.Rule?.Name);
            Assert.Equal(identity, step.Rule?.Description);
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
        public void WorkStartedUnderARecordingIsCollectedWhereverItRuns()
        {
            // The recording belongs to the call, so work begun under it reports to it even
            // on another thread. Held per thread this collected nothing, because the new
            // thread had never seen the recording.
            using var recording = RewriteRecording.Start();
            Parse("a / (b / c)").Simplify();
            var mine = recording.Steps.Count;
            Assert.NotEqual(0, mine);

            var other = new Thread(() => Parse("sin(x) / tan(x) + u / (v / w)").Simplify());
            other.Start();
            other.Join();

            Assert.True(recording.Steps.Count > mine);
        }

        [Fact]
        public async Task ARecordingSurvivesAnAwait()
        {
            using var recording = RewriteRecording.Start();
            await Task.Delay(20).ConfigureAwait(false);
            Parse("a / (b / c)").Simplify();
            Assert.NotEmpty(recording.Steps);
        }

        [Fact]
        public async Task SiblingRecordingsDoNotSeeEachOther()
        {
            // The isolation that the per-thread field did give and that this must keep. The
            // barrier makes both recordings open before either does any work.
            using var barrier = new Barrier(2);
            async Task<(int mine, IReadOnlyList<RewriteStep> steps)> Branch(string expr)
            {
                await Task.Yield();
                using var recording = RewriteRecording.Start();
                barrier.SignalAndWait();
                Parse(expr).Simplify();
                await Task.Delay(20).ConfigureAwait(false);
                return (recording.Steps.Count, recording.Steps);
            }

            var left = Branch("a / (b / c)");
            var right = Branch("sin(x) / tan(x)");
            var results = await Task.WhenAll(left, right);

            Assert.All(results, r => Assert.NotEqual(0, r.mine));
            // Neither collected the other's work: together they would be the union, and each
            // is strictly smaller than that.
            var combined = results.Sum(r => r.mine);
            Assert.All(results, r => Assert.True(r.mine < combined));
        }

        [Fact]
        public async Task ARecordingOpenedInsideATaskDoesNotEscapeIt()
        {
            RewriteRecording? inner = null;
            await Task.Run(() =>
            {
                // Left open on purpose: what is under test is that the pointer does not leak
                // out of the task, not that Dispose puts it back.
                inner = RewriteRecording.Start();
                Parse("a / (b / c)").Simplify();
            });

            var recording = Assert.IsType<RewriteRecording>(inner);
            var whenTheTaskEnded = recording.Steps.Count;
            Assert.NotEqual(0, whenTheTaskEnded);

            Parse("sin(x) / tan(x) + u / (v / w)").Simplify();
            Assert.Equal(whenTheTaskEnded, recording.Steps.Count);
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
                // Nothing the simplifier applies may claim to have produced a different object.
                Assert.Equal(TransformationRelation.Equivalence, step.Relation);
                // And nothing may claim a proof it has not got. This asserted `NotEqual(Sound)` --
                // that every step was conditional -- which was true only because a step reported
                // its *set's* tier, and a set's tier is the minimum over its rules. A step now
                // reports the tier of the rule that fired, and 181 of the 322 rules written as
                // data really are Sound, so the assertion is that the tier is one of the two the
                // simplifier is allowed to apply rather than that it is always the weaker.
                Assert.True(step.Soundness is Soundness.Sound or Soundness.SoundUnderAssumptions,
                    $"{step.RuleSet.Name}/{step.Rule?.Name} claims {step.Soundness}");
            }
        }
    }
}
