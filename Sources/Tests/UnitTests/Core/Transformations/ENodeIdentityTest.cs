//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core;
using AngouriMath.Core.Transformations;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Core.Transformations
{
    /// <summary>
    /// An e-node's identity has to include everything that makes two entities unequal, or the
    /// graph unions values that are not the same value. <see cref="Entity.Codomain"/> is the one
    /// piece of per-node data the library carries that is not a child, and it is exactly such a
    /// thing: <c>abs(x)</c> and <c>domain(abs(x), Any)</c> are different entities that evaluate
    /// differently, and an e-class holding both is asserting they are equal.
    /// </summary>
    [Trait("Area", "Core")]
    public sealed class ENodeIdentityTest
    {
        [Fact]
        public void ACodomainIsPartOfWhatMakesTwoEntitiesUnequal()
        {
            var narrow = "abs(x)".ToEntity();
            var wide = narrow.WithCodomain(Domain.Any);

            Assert.NotEqual(narrow, wide);
            Assert.NotEqual(narrow.Codomain, wide.Codomain);
        }

        /// <summary>
        /// The defect, stated as the graph sees it: two unequal entities inserted separately
        /// must not share an e-class, because an e-class is the graph's assertion that its
        /// members are equal.
        /// </summary>
        [Fact]
        public void TwoEntitiesDifferingOnlyInCodomainGetDifferentClasses()
        {
            var graph = new EGraph();
            var narrow = graph.AddEntity("abs(x)".ToEntity());
            var wide = graph.AddEntity("abs(x)".ToEntity().WithCodomain(Domain.Any));

            Assert.NotEqual(graph.Find(narrow), graph.Find(wide));
        }

        /// <summary>
        /// A rebuild re-creates every e-node canonically after a union. It used to do that from
        /// the operator and children alone, dropping the codomain — so a union anywhere in the
        /// graph quietly widened or narrowed unrelated nodes. Two unions here, one of them
        /// touching a child of the codomain-bearing node so that its canonical form changes.
        /// </summary>
        [Fact]
        public void ACodomainSurvivesARebuild()
        {
            var graph = new EGraph();
            var wide = graph.AddEntity("abs(x)".ToEntity().WithCodomain(Domain.Any));
            var y = graph.AddEntity("y".ToEntity());
            var x = graph.AddEntity("x".ToEntity());

            graph.Union(x, y);
            graph.Rebuild();

            var extracted = graph.Extract(wide, e => e.Complexity);
            Assert.NotNull(extracted);
            Assert.Equal(Domain.Any, extracted!.Codomain);
        }

        /// <summary>
        /// And whichever is extracted must be the one that was put in, not the one that happened
        /// to be inserted last. Inserted narrow-then-wide and wide-then-narrow, so that an
        /// implementation where the last writer wins fails on one of the two orders.
        /// </summary>
        [Fact]
        public void ExtractionReturnsTheCodomainThatWasInserted()
        {
            foreach (var wideFirst in new[] { false, true })
            {
                var graph = new EGraph();
                var narrowEntity = "abs(x)".ToEntity();
                var wideEntity = narrowEntity.WithCodomain(Domain.Any);

                var first = graph.AddEntity(wideFirst ? wideEntity : narrowEntity);
                var second = graph.AddEntity(wideFirst ? narrowEntity : wideEntity);
                var narrow = wideFirst ? second : first;
                var wide = wideFirst ? first : second;

                var narrowOut = graph.Extract(narrow, e => e.Complexity);
                var wideOut = graph.Extract(wide, e => e.Complexity);

                Assert.NotNull(narrowOut);
                Assert.NotNull(wideOut);
                Assert.Equal(narrowEntity.Codomain, narrowOut!.Codomain);
                Assert.Equal(Domain.Any, wideOut!.Codomain);
            }
        }
    }
}
