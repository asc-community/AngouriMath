//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Linq;
using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Core.Transformations;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Core.Transformations
{
    public sealed class EGraphTest
    {
        [Fact]
        public void AddingTheSameLeafTwiceReturnsTheSameClass()
        {
            var graph = new EGraph();
            var first = graph.Add("x");
            var second = graph.Add("x");
            Assert.Equal(first, second);
        }

        [Fact]
        public void AddingTwoDifferentLeavesReturnsDifferentClasses()
        {
            var graph = new EGraph();
            var x = graph.Add("x");
            var y = graph.Add("y");
            Assert.NotEqual(x, y);
        }

        [Fact]
        public void UnionMakesFindAgreeOnBothClasses()
        {
            var graph = new EGraph();
            var x = graph.Add("x");
            var y = graph.Add("y");
            graph.Union(x, y);
            Assert.Equal(graph.Find(x), graph.Find(y));
        }

        [Fact]
        public void UnionOfTheSameClassReportsNoChange()
        {
            var graph = new EGraph();
            var x = graph.Add("x");
            Assert.False(graph.Union(x, x));
        }

        [Fact]
        public void UnionOfTwoDifferentClassesReportsAChange()
        {
            var graph = new EGraph();
            var x = graph.Add("x");
            var y = graph.Add("y");
            Assert.True(graph.Union(x, y));
        }

        [Fact]
        public void UnionReducesTheClassCountByOne()
        {
            var graph = new EGraph();
            var x = graph.Add("x");
            var y = graph.Add("y");
            var before = graph.ClassCount;
            graph.Union(x, y);
            Assert.Equal(before - 1, graph.ClassCount);
        }

        [Fact]
        public void RebuildMergesNodesThatBecomeCongruentAfterAUnion()
        {
            // f(x) and f(y) are different nodes until x and y are unioned; the congruence
            // rebuild is what then notices the two applications of f agree too.
            var graph = new EGraph();
            var x = graph.Add("x");
            var y = graph.Add("y");
            var fx = graph.Add("f", x);
            var fy = graph.Add("f", y);
            Assert.NotEqual(graph.Find(fx), graph.Find(fy));

            graph.Union(x, y);
            graph.Rebuild();

            Assert.Equal(graph.Find(fx), graph.Find(fy));
        }

        [Fact]
        public void NodesOfReturnsTheNodeJustAdded()
        {
            var graph = new EGraph();
            var x = graph.Add("x");
            Assert.Contains(new ENode("x", System.Array.Empty<int>()), graph.NodesOf(x));
        }

        [Fact]
        public void ClassesListsEveryClassAdded()
        {
            var graph = new EGraph();
            var x = graph.Add("x");
            var y = graph.Add("y");
            Assert.Equal(new[] { x, y }, graph.Classes.OrderBy(id => id));
        }

        [Theory]
        // x + 0 -> x, and commutatively 0 + x -> x.
        [InlineData("Sumf", "x", "0", true)]
        [InlineData("Sumf", "0", "x", true)]
        // x - 0 -> x, but 0 - x is a different value (its negation), not a fold.
        [InlineData("Minusf", "x", "0", true)]
        [InlineData("Minusf", "0", "x", false)]
        // x * 1 -> x, and commutatively 1 * x -> x.
        [InlineData("Mulf", "x", "1", true)]
        [InlineData("Mulf", "1", "x", true)]
        // x / 1 -> x, but 1 / x is a different value (its reciprocal), not a fold.
        [InlineData("Divf", "x", "1", true)]
        [InlineData("Divf", "1", "x", false)]
        // x ^ 1 -> x, but 1 ^ x is the constant 1, not this operand -- not this fold either.
        [InlineData("Powf", "x", "1", true)]
        [InlineData("Powf", "1", "x", false)]
        public void NeutralElementFoldsIntoTheOtherOperandsClassOnlyWhenTheValuesAgree(
            string op, string leftLeaf, string rightLeaf, bool shouldFold)
        {
            var graph = new EGraph();
            var left = graph.Add(leftLeaf);
            var right = graph.Add(rightLeaf);
            var before = graph.NodeCount;

            var result = graph.Add(op, left, right);

            var nonLeafOperand = leftLeaf == "x" ? left : right;
            if (shouldFold)
            {
                Assert.Equal(graph.Find(nonLeafOperand), graph.Find(result));
                Assert.Equal(before, graph.NodeCount);
            }
            else
            {
                Assert.NotEqual(graph.Find(nonLeafOperand), graph.Find(result));
                Assert.Equal(before + 1, graph.NodeCount);
            }
        }

        [Theory]
        [InlineData("x + 1")]
        [InlineData("sin(x) * cos(y)")]
        [InlineData("(x + 1) ^ 2")]
        public void AnEntityAddedAndExtractedUnchangedRoundTrips(string source)
        {
            var expr = source.ToEntity();
            var graph = new EGraph();
            var root = graph.AddEntity(expr);

            var extracted = graph.Extract(root, CostModel.Default.Cost);

            Assert.NotNull(extracted);
            Assert.True(expr.Equals(extracted));
        }

        [Fact]
        public void FoldingOnInsertionMeansNoRuleIsNeededToDropAnAddedZero()
        {
            var expr = "x + 0".ToEntity();
            var graph = new EGraph();
            var root = graph.AddEntity(expr);

            var extracted = graph.Extract(root, CostModel.Default.Cost);

            Assert.True("x".ToEntity().Equals(extracted));
        }

        /// <summary>
        /// <see cref="EGraph.Extract"/> rebuilds a non-leaf node through
        /// <see cref="AngouriMath.Core.Transformations.Matching.MatchPattern.ConstructNode"/>,
        /// which never restores <see cref="Entity.Codomain"/> -- an init-settable, per-node-type
        /// property the rest of the codebase treats as load-bearing and explicitly preserves on
        /// every <c>Replace</c>. Caught in code review before this PR was merged.
        /// </summary>
        [Fact]
        public void ExtractPreservesANarrowedCodomain()
        {
            Entity narrowed = MathS.Sqrt(-1).WithCodomain(Domain.Real);
            var graph = new EGraph();
            var root = graph.AddEntity(narrowed);

            var extracted = graph.Extract(root, CostModel.Default.Cost);

            Assert.NotNull(extracted);
            Assert.Equal(Domain.Real, extracted!.Codomain);
            Assert.Equal(narrowed, extracted);
            Assert.Equal(narrowed.Evaled, extracted.Evaled);
        }

        /// <summary>
        /// <see cref="EGraph.Key"/> keys a leaf by <see cref="Entity.Stringize"/>, and
        /// <see cref="Entity.Constant.EulerIntrinsic"/> prints identically to the ordinary named
        /// constant <c>e</c> -- the two are <see cref="object.Equals(object)"/>-equal by design,
        /// but only <see cref="EulerIntrinsic"/> is meant to stay outside what a binder over the
        /// name <c>e</c> can capture. Re-parsing the printed form silently swaps it for the named
        /// constant, which is invisible to every equality-based check and only shows up at a
        /// binder. Caught in code review before this PR was merged.
        /// </summary>
        [Fact]
        public void ExtractPreservesEulerIntrinsicIdentity()
        {
            var graph = new EGraph();
            var id = graph.AddEntity(Entity.Constant.EulerIntrinsic);

            var extracted = graph.Extract(id, CostModel.Default.Cost);

            Assert.True(ReferenceEquals(Entity.Constant.EulerIntrinsic, extracted));
        }
    }
}
