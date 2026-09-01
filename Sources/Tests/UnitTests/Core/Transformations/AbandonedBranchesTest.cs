//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Linq;
using AngouriMath.Core.Transformations;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Core.Transformations
{
    /// <summary>
    /// Where the search went and came back from.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/273">#273</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// #273 asks for two things: recording the steps taken while solving, and being able to see
    /// the return to a branch's root where a method fails. The first was already there —
    /// <see cref="RewriteRecording"/> and <see cref="DerivationPath"/>. This is the second.
    /// </para>
    /// <para>
    /// The data was recorded all along: <c>PathFrom</c> searches the recorded edges for a chain
    /// from the input to the result and throws the rest away. Those are the branches, and each
    /// carries its own <see cref="DerivationStep.Before"/> — the expression the search was at
    /// when it tried this, and the one it returned to.
    /// </para>
    /// </remarks>
    [Trait("Area", "Core")]
    public sealed class AbandonedBranchesTest
    {
        /// <summary>
        /// The kept chain is a branch of a search, not a walk to the answer, and the abandoned
        /// steps are what says so. <c>(x + 1)^2</c> is the clearest small case: the search
        /// expands it and comes back, because the unexpanded form rates better.
        /// </summary>
        [Fact]
        public void TheSearchExpandsAndComesBack()
        {
            var path = DerivationPath.OfSimplifying("(x + 1)^2".ToEntity());
            Assert.NotNull(path);

            Assert.NotEmpty(path!.Abandoned);
            // It reached the expanded form and did not keep it.
            var expanded = "x ^ 2 + 2 * x + 1".ToEntity();
            Assert.Contains(path.Abandoned, step => step.After == expanded);
            Assert.DoesNotContain(path.Steps, step => step.After == expanded);
            Assert.NotEqual(expanded, path.Result);
        }

        /// <summary>
        /// Every abandoned step leaves from an expression the search had actually reached: its
        /// root is the input, or something a kept step or another attempt produced. A step
        /// leaving from nowhere would mean the branches were being read off the wrong recording.
        /// </summary>
        [Fact]
        public void EveryBranchLeavesFromSomewhereTheSearchHadBeen()
        {
            var path = DerivationPath.OfSimplifying("x ^ (-1) / (y / z)".ToEntity());
            Assert.NotNull(path);

            var reached = new System.Collections.Generic.HashSet<Entity> { path!.Input };
            foreach (var step in path.Steps)
                reached.Add(step.After);
            foreach (var step in path.Abandoned)
                reached.Add(step.After);

            foreach (var step in path.Abandoned)
                Assert.Contains(step.Before, reached);
        }

        /// <summary>
        /// Nothing on the kept chain is also reported as abandoned. It was the same edge in both
        /// lists before they were deduplicated, which made the branch list read as though the
        /// search had rejected the very step it took.
        /// </summary>
        [Fact]
        public void AKeptStepIsNotAlsoAbandoned()
        {
            foreach (var source in new[] { "(x + 1)^2", "x ^ (-1) / (y / z)", "sin(x)^2 + cos(x)^2" })
            {
                var path = DerivationPath.OfSimplifying(source.ToEntity());
                Assert.NotNull(path);
                foreach (var kept in path!.Steps)
                    Assert.DoesNotContain(path.Abandoned,
                        step => step.Before == kept.Before
                            && step.After == kept.After
                            && step.Name == kept.Name);
            }
        }

        /// <summary>
        /// And no step is reported twice. The simplifier runs the same passes over the same
        /// expressions at every level of its candidate search, so the raw edges are mostly one
        /// rewrite recorded over and over: <c>x^(-1)/(y/z)</c> produced 425 of them across 13
        /// distinct steps. A list of 425 is a record of how often the search was asked rather
        /// than of where it went.
        /// </summary>
        [Fact]
        public void NoBranchIsReportedTwice()
        {
            var path = DerivationPath.OfSimplifying("x ^ (-1) / (y / z)".ToEntity());
            Assert.NotNull(path);

            var distinct = path!.Abandoned
                .Select(step => (step.Before, step.After, step.Name))
                .Distinct()
                .Count();
            Assert.Equal(distinct, path.Abandoned.Count);
        }

        /// <summary>
        /// An expression that is already its own answer abandoned nothing, and one whose whole
        /// derivation was kept abandoned nothing either. Both are zero rather than "unknown",
        /// and asserting them keeps the list from filling up with noise unnoticed.
        /// </summary>
        [Theory]
        [InlineData("2 + 2")]
        [InlineData("sin(x)^2 + cos(x)^2")]
        public void NothingIsAbandonedWhereNothingWasTried(string source)
        {
            var path = DerivationPath.OfSimplifying(source.ToEntity());
            Assert.NotNull(path);
            Assert.Empty(path!.Abandoned);
        }

        /// <summary>
        /// The count of explored expressions and the two lists tell one story: the search
        /// produced <see cref="DerivationPath.ExpressionsExplored"/> distinct expressions, and
        /// what the two lists reach cannot exceed that.
        /// </summary>
        [Fact]
        public void TheListsAgreeWithWhatWasExplored()
        {
            var path = DerivationPath.OfSimplifying("x ^ (-1) / (y / z)".ToEntity());
            Assert.NotNull(path);

            var produced = path!.Steps.Select(step => step.After)
                .Concat(path.Abandoned.Select(step => step.After))
                .Distinct()
                .Count();
            Assert.True(produced <= path.ExpressionsExplored,
                $"the two lists reach {produced} expressions, but only "
                + $"{path.ExpressionsExplored} were recorded as produced");
        }
    }
}
