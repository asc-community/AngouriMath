//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Collections.Generic;
using System.Linq;
using AngouriMath;
using AngouriMath.Core.Transformations;
using Xunit;

namespace AngouriMath.Tests.Core.Transformations
{
    /// <summary>
    /// The derivation as a path — <a href="https://github.com/asc-community/AngouriMath/issues/28">#28</a>
    /// and <a href="https://github.com/asc-community/AngouriMath/issues/273">#273</a>.
    /// </summary>
    [Trait("Area", "Transformations")]
    public sealed class DerivationPathTest
    {
        // Uncached, so that each expression is a fresh tree, as in RewriteRecordingTest: an
        // Entity memoises InnerSimplified on itself, and a cached one handed back a second
        // time has already done part of the work.
        private static Entity Parse(string raw) => MathS.FromString(raw, useCache: false);

        /// <summary>
        /// Eight shapes, chosen so that between them they go through every kind of step the
        /// path is made of: rewrite passes, inner simplification, the boolean minimiser, the
        /// polynomial rearrangement, expansion and factoring.
        /// </summary>
        public static IEnumerable<object[]> Shapes => new[]
        {
            new object[] { "x^(-1)/(y/z)" },         // the reporter's expression on #28
            new object[] { "a / (b / c)" },          // one rewrite and done
            new object[] { "sin(x)^2 + cos(x)^2" },  // a trigonometric identity
            new object[] { "(x2 - 1) / (x - 1)" },   // a quotient that cancels
            new object[] { "x + x + x" },            // like terms
            new object[] { "a and b or a and not b" },// boolean, which reaches the minimiser
            new object[] { "(x + 1)^2 - x^2 - 2*x" },// only settles once expanded
            new object[] { "1/(sqrt(3) + 5)" },      // a surd in the denominator
        };

        /// <summary>
        /// The claim. Each step starts where the one before it ended, the first starts at the
        /// input, and the last lands on what <see cref="Entity.Simplify(int)"/> returned —
        /// compared as expressions, since two writings of one expression print differently
        /// and a printed comparison would pass or fail for the wrong reason.
        /// </summary>
        [Theory]
        [MemberData(nameof(Shapes))]
        public void TheDerivationIsAPathFromTheInputToTheAnswer(string raw)
        {
            using var recording = RewriteRecording.Start();
            var input = Parse(raw);
            var answer = input.Simplify();

            var path = recording.PathFrom(input, answer);
            Assert.NotNull(path);
            Assert.Equal(input, path!.Input);
            Assert.Equal(answer, path.Result);

            var at = input;
            foreach (var step in path.Steps)
            {
                Assert.Equal(at, step.Before);
                Assert.NotEqual(step.Before, step.After);
                at = step.After;
            }
            Assert.Equal(answer, at);
        }

        /// <summary>
        /// The path is the answer's, not a tour of the search: it can never be longer than what
        /// the search produced, and none of what the search dropped is on it, since nothing
        /// leads from a discarded candidate to the value that was returned.
        /// </summary>
        [Theory]
        [MemberData(nameof(Shapes))]
        public void ThePathIsNoLongerThanTheSearchThatFoundIt(string raw)
        {
            using var recording = RewriteRecording.Start();
            var input = Parse(raw);
            var path = recording.PathFrom(input, input.Simplify());

            Assert.NotNull(path);
            Assert.True(path!.ExpressionsExplored >= path.Steps.Count,
                $"{raw}: the path is {path.Steps.Count} steps out of {path.ExpressionsExplored} "
                + "expressions produced, which is more steps than there were expressions to take them between");
        }

        /// <summary>
        /// And on an expression the search really has to work at, the path is a small part of
        /// what happened — which is the whole difference between a derivation and a log.
        /// </summary>
        [Fact]
        public void ThePathIsASmallPartOfWhatHappened()
        {
            using var recording = RewriteRecording.Start();
            var input = Parse("x^(-1)/(y/z)");
            var path = recording.PathFrom(input, input.Simplify());

            Assert.NotNull(path);
            Assert.True(path!.Steps.Count < path.ExpressionsExplored,
                $"the path kept all {path.Steps.Count} of the expressions the search produced, so it "
                + "left nothing out and there is nothing here to have chosen between");
            Assert.True(recording.Steps.Count > 20 * path.Steps.Count,
                $"{recording.Steps.Count} rewrites were recorded against a {path.Steps.Count}-step path, "
                + "which is not enough of a difference to be evidence of anything");
        }

        /// <summary>
        /// Recording is meant to be an observation and not a change of behaviour, so the answer
        /// under a recording has to be the answer without one.
        /// </summary>
        [Theory]
        [MemberData(nameof(Shapes))]
        public void RecordingDoesNotMoveTheAnswer(string raw)
        {
            var unwatched = Parse(raw).Simplify();

            using var recording = RewriteRecording.Start();
            Assert.Equal(unwatched, Parse(raw).Simplify());
        }

        /// <summary>Every step says what did it, and names a set the registry knows where it was one.</summary>
        [Theory]
        [MemberData(nameof(Shapes))]
        public void EveryStepNamesWhatDidIt(string raw)
        {
            using var recording = RewriteRecording.Start();
            var input = Parse(raw);
            var path = recording.PathFrom(input, input.Simplify());

            Assert.NotNull(path);
            foreach (var step in path!.Steps)
            {
                Assert.False(string.IsNullOrWhiteSpace(step.Name));
                if (step.RuleSet is null)
                {
                    // Not one rule set applied once, so there is no set to name and none is
                    // invented -- which is not the same as nothing having fired inside it, since
                    // a step like SimplifyChildren is a chain of rule sets.
                    Assert.Null(step.Relation);
                    Assert.Null(step.Soundness);
                    continue;
                }
                Assert.Contains(step.RuleSet, RewriteRules.All);
                Assert.Equal(step.RuleSet.Name, step.Name);
                Assert.Equal(step.RuleSet.Relation, step.Relation);
                Assert.Equal(step.RuleSet.Soundness, step.Soundness);
            }
        }

        /// <summary>
        /// The rewrites inside a step are the ones that fired in it, and they are the grain #28
        /// asked for: the reporter's own rule, on the reporter's own expression, on the step of
        /// the path that applied it.
        /// </summary>
        [Fact]
        public void AStepCarriesTheRulesThatFiredInsideIt()
        {
            using var recording = RewriteRecording.Start();
            var input = Parse("x^(-1)/(y/z)");
            var path = recording.PathFrom(input, input.Simplify());

            Assert.NotNull(path);
            var carrying = Assert.Single(path!.Steps.Where(step => step.Rewrites.Any(rewrite =>
                rewrite.Rule?.PatternSource == "Divf(var any1, Divf(var any2, var any3))"
                && rewrite.Rule?.ReplacementSource == "any1 * any3 / any2")));

            // and the rewrite it carries is a rewrite of a subexpression of the step it is in
            var rewrite = carrying.Rewrites.First(step => step.Rule?.ReplacementSource == "any1 * any3 / any2");
            Assert.Contains(rewrite.Before, carrying.Before.Nodes);
        }

        /// <summary>
        /// Why this exists at all, pinned so that the two views cannot be confused for one
        /// another. <see cref="RewriteRecording.Derivation"/> is a set of rewrites drawn from
        /// every candidate the search generated; read in order it does not chain, and it is a
        /// list of subexpressions rather than of whole expressions.
        /// </summary>
        [Fact]
        public void TheRewriteListIsStillNotAPath()
        {
            using var recording = RewriteRecording.Start();
            var input = Parse("x^(-1)/(y/z)");
            var answer = input.Simplify();

            var rewrites = recording.Derivation;
            Assert.True(rewrites.Count > 1, "with one rewrite there is nothing to fail to chain");
            Assert.Contains(rewrites.Zip(rewrites.Skip(1)), pair => pair.First.After != pair.Second.Before);
            Assert.NotEqual(answer, rewrites[rewrites.Count - 1].After);

            // and the path, over the same recording, does chain and does land on the answer
            var path = recording.PathFrom(input, answer);
            Assert.NotNull(path);
            Assert.Equal(answer, path!.Steps[path.Steps.Count - 1].After);
        }

        /// <summary>An expression that needed no work is a path of no steps, not a failure.</summary>
        [Fact]
        public void NothingToDoIsAPathOfLengthZero()
        {
            using var recording = RewriteRecording.Start();
            Entity leaf = MathS.Var("x");
            var answer = leaf.Simplify();

            var path = recording.PathFrom(leaf, answer);
            Assert.NotNull(path);
            Assert.Empty(path!.Steps);
            Assert.Equal(leaf, path.Result);
        }

        /// <summary>
        /// A result this recording never saw produced has no path here, and that is reported as
        /// "I cannot account for this" rather than as an empty one, which would read as
        /// "nothing happened".
        /// </summary>
        [Fact]
        public void AResultTheRecordingNeverSawHasNoPath()
        {
            using var recording = RewriteRecording.Start();
            var input = Parse("a / (b / c)");
            input.Simplify();

            Assert.Null(recording.PathFrom(input, Parse("q + 17")));
        }

        /// <summary>The whole of it in one call, which is what a caller asking #28's question wants.</summary>
        [Fact]
        public void OfSimplifyingOpensAndClosesTheRecordingItself()
        {
            var input = Parse("x^(-1)/(y/z)");
            var path = DerivationPath.OfSimplifying(input);

            Assert.NotNull(path);
            Assert.Equal(input.Simplify(), path!.Result);
            Assert.NotEmpty(path.Steps);
            // nothing left open afterwards
            using var after = RewriteRecording.Start();
            Assert.Empty(after.Steps);
        }

        /// <summary>
        /// The path written out, which is the form #28 asked for: each stage of the expression,
        /// and beside it what turned the one before into it.
        /// </summary>
        [Fact]
        public void ThePathPrintsAsTheStagesAndWhatMadeThem()
        {
            var path = DerivationPath.OfSimplifying(Parse("x^(-1)/(y/z)"));

            Assert.NotNull(path);
            var lines = path!.ToString().Split('\n');
            Assert.Equal(path.Steps.Count + 1, lines.Length);
            Assert.Equal(path.Input.Stringize(), lines[0]);
            foreach (var (line, step) in lines.Skip(1).Zip(path.Steps))
            {
                Assert.Contains(step.After.Stringize(), line);
                Assert.Contains(step.Name, line);
            }
        }
    }
}
